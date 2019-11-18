using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Core.Models;
using DemoInfo;
using Player = Core.Models.Player;
using Team = Core.Models.Team;

namespace Services.Concrete.Analyzer
{
	public class EbotAnalyzer : DemoAnalyzer
	{
		// Counter of player team swap (used detect end of a half)
		// TODO remove
		private int _playerTeamCount;

		private bool _isMatchStartedOccured = false;

		private bool _isTeamsInitialized = false;

		private bool _isRoundOffiallyEndedOccured = false;

		/// <summary>
		/// Used to avoid additional score update when a match is over (detected by eBot text)
		/// </summary>
		private bool _isMatchEnded = false;

		/// <summary>
		/// Used to make some specific check on Faceit demos and avoid some on eBot demos
		/// </summary>
		private bool _isFaceit = false;

		private readonly Regex _scoreRegex = new Regex("^eBot: (.*) (?<score1>[0-9]+) - (?<score2>[0-9]+) (.*)$");

		private readonly Regex _endMatchRegex = new Regex("^eBot: (.*) win(.*)$|^ \\[FACEIT\\^\\] (.*) won the match(.*)$");

		private readonly Regex _faceItLiveRegex = new Regex("^ (\\[FACEIT\\^\\]|\\[ANNA\\^\\]) LIVE!$");

		private const string EBOT_LIVE = "eBot: LIVE!";

		private const string BEGIN_FIRST_SIDE_OVERTIME = "eBot: 1st Side OT: LIVE!";

		private const string BEGIN_SECOND_SIDE_OVERTIME = "eBot: 2nd Side OT: LIVE!";

		private const string PLEASE_WRITE_READY = "eBot: Please write !ready when your team is ready !";

		private const string STOP_ROUND = "eBot: This round has been cancelled, we will restart at the begin of the round";

		private const string ROUND_RESTORED = "eBot: Round restored, going live !";

		private const string MATCH_UNPAUSED = "eBot: This round has been cancelled, we will restart at the begin of the round";

		public EbotAnalyzer(Demo demo)
		{
			Parser = new DemoParser(File.OpenRead(demo.Path));
			// Reset to have update on UI
			demo.ResetStats();
			Demo = demo;
			RegisterEvents();
			IsMatchStarted = true;
		}

		protected sealed override void RegisterEvents()
		{
			Parser.MatchStarted += HandleMatchStarted;
			Parser.RoundMVP += HandleRoundMvp;
			Parser.PlayerKilled += HandlePlayerKilled;
			Parser.RoundStart += HandleRoundStart;
			Parser.RoundOfficiallyEnd += HandleRoundOfficiallyEnd;
			Parser.BombPlanted += HandleBombPlanted;
			Parser.BombDefused += HandleBombDefused;
			Parser.BombExploded += HandleBombExploded;
			Parser.TickDone += HandleTickDone;
			Parser.WeaponFired += HandleWeaponFired;
			Parser.RoundEnd += HandleRoundEnd;
			Parser.FlashNadeExploded += HandleFlashNadeExploded;
			Parser.ExplosiveNadeExploded += HandleExplosiveNadeExploded;
			Parser.SmokeNadeStarted += HandleSmokeNadeStarted;
			Parser.FireNadeEnded += HandleFireNadeEnded;
			Parser.LastRoundHalf += HandleLastRoundHalf;
			Parser.PlayerTeam += HandlePlayerTeam;
			Parser.WinPanelMatch += HandleWinPanelMatch;
			Parser.SmokeNadeEnded += HandleSmokeNadeEnded;
			Parser.FireNadeStarted += HandleFireNadeStarted;
			Parser.DecoyNadeStarted += HandleDecoyNadeStarted;
			Parser.DecoyNadeEnded += HandleDecoyNadeEnded;
			Parser.PlayerHurt += HandlePlayerHurted;
			Parser.SayText += HandleSayText;
			Parser.SayText2 += HandleSayText2;
			Parser.PlayerDisconnect += HandlePlayerDisconnect;
			Parser.FreezetimeEnded += HandleFreezetimeEnded;
		}

