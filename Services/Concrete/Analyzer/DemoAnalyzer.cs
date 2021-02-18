#region Using
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.Models;
using Core.Models.Events;
using Core.Models.protobuf;
using Core.Models.Source;
using DemoInfo;
using ProtoBuf;
using Player = Core.Models.Player;
using Team = Core.Models.Team;
#endregion

namespace Services.Concrete.Analyzer
{
	public abstract class DemoAnalyzer
	{
		#region Properties

		public DemoParser Parser { get; set; }

		public Demo Demo { get; set; }
		protected Demo DemoBackup { get; set; }

		public Round CurrentRound { get; set; } = new Round();
		public Overtime CurrentOvertime { get; set; } = new Overtime();
		public ClutchEvent CurrentClutch { get; set; }

		public bool IsMatchStarted { get; set; } = false;
		public bool IsFirstKillOccured { get; set; }
		public bool IsHalfMatch { get; set; } = false;
		public bool IsOvertime { get; set; } = false;
		public bool IsLastRoundHalf { get; set; }
		/// <summary>
		/// Everytime a swap is required (half time / forced players swap by the server) the teams side are swapped when a new round start
		/// </summary>
		public bool IsSwapTeamRequired { get; set; } = false;
		/// <summary>
		/// Sometimes round_end event isn't triggered, this variable is used to handle this case and update scores at round_officialy_end event
		/// </summary>
		public bool IsRoundEndOccured { get; set; }
		/// <summary>
		/// Keep track of player_team events to detect swap teams
		/// </summary>
		protected int PlayerTeamCount { get; set; }
		/// <summary>
		/// Used to detect when the first shot occured to be able to have the right equipement money value
		/// I use this because the event buytime_ended isn't raised everytime
		/// </summary>
		public bool IsFirstShotOccured { get; set; } = false;
		/// <summary>
		/// Mainly used to detect game pauses
		/// </summary>
		public bool IsFreezetime { get; set; } = false;
		/// <summary>
		/// MR overtimes used to detect overtimes end
		/// It's generally 3 or 5
		/// </summary>
		protected int MrOvertime;
		/// <summary>
		/// When there is an overtime we start counting rounds
		/// to detect each overtimes end based on the value of MrOvertime
		/// </summary>
		protected int RoundCountOvertime;
		/// <summary>
		/// Number of overtime
		/// </summary>
		public int OvertimeCount { get; set; } = 0;
		/// <summary>
		/// Flag to know when the game is paused (used only with eBot atm)
		/// </summary>
		public bool IsGamePaused { get; set; }

		public Dictionary<DemoInfo.Player, int> KillsThisRound { get; set; } = new Dictionary<DemoInfo.Player, int>();

		private Player _playerInClutch1 = null;
		private Player _playerInClutch2 = null;

		private static readonly Regex LocalRegex = new Regex("^[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]+(\\:[0-9]{1,5})?$");
		private static readonly Regex FILENAME_FACEIT_REGEX = new Regex("^[0-9]+_team[a-z0-9-]+-team[a-z0-9-]+_de_[a-z0-9]+\\.dem");
		private static readonly Regex FILENAME_EBOT_REGEX = new Regex("^([0-9]*)_(.*?)-(.*?)_(.*?)(.dem)$");

		public bool AnalyzePlayersPosition { get; set; } = false;
		public bool AnalyzeFlashbang { get; set; } = false;

		// HTLV rating variables http://www.hltv.org/?pageid=242&eventid=0
		const double AVERAGE_KPR = 0.679; // average kills per round
		const double AVERAGE_SPR = 0.317; // average survived rounds per round
		const double AVERAGE_RMK = 1.277; // average value calculated from rounds with multiple kills

		// HTLV2 rating variables https://flashed.gg/posts/reverse-engineering-hltv-rating/
		const double HLTV2_KAST_MOD = 0.0073; // KAST modifier
		const double HLTV2_KPR_MOD = 0.3591; // KPR modifier
		const double HLTV2_DPR_MOD = -0.5329; // DPR modifier
		const double HLTV2_IMPACT_MOD = 0.2372; // Impact modifier
		const double HLTV2_IMPACT_KPR_MOD = 2.13; //Impact KPR modifier
		const double HLTV2_IMPACT_APR_MOD = 0.42; //Impact AssistPerRound modifier
		const double HLTV2_IMPACT_OFFSET_MOD = -0.41; //Impact base modifier
		const double HLTV2_ADR_MOD = 0.0032; // ADR modifier
		const double HLTV2_OFFSET_MOD = 0.1587; // HLTV2 base modifier
		

		// Money awards
		private const int LOSS_ROW_1 = 1400;
		private const int LOSS_ROW_2 = 1900;
		private const int LOSS_ROW_3 = 2400;
		private const int LOSS_ROW_4 = 2900;
		private const int LOSS_ROW_5 = 3400;
		private const int WIN_BOMB_TARGET = 3500;
		private const int WIN_BOMB_DEFUSED = 3500;
		private const int WIN_TIME_OVER = 3250;
		private const int WIN_ELIMINATION = 3250;
		private const int BOMB_PLANTED_BONUS = 800;
		private const int TEAMKILL = -300;

		private const int ESEA_ASSIST_THRESHOLD = 50;

		/// <summary>
		/// TODO use internal demoinfo detection https://github.com/StatsHelix/demoinfo/commit/62bb523cf36c757cffe3bed472971dc67b6511d1#r26059706
		/// As molotov thrower isn't networked eveytime, this 3 queues are used to know who thrown a moloto
		/// </summary>
		public readonly Queue<Player> LastPlayersThrownMolotov = new Queue<Player>();

		public readonly Queue<Player> LastPlayersFireStartedMolotov = new Queue<Player>();

		public readonly Queue<Player> LastPlayersFireEndedMolotov = new Queue<Player>();

		/// <summary>
		/// Used to distinct molotovs and icendiaries grenades.
		/// As FireEventArgs doesn't distinct molo / inc, we use it to know the right nade type on each FireNade events.
		/// </summary>
		public readonly Queue<EquipmentElement> LastNadeTypeThrown = new Queue<EquipmentElement>();

		/// <summary>
		/// Last player who thrown a flashbang, used for flashbangs stats
		/// </summary>
		public Player LastPlayerExplodedFlashbang { get; set; }

		public Queue<Player> PlayersFlashQueue = new Queue<Player>();

		protected Action<string, float> ProgressCallback;

		#endregion

		public abstract Task<Demo> AnalyzeDemoAsync(CancellationToken token, Action<string, float> progressCallback = null);

		protected abstract void RegisterEvents();

		protected abstract void HandleMatchStarted(object sender, MatchStartedEventArgs e);
		protected abstract void HandleRoundStart(object sender, RoundStartedEventArgs e);

		public static DemoAnalyzer Factory(Demo demo)
		{
			switch (demo.SourceName)
			{
				case Valve.NAME:
				case PopFlash.NAME:
				case Wanmei.NAME:
					return new ValveAnalyzer(demo);
				case Esea.NAME:
					return new EseaAnalyzer(demo);
				case Ebot.NAME:
				case Faceit.NAME:
				case Esl.NAME:
					return new EbotAnalyzer(demo);
				case Cevo.NAME:
					return new CevoAnalyzer(demo);
				case Pov.NAME:
					return null;
				default:
					return null;
			}
		}

		public static Demo ParseDemoHeader(string pathDemoFile)
		{
			DemoParser parser = new DemoParser(File.OpenRead(pathDemoFile));

			DateTime dateFile = File.GetCreationTime(pathDemoFile);

			if (dateFile > File.GetLastWriteTime(pathDemoFile))
				dateFile = File.GetLastWriteTime(pathDemoFile);

			Demo demo = new Demo
			{
				Name = Path.GetFileName(pathDemoFile),
				Path = pathDemoFile,
				Date = dateFile
			};

			try
			{
				parser.ParseHeader();
			}
			catch (Exception)
			{
				// Silently ignore no CSGO demos or unreadable file
				return null;
			}

			DemoHeader header = parser.Header;
			FileInfo fi = new FileInfo(demo.Path);
			int seconds = (int)fi.LastWriteTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
			demo.Id = header.MapName.Replace("/", "") + "_" + header.SignonLength + header.PlaybackTicks + header.PlaybackFrames + seconds + fi.Length;
			demo.ClientName = header.ClientName;
			demo.Hostname = header.ServerName;
			if (header.PlaybackTicks != 0 && header.PlaybackTime != 0)
			{
				demo.ServerTickrate = header.PlaybackTicks / header.PlaybackTime;
			}
			else
			{
				demo.Status = "corrupted";
			}
			if (header.PlaybackFrames != 0 && header.PlaybackTime != 0)
			{
				demo.Tickrate = (int)Math.Round((double)header.PlaybackFrames / header.PlaybackTime);
			}
			demo.Duration = header.PlaybackTime;
			// Valve maps moved from competitive to scrimmage (only mirage ATM) contains the suffix "_scrimmagemap" in the header.
			demo.MapName = header.MapName.Replace("_scrimmagemap", "");
			demo.Source = DetermineDemoSource(demo, header);
			demo.Ticks = header.PlaybackTicks;

			// Read .info file to get the real match date
			string infoFilePath = demo.Path + ".info";
			if (File.Exists(infoFilePath))
			{
				using (FileStream file = File.OpenRead(infoFilePath))
				{
					try
					{
						CDataGCCStrike15_v2_MatchInfo infoMsg = Serializer.Deserialize<CDataGCCStrike15_v2_MatchInfo>(file);
						DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
						demo.Date = date.AddSeconds(infoMsg.matchtime).ToLocalTime();
					}
					catch (Exception)
					{
						// silently ignore old .info files
						// Maybe add the old message to handle it?
					}
				}
			}

			return demo;
		}

