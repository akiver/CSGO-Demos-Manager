using System;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Core.Models.Events;
using Core.Models.Serialization;
using DemoInfo;

namespace Core.Models
{
	public enum RoundType
	{
		ECO = 0,
		SEMI_ECO = 1,
		NORMAL = 2,
		FORCE_BUY = 3,
		PISTOL_ROUND = 4
	}

	public static class RoundExtenstions
	{
		public static string AsString(this RoundEndReason reason)
		{
			switch (reason)
			{
				case RoundEndReason.CTWin:
					return AppSettings.CT_WIN;
				case RoundEndReason.TerroristWin:
					return AppSettings.T_WIN;
				case RoundEndReason.TargetBombed:
					return AppSettings.BOMB_EXPLODED;
				case RoundEndReason.BombDefused:
					return AppSettings.BOMB_DEFUSED;
				case RoundEndReason.CTSurrender:
					return AppSettings.CT_SURRENDER;
				case RoundEndReason.TerroristsSurrender:
					return AppSettings.T_SURRENDER;
				case RoundEndReason.TargetSaved:
					return AppSettings.TARGET_SAVED;
				default:
					return AppSettings.UNKNOWN;
			}
		}

		public static string AsString(this RoundType type)
		{
			switch (type)
			{
				case RoundType.ECO:
					return AppSettings.ECO;
				case RoundType.SEMI_ECO:
					return AppSettings.SEMI_ECO;
				case RoundType.FORCE_BUY:
					return AppSettings.FORCE_BUY;
				case RoundType.NORMAL:
					return AppSettings.NORMAL;
				case RoundType.PISTOL_ROUND:
					return AppSettings.PISTOL_ROUND;
				default:
					return AppSettings.UNKNOWN;
			}
		}
	}

	public class Round : ObservableObject
	{
		#region Properties

		/// <summary>
		/// Round's number
		/// </summary>
		private int _number = 1;

		/// <summary>
		/// Round's start tick
		/// </summary>
		private int _tick;

		/// <summary>
		/// Round's end tick
		/// </summary>
		private int _endTick;

		/// <summary>
		/// Round's officially ended tick
		/// </summary>
		private int _endTickOfficially;

		/// <summary>
		/// Round's duration (seconds).
		/// </summary>
		private float _duration;

		/// <summary>
		/// Tick when the round's freezetime ended.
		/// </summary>
		private int _freezetimeEndTick;

		/// <summary>
		/// Why the round ended
		/// </summary>
		private RoundEndReason _endReason;

		/// <summary>
		/// Equipement value of the team that started the round as T
		/// </summary>
		private int _equipementValueTeamT;

		/// <summary>
		/// Equipement value of the team that started the round as CT
		/// </summary>
		private int _equipementValueTeamCt;

		/// <summary>
		/// Start money of the team that started the round as CT
		/// </summary>
		private int _startMoneyTeamCt;

		/// <summary>
		/// Start money of the team that started the round as T
		/// </summary>
		private int _startMoneyTeamT;

		/// <summary>
		/// Round's side winner
		/// </summary>
		private Side _winnerSide;

		/// <summary>
		/// Refers to the side currently on eco / semi-eco or force buy
		/// </summary>
		private Side _sideTrouble;

		/// <summary>
		/// Round's type (pistol round, eco...)
		/// </summary>
		private RoundType _type;

		/// <summary>
		/// Refers to the team currently on eco / semi-eco or force buy
		/// </summary>
		private string _teamTroubleName;

		/// <summary>
		/// Round's team winner
		/// </summary>
		private string _winnerName;

		/// <summary>
		/// CT team's name
		/// </summary>
		private string _teamCt;

		/// <summary>
		/// T team's name
		/// </summary>
		private string _teamT;

		/// <summary>
		/// Number of 1K during the round
		/// </summary>
		private int _onekillCount;

		/// <summary>
		/// Number of 2K during the round
		/// </summary>
		private int _twokillCount;

		/// <summary>
		/// Number of 3K during the round
		/// </summary>
		private int _threekillCount;

		/// <summary>
		/// Number of 4K during the round
		/// </summary>
		private int _fourkillCount;

		/// <summary>
		/// Number of 5K during the round
		/// </summary>
		private int _fivekillCount;

		/// <summary>
		/// Total kill count during the round
		/// </summary>
		private int _killCount;

