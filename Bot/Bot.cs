using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Services.Concrete;

namespace SuspectsBot
{
	public class Bot
	{
		private readonly SteamService _steamService;

		private readonly CacheService _cacheService;

		public bool IsRunning { get; set; }

		/// <summary>
		/// If CSGO DM is running, this flag is set to true when it has fully loaded
		/// </summary>
		private bool _isCsgoDmLoaded;

		private CancellationTokenSource _cts;

		private Process _csgoProcess;

		public bool IsCsgoDmLoaded
		{
			get { return _isCsgoDmLoaded; }
			set
			{
				_isCsgoDmLoaded = value;
				// Start a check when CSGO DM is loaded
				if (value) Task.Factory.StartNew(async () => await Check());
			}
		}

		/// <summary>
		/// Last time a check has been done
		/// </summary>
		private DateTime _lastCheckDate;

		public event EventHandler<SuspectBannedEventArgs> SuspectBanned;

		public event EventHandler<CsgoClosedEvent> CsgoClosed;

		public async Task<bool> Check()
        {
            _lastCheckDate = DateTime.Now;
            List<string> steamIdList = await _cacheService.GetSuspectsListFromCache();
			List<string> bannedSteamIdList = await _cacheService.GetSuspectsBannedList();
			List<string> newBannedSteamIdList = await _steamService.GetNewSuspectBannedArray(steamIdList, bannedSteamIdList);

			if (newBannedSteamIdList.Count > 0)
			{
				// add new ban to cache
				foreach (string steamId in newBannedSteamIdList)
				{
					await _cacheService.AddSteamIdToBannedList(steamId);
				}
				SuspectBannedEventArgs e = new SuspectBannedEventArgs
				{
					SteamIdList = newBannedSteamIdList
				};
				SuspectBanned?.Invoke(this, e);

				return true;
			}

			return false;
		}

		public Bot()
		{
			_cacheService = new CacheService();
			_steamService = new SteamService();
			Start();
		}

		public void Start()
		{
			Task.Run(async () =>
			{
				IsRunning = true;
				if (_cts == null) _cts = new CancellationTokenSource();
				while (IsRunning)
				{
					try
					{
						if (ShouldCheck()) await Check();
						if (Properties.Settings.Default.SendDownloadNotifications && _csgoProcess == null && AppSettings.IsCsgoRunning())
						{
							Process[] processes = Process.GetProcessesByName(AppSettings.CSGO_PROCESS_NAME);
							_csgoProcess = processes[0];
							_csgoProcess.EnableRaisingEvents = true;
							_csgoProcess.Exited += OnCsgoExited;
						}
						await Task.Delay(60000, _cts.Token);
					}
					catch (Exception e)
					{
						if (!(e is TaskCanceledException))
						{
							Logger.Instance.Log(e);
						}
					}
				}
			});
		}

		private void OnCsgoExited(object sender, EventArgs eventArgs)
		{
			CsgoClosed?.Invoke(this, new CsgoClosedEvent());
			_csgoProcess = null;
		}

		public void Stop()
		{
			IsRunning = false;
			_cts.Cancel();
			_cts = null;
			_lastCheckDate = DateTime.MinValue;
		}

		private bool ShouldCheck()
		{
			if (!AppSettings.IsInternetConnectionAvailable())
			{
				return false;
			}
			if ((DateTime.Now - _lastCheckDate).TotalMinutes < Properties.Settings.Default.CheckDelayMinutes)
			{
				return false;
			}
			if (Win32Utils.IsForegroundWwindowFullScreen())
			{
				return false;
			}
			if (AppSettings.IsCsgoRunning())
			{
				return false;
			}
			// don't check if CSGO DM is started but not fully loaded to prevent early notifications
			if (AppSettings.IsRunning() && !IsCsgoDmLoaded)
			{
				return false;
			}

			return true;
		}
	}

	public class SuspectBannedEventArgs : EventArgs
	{
		public List<string> SteamIdList { get; set; }
	}

	public class CsgoClosedEvent : EventArgs
	{
	}
}