		private static Source DetermineDemoSource(Demo demo, DemoHeader header)
		{
			// Check if it's a POV demo
			Match match = LocalRegex.Match(header.ServerName);
			if (match.Success || header.ServerName.Contains("localhost"))
			{
				demo.Type = Pov.NAME;
				return Source.Factory(Pov.NAME);
			}

			// Check for faceit demos
			// (Before May 2015) Faceit : uses regex - no false positive but could miss some Faceit demo (when premade playing because of custom team name)
			// (May 2015) Faceit : uses hostname
			if (demo.Hostname.Contains(Faceit.NAME, StringComparison.OrdinalIgnoreCase)
				|| FILENAME_FACEIT_REGEX.Match(demo.Name).Success)
			{
				return Source.Factory(Faceit.NAME);
			}

			// Check for cevo demos
			if (demo.Hostname.Contains(Cevo.NAME, StringComparison.OrdinalIgnoreCase))
			{
				return Source.Factory(Cevo.NAME);
			}

			// Check for ESL demos
			if (demo.Hostname.Contains(Esl.NAME, StringComparison.OrdinalIgnoreCase))
			{
				return Source.Factory(Esl.NAME);
			}

			// Check for ebot demos
			if (demo.Hostname.Contains(Ebot.NAME, StringComparison.OrdinalIgnoreCase)
				|| FILENAME_EBOT_REGEX.Match(demo.Name).Success
				|| demo.Hostname.Contains("ksnetwork.es", StringComparison.OrdinalIgnoreCase)
				)
			{
				return Source.Factory(Ebot.NAME);
			}

			// Check for esea demos, appart filename or hostname, there is no magic to detect it
			if (demo.Name.Contains(Esea.NAME, StringComparison.OrdinalIgnoreCase)
				|| demo.Hostname.Contains(Esea.NAME, StringComparison.OrdinalIgnoreCase))
			{
				return Source.Factory(Esea.NAME);
			}

			if (demo.Name.Contains(PopFlash.NAME, StringComparison.OrdinalIgnoreCase)
				|| demo.Hostname.Contains(PopFlash.NAME, StringComparison.OrdinalIgnoreCase))
			{
				return Source.Factory(PopFlash.NAME);
			}

			// If none of the previous checks matched, we use ValveAnalyzer
			return Source.Factory(Valve.NAME);
		}

		#region Events Handlers

		/// <summary>
		/// Handle each tick
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void HandleTickDone(object sender, TickDoneEventArgs e)
		{
			ProgressCallback?.Invoke(Demo.Id, Parser.ParsingProgess);

			if (!IsMatchStarted || IsFreezetime) return;

			if (AnalyzeFlashbang)
			{
				if (LastPlayerExplodedFlashbang != null)
				{
					foreach (DemoInfo.Player player in Parser.PlayingParticipants)
					{
						Player pl = Demo.Players.FirstOrDefault(p => p.SteamId == player.SteamID);
						if (pl != null && player.FlashDuration >= pl.FlashDurationTemp)
						{
							PlayerBlindedEvent playerBlindedEvent = new PlayerBlindedEvent(Parser.CurrentTick, Parser.CurrentTime)
							{
								ThrowerSteamId = LastPlayerExplodedFlashbang.SteamId,
								ThrowerName = LastPlayerExplodedFlashbang.Name,
								ThrowerTeamName = LastPlayerExplodedFlashbang.TeamName,
								VictimSteamId = pl.SteamId,
								VictimName = pl.Name,
								VictimTeamName = pl.TeamName,
								RoundNumber = CurrentRound.Number,
								Duration = player.FlashDuration - pl.FlashDurationTemp
							};
							Application.Current.Dispatcher.Invoke(() => Demo.PlayerBlinded.Add(playerBlindedEvent));
						}
					}
				}
				AnalyzeFlashbang = false;
			}

			if (!AnalyzePlayersPosition) return;

			if (Parser.PlayingParticipants.Any() && Demo.Players.Any())
			{
				// Update players position
				foreach (DemoInfo.Player player in Parser.PlayingParticipants)
				{
					if (!player.IsAlive) continue;
					Player pl = Demo.Players.FirstOrDefault(p => p.SteamId == player.SteamID);
					if (pl == null || pl.SteamId == 0) continue;

					PositionPoint positionPoint = new PositionPoint
					{
						X = player.Position.X,
						Y = player.Position.Y,
						RoundNumber = CurrentRound.Number,
						Team = player.Team.ToSide(),
						PlayerName = player.Name,
						PlayerSteamId = player.SteamID,
						PlayerHasBomb = player.Weapons.FirstOrDefault(w => w.Weapon == EquipmentElement.Bomb) != null
					};
					Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
				}
			}
		}

		/// <summary>
		/// killer == null => world
		/// killer.SteamID == 0 => BOT
		/// if a player is killed by bomb explosion, killer == planter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void HandlePlayerKilled(object sender, PlayerKilledEventArgs e)
		{
			if (!IsMatchStarted || IsFreezetime || e.Victim == null) return;

			Player killed = Demo.Players.FirstOrDefault(player => player.SteamId == e.Victim.SteamID);
			if (killed == null) return;
			Weapon weapon = Weapon.WeaponList.FirstOrDefault(w => w.Element == e.Weapon.Weapon);
			if (weapon == null) return;
			Player killer = null;
			Player assister = null;

			KillEvent killEvent = new KillEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				Weapon = weapon,
				KillerSteamId = e.Killer?.SteamID ?? 0,
				KillerName = e.Killer?.Name ?? string.Empty,
				KillerSide = e.Killer?.Team.ToSide() ?? Side.None,
				KilledSide = e.Victim.Team.ToSide(),
				KilledSteamId = e.Victim.SteamID,
				KilledName = e.Victim.Name,
				KilledTeam = killed.TeamName,
				VictimIsBlinded = e.Victim.FlashDuration > 0,
				KillerVelocityX = e.Killer?.Velocity.X ?? 0,
				KillerVelocityY = e.Killer?.Velocity.Y ?? 0,
				KillerVelocityZ = e.Killer?.Velocity.Z ?? 0,
				KillerIsBlinded = e.Killer?.FlashDuration > 0,
				RoundNumber = CurrentRound.Number,
				IsHeadshot = e.Headshot,
				IsKillerCrouching = e.Killer?.IsDucking ?? false,
				Point = new KillHeatmapPoint
				{
					KillerX = e.Killer?.Position.X ?? 0,
					KillerY = e.Killer?.Position.Y ?? 0,
					VictimX = e.Victim.Position.X,
					VictimY = e.Victim.Position.Y
				},
				TimeDeathSeconds = (float)Math.Round((Parser.IngameTick - CurrentRound.FreezetimeEndTick) / Demo.ServerTickrate, 2),
			};

			killed.IsAlive = false;
			killed.TimeDeathRounds[CurrentRound.Number] = killEvent.TimeDeathSeconds;
			if (e.Killer != null)
			{
				killer = Demo.Players.FirstOrDefault(player => player.SteamId == e.Killer.SteamID);
			}
			else
			{
				// suicide, probably because he missed the jump from upper B on train :)
				killed.SuicideCount++;
			}

			if (killer != null )
			{
				killEvent.KillerTeam = killer.TeamName;
				// add kill to the current round only if it's not a TK / suicide
				if (e.Killer.Team != e.Victim.Team)
				{
					if (!KillsThisRound.ContainsKey(e.Killer))
					{
						KillsThisRound[e.Killer] = 0;
					}
					KillsThisRound[e.Killer]++;
				}

				// add money reward only if it's not a suicide
				if (e.Victim.SteamID != killer.SteamId)
				{
					if (!killer.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
						killer.RoundsMoneyEarned[CurrentRound.Number] = weapon.KillAward;
					else
						killer.RoundsMoneyEarned[CurrentRound.Number] += weapon.KillAward;
				}

				// if it's a TK, the player lost 300$
				if (e.Killer.Team == e.Victim.Team)
				{
					if (killer.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
						killer.RoundsMoneyEarned[CurrentRound.Number] += TEAMKILL;
				}
				ProcessTradeKill(killEvent);
			}

			if (e.Assister != null)
			{
				if (e.Killer != null && this is EseaAnalyzer)
				{
					Dictionary<long, int> damagesPerPlayer = new Dictionary<long, int>();
					foreach (PlayerHurtedEvent hurtedEvent in CurrentRound.PlayersHurted)
					{
						if (hurtedEvent.HurtedSteamId == e.Victim.SteamID && hurtedEvent.AttackerSteamId != e.Killer.SteamID)
						{
							if (!damagesPerPlayer.ContainsKey(hurtedEvent.AttackerSteamId))
								damagesPerPlayer[hurtedEvent.AttackerSteamId] = hurtedEvent.HealthDamage;
							else
								damagesPerPlayer[hurtedEvent.AttackerSteamId] += hurtedEvent.HealthDamage;
						}
					}
					if (damagesPerPlayer.Any())
					{
						KeyValuePair<long, int> higherDamageDone = damagesPerPlayer.FirstOrDefault(d => d.Value == damagesPerPlayer.Values.Max());
						if (higherDamageDone.Value > ESEA_ASSIST_THRESHOLD)
						{
							assister = Demo.Players.FirstOrDefault(player => player.SteamId == higherDamageDone.Key);
						}
					}
				}
				else
				{
					assister = Demo.Players.FirstOrDefault(player => player.SteamId == e.Assister.SteamID);
				}
			}

			ProcessOpenAndEntryKills(killEvent);
			ProcessClutches();
			ProcessPlayersRating();

			if (assister != null)
			{
				killEvent.AssisterSteamId = assister.SteamId;
				killEvent.AssisterName = assister.Name;
				Application.Current.Dispatcher.Invoke(() => assister.Assists.Add(killEvent));
			}
			Application.Current.Dispatcher.Invoke(delegate
			{
				Demo.Kills.Add(killEvent);
				CurrentRound.Kills.Add(killEvent);
				killer?.Kills.Add(killEvent);
				killed.Deaths.Add(killEvent);
			});

			if (AnalyzePlayersPosition)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Victim.Position.X,
					Y = e.Victim.Position.Y,
					PlayerName = e.Killer?.Name ?? string.Empty,
					PlayerSteamId = e.Killer?.SteamID ?? 0,
					Team = e.Killer?.Team.ToSide() ?? Side.None,
					Event = killEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleRoundEnd(object sender, RoundEndedEventArgs e)
		{
			IsRoundEndOccured = true;
			if (!IsMatchStarted || IsFreezetime) return;

			CurrentRound.EndTick = Parser.IngameTick;
			CurrentRound.Duration = (float)Math.Round((CurrentRound.EndTick - CurrentRound.Tick) / Demo.ServerTickrate, 2);
			CurrentRound.EndReason = e.Reason;
			UpdateTeamScore(e);
			ProcessRoundEndReward(e);
			if (e.Reason == RoundEndReason.CTSurrender)
			{
				Demo.Surrender = IsHalfMatch ? Demo.TeamT : Demo.TeamCT;
			}
			else if (e.Reason == RoundEndReason.TerroristsSurrender)
			{
				Demo.Surrender = IsHalfMatch ? Demo.TeamCT : Demo.TeamT;
			}

			Application.Current.Dispatcher.Invoke(delegate
			{
				// update round won status for entry kills / hold kills
				if (e.Winner.ToSide() == CurrentRound.EntryKillEvent?.KillerSide) CurrentRound.EntryKillEvent.HasWonRound = true;
				if (e.Winner.ToSide() == CurrentRound.EntryHoldKillEvent?.KillerSide) CurrentRound.EntryHoldKillEvent.HasWonRound = true;
				List<Player> playerWithEntryKill = Demo.Players.Where(p => p.HasEntryKill).ToList();
				foreach (Player player in playerWithEntryKill)
				{
					if (player.Side == e.Winner.ToSide()) player.EntryKills.Last().HasWonRound = true;
				}
				List<Player> playerWithEntryHoldKill = Demo.Players.Where(p => p.HasEntryHoldKill).ToList();
				foreach (Player player in playerWithEntryHoldKill)
				{
					if (player.Side == e.Winner.ToSide()) player.EntryHoldKills.Last().HasWonRound = true;
				}

				ComputeEseaRws();
			});
		}

		/// <summary>
		/// Between round_end and round_officialy_ended events may happened so we update data at round_officialy_end
		/// However at the last round of a match, round officially end isn't raised (on Valve demos)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void HandleRoundOfficiallyEnd(object sender, RoundOfficiallyEndedEventArgs e)
		{
			if (!IsMatchStarted || IsFreezetime) return;

			CurrentRound.EndTickOfficially = Parser.IngameTick;
			CurrentRound.Duration = (float)Math.Round((CurrentRound.EndTickOfficially - CurrentRound.Tick) / Demo.ServerTickrate, 2);

			// sometimes round_end isn't triggered, I update scores here
			if (!IsRoundEndOccured)
			{
				Team teamCt = GetTeamBySide(Side.CounterTerrorist);
				if (teamCt.Score < Parser.CTScore)
				{
					UpdateTeamScore(new RoundEndedEventArgs
					{
						Winner = DemoInfo.Team.CounterTerrorist
					});
				}
				else
				{
					UpdateTeamScore(new RoundEndedEventArgs
					{
						Winner = DemoInfo.Team.Terrorist
					});
				}
			}

			CheckForSpecialClutchEnd();
			UpdateKillsCount();
			UpdatePlayerScore();

			if (!IsLastRoundHalf || !IsRoundEndOccured)
			{
				Application.Current.Dispatcher.Invoke(() => Demo.Rounds.Add(CurrentRound));
			}

			if (!IsOvertime && IsLastRoundHalf)
			{
				IsLastRoundHalf = false;
				IsHalfMatch = true;
			}

			if (IsSwapTeamRequired)
			{
				SwapTeams();
				IsSwapTeamRequired = false;
				if (IsOvertime) IsHalfMatch = !IsHalfMatch;
			}

			Demo.RaiseScoresChanged();
		}

		protected void HandleFreezetimeEnded(object sender, FreezetimeEndedEventArgs e)
		{
			if (!IsMatchStarted) return;

			IsFreezetime = false;
			CurrentRound.FreezetimeEndTick = Parser.IngameTick;
		}

		protected void HandleBombPlanted(object sender, BombEventArgs e)
		{
			if (!IsMatchStarted || e.Player == null) return;

			BombPlantedEvent bombPlantedEvent = new BombPlantedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				PlanterSteamId = e.Player.SteamID,
				PlanterName = e.Player.Name,
				Site = e.Site.ToString(),
				X = e.Player.Position.X,
				Y = e.Player.Position.Y
			};

			if (e.Player.SteamID != 0)
			{
				Player player = Demo.Players.FirstOrDefault(p => p.SteamId == e.Player.SteamID);
				if (player != null) player.BombPlantedCount++;
			}
			Demo.BombPlantedCount++;
			CurrentRound.BombPlantedCount++;
			Application.Current.Dispatcher.Invoke(() => Demo.BombPlanted.Add(bombPlantedEvent));
			CurrentRound.BombPlanted = bombPlantedEvent;

			if (AnalyzePlayersPosition && e.Player.SteamID != 0)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Player.Position.X,
					Y = e.Player.Position.Y,
					PlayerSteamId = e.Player.SteamID,
					PlayerName = e.Player.Name,
					Team = e.Player.Team.ToSide(),
					Event = bombPlantedEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleBombDefused(object sender, BombEventArgs e)
		{
			if (!IsMatchStarted || e.Player == null) return;

			BombDefusedEvent bombDefusedEvent = new BombDefusedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				DefuserSteamId = e.Player.SteamID,
				DefuserName = e.Player.Name,
				Site = e.Site.ToString()
			};

			if (e.Player.SteamID != 0)
			{
				Player player = Demo.Players.FirstOrDefault(p => p.SteamId == e.Player.SteamID);
				if (player != null) player.BombDefusedCount++;
			}
			Demo.BombDefusedCount++;
			CurrentRound.BombDefusedCount++;
			Application.Current.Dispatcher.Invoke(() => Demo.BombDefused.Add(bombDefusedEvent));
			CurrentRound.BombDefused = bombDefusedEvent;

			if (AnalyzePlayersPosition && bombDefusedEvent.DefuserSteamId != 0)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Player.Position.X,
					Y = e.Player.Position.Y,
					PlayerSteamId = e.Player.SteamID,
					PlayerName = e.Player.Name,
					Team = e.Player.Team.ToSide(),
					Event = bombDefusedEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleBombExploded(object sender, BombEventArgs e)
		{
			if (!IsMatchStarted || e.Player == null) return;

			BombExplodedEvent bombExplodedEvent = new BombExplodedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				Site = e.Site.ToString(),
				PlanterSteamId = e.Player.SteamID,
				PlanterName = e.Player.Name
			};

			Player planter = Demo.Players.FirstOrDefault(p => p.SteamId == e.Player.SteamID);
			if (planter != null) planter.BombExplodedCount++;
			Demo.BombExplodedCount++;
			CurrentRound.BombExplodedCount++;
			Application.Current.Dispatcher.Invoke(() => Demo.BombExploded.Add(bombExplodedEvent));
			CurrentRound.BombExploded = bombExplodedEvent;

			if (AnalyzePlayersPosition && planter != null)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = Demo.BombPlanted.Last().X,
					Y = Demo.BombPlanted.Last().Y,
					PlayerSteamId = e.Player.SteamID,
					PlayerName = e.Player.Name,
					Team = e.Player.Team.ToSide(),
					Event = bombExplodedEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleWeaponFired(object sender, WeaponFiredEventArgs e)
		{
			if (!IsMatchStarted || e.Shooter == null || e.Weapon == null) return;

			if (!IsFirstShotOccured)
			{
				IsFreezetime = false; // fix for demos that round_freeze_end sometimes not fired https://github.com/akiver/CSGO-Demos-Manager/issues/273
				IsFirstShotOccured = true;
				// update the equipement value for each player
				foreach (DemoInfo.Player pl in Parser.PlayingParticipants)
				{
					Player player = Demo.Players.FirstOrDefault(p => p.SteamId == pl.SteamID);
					if (player != null) player.EquipementValueRounds[CurrentRound.Number] = pl.CurrentEquipmentValue;
				}

				CurrentRound.EquipementValueTeamCt = Parser.Participants.Where(a => a.Team.ToSide() == Side.CounterTerrorist).Sum(a => a.CurrentEquipmentValue);
				CurrentRound.EquipementValueTeamT = Parser.Participants.Where(a => a.Team.ToSide() == Side.Terrorist).Sum(a => a.CurrentEquipmentValue);

				// Not 100% accurate maybe improved it with current equipement...
				if (CurrentRound.StartMoneyTeamCt == 4000 && CurrentRound.StartMoneyTeamT == 4000)
				{
					CurrentRound.Type = RoundType.PISTOL_ROUND;
				}
				else
				{
					double diffPercent = Math.Abs(Math.Round((((double)CurrentRound.EquipementValueTeamCt - CurrentRound.EquipementValueTeamT) / (((double)CurrentRound.EquipementValueTeamCt + CurrentRound.EquipementValueTeamT) / 2) * 100), 2));
					if (diffPercent >= 90)
					{
						CurrentRound.Type = RoundType.ECO;
					}
					else if (diffPercent >= 75 && diffPercent < 90)
					{
						CurrentRound.Type = RoundType.SEMI_ECO;
					}
					else if (diffPercent >= 50 && diffPercent < 75)
					{
						CurrentRound.Type = RoundType.FORCE_BUY;
					}
					else
					{
						CurrentRound.Type = RoundType.NORMAL;
					}

					if (CurrentRound.Type != RoundType.NORMAL)
					{
						CurrentRound.SideTrouble = CurrentRound.EquipementValueTeamCt > CurrentRound.EquipementValueTeamT
							? Side.Terrorist : Side.CounterTerrorist;
						CurrentRound.TeamTroubleName = GetTeamBySide(CurrentRound.SideTrouble).Name;
					}
				}
			}

			Player shooter = Demo.Players.FirstOrDefault(p => p.SteamId == e.Shooter.SteamID);
			Weapon weapon = Weapon.WeaponList.FirstOrDefault(w => w.Element == e.Weapon.Weapon);
			if (shooter == null || weapon == null) return;
			shooter.ShotCount++;

			WeaponFireEvent shoot = new WeaponFireEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				ShooterSteamId = shooter.SteamId,
				ShooterName = shooter.Name,
				Weapon = weapon,
				RoundNumber = CurrentRound.Number,
				ShooterVelocityX = e.Shooter.Velocity.X,
				ShooterVelocityY = e.Shooter.Velocity.Y,
				ShooterVelocityZ = e.Shooter.Velocity.Z,
				ShooterSide = e.Shooter.Team.ToSide(),
				Point = new HeatmapPoint
				{
					X = e.Shooter.Position.X,
					Y = e.Shooter.Position.Y
				},
				ShooterPosX = e.Shooter.Position.X,
				ShooterPosY = e.Shooter.Position.Y,
				ShooterPosZ = e.Shooter.Position.Z,
				ShooterAnglePitch = e.Shooter.ViewDirectionY,
				ShooterAngleYaw = e.Shooter.ViewDirectionX
			};

			if (e.Weapon.Class == EquipmentClass.Grenade)
			{
				Application.Current.Dispatcher.Invoke(() => CurrentRound.WeaponFired.Add(shoot));
			}

			switch (e.Weapon.Weapon)
			{
				case EquipmentElement.Incendiary:
					shooter.IncendiaryThrownCount++;
					LastNadeTypeThrown.Enqueue(EquipmentElement.Incendiary);
					break;
				case EquipmentElement.Molotov:
					LastNadeTypeThrown.Enqueue(EquipmentElement.Molotov);
					shooter.MolotovThrownCount++;
					break;
				case EquipmentElement.Decoy:
					shooter.DecoyThrownCount++;
					break;
				case EquipmentElement.Flash:
					shooter.FlashbangThrownCount++;
					PlayersFlashQueue.Enqueue(shooter);
					break;
				case EquipmentElement.HE:
					shooter.HeGrenadeThrownCount++;
					break;
				case EquipmentElement.Smoke:
					shooter.SmokeThrownCount++;
					break;
			}

			Application.Current.Dispatcher.Invoke(() => Demo.WeaponFired.Add(shoot));

			if (e.Shooter.SteamID == 0) return;

			switch (e.Weapon.Weapon)
			{
				case EquipmentElement.Incendiary:
				case EquipmentElement.Molotov:
					LastPlayersThrownMolotov.Enqueue(Demo.Players.First(p => p.SteamId == e.Shooter.SteamID));
					if (!AnalyzePlayersPosition) return;
					goto case EquipmentElement.Decoy;
				case EquipmentElement.Decoy:
				case EquipmentElement.Flash:
				case EquipmentElement.HE:
				case EquipmentElement.Smoke:
					PositionPoint positionPoint = new PositionPoint
					{
						X = e.Shooter.Position.X,
						Y = e.Shooter.Position.Y,
						PlayerSteamId = e.Shooter.SteamID,
						PlayerName = e.Shooter.Name,
						Team = e.Shooter.Team.ToSide(),
						Event = shoot,
						RoundNumber = CurrentRound.Number
					};
					Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
					break;
			}
		}

		protected void HandleFireNadeStarted(object sender, FireEventArgs e)
		{
			// ignore molotovs thrown by a BOT since we didn't add BOT in queues
			if (!IsMatchStarted || e.ThrownBy != null && e.ThrownBy.SteamID == 0) return;

			switch (e.NadeType)
			{
				case EquipmentElement.Incendiary:
				case EquipmentElement.Molotov:
					MolotovFireStartedEvent molotovEvent = new MolotovFireStartedEvent(Parser.IngameTick, Parser.CurrentTime)
					{
						Point = new HeatmapPoint
						{
							X = e.Position.X,
							Y = e.Position.Y
						},
						RoundNumber = CurrentRound.Number
					};
					Player thrower = null;
					if (e.ThrownBy != null)
					{
						thrower = Demo.Players.FirstOrDefault(p => p.SteamId == e.ThrownBy.SteamID);
						molotovEvent.ThrowerSide = e.ThrownBy.Team.ToSide();
					}

					if (LastPlayersThrownMolotov.Any())
					{
						LastPlayersFireStartedMolotov.Enqueue(LastPlayersThrownMolotov.Peek());
						// Remove the last player who thrown a molo
						thrower = LastPlayersThrownMolotov.Dequeue();
					}

					if (thrower != null)
					{
						molotovEvent.ThrowerSteamId = thrower.SteamId;
						molotovEvent.ThrowerName = thrower.Name;

						if (AnalyzePlayersPosition)
						{
							PositionPoint positionPoint = new PositionPoint
							{
								X = e.Position.X,
								Y = e.Position.Y,
								PlayerSteamId = thrower.SteamId,
								PlayerName = thrower.Name,
								Team = thrower.Side,
								Event = molotovEvent,
								RoundNumber = CurrentRound.Number
							};
							Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
						}
					}

					if (LastNadeTypeThrown.Count > 0)
					{
						EquipmentElement lastNadeType = LastNadeTypeThrown.Dequeue();
						if (lastNadeType == EquipmentElement.Molotov)
							Application.Current.Dispatcher.Invoke(() => Demo.MolotovsFireStarted.Add(molotovEvent));
						else
							Application.Current.Dispatcher.Invoke(() => Demo.IncendiariesFireStarted.Add(molotovEvent));
					}
					break;
			}
		}

		protected void HandleFireNadeEnded(object sender, FireEventArgs e)
		{
			if (!AnalyzePlayersPosition || !IsMatchStarted) return;

			switch (e.NadeType)
			{
				case EquipmentElement.Incendiary:
				case EquipmentElement.Molotov:
					MolotovFireEndedEvent molotovEvent = new MolotovFireEndedEvent(Parser.IngameTick, Parser.CurrentTime)
					{
						Point = new HeatmapPoint
						{
							X = e.Position.X,
							Y = e.Position.Y
						}
					};

					Player thrower = null;

					// Thrower is not indicated every time
					if (e.ThrownBy != null)
					{
						thrower = Demo.Players.FirstOrDefault(player => player.SteamId == e.ThrownBy.SteamID);
					}

					if (LastPlayersFireStartedMolotov.Any())
					{
						LastPlayersFireEndedMolotov.Enqueue(LastPlayersFireStartedMolotov.Peek());
						// Remove the last player who started a fire
						thrower = LastPlayersFireStartedMolotov.Dequeue();
					}

					if (thrower != null)
					{
						molotovEvent.ThrowerSteamId = thrower.SteamId;
						molotovEvent.ThrowerName = thrower.Name;

						PositionPoint positionPoint = new PositionPoint
						{
							X = e.Position.X,
							Y = e.Position.Y,
							PlayerSteamId = thrower.SteamId,
							PlayerName = thrower.Name,
							Team = thrower.Side,
							Event = molotovEvent,
							RoundNumber = CurrentRound.Number
						};
						Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
					}
					break;
			}
		}

		protected void HandleExplosiveNadeExploded(object sender, GrenadeEventArgs e)
		{
			if (!IsMatchStarted || e.ThrownBy == null) return;
			Player thrower = Demo.Players.FirstOrDefault(player => player.SteamId == e.ThrownBy.SteamID);
			ExplosiveNadeExplodedEvent explosiveEvent = new ExplosiveNadeExplodedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				ThrowerSteamId = thrower?.SteamId ?? 0,
				ThrowerName = thrower == null ? string.Empty : thrower.Name,
				ThrowerSide = e.ThrownBy.Team.ToSide(),
				Point = new HeatmapPoint
				{
					X = e.Position.X,
					Y = e.Position.Y
				}
			};

			Application.Current.Dispatcher.Invoke(() => CurrentRound.ExplosiveGrenadesExploded.Add(explosiveEvent));

			if (AnalyzePlayersPosition && thrower != null)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Position.X,
					Y = e.Position.Y,
					PlayerSteamId = e.ThrownBy.SteamID,
					PlayerName = e.ThrownBy.Name,
					Team = e.ThrownBy.Team.ToSide(),
					Event = explosiveEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleFlashNadeExploded(object sender, FlashEventArgs e)
		{
			if(!IsMatchStarted || e.ThrownBy == null || !PlayersFlashQueue.Any()) return;

			if (PlayersFlashQueue.Any())
			{
				LastPlayerExplodedFlashbang = PlayersFlashQueue.Dequeue();
				// update flash intensity value for each player when the flash poped
				foreach (DemoInfo.Player player in Parser.PlayingParticipants)
				{
					Player pl = Demo.Players.FirstOrDefault(p => p.SteamId == player.SteamID);
					if (pl != null) pl.FlashDurationTemp = player.FlashDuration;
				}
				// set it to true to start analyzing flashbang status at each tick
				AnalyzeFlashbang = true;
			}

			Player thrower = Demo.Players.FirstOrDefault(player => player.SteamId == e.ThrownBy.SteamID);
			FlashbangExplodedEvent flashbangEvent = new FlashbangExplodedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				ThrowerSteamId = thrower?.SteamId ?? 0,
				ThrowerName = thrower == null ? string.Empty : thrower.Name,
				ThrowerSide = e.ThrownBy.Team.ToSide(),
				Point = new HeatmapPoint
				{
					X = e.Position.X,
					Y = e.Position.Y
				},
				RoundNumber = CurrentRound.Number,
			};

			if (e.FlashedPlayers != null)
			{
				foreach (DemoInfo.Player pl in e.FlashedPlayers)
				{
					Player player = Demo.Players.FirstOrDefault(p => p.SteamId == pl.SteamID);
					if (player != null)
					{
						flashbangEvent.FlashedPlayerSteamIdList.Add(player.SteamId);
					}
				}
			}

			Application.Current.Dispatcher.Invoke(() => CurrentRound.FlashbangsExploded.Add(flashbangEvent));

			if (AnalyzePlayersPosition && thrower != null)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Position.X,
					Y = e.Position.Y,
					PlayerSteamId = e.ThrownBy.SteamID,
					PlayerName = e.ThrownBy.Name,
					Team = e.ThrownBy.Team.ToSide(),
					RoundNumber = CurrentRound.Number,
					Event = flashbangEvent
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleSmokeNadeStarted(object sender, SmokeEventArgs e)
		{
			if (!IsMatchStarted || e.ThrownBy == null) return;

			Player thrower = Demo.Players.FirstOrDefault(player => player.SteamId == e.ThrownBy.SteamID);

			SmokeNadeStartedEvent smokeEvent = new SmokeNadeStartedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				ThrowerSteamId = thrower?.SteamId ?? 0,
				ThrowerName = thrower == null ? string.Empty : thrower.Name,
				ThrowerSide = e.ThrownBy.Team.ToSide(),
				Point = new HeatmapPoint
				{
					X = e.Position.X,
					Y = e.Position.Y
				}
			};

			Application.Current.Dispatcher.Invoke(() => CurrentRound.SmokeStarted.Add(smokeEvent));

			if (AnalyzePlayersPosition && thrower != null)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Position.X,
					Y = e.Position.Y,
					PlayerSteamId = e.ThrownBy.SteamID,
					PlayerName = e.ThrownBy.Name,
					Team = e.ThrownBy.Team.ToSide(),
					Event = smokeEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleSmokeNadeEnded(object sender, SmokeEventArgs e)
		{
			if (!AnalyzePlayersPosition || !IsMatchStarted || e.ThrownBy == null) return;

			Player thrower = Demo.Players.FirstOrDefault(player => player.SteamId == e.ThrownBy.SteamID);

			SmokeNadeEndedEvent smokeEvent = new SmokeNadeEndedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				ThrowerSteamId = thrower?.SteamId ?? 0,
				ThrowerName = thrower == null ? string.Empty : thrower.Name
			};

			if (thrower != null)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Position.X,
					Y = e.Position.Y,
					PlayerName = e.ThrownBy.Name,
					PlayerSteamId = e.ThrownBy.SteamID,
					Team = e.ThrownBy.Team.ToSide(),
					Event = smokeEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleDecoyNadeStarted(object sender, DecoyEventArgs e)
		{
			if (!IsMatchStarted || e.ThrownBy == null) return;

			Player thrower = Demo.Players.FirstOrDefault(player => player.SteamId == e.ThrownBy.SteamID);
			DecoyStartedEvent decoyStartedEvent = new DecoyStartedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				ThrowerSteamId = thrower?.SteamId ?? 0,
				ThrowerName = thrower == null ? string.Empty : thrower.Name,
				RoundNumber = CurrentRound.Number,
				ThrowerSide = e.ThrownBy.Team.ToSide(),
				Point = new HeatmapPoint
				{
					X = e.Position.X,
					Y = e.Position.Y
				}
			};
			Application.Current.Dispatcher.Invoke(() => Demo.DecoyStarted.Add(decoyStartedEvent));

			if (AnalyzePlayersPosition && thrower != null)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Position.X,
					Y = e.Position.Y,
					PlayerSteamId = e.ThrownBy.SteamID,
					PlayerName = e.ThrownBy.Name,
					Team = e.ThrownBy.Team.ToSide(),
					Event = decoyStartedEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleDecoyNadeEnded(object sender, DecoyEventArgs e)
		{
			if (!AnalyzePlayersPosition || !IsMatchStarted || e.ThrownBy == null) return;

			Player thrower = Demo.Players.FirstOrDefault(player => player.SteamId == e.ThrownBy.SteamID);
			DecoyEndedEvent decoyEndedEvent = new DecoyEndedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				ThrowerSteamId = thrower?.SteamId ?? 0,
				ThrowerName = thrower == null ? string.Empty : thrower.Name
			};

			if (thrower != null)
			{
				PositionPoint positionPoint = new PositionPoint
				{
					X = e.Position.X,
					Y = e.Position.Y,
					PlayerSteamId = e.ThrownBy.SteamID,
					PlayerName = e.ThrownBy.Name,
					Team = e.ThrownBy.Team.ToSide(),
					Event = decoyEndedEvent,
					RoundNumber = CurrentRound.Number
				};
				Application.Current.Dispatcher.Invoke(() => Demo.PositionPoints.Add(positionPoint));
			}
		}

