using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Core.Models;
using DemoInfo;
using Player = Core.Models.Player;

namespace Services.Concrete.Analyzer
{
	public class EseaAnalyzer : DemoAnalyzer
	{
		// Keep track of match_started events occured during each rounds to detect when the match is live
		private readonly Dictionary<int, int> _matchStartedByRound = new Dictionary<int, int>();

		public EseaAnalyzer(Demo demo)
		{
			Parser = new DemoParser(File.OpenRead(demo.Path));
			// Reset to have update on UI
			demo.ResetStats();
			Demo = demo;
			RegisterEvents();
		}

		protected sealed override void RegisterEvents()
		{
			Parser.MatchStarted += HandleMatchStarted;
			Parser.RoundMVP += HandleRoundMvp;
			Parser.PlayerKilled += HandlePlayerKilled;
			Parser.RoundStart += HandleRoundStart;
			Parser.RoundOfficiallyEnd += HandleRoundOfficiallyEnd;
			Parser.FreezetimeEnded += HandleFreezetimeEnded;
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
			Parser.SmokeNadeEnded += HandleSmokeNadeEnded;
			Parser.FireNadeStarted += HandleFireNadeStarted;
			Parser.DecoyNadeStarted += HandleDecoyNadeStarted;
			Parser.DecoyNadeEnded += HandleDecoyNadeEnded;
			Parser.PlayerHurt += HandlePlayerHurted;
			Parser.PlayerDisconnect += HandlePlayerDisconnect;
			Parser.PlayerTeam += HandlePlayerTeam;
			Parser.SayText += HandleSayText;
			Parser.SayText2 += HandleSayText2;
		}

		public override async Task<Demo> AnalyzeDemoAsync(CancellationToken token, Action<string, float> progressCallback = null)
		{
			ProgressCallback = progressCallback;
			Parser.ParseHeader();

			await Task.Run(() => Parser.ParseToEnd(token), token);

			ProcessAnalyzeEnded();

			return Demo;
		}

		#region Handlers

		protected new void HandlePlayerTeam(object sender, PlayerTeamEventArgs e)
		{
			if (e.Swapped == null || e.Swapped.SteamID == 0) return;

			// Keep track of the number team_player events to detect teams swap
			if (e.OldTeam != e.NewTeam)
			{
				PlayerTeamCount++;
				if (PlayerTeamCount > 7)
				{
					PlayerTeamCount = 0;
					IsSwapTeamRequired = true;
					// detect MR overtimes to be able to add OT at the right time
					if (IsOvertime && MrOvertime == 0)
					{
						MrOvertime = Parser.CTScore + Parser.TScore - 30;
						// add first OT rounds to the counter
						RoundCountOvertime = MrOvertime - 1;
					}
				}
			}

			base.HandlePlayerTeam(sender, e);
		}

		protected new void HandleFreezetimeEnded(object sender, FreezetimeEndedEventArgs e)
		{
			IsFreezetime = false;
			base.HandleFreezetimeEnded(sender, e);
		}

		protected override void HandleMatchStarted(object sender, MatchStartedEventArgs e)
		{
			PlayerTeamCount = 0;
			IsMatchStarted = false;

			// increment the match_started counter to detect when the match is live
			if (!_matchStartedByRound.ContainsKey(CurrentRound.Number))
				_matchStartedByRound[CurrentRound.Number] = 1;
			else
				++_matchStartedByRound[CurrentRound.Number];

			bool isMatchStarted = false;
			if (_matchStartedByRound.ContainsKey(CurrentRound.Number - 1))
			{
				isMatchStarted = _matchStartedByRound[CurrentRound.Number] + _matchStartedByRound[CurrentRound.Number - 1] > 3;
			}

			// the match is live after 3 restarts
			if (_matchStartedByRound[CurrentRound.Number] > 2 || isMatchStarted)
			{
				IsMatchStarted = true;
				// https://github.com/akiver/CSGO-Demos-Manager/issues/76
				// some ESEA demos have 1 match_started between round_end event
				// that prevent to create a new round when it should had been created
				if (Demo.Rounds.Count == CurrentRound.Number) CreateNewRound();
			}

			if (CurrentRound.Number == 1) InitPlayers();
		}

