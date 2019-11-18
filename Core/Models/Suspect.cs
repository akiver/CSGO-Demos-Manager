using System;
using GalaSoft.MvvmLight;

namespace Core.Models
{
	public class Suspect : ObservableObject
	{
		#region Properties

		/// <summary>
		/// 64bit SteamID of the user
		/// </summary>
		private string _steamId;

		/// <summary>
		/// This represents whether the profile is visible or not, and if it is visible, why you are allowed to see it.
		/// Note that because this WebAPI does not use authentication, there are only two possible values returned:
		/// 1 - the profile is not visible to you (Private, Friends Only, etc)
		/// 3 - the profile is "Public", and the data is visible.
		/// Mike Blaszczak's post on Steam forums says, "The community visibility state this API returns is different than the privacy state.
		/// It's the effective visibility state from the account making the request to the account being viewed given the requesting account's relationship to the viewed account."
		/// </summary>
		private int _communityVisibilityState;

		/// <summary>
		/// If set, indicates the user has a community profile configured (will be set to '1')
		/// </summary>
		private int _profileState;

		/// <summary>
		/// The player's persona name (display name)
		/// </summary>
		private string _nickname;

		/// <summary>
		/// The last time the user was online, in unix time.
		/// </summary>
		private int _lastLogOff = 0;

		/// <summary>
		/// The full URL of the player's Steam Community profile.
		/// </summary>
		private string _profileUrl;

		/// <summary>
		/// The full URL of the player's 184x184px avatar.
		/// If the user hasn't configured an avatar, this will be the default ? avatar.
		/// </summary>
		private string _avatarUrl;

		/// <summary>
		/// The user's current status.
		/// 0 - Offline, 1 - Online, 2 - Busy, 3 - Away, 4 - Snooze, 5 - looking to trade, 6 - looking to play.
		/// If the player's profile is private, this will always be "0", except is the user has set his status to looking to trade or looking to play,
		/// because a bug makes those status appear even if the profile is private.
		/// </summary>
		private int _currentStatus;

		/// <summary>
		/// Indicates whether or not the player is banned from Steam Community.
		/// </summary>
		private bool _communityBanned;

		/// <summary>
		/// Indicates whether or not the player has VAC bans on record.
		/// </summary>
		private bool _vacBanned;

		/// <summary>
		/// The player's ban status in the economy.
		/// If the player has no bans on record the string will be "none", if the player is on probation it will say "probation", etc.
		/// </summary>
		private string _economyBan;

		/// <summary>
		/// VAC ban count (only if at least one ban)
		/// </summary>
		private int _banCount;

		/// <summary>
		/// Days count since last ban (only if at least one ban)
		/// </summary>
		private int _daySinceLastBanCount;

		/// <summary>
		/// Game ban count
		/// </summary>
		private int _gameBanCount;

		#endregion

		#region Accessors

		public string SteamId
		{
			get { return _steamId; }
			set { Set(ref _steamId, value); }
		}

		public string Nickname
		{
			get { return _nickname; }
			set { Set(ref _nickname, value); }
		}

		public string AvatarUrl
		{
			get { return _avatarUrl; }
			set { Set(ref _avatarUrl, value); }
		}

		public int CurrentStatus
		{
			get { return _currentStatus; }
			set { Set(ref _currentStatus, value); }
		}

		public int CommunityVisibilityState
		{
			get { return _communityVisibilityState; }
			set { Set(ref _communityVisibilityState, value); }
		}

		public string LastLogOffDate
		{
			get
			{
				DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				dt = dt.AddSeconds(_lastLogOff).ToLocalTime();
				return string.Format("{0:d/M/yyyy HH:mm:ss}", dt);
			}
		}

		public int LastLogOff
		{
			get { return _lastLogOff; }
			set { Set(ref _lastLogOff, value); }
		}

		public int ProfileState
		{
			get { return _profileState; }
			set { Set(ref _profileState, value); }
		}

		public string ProfileUrl
		{
			get { return _profileUrl; }
			set { Set(ref _profileUrl, value); }
		}

		public bool CommunityBanned
		{
			get { return _communityBanned; }
			set { Set(ref _communityBanned, value); }
		}

		public bool VacBanned
		{
			get { return _vacBanned; }
			set { Set(ref _vacBanned, value); }
		}

		public int BanCount
		{
			get { return _banCount; }
			set { Set(ref _banCount, value); }
		}

		public int GameBanCount
		{
			get { return _gameBanCount; }
			set { Set(ref _gameBanCount, value); }
		}

		public int DaySinceLastBanCount
		{
			get { return _daySinceLastBanCount; }
			set { Set(ref _daySinceLastBanCount, value); }
		}

		public string EconomyBan
		{
			get { return _economyBan; }
			set { Set(ref _economyBan, value); }
		}

		#endregion
	}
}