		/// <summary>
		/// Total jump kill during the round
		/// </summary>
		private int _jumpKillCount;

		/// <summary>
		/// Total crouch kill during the round
		/// </summary>
		private int _crouchKillCount;

		/// <summary>
		/// Total trade kill during the round
		/// </summary>
		private int _tradeKillCount;

		/// <summary>
		/// Total smoke thrown during the round
		/// </summary>
		private int _smokeThrownCount;

		/// <summary>
		/// Total flashbang thrown during the round
		/// </summary>
		private int _flashbangThrownCount;

		/// <summary>
		/// Total decoy thrown during the round
		/// </summary>
		private int _decoyThrownCount;

		/// <summary>
		/// Total molotov thrown during the round
		/// </summary>
		private int _molotovThrownCount;

		/// <summary>
		/// Total incendiary thrown during the round
		/// </summary>
		private int _incendiaryThrownCount;

		/// <summary>
		/// Total HE Grenade thrown during the round
		/// </summary>
		private int _heThrownCount;

		/// <summary>
		/// Total health damage done during the round
		/// </summary>
		private int _damageHealthCount;

		/// <summary>
		/// Total armor damage done during the round
		/// </summary>
		private int _damageArmorCount;

		/// <summary>
		/// Average health damage per player done during the round
		/// </summary>
		private double _averageHealthDamagePerPlayer;

		/// <summary>
		/// Total bomb defused
		/// </summary>
		private int _bombDefusedCount;

		/// <summary>
		/// Total bomb planted
		/// </summary>
		private int _bombPlantedCount;

		/// <summary>
		/// Total bomb exploded
		/// </summary>
		private int _bombExplodedCount;

		/// <summary>
		/// Infos on bomb planted during the round
		/// </summary>
		private BombPlantedEvent _bombPlanted;

		/// <summary>
		/// Infos on bomb defused during the round
		/// </summary>
		private BombDefusedEvent _bombDefused;

		/// <summary>
		/// Infos on bomb exploded during the round
		/// </summary>
		private BombExplodedEvent _bombExploded;

		/// <summary>
		/// Infos on the round's entry kill (first kill as T)
		/// </summary>
		private EntryKillEvent _entryKillEvent;

		/// <summary>
		/// Infos on the round's entry hold (first kill as CT)
		/// </summary>
		private EntryHoldKillEvent _entryHoldKillEvent;

		/// <summary>
		/// Kills done during the round
		/// </summary>
		private ObservableCollection<KillEvent> _kills;

		/// <summary>
		/// Infos about flashbangs exploded events
		/// </summary>
		private ObservableCollection<FlashbangExplodedEvent> _flashbangsExploded;

		/// <summary>
		/// Infos about HE Grenades that exploded during the round
		/// </summary>
		private ObservableCollection<ExplosiveNadeExplodedEvent> _explosiveGrenadesExploded;

		/// <summary>
		/// Infos about smokes poped during the round
		/// </summary>
		private ObservableCollection<SmokeNadeStartedEvent> _smokeStarted;

		/// <summary>
		/// Infos on players damages made during the round
		/// </summary>
		private ObservableCollection<PlayerHurtedEvent> _playerHurted;

		/// <summary>
		/// Infos about shoots done during the round
		/// </summary>
		private ObservableCollection<WeaponFireEvent> _weaponFired;

        /// <summary>
        /// A flag to improve performance by disabling internal calculations (sometimes it is better to do them at the end)
        /// </summary>
        private bool _keepUpdated = false;

        #endregion

        #region Accessors

        [JsonProperty("number")]
		public int Number
		{
			get { return _number; }
			set { Set(ref _number, value); }
		}

		[JsonProperty("tick")]
		public int Tick
		{
			get { return _tick; }
			set { Set(ref _tick, value); }
		}

		[JsonProperty("end_tick")]
		public int EndTick
		{
			get { return _endTick; }
			set { Set(ref _endTick, value); }
		}

		[JsonProperty("end_tick_officially")]
		public int EndTickOfficially
		{
			get { return _endTickOfficially; }
			set { Set(ref _endTickOfficially, value); }
		}

		[JsonProperty("freezetime_end_tick")]
		public int FreezetimeEndTick
		{
			get { return _freezetimeEndTick; }
			set { Set(ref _freezetimeEndTick, value); }
		}

		[JsonProperty("end_reason")]
		[JsonConverter(typeof(EndReasonToStringConverter))]
		public RoundEndReason EndReason
		{
			get { return _endReason; }
			set { Set(ref _endReason, value); }
		}

		[JsonProperty("kills", IsReference = false)]
		public ObservableCollection<KillEvent> Kills
		{
			get { return _kills; }
			set { Set(ref _kills, value); }
		}

		[JsonProperty("winner_side")]
		[JsonConverter(typeof(SideToStringConverter))]
		public Side WinnerSide
		{
			get { return _winnerSide; }
			set { Set(ref _winnerSide, value); }
		}

		[JsonProperty("winner_name")]
		public string WinnerName
		{
			get { return _winnerName; }
			set { Set(ref _winnerName, value); }
		}

		[JsonProperty("team_t_name")]
		public string TeamTname
		{
			get { return _teamT; }
			set { Set(ref _teamT, value); }
		}

		[JsonProperty("team_ct_name")]
		public string TeamCtName
		{
			get { return _teamCt; }
			set { Set(ref _teamCt, value); }
		}

		[JsonProperty("kill_count")]
		public int KillCount
		{
			get { return _killCount; }
			set { Set(ref _killCount, value); }
		}

		[JsonProperty("5k_count")]
		public int FiveKillCount
		{
			get { return _fivekillCount; }
			set { Set(ref _fivekillCount, value); }
		}

		[JsonProperty("4k_count")]
		public int FourKillCount
		{
			get { return _fourkillCount; }
			set { Set(ref _fourkillCount, value); }
		}

		[JsonProperty("3k_count")]
		public int ThreeKillCount
		{
			get { return _threekillCount; }
			set { Set(ref _threekillCount, value); }
		}

		[JsonProperty("2k_count")]
		public int TwoKillCount
		{
			get { return _twokillCount; }
			set { Set(ref _twokillCount, value); }
		}

		[JsonProperty("1k_count")]
		public int OneKillCount
		{
			get { return _onekillCount; }
			set { Set(ref _onekillCount, value); }
		}

		[JsonProperty("jump_kill_count")]
		public int JumpKillCount
		{
			get { return _jumpKillCount; }
			set { Set(ref _jumpKillCount, value); }
		}

		[JsonProperty("crouch_kill_count")]
		public int CrouchKillCount
		{
			get { return _crouchKillCount; }
			set { Set(ref _crouchKillCount, value); }
		}

		[JsonProperty("trade_kill_count")]
		public int TradeKillCount
		{
			get { return _tradeKillCount; }
			set { Set(ref _tradeKillCount, value); }
		}

		[JsonProperty("equipement_value_team_t")]
		public int EquipementValueTeamT
		{
			get { return _equipementValueTeamT; }
			set { Set(ref _equipementValueTeamT, value); }
		}

		[JsonProperty("equipement_value_team_ct")]
		public int EquipementValueTeamCt
		{
			get { return _equipementValueTeamCt; }
			set { Set(ref _equipementValueTeamCt, value); }
		}

		[JsonProperty("start_money_team_t")]
		public int StartMoneyTeamT
		{
			get { return _startMoneyTeamT; }
			set { Set(ref _startMoneyTeamT, value); }
		}

		[JsonProperty("start_money_team_ct")]
		public int StartMoneyTeamCt
		{
			get { return _startMoneyTeamCt; }
			set { Set(ref _startMoneyTeamCt, value); }
		}

		[JsonProperty("bomb_defused_count")]
		public int BombDefusedCount
		{
			get { return _bombDefusedCount; }
			set { Set(ref _bombDefusedCount, value); }
		}

		[JsonProperty("bomb_exploded_count")]
		public int BombExplodedCount
		{
			get { return _bombExplodedCount; }
			set { Set(ref _bombExplodedCount, value); }
		}

		[JsonProperty("bomb_planted_count")]
		public int BombPlantedCount
		{
			get { return _bombPlantedCount; }
			set { Set(ref _bombPlantedCount, value); }
		}

		[JsonProperty("entry_kill")]
		public EntryKillEvent EntryKillEvent
		{
			get { return _entryKillEvent; }
			set { Set(ref _entryKillEvent, value); }
		}

		[JsonProperty("entry_hold_kill")]
		public EntryHoldKillEvent EntryHoldKillEvent
		{
			get { return _entryHoldKillEvent; }
			set { Set(ref _entryHoldKillEvent, value); }
		}

		[JsonProperty("bomb_planted")]
		public BombPlantedEvent BombPlanted
		{
			get { return _bombPlanted; }
			set { Set(ref _bombPlanted, value); }
		}

		[JsonProperty("bomb_defused")]
		public BombDefusedEvent BombDefused
		{
			get { return _bombDefused; }
			set { Set(ref _bombDefused, value); }
		}

		[JsonProperty("bomb_exploded")]
		public BombExplodedEvent BombExploded
		{
			get { return _bombExploded; }
			set { Set(ref _bombExploded, value); }
		}

		[JsonProperty("type")]
		public RoundType Type
		{
			get { return _type; }
			set { Set(ref _type, value); }
		}

		[JsonProperty("team_trouble_name")]
		public string TeamTroubleName
		{
			get { return _teamTroubleName; }
			set { Set(ref _teamTroubleName, value); }
		}

		[JsonProperty("side_trouble")]
		[JsonConverter(typeof(SideToStringConverter))]
		public Side SideTrouble
		{
			get { return _sideTrouble; }
			set { Set(ref _sideTrouble, value); }
		}

		[JsonProperty("flashbang_thrown_count")]
		public int FlashbangThrownCount
		{
			get { return _flashbangThrownCount; }
			set { Set(ref _flashbangThrownCount, value); }
		}

		[JsonProperty("smoke_thrown_count")]
		public int SmokeThrownCount
		{
			get { return _smokeThrownCount; }
			set { Set(ref _smokeThrownCount, value); }
		}

		[JsonProperty("he_thrown_count")]
		public int HeGrenadeThrownCount
		{
			get { return _heThrownCount; }
			set { Set(ref _heThrownCount, value); }
		}

		[JsonProperty("decoy_thrown_count")]
		public int DecoyThrownCount
		{
			get { return _decoyThrownCount; }
			set { Set(ref _decoyThrownCount, value); }
		}

		[JsonProperty("molotov_thrown_count")]
		public int MolotovThrownCount
		{
			get { return _molotovThrownCount; }
			set { Set(ref _molotovThrownCount, value); }
		}

		[JsonProperty("incendiary_thrown_count")]
		public int IncendiaryThrownCount
		{
			get { return _incendiaryThrownCount; }
			set { Set(ref _incendiaryThrownCount, value); }
		}

		[JsonProperty("players_hurted", IsReference = false)]
		public ObservableCollection<PlayerHurtedEvent> PlayersHurted
		{
			get { return _playerHurted; }
			set { Set(ref _playerHurted, value); }
		}

		[JsonProperty("weapon_fired", IsReference = false)]
		public ObservableCollection<WeaponFireEvent> WeaponFired
		{
			get { return _weaponFired; }
			set { Set(ref _weaponFired, value); }
		}

		[JsonProperty("duration")]
		public float Duration
		{
			get { return _duration; }
			set { Set(ref _duration, value); }
		}

		[JsonProperty("damage_health_count")]
		public int DamageHealthCount
		{
			get { return _damageHealthCount; }
			set { Set(ref _damageHealthCount, value); }
		}

		[JsonProperty("damage_armor_count")]
		public int DamageArmorCount
		{
			get { return _damageArmorCount; }
			set { Set(ref _damageArmorCount, value); }
		}

		[JsonProperty("average_health_damage_per_player")]
		public double AverageHealthDamagePerPlayer
		{
			get { return _averageHealthDamagePerPlayer; }
			set { Set(ref _averageHealthDamagePerPlayer, value); }
		}

		[JsonProperty("flashbangs_exploded")]
		public ObservableCollection<FlashbangExplodedEvent> FlashbangsExploded
		{
			get { return _flashbangsExploded; }
			set { Set(ref _flashbangsExploded, value); }
		}

		[JsonProperty("smokes_started")]
		public ObservableCollection<SmokeNadeStartedEvent> SmokeStarted
		{
			get { return _smokeStarted; }
			set { Set(ref _smokeStarted, value); }
		}

		[JsonProperty("he_exploded")]
		public ObservableCollection<ExplosiveNadeExplodedEvent> ExplosiveGrenadesExploded
		{
			get { return _explosiveGrenadesExploded; }
			set { Set(ref _explosiveGrenadesExploded, value); }
		}

