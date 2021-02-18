using System;

namespace Services.Models.Excel
{
	public class PlayerStats
	{
		public int MatchCount { get; set; }

		public int KillCount { get; set; }

		public int AssistCount { get; set; }

		public int DeathCount { get; set; }

		public decimal KillPerDeath => DeathCount == 0 ? KillCount : Math.Round((decimal)KillCount / DeathCount, 2);

		public int HeadshotCount { get; set; }

		public decimal HeadshotPercent => KillCount == 0 ? 0 : Math.Round(HeadshotCount / (decimal)KillCount * 100, 2);

		public int RoundCount { get; set; }

		public float Rating { get; set; }

		public float Rating2 { get; set; }

		public decimal EseaRws { get; set; }

		public int FiveKillCount { get; set; }

		public int FourKillCount { get; set; }

		public int ThreeKillCount { get; set; }

		public int TwoKillCount { get; set; }

		public int OneKillCount { get; set; }

		public int TeamKillCount { get; set; }

		public int JumpKillCount { get; set; }

		public int CrouchKillCount { get; set; }

		public int BombPlantedCount { get; set; }

		public int BombDefusedCount { get; set; }

		public int BombExplodedCount { get; set; }

		public int MvpCount { get; set; }

		public int ScoreCount { get; set; }

		public int ClutchCount { get; set; }

		public int ClutchWonCount { get; set; }

		public int ClutchLostCount { get; set; }

		public decimal ClutchWonPercent => ClutchCount == 0 ? 0 : Math.Round((decimal)(ClutchWonCount * 100) / ClutchCount, 2);

		public int Clutch1V1Count { get; set; }

		public int Clutch1V2Count { get; set; }

		public int Clutch1V3Count { get; set; }

		public int Clutch1V4Count { get; set; }

		public int Clutch1V5Count { get; set; }

		public int Clutch1V1WonCount { get; set; }

		public int Clutch1V2WonCount { get; set; }

		public int Clutch1V3WonCount { get; set; }

		public int Clutch1V4WonCount { get; set; }

		public int Clutch1V5WonCount { get; set; }

		public int Clutch1V1LossCount { get; set; }

		public int Clutch1V2LossCount { get; set; }

		public int Clutch1V3LossCount { get; set; }

		public int Clutch1V4LossCount { get; set; }

		public int Clutch1V5LossCount { get; set; }

		public decimal Clutch1V1WonPercent => Clutch1V1Count == 0 ? 0 : Math.Round((decimal)(Clutch1V1WonCount * 100) / Clutch1V1Count, 2);

		public decimal Clutch1V2WonPercent => Clutch1V2Count == 0 ? 0 : Math.Round((decimal)(Clutch1V2WonCount * 100) / Clutch1V2Count, 2);

		public decimal Clutch1V3WonPercent => Clutch1V3Count == 0 ? 0 : Math.Round((decimal)(Clutch1V3WonCount * 100) / Clutch1V3Count, 2);

		public decimal Clutch1V4WonPercent => Clutch1V4Count == 0 ? 0 : Math.Round((decimal)(Clutch1V4WonCount * 100) / Clutch1V4Count, 2);

		public decimal Clutch1V5WonPercent => Clutch1V5Count == 0 ? 0 : Math.Round((decimal)(Clutch1V5WonCount * 100) / Clutch1V5Count, 2);

		public int EntryKillCount { get; set; }

		public int EntryKillWinCount { get; set; }

		public int EntryKillLossCount { get; set; }

		public decimal EntryKillWinPercent => EntryKillCount == 0 ? 0 : Math.Round((decimal)(EntryKillWinCount * 100) / EntryKillCount, 2);

		public int EntryHoldKillCount { get; set; }

		public int EntryHoldKillWonCount { get; set; }

		public int EntryHoldKillLossCount { get; set; }

		public int TradeKillCount { get; set; }

		public int TradeDeathCount { get; set; }

		public double AverageTimeDeathSeconds { get; set; }

		public decimal EntryHoldKillWinPercent => EntryHoldKillCount == 0 ? 0 : Math.Round((decimal)(EntryHoldKillWonCount * 100) / EntryHoldKillCount, 2);

		public decimal KillPerRound => RoundCount == 0 ? 0 : Math.Round((decimal)KillCount / RoundCount, 2);

		public decimal AssistPerRound => RoundCount == 0 ? 0 : Math.Round((decimal)AssistCount / RoundCount, 2);

		public decimal DeathPerRound => RoundCount == 0 ? 0 : Math.Round((decimal)DeathCount / RoundCount, 2);

		public decimal AverageDamagePerRound => RoundCount == 0 ? 0 : Math.Round((decimal)DamageHealthCount / RoundCount, 2);

		public int DamageHealthCount { get; set; }

		public int DamageArmorCount { get; set; }

		public int FlashbangThrownCount { get; set; }

		public int SmokeThrownCount { get; set; }

		public int HeGrenadeThrownCount { get; set; }

		public int DecoyThrownCount { get; set; }

		public int MolotovThrownCount { get; set; }

		public int IncendiaryThrownCount { get; set; }

		public int RankMax { get; set; }

		public bool IsVacBanned { get; set; }

		public bool IsOverwatchBanned { get; set; }
	}
}