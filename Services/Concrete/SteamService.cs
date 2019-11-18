using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Core;
using Core.Models;
using Core.Models.Steam;
using Newtonsoft.Json;
using Services.Interfaces;

namespace Services.Concrete
{
	public class SteamService : ISteamService
	{
		private const string PLAYERS_BAN_URL = "http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={0}&steamids={1}";
		private const string PLAYERS_SUMMARIES_URL = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v1/?key={0}&steamids={1}";
		private const string STEAM_RESOLVE_VANITY_URL = "http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={0}&vanityurl={1}";
		private const string BOILER_EXE_NAME = "boiler.exe";
		private const string BOILER_SHA1 = "80F2C8A1F51118FA450AB9E700645508172B01B8";

		/// <summary>
		/// Return suspect list that have been banned
		/// </summary>
		/// <param name="suspectIdList"></param>
		/// <param name="suspectBannedIdList"></param>
		/// <returns></returns>
		public async Task<List<Suspect>> GetNewSuspectBannedList(List<string> suspectIdList, List<string> suspectBannedIdList)
		{
			List<Suspect> newSuspectBannedList = new List<Suspect>();
			// Get the SteamIDs that aren't in the banned list
			List<string> idToCheck = suspectIdList.Where(steamId => !suspectBannedIdList.Contains(steamId)).ToList();

			// Add new banned suspects to the returned list
			if (idToCheck.Any())
			{
				IEnumerable<Suspect> suspects = await GetBanStatusForUserList(idToCheck);
				newSuspectBannedList.AddRange(suspects.Where(suspect => suspect.VacBanned || suspect.BanCount > 0));
			}

			return newSuspectBannedList;
		}