		protected void HandleRoundMvp(object sender, RoundMVPEventArgs e)
		{
			if (!IsMatchStarted || IsFreezetime || e.Player == null || e.Player.SteamID == 0) return;
			Player playerMvp = Demo.Players.FirstOrDefault(player => player.SteamId == e.Player.SteamID);
			if (playerMvp != null) playerMvp.RoundMvpCount++;
		}

		/// <summary>
		/// When a player is hurted
		/// Trigerred only with demos > 06/30/2015
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void HandlePlayerHurted(object sender, PlayerHurtEventArgs e)
		{
			if (!IsMatchStarted || e.Player == null || e.Weapon == null) return;

			// may be a bot on MM demos
			Player hurted = Demo.Players.FirstOrDefault(player => player.SteamId == e.Player.SteamID);
			Weapon weapon = Weapon.WeaponList.FirstOrDefault(w => w.Element == e.Weapon.Weapon);
			if (hurted == null || weapon == null) return;
			Player attacker = null;
			// attacker may be null (hurted by world)
			if (e.Attacker != null) attacker = Demo.Players.FirstOrDefault(player => player.SteamId == e.Attacker.SteamID);
			if (attacker != null) attacker.HitCount++;

			PlayerHurtedEvent playerHurtedEvent = new PlayerHurtedEvent(Parser.IngameTick, Parser.CurrentTime)
			{
				AttackerSteamId = attacker?.SteamId ?? 0,
				AttackerSide = e.Attacker?.Team.ToSide() ?? Side.None,
				HurtedSteamId = hurted.SteamId,
				ArmorDamage = e.ArmorDamage,
				HealthDamage = e.Player.HP < e.HealthDamage ? e.Player.HP : e.HealthDamage,
				HitGroup = e.Hitgroup,
				Weapon = weapon,
				RoundNumber = CurrentRound.Number,
				VictimX = e.Player.Position.X,
				VictimY = e.Player.Position.Y,
				AttackerX = e.Attacker?.Position.X ?? 0,
				AttackerY = e.Attacker?.Position.Y ?? 0,
			};

			Application.Current.Dispatcher.Invoke(delegate
			{
				Demo.PlayersHurted.Add(playerHurtedEvent);
				attacker?.PlayersHurted.Add(playerHurtedEvent);
				hurted.PlayersHurted.Add(playerHurtedEvent);
				CurrentRound.PlayersHurted.Add(playerHurtedEvent);
			});
		}