		protected override void HandleRoundStart(object sender, RoundStartedEventArgs e)
		{
			if (!IsMatchStarted) return;

			if (IsSwapTeamRequired && CurrentRound.Number == 1) IsSwapTeamRequired = false;

			// Detect teams name only during first half
			if (CurrentRound.Number < 15)
			{
				if (!string.IsNullOrEmpty(Parser.CTClanName)) Demo.TeamCT.Name = Parser.CTClanName;
				if (!string.IsNullOrEmpty(Parser.TClanName)) Demo.TeamT.Name = Parser.TClanName;
			}

			CreateNewRound();
		}

		protected new void HandleRoundOfficiallyEnd(object sender, RoundOfficiallyEndedEventArgs e)
		{
			base.HandleRoundOfficiallyEnd(sender, e);

			if (!IsMatchStarted || IsFreezetime) return;

			int score = Parser.CTScore + Parser.TScore;
			if (score < 15) return;

			// Detect half match
			if (score == 15)
			{
				IsMatchStarted = false;
				IsHalfMatch = true;
			}

			// count rounds played in case of overtime to detect overtime end
			if (IsOvertime)
			{
				++RoundCountOvertime;
				// if the number of rounds played during OT == 2x MR OT detected, the OT is over
				if (MrOvertime * 2 == RoundCountOvertime)
				{
					Application.Current.Dispatcher.Invoke(() => Demo.Overtimes.Add(CurrentOvertime));
					CreateNewOvertime();
					RoundCountOvertime = 0;
				}
			}

			// detect first overtime, the next overtimes are detected
			// by counting rounds and compare the value to MrOvertime
			if (score == 30)
			{
				IsMatchStarted = false;
				IsOvertime = true;

				// Create a new round when the score is 15-15 because round_start isn't raised
				CreateNewRound();

				// Init the first OT
				CreateNewOvertime();
			}
		}

		#endregion

		#region Process

		private void InitPlayers()
		{
			// Add all players to our ObservableCollection of PlayerExtended
			foreach (DemoInfo.Player player in Parser.PlayingParticipants)
			{
				if (player.SteamID != 0)
				{
					Application.Current.Dispatcher.Invoke(delegate
					{
						Player pl = Demo.Players.FirstOrDefault(p => p.SteamId == player.SteamID);
						if (pl == null)
						{
							pl = new Player
							{
								SteamId = player.SteamID,
								Name = player.Name,
								Side = player.Team.ToSide()
							};
                            pl.EnableUpdates();
							Demo.Players.Add(pl);
						}

						if (pl.Side == Side.CounterTerrorist)
						{
							pl.TeamName = Demo.TeamCT.Name;
							// Check swap
							if (Demo.TeamT.Players.Contains(pl))
							{
								Demo.TeamCT.Players.Add(Demo.TeamT.Players.First(p => p.Equals(pl)));
								Demo.TeamT.Players.Remove(pl);
							}
							else
							{
								if (!Demo.TeamCT.Players.Contains(pl)) Demo.TeamCT.Players.Add(pl);
							}
						}

						if (pl.Side == Side.Terrorist)
						{
							pl.TeamName = Demo.TeamT.Name;
							// Check swap
							if (Demo.TeamCT.Players.Contains(pl))
							{
								Demo.TeamT.Players.Add(Demo.TeamCT.Players.First(p => p.Equals(pl)));
								Demo.TeamCT.Players.Remove(pl);
							}
							else
							{
								if (!Demo.TeamT.Players.Contains(pl)) Demo.TeamT.Players.Add(pl);
							}
						}
					});
				}
			}
		}

		#endregion
	}
}