		public override async Task<Demo> AnalyzeDemoAsync(CancellationToken token, Action<string, float> progressCallback = null)
		{
			ProgressCallback = progressCallback;
			Parser.ParseHeader();

			await Task.Run(() => Parser.ParseToEnd(token), token);

			ProcessAnalyzeEnded();

			return Demo;
		}

		protected new void HandleRoundOfficiallyEnd(object sender, RoundOfficiallyEndedEventArgs e)
		{
			base.HandleRoundOfficiallyEnd(sender, e);
			_isRoundOffiallyEndedOccured = true;

			// count rounds played in case of overtime to detect overtime end
			if (IsOvertime)
			{
				++RoundCountOvertime;
				// if the number of rounds played is equals to the double of MR, the OT is over
				if (MrOvertime * 2 == RoundCountOvertime)
				{
					Application.Current.Dispatcher.Invoke(() => Demo.Overtimes.Add(CurrentOvertime));
					CreateNewOvertime();
					RoundCountOvertime = 0;
				}
			}

			// First OT detection
			if (Parser.CTScore == 15 && Parser.TScore == 15)
			{
				IsOvertime = true;
				CreateNewOvertime();
			}
		}

		protected new void HandleWeaponFired(object sender, WeaponFiredEventArgs e)
		{
			if (!_isTeamsInitialized) return;
			base.HandleWeaponFired(sender, e);
		}

		protected new void HandlePlayerTeam(object sender, PlayerTeamEventArgs e)
		{
			if (e.Swapped == null || e.Swapped.SteamID == 0) return;

			// Keep track of the number team_player events to detect teams swap
			if (e.OldTeam != e.NewTeam && e.NewTeam != DemoInfo.Team.Spectate && e.OldTeam != DemoInfo.Team.Spectate)
			{
				PlayerTeamCount++;
				if (PlayerTeamCount > 7)
				{
					PlayerTeamCount = 0;
					IsSwapTeamRequired = true;
					// detect MR overtimes to be able to add OT at the right time
					if (IsOvertime)
					{
						if (MrOvertime == 0) MrOvertime = Demo.ScoreTeamT + Demo.ScoreTeamCt - 30;
						IsHalfMatch = !IsHalfMatch;
					}
				}
			}

			Player player = Demo.Players.FirstOrDefault(p => p.SteamId == e.Swapped.SteamID);
			if (player != null)
			{
				player.IsConnected = true;
				player.Side = e.NewTeam.ToSide();
			}

			if (!IsMatchStarted) return;
			if (e.Silent) _playerTeamCount++;
		}

		protected void HandleLastRoundHalf(object sender, LastRoundHalfEventArgs e)
		{
			if (!IsMatchStarted) return;
			if (_isFaceit && IsOvertime) IsLastRoundHalf = true;
		}

		protected new void HandleSayText(object sender, SayTextEventArgs e)
		{
			base.HandleSayText(sender, e);

			// game pause
			if (e.Text == STOP_ROUND)
			{
				IsMatchStarted = false;
				IsGamePaused = true;
				BackupToLastRound();
				return;
			}

			// live after pause
			if (e.Text == MATCH_UNPAUSED || e.Text == ROUND_RESTORED)
			{
				IsMatchStarted = true;
				IsGamePaused = false;
				return;
			}

			// Beginning of the match
			Match faceItLive = _faceItLiveRegex.Match(e.Text);
			if (e.Text == EBOT_LIVE || faceItLive.Success)
			{
				Demo.ResetStats(false);
				InitMatch();
				CreateNewRound(true);
				IsMatchStarted = true;
			}

			Match scoreUpdateEbot = _scoreRegex.Match(e.Text);
			// Score update
			if (!scoreUpdateEbot.Success)
			{
				_isFaceit = true;
				return;
			}

			// End of the match (OT or not)
			Match matchEnd = _endMatchRegex.Match(e.Text);
			if (matchEnd.Success)
			{
				IsMatchStarted = false;
				_isMatchEnded = true;
				return;
			}

			if (IsOvertime)
			{
				// if eBot is waiting for !ready, the match isn't started
				if (e.Text == PLEASE_WRITE_READY) IsMatchStarted = false;

				// announce the beginning of an overtime
				if (e.Text == BEGIN_FIRST_SIDE_OVERTIME || e.Text == BEGIN_SECOND_SIDE_OVERTIME)
				{
					IsMatchStarted = true;
				}
			}
		}

