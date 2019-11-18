using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace Core.Models
{
	public class Team : ObservableObject
	{
		#region Properties

		private string _name;

		private int _score;

		private int _scoreFirstHalf;

		private int _scoreSecondHalf;

		#endregion

		#region Accessors

		[JsonProperty("team_name")]
		public string Name
		{
			get { return _name; }
			set { Set(ref _name, value); }
		}

		[JsonProperty("score")]
		public int Score
		{
			get { return _score; }
			set { Set(ref _score, value); }
		}

		[JsonProperty("score_first_half")]
		public int ScoreFirstHalf
		{
			get { return _scoreFirstHalf; }
			set { Set(ref _scoreFirstHalf, value); }
		}

		[JsonProperty("score_second_half")]
		public int ScoreSecondHalf
		{
			get { return _scoreSecondHalf; }
			set { Set(ref _scoreSecondHalf, value); }
		}

		[JsonProperty("team_players", IsReference = false)]
		public ObservableCollection<Player> Players { get; set; }

		[JsonIgnore]
		public Side CurrentSide { get; set; }

		/// <summary>
		/// Keep track of loss serie to determine money reward at each round
		/// </summary>
		[JsonIgnore]
		public int LossRowCount { get; set; }

		[JsonIgnore]
		public int EntryHoldKillCount => Players.SelectMany(p => p.EntryHoldKills).Count();

		[JsonIgnore]
		public int EntryKillCount => Players.SelectMany(p => p.EntryKills).Count();

		[JsonIgnore]
		public int EntryHoldKillWonCount => Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon);

		[JsonIgnore]
		public int EntryHoldKillLossCount => Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon == false);

		[JsonIgnore]
		public decimal RatioEntryHoldKill
		{
			get
			{
				int total = Players.SelectMany(p => p.EntryHoldKills).Count();
				int won = Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon);
				int loss = Players.SelectMany(p => p.EntryHoldKills).Count(e => e.HasWon == false);

				decimal percent = 0;
				if (EntryKillWonCount == 0) return percent;
				if (loss == 0) return 100;
				percent = won / (decimal)total * 100;
				percent = Math.Round(percent, 0);

				return percent;
			}
		}

		[JsonIgnore]
		public int EntryKillWonCount => Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon);

		[JsonIgnore]
		public int EntryKillLossCount => Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon == false);

		[JsonIgnore]
		public int FlashbangThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
			.Sum(playerExtended => playerExtended.FlashbangThrownCount);

		[JsonIgnore]
		public int HeGrenadeThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
			.Sum(playerExtended => playerExtended.HeGrenadeThrownCount);

		[JsonIgnore]
		public int SmokeThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
			.Sum(playerExtended => playerExtended.SmokeThrownCount);

		[JsonIgnore]
		public int MolotovThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
			.Sum(playerExtended => playerExtended.MolotovThrownCount);

		[JsonIgnore]
		public int IncendiaryThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
			.Sum(playerExtended => playerExtended.IncendiaryThrownCount);

		[JsonIgnore]
		public int DecoyThrownCount => Players.Where(playerExtended => playerExtended.TeamName == Name)
			.Sum(playerExtended => playerExtended.DecoyThrownCount);

		[JsonIgnore]
		public int TradeKillCount => Players.Sum(p => p.TradeKillCount);

		[JsonIgnore]
		public int TradeDeathCount => Players.Sum(p => p.TradeDeathCount);

		[JsonIgnore]
		public decimal RatioEntryKill
		{
			get
			{
				int entryKillCount = Players.SelectMany(p => p.EntryKills).Count();
				int entryKillWin = Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon);
				int entryKillLoss = Players.SelectMany(p => p.EntryKills).Count(e => e.HasWon == false);

				decimal entryKillPercent = 0;
				if (EntryKillWonCount == 0) return entryKillPercent;
				if (entryKillLoss == 0) return 100;
				entryKillPercent = (entryKillWin / (decimal)entryKillCount) * 100;
				entryKillPercent = Math.Round(entryKillPercent, 0);

				return entryKillPercent;
			}
		}

		[JsonIgnore]
		public int MatchCount { get; set; } = 1;

		[JsonIgnore]
		public int WinCount { get; set; } = 0;

		[JsonIgnore]
		public int LostCount { get; set; } = 0;

		[JsonIgnore]
		public int KillCount => Players.Sum(player => player.KillCount);

		[JsonIgnore]
		public int AssistCount => Players.Sum(player => player.AssistCount);

		[JsonIgnore]
		public int DeathCount => Players.Sum(player => player.DeathCount);

		[JsonIgnore]
		public int RoundCount { get; set; } = 0;

		[JsonIgnore]
		public int WinRoundCount { get; set; } = 0;

		[JsonIgnore]
		public int LostRoundCount { get; set; } = 0;

		[JsonIgnore]
		public int WinRoundCtCount { get; set; } = 0;

		[JsonIgnore]
		public int LostRoundCtCount { get; set; } = 0;

		[JsonIgnore]
		public int WinRoundTCount { get; set; } = 0;

		[JsonIgnore]
		public int LostRoundTCount { get; set; } = 0;

		[JsonIgnore]
		public int WinPistolRoundCount { get; set; } = 0;

		[JsonIgnore]
		public int WinEcoRoundCount { get; set; } = 0;

		[JsonIgnore]
		public int WinSemiEcoRoundCount { get; set; } = 0;

		[JsonIgnore]
		public int WinForceBuyRoundCount { get; set; } = 0;

		[JsonIgnore]
		public int BombPlantedCount => Players.Sum(player => player.BombPlantedCount);

		[JsonIgnore]
		public int BombDefusedCount => Players.Sum(player => player.BombDefusedCount);

		[JsonIgnore]
		public int BombExplodedCount => Players.Sum(player => player.BombExplodedCount);

		[JsonIgnore]
		public int BombPlantedOnACount { get; set; } = 0;

		[JsonIgnore]
		public int BombPlantedOnBCount { get; set; } = 0;

		[JsonIgnore]
		public int FiveKillCount => Players.Sum(player => player.FiveKillCount);

		[JsonIgnore]
		public int FourKillCount => Players.Sum(player => player.FourKillCount);

		[JsonIgnore]
		public int ThreeKillCount => Players.Sum(player => player.ThreeKillCount);

		[JsonIgnore]
		public int TwoKillCount => Players.Sum(player => player.TwoKillCount);

		[JsonIgnore]
		public int OneKillCount => Players.Sum(player => player.OneKillCount);

		[JsonIgnore]
		public int JumpKillCount => Players.Sum(player => player.JumpKillCount);

		[JsonIgnore]
		public int CrouchKillCount => Players.Sum(player => player.CrouchKillCount);

		#endregion

		public override bool Equals(object obj)
		{
			var item = obj as Team;

			return item != null
				&& string.Compare(Name, item.Name, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public Team()
		{
			Players = new ObservableCollection<Player>();
			Players.CollectionChanged += OnPlayersCollectionChanged;
		}

		public void Clear()
		{
			Players.Clear();
			LossRowCount = 0;
		}

		public void ResetStats()
		{
			Score = 0;
			ScoreFirstHalf = 0;
			ScoreSecondHalf = 0;
			BombPlantedOnACount = 0;
			BombPlantedOnBCount = 0;
			RoundCount = 0;
			LossRowCount = 0;
			LostCount = 0;
			LostRoundCount = 0;
			LostRoundCtCount = 0;
			LostRoundTCount = 0;
			MatchCount = 0;
			WinCount = 0;
			WinEcoRoundCount = 0;
			WinForceBuyRoundCount = 0;
			WinPistolRoundCount = 0;
			WinRoundCount = 0;
			WinRoundCtCount = 0;
			WinRoundTCount = 0;
			WinSemiEcoRoundCount = 0;
		}

		public Team Clone()
		{
			Team team = new Team
			{
				Name = Name,
				BombPlantedOnACount = BombPlantedOnACount,
				BombPlantedOnBCount = BombPlantedOnBCount,
				CurrentSide = CurrentSide,
				LossRowCount = LossRowCount,
				LostCount = LostCount,
				LostRoundCount = LostRoundCount,
				LostRoundCtCount = LostRoundCtCount,
				LostRoundTCount = LostRoundTCount,
				MatchCount = MatchCount,
				RoundCount = RoundCount,
				Score = Score,
				ScoreSecondHalf = ScoreSecondHalf,
				ScoreFirstHalf = ScoreFirstHalf,
				WinCount = WinCount,
				WinEcoRoundCount = WinEcoRoundCount,
				WinForceBuyRoundCount = WinForceBuyRoundCount,
				WinPistolRoundCount = WinPistolRoundCount,
				WinRoundCount = WinRoundCount,
				WinRoundCtCount = WinRoundCtCount,
				WinRoundTCount = WinRoundTCount,
				WinSemiEcoRoundCount = WinSemiEcoRoundCount,
				Players = new ObservableCollection<Player>(),
			};

			foreach (Player player in Players)
				team.Players.Add(player.Clone());

			return team;
		}

		public void BackupFromTeam(Team team)
		{
			Score = team.Score;
			ScoreFirstHalf = team.ScoreFirstHalf;
			ScoreSecondHalf = team.ScoreSecondHalf;
			BombPlantedOnACount = team.BombPlantedOnACount;
			BombPlantedOnBCount = team.BombPlantedOnBCount;
			CurrentSide = team.CurrentSide;
			RoundCount = team.RoundCount;
			LossRowCount = team.LossRowCount;
			LostCount = team.LostCount;
			LostRoundCount = team.LostRoundCount;
			LostRoundCtCount = team.LostRoundCtCount;
			LostRoundTCount = team.LostRoundTCount;
			MatchCount = team.MatchCount;
			WinCount = team.WinCount;
			WinEcoRoundCount = team.WinEcoRoundCount;
			WinForceBuyRoundCount = team.WinForceBuyRoundCount;
			WinPistolRoundCount = team.WinPistolRoundCount;
			WinRoundCount = team.WinRoundCount;
			WinRoundCtCount = team.WinRoundCtCount;
			WinRoundTCount = team.WinRoundTCount;
			WinSemiEcoRoundCount = team.WinSemiEcoRoundCount;
		}

		#region Collections events

		private void OnPlayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Players.Any())
			{
				foreach (var player in Players)
				{
					if (player != null)
					{
						player.Kills.CollectionChanged += OnKillsCollectionChanged;
						player.Deaths.CollectionChanged += OnDeathsCollectionChanged;
						player.Assists.CollectionChanged += OnAssistsCollectionChanged;
						player.EntryKills.CollectionChanged += OnEntryKillsCollectionChanged;
						player.EntryHoldKills.CollectionChanged += OnEntryHoldKillsCollectionChanged;
					}
				}
			}
		}

		private void OnDeathsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(DeathCount));
		}

		private void OnAssistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(AssistCount));
		}

		private void OnKillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(KillCount));
			RaisePropertyChanged(nameof(JumpKillCount));
			RaisePropertyChanged(nameof(CrouchKillCount));
		}

		private void OnEntryKillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(EntryKillCount));
			RaisePropertyChanged(nameof(EntryKillWonCount));
			RaisePropertyChanged(nameof(EntryKillLossCount));
			RaisePropertyChanged(nameof(RatioEntryKill));
		}

		private void OnEntryHoldKillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(EntryHoldKillCount));
			RaisePropertyChanged(nameof(EntryHoldKillWonCount));
			RaisePropertyChanged(nameof(EntryHoldKillLossCount));
			RaisePropertyChanged(nameof(RatioEntryHoldKill));
		}

		#endregion
	}
}