		/// <summary>
		/// Handler for when a player disconnect from the server
		/// Used to avoid wrong clutch count as the player may be considered as alive and in a specific team whereas he is not
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void HandlePlayerDisconnect(object sender, PlayerDisconnectEventArgs e)
		{
			if (e.Player == null) return;
			Player playerDisconnected = Demo.Players.FirstOrDefault(p => p.SteamId == e.Player.SteamID);
			if (playerDisconnected == null) return;
			playerDisconnected.IsAlive = false;
			playerDisconnected.Side = Side.None;
			playerDisconnected.IsConnected = false;
		}

		protected void HandlePlayerTeam(object sender, PlayerTeamEventArgs e)
		{
			if (e.Swapped == null || e.Swapped.SteamID == 0) return;
			Player player = Demo.Players.FirstOrDefault(p => p.SteamId == e.Swapped.SteamID);
			if (player == null)
			{
				Player newPlayer = new Player
				{
					SteamId = e.Swapped.SteamID,
					Name = e.Swapped.Name,
					Side = e.NewTeam.ToSide()
				};
				Application.Current.Dispatcher.Invoke(() => Demo.Players.Add(newPlayer));
				if (e.NewTeam.ToSide() == Side.CounterTerrorist)
				{
					Application.Current.Dispatcher.Invoke(() => Demo.TeamCT.Players.Add(newPlayer));
					newPlayer.TeamName = Demo.TeamCT.Name;
				}
				else
				{
					Application.Current.Dispatcher.Invoke(() => Demo.TeamT.Players.Add(newPlayer));
					newPlayer.TeamName = Demo.TeamT.Name;
				}
				return;
			}
			player.IsConnected = true;
			player.Side = e.NewTeam.ToSide();
		}