		#endregion

		public Round()
		{
			PlayersHurted = new ObservableCollection<PlayerHurtedEvent>();
			WeaponFired = new ObservableCollection<WeaponFireEvent>();
			ExplosiveGrenadesExploded = new ObservableCollection<ExplosiveNadeExplodedEvent>();
			SmokeStarted = new ObservableCollection<SmokeNadeStartedEvent>();
			FlashbangsExploded = new ObservableCollection<FlashbangExplodedEvent>();
			Kills = new ObservableCollection<KillEvent>();

        }
        public void EnableUpdates()
        {
            if (_keepUpdated)
            {
                return;
            }

            Kills.CollectionChanged += OnKillsCollectionChanged;
            PlayersHurted.CollectionChanged += OnPlayersHurtedCollectionChanged;
            WeaponFired.CollectionChanged += OnWeaponFiredCollectionChanged;
            _keepUpdated = true;
            OnWeaponFiredCollectionChanged(null, null);
            OnPlayersHurtedCollectionChanged(null, null);
            OnWeaponFiredCollectionChanged(null, null);
        }

        public override bool Equals(object obj)
		{
			var item = obj as Round;

			return item != null && Number.Equals(item.Number);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public Round Clone()
		{
			Round round = new Round
			{
				Number = Number,
				DecoyThrownCount = DecoyThrownCount,
				BombDefusedCount = BombDefusedCount,
				SmokeThrownCount = SmokeThrownCount,
				TradeKillCount = TradeKillCount,
				KillCount = KillCount,
				BombExplodedCount = BombExplodedCount,
				AverageHealthDamagePerPlayer = AverageHealthDamagePerPlayer,
				BombDefused = BombDefused,
				BombPlantedCount = BombPlantedCount,
				BombExploded = BombExploded,
				BombPlanted = BombPlanted,
				SideTrouble = SideTrouble,
				CrouchKillCount = CrouchKillCount,
				DamageArmorCount = DamageArmorCount,
				DamageHealthCount = DamageHealthCount,
				EndReason = EndReason,
				EndTick = EndTick,
				Duration = Duration,
				FreezetimeEndTick = FreezetimeEndTick,
				EndTickOfficially = EndTickOfficially,
				EntryHoldKillEvent = EntryHoldKillEvent,
				EntryKillEvent = EntryKillEvent,
				EquipementValueTeamCt = EquipementValueTeamCt,
				EquipementValueTeamT = EquipementValueTeamT,
				FiveKillCount = FiveKillCount,
				FourKillCount = FourKillCount,
				ThreeKillCount = ThreeKillCount,
				TwoKillCount = TwoKillCount,
				OneKillCount = OneKillCount,
				FlashbangThrownCount = FlashbangThrownCount,
				HeGrenadeThrownCount = HeGrenadeThrownCount,
				IncendiaryThrownCount = IncendiaryThrownCount,
				JumpKillCount = JumpKillCount,
				MolotovThrownCount = MolotovThrownCount,
				StartMoneyTeamCt = StartMoneyTeamCt,
				StartMoneyTeamT = StartMoneyTeamT,
				TeamCtName = TeamCtName,
				TeamTname = TeamTname,
				TeamTroubleName = TeamTroubleName,
				Tick = Tick,
				Type = Type,
				WinnerName = WinnerName,
				WinnerSide = WinnerSide,
				PlayersHurted = new ObservableCollection<PlayerHurtedEvent>(),
				Kills = new ObservableCollection<KillEvent>(),
				WeaponFired = new ObservableCollection<WeaponFireEvent>(),
				ExplosiveGrenadesExploded = new ObservableCollection<ExplosiveNadeExplodedEvent>(),
				FlashbangsExploded = new ObservableCollection<FlashbangExplodedEvent>(),
				SmokeStarted = new ObservableCollection<SmokeNadeStartedEvent>(),
			};

			foreach (PlayerHurtedEvent e in PlayersHurted)
				round.PlayersHurted.Add(e);
			foreach (ExplosiveNadeExplodedEvent e in ExplosiveGrenadesExploded)
				round.ExplosiveGrenadesExploded.Add(e);
			foreach (FlashbangExplodedEvent e in FlashbangsExploded)
				round.FlashbangsExploded.Add(e);
			foreach (SmokeNadeStartedEvent e in SmokeStarted)
				round.SmokeStarted.Add(e);
			foreach (KillEvent k in Kills)
				round.Kills.Add(k);
			foreach (WeaponFireEvent e in WeaponFired)
				round.WeaponFired.Add(e);

            round.EnableUpdates();
			return round;
		}

		public void Reset(int tick)
		{
			AverageHealthDamagePerPlayer = 0;
			BombDefused = null;
			BombDefusedCount = 0;
			BombExploded = null;
			BombExplodedCount = 0;
			BombPlanted = null;
			BombPlantedCount = 0;
			CrouchKillCount = 0;
			DamageArmorCount = 0;
			DamageHealthCount = 0;
			DecoyThrownCount = 0;
			EntryHoldKillEvent = null;
			EntryKillEvent = null;
			EquipementValueTeamCt = 0;
			EquipementValueTeamT = 0;
			FiveKillCount = 0;
			FourKillCount = 0;
			IncendiaryThrownCount = 0;
			JumpKillCount = 0;
			KillCount = 0;
			MolotovThrownCount = 0;
			OneKillCount = 0;
			SideTrouble = Side.None;
			SmokeThrownCount = 0;
			TeamTroubleName = string.Empty;
			ThreeKillCount = 0;
			TradeKillCount = 0;
			Tick = tick;
			Type = RoundType.NORMAL;
			TwoKillCount = 0;
			WinnerName = string.Empty;
			WinnerSide = Side.None;

			ExplosiveGrenadesExploded.Clear();
			FlashbangsExploded.Clear();
			Kills.Clear();
			PlayersHurted.Clear();
			SmokeStarted.Clear();
			WeaponFired.Clear();
		}

		#region Handler collections changed

		private void OnKillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			KillCount = Kills.Count;
			JumpKillCount = Kills.Count(killEvent => killEvent.KillerVelocityZ > 0);
			CrouchKillCount = Kills.Count(killEvent => killEvent.IsKillerCrouching);
			TradeKillCount = Kills.Count(k => k.IsTradeKill);
			
			var kills = Kills.Where(k => k.KillerSide != k.KilledSide && k.KillerSteamId != 0)
				.GroupBy(k => k.KillerSteamId)
				.Select(group => new
				{
					SteamId = group.Key,
					KillCount = group.Count()
				}).ToList();
			OneKillCount = kills.Count(k => k.KillCount == 1);
			TwoKillCount = kills.Count(k => k.KillCount == 2);
			ThreeKillCount = kills.Count(k => k.KillCount == 3);
			FourKillCount = kills.Count(k => k.KillCount == 4);
			FiveKillCount = kills.Count(k => k.KillCount == 5);
		}

