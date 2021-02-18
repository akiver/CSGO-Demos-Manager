using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using NPOI.SS.UserModel;
using Services.Models.Excel;

namespace Services.Concrete.Excel.Sheets.Multiple
{
	public class PlayersSheet : AbstractMultipleSheet
	{
		public PlayersSheet(IWorkbook workbook, List<Demo> demos)
		{
			Headers = new Dictionary<string, CellType>(){
				{ "Name", CellType.String },
				{ "SteamID", CellType.String },
				{ "Team", CellType.String },
				{ "Match", CellType.Numeric },
				{ "Kills", CellType.Numeric },
				{ "Assists", CellType.Numeric },
				{ "Deaths", CellType.Numeric },
				{ "K/D", CellType.Numeric },
				{ "HS", CellType.Numeric },
				{ "HS%", CellType.Numeric },
				{ "Rounds", CellType.Numeric },
				{ "RWS", CellType.Numeric },
				{ "Rating", CellType.Numeric },
				{ "Rating 2", CellType.Numeric },
				{ "ATD (s)", CellType.Numeric },
				{ "5K", CellType.Numeric },
				{ "4K", CellType.Numeric },
				{ "3K", CellType.Numeric },
				{ "2K", CellType.Numeric },
				{ "1K", CellType.Numeric },
				{ "Trade Kill", CellType.Numeric },
				{ "Trade Death", CellType.Numeric },
				{ "Team kill", CellType.Numeric },
				{ "Jump kill", CellType.Numeric },
				{ "Crouch kill", CellType.Numeric },
				{ "Bomb planted", CellType.Numeric },
				{ "Bomb defused", CellType.Numeric },
				{ "Bomb exploded", CellType.Numeric },
				{ "MVP", CellType.Numeric },
				{ "Score", CellType.Numeric },
				{ "Clutch", CellType.Numeric },
				{ "Clutch won", CellType.Numeric },
				{ "Clutch lost", CellType.Numeric },
				{ "Clutch won %", CellType.Numeric },
				{ "1v1", CellType.Numeric },
				{ "1v1 won", CellType.Numeric },
				{ "1v1 loss", CellType.Numeric },
				{ "1v1 won %", CellType.Numeric },
				{ "1v2", CellType.Numeric },
				{ "1v2 won", CellType.Numeric },
				{ "1v2 loss", CellType.Numeric },
				{ "1v2 won %", CellType.Numeric },
				{ "1v3", CellType.Numeric },
				{ "1v3 won", CellType.Numeric },
				{ "1v3 loss", CellType.Numeric },
				{ "1v3 won %", CellType.Numeric },
				{ "1v4", CellType.Numeric },
				{ "1v4 won", CellType.Numeric },
				{ "1v4 loss", CellType.Numeric },
				{ "1v4 won %", CellType.Numeric },
				{ "1v5", CellType.Numeric },
				{ "1v5 won", CellType.Numeric },
				{ "1v5 loss", CellType.Numeric },
				{ "1v5 won %", CellType.Numeric },
				{ "Entry kill", CellType.Numeric },
				{ "Entry kill win", CellType.Numeric },
				{ "Entry kill lost", CellType.Numeric },
				{ "Entry kill win %", CellType.Numeric },
				{ "Entry hold kill", CellType.Numeric },
				{ "Entry hold kill win", CellType.Numeric },
				{ "Entry hold kill lost", CellType.Numeric },
				{ "Entry hold kill win %", CellType.Numeric },
				{ "KPR", CellType.Numeric },
				{ "APR", CellType.Numeric },
				{ "DPR", CellType.Numeric },
				{ "ADR", CellType.Numeric },
				{ "TDH", CellType.Numeric },
				{ "TDA", CellType.Numeric },
				{ "Flashbang thrown", CellType.Numeric },
				{ "Smoke thrown", CellType.Numeric },
				{ "HE thrown", CellType.Numeric },
				{ "Decoy thrown", CellType.Numeric },
				{ "Molotov thrown", CellType.Numeric },
				{ "Incendiary thrown", CellType.Numeric },
				{ "Rank max", CellType.Numeric },
				{ "VAC", CellType.Boolean },
				{ "OW", CellType.Boolean }
			};
			Demos = demos;
			Sheet = workbook.CreateSheet("Players");
		}

		public override async Task GenerateContent()
		{
			await Task.Factory.StartNew(() =>
			{
				Dictionary<Player, PlayerStats> data = new Dictionary<Player, PlayerStats>();

				foreach (Demo demo in Demos)
				{
					foreach (Player player in demo.Players)
					{
						if(!data.ContainsKey(player)) data.Add(player, new PlayerStats());
						data[player].MatchCount++;
						data[player].RankMax = data[player].RankMax < player.RankNumberNew ? player.RankNumberNew : data[player].RankMax;
						data[player].KillCount += player.KillCount;
						data[player].AssistCount += player.AssistCount;
						data[player].DeathCount += player.DeathCount;
						data[player].FiveKillCount += player.FiveKillCount;
						data[player].FourKillCount += player.FourKillCount;
						data[player].ThreeKillCount += player.ThreeKillCount;
						data[player].TwoKillCount += player.TwoKillCount;
						data[player].OneKillCount += player.OneKillCount;
						data[player].HeadshotCount += player.HeadshotCount;
						data[player].TeamKillCount += player.TeamKillCount;
						data[player].JumpKillCount += player.JumpKillCount;
						data[player].CrouchKillCount += player.CrouchKillCount;
						data[player].ClutchCount += player.ClutchCount;
						data[player].ClutchWonCount += player.ClutchWonCount;
						data[player].ClutchLostCount += player.ClutchLostCount;
						data[player].Clutch1V1Count += player.Clutch1V1Count;
						data[player].Clutch1V2Count += player.Clutch1V2Count;
						data[player].Clutch1V3Count += player.Clutch1V3Count;
						data[player].Clutch1V4Count += player.Clutch1V4Count;
						data[player].Clutch1V5Count += player.Clutch1V5Count;
						data[player].Clutch1V1WonCount += player.Clutch1V1WonCount;
						data[player].Clutch1V2WonCount += player.Clutch1V2WonCount;
						data[player].Clutch1V3WonCount += player.Clutch1V3WonCount;
						data[player].Clutch1V4WonCount += player.Clutch1V4WonCount;
						data[player].Clutch1V5WonCount += player.Clutch1V5WonCount;
						data[player].Clutch1V1LossCount += player.Clutch1V1LossCount;
						data[player].Clutch1V2LossCount += player.Clutch1V2LossCount;
						data[player].Clutch1V3LossCount += player.Clutch1V3LossCount;
						data[player].Clutch1V4LossCount += player.Clutch1V4LossCount;
						data[player].Clutch1V5LossCount += player.Clutch1V5LossCount;
						data[player].BombPlantedCount += player.BombPlantedCount;
						data[player].BombDefusedCount += player.BombDefusedCount;
						data[player].BombExplodedCount += player.BombExplodedCount;
						data[player].MvpCount += player.RoundMvpCount;
						data[player].ScoreCount += player.Score;
						data[player].Rating += player.RatingHltv;
						data[player].Rating2 += player.RatingHltv2;
						data[player].EseaRws += player.EseaRws;
						data[player].RoundCount += player.RoundPlayedCount;
						data[player].IsVacBanned = data[player].IsVacBanned || player.IsVacBanned;
						data[player].IsOverwatchBanned = data[player].IsOverwatchBanned || player.IsOverwatchBanned;
						data[player].FlashbangThrownCount += player.FlashbangThrownCount;
						data[player].SmokeThrownCount += player.SmokeThrownCount;
						data[player].HeGrenadeThrownCount += player.HeGrenadeThrownCount;
						data[player].DecoyThrownCount += player.DecoyThrownCount;
						data[player].MolotovThrownCount += player.MolotovThrownCount;
						data[player].IncendiaryThrownCount += player.IncendiaryThrownCount;
						data[player].DamageHealthCount += player.TotalDamageHealthCount;
						data[player].DamageArmorCount += player.TotalDamageArmorCount;
						data[player].EntryKillCount += player.EntryKills.Count;
						data[player].EntryKillWinCount += player.EntryKillWonCount;
						data[player].EntryKillLossCount += player.EntryKillLossCount;
						data[player].EntryHoldKillCount += player.EntryHoldKills.Count;
						data[player].EntryHoldKillWonCount += player.EntryHoldKillWonCount;
						data[player].EntryHoldKillLossCount += player.EntryHoldKillLossCount;
						data[player].TradeKillCount += player.TradeKillCount;
						data[player].TradeDeathCount += player.TradeDeathCount;
						data[player].AverageTimeDeathSeconds += player.AverageTimeDeath;
					}
				}

				int rowCount = 1;
				foreach (KeyValuePair<Player, PlayerStats> keyValuePair in data)
				{
					IRow row = Sheet.CreateRow(rowCount++);
					int columnNumber = 0;
					SetCellValue(row, columnNumber++, CellType.String, keyValuePair.Key.Name);
					SetCellValue(row, columnNumber++, CellType.String, keyValuePair.Key.SteamId);
					SetCellValue(row, columnNumber++, CellType.String, keyValuePair.Key.TeamName);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.MatchCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.KillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.AssistCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.DeathCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.KillPerDeath);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.HeadshotCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.HeadshotPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.RoundCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, Math.Round(keyValuePair.Value.EseaRws / keyValuePair.Value.MatchCount, 2));
					SetCellValue(row, columnNumber++, CellType.Numeric, Math.Round(keyValuePair.Value.Rating / keyValuePair.Value.MatchCount, 2));
					SetCellValue(row, columnNumber++, CellType.Numeric, Math.Round(keyValuePair.Value.Rating2 / keyValuePair.Value.MatchCount, 2));
					SetCellValue(row, columnNumber++, CellType.Numeric, Math.Round(keyValuePair.Value.AverageTimeDeathSeconds / keyValuePair.Value.MatchCount, 2));
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.FiveKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.FourKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.ThreeKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.TwoKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.OneKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.TradeKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.TradeDeathCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.TeamKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.JumpKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.CrouchKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.BombPlantedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.BombDefusedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.BombExplodedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.MvpCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.ScoreCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.ClutchCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.ClutchWonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.ClutchLostCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.ClutchWonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V1Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V1WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V1LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V1WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V2Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V2WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V2LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V2WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V3Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V3WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V3LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V3WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V4Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V4WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V4LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V4WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V5Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V5WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V5LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.Clutch1V5WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryKillWinCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryKillLossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryKillWinPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryHoldKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryHoldKillWonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryHoldKillLossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.EntryHoldKillWinPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.KillPerRound);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.AssistPerRound);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.DeathPerRound);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.AverageDamagePerRound);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.DamageHealthCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.DamageArmorCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.FlashbangThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.SmokeThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.HeGrenadeThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.DecoyThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.MolotovThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.IncendiaryThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, keyValuePair.Value.RankMax);
					SetCellValue(row, columnNumber++, CellType.Boolean, keyValuePair.Value.IsVacBanned);
					SetCellValue(row, columnNumber, CellType.Boolean, keyValuePair.Value.IsOverwatchBanned);
				}
			});
		}
	}
}
