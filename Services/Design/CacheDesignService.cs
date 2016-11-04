using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Models;
using Core.Models.Events;
using Core.Models.Source;
using Services.Interfaces;
using Services.Models;

namespace Services.Design
{
	public class CacheDesignService : ICacheService
	{
		public DemoFilter Filter { get; set; }

		public bool HasDemoInCache(string demoId)
		{
			return true;
		}

		public Task<Demo> GetDemoDataFromCache(string demoId)
		{
			Random r = new Random();
			Demo demo = new Demo();
			demo.Source = new Valve();
			demo.Id = "de_dust25445648778447878";
			demo.Name = "esea_nip_vs_titan.dem";
			demo.Tickrate = 32;
			demo.MapName = "de_dust2";
			demo.ClientName = "localhost";
			demo.Hostname = "local";
			demo.OneKillCount = 90;
			demo.TwoKillCount = 20;
			demo.ThreeKillCount = 3;
			demo.FourKillCount = 2;
			demo.FiveKillCount = 1;
			demo.Path = "C:\\mydemo.dem";
			demo.ScoreTeam1 = 16;
			demo.ScoreTeam2 = 6;
			demo.ServerTickrate = 128;
			demo.ScoreFirstHalfTeam1 = 10;
			demo.ScoreFirstHalfTeam2 = 5;
			demo.ScoreSecondHalfTeam1 = 6;
			demo.ScoreSecondHalfTeam2 = 1;
			demo.Type = "GOTV";
			demo.Comment = "My comment";
			demo.Status = AppSettings.DefaultStatus[0].Label;

			Team teamCt = new Team();
			teamCt.Name = "Team 1";
			Team teamT = new Team();
			teamCt.Name = "Team 2";

			for (int i = 0; i < 10; i++)
			{
				Player p = new Player();
				p.Name = "Player " + (i + 1);
				p.AvatarUrl = "http://fakeimg.pl/184x184/";
				p.RankNumberOld = r.Next(18);
				p.KillCount = r.Next(40);
				p.AssistCount = r.Next(10);
				p.DeathCount = r.Next(20);
				p.HeadshotCount = r.Next(20);
				p.BombDefusedCount = r.Next(5);
				p.BombPlantedCount = r.Next(5);
				p.BombExplodedCount = r.Next(5);
				if (i < 5)
				{
					teamCt.Players.Add(p);
				}
				else
				{
					teamT.Players.Add(p);
				}
				demo.Players.Add(p);
			}

			for (int i = 0; i < 25; i++)
			{
				Round round = new Round();
				round.Tick = i * 300;
				round.Number = i + 1;
				if (i == 1 || i == 16)
					round.Type = RoundType.PISTOL_ROUND;
				else
					round.Type = r.Next(100) < 50 ? RoundType.NORMAL : RoundType.FORCE_BUY;

				if (r.Next(100) < 20)
				{
					round.BombDefused = new BombDefusedEvent(r.Next(10000), r.Next(1000));
				}
				if (r.Next(100) < 20 && round.BombDefused == null)
				{
					round.BombPlanted = new BombPlantedEvent(r.Next(10000), r.Next(1000));
				}
				if (round.BombDefused != null || round.BombExploded != null)
				{
					round.BombPlanted = new BombPlantedEvent(r.Next(10000), r.Next(1000));
				}
				round.KillCount = r.Next(10);
				round.FiveKillCount = r.Next(1);
				round.FourKillCount = r.Next(1);
				round.ThreeKillCount = r.Next(2);
				round.TwoKillCount = r.Next(3);
				round.OneKillCount = r.Next(5);
				round.WinnerSide = r.Next(100) < 50 ? Side.CounterTerrorist : Side.Terrorist;
				round.WinnerName = r.Next(100) < 50 ? teamCt.Name : teamT.Name;
				round.CrouchKillCount = r.Next(8);
				round.JumpKillCount = r.Next(1);
				round.EquipementValueTeam1 = r.Next(4000, 30000);
				round.EquipementValueTeam2 = r.Next(4000, 30000);
				round.TradeKillCount = r.Next(4);
				demo.Rounds.Add(round);
			}

			demo.MostBombPlantedPlayer = demo.Players[r.Next(10)];
			demo.MostEntryKillPlayer = demo.Players[r.Next(10)];
			demo.MostHeadshotPlayer = demo.Players[r.Next(10)];
			demo.MostKillingWeapon = Weapon.WeaponList[r.Next(30)];
			demo.TeamCT = teamCt;
			demo.TeamT = teamT;

			return Task.FromResult(demo);
		}

		public Task WriteDemoDataCache(Demo demo)
		{
			return Task.FromResult(0);
		}

		public Task<bool> AddSuspectToCache(string suspectSteamCommunityId)
		{
			return Task.FromResult(true);
		}

		public Task<List<string>> GetSuspectsListFromCache()
		{
			List<string> suspecIdtList = new List<string>()
			{
				"121545454",
				"5455155"
			};

			return Task.FromResult(suspecIdtList);
		}

		public Task<bool> RemoveSuspectFromCache(string steamId)
		{
			return Task.FromResult(true);
		}

		public Task ClearDemosFile()
		{
			return Task.FromResult(true);
		}

