using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Events;
using Services.Models;

namespace Services.Interfaces
{
	public interface ICacheService
	{
		/// <summary>
		/// Filter used to get only demos for specific player, date and folder
		/// </summary>
		DemoFilter Filter { get; set; }

		bool HasDemoInCache(string demoId);

		Task<Demo> GetDemoDataFromCache(string demoId);

		Task WriteDemoDataCache(Demo demo);

		Task<bool> AddSuspectToCache(string suspectSteamCommunityId);

		Task<List<string>> GetSuspectsListFromCache();

		Task<bool> RemoveSuspectFromCache(string steamId);

		Task ClearDemosFile();

		Task CreateBackupCustomDataFile(string filePath);

		bool ContainsDemos();

		Task<bool> AddSteamIdToBannedList(string steamId);

		Task<List<string>> GetSuspectsBannedList();

		Task<bool> AddAccountAsync(Account account);

		Task<bool> RemoveAccountAsync(Account account);

		Task<bool> UpdateAccountAsync(Account account);

		Task<List<Account>> GetAccountListAsync();

		Task<Account> GetAccountAsync(long steamId);

		Task<List<string>> GetFoldersAsync();

		Task<bool> AddFolderAsync(string path);

		Task<bool> RemoveFolderAsync(string path);

		/// <summary>
		/// Return all the demos inside each folders
		/// </summary>
		/// <returns></returns>
		Task<List<Demo>> GetDemoListAsync();

		/// <summary>
		/// Return filtered demos
		/// </summary>
		/// <returns></returns>
		Task<List<Demo>> GetFilteredDemoListAsync();

		Task<List<string>> GetPlayersWhitelist();

		Task<bool> AddPlayerToWhitelist(string suspectSteamCommunityId);

		Task<bool> RemovePlayerFromWhitelist(string steamId);

		/// <summary>
		/// Export demos to JSON files
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		Task<bool> GenerateJsonAsync(Demo demo, string folderPath);

		/// <summary>
		/// Return the size of the cache folder
		/// </summary>
		/// <returns></returns>
		Task<long> GetCacheSizeAsync();

		/// <summary>
		/// Remove demo from cache
		/// </summary>
		/// <param name="demoId"></param>
		/// <returns></returns>
		Task<bool> RemoveDemo(string demoId);

		/// <summary>
		/// Return the list of WeaponFire events of the demo
		/// </summary>
		/// <param name="demo"></param>
		/// <returns></returns>
		Task<ObservableCollection<WeaponFireEvent>> GetDemoWeaponFiredAsync(Demo demo);

		/// <summary>
		/// Return the list of PlayerBlindedEvent of the demo
		/// </summary>
		/// <param name="demo"></param>
		/// <returns></returns>
		Task<ObservableCollection<PlayerBlindedEvent>> GetDemoPlayerBlindedAsync(Demo demo);

		/// <summary>
		/// Return the last RankInfo detected for the selected account
		/// </summary>
		/// <returns></returns>
		Task<RankInfo> GetLastRankInfoAsync(long steamId);

		/// <summary>
		/// Return the last Rank detected for the steamId
		/// </summary>
		/// <returns></returns>
		Task<Rank> GetLastRankAsync(long steamId);

		/// <summary>
		/// Save the RankInfo
		/// </summary>
		/// <param name="rankInfo"></param>
		/// <returns></returns>
		Task<bool> SaveLastRankInfoAsync(RankInfo rankInfo);

		/// <summary>
		/// Return all RankInfo
		/// </summary>
		/// <returns></returns>
		Task<List<RankInfo>> GetRankInfoListAsync();

		/// <summary>
		/// Update the RankInfo if it's necessary (based on the demo's data)
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="steamId"></param>
		/// <returns></returns>
		Task<bool> UpdateRankInfoAsync(Demo demo, long steamId);

		/// <summary>
		/// Clear the RankInfo list
		/// </summary>
		/// <returns></returns>
		Task ClearRankInfoAsync();

		/// <summary>
		/// Remove a specific RankInfo based on the SteamID
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns></returns>
		Task<bool> RemoveRankInfoAsync(long steamId);

		/// <summary>
		/// Remove all vdm files within all folders
		/// </summary>
		/// <returns></returns>
		Task<bool> DeleteVdmFiles();

		/// <summary>
		/// Check if the dummy cache file exists
		/// </summary>
		/// <returns></returns>
		bool HasDummyCacheFile();

		/// <summary>
		/// Delete the dummy cache file
		/// </summary>
		/// <returns></returns>
		void DeleteDummyCacheFile();

		/// <summary>
		/// Add DemoBasicData to cache
		/// </summary>
		/// <param name="demo"></param>
		/// <returns></returns>
		Task<DemoBasicData> AddDemoBasicDataAsync(Demo demo);

		/// <summary>
		/// Return DemoBasicData demos list
		/// </summary>
		/// <returns></returns>
		Task<List<DemoBasicData>> GetDemoBasicDataAsync();

		/// <summary>
		/// Clean up possible missing demos from cache
		/// </summary>
		/// <returns></returns>
		Task<bool> InitDemoBasicDataList();

        Task<bool> InitDemoCache();

        Task<bool> RefreshDemoCache();
	}
}