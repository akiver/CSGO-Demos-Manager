using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core;
using Core.Models;
using Core.Models.Events;
using Core.Models.Serialization;
using Core.Models.Source;
using Services.Interfaces;
using Services.Models;

namespace Services.Concrete
{
	public class CacheService : ICacheService
	{
        #region Properties

        /// <summary>
        /// Directory's path where are located JSON files
        /// </summary>
        private readonly string _pathFolderCache;

        /// <summary>
        /// JSON Settings
        /// </summary>
        private readonly JsonSerializerSettings _settingsJson = new JsonSerializerSettings();

		private const string SUSPECT_FILENAME = "suspects.json";

		private const string SUSPECT_WHITELIST_FILENAME = "suspects_whitelist.json";

		/// <summary>
		///  Contains the ids of suspects that as been detected as banned
		/// </summary>
		private const string SUSPECT_BANNED_FILENAME = "suspects_banned.json";

		/// <summary>
		/// Contains the tracked Account
		/// </summary>
		private const string ACCOUNTS_FILENAME = "accounts.json";

		/// <summary>
		/// Contains the folders list saved
		/// </summary>
		private const string FOLDERS_FILENAME = "folders.json";

		/// <summary>
		/// Contains the last RankInfo detected for each account
		/// </summary>
		private const string RANKS_FILENAME = "ranks.json";

		/// <summary>
		/// Demo filename regex
		/// </summary>
		private readonly Regex _demoFilePattern = new Regex("^(.*?)(_|[0-9]+)(.json)$");

		/// <summary>
		/// Regex for demo's related files
		/// </summary>
		private readonly Regex _demoRelatedFilePattern = new Regex("^(.*?)(_|[0-9]+)_([^0-9]+\\.json)$");

		/// <summary>
		/// Filename suffix containing WeaponFire events
		/// </summary>
		private const string WEAPON_FIRED_FILE_SUFFIX = "_weapon_fired.json";

		/// <summary>
		/// Filename suffix containing PlayerBlinded events
		/// </summary>
		private const string PLAYER_BLINDED_FILE_SUFFIX = "_players_blinded.json";

		/// <summary>
		/// Filename containing demos basic data
		/// </summary>
		private const string FILENAME_DEMOS_BASIC_DATA = "demos.json";

		public DemoFilter Filter { get; set; }

		private readonly string[] _requiredFiles = {
			FILENAME_DEMOS_BASIC_DATA,
			SUSPECT_BANNED_FILENAME,
			SUSPECT_FILENAME,
			SUSPECT_WHITELIST_FILENAME,
			ACCOUNTS_FILENAME,
			RANKS_FILENAME,
		};

		#endregion

		public CacheService()
		{
			_pathFolderCache = AppSettings.GetFolderCachePath();
			foreach (string fileName in _requiredFiles)
			{
				string filePath = GetFilePath(fileName);
				if (!File.Exists(filePath)) File.Create(filePath);
			}
			Filter = new DemoFilter();
#if DEBUG

			_settingsJson.Formatting = Formatting.Indented;
#endif
			_settingsJson.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			_settingsJson.PreserveReferencesHandling = PreserveReferencesHandling.All;
			_settingsJson.NullValueHandling = NullValueHandling.Include;
		}

		public async Task<List<Demo>> GetDemoListAsync()
		{
			List<Demo> demos = new List<Demo>();
			string[] fileList = Directory.GetFiles(_pathFolderCache);

			foreach (string file in fileList)
			{
				if (File.Exists(file))
				{
					Match match = _demoFilePattern.Match(file);
					if (match.Success)
					{
						string json = File.ReadAllText(file);
						try
						{
							Demo demo = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Demo>(json, _settingsJson));
							demos.Add(demo);
						}
						catch (Exception e)
						{
							Logger.Instance.Log(e);
							throw;
						}
					}
				}
			}

			return demos;
		}

		public async Task<List<Demo>> GetFilteredDemoListAsync()
		{
			List<Demo> demos = new List<Demo>();
			try
			{
				if (Filter == null) return await GetDemoListAsync();
				// get basic demos data
				List<DemoBasicData> demoBasicDataList = await GetDemoBasicDataAsync();
				List<string> demoIdList = new List<string>();
				foreach (DemoBasicData basicData in demoBasicDataList)
				{
					// skip if the player isn't in the demo
					if (Filter.SteamId != 0 && !basicData.SteamIdList.Contains(Filter.SteamId))
						continue;

					// Skip if the demo's date isn't in the dates interval
					if (basicData.Date < Filter.From || basicData.Date > Filter.To.AddDays(1))
						continue;

					// skip if the demo is not in the selected folder
					if (!string.IsNullOrEmpty(Filter.Folder) && Path.GetDirectoryName(basicData.Path) != Filter.Folder || !File.Exists(basicData.Path))
						continue;

					// don't add it if its not cached
					if (!HasDemoInCache(basicData.Id)) continue;

					demoIdList.Add(basicData.Id);
				}

				foreach (string demoId in demoIdList)
				{
					Demo demo = await GetDemoDataFromCache(demoId);
					demos.Add(demo);
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}

			return demos;
		}

		/// <summary>
		/// Check if the JSON file for the demo exist
		/// </summary>
		/// <param name="demoId"></param>
		/// <returns></returns>
		public bool HasDemoInCache(string demoId)
		{
			string pathDemoFileJson = GetDemoFilePath(demoId);
			return File.Exists(pathDemoFileJson);
		}

		/// <summary>
		/// Return the demo's data from its JSON file
		/// </summary>
		/// <param name="demoId"></param>
		/// <returns></returns>
		public async Task<Demo> GetDemoDataFromCache(string demoId)
		{
			string pathDemoFileJson = GetDemoFilePath(demoId);

			string json = File.ReadAllText(pathDemoFileJson);

			Demo demo;
			try
			{
				demo = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Demo>(json, _settingsJson));
                demo.EnableUpdates();
				demo.Source = Source.Factory(demo.SourceName);
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				throw;
			}

			return demo;
		}

		/// <summary>
		/// Write the JSON file with demo's data
		/// Overwrite the file if it exists
		/// </summary>
		/// <param name="demo"></param>
		/// <returns></returns>
		public async Task WriteDemoDataCache(Demo demo)
		{
			string pathDemoFileJson = GetDemoFilePath(demo.Id);
			string pathDemoWeaponFiredFileJson = GetWeaponFiredFilePath(demo.Id);
			string pathDemoPlayerBlindedFileJson = GetPlayerBlindedFilePath(demo.Id);

			try
			{
				string json;
				// Save WeaponFire events to dedicated file and release it from RAM
				if (demo.WeaponFired.Count > 0)
				{
					json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(demo.WeaponFired, _settingsJson));
					File.WriteAllText(pathDemoWeaponFiredFileJson, json);
					demo.WeaponFired.Clear();
				}
				// Save PlayerBlindedEvent to dedicated file and release it from RAM
				if (demo.PlayerBlinded.Count > 0)
				{
					json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(demo.PlayerBlinded, _settingsJson));
					File.WriteAllText(pathDemoPlayerBlindedFileJson, json);
					demo.PlayerBlinded.Clear();
				}
				// save basic demo data
				await AddDemoBasicDataAsync(demo);