		public Task CreateBackupCustomDataFile(string filePath)
		{
			return Task.FromResult(true);
		}

		public bool ContainsDemos()
		{
			return true;
		}

		public Task<bool> AddSteamIdToBannedList(string steamId)
		{
			return Task.FromResult(true);
		}

		public Task<List<string>> GetSuspectsBannedList()
		{
			List<string> suspecIdtList = new List<string>()
			{
				"121545454",
				"5455155"
			};

			return Task.FromResult(suspecIdtList);
		}

		public Task<bool> AddAccountAsync(Account account)
		{
			return Task.FromResult(true);
		}

		public Task<bool> UpdateAccountAsync(Account account)
		{
			return Task.FromResult(true);
		}

		public Task<bool> RemoveAccountAsync(Account account)
		{
			return Task.FromResult(true);
		}

		public Task<List<Account>> GetAccountListAsync()
		{
			return Task.FromResult(new List<Account>());
		}

		public Task<Account> GetAccountAsync(long steamId)
		{
			return Task.FromResult(new Account());
		}

		public Task<List<string>> GetFoldersAsync()
		{
			return Task.FromResult(new List<string>());
		}

		public Task<bool> AddFolderAsync(string path)
		{
			return Task.FromResult(true);
		}

		public Task<bool> RemoveFolderAsync(string path)
		{
			return Task.FromResult(true);
		}

		public Task<List<string>> GetPlayersWhitelist()
		{
			return Task.FromResult(new List<string>());
		}

		public Task<bool> AddPlayerToWhitelist(string suspectSteamCommunityId)
		{
			return Task.FromResult(true);
		}

		public Task<bool> RemovePlayerFromWhitelist(string steamId)
		{
			return Task.FromResult(true);
		}

		public Task<bool> GenerateJsonAsync(Demo demo, string folderPath)
		{
			return Task.FromResult(true);
		}

		public Task<long> GetCacheSizeAsync()
		{
			return Task.FromResult((long)145214);
		}

		public Task<bool> RemoveDemo(string demoId)
		{
			return Task.FromResult(true);
		}

		public Task<ObservableCollection<WeaponFireEvent>> GetDemoWeaponFiredAsync(Demo demo)
		{
			Random r = new Random();
			ObservableCollection<WeaponFireEvent> weaponFire = new ObservableCollection<WeaponFireEvent>();
			for (int i = 0; i < 1000; i++)
			{
				weaponFire.Add(new WeaponFireEvent(r.Next(10000), r.Next(1000))
				{
					Weapon = Weapon.WeaponList[r.Next(30)],
					ShooterName = "Player " + r.Next(10),
					RoundNumber = r.Next(20)
				});
			}
			return Task.FromResult(weaponFire);
		}

		public Task<ObservableCollection<PlayerBlindedEvent>> GetDemoPlayerBlindedAsync(Demo demo)
		{
			return Task.FromResult(new ObservableCollection<PlayerBlindedEvent>());
		}

		public Task<RankInfo> GetLastRankInfoAsync(long steamId)
		{
			return Task.FromResult(new RankInfo
			{
				Number = 1,
				SteamId = 158841,
				LastDate = DateTime.Now
			});
		}

		public Task<Rank> GetLastRankAsync(long steamId)
		{
			return Task.FromResult(AppSettings.RankList[0]);
		}

		public Task<bool> SaveLastRankInfoAsync(RankInfo rankInfo)
		{
			return Task.FromResult(true);
		}

		public Task<List<RankInfo>> GetRankInfoListAsync()
		{
			return Task.FromResult(new List<RankInfo>
			{
				new RankInfo
				{
					Number = 2,
					LastDate = DateTime.Now,
					SteamId = 54454,
				}
			});
		}

		public Task<bool> UpdateRankInfoAsync(Demo demo, long steamId)
		{
			return Task.FromResult(true);
		}

		public Task ClearRankInfoAsync()
		{
			return Task.FromResult(true);
		}

		public Task<bool> RemoveRankInfoAsync(long steamId)
		{
			return Task.FromResult(true);
		}

		public Task<bool> DeleteVdmFiles()
		{
			return Task.FromResult(true);
		}

		public bool HasDummyCacheFile()
		{
			return true;
		}

		public void DeleteDummyCacheFile()
		{
		}

		public Task<DemoBasicData> AddDemoBasicDataAsync(Demo demo)
		{
			return Task.FromResult(new DemoBasicData());
		}

		public Task<List<DemoBasicData>> GetDemoBasicDataAsync()
		{
			return Task.FromResult(new List<DemoBasicData>());
		}

		public Task<bool> InitDemoBasicDataList()
		{
			return Task.FromResult(false);
		}

		public Task<bool> InitDemoCache()
		{
			return Task.FromResult(false);
		}

		public Task<bool> RefreshDemoCache()
		{
			return Task.FromResult(false);
		}

		public Task<List<Demo>> GetDemoListAsync()
		{
			List<Demo> demos = new List<Demo>();
			return Task.FromResult(demos);
		}

		public Task<List<Demo>> GetFilteredDemoListAsync()
		{
			List<Demo> demos = new List<Demo>();
			return Task.FromResult(demos);
		}
	}
}