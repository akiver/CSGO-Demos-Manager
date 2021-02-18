using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Events;
using DemoInfo;
using Services.Interfaces;
using Services.Models.Charts;
using Services.Models.Stats;
using Player = Core.Models.Player;

namespace Services.Concrete
{
	public class AccountStatsService : IAccountStatsService
	{
		public long SelectedStatsAccountSteamId { get; set; }

		public Task<Demo> MapSelectedAccountValues(Demo demo, long accountSteamId = 0)
		{
			if (accountSteamId == 0) return Task.FromResult(demo);
			demo.WinStatus = GetWinStatus(demo, accountSteamId);

			Player player = demo.Players.FirstOrDefault(p => p.SteamId == accountSteamId);
			if (player != null)
			{
				demo.KillCount = player.KillCount;
				demo.OneKillCount = player.OneKillCount;
				demo.TwoKillCount = player.TwoKillCount;
				demo.ThreeKillCount = player.ThreeKillCount;
				demo.FourKillCount = player.FourKillCount;
				demo.FiveKillCount = player.FiveKillCount;
				demo.ClutchCount = player.ClutchCount;
				demo.AssistPerRound = player.AssistPerRound;
				demo.BombDefusedCount = player.BombDefusedCount;
				demo.BombExplodedCount = player.BombExplodedCount;
				demo.BombPlantedCount = player.BombPlantedCount;
				demo.CrouchKillCount = player.CrouchKillCount;
				demo.JumpKillCount = player.JumpKillCount;
				demo.TradeKillCount = player.TradeKillCount;
				demo.KillPerRound = player.KillPerRound;
				demo.DeathPerRound = player.DeathPerRound;
				demo.DamageHealthCount = player.TotalDamageHealthCount;
				demo.DamageArmorCount = player.TotalDamageArmorCount;
				demo.AverageHealthDamage = player.AverageHealthDamage;
				demo.AverageHltvRating = player.RatingHltv;
				demo.AverageHltv2Rating = player.RatingHltv2;
				demo.AverageEseaRws = player.EseaRws;
				demo.ClutchWonCount = player.ClutchWonCount;
				demo.WinStatus = GetWinStatus(demo, accountSteamId);
			}

			return Task.FromResult(demo);
		}

		public async Task<OverallStats> GetGeneralAccountStatsAsync(List<Demo> demos)
		{
			OverallStats stats = new OverallStats();
			if (SelectedStatsAccountSteamId == 0) return stats;

			await Task.Run(() =>
			{
				foreach (Demo demo in demos)
				{
					Player player = demo.Players.FirstOrDefault(p => p.SteamId == SelectedStatsAccountSteamId);
					if (player != null)
					{
						stats.MatchCount++;
						stats.KillCount += player.KillCount;
						stats.AssistCount += player.AssistCount;
						stats.DeathCount += player.DeathCount;
						stats.KnifeKillCount += demo.Kills.Count(k => k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Knife);
						stats.EntryKillCount += player.EntryKills.Count;
						stats.FiveKillCount += player.FiveKillCount;
						stats.FourKillCount += player.FourKillCount;
						stats.ThreeKillCount += player.ThreeKillCount;
						stats.TwoKillCount += player.TwoKillCount;
						stats.HeadshotCount += player.HeadshotCount;
						stats.BombDefusedCount += player.BombDefusedCount;
						stats.BombExplodedCount += player.BombExplodedCount;
						stats.BombPlantedCount += player.BombPlantedCount;
						stats.MvpCount += player.RoundMvpCount;
						stats.DamageCount += player.TotalDamageHealthCount;
						stats.RoundCount += player.RoundPlayedCount;
						stats.ClutchCount += player.ClutchCount;
						stats.ClutchWin += player.ClutchWonCount;
						stats.HltvRating += player.RatingHltv;
						stats.Hltv2Rating += player.RatingHltv2;
						stats.EseaRws += (double)player.EseaRws;
						stats.OneVersusOneCount += player.Clutch1V1WonCount;
						stats.OneVersusTwoCount += player.Clutch1V2WonCount;
						stats.OneVersusThreeCount += player.Clutch1V3WonCount;
						stats.OneVersusFourCount += player.Clutch1V4WonCount;
						stats.OneVersusFiveCount += player.Clutch1V5WonCount;
						switch (GetWinStatus(demo, SelectedStatsAccountSteamId))
						{
							case "lost":
							case "lost-s":
								stats.MatchLossCount++;
								break;
							case "draw":
								stats.MatchDrawCount++;
								break;
							case "won":
							case "won-s":
								stats.MatchWinCount++;
								break;
						}
					}
					stats.TotalMatchesDuration += demo.Duration;
				}

				if (stats.KillCount != 0 && stats.DeathCount != 0)
				{
					stats.KillDeathRatio = Math.Round(stats.KillCount / (decimal)stats.DeathCount, 2);
				}
				if (stats.KillCount != 0 && stats.HeadshotCount != 0)
				{
					stats.HeadshotRatio = Math.Round(((decimal)stats.HeadshotCount * 100) / stats.KillCount, 2);
				}
				if (stats.HltvRating > 0)
				{
					stats.HltvRating = Math.Round(stats.HltvRating / stats.MatchCount, 2);
				}
				if (stats.Hltv2Rating > 0)
				{
					stats.Hltv2Rating = Math.Round(stats.Hltv2Rating / stats.MatchCount, 2);
				}
				if (stats.EseaRws > 0)
				{
					stats.EseaRws = Math.Round(stats.EseaRws / stats.MatchCount, 2);
				}
			});

			return stats;
		}

		public async Task<MapStats> GetMapStatsAsync(List<Demo> demos)
		{
			MapStats stats = new MapStats();
			if (SelectedStatsAccountSteamId == 0) return stats;

			await Task.Run(() =>
			{
				if (demos.Any())
				{
					List<Demo> demosPlayerList = demos.Where(demo => demo.Players.FirstOrDefault(p => p.SteamId == SelectedStatsAccountSteamId) != null).ToList();
					if (demosPlayerList.Any())
					{
						stats.Dust2WinCount = demosPlayerList.Count(d => d.MapName == "de_dust2" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.Dust2LossCount = demosPlayerList.Count(d => d.MapName == "de_dust2" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.Dust2DrawCount = demosPlayerList.Count(d => d.MapName == "de_dust2" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						int matchCount = stats.Dust2WinCount + stats.Dust2LossCount + stats.Dust2DrawCount;
						if (matchCount > 0)
						{
							stats.Dust2WinPercentage = Math.Round((stats.Dust2WinCount / (double)matchCount * 100), 2);
						}

						stats.MirageWinCount = demosPlayerList.Count(d => d.MapName == "de_mirage" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.MirageLossCount = demosPlayerList.Count(d => d.MapName == "de_mirage" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.MirageDrawCount = demosPlayerList.Count(d => d.MapName == "de_mirage" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.MirageWinCount + stats.MirageLossCount + stats.MirageDrawCount;
						if (matchCount > 0)
						{
							stats.MirageWinPercentage = Math.Round((stats.MirageWinCount / (double)matchCount * 100), 2);
						}

						stats.InfernoWinCount = demosPlayerList.Count(d => d.MapName == "de_inferno" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.InfernoLossCount = demosPlayerList.Count(d => d.MapName == "de_inferno" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.InfernoDrawCount = demosPlayerList.Count(d => d.MapName == "de_inferno" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.InfernoWinCount + stats.InfernoLossCount + stats.InfernoDrawCount;
						if (matchCount > 0)
						{
							stats.InfernoWinPercentage = Math.Round((stats.InfernoWinCount / (double)matchCount * 100), 2);
						}

						stats.TrainWinCount = demosPlayerList.Count(d => d.MapName == "de_train" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.TrainLossCount = demosPlayerList.Count(d => d.MapName == "de_train" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.TrainDrawCount = demosPlayerList.Count(d => d.MapName == "de_train" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.TrainWinCount + stats.TrainLossCount + stats.TrainDrawCount;
						if (matchCount > 0)
						{
							stats.TrainWinPercentage = Math.Round((stats.TrainWinCount / (double)matchCount * 100), 2);
						}

						stats.OverpassWinCount = demosPlayerList.Count(d => d.MapName == "de_overpass" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.OverpassLossCount = demosPlayerList.Count(d => d.MapName == "de_overpass" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.OverpassDrawCount = demosPlayerList.Count(d => d.MapName == "de_overpass" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.OverpassWinCount + stats.OverpassLossCount + stats.OverpassDrawCount;
						if (matchCount > 0)
						{
							stats.OverpassWinPercentage = Math.Round((stats.OverpassWinCount / (double)matchCount * 100), 2);
						}

						stats.CacheWinCount = demosPlayerList.Count(d => d.MapName == "de_cache" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.CacheLossCount = demosPlayerList.Count(d => d.MapName == "de_cache" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.CacheDrawCount = demosPlayerList.Count(d => d.MapName == "de_cache" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.CacheWinCount + stats.CacheLossCount + stats.CacheDrawCount;
						if (matchCount > 0)
						{
							stats.CacheWinPercentage = Math.Round((stats.CacheWinCount / (double)matchCount * 100), 2);
						}

						stats.CobblestoneWinCount = demosPlayerList.Count(d => d.MapName == "de_cbble" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.CobblestoneLossCount = demosPlayerList.Count(d => d.MapName == "de_cbble" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.CobblestoneDrawCount = demosPlayerList.Count(d => d.MapName == "de_cbble" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.CobblestoneWinCount + stats.CobblestoneLossCount + stats.CobblestoneDrawCount;
						if (matchCount > 0)
						{
							stats.CobblestoneWinPercentage = Math.Round((stats.CobblestoneWinCount / (double)matchCount * 100), 2);
						}

						stats.VertigoWinCount = demosPlayerList.Count(d => d.MapName == "de_vertigo" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.VertigoLossCount = demosPlayerList.Count(d => d.MapName == "de_vertigo" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.VertigoDrawCount = demosPlayerList.Count(d => d.MapName == "de_vertigo" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.VertigoWinCount + stats.VertigoLossCount + stats.VertigoDrawCount;
						if (matchCount > 0)
						{
							stats.VertigoWinPercentage = Math.Round((stats.VertigoWinCount / (double)matchCount * 100), 2);
						}

						stats.NukeWinCount = demosPlayerList.Count(d => d.MapName == "de_nuke" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == 2));
						stats.NukeLossCount = demosPlayerList.Count(d => d.MapName == "de_nuke" && (GetWinStatusCode(d, SelectedStatsAccountSteamId) == -1 || GetWinStatusCode(d, SelectedStatsAccountSteamId) == -2));
						stats.NukeDrawCount = demosPlayerList.Count(d => d.MapName == "de_nuke" && GetWinStatusCode(d, SelectedStatsAccountSteamId) == 0);
						matchCount = stats.NukeWinCount + stats.NukeLossCount + stats.NukeDrawCount;
						if (matchCount > 0)
						{
							stats.NukeWinPercentage = Math.Round((stats.NukeWinCount / (double)matchCount * 100), 2);
						}
					}
				}
			});

			return stats;
		}

		public async Task<WeaponStats> GetWeaponStatsAsync(List<Demo> demos)
		{
			WeaponStats stats = new WeaponStats();
			if (SelectedStatsAccountSteamId == 0) return stats;

			await Task.Run(() =>
			{
				List<Demo> demosPlayerList = demos.Where(demo => demo.Players.FirstOrDefault(p => p.SteamId == SelectedStatsAccountSteamId) != null).ToList();
				if (demosPlayerList.Any())
				{
					foreach (Demo demo in demosPlayerList)
					{
						// Rifles
						stats.KillAk47Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.AK47);
						stats.DeathAk47Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.AK47);

						stats.KillM4A4Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.M4A4);
						stats.DeathM4A4Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.M4A4);

						stats.KillM4A1Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.M4A1);
						stats.DeathM4A1Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.M4A1);

						stats.KillAugCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.AUG);
						stats.DeathAugCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.AUG);

						stats.KillGalilarCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Gallil);
						stats.DeathGalilarCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Gallil);

						stats.KillSg553Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.SG556);
						stats.DeathSg553Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.SG556);

						stats.KillFamasCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Famas);
						stats.DeathFamasCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Famas);

						// Snipers
						stats.KillAwpCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.AWP);
						stats.DeathAwpCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.AWP);

						stats.KillScoutCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Scout);
						stats.DeathScoutCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Scout);

						stats.KillScar20Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Scar20);
						stats.DeathScar20Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Scar20);

						stats.KillG3Sg1Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.G3SG1);
						stats.DeathG3Ssg1Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.G3SG1);

						// SMGs
						stats.KillMp7Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.MP7);
						stats.DeathMp7Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.MP7);

						stats.KillMp5Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.MP5SD);
						stats.DeathMp5Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.MP5SD);

						stats.KillMp9Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.MP9);
						stats.DeathMp9Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.MP9);

						stats.KillP90Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.P90);
						stats.DeathP90Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.P90);

						stats.KillBizonCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Bizon);
						stats.DeathBizonCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Bizon);

						stats.KillMac10Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Mac10);
						stats.DeathMac10Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Mac10);

						stats.KillUmp45Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.UMP);
						stats.DeathUmp45Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.UMP);

						// Heavy
						stats.KillNovaCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Nova);
						stats.DeathNovaCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Nova);

						stats.KillXm1014Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.XM1014);
						stats.DeathXm1014Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.XM1014);

						stats.KillMag7Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Swag7);
						stats.DeathMag7Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Swag7);

						stats.KillSawedOffCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.SawedOff);
						stats.DeathSawedOffCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.SawedOff);

						stats.KillM249Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.M249);
						stats.DeathM249Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.M249);

						stats.KillNegevCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Negev);
						stats.DeathNegevCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Negev);

						// Pistols
						stats.KillGlockCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Glock);
						stats.DeathGlockCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Glock);

						stats.KillUspCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.USP);
						stats.DeathUspCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.USP);

						stats.KillP2000Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.P2000);
						stats.DeathP2000Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.P2000);

						stats.KillP250Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.P250);
						stats.DeathP250Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.P250);

						stats.KillCz75Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.CZ);
						stats.DeathCz75Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.CZ);

						stats.KillDeagleCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Deagle);
						stats.DeathDeagleCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Deagle);

						stats.KillDualEliteCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.DualBarettas);
						stats.DeathDualEliteCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.DualBarettas);

						stats.KillTec9Count +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Tec9);
						stats.DeathTec9Count +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Tec9);

						stats.KillFiveSevenCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.FiveSeven);
						stats.DeathFiveSevenCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.FiveSeven);

						// Equipment
						stats.KillHeGrenadeCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.HE);
						stats.DeathHeGrenadeCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.HE);

						stats.KillMolotovCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Molotov);
						stats.DeathMolotovCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Molotov);

						stats.KillIncendiaryCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Incendiary);
						stats.DeathIncendiaryCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Incendiary);

						stats.KillTazerCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Zeus);
						stats.DeathTazerCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Zeus);

						stats.KillKnifeCount +=
							demo.Kills.Count(
								k => k.KillerSteamId != 0 && k.KillerSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Knife);
						stats.DeathKnifeCount +=
							demo.Kills.Count(
								k => k.KilledSteamId != 0 && k.KilledSteamId == SelectedStatsAccountSteamId && k.Weapon.Element == EquipmentElement.Knife);

						stats.FlashbangThrownCount +=
							demo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).FlashbangThrownCount;
						stats.SmokeThrownCount +=
							demo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).SmokeThrownCount;
						stats.DecoyThrownCount +=
							demo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).DecoyThrownCount;
						stats.HeGrenadeThrownCount +=
							demo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).HeGrenadeThrownCount;
						stats.MolotovThrownCount +=
							demo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).MolotovThrownCount;
						stats.IncendiaryThrownCount +=
							demo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).IncendiaryThrownCount;
					}
				}
			});

			return stats;
		}

		public async Task<List<RankDateChart>> GetRankDateChartDataAsync(List<Demo> demos, string scale = "none")
		{
			List<RankDateChart> datas = new List<RankDateChart>();
			await Task.Run(() =>
			{
				if (demos.Any())
				{
					List<Demo> demosPlayerList = demos.Where(demo => demo.Players.FirstOrDefault(p => p.SteamId == SelectedStatsAccountSteamId) != null).ToList();
					if (demosPlayerList.Any())
					{
						// Sort by date
						demosPlayerList.Sort((d1, d2) => d1.Date.CompareTo(d2.Date));
						for (int i = 0; i < demosPlayerList.Count; i++)
						{
							Demo currentDemo = demosPlayerList[i];
							Demo nextDemo = demosPlayerList.ElementAtOrDefault(i + 1);
							// Ignore demos where all players have no rank, sometimes CCSUsrMsg_ServerRankUpdate isn't raised
							if (currentDemo.Players.Count(p => p.RankNumberOld != 0) > 0)
							{
								int newRankNumber = currentDemo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).RankNumberNew;
								int oldRankNumber = currentDemo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).RankNumberOld;
								int winCount = currentDemo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId).WinCount;
								switch (scale)
								{
									case "none":
										datas.Add(new RankDateChart
										{
											Date = currentDemo.Date,
											OldRank = oldRankNumber,
											NewRank = newRankNumber,
											WinCount = winCount,
											WinStatus = GetWinStatusCode(currentDemo, SelectedStatsAccountSteamId)
										});
										break;
									case "day":
										if (nextDemo == null || nextDemo.Date.Day != currentDemo.Date.Day)
										{
											RankDateChart previousData = datas.LastOrDefault();
											oldRankNumber = previousData != null ? (int)previousData.NewRank : oldRankNumber;
											datas.Add(new RankDateChart
											{
												Date = currentDemo.Date,
												OldRank = oldRankNumber,
												NewRank = newRankNumber,
												WinCount = winCount,
												WinStatus = GetWinStatusCode(currentDemo, SelectedStatsAccountSteamId)
											});
										}
										break;
									case "month":
										if (nextDemo == null || nextDemo.Date.Month != currentDemo.Date.Month)
										{
											RankDateChart previousData = datas.LastOrDefault();
											oldRankNumber = previousData != null ? (int)previousData.NewRank : oldRankNumber;
											datas.Add(new RankDateChart
											{
												Date = currentDemo.Date,
												OldRank = oldRankNumber,
												NewRank = newRankNumber,
												WinCount = winCount,
												WinStatus = GetWinStatusCode(currentDemo, SelectedStatsAccountSteamId)
											});
										}
										break;
								}
							}
						}
					}
				}
			});

			return datas;
		}

		public async Task<ProgressStats> GetProgressStatsAsync(List<Demo> demos)
		{
			ProgressStats stats = new ProgressStats();
			await Task.Run(() =>
			{
				if (demos.Any())
				{
					List<Demo> demosPlayerList = demos.Where(demo => demo.Players.FirstOrDefault(p => p.SteamId == SelectedStatsAccountSteamId) != null).ToList();
					if (demosPlayerList.Any())
					{
						double maximumVelocity = 0;
						demosPlayerList.Sort((d1, d2) => d1.Date.CompareTo(d2.Date));
						DateTime currentDate = new DateTime(demosPlayerList[0].Date.Year, demosPlayerList[0].Date.Month, demosPlayerList[0].Date.Day);
						int matchCount = 0;
						int winCount = 0;
						int headshotCount = 0;
						int killCount = 0;
						int deathCount = 0;
						int damageCount = 0;
						int rifleKillCount = 0;
						int heavyKillCount = 0;
						int sniperKillCount = 0;
						int pistolKillCount = 0;
						int smgKillCount = 0;
						int crouchKillCount = 0;
						Dictionary<WeaponType, double> velocityStats = new Dictionary<WeaponType, double>();
						foreach (Demo demo in demosPlayerList)
						{
							matchCount++;
							DateTime demoDate = new DateTime(demo.Date.Year, demo.Date.Month, demo.Date.Day);
							if (demo.Equals(demosPlayerList.First()) || demo.Equals(demosPlayerList.Last())
								|| demoDate >= currentDate.AddDays(7))
							{
								stats.Win.Add(new WinDateChart
								{
									Date = demoDate,
									WinPercentage = 0
								});
								stats.HeadshotRatio.Add(new HeadshotDateChart
								{
									Date = demoDate,
									HeadshotPercentage = 0
								});
								stats.Damage.Add(new DamageDateChart
								{
									Date = demoDate,
									DamageCount = 0
								});
								stats.Kill.Add(new KillDateChart
								{
									Date = demoDate,
									KillAverage = 0,
									DeathAverage = 0
								});
								stats.KillVelocityRifle.Add(new KillVelocityChart
								{
									Date = demoDate,
									VelocityAverage = 0
								});
								stats.KillVelocityPistol.Add(new KillVelocityChart
								{
									Date = demoDate,
									VelocityAverage = 0
								});
								stats.KillVelocitySmg.Add(new KillVelocityChart
								{
									Date = demoDate,
									VelocityAverage = 0
								});
								stats.KillVelocitySniper.Add(new KillVelocityChart
								{
									Date = demoDate,
									VelocityAverage = 0
								});
								stats.KillVelocityHeavy.Add(new KillVelocityChart
								{
									Date = demoDate,
									VelocityAverage = 0
								});
								stats.CrouchKill.Add(new CrouchKillDateChart
								{
									Date = demoDate,
									CrouchKillPercentage = 0
								});

								currentDate = demoDate;
								matchCount = 1;
								winCount = 0;
								headshotCount = 0;
								killCount = 0;
								deathCount = 0;
								damageCount = 0;
								rifleKillCount = 0;
								heavyKillCount = 0;
								sniperKillCount = 0;
								pistolKillCount = 0;
								smgKillCount = 0;
								crouchKillCount = 0;
								velocityStats.Clear();
							}

							if (GetWinStatusCode(demo, SelectedStatsAccountSteamId) == 1 || GetWinStatusCode(demo, SelectedStatsAccountSteamId) == 2) winCount++;
							if (winCount > 0) stats.Win.Last().WinPercentage = Math.Round((winCount / (double)matchCount * 100), 2);
							Player player = demo.Players.First(p => p.SteamId == SelectedStatsAccountSteamId);
							damageCount += player.TotalDamageHealthCount;
							killCount += player.KillCount;
							deathCount += player.DeathCount;
							headshotCount += player.HeadshotCount;
							crouchKillCount += player.CrouchKillCount;

							foreach (KillEvent e in demo.Kills)
							{
								if (e.KillerSteamId == SelectedStatsAccountSteamId)
								{
									if (!velocityStats.ContainsKey(e.Weapon.Type)) velocityStats.Add(e.Weapon.Type, 0);
									switch (e.Weapon.Type)
									{
										case WeaponType.Rifle:
											rifleKillCount++;
											break;
										case WeaponType.Heavy:
											heavyKillCount++;
											break;
										case WeaponType.Pistol:
											pistolKillCount++;
											break;
										case WeaponType.SMG:
											smgKillCount++;
											break;
										case WeaponType.Sniper:
											sniperKillCount++;
											break;
									}
									velocityStats[e.Weapon.Type] += Math.Abs(e.KillerVelocityY + e.KillerVelocityX + e.KillerVelocityZ);
								}
							}

							stats.Damage.Last().DamageCount = Math.Round((double)damageCount / matchCount, 2);
							stats.Kill.Last().KillAverage = Math.Round((double)killCount / matchCount, 1);
							stats.Kill.Last().DeathAverage = Math.Round((double)deathCount / matchCount, 1);
							if (killCount > 0)
							{
								stats.HeadshotRatio.Last().HeadshotPercentage = Math.Round((headshotCount / (double)killCount * 100), 2);
								stats.CrouchKill.Last().CrouchKillPercentage = Math.Round((crouchKillCount / (double)killCount * 100), 2);
							}

							if (velocityStats.ContainsKey(WeaponType.Rifle))
								stats.KillVelocityRifle.Last().VelocityAverage = Math.Round(velocityStats[WeaponType.Rifle] / rifleKillCount, 1);
							if (velocityStats.ContainsKey(WeaponType.Pistol))
								stats.KillVelocityPistol.Last().VelocityAverage = Math.Round(velocityStats[WeaponType.Pistol] / pistolKillCount, 1);
							if (velocityStats.ContainsKey(WeaponType.Sniper))
								stats.KillVelocitySniper.Last().VelocityAverage = Math.Round(velocityStats[WeaponType.Sniper] / sniperKillCount, 1);
							if (velocityStats.ContainsKey(WeaponType.SMG))
								stats.KillVelocitySmg.Last().VelocityAverage = Math.Round(velocityStats[WeaponType.SMG] / smgKillCount, 1);
							if (velocityStats.ContainsKey(WeaponType.Heavy))
								stats.KillVelocityHeavy.Last().VelocityAverage = Math.Round(velocityStats[WeaponType.Heavy] / heavyKillCount, 1);
						}
						maximumVelocity = stats.KillVelocityPistol.Select(k
							=> k.VelocityAverage).Concat(new[] { maximumVelocity }).Max();
						maximumVelocity = stats.KillVelocityRifle.Select(k
							=> k.VelocityAverage).Concat(new[] { maximumVelocity }).Max();
						maximumVelocity = stats.KillVelocityHeavy.Select(k
							=> k.VelocityAverage).Concat(new[] { maximumVelocity }).Max();
						maximumVelocity = stats.KillVelocitySmg.Select(k
							=> k.VelocityAverage).Concat(new[] { maximumVelocity }).Max();
						maximumVelocity = stats.KillVelocitySniper.Select(k
							=> k.VelocityAverage).Concat(new[] { maximumVelocity }).Max();
						stats.MaximumVelocity = maximumVelocity;
					}
				}
			});

			return stats;
		}

		/// <summary>
		/// Return the result of the match for the SteamID
		/// -3 : player not found / error
		/// -2 : loss by surrender
		/// -1 : loss
		/// 0 : draw
		/// 1 : win
		/// 2 : win by surrender
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="accountSteamId"></param>
		/// <returns></returns>
		private static int GetWinStatusCode(Demo demo, long accountSteamId = 0)
		{
			if (accountSteamId == 0 || demo.ScoreTeamCt == 0 && demo.ScoreTeamT == 0) return -3;

			Player player = demo.Players.FirstOrDefault(p => p.SteamId == accountSteamId);
			if (player == null) return -3;

			if (demo.Surrender != null)
			{
				player = demo.Surrender.Players.FirstOrDefault(p => p.SteamId == accountSteamId);
				if (player != null) return -2;
				return 2;
			}

			// was in CT?
			if (player.TeamName == demo.TeamCT.Name)
			{
				if (demo.ScoreTeamCt == demo.ScoreTeamT) return 0;
				if (demo.ScoreTeamCt > demo.ScoreTeamT) return 1;
				return -1;
			}

			// was in T?
			if (player.TeamName == demo.TeamT.Name)
			{
				if (demo.ScoreTeamCt == demo.ScoreTeamT) return 0;
				if (demo.ScoreTeamCt < demo.ScoreTeamT) return 1;
				return -1;
			}

			return -3;
		}

		private static string GetWinStatus(Demo demo, long accountSteamId = 0)
		{
			int code = GetWinStatusCode(demo, accountSteamId);
			switch (code)
			{
				case -1:
					return "lost";
				case -2:
					return "lost-s";
				case 0:
					return "draw";
				case 1:
					return "won";
				case 2:
					return "won-s";
				default:
					return string.Empty;
			}
		}
	}
}