		private void OnPlayersHurtedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DamageHealthCount = PlayersHurted.Where(playerHurtedEvent => playerHurtedEvent.AttackerSteamId != 0)
				.Sum(playerHurtedEvent => playerHurtedEvent.HealthDamage);
			DamageArmorCount = PlayersHurted.Where(playerHurtedEvent => playerHurtedEvent.AttackerSteamId != 0)
				.Sum(playerHurtedEvent => playerHurtedEvent.ArmorDamage);
			AverageHealthDamagePerPlayer = GetAverageHealthDamagePerPlayer();
		}

		private void OnWeaponFiredCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IncendiaryThrownCount = WeaponFired.Count(w => w.Weapon.Element == EquipmentElement.Incendiary);
			MolotovThrownCount = WeaponFired.Count(w => w.Weapon.Element == EquipmentElement.Molotov);
			DecoyThrownCount = WeaponFired.Count(w => w.Weapon.Element == EquipmentElement.Decoy);
			HeGrenadeThrownCount = WeaponFired.Count(w => w.Weapon.Element == EquipmentElement.HE);
			SmokeThrownCount = WeaponFired.Count(w => w.Weapon.Element == EquipmentElement.Smoke);
			FlashbangThrownCount = WeaponFired.Count(w => w.Weapon.Element == EquipmentElement.Flash);
		}

		#endregion

		private double GetAverageHealthDamagePerPlayer()
		{
			double total = PlayersHurted.Aggregate<PlayerHurtedEvent, double>(0, (current, playerHurtedEvent) =>
					current + playerHurtedEvent.HealthDamage);
			if (Math.Abs(total) < 0.1) return total;
			return Math.Round(total / 10, 1);
		}
	}
}