				// Save general demo's data
				json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(demo, _settingsJson));
				File.WriteAllText(pathDemoFileJson, json);
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				throw;
			}
		}

		/// <summary>
		/// Add a player to the suspects list
		/// </summary>
		/// <param name="suspectSteamCommunityId"></param>
		/// <returns></returns>
		public async Task<bool> AddSuspectToCache(string suspectSteamCommunityId)
		{
			// Check if he is already in the current suspect list
			List<string> ids = await GetSuspectsListFromCache();
			if (ids.Contains(suspectSteamCommunityId)) return false;

			// Check if he is in the user's account list
			List<Account> accountList = await GetAccountListAsync();
			if (accountList.Any(account => account.SteamId == suspectSteamCommunityId)) return false;

			// Remove from whitelist
			await RemovePlayerFromWhitelist(suspectSteamCommunityId);

			ids.Add(suspectSteamCommunityId);

			string json;
			try
			{
				json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(ids));
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				throw;
			}

			string pathSuspectsFileJson = GetFilePath(SUSPECT_FILENAME);
			File.WriteAllText(pathSuspectsFileJson, json);

			return true;
		}

		/// <summary>
		/// Add a SteamId to the suspects banned list
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns></returns>
		public async Task<bool> AddSteamIdToBannedList(string steamId)
		{
			// Get current list
			List<string> ids = await GetSuspectsBannedList();

			// Check if already in the list
			if (ids.Contains(steamId)) return false;

			ids.Add(steamId);
			// If not add it and update
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(ids));

			string pathSuspectsBannedFileJson = GetFilePath(SUSPECT_BANNED_FILENAME);
			File.WriteAllText(pathSuspectsBannedFileJson, json);

			return true;
		}

		/// <summary>
		/// Return the suspects SteamID from the list
		/// </summary>
		/// <returns></returns>
		public async Task<List<string>> GetSuspectsListFromCache()
		{
			string pathSuspectsFileJson = GetFilePath(SUSPECT_FILENAME);
			if (!File.Exists(pathSuspectsFileJson)) return new List<string>();
			string json = File.ReadAllText(pathSuspectsFileJson);
			List<string> ids = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<string>>(json));
			if (ids == null) ids = new List<string>();

			return ids;
		}

		/// <summary>
		/// Return the suspects banned SteamID from the list
		/// </summary>
		/// <returns></returns>
		public async Task<List<string>> GetSuspectsBannedList()
		{
			string pathSuspectsFileJson = GetFilePath(SUSPECT_BANNED_FILENAME);
			if (!File.Exists(pathSuspectsFileJson)) return new List<string>();
			string json = File.ReadAllText(pathSuspectsFileJson);
			List<string> ids = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<string>>(json));
			if (ids == null) ids = new List<string>();

			return ids;
		}

		/// <summary>
		/// Return the accounts SteamID from the list saved in accounts.json
		/// </summary>
		/// <returns></returns>
		public async Task<List<Account>> GetAccountListAsync()
		{
			string pathAccountsFileJson = GetFilePath(ACCOUNTS_FILENAME);
			if (!File.Exists(pathAccountsFileJson)) return new List<Account>();
			string json = File.ReadAllText(pathAccountsFileJson);
			List<Account> accounts = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Account>>(json));
			if (accounts == null) accounts = new List<Account>();

			return accounts;
		}

		public async Task<Account> GetAccountAsync(long steamId)
		{
			string pathAccountsFileJson = GetFilePath(ACCOUNTS_FILENAME);
			if (!File.Exists(pathAccountsFileJson)) return null;
			string json = File.ReadAllText(pathAccountsFileJson);
			List<Account> accounts = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Account>>(json));
			Account account = accounts?.FirstOrDefault(a => a.SteamId == steamId.ToString());
			return account;
		}

		public async Task<bool> AddAccountAsync(Account newAccount)
		{
			// Get current list
			List<Account> accounts = await GetAccountListAsync();

			// Check if already in the list
			if (accounts.Any(account => account.SteamId == newAccount.SteamId)) return false;
			accounts.Add(newAccount);

			// If not add it and update
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(accounts));

			string pathAccountsFileJson = GetFilePath(ACCOUNTS_FILENAME);
			File.WriteAllText(pathAccountsFileJson, json);

			return true;
		}

		public async Task<bool> UpdateAccountAsync(Account account)
		{
			List<Account> accounts = await GetAccountListAsync();

			int accountIndex = accounts.FindIndex(a => a.SteamId == account.SteamId);

			if (accountIndex == -1) return false;

			accounts[accountIndex] = account;
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(accounts));

			string pathAccountsFileJson = GetFilePath(ACCOUNTS_FILENAME);
			File.WriteAllText(pathAccountsFileJson, json);

			return true;
		}

		public async Task<bool> RemoveAccountAsync(Account accountToRemove)
		{
			List<Account> accounts = await GetAccountListAsync();
			Account accountFromJson = accounts.FirstOrDefault(a => a.SteamId == accountToRemove.SteamId);
			if (accountFromJson == null) return false;
			accounts.Remove(accountFromJson);
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(accounts));
			string pathAccountsFileJson = GetFilePath(ACCOUNTS_FILENAME);
			File.WriteAllText(pathAccountsFileJson, json);

			// remove its RankInfo data
			await RemoveRankInfoAsync(Convert.ToInt64(accountToRemove.SteamId));

			return true;
		}

		/// <summary>
		/// Remove a suspect from the list
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns></returns>
		public async Task<bool> RemoveSuspectFromCache(string steamId)
		{
			List<string> ids = await GetSuspectsListFromCache();
			if (!ids.Contains(steamId)) return false;

			ids.Remove(steamId);
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(ids));
			string pathSuspectsFileJson = GetFilePath(SUSPECT_FILENAME);
			File.WriteAllText(pathSuspectsFileJson, json);

			// If this suspect is in the banned suspects list, we remove it
			List<string> suspectBannedIdList = await GetSuspectsBannedList();
			if (suspectBannedIdList.Contains(steamId))
			{
				suspectBannedIdList.Remove(steamId);
				json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(suspectBannedIdList));
				pathSuspectsFileJson = GetFilePath(SUSPECT_BANNED_FILENAME);
				File.WriteAllText(pathSuspectsFileJson, json);
			}

			return true;
		}

		/// <summary>
		/// Delete all JSON demos files from cache folder
		/// </summary>
		/// <returns></returns>
		public Task ClearDemosFile()
		{
			return Task.Run(() =>
			{
				string[] fileList = Directory.GetFiles(_pathFolderCache);

				foreach (string file in fileList)
				{
					if (File.Exists(file))
					{
						Match match = _demoFilePattern.Match(file);
						Match matchDemoRelatedFile = _demoRelatedFilePattern.Match(file);
						if (match.Success || matchDemoRelatedFile.Success)
						{
							File.Delete(file);
						}
					}
				}
			});
		}

		/// <summary>
		/// Remove a specific demo from cache
		/// </summary>
		/// <returns></returns>
		public Task<bool> RemoveDemo(string demoId)
		{
			bool isDeleted = false;
			string demoFilePath = GetDemoFilePath(demoId);
			string weaponFiredFilePath = GetWeaponFiredFilePath(demoId);
			string playerBlindedFilePath = GetPlayerBlindedFilePath(demoId);
			if (File.Exists(demoFilePath))
			{
				File.Delete(demoFilePath);
				isDeleted = true;
			}
			if (File.Exists(weaponFiredFilePath)) File.Delete(weaponFiredFilePath);
			if (File.Exists(playerBlindedFilePath)) File.Delete(playerBlindedFilePath);
			return Task.FromResult(isDeleted);
		}

		public async Task<ObservableCollection<WeaponFireEvent>> GetDemoWeaponFiredAsync(Demo demo)
		{
			try
			{
				ObservableCollection<WeaponFireEvent> weaponFiredList = new ObservableCollection<WeaponFireEvent>();
				string pathDemoFileJson = GetWeaponFiredFilePath(demo.Id);
				if (!File.Exists(pathDemoFileJson)) return weaponFiredList;
				string json = File.ReadAllText(pathDemoFileJson);

				weaponFiredList = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ObservableCollection<WeaponFireEvent>>(json, _settingsJson));

				return weaponFiredList;
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				throw;
			}
		}

		public async Task<ObservableCollection<PlayerBlindedEvent>> GetDemoPlayerBlindedAsync(Demo demo)
		{
			string pathFile = GetPlayerBlindedFilePath(demo.Id);
			string json = File.ReadAllText(pathFile);

			ObservableCollection<PlayerBlindedEvent> playerBlindedList;
			try
			{
				playerBlindedList = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ObservableCollection<PlayerBlindedEvent>>(json, _settingsJson));
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				throw;
			}

			return playerBlindedList;
		}

		/// <summary>
		/// Create a backup file of all demos with custom data
		/// </summary>
		/// <param name="filePath">Backup file path</param>
		/// <returns></returns>
		public async Task CreateBackupCustomDataFile(string filePath)
		{
			List<Demo> demos = new List<Demo>();
			string[] fileList = Directory.GetFiles(_pathFolderCache);
			foreach (string file in fileList)
			{
				if (file != null)
				{
					Match match = _demoFilePattern.Match(Path.GetFileName(file));
					if (match.Success)
					{
						string json = File.ReadAllText(file);
						dynamic demo = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(json));
						Demo newDemo = new Demo
						{
							Id = demo.id,
							Status = demo.status,
							Comment = demo.comment
						};
						demos.Add(newDemo);
					}
				}
			}

			string jsonBackup = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(demos, new DemoListBackupConverter()));
			File.WriteAllText(filePath, jsonBackup);
		}

		public async Task<bool> GenerateJsonAsync(Demo demo, string folderPath)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				PreserveReferencesHandling = PreserveReferencesHandling.None,
				NullValueHandling = NullValueHandling.Include,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = Formatting.Indented
			};
			string filePath = folderPath + Path.DirectorySeparatorChar + demo.Name + ".json";
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(demo, settings));
			File.WriteAllText(filePath, json);

			return true;
		}

		/// <summary>
		/// Check if there is any demos in the cache folder
		/// </summary>
		/// <returns></returns>
		public bool ContainsDemos()
		{
			string[] fileList = Directory.GetFiles(_pathFolderCache);
			foreach (string file in fileList)
			{
				Match match = _demoFilePattern.Match(file);
				if (match.Success) return true;
			}
			return false;
		}

		public async Task<List<string>> GetFoldersAsync()
		{
			List<string> folders = new List<string>();
			string pathFoldersFileJson = GetFilePath(FOLDERS_FILENAME);
			if (!File.Exists(pathFoldersFileJson))
			{
				folders = await InitCsgoFolders(folders);
			}
			else
			{
				string json = File.ReadAllText(pathFoldersFileJson);
				folders = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<string>>(json));
				if (folders == null) return new List<string>();
				if (!folders.Any())
				{
					folders = await InitCsgoFolders(folders);
				}
			}
			
			return folders;
		}

		public async Task<bool> AddFolderAsync(string path)
		{
			List<string> folders = await GetFoldersAsync();
			if (folders.Contains(path)) return false;
			folders.Add(path);
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(folders));
			string pathFoldersFileJson = GetFilePath(FOLDERS_FILENAME);
			File.WriteAllText(pathFoldersFileJson, json);

			return true;
		}

		public async Task<bool> RemoveFolderAsync(string path)
		{
			List<string> folders = await GetFoldersAsync();
			if (!folders.Contains(path)) return false;
			folders.Remove(path);
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(folders));
			string pathFoldersFileJson = GetFilePath(FOLDERS_FILENAME);
			File.WriteAllText(pathFoldersFileJson, json);

			return true;
		}

		/// <summary>
		/// Add the "csgo" and "replays" folders if they exist
		/// </summary>
		private async Task<List<string>> InitCsgoFolders(List<string> folders)
		{
			string csgoPath = AppSettings.GetCsgoPath();
			if (csgoPath != null)
			{
				string csgoFolderPath = Path.GetFullPath(csgoPath).ToLower();
				if (Directory.Exists(csgoFolderPath)) folders.Add(csgoFolderPath);
				string replayFolderPath = Path.GetFullPath(csgoFolderPath + "/replays").ToLower();
				if (Directory.Exists(replayFolderPath)) folders.Add(replayFolderPath);
			}

			if (folders.Any())
			{
				string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(folders));
				string pathFoldersFileJson = GetFilePath(FOLDERS_FILENAME);
				File.WriteAllText(pathFoldersFileJson, json);
			}

			return folders;
		}

		/// <summary>
		/// Return the players SteamID from the whitelist
		/// </summary>
		/// <returns></returns>
		public async Task<List<string>> GetPlayersWhitelist()
		{
			string pathWhitelistFileJson = GetFilePath(SUSPECT_WHITELIST_FILENAME);
			if (!File.Exists(pathWhitelistFileJson)) return new List<string>();
			string json = File.ReadAllText(pathWhitelistFileJson);
			List<string> ids = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<string>>(json));
			if (ids == null) ids = new List<string>();

			return ids;
		}

		/// <summary>
		/// Add player to the whitelist
		/// </summary>
		/// <param name="suspectSteamCommunityId"></param>
		/// <returns></returns>
		public async Task<bool> AddPlayerToWhitelist(string suspectSteamCommunityId)
		{
			// Get current list
			List<string> ids = await GetPlayersWhitelist();

			// Check if he is already in the whitelist
			if (ids.Contains(suspectSteamCommunityId)) return false;

			// remove from suspects list
			await RemoveSuspectFromCache(suspectSteamCommunityId);

			ids.Add(suspectSteamCommunityId);

			string json;
			try
			{
				json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(ids));
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				throw;
			}

			string pathWhitelistFileJson = GetFilePath(SUSPECT_WHITELIST_FILENAME);
			File.WriteAllText(pathWhitelistFileJson, json);

			// remove players from suspects / banned list
			foreach (string steamId in ids)
			{
				await Task.Factory.StartNew(() => RemoveSuspectFromCache(steamId));
			}

			return true;
		}

		/// <summary>
		/// Remove a player from the whitelist
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns></returns>
		public async Task<bool> RemovePlayerFromWhitelist(string steamId)
		{
			List<string> ids = await GetPlayersWhitelist();
			if (!ids.Contains(steamId)) return false;

			ids.Remove(steamId);
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(ids));
			string pathWhitelistFileJson = GetFilePath(SUSPECT_WHITELIST_FILENAME);
			File.WriteAllText(pathWhitelistFileJson, json);

			// If this player is in the banned suspects list, we remove it
			List<string> suspectBannedIdList = await GetSuspectsBannedList();
			if (suspectBannedIdList.Contains(steamId))
			{
				suspectBannedIdList.Remove(steamId);
				json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(suspectBannedIdList));
				string pathSuspectsBannedFileJson = GetFilePath(SUSPECT_BANNED_FILENAME);
				File.WriteAllText(pathSuspectsBannedFileJson, json);
			}

			return true;
		}

		public Task<long> GetCacheSizeAsync()
		{
			return Task.FromResult(new DirectoryInfo(_pathFolderCache).GetFiles("*.json", SearchOption.AllDirectories).Sum(file => file.Length));
		}

		private string GetDemoFilePath(string demoId)
		{
			return _pathFolderCache + Path.DirectorySeparatorChar + demoId + ".json";
		}

		private string GetWeaponFiredFilePath(string demoId)
		{
			return _pathFolderCache + Path.DirectorySeparatorChar + demoId + WEAPON_FIRED_FILE_SUFFIX;
		}

		private string GetPlayerBlindedFilePath(string demoId)
		{
			return _pathFolderCache + Path.DirectorySeparatorChar + demoId + PLAYER_BLINDED_FILE_SUFFIX;
		}

		public async Task<List<RankInfo>> GetRankInfoListAsync()
		{
			string pathRankFile = GetFilePath(RANKS_FILENAME);
			string json = File.ReadAllText(pathRankFile);
			List<RankInfo>  rankAccountList = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<RankInfo>>(json));
			if (rankAccountList == null) rankAccountList = new List<RankInfo>();

			return rankAccountList;
		}

		public async Task<RankInfo> GetLastRankInfoAsync(long steamId = 0)
		{
			RankInfo rankInfo = null;
			List<RankInfo> rankAccountList = await GetRankInfoListAsync();
			if (rankAccountList.Any())
			{
				rankInfo = rankAccountList.FirstOrDefault(r => r.SteamId == steamId);
			}

			return rankInfo;
		}

		public async Task<Rank> GetLastRankAsync(long steamId = 0)
		{
			Rank rank = null;
			List<RankInfo> rankAccountList = await GetRankInfoListAsync();
			if (rankAccountList.Any())
			{
				RankInfo rankInfo = rankAccountList.FirstOrDefault(r => r.SteamId == steamId);
				if (rankInfo != null) rank = AppSettings.RankList.FirstOrDefault(r => r.Number == rankInfo.Number);
			}

			return rank;
		}

		public async Task<bool> SaveLastRankInfoAsync(RankInfo rankInfo)
		{
			try
			{
				// Get the current RankInfo list
				List<RankInfo> rankInfoList = await GetRankInfoListAsync();
				// Check if it's already in the list
				RankInfo lastRankInfo = rankInfoList.FirstOrDefault(r => r.SteamId == rankInfo.SteamId);
				if (lastRankInfo != null)
				{
					// Known account, update it
					rankInfoList.Remove(lastRankInfo);
					rankInfoList.Add(rankInfo);
				}
				else
				{
					// New account, add it
					rankInfoList.Add(rankInfo);
				}

				string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(rankInfoList));
				string pathWhitelistFileJson = GetFilePath(RANKS_FILENAME);
				File.WriteAllText(pathWhitelistFileJson, json);
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				throw;
			}

			return true;
		}

		public async Task<bool> UpdateRankInfoAsync(Demo demo, long steamId)
		{
			// We don't care about no valve demos
			if (demo.SourceName != "valve" || steamId == 0) return false;
			// Check if the player is in the demo
			Player player = demo.Players.FirstOrDefault(p => p.SteamId == steamId);
			if (player == null) return false;
			// Don't update if demo's date is higher than the known last rank date detected
			RankInfo lastRankInfo = await GetLastRankInfoAsync(steamId);
			if (lastRankInfo != null && lastRankInfo.LastDate > demo.Date) return false;
			lastRankInfo = new RankInfo
			{
				SteamId = player.SteamId,
				Number = player.RankNumberNew,
				LastDate = demo.Date
			};
			await SaveLastRankInfoAsync(lastRankInfo);

			return true;
		}

		public Task ClearRankInfoAsync()
		{
			return Task.Run(() =>
			{
				string pathRankFile = GetFilePath(RANKS_FILENAME);
				File.WriteAllText(pathRankFile, string.Empty);
			});
		}

		public async Task<bool> RemoveRankInfoAsync(long steamId)
		{
			List<RankInfo> rankInfoList = await GetRankInfoListAsync();
			RankInfo rankInfo = rankInfoList.FirstOrDefault(r => r.SteamId == steamId);
			if (rankInfo == null) return false;

			rankInfoList.Remove(rankInfo);
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(rankInfoList));
			string pathRankFile = GetFilePath(RANKS_FILENAME);
			File.WriteAllText(pathRankFile, json);

			return true;
		}

		public async Task<bool> DeleteVdmFiles()
		{
			List<string> folders = await GetFoldersAsync();
			try
			{
				foreach (string folderPath in folders)
				{
					foreach (string file in Directory.GetFiles(folderPath, "*.vdm").Where(
						item => item.EndsWith(".vdm")))
					{
						File.Delete(file);
					}
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				return false;
			}

			return true;
		}

		public bool HasDummyCacheFile()
		{
			return File.Exists(_pathFolderCache + Path.DirectorySeparatorChar +  AppSettings.DUMMY_CACHE_FILENAME);
		}

		public void DeleteDummyCacheFile()
		{
			string filePath = _pathFolderCache + Path.DirectorySeparatorChar + AppSettings.DUMMY_CACHE_FILENAME;
			if (File.Exists(filePath)) File.Delete(filePath); 
		}

		public async Task<DemoBasicData> AddDemoBasicDataAsync(Demo demo)
		{
			string filePath = GetFilePath(FILENAME_DEMOS_BASIC_DATA);
			List<DemoBasicData> data = await GetDemoBasicDataAsync();
			DemoBasicData demoData = new DemoBasicData
			{
				Id = demo.Id,
				Date = demo.Date,
				SteamIdList = demo.Players.Select(player => player.SteamId).ToList(),
				Path = demo.Path
			};
			if (data.Contains(demoData)) data.Remove(demoData);
			data.Add(demoData);
			string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(data));
			File.WriteAllText(filePath, json);

			return demoData;
		}

		public async Task<List<DemoBasicData>> GetDemoBasicDataAsync()
		{
			List<DemoBasicData> data = new List<DemoBasicData>();
			string filePath = GetFilePath(FILENAME_DEMOS_BASIC_DATA);
			if (!File.Exists(filePath)) return data;
			string json = File.ReadAllText(filePath);
			try
			{
				data = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<DemoBasicData>>(json));
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
			
			return data ?? new List<DemoBasicData>();
		}

		public async Task<bool> InitDemoBasicDataList()
		{
			bool hasUpdated = false;
			List<DemoBasicData> data = await GetDemoBasicDataAsync();
			for (int i = 0; i < data.Count; i++)
			{
				// demo has been moved / removed, remove it from cache
				if (!string.IsNullOrEmpty(data[i].Path) && !File.Exists(data[i].Path))
				{
					await RemoveDemo(data[i].Id);
					data.RemoveAt(i);
					hasUpdated = true;
				}
			}

			if (hasUpdated)
			{
				string filePath = GetFilePath(FILENAME_DEMOS_BASIC_DATA);
				string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(data));
				File.WriteAllText(filePath, json);
			}

			return hasUpdated;
		}

		/// <summary>
		/// Return file path for a given file name
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private string GetFilePath(string fileName)
		{
			return _pathFolderCache + Path.DirectorySeparatorChar + fileName;
		}
	}
}