		protected void HandleWinPanelMatch(object sender, WinPanelMatchEventArgs e)
		{
			if (!_isRoundOffiallyEndedOccured)
			{
				// Update players stats here because some demos doesn't have round_officialy_ended at the last round
				UpdateKillsCount();
				UpdatePlayerScore();
			}

			Application.Current.Dispatcher.Invoke(delegate
			{
				if (!IsOvertime || !_isFaceit) Demo.Rounds.Add(CurrentRound);
			});

			IsMatchStarted = false;
		}

		protected override void HandleMatchStarted(object sender, MatchStartedEventArgs e)
		{
			if (!IsMatchStarted) return;

			if (!_isMatchStartedOccured && !IsOvertime)
			{
				_isMatchStartedOccured = true;
				// Reset demo start recording before
				Demo.ResetStats(false);
				InitMatch();
				CreateNewRound(true);
			}
		}

		protected override void HandleRoundStart(object sender, RoundStartedEventArgs e)
		{
			if (_playerTeamCount > 8 && !_isMatchEnded && !IsGamePaused) IsMatchStarted = true;
			if (!IsMatchStarted) return;
			UpdateTeams();
			_isRoundOffiallyEndedOccured = false;

			// Reset until both scores > 0
			// Sometimes parser scores are reseted at the end of the match, check the actual demo scores too
			if (Parser.CTScore == 0 && Parser.TScore == 0 && Demo.ScoreTeamCt < 16 && Demo.ScoreTeamT < 16)
			{
				Demo.ResetStats(false);
				InitMatch();
			}

			CreateNewRound();

			if (Demo.ScoreTeamCt + Demo.ScoreTeamT == 15) IsHalfMatch = true;

			if(!IsOvertime) return;

			_playerTeamCount = 0;
		}

		protected new void HandleRoundEnd(object sender, RoundEndedEventArgs e)
		{
			// ebot sometimes doesn't have the right teams name at the first round when there was a teams swap
			if (CurrentRound.Number == 1 && CurrentRound.TeamCtName != Demo.TeamCT.Name)
			{
				CurrentRound.TeamCtName = Demo.TeamCT.Name;
				CurrentRound.TeamTname = Demo.TeamT.Name;
			}

			base.HandleRoundEnd(sender, e);

			if (IsLastRoundHalf)
				Application.Current.Dispatcher.Invoke(() => Demo.Rounds.Add(CurrentRound));
		}

		protected new void HandleTickDone(object sender, TickDoneEventArgs e)
		{
			ProgressCallback?.Invoke(Demo.Id, Parser.ParsingProgess);

			if (!_isTeamsInitialized)
			{
				Application.Current.Dispatcher.Invoke(delegate
				{
					if (Demo.Players.Count < 10) InitMatch();
					Demo.Rounds.Clear();
					CreateNewRound();
					// since some demos are already started we can't use the freezetime event
					if (CurrentRound.Number == 1) IsFreezetime = false;
				});
			}

			base.HandleTickDone(sender, e);
		}

		private void InitMatch()
		{
			if (!string.IsNullOrEmpty(Parser.CTClanName)) Demo.TeamCT.Name = Parser.CTClanName;
			if (!string.IsNullOrEmpty(Parser.TClanName)) Demo.TeamT.Name = Parser.TClanName;
			Demo.TeamCT.CurrentSide = Side.CounterTerrorist;
			Demo.TeamT.CurrentSide = Side.Terrorist;
			UpdatePlayers();

			CurrentRound.EquipementValueTeamCt = Parser.Participants.Where(a => a.Team.ToSide() == Side.CounterTerrorist).Sum(a => a.CurrentEquipmentValue);
			CurrentRound.EquipementValueTeamT = Parser.Participants.Where(a => a.Team.ToSide() == Side.Terrorist).Sum(a => a.CurrentEquipmentValue);
			foreach (Player player in Demo.Players)
			{
				if (!player.RoundsMoneyEarned.ContainsKey(CurrentRound.Number))
					player.RoundsMoneyEarned[CurrentRound.Number] = 0;
			}

			if (Demo.Players.Count >= 10) _isTeamsInitialized = true;
		}

		/// <summary>
		/// Update teams data
		/// </summary>
		private void UpdateTeams()
		{
			Team teamCt = GetTeamBySide(Side.CounterTerrorist);
			Team teamT = GetTeamBySide(Side.Terrorist);

			if (!string.IsNullOrEmpty(Parser.TClanName)
				&& !string.IsNullOrEmpty(Parser.TClanName)
				&& teamCt.Name == Parser.TClanName
				&& teamT.Name == Parser.CTClanName)
			{
				SwapTeams();
			}

			UpdatePlayers();
		}

		/// <summary>
		/// Update players and their team relation
		/// </summary>
		protected void UpdatePlayers()
		{
			Team teamCt = GetTeamBySide(Side.CounterTerrorist);
			Team teamT = GetTeamBySide(Side.Terrorist);

			foreach (DemoInfo.Player player in Parser.PlayingParticipants)
			{
				if (player.SteamID != 0)
				{
					Player pl = Demo.Players.FirstOrDefault(p => p.SteamId == player.SteamID);
					if (pl != null)
					{
						pl.Side = player.Team.ToSide();
						if (player.Team == DemoInfo.Team.CounterTerrorist)
						{
							if (teamT.Players.Contains(pl))
							{
								Application.Current.Dispatcher.Invoke(() => teamT.Players.Remove(pl));
							}
							if (!teamCt.Players.Contains(pl))
							{
								Application.Current.Dispatcher.Invoke(() => teamCt.Players.Add(pl));
							}
						}
						else if (player.Team == DemoInfo.Team.Terrorist)
						{
							if (teamCt.Players.Contains(pl))
							{
								Application.Current.Dispatcher.Invoke(() => teamCt.Players.Remove(pl));
							}
							if (!teamT.Players.Contains(pl))
							{
								Application.Current.Dispatcher.Invoke(() => teamT.Players.Add(pl));
							}
						}
					}
					else
					{
						// new player
						pl = new Player
						{
							SteamId = player.SteamID,
							Name = player.Name,
							Side = player.Team.ToSide()
						};
						Application.Current.Dispatcher.Invoke(() => Demo.Players.Add(pl));

						if (player.Team == DemoInfo.Team.CounterTerrorist)
						{
							pl.TeamName = Demo.TeamCT.Name;
							// Check swap
							if (Demo.TeamT.Players.Contains(pl))
							{
								Application.Current.Dispatcher.Invoke(delegate
								{
									Demo.TeamCT.Players.Add(Demo.TeamT.Players.First(p => p.Equals(pl)));
									Demo.TeamT.Players.Remove(pl);
								});
							}
							else
							{
								if (!Demo.TeamCT.Players.Contains(pl))
								{
									Application.Current.Dispatcher.Invoke(() => Demo.TeamCT.Players.Add(pl));
								}
							}
						}
						else if (player.Team == DemoInfo.Team.Terrorist)
						{
							pl.TeamName = Demo.TeamT.Name;
							// Check swap
							if (Demo.TeamCT.Players.Contains(pl))
							{
								Application.Current.Dispatcher.Invoke(delegate
								{
									Demo.TeamT.Players.Add(Demo.TeamCT.Players.First(p => p.Equals(pl)));
									Demo.TeamCT.Players.Remove(pl);
								});
							}
							else
							{
								if (!Demo.TeamT.Players.Contains(pl))
								{
									Application.Current.Dispatcher.Invoke(() => Demo.TeamT.Players.Add(pl));
								}
							}
						}

                        pl.EnableUpdates();
					}
				}
			}
		}
	}
}