		public async Task<Suspect> GetBanStatusForUser(string steamId)
		{
			Suspect suspect = new Suspect();
			try
			{
				using (var httpClient = new HttpClient())
				{
					//  Grab general infos from user
					string url = string.Format(PLAYERS_SUMMARIES_URL, Properties.Resources.steam_api_key, steamId);
					HttpResponseMessage result = await httpClient.GetAsync(url);
					if (result.StatusCode == HttpStatusCode.OK)
					{
						string json = await result.Content.ReadAsStringAsync();
						JObject o = JObject.Parse(json);
						var playerSummary = o.SelectToken("response.players.player[0]").ToObject<PlayerSummary>();
						if (playerSummary == null) return null;

						// Grab VAC ban infos from user
						url = string.Format(PLAYERS_BAN_URL, Properties.Resources.steam_api_key, steamId);
						result = await httpClient.GetAsync(url);
						if (result.StatusCode == HttpStatusCode.OK)
						{
							json = await result.Content.ReadAsStringAsync();
							o = JObject.Parse(json);
							var playerBan = o.SelectToken("players[0]").ToObject<PlayerBan>();
							if (playerBan == null) return null;

							suspect = new Suspect
							{
								SteamId = playerSummary.SteamId,
								ProfileUrl = playerSummary.ProfileUrl,
								Nickname = playerSummary.PersonaName,
								LastLogOff = playerSummary.LastLogoff,
								CurrentStatus = playerSummary.PersonaState,
								ProfileState = playerSummary.ProfileState,
								AvatarUrl = playerSummary.AvatarFull,
								CommunityVisibilityState = playerSummary.CommunityVisibilityState,
								DaySinceLastBanCount = playerBan.DaysSinceLastBan,
								BanCount = playerBan.NumberOfVacBans,
								VacBanned = playerBan.VacBanned,
								CommunityBanned = playerBan.CommunityBanned,
								EconomyBan = playerBan.EconomyBan,
								GameBanCount = playerBan.NumberOfGameBans
							};

							return suspect;
						}

						return null;
					}

					return null;
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}

			return suspect;
		}

		public async Task<List<Suspect>> GetBanStatusForUserList(List<string> allUsers)
		{
			List<Suspect> suspects = new List<Suspect>(allUsers.Count);
			try
            {
				using (var httpClient = new HttpClient())
				{
					foreach (var users in allUsers.Batch(80))
					{
						string ids = string.Join(",", users.ToArray());
						//  Grab general infos from user
						string url = string.Format(PLAYERS_SUMMARIES_URL, Properties.Resources.steam_api_key, ids);
                        if (url.Length >= 2083)
                        {
                            Logger.Instance.Log(new Exception("The following URL is too long: " + url));
                        }

                        HttpResponseMessage result = await httpClient.GetAsync(url);
                        result.EnsureSuccessStatusCode();
					    string json = await result.Content.ReadAsStringAsync();
					    var o = JObject.Parse(json);
					    var playerSummaryList = o.SelectToken("response.players.player").ToObject<List<PlayerSummary>>();
					    if (playerSummaryList == null) return null;

					    // Grab VAC ban infos from user
					    url = string.Format(PLAYERS_BAN_URL, Properties.Resources.steam_api_key, ids);
                        if (url.Length >= 2083)
                        {
                            Logger.Instance.Log(new Exception("The following URL is too long: " + url));
                        }
                        result = await httpClient.GetAsync(url);
                        result.EnsureSuccessStatusCode();
						json = await result.Content.ReadAsStringAsync();
						o = JObject.Parse(json);
						var playerBanList = o.SelectToken("players").ToObject<List<PlayerBan>>();
						if (playerBanList == null) return null;

						foreach (PlayerSummary playerSummary in playerSummaryList)
						{
							if (playerSummary != null)
							{
								Suspect suspect = new Suspect
								{
									SteamId = playerSummary.SteamId,
									ProfileUrl = playerSummary.ProfileUrl,
									Nickname = playerSummary.PersonaName,
									LastLogOff = playerSummary.LastLogoff,
									CurrentStatus = playerSummary.PersonaState,
									ProfileState = playerSummary.ProfileState,
									AvatarUrl = playerSummary.AvatarFull,
									CommunityVisibilityState = playerSummary.CommunityVisibilityState
								};
								suspects.Add(suspect);
							}
						}

						foreach (PlayerBan playerBan in playerBanList)
						{
							suspects.First(pl => pl.SteamId == playerBan.SteamId).DaySinceLastBanCount = playerBan.DaysSinceLastBan;
							suspects.First(pl => pl.SteamId == playerBan.SteamId).BanCount = playerBan.NumberOfVacBans;
							suspects.First(pl => pl.SteamId == playerBan.SteamId).VacBanned = playerBan.VacBanned;
							suspects.First(pl => pl.SteamId == playerBan.SteamId).CommunityBanned = playerBan.CommunityBanned;
							suspects.First(pl => pl.SteamId == playerBan.SteamId).EconomyBan = playerBan.EconomyBan;
							suspects.First(pl => pl.SteamId == playerBan.SteamId).GameBanCount = playerBan.NumberOfGameBans;
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}

			return suspects;
		}

		public async Task<List<PlayerSummary>> GetUserSummaryAsync(List<string> users)
		{
			List<PlayerSummary> playerSummaryList = new List<PlayerSummary>();
			try
			{
				using (var httpClient = new HttpClient())
				{
					string ids = string.Join(",", users.ToArray());
					//  Grab general infos from user
					string url = string.Format(PLAYERS_SUMMARIES_URL, Properties.Resources.steam_api_key, ids);
					HttpResponseMessage result = await httpClient.GetAsync(url);
					if (result.StatusCode == HttpStatusCode.OK)
					{
						string json = await result.Content.ReadAsStringAsync();
						JObject o = JObject.Parse(json);
						playerSummaryList = o.SelectToken("response.players.player").ToObject<List<PlayerSummary>>();
						if (playerSummaryList == null) return new List<PlayerSummary>();
					}
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}

			return playerSummaryList;
		}

		public async Task<int> GenerateMatchListFile(CancellationToken ct)
		{
			int result = await StartBoiler(ct);

			return result;
		}

		public async Task<int> DownloadDemoFromShareCode(string shareCode, CancellationToken ct)
		{
			ShareCode.ShareCodeStruct s = ShareCode.Decode(shareCode);

			int result = await StartBoiler(ct, args: $"{s.MatchId} {s.OutcomeId} {s.TokenId}");

			return result;
		}

		public async Task<string> GetSteamIdFromSteamProfileUsername(string username)
		{
			try
			{
				using (var httpClient = new HttpClient())
				{
					string url = string.Format(STEAM_RESOLVE_VANITY_URL, Properties.Resources.steam_api_key, username);
					HttpResponseMessage result = await httpClient.GetAsync(url);
					if (result.StatusCode == HttpStatusCode.OK)
					{
						string json = await result.Content.ReadAsStringAsync();
						VanityUrlResponse obj = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<VanityUrlResponse>(json));
						if (obj?.Response != null && obj.Response.Success == 1)
						{
							return obj.Response.SteamId;
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}

			return string.Empty;
		}

		public async Task<string> GetSteamIdFromUrlOrSteamId(string urlOrSteamId)
		{
			long steamInputAsLong;
			bool isSteamInputLong = long.TryParse(urlOrSteamId, out steamInputAsLong);
			if (isSteamInputLong) return urlOrSteamId;
			Match match = AppSettings.STEAM_COMMUNITY_URL_REGEX.Match(urlOrSteamId);
			if (match.Success)
			{
				string value = match.Groups["steamID"].Value;
				long steamUrl;
				bool isLongSteam = long.TryParse(value, out steamUrl);
				if (isLongSteam) return value;
				// value is player's username
				string steamId = await GetSteamIdFromSteamProfileUsername(value);
				return steamId;
			}

			return string.Empty;
		}

		private static string GetSha1HashFile(string filePath)
		{
			using (FileStream stream = File.OpenRead(filePath))
			{
				SHA1Managed sha = new SHA1Managed();
				byte[] hash = sha.ComputeHash(stream);
				return BitConverter.ToString(hash).Replace("-", string.Empty);
			}
		}

		public async Task<List<string>> GetNewSuspectBannedArray(List<string> suspectIdList, List<string> suspectBannedIdList)
		{
			// Get the SteamIDs that aren't in the banned list
			List<string> idToCheck = suspectIdList.Where(steamId => !suspectBannedIdList.Contains(steamId)).ToList();
			List<string> steamIdBannedList = new List<string>();
			// Add new banned suspects to the returned list
			if (idToCheck.Any())
			{
				IEnumerable<Suspect> suspects = await GetBanStatusForUserList(idToCheck);
				foreach (Suspect suspect in suspects)
				{
					if (suspect.VacBanned || suspect.BanCount > 0)
					{
						steamIdBannedList.Add(suspect.SteamId);
					}
				}
			}

			return steamIdBannedList;
		}

		private static async Task<int> StartBoiler(CancellationToken ct, string args = "")
		{
            var boilerExe = BOILER_EXE_NAME;
            var workDir = Environment.CurrentDirectory;
            if (!File.Exists(boilerExe))
            {
                // find relative to assembly
                var loc = Path.GetDirectoryName(typeof(SteamService).Assembly.Location);
                var localBoiler = Path.Combine(loc, BOILER_EXE_NAME);
                if (File.Exists(localBoiler))
                {
                    boilerExe = localBoiler;
                    workDir = Path.GetDirectoryName(boilerExe);
                }
            }

            // https://www.reddit.com/r/GlobalOffensive/comments/2uqovq/boiler_a_tool_to_archive_your_match_history/
            // https://bitbucket.org/ACB/boiler/src/master/boiler/

            // Maybe replace with https://steamworks.github.io/snippits/??
            ct.ThrowIfCancellationRequested();
			string hash = GetSha1HashFile(boilerExe);
			if (!hash.Equals(BOILER_SHA1)) return 2;

			Process[] currentProcess = Process.GetProcessesByName("csgo");
			if (currentProcess.Length > 0) currentProcess[0].Kill();

            Process boiler = new Process
            {
                StartInfo =
                {
                    FileName = boilerExe,
                    Arguments = $"\"{AppSettings.GetMatchListDataFilePath()}\" {args}",
                    WorkingDirectory = workDir,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			boiler.Start();
			await boiler.WaitForExitAsync(ct);

			return boiler.ExitCode;
		}
	}
}