		protected void HandleSayText2(object sender, SayText2EventArgs e)
		{
			e.Text = CleanUpChatText(e.Text);
			string message = string.Empty;
			if (e.Sender != null) message += e.Sender.Name + ": ";
			message += e.Text;
			Demo.ChatMessageList.Add(message);
		}

		protected void HandleSayText(object sender, SayTextEventArgs e)
		{
			e.Text = CleanUpChatText(e.Text);
			Demo.ChatMessageList.Add(e.Text);
		}

		#endregion

		#region Process

		/// <summary>
		/// Update kills count for each player, current round and demo
		/// </summary>
		protected void UpdateKillsCount()
		{
			foreach (KeyValuePair<DemoInfo.Player, int> pair in KillsThisRound)
			{
				// Keep individual kills for each players
				Player player = Demo.Players.FirstOrDefault(p => p.SteamId == pair.Key.SteamID);
				if (player == null) continue;
				switch (pair.Value)
				{
					case 1:
						player.OneKillCount++;
						Demo.OneKillCount++;
						break;
					case 2:
						player.TwoKillCount++;
						Demo.TwoKillCount++;
						break;
					case 3:
						player.ThreeKillCount++;
						Demo.ThreeKillCount++;
						break;
					case 4:
						player.FourKillCount++;
						Demo.FourKillCount++;
						break;
					case 5:
						player.FiveKillCount++;
						Demo.FiveKillCount++;
						break;
				}
			}
		}

		protected void CreateNewRound(bool force = false)
		{
			if (IsSwapTeamRequired) SwapTeams();
			PlayerTeamCount = 0;
			// do not create a new round if the freezetime didn't ended, it means the game is paused
			if (!force && IsFreezetime) return;

			CurrentRound = new Round
			{
				Tick = Parser.IngameTick,
				Number = Demo.Rounds.Count + 1,
				SideTrouble = Side.None,
				WinnerSide = Side.None,
				TeamTname = GetTeamBySide(Side.Terrorist).Name,
				TeamCtName = GetTeamBySide(Side.CounterTerrorist).Name,
				EndTick = 0,
				EndTickOfficially = 0,
			};

			IsFreezetime = true;
			IsRoundEndOccured = false;
			CurrentClutch = null;
			LastPlayerExplodedFlashbang = null;
			IsFirstKillOccured = false;
			IsFirstShotOccured = false;
			_playerInClutch1 = null;
			_playerInClutch2 = null;
			KillsThisRound.Clear();
			ClearQueues();

			// Update players infos
			foreach (Player pl in Demo.Players)
			{
				pl.HasEntryKill = false;
				pl.HasEntryHoldKill = false;
				pl.IsControllingBot = false;
				if (pl.IsConnected) pl.RoundPlayedCount++;
				if (!pl.RoundsMoneyEarned.ContainsKey(CurrentRound.Number)) pl.RoundsMoneyEarned.Add(CurrentRound.Number, 0);
				if (!pl.StartMoneyRounds.ContainsKey(CurrentRound.Number)) pl.StartMoneyRounds.Add(CurrentRound.Number, 0);
				if (!pl.EquipementValueRounds.ContainsKey(CurrentRound.Number)) pl.EquipementValueRounds.Add(CurrentRound.Number, 0);
				if (!pl.TimeDeathRounds.ContainsKey(CurrentRound.Number)) pl.TimeDeathRounds.Add(CurrentRound.Number, 0);
				DemoInfo.Player player = Parser.PlayingParticipants.FirstOrDefault(p => p.SteamID == pl.SteamId);
				if (player != null)
				{
					pl.Side = player.Team.ToSide();
					pl.StartMoneyRounds[CurrentRound.Number] = player.Money;
					pl.IsAlive = true;
				}
				else
				{
					pl.IsAlive = false;
				}
			}

			// Sometimes parser return wrong money values on 1st round of side
			if (CurrentRound.Number == 1 || CurrentRound.Number == 16)
			{
				CurrentRound.StartMoneyTeamCt = 4000;
				CurrentRound.StartMoneyTeamT = 4000;
			}
			else
			{
				CurrentRound.StartMoneyTeamCt = Parser.Participants.Where(a => a.Team.ToSide() == Side.CounterTerrorist).Sum(a => a.Money);
				CurrentRound.StartMoneyTeamT = Parser.Participants.Where(a => a.Team.ToSide() == Side.Terrorist).Sum(a => a.Money);
			}

			// save demo's state in case of game pause
			DemoBackup = Demo.Copy();
		}

		protected void CreateNewOvertime()
		{
			CurrentOvertime = new Overtime
			{
				Number = ++OvertimeCount
			};
		}

		/// <summary>
		/// Update the players score (displayed on the scoreboard)
		/// </summary>
		protected void UpdatePlayerScore()
		{
			// Update players score
			foreach (DemoInfo.Player player in Parser.PlayingParticipants)
			{
				Player pl = Demo.Players.FirstOrDefault(p => p.SteamId == player.SteamID);
				if (pl != null)
				{
					pl.Score = player.AdditionaInformations.Score;
				}
			}
		}

		/// <summary>
		/// Swap players team and change teams side
		/// </summary>
		protected void SwapTeams()
		{
			foreach (Player pl in Demo.Players)
			{
				pl.Side = pl.Side == Side.Terrorist ? Side.CounterTerrorist : Side.Terrorist;
			}
			Demo.TeamCT.CurrentSide = Demo.TeamCT.CurrentSide == Side.CounterTerrorist ? Side.Terrorist : Side.CounterTerrorist;
			Demo.TeamT.CurrentSide = Demo.TeamT.CurrentSide == Side.CounterTerrorist ? Side.Terrorist : Side.CounterTerrorist;
			IsSwapTeamRequired = false;
		}

		/// <summary>
		/// Check if there is any clutch situation and if so add clutch to player
		/// </summary>
		protected void ProcessClutches()
		{
			int terroristAliveCount = Parser.PlayingParticipants.Count(p => p.Team.ToSide() == Side.Terrorist && p.IsAlive);
			int counterTerroristAliveCount = Parser.PlayingParticipants.Count(p => p.Team.ToSide() == Side.CounterTerrorist && p.IsAlive);

			// First dectection of a 1vX situation, a terro is in clutch
			if (_playerInClutch1 == null && terroristAliveCount == 1)
			{
				// Set the number of opponent in his clutch
				DemoInfo.Player pl = Parser.PlayingParticipants.FirstOrDefault(p => p.Team.ToSide() == Side.Terrorist && p.IsAlive);
				if (pl == null) return;
				_playerInClutch1 = Demo.Players.FirstOrDefault(p => p.SteamId == pl.SteamID && p.IsAlive);
				if (_playerInClutch1 != null)
				{
					Application.Current.Dispatcher.Invoke(delegate
					{
						_playerInClutch1.Clutches.Add(new ClutchEvent(Parser.IngameTick, Parser.CurrentTime)
						{
							RoundNumber = CurrentRound.Number,
							OpponentCount = counterTerroristAliveCount
						});
					});
					return;
				}
			}

			// First dectection of a 1vX situation, a CT is in clutch
			if (_playerInClutch1 == null && counterTerroristAliveCount == 1)
			{
				DemoInfo.Player pl = Parser.PlayingParticipants.FirstOrDefault(p => p.Team.ToSide() == Side.CounterTerrorist && p.IsAlive);
				if (pl == null) return;
				_playerInClutch1 = Demo.Players.FirstOrDefault(p => p.SteamId == pl.SteamID && p.IsAlive);
				if (_playerInClutch1 != null)
				{
					Application.Current.Dispatcher.Invoke(delegate
					{
						_playerInClutch1.Clutches.Add(new ClutchEvent(Parser.IngameTick, Parser.CurrentTime)
						{
							RoundNumber = CurrentRound.Number,
							OpponentCount = terroristAliveCount
						});
					});
					return;
				}
			}

			// 1v1 detection
			if (counterTerroristAliveCount == 1 && terroristAliveCount == 1 && _playerInClutch1 != null)
			{
				DemoInfo.Player player1 = Parser.PlayingParticipants.FirstOrDefault(p => p.SteamID == _playerInClutch1.SteamId);
				if (player1 == null) return;
				Side player2Team = player1.Team.ToSide() == Side.CounterTerrorist ? Side.Terrorist : Side.CounterTerrorist;
				DemoInfo.Player player2 = Parser.PlayingParticipants.FirstOrDefault(p => p.Team.ToSide() == player2Team);
				if (player2 == null) return;
				_playerInClutch2 = Demo.Players.FirstOrDefault(p => p.SteamId == player2.SteamID && p.IsAlive);
				if (_playerInClutch2 == null) return;
				Application.Current.Dispatcher.Invoke(delegate
				{
					_playerInClutch2.Clutches.Add(new ClutchEvent(Parser.IngameTick, Parser.CurrentTime)
					{
						RoundNumber = CurrentRound.Number,
						OpponentCount = 1
					});
				});
			}
		}

		/// <summary>
		/// Calculate HLTV rating for each players
		/// </summary>
		protected void ProcessPlayersRating()
		{
			foreach (Player player in Demo.Players)
			{
				player.RatingHltv = (float)ComputeHltvOrgRating(Demo.Rounds.Count, player.KillCount, player.DeathCount, new int[5]
				{
					player.OneKillCount, player.TwoKillCount, player.ThreeKillCount, player.FourKillCount, player.FiveKillCount
				});

				player.RatingHltv2 = (float)ComputeHltv2OrgRating(player, Demo);

			}
		}


		/// <summary>
		/// Compute HLTV.org's player rating from player data
		/// </summary>
		/// <param name="roundCount">total number of rounds for the game</param>
		/// <param name="kills">number of kills of a player during the whole game</param>
		/// <param name="deaths">number of deaths of a player during the whole game</param>
		/// <param name="nKills">an array of int containing the total number of x kills in a round during a game for a player. nKills[0] = number of 1 kill, nKills[1] = number of 2 kills, ...</param>
		/// <returns></returns>
		private static double ComputeHltvOrgRating(int roundCount, int kills, int deaths, int[] nKills)
		{
			// Kills/Rounds/AverageKPR
			double killRating = kills / (double)roundCount / AVERAGE_KPR;
			// (Rounds-Deaths)/Rounds/AverageSPR
			double survivalRating = (roundCount - deaths) / (double)roundCount / AVERAGE_SPR;
			// (1K + 4*2K + 9*3K + 16*4K + 25*5K)/Rounds/AverageRMK
			double roundsWithMultipleKillsRating = (nKills[0] + 4 * nKills[1] + 9 * nKills[2] + 16 * nKills[3] + 25 * nKills[4]) / (double)roundCount / AVERAGE_RMK;

			return Math.Round((killRating + 0.7 * survivalRating + roundsWithMultipleKillsRating) / 2.7, 3);
		}

		/// <summary>
		/// Compute HLTV.org's player rating from player data 2.0
		/// </summary>
		/// <param name="player">Player object to be calculated</param>
		/// <param name="demo">Demo object to be calculated</param>
		/// <returns></returns>
		private static double ComputeHltv2OrgRating(Player player, Demo demo)
		{
			//KAST
			double KAST = 0;
			int KASTrounds = 0;
			for (int round = 0; round < demo.Rounds.Count; round++)
            {
				if ((player.Kills.Any(k => k.RoundNumber == round)) || (player.Assists.Any(a => a.RoundNumber == round)) || (player.Deaths.Any(d => d.RoundNumber == round && d.IsTradeKill)) || (!player.Deaths.Any(d => d.RoundNumber == round)))
                {
					KASTrounds++;
				}
            }
			KAST = HLTV2_KAST_MOD * (((double)KASTrounds / (double)demo.Rounds.Count)*100.0F);

			//KPR
			double KPR = HLTV2_KPR_MOD * player.KillPerRound;

			//DPR
			double DPR = HLTV2_DPR_MOD * player.DeathPerRound;

			//ADR
			double ADR = HLTV2_ADR_MOD * player.AverageHealthDamage;

			//Impact
			double Impact = HLTV2_IMPACT_MOD * ((HLTV2_IMPACT_KPR_MOD * player.KillPerRound) + (HLTV2_IMPACT_APR_MOD * player.AssistPerRound) + HLTV2_IMPACT_OFFSET_MOD);

			//HLTV2
			double HLTV2 = KAST + KPR + DPR + Impact + ADR + HLTV2_OFFSET_MOD;

			return Math.Round(HLTV2, 3);
		}

		protected void ComputeEseaRws()
		{
			int totalDamageWinner = 0;
			Dictionary<long, int> playerDamages = new Dictionary<long, int>();
			foreach (PlayerHurtedEvent e in CurrentRound.PlayersHurted.Where(
				e => e.AttackerSide == CurrentRound.WinnerSide))
			{
				if (!playerDamages.ContainsKey(e.AttackerSteamId))
				{
					playerDamages.Add(e.AttackerSteamId, e.HealthDamage);
				}
				else
				{
					playerDamages[e.AttackerSteamId] += e.HealthDamage;
				}
				totalDamageWinner += e.HealthDamage;
			}

			foreach (Player player in Demo.Players)
			{
				if (player.TeamName == CurrentRound.WinnerName && player.IsConnected)
				{
					switch (CurrentRound.EndReason)
					{
						case RoundEndReason.CTWin:
						case RoundEndReason.TerroristWin:
							if (totalDamageWinner > 0 && playerDamages.ContainsKey(player.SteamId))
								player.EseaRwsPointCount += (decimal)playerDamages[player.SteamId] / totalDamageWinner * 100;
							break;
						case RoundEndReason.BombDefused:
						case RoundEndReason.TargetBombed:
							if (CurrentRound.BombDefused != null && CurrentRound.BombDefused.DefuserSteamId == player.SteamId)
								player.EseaRwsPointCount += 30;
							if (CurrentRound.BombPlanted != null && CurrentRound.BombPlanted.PlanterSteamId == player.SteamId)
								player.EseaRwsPointCount += 30;
							if (totalDamageWinner > 0 && playerDamages.ContainsKey(player.SteamId))
								player.EseaRwsPointCount += (decimal)playerDamages[player.SteamId] / totalDamageWinner * 70;
							break;
					}
				}
				if (player.RoundPlayedCount > 0)
					player.EseaRws = Math.Round(player.EseaRwsPointCount / player.RoundPlayedCount, 2);
			}
		}

		protected void ProcessTradeKill(KillEvent killEvent)
		{
			KillEvent lastKilledEvent = CurrentRound.Kills.LastOrDefault(k => k.KillerSteamId == killEvent.KilledSteamId);
			if (lastKilledEvent != null && Parser.CurrentTime - lastKilledEvent.Seconds <= 4)
			{
				killEvent.IsTradeKill = true;
				Player killer = Demo.Players.FirstOrDefault(p => p.SteamId == killEvent.KillerSteamId);
				if (killer != null) killer.TradeKillCount++;
				Player killed = Demo.Players.FirstOrDefault(p => p.SteamId == killEvent.KilledSteamId);
				if (killed != null) killed.TradeDeathCount++;
			}
		}

		protected void ProcessOpenAndEntryKills(KillEvent killEvent)
		{
			if (killEvent.KillerSteamId == 0 || IsFirstKillOccured) return;

			Player killed = Demo.Players.FirstOrDefault(p => Equals(p.SteamId, killEvent.KilledSteamId));
			if (killed == null) return;
			Player killer = Demo.Players.FirstOrDefault(p => Equals(p.SteamId, killEvent.KillerSteamId));
			if (killer == null) return;

			Application.Current.Dispatcher.Invoke(delegate
			{
				if (killEvent.KillerSide == Side.Terrorist)
				{
					// This is an entry kill
					killer.HasEntryKill = true;
					killed.HasEntryKill = true;
					EntryKillEvent entryKillEvent = new EntryKillEvent(Parser.IngameTick, Parser.CurrentTime)
					{
						RoundNumber = CurrentRound.Number,
						KilledSteamId = killed.SteamId,
						KilledName = killed.Name,
						KilledSide = killEvent.KilledSide,
						KillerSteamId = killer.SteamId,
						KillerName = killer.Name,
						KillerSide = killEvent.KillerSide,
						Weapon = killEvent.Weapon
					};
					CurrentRound.EntryKillEvent = entryKillEvent;
					EntryKillEvent killerEvent = CurrentRound.EntryKillEvent.Clone();
					killerEvent.HasWon = true;
					killer.EntryKills.Add(killerEvent);
					EntryKillEvent killedEvent = CurrentRound.EntryKillEvent.Clone();
					killedEvent.HasWon = false;
					killed.EntryKills.Add(killedEvent);
				}
				else
				{
					// CT done the kill , it's an entry hold kill
					killer.HasEntryHoldKill = true;
					killed.HasEntryHoldKill = true;
					EntryHoldKillEvent entryHoldKillEvent = new EntryHoldKillEvent(Parser.IngameTick, Parser.CurrentTime)
					{
						RoundNumber = CurrentRound.Number,
						KilledSteamId = killed.SteamId,
						KilledName = killed.Name,
						KilledSide = killEvent.KilledSide,
						KillerSteamId = killer.SteamId,
						KillerName = killer.Name,
						KillerSide = killEvent.KillerSide,
						Weapon = killEvent.Weapon
					};
					CurrentRound.EntryHoldKillEvent = entryHoldKillEvent;
					EntryHoldKillEvent killerEvent = CurrentRound.EntryHoldKillEvent.Clone();
					killerEvent.HasWon = true;
					killer.EntryHoldKills.Add(killerEvent);
					EntryHoldKillEvent killedEvent = CurrentRound.EntryHoldKillEvent.Clone();
					killedEvent.HasWon = false;
					killed.EntryHoldKills.Add(killedEvent);
				}
			});

			IsFirstKillOccured = true;
		}

		/// <summary>
		/// Update the teams score and current round teams data
		/// </summary>
		/// <param name="e"></param>
		protected void UpdateTeamScore(RoundEndedEventArgs e)
		{
			Side winnerSide = e.Winner.ToSide();
			Side looserSide = winnerSide == Side.Terrorist ? Side.CounterTerrorist : Side.Terrorist;

			Team winner = GetTeamBySide(winnerSide);
			Team looser = GetTeamBySide(looserSide);

			winner.Score++;
			winner.LossRowCount = 0;
			looser.LossRowCount++;
			CurrentRound.WinnerName = winner.Name;
			CurrentRound.WinnerSide = e.Winner.ToSide();

			if (!IsOvertime)
			{
				if (IsHalfMatch) winner.ScoreSecondHalf++;
				else winner.ScoreFirstHalf++;
			}
			else
			{
				if (winner.Name == Demo.TeamCT.Name)
					CurrentOvertime.ScoreTeamCt++;
				else
					CurrentOvertime.ScoreTeamT++;
			}

			Demo.RaiseScoresChanged();
		}

		/// <summary>
		/// A clutch can be lost / won by bomb exploded / defused
		/// It checks if one of the players currently in a clutch won
		/// </summary>
		protected void CheckForSpecialClutchEnd()
		{
			if (_playerInClutch1 == null) return;

			DemoInfo.Player pl = Parser.PlayingParticipants.FirstOrDefault(p => p.SteamID == _playerInClutch1.SteamId);
			if (pl == null) return;

			// 1vX
			if (_playerInClutch2 == null)
			{
				if (pl.Team.ToSide() == Side.Terrorist && CurrentRound.WinnerSide == Side.Terrorist
					|| pl.Team.ToSide() == Side.CounterTerrorist && CurrentRound.WinnerSide == Side.CounterTerrorist)
				{
					_playerInClutch1.Clutches.Last().HasWon = true;
				}
			} else if (_playerInClutch2 != null)
			{
				// 1V1
				switch (CurrentRound.WinnerSide)
				{
					case Side.CounterTerrorist:
						if (pl.Team.ToSide() == Side.CounterTerrorist)
						{
							// CT won
							_playerInClutch1.Clutches.Last().HasWon = true;
						}
						else
						{
							// T won
							_playerInClutch2.Clutches.Last().HasWon = true;
						}
						break;
					case Side.Terrorist:
						if (pl.Team.ToSide() == Side.Terrorist)
						{
							// T won
							_playerInClutch1.Clutches.Last().HasWon = true;
						}
						else
						{
							// CT won
							_playerInClutch2.Clutches.Last().HasWon = true;
						}
						break;
				}
			}
		}

		protected void ProcessRoundEndReward(RoundEndedEventArgs e)
		{
			Team looser = CurrentRound.WinnerName == Demo.TeamCT.Name ? Demo.TeamT : Demo.TeamCT;
			foreach (Player player in Demo.Players)
			{
				switch (e.Reason)
				{
					case RoundEndReason.BombDefused:
						if (player.Side == Side.CounterTerrorist)
						{
							if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
								player.RoundsMoneyEarned[CurrentRound.Number] = WIN_BOMB_DEFUSED;
							else
								player.RoundsMoneyEarned[CurrentRound.Number] += WIN_BOMB_DEFUSED;
						}
						else
						{
							switch (looser.LossRowCount)
							{
								case 1:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_1 + BOMB_PLANTED_BONUS;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_1 + BOMB_PLANTED_BONUS;
									break;
								case 2:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_2 + BOMB_PLANTED_BONUS;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_2 + BOMB_PLANTED_BONUS;
									break;
								case 3:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_3 + BOMB_PLANTED_BONUS;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_3 + BOMB_PLANTED_BONUS;
									break;
								case 4:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_4 + BOMB_PLANTED_BONUS;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_4 + BOMB_PLANTED_BONUS;
									break;
								default:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_5 + BOMB_PLANTED_BONUS;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_5 + BOMB_PLANTED_BONUS;
									break;
							}
						}
						break;
					case RoundEndReason.TargetBombed:
						if (player.Side == Side.Terrorist)
						{
							if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
								player.RoundsMoneyEarned[CurrentRound.Number] = WIN_BOMB_TARGET;
							else
								player.RoundsMoneyEarned[CurrentRound.Number] += WIN_BOMB_TARGET;
						}
						else
							UpdateLooserMoneyReward(looser, player);
						break;
					case RoundEndReason.CTWin:
						if (player.Side == Side.CounterTerrorist)
						{
							if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
								player.RoundsMoneyEarned[CurrentRound.Number] = WIN_ELIMINATION;
							else
								player.RoundsMoneyEarned[CurrentRound.Number] += WIN_ELIMINATION;
						}
						else
						{
							switch (looser.LossRowCount)
							{
								case 1:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = CurrentRound.BombPlanted != null
											? LOSS_ROW_1 + BOMB_PLANTED_BONUS
											: LOSS_ROW_1;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += CurrentRound.BombPlanted != null
											? LOSS_ROW_1 + BOMB_PLANTED_BONUS
											: LOSS_ROW_1;
									break;
								case 2:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = CurrentRound.BombPlanted != null
											? LOSS_ROW_2 + BOMB_PLANTED_BONUS
											: LOSS_ROW_2;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += CurrentRound.BombPlanted != null
											? LOSS_ROW_2 + BOMB_PLANTED_BONUS
											: LOSS_ROW_2;
									break;
								case 3:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = CurrentRound.BombPlanted != null
											? LOSS_ROW_3 + BOMB_PLANTED_BONUS
											: LOSS_ROW_3;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += CurrentRound.BombPlanted != null
											? LOSS_ROW_3 + BOMB_PLANTED_BONUS
											: LOSS_ROW_3;
									break;
								case 4:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = CurrentRound.BombPlanted != null
											? LOSS_ROW_4 + BOMB_PLANTED_BONUS
											: LOSS_ROW_4;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += CurrentRound.BombPlanted != null
											? LOSS_ROW_4 + BOMB_PLANTED_BONUS
											: LOSS_ROW_4;
									break;
								default:
									if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
										player.RoundsMoneyEarned[CurrentRound.Number] = CurrentRound.BombPlanted != null
											? LOSS_ROW_5 + BOMB_PLANTED_BONUS
											: LOSS_ROW_5;
									else
										player.RoundsMoneyEarned[CurrentRound.Number] += CurrentRound.BombPlanted != null
											? LOSS_ROW_5 + BOMB_PLANTED_BONUS
											: LOSS_ROW_5;
									break;
							}
						}
						break;
					case RoundEndReason.TerroristWin:
						if (player.Side == Side.Terrorist)
						{
							if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
								player.RoundsMoneyEarned[CurrentRound.Number] = WIN_ELIMINATION;
							else
								player.RoundsMoneyEarned[CurrentRound.Number] += WIN_ELIMINATION;
						}
						else
							UpdateLooserMoneyReward(looser, player);
						break;
					case RoundEndReason.TargetSaved:
						if (player.Side == Side.CounterTerrorist)
						{
							if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
								player.RoundsMoneyEarned[CurrentRound.Number] = WIN_TIME_OVER;
							else
								player.RoundsMoneyEarned[CurrentRound.Number] += WIN_TIME_OVER;
						}
						else
							if (!player.IsAlive) UpdateLooserMoneyReward(looser, player);
						break;
				}
			}
		}

		private void UpdateLooserMoneyReward(Team looser, Player player)
		{
			switch (looser.LossRowCount)
			{
				case 1:
					if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
						player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_1;
					else
						player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_1;
					break;
				case 2:
					if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
						player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_2;
					else
						player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_2;
					break;
				case 3:
					if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
						player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_3;
					else
						player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_3;
					break;
				case 4:
					if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
						player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_4;
					else
						player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_4;
					break;
				default:
					if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
						player.RoundsMoneyEarned[CurrentRound.Number] = LOSS_ROW_5;
					else
						player.RoundsMoneyEarned[CurrentRound.Number] += LOSS_ROW_5;
					break;
			}
		}

		protected void ProcessAnalyzeEnded()
		{
			// As round_officialy_ended isn't raised we add the last round / OT after the analyze
			if (Demo.Rounds.Count < Demo.ScoreTeamCt + Demo.ScoreTeamT)
			{
				UpdateKillsCount();
				Application.Current.Dispatcher.Invoke(() => Demo.Rounds.Add(CurrentRound));
			}

			// Add last overtime if there was an overtime at the end
			if (IsOvertime) Application.Current.Dispatcher.Invoke(() => Demo.Overtimes.Add(CurrentOvertime));

			// Scores are reset on ESEA demos when the match is over
			if (Parser.TScore != Parser.CTScore || Parser.CTScore == 0 && Parser.TScore == 0)
				Demo.Winner = Demo.ScoreTeamCt > Demo.ScoreTeamT ? Demo.TeamCT : Demo.TeamT;

			if (Demo.Players.Any())
			{
				UpdatePlayerScore();
				ProcessPlayersRating();
				CheckForSpecialClutchEnd();
				Demo.MostHeadshotPlayer = Demo.Players.OrderByDescending(x => x.HeadshotPercent).First();
				Demo.MostBombPlantedPlayer = Demo.Players.OrderByDescending(x => x.BombPlantedCount).First();
				Demo.MostEntryKillPlayer = Demo.Players.OrderByDescending(x => x.EntryKills.Count).First();
			}
			if (Demo.Kills.Any())
			{
				var weapons = Demo.Kills.GroupBy(k => k.Weapon).Select(weap => new
				{
					Weapon = weap.Key,
					Count = weap.Count()
				}).OrderByDescending(w => w.Count);
				if (weapons.Any()) Demo.MostKillingWeapon = weapons.Select(w => w.Weapon).First();
			}

			ClearQueues();
			DemoBackup = null;
		}

		private static string CleanUpChatText(string message)
		{
			return Regex.Replace(message, @"[\u0001\u0002\u0003\u0004\u0005\u0006\u0007]", string.Empty);
		}

		private void ClearQueues()
		{
			LastPlayersFireStartedMolotov.Clear();
			LastPlayersFireEndedMolotov.Clear();
			LastPlayersThrownMolotov.Clear();
			LastNadeTypeThrown.Clear();
			PlayersFlashQueue.Clear();
		}

		protected Team GetTeamBySide(Side side)
		{
			return Demo.TeamCT.CurrentSide == side ? Demo.TeamCT : Demo.TeamT;
		}

		/// <summary>
		/// Restore demo's data to the last round
		/// </summary>
		protected void BackupToLastRound()
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				CurrentRound.Reset(Parser.IngameTick);
				Demo.BackupFromDemo(DemoBackup);
			});
			IsFreezetime = true;
			IsRoundEndOccured = false;
			IsFirstShotOccured = false;
			KillsThisRound.Clear();
			IsFirstKillOccured = false;
			IsSwapTeamRequired = false;
		}

		#endregion
	}
}
