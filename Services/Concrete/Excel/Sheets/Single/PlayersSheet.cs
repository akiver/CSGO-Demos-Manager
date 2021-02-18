using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using NPOI.SS.UserModel;

namespace Services.Concrete.Excel.Sheets.Single
{
	public class PlayersSheet : AbstractSingleSheet
	{
		public PlayersSheet(IWorkbook workbook, Demo demo)
		{
			Headers = new Dictionary<string, CellType>(){
				{ "Name", CellType.String },
				{ "SteamID", CellType.String },
				{ "Rank", CellType.Numeric },
				{ "Team", CellType.String },
				{ "Kills", CellType.Numeric },
				{ "Assists", CellType.Numeric },
				{ "Deaths", CellType.Numeric },
				{ "K/D", CellType.Numeric },
				{ "HS", CellType.Numeric },
				{ "HS%", CellType.Numeric },
				{ "Team kill", CellType.Numeric },
				{ "Entry kill", CellType.Numeric },
				{ "Bomb planted", CellType.Numeric },
				{ "Bomb defused", CellType.Numeric },
				{ "MVP", CellType.Numeric },
				{ "Score", CellType.Numeric },
				{ "RWS", CellType.Numeric },
				{ "Rating", CellType.Numeric },
				{ "Rating 2", CellType.Numeric },
				{ "ATD (s)", CellType.Numeric },
				{ "KPR", CellType.Numeric },
				{ "APR", CellType.Numeric },
				{ "DPR", CellType.Numeric },
				{ "ADR", CellType.Numeric },
				{ "TDH", CellType.Numeric },
				{ "TDA", CellType.Numeric },
				{ "5K", CellType.Numeric },
				{ "4K", CellType.Numeric },
				{ "3K", CellType.Numeric },
				{ "2K", CellType.Numeric },
				{ "1K", CellType.Numeric },
				{ "Trade kill", CellType.Numeric },
				{ "Trade death", CellType.Numeric },
				{ "Crouch kill", CellType.Numeric },
				{ "Jump kill", CellType.Numeric },
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
				{ "Flashbang", CellType.Numeric },
				{ "Smoke", CellType.Numeric },
				{ "HE", CellType.Numeric },
				{ "Decoy", CellType.Numeric },
				{ "Molotov", CellType.Numeric },
				{ "Incendiary", CellType.Numeric },
				{ "VAC", CellType.Boolean },
				{ "OW", CellType.Boolean },
			};
			Demo = demo;
			Sheet = workbook.CreateSheet("Players");
		}

		public override async Task GenerateContent()
		{
			await Task.Factory.StartNew(() =>
			{
				var rowNumber = 1;

				foreach (Player player in Demo.Players)
				{
					IRow row = Sheet.CreateRow(rowNumber);
					int columnNumber = 0;
					SetCellValue(row, columnNumber++, CellType.String, player.Name);
					SetCellValue(row, columnNumber++, CellType.String, player.SteamId.ToString());
					SetCellValue(row, columnNumber++, CellType.Numeric, player.RankNumberNew);
					SetCellValue(row, columnNumber++, CellType.String, player.TeamName);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.KillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.AssistCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.DeathCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, (double)player.KillDeathRatio);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.HeadshotCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.HeadshotPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.TeamKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.EntryKills.Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.BombPlantedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.BombDefusedCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.RoundMvpCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Score);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.EseaRws);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.RatingHltv);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.RatingHltv2);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.AverageTimeDeath);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.KillPerRound);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.AssistPerRound);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.DeathPerRound);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.AverageHealthDamage);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.TotalDamageHealthCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.TotalDamageArmorCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.FiveKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.FourKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.ThreeKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.TwoKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.OneKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.TradeKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.TradeDeathCount);
					SetCellValue(row, columnNumber++, CellType.Boolean, player.CrouchKillCount);
					SetCellValue(row, columnNumber++, CellType.Boolean, player.JumpKillCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V1Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V1WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V1LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V1WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V2Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V2WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V2LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V2WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V3Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V3WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V3LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V3WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V4Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V4WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V4LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V4WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V5Count);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V5WonCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V5LossCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.Clutch1V5WonPercent);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.FlashbangThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.SmokeThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.HeGrenadeThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.DecoyThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.MolotovThrownCount);
					SetCellValue(row, columnNumber++, CellType.Numeric, player.IncendiaryThrownCount);
					SetCellValue(row, columnNumber++, CellType.Boolean, player.IsVacBanned);
					SetCellValue(row, columnNumber, CellType.Boolean, player.IsOverwatchBanned);

					rowNumber++;
				}
			});
		}
	}
}