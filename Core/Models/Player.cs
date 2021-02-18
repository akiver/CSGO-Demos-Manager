using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Core.Models.Events;
using Core.Models.Serialization;

namespace Core.Models
{
	/// <summary>
	/// Represent a player
	/// </summary>
	public class Player : ObservableObject
	{
		#region Properties

		/// <summary>
		/// Number of 1 Kill that the player made during the match
		/// </summary>
		private int _onekillCount;

		/// <summary>
		/// Number of 2 kills that the player made during the match
		/// </summary>
		private int _twokillCount;

		/// <summary>
		/// Number of 3 kills that the player made during the match
		/// </summary>
		private int _threekillCount;

		/// <summary>
		/// Number of 4 kills that the player made during the match
		/// </summary>
		private int _fourKillCount;

		/// <summary>
		/// Number of 5 kills that the player made during the match
		/// </summary>
		private int _fiveKillCount;

		/// <summary>
		/// Number of heashots that the player made during the match
		/// </summary>
		private int _headshotCount;

		/// <summary>
		/// Number of assists that the player made during the match
		/// </summary>
		private int _assistCount;

		/// <summary>
		/// Number of deaths that the player had during the match
		/// </summary>
		private int _deathCount;

		/// <summary>
		/// Total number of kills that the player made during the match
		/// </summary>
		private int _killCount;

		/// <summary>
		/// Number of kills that the player made while crouching
		/// </summary>
		private int _crouchKillCount;

		/// <summary>
		/// Number of kills that the player made when is wasn't on the floor
		/// </summary>
		private int _jumpKillCount;

		/// <summary>
		/// Number of teamkill that the player made during the match
		/// </summary>
		private int _teamKillCount;

		/// <summary>
		/// Number MVP that the player had during the match
		/// </summary>
		private int _roundMvpCount;

		/// <summary>
		/// Number of bomb which the player had planted during the match
		/// </summary>
		private int _bombPlantedCount;

		/// <summary>
		/// Number of bomb which the player had defused during the match
		/// </summary>
		private int _bombDefusedCount;

		/// <summary>
		/// Number of bomb exploded which was planted by the player
		/// </summary>
		private int _bombExplodedCount;

		/// <summary>
		/// Score of the player (calculated by the game)
		/// </summary>
		private int _score;

		/// <summary>
		/// Indicate if the player is controlling a BOT
		/// </summary>
		private bool _isControllingBot;

		/// <summary>
		/// Indicate if the player is alive
		/// </summary>
		private bool _isAlive = true;

		/// <summary>
		/// Flag to detect disconnect / reconnect
		/// </summary>
		private bool _isConnected = true;

		/// <summary>
		/// Player kills / Deaths ratio
		/// </summary>
		private decimal _killsDeathsRatio;

		/// <summary>
		/// Player's current team side (change when half side is over)
		/// </summary>
		private Side _side;

		/// <summary>
		/// Player's team name
		/// </summary>
		private string _teamName;

		/// <summary>
		/// Player's SteamID 64
		/// </summary>
		private long _steamId;

		/// <summary>
		/// Player's nickname used when he was playing
		/// </summary>
		private string _name = "";

		/// <summary>
		/// Flag to know if the player had an entry kill during a round
		/// </summary>
		private bool _hasEntryKill;

		/// <summary>
		/// Flag to know if the player had an entry hold kill during a round
		/// </summary>
		private bool _hasEntryHoldKill;

		/// <summary>
		/// Rating based on hltv.org formula that the player made during the match
		/// </summary>
		private float _ratingHltv;

		/// <summary>
		/// Rating based on hltv.org formula that the player made during the match
		/// </summary>
		private float _ratingHltv2;

		/// <summary>
		/// ESEA points use to calculate RWS
		/// </summary>
		private decimal _eseaRwsPointCount = 0;

		/// <summary>
		/// ESEA RWS metric
		/// </summary>
		private decimal _eseaRws;

		/// <summary>
		/// How many shots the player fired
		/// </summary>
		private int _shotCount;

		/// <summary>
		/// How many hits the player had
		/// </summary>
		private int _hitCount;

		/// <summary>
		/// Indicates if the player has the bomb, used for the overview animation
		/// </summary>
		private bool _hasBomb = false;

		/// <summary>
		/// Indicates if the player has been VAC banned
		/// </summary>
		private bool _isVacBanned;

		/// <summary>
		/// Indicates if the player has been overwatch banned
		/// </summary>
		private bool _isOverwatchBanned;

		/// <summary>
		/// Rank level at the start of the match
		/// </summary>
		private int _rankNumberOld = 0;

		/// <summary>
		/// Rank level at the end of the match
		/// </summary>
		private int _rankNumberNew = 0;

		/// <summary>
		/// Number MM won (from protobuf message)
		/// </summary>
		private int _winCount = 0;

		/// <summary>
		/// Total trade kill
		/// </summary>
		private int _tradeKillCount;

		/// <summary>
		/// Total trade death
		/// </summary>
		private int _tradeDeathCount;

		/// <summary>
		/// Number of flashbang thrown by the player
		/// </summary>
		private int _flashbangThrownCount;

		/// <summary>
		/// Number of smoke thrown by the player
		/// </summary>
		private int _smokeThrownCount;

		/// <summary>
		/// Number of HE Grenade thrown by the player
		/// </summary>
		private int _heGrenadeThrowCount;

		/// <summary>
		/// Number of molotov thrown by the player
		/// </summary>
		private int _molotovThrownCount;

		/// <summary>
		/// Number of incendiary thrown by the player
		/// </summary>
		private int _incendiaryThrowCount;

		/// <summary>
		/// Number of decoy thrown by the player
		/// </summary>
		private int _decoyThrownCount;

		/// <summary>
		/// Number of round that the player played
		/// </summary>
		private int _roundPlayedCount = 0;

		/// <summary>
		/// Avatar URL of the player (get from Steam API)
		/// </summary>
		private string _avatarUrl = AppSettings.CORE_URI + "/resources/images/avatar.jpg";

		/// <summary>
		/// Temp variable to calculate flash duration
		/// </summary>
		private float _flashDurationTemp = 0;

		/// <summary>
		/// Start money the player had at each rounds
		/// </summary>
		private Dictionary<int, int> _startMoneyRounds = new Dictionary<int, int>();

		/// <summary>
		/// Equipement value the player at each round
		/// </summary>
		private Dictionary<int, int> _equipementValueRounds = new Dictionary<int, int>();

		/// <summary>
		/// Money earned at each round
		/// </summary>
		private Dictionary<int, int> _roundsMoneyEarned = new Dictionary<int, int>();

		/// <summary>
		/// Player's time of death for each round in seconds.
		/// </summary>
		private Dictionary<int, float> _timeDeathRounds = new Dictionary<int, float>();

		/// <summary>
		/// Player's kills data
		/// </summary>
		private ObservableCollection<KillEvent> _kills;

		/// <summary>
		/// Player's deaths data
		/// </summary>
		private ObservableCollection<KillEvent> _deaths;

		/// <summary>
		/// Player's assists data
		/// </summary>
		private ObservableCollection<KillEvent> _assists;

		/// <summary>
		/// Player's entry kills data
		/// </summary>
		private ObservableCollection<EntryKillEvent> _entryKills;

		/// <summary>
		/// Player's entry hold kills data
		/// </summary>
		private ObservableCollection<EntryHoldKillEvent> _entryHoldKills;

		/// <summary>
		/// List of PlayerHurtedEvent in which the player is involved
		/// </summary>
		private ObservableCollection<PlayerHurtedEvent> _playersHurted;

		/// <summary>
		/// List of clutches that the player is concerned
		/// </summary>
		private ObservableCollection<ClutchEvent> _clutches;

		#endregion

		#region Accessors

		[JsonProperty("steamid")]
		[JsonConverter(typeof(LongToStringConverter))]
		public long SteamId
		{
			get { return _steamId; }
			set { Set(() => SteamId, ref _steamId, value); }
		}

		[JsonProperty("name")]
		public string Name
		{
			get { return _name; }
			set { Set(() => Name, ref _name, value); }
		}

		[JsonProperty("kill_count")]
		public int KillCount
		{
			get { return _killCount; }
			set
			{
				RaisePropertyChanged(() => KillDeathRatio);
				Set(() => KillCount, ref _killCount, value);
			}
		}

		[JsonProperty("crouch_kill_count")]
		public int CrouchKillCount
		{
			get { return _crouchKillCount; }
			set { Set(() => CrouchKillCount, ref _crouchKillCount, value); }
		}

		[JsonProperty("jump_kill_count")]
		public int JumpKillCount
		{
			get { return _jumpKillCount; }
			set { Set(() => JumpKillCount, ref _jumpKillCount, value); }
		}

		[JsonProperty("score")]
		public int Score
		{
			get { return _score; }
			set { Set(() => Score, ref _score, value); }
		}

		[JsonProperty("tk_count")]
		public int TeamKillCount
		{
			get { return _teamKillCount; }
			set { Set(() => TeamKillCount, ref _teamKillCount, value); }
		}

		[JsonProperty("assist_count")]
		public int AssistCount
		{
			get { return _assistCount; }
			set
			{
				Set(() => AssistCount, ref _assistCount, value);
				RaisePropertyChanged(() => AssistPerRound);
			}
		}

		[JsonProperty("trade_kill_count")]
		public int TradeKillCount
		{
			get { return _tradeKillCount; }
			set { Set(() => TradeKillCount, ref _tradeKillCount, value); }
		}

		[JsonProperty("trade_death_count")]
		public int TradeDeathCount
		{
			get { return _tradeDeathCount; }
			set { Set(() => TradeDeathCount, ref _tradeDeathCount, value); }
		}

		[JsonProperty("bomb_planted_count")]
		public int BombPlantedCount
		{
			get { return _bombPlantedCount; }
			set { Set(() => BombPlantedCount, ref _bombPlantedCount, value); }
		}

		[JsonProperty("bomb_defused_count")]
		public int BombDefusedCount
		{
			get { return _bombDefusedCount; }
			set { Set(() => BombDefusedCount, ref _bombDefusedCount, value); }
		}

		[JsonProperty("bomb_exploded_count")]
		public int BombExplodedCount
		{
			get { return _bombExplodedCount; }
			set { Set(() => BombExplodedCount, ref _bombExplodedCount, value); }
		}

		[JsonProperty("death_count")]
		public int DeathCount
		{
			get { return _deathCount; }
			set
			{
				RaisePropertyChanged(() => KillDeathRatio);
				Set(() => DeathCount, ref _deathCount, value);
			}
		}

		[JsonProperty("5k_count")]
		public int FiveKillCount
		{
			get { return _fiveKillCount; }
			set { Set(() => FiveKillCount, ref _fiveKillCount, value); }
		}

		[JsonProperty("4k_count")]
		public int FourKillCount
		{
			get { return _fourKillCount; }
			set { Set(() => FourKillCount, ref _fourKillCount, value); }
		}

		[JsonProperty("3k_count")]
		public int ThreeKillCount
		{
			get { return _threekillCount; }
			set { Set(() => ThreeKillCount, ref _threekillCount, value); }
		}

		[JsonProperty("2k_count")]
		public int TwoKillCount
		{
			get { return _twokillCount; }
			set { Set(() => TwoKillCount, ref _twokillCount, value); }
		}

		[JsonProperty("1k_count")]
		public int OneKillCount
		{
			get { return _onekillCount; }
			set { Set(() => OneKillCount, ref _onekillCount, value); }
		}

		[JsonProperty("hs_count")]
		public int HeadshotCount
		{
			get { return _headshotCount; }
			set
			{
				Set(() => HeadshotCount, ref _headshotCount, value);
				RaisePropertyChanged(() => HeadshotPercent);
			}
		}

		[JsonProperty("kd")]
		public decimal KillDeathRatio
		{
			get
			{
				if (_killCount != 0 && _deathCount != 0) return Math.Round(_killCount / (decimal)_deathCount, 2);
				return 0;
			}
			set { Set(() => KillDeathRatio, ref _killsDeathsRatio, value); }
		}

		[JsonProperty("mvp_count")]
		public int RoundMvpCount
		{
			get { return _roundMvpCount; }
			set { Set(() => RoundMvpCount, ref _roundMvpCount, value); }
		}

		[JsonProperty("hltv_rating")]
		public float RatingHltv
		{
			get { return _ratingHltv; }
			set { Set(() => RatingHltv, ref _ratingHltv, value); }
		}

		[JsonProperty("hltv2_rating")]
		public float RatingHltv2
		{
			get { return _ratingHltv2; }
			set { Set(() => RatingHltv2, ref _ratingHltv2, value); }
		}

		[JsonIgnore]
		public decimal EseaRwsPointCount
		{
			get { return _eseaRwsPointCount; }
			set { Set(() => EseaRwsPointCount, ref _eseaRwsPointCount, value); }
		}

		[JsonProperty("esea_rws")]
		public decimal EseaRws
		{
			get { return _eseaRws; }
			set { Set(() => EseaRws, ref _eseaRws, value); }
		}

		[JsonProperty("shot_count")]
		public int ShotCount
		{
			get { return _shotCount; }
			set
			{
				Set(() => ShotCount, ref _shotCount, value);
				RaisePropertyChanged(() => Accuracy);
			}
		}

		[JsonProperty("hit_count")]
		public int HitCount
		{
			get { return _hitCount; }
			set
			{
				Set(() => HitCount, ref _hitCount, value);
				RaisePropertyChanged(() => Accuracy);
			}
		}

		[JsonIgnore]
		public double Accuracy => ShotCount == 0 ? 0 : Math.Round(HitCount / (double)ShotCount * 100, 2);

		[JsonProperty("is_vac_banned")]
		public bool IsVacBanned
		{
			get { return _isVacBanned; }
			set { Set(() => IsVacBanned, ref _isVacBanned, value); }
		}

		[JsonProperty("is_ow_banned")]
		public bool IsOverwatchBanned
		{
			get { return _isOverwatchBanned; }
			set { Set(() => IsOverwatchBanned, ref _isOverwatchBanned, value); }
		}

		[JsonProperty("flashbang_count")]
		public int FlashbangThrownCount
		{
			get { return _flashbangThrownCount; }
			set { Set(() => FlashbangThrownCount, ref _flashbangThrownCount, value); }
		}

		[JsonProperty("smoke_count")]
		public int SmokeThrownCount
		{
			get { return _smokeThrownCount; }
			set { Set(() => SmokeThrownCount, ref _smokeThrownCount, value); }
		}

		[JsonProperty("he_count")]
		public int HeGrenadeThrownCount
		{
			get { return _heGrenadeThrowCount; }
			set { Set(() => HeGrenadeThrownCount, ref _heGrenadeThrowCount, value); }
		}

		[JsonProperty("molotov_count")]
		public int MolotovThrownCount
		{
			get { return _molotovThrownCount; }
			set { Set(() => MolotovThrownCount, ref _molotovThrownCount, value); }
		}

		[JsonProperty("incendiary_count")]
		public int IncendiaryThrownCount
		{
			get { return _incendiaryThrowCount; }
			set { Set(() => IncendiaryThrownCount, ref _incendiaryThrowCount, value); }
		}

		[JsonProperty("decoy_count")]
		public int DecoyThrownCount
		{
			get { return _decoyThrownCount; }
			set { Set(() => DecoyThrownCount, ref _decoyThrownCount, value); }
		}

		[JsonProperty("round_count")]
		public int RoundPlayedCount
		{
			get { return _roundPlayedCount; }
			set { Set(() => RoundPlayedCount, ref _roundPlayedCount, value); }
		}

		[JsonProperty("team_name")]
		public string TeamName
		{
			get { return _teamName; }
			set { Set(() => TeamName, ref _teamName, value); }
		}

		[JsonProperty("start_money_rounds")]
		public Dictionary<int, int> StartMoneyRounds
		{
			get { return _startMoneyRounds; }
			set { Set(() => StartMoneyRounds, ref _startMoneyRounds, value); }
		}

		[JsonProperty("equipement_value_rounds")]
		public Dictionary<int, int> EquipementValueRounds
		{
			get { return _equipementValueRounds; }
			set { Set(() => EquipementValueRounds, ref _equipementValueRounds, value); }
		}

		[JsonProperty("rounds_money_earned")]
		public Dictionary<int, int> RoundsMoneyEarned
		{
			get { return _roundsMoneyEarned; }
			set { Set(() => RoundsMoneyEarned, ref _roundsMoneyEarned, value); }
		}

		[JsonProperty("time_death_rounds")]
		public Dictionary<int, float> TimeDeathRounds
		{
			get { return _timeDeathRounds; }
			set { Set(() => TimeDeathRounds, ref _timeDeathRounds, value); }
		}

		[JsonProperty("entry_kills")]
		public ObservableCollection<EntryKillEvent> EntryKills
		{
			get { return _entryKills; }
			set { Set(() => EntryKills, ref _entryKills, value); }
		}

		[JsonProperty("entry_hold_kills")]
		public ObservableCollection<EntryHoldKillEvent> EntryHoldKills
		{
			get { return _entryHoldKills; }
			set { Set(() => EntryHoldKills, ref _entryHoldKills, value); }
		}

		[JsonProperty("kills", IsReference = false)]
		public ObservableCollection<KillEvent> Kills
		{
			get { return _kills; }
			set { Set(() => Kills, ref _kills, value); }
		}

		[JsonProperty("deaths", IsReference = false)]
		public ObservableCollection<KillEvent> Deaths
		{
			get { return _deaths; }
			set { Set(() => Deaths, ref _deaths, value); }
		}

		[JsonProperty("assits", IsReference = false)]
		public ObservableCollection<KillEvent> Assists
		{
			get { return _assists; }
			set { Set(() => Assists, ref _assists, value); }
		}

		[JsonProperty("players_hurted")]
		public ObservableCollection<PlayerHurtedEvent> PlayersHurted
		{
			get { return _playersHurted; }
			set { Set(() => PlayersHurted, ref _playersHurted, value); }
		}

		[JsonProperty("clutches")]
		public ObservableCollection<ClutchEvent> Clutches
		{
			get { return _clutches; }
			set { Set(() => Clutches, ref _clutches, value); }
		}

		[JsonProperty("rank_old")]
		public int RankNumberOld
		{
			get { return _rankNumberOld; }
			set { Set(() => RankNumberOld, ref _rankNumberOld, value); }
		}

		[JsonProperty("rank_new")]
		public int RankNumberNew
		{
			get { return _rankNumberNew; }
			set { Set(() => RankNumberNew, ref _rankNumberNew, value); }
		}

		[JsonProperty("win_count")]
		public int WinCount
		{
			get { return _winCount; }
			set { Set(() => WinCount, ref _winCount, value); }
		}

		[JsonProperty("entry_kill_won_count")]
		public int EntryKillWonCount
		{
			get { return EntryKills.Count(e => e.HasWon); }
		}

		[JsonProperty("entry_kill_loss_count")]
		public int EntryKillLossCount
		{
			get { return EntryKills.Count(e => e.HasWon == false); }
		}

		[JsonProperty("entry_hold_kill_won_count")]
		public int EntryHoldKillWonCount
		{
			get { return EntryHoldKills.Count(e => e.HasWon); }
		}

		[JsonProperty("entry_hold_kill_loss_count")]
		public int EntryHoldKillLossCount
		{
			get { return EntryHoldKills.Count(e => e.HasWon == false); }
		}

		[JsonIgnore]
		public string AvatarUrl
		{
			get { return _avatarUrl; }
			set { Set(() => AvatarUrl, ref _avatarUrl, value); }
		}

		[JsonProperty("clutch_count")]
		public int ClutchCount => Clutches.Count;

		[JsonProperty("clutch_loss_count")]
		public int ClutchLostCount => Clutches.Count(c => !c.HasWon);

		[JsonProperty("clutch_won_count")]
		public int ClutchWonCount => Clutches.Count(c => c.HasWon);

		[JsonProperty("1v1_won_count")]
		public int Clutch1V1WonCount => Clutches.Count(c => c.OpponentCount == 1 && c.HasWon);

		[JsonProperty("1v2_won_count")]
		public int Clutch1V2WonCount => Clutches.Count(c => c.OpponentCount == 2 && c.HasWon);

		[JsonProperty("1v3_won_count")]
		public int Clutch1V3WonCount => Clutches.Count(c => c.OpponentCount == 3 && c.HasWon);

		[JsonProperty("1v4_won_count")]
		public int Clutch1V4WonCount => Clutches.Count(c => c.OpponentCount == 4 && c.HasWon);

		[JsonProperty("1v5_won_count")]
		public int Clutch1V5WonCount => Clutches.Count(c => c.OpponentCount == 5 && c.HasWon);

		[JsonProperty("1v1_loss_count")]
		public int Clutch1V1LossCount => Clutches.Count(c => c.OpponentCount == 1 && !c.HasWon);

		[JsonProperty("1v2_loss_count")]
		public int Clutch1V2LossCount => Clutches.Count(c => c.OpponentCount == 2 && !c.HasWon);

		[JsonProperty("1v3_loss_count")]
		public int Clutch1V3LossCount => Clutches.Count(c => c.OpponentCount == 3 && !c.HasWon);

		[JsonProperty("1v4_loss_count")]
		public int Clutch1V4LossCount => Clutches.Count(c => c.OpponentCount == 4 && !c.HasWon);

		[JsonProperty("1v5_loss_count")]
		public int Clutch1V5LossCount => Clutches.Count(c => c.OpponentCount == 5 && !c.HasWon);

		[JsonProperty("1v1_count")]
		public int Clutch1V1Count => Clutches.Count(c => c.OpponentCount == 1);

		[JsonProperty("1v2_count")]
		public int Clutch1V2Count => Clutches.Count(c => c.OpponentCount == 2);

		[JsonProperty("1v3_count")]
		public int Clutch1V3Count => Clutches.Count(c => c.OpponentCount == 3);

		[JsonProperty("1v4_count")]
		public int Clutch1V4Count => Clutches.Count(c => c.OpponentCount == 4);

		[JsonProperty("1v5_count")]
		public int Clutch1V5Count => Clutches.Count(c => c.OpponentCount == 5);

		[JsonIgnore]
		public decimal ClutchWonPercent => ClutchCount == 0 ? 0 : Math.Round((decimal)(ClutchWonCount * 100) / ClutchCount, 2);

		[JsonIgnore]
		public decimal Clutch1V1WonPercent => Clutch1V1Count == 0 ? 0 : Math.Round((decimal)(Clutch1V1WonCount * 100) / Clutch1V1Count, 2);

		[JsonIgnore]
		public decimal Clutch1V2WonPercent => Clutch1V2Count == 0 ? 0 : Math.Round((decimal)(Clutch1V2WonCount * 100) / Clutch1V2Count, 2);

		[JsonIgnore]
		public decimal Clutch1V3WonPercent => Clutch1V3Count == 0 ? 0 : Math.Round((decimal)(Clutch1V3WonCount * 100) / Clutch1V3Count, 2);

		[JsonIgnore]
		public decimal Clutch1V4WonPercent => Clutch1V4Count == 0 ? 0 : Math.Round((decimal)(Clutch1V4WonCount * 100) / Clutch1V4Count, 2);

		[JsonIgnore]
		public decimal Clutch1V5WonPercent => Clutch1V5Count == 0 ? 0 : Math.Round((decimal)(Clutch1V5WonCount * 100) / Clutch1V5Count, 2);

		[JsonProperty("suicide_count")]
		public int SuicideCount { get; set; }

		/// <summary>
		/// Total health damage made by the player
		/// </summary>
		[JsonProperty("total_damage_health_done")]
		public int TotalDamageHealthCount => PlayersHurted.ToList()
			.Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.AttackerSteamId == SteamId)
			.Sum(playerHurtedEvent => playerHurtedEvent.HealthDamage);

		/// <summary>
		/// Total armor damage made by the player
		/// </summary>
		[JsonProperty("total_damage_armor_done")]
		public int TotalDamageArmorCount => PlayersHurted.ToList()
			.Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.AttackerSteamId == SteamId)
			.Sum(playerHurtedEvent => playerHurtedEvent.ArmorDamage);

		/// <summary>
		/// Total health damage the player has received
		/// </summary>
		[JsonProperty("total_damage_health_received")]
		public int TotalDamageHealthReceivedCount => PlayersHurted.ToList()
			.Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.HurtedSteamId == SteamId)
			.Sum(playerHurtedEvent => playerHurtedEvent.HealthDamage);

		/// <summary>
		/// Total armor damage the player has received
		/// </summary>
		[JsonProperty("total_damage_armor_received")]
		public int TotalDamageArmorReceivedCount => PlayersHurted.ToList()
			.Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.HurtedSteamId == SteamId)
			.Sum(playerHurtedEvent => playerHurtedEvent.ArmorDamage);

		/// <summary>
		/// Average health damage the player has done during the match
		/// </summary>
		[JsonProperty("average_health_damage")]
		public double AverageHealthDamage
		{
			get
			{
				double total = 0;

				if (PlayersHurted.Any())
				{
					total = PlayersHurted.ToList().Where(playerHurtedEvent => playerHurtedEvent != null && playerHurtedEvent.AttackerSteamId == SteamId)
						.Aggregate(total, (current, playerHurtedEvent) => current + playerHurtedEvent.HealthDamage);
					if (Math.Abs(total) < 0.1) return total;
				}
				if (RoundPlayedCount > 0) total = Math.Round(total / RoundPlayedCount, 1);

				return total;
			}
		}

		[JsonIgnore]
		public decimal HeadshotPercent
		{
			get
			{
				decimal headshotPercent = 0;
				if (_headshotCount <= 0) return headshotPercent;
				if(_killCount > 0) headshotPercent = (_headshotCount * 100) / (decimal)_killCount;
				headshotPercent = Math.Round(headshotPercent, 2);

				return headshotPercent;
			}
		}

		[JsonIgnore]
		public decimal RatioEntryKill
		{
			get
			{
				int entryKillCount = EntryKills.Count();
				int entryKillWin = EntryKills.Count(e => e.HasWon);
				int entryKillLoss = EntryKills.Count(e => e.HasWon == false);

				decimal entryKillPercent = 0;
				if (entryKillWin == 0) return entryKillPercent;
				if (entryKillLoss == 0) return 100;
				entryKillPercent = (entryKillWin / (decimal)entryKillCount) * 100;
				entryKillPercent = Math.Round(entryKillPercent, 0);

				return entryKillPercent;
			}
		}

		[JsonIgnore]
		public decimal RatioEntryHoldKill
		{
			get
			{
				int entryHoldKillCount = EntryHoldKills.Count();
				int entryHoldKillWin = EntryHoldKills.Count(e => e.HasWon);
				int entryHoldKillLoss = EntryHoldKills.Count(e => e.HasWon == false);

				decimal entryHoldKillPercent = 0;
				if (entryHoldKillWin == 0) return entryHoldKillPercent;
				if (entryHoldKillLoss == 0) return 100;
				entryHoldKillPercent = entryHoldKillWin / (decimal)entryHoldKillCount * 100;
				entryHoldKillPercent = Math.Round(entryHoldKillPercent, 0);

				return entryHoldKillPercent;
			}
		}

		[JsonProperty("kill_per_round")]
		public double KillPerRound
		{
			get
			{
				if (RoundPlayedCount > 0) return Math.Round((double)KillCount / RoundPlayedCount, 2);
				return 0;
			}
		}

		[JsonProperty("assist_per_round")]
		public double AssistPerRound
		{
			get
			{
				if (RoundPlayedCount > 0) return Math.Round((double)AssistCount / RoundPlayedCount, 2);
				return 0;
			}
		}

		[JsonProperty("death_per_round")]
		public double DeathPerRound
		{
			get
			{
				if (RoundPlayedCount > 0) return Math.Round((double)DeathCount / RoundPlayedCount, 2);
				return 0;
			}
		}

		[JsonProperty("total_time_death")]
		public float TotalTimeDeath => TimeDeathRounds.Sum(kvp => kvp.Value);

		[JsonProperty("avg_time_death")]
		public double AverageTimeDeath
		{
			get
			{
				if (RoundPlayedCount > 0) return Math.Round((double)TotalTimeDeath / RoundPlayedCount, 2);
				return 0;
			}
		}

		[JsonIgnore]
		public bool IsControllingBot
		{
			get { return _isControllingBot; }
			set { Set(() => IsControllingBot, ref _isControllingBot, value); }
		}

		[JsonIgnore]
		public Side Side
		{
			get { return _side; }
			set { Set(() => Side, ref _side, value); }
		}

		[JsonIgnore]
		public bool IsAlive
		{
			get { return _isAlive; }
			set { Set(() => IsAlive, ref _isAlive, value); }
		}

		[JsonIgnore]
		public bool IsConnected
		{
			get { return _isConnected; }
			set { Set(() => IsConnected, ref _isConnected, value); }
		}

		[JsonIgnore]
		public float FlashDurationTemp
		{
			get { return _flashDurationTemp; }
			set { Set(() => FlashDurationTemp, ref _flashDurationTemp, value); }
		}

		[JsonIgnore]
		public bool HasBomb
		{
			get { return _hasBomb; }
			set { Set(() => HasBomb, ref _hasBomb, value); }
		}

		[JsonIgnore]
		public bool HasEntryKill
		{
			get { return _hasEntryKill; }
			set { Set(() => HasEntryKill, ref _hasEntryKill, value); }
		}

		[JsonIgnore]
		public bool HasEntryHoldKill
		{
			get { return _hasEntryHoldKill; }
			set { Set(() => HasEntryHoldKill, ref _hasEntryHoldKill, value); }
		}

		[JsonIgnore]
		public int MatchCount { get; set; } = 1;

		#endregion

		public override bool Equals(object obj)
		{
			var item = obj as Player;

			return item != null && SteamId.Equals(item.SteamId);
		}

		public override int GetHashCode()
		{
			return 1;
		}

		public Player()
		{
			EntryKills = new ObservableCollection<EntryKillEvent>();
			EntryHoldKills = new ObservableCollection<EntryHoldKillEvent>();
			PlayersHurted = new ObservableCollection<PlayerHurtedEvent>();
			Clutches = new ObservableCollection<ClutchEvent>();
			Kills = new ObservableCollection<KillEvent>();
			Deaths = new ObservableCollection<KillEvent>();
			Assists = new ObservableCollection<KillEvent>();
			Kills.CollectionChanged += OnKillsCollectionChanged;
			Deaths.CollectionChanged += OnDeathsCollectionChanged;
			Assists.CollectionChanged += OnAssistsCollectionChanged;
			EntryKills.CollectionChanged += OnEntryKillsCollectionChanged;
			EntryHoldKills.CollectionChanged += OnEntryHoldKillsCollectionChanged;
			PlayersHurted.CollectionChanged += OnPlayersHurtedCollectionChanged;
			Clutches.CollectionChanged += OnClutchesCollectionChanged;
			Side = Side.None;
		}

		private void OnAssistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			AssistCount = Assists.Count;
		}

		private void OnDeathsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DeathCount = Deaths.Count + SuicideCount;
			RaisePropertyChanged(() => AverageTimeDeath);
		}

		private void OnKillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			TeamKillCount = Kills.Count(k => k.KilledSide == k.KillerSide);
			KillCount = Kills.Count(k => k.KilledSide != k.KillerSide) - TeamKillCount - SuicideCount;
			HeadshotCount = Kills.Count(k => k.KilledSide != k.KillerSide && k.IsHeadshot);
			CrouchKillCount = Kills.Count(k => k.KilledSide != k.KillerSide && k.IsKillerCrouching);
			JumpKillCount = Kills.Count(k => k.KilledSide != k.KillerSide && k.KillerVelocityZ > 0);
		}

		private void OnPlayersHurtedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => TotalDamageHealthCount);
			RaisePropertyChanged(() => TotalDamageArmorCount);
			RaisePropertyChanged(() => AverageHealthDamage);
		}

		private void OnEntryKillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => EntryKillWonCount);
			RaisePropertyChanged(() => EntryKillLossCount);
			RaisePropertyChanged(() => RatioEntryKill);
		}

		private void OnEntryHoldKillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => EntryHoldKillWonCount);
			RaisePropertyChanged(() => EntryHoldKillLossCount);
			RaisePropertyChanged(() => RatioEntryHoldKill);
		}

		private void OnClutchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RaisePropertyChanged(() => ClutchCount);
			RaisePropertyChanged(() => ClutchLostCount);
			RaisePropertyChanged(() => Clutch1V1Count);
			RaisePropertyChanged(() => Clutch1V2Count);
			RaisePropertyChanged(() => Clutch1V3Count);
			RaisePropertyChanged(() => Clutch1V4Count);
			RaisePropertyChanged(() => Clutch1V5Count);
			RaisePropertyChanged(() => Clutch1V1WonCount);
			RaisePropertyChanged(() => Clutch1V2WonCount);
			RaisePropertyChanged(() => Clutch1V3WonCount);
			RaisePropertyChanged(() => Clutch1V4WonCount);
			RaisePropertyChanged(() => Clutch1V5WonCount);
			RaisePropertyChanged(() => Clutch1V1LossCount);
			RaisePropertyChanged(() => Clutch1V2LossCount);
			RaisePropertyChanged(() => Clutch1V3LossCount);
			RaisePropertyChanged(() => Clutch1V4LossCount);
			RaisePropertyChanged(() => Clutch1V5LossCount);
		}

		public void ResetStats()
		{
			AssistCount = 0;
			BombDefusedCount = 0;
			BombExplodedCount = 0;
			BombPlantedCount = 0;
			CrouchKillCount = 0;
			DecoyThrownCount = 0;
			DeathCount = 0;
			EseaRws = 0;
			EseaRwsPointCount = 0;
			FiveKillCount = 0;
			FlashbangThrownCount = 0;
			FlashDurationTemp = 0;
			FourKillCount = 0;
			HasBomb = false;
			HasEntryHoldKill = false;
			HasEntryKill = false;
			HeadshotCount = 0;
			HeGrenadeThrownCount = 0;
			IncendiaryThrownCount = 0;
			IsAlive = true;
			IsConnected = true;
			IsControllingBot = false;
			JumpKillCount = 0;
			KillCount = 0;
			MolotovThrownCount = 0;
			OneKillCount = 0;
			RatingHltv = 0;
			RatingHltv2 = 0;
			RoundMvpCount = 0;
			RoundPlayedCount = 0;
			Score = 0;
			SmokeThrownCount = 0;
			TeamKillCount = 0;
			ThreeKillCount = 0;
			TradeDeathCount = 0;
			TradeKillCount = 0;
			TwoKillCount = 0;

			Assists.Clear();
			Clutches.Clear();
			Deaths.Clear();
			EntryHoldKills.Clear();
			EntryKills.Clear();
			EquipementValueRounds.Clear();
			Kills.Clear();
			PlayersHurted.Clear();
			RoundsMoneyEarned.Clear();
			StartMoneyRounds.Clear();
			TimeDeathRounds.Clear();
		}

		public Player Clone()
		{
			Player player = new Player
			{
				Name = Name,
				SteamId = SteamId,
				Score = Score,
				DecoyThrownCount = DecoyThrownCount,
				KillCount = KillCount,
				BombDefusedCount = BombDefusedCount,
				MatchCount = MatchCount,
				DeathCount = DeathCount,
				SuicideCount = SuicideCount,
				HitCount = HitCount,
				AssistCount = AssistCount,
				AvatarUrl = AvatarUrl,
				BombExplodedCount = BombExplodedCount,
				BombPlantedCount = BombPlantedCount,
				CrouchKillCount = CrouchKillCount,
				FiveKillCount = FiveKillCount,
				FourKillCount = FourKillCount,
				ThreeKillCount = ThreeKillCount,
				TwoKillCount = TwoKillCount,
				OneKillCount = OneKillCount,
				EseaRws = EseaRws,
				EseaRwsPointCount = EseaRwsPointCount,
				FlashDurationTemp = FlashDurationTemp,
				FlashbangThrownCount = FlashbangThrownCount,
				HeadshotCount = HeadshotCount,
				HasBomb = HasBomb,
				HasEntryHoldKill = HasEntryHoldKill,
				HasEntryKill = HasEntryKill,
				IsAlive = IsAlive,
				HeGrenadeThrownCount = HeGrenadeThrownCount,
				IsConnected = IsConnected,
				IsControllingBot = IsControllingBot,
				IncendiaryThrownCount = IncendiaryThrownCount,
				IsOverwatchBanned = IsOverwatchBanned,
				IsVacBanned = IsVacBanned,
				JumpKillCount = JumpKillCount,
				KillDeathRatio = KillDeathRatio,
				MolotovThrownCount = MolotovThrownCount,
				RankNumberNew = RankNumberNew,
				RankNumberOld = RankNumberOld,
				RatingHltv = RatingHltv,
				RatingHltv2 = RatingHltv2,
				RoundPlayedCount = RoundPlayedCount,
				RoundMvpCount = RoundMvpCount,
				ShotCount = ShotCount,
				Side = Side,
				SmokeThrownCount = SmokeThrownCount,
				TeamKillCount = TeamKillCount,
				TeamName = TeamName,
				TradeDeathCount = TradeDeathCount,
				TradeKillCount = TradeKillCount,
				WinCount = WinCount,
				PlayersHurted = new ObservableCollection<PlayerHurtedEvent>(),
				Kills = new ObservableCollection<KillEvent>(),
				Assists = new ObservableCollection<KillEvent>(),
				Deaths = new ObservableCollection<KillEvent>(),
				EntryHoldKills = new ObservableCollection<EntryHoldKillEvent>(),
				RoundsMoneyEarned = new Dictionary<int, int>(),
				Clutches = new ObservableCollection<ClutchEvent>(),
				EntryKills = new ObservableCollection<EntryKillEvent>(),
				EquipementValueRounds = new Dictionary<int, int>(),
				StartMoneyRounds = new Dictionary<int, int>(),
			};

			foreach (PlayerHurtedEvent e in PlayersHurted)
				player.PlayersHurted.Add(e);
			foreach (KillEvent e in Kills)
				player.Kills.Add(e);
			foreach (KillEvent e in Deaths)
				player.Deaths.Add(e);
			foreach (KillEvent a in Assists)
				player.Assists.Add(a);
			foreach (EntryHoldKillEvent e in EntryHoldKills)
				player.EntryHoldKills.Add(e);
			foreach (ClutchEvent e in Clutches)
				player.Clutches.Add(e);
			foreach (KeyValuePair<int, int> kvp in EquipementValueRounds)
				player.EquipementValueRounds.Add(kvp.Key, kvp.Value);
			foreach (EntryKillEvent e in EntryKills)
				player.EntryKills.Add(e);
			foreach (KeyValuePair<int, int> kvp in RoundsMoneyEarned)
				player.RoundsMoneyEarned.Add(kvp.Key, kvp.Value);
			foreach (KeyValuePair<int, int> kvp in StartMoneyRounds)
				player.StartMoneyRounds.Add(kvp.Key, kvp.Value);
			foreach (KeyValuePair<int, float> kvp in TimeDeathRounds)
				player.TimeDeathRounds.Add(kvp.Key, kvp.Value);

			return player;
		}

		/// <summary>
		/// Restore player's data from a Player object
		/// </summary>
		/// <param name="player"></param>
		public void BackupFromPlayer(Player player)
		{
			Kills.Clear();
			foreach (KillEvent e in player.Kills) Kills.Add(e);

			Deaths.Clear();
			foreach (KillEvent e in player.Deaths) Deaths.Add(e);

			Assists.Clear();
			foreach (KillEvent e in player.Assists) Assists.Add(e);

			Clutches.Clear();
			foreach (ClutchEvent e in player.Clutches) Clutches.Add(e);

			EntryHoldKills.Clear();
			foreach (EntryHoldKillEvent e in player.EntryHoldKills) EntryHoldKills.Add(e);

			EntryKills.Clear();
			foreach (EntryKillEvent e in player.EntryKills) EntryKills.Add(e);

			PlayersHurted.Clear();
			foreach (PlayerHurtedEvent e in player.PlayersHurted) PlayersHurted.Add(e);

			RoundsMoneyEarned.Clear();
			foreach (KeyValuePair<int, int> kvp in player.RoundsMoneyEarned) RoundsMoneyEarned.Add(kvp.Key, kvp.Value);

			StartMoneyRounds.Clear();
			foreach (KeyValuePair<int, int> kvp in player.StartMoneyRounds) StartMoneyRounds.Add(kvp.Key, kvp.Value);

			EquipementValueRounds.Clear();
			foreach (KeyValuePair<int, int> kvp in player.EquipementValueRounds) EquipementValueRounds.Add(kvp.Key, kvp.Value);

			TimeDeathRounds.Clear();
			foreach (KeyValuePair<int, float> kvp in player.TimeDeathRounds) TimeDeathRounds.Add(kvp.Key, kvp.Value);

			AssistCount = player.AssistCount;
			BombDefusedCount = player.BombDefusedCount;
			BombExplodedCount = player.BombExplodedCount;
			BombPlantedCount = player.BombPlantedCount;
			CrouchKillCount = player.CrouchKillCount;
			DeathCount = player.DeathCount;
			DecoyThrownCount = player.DecoyThrownCount;
			EseaRws = player.EseaRws;
			EseaRwsPointCount = player.EseaRwsPointCount;
			FiveKillCount = player.FiveKillCount;
			FlashbangThrownCount = player.FlashbangThrownCount;
			FlashDurationTemp = player.FlashDurationTemp;
			FourKillCount = player.FourKillCount;
			HasBomb = player.HasBomb;
			HasEntryHoldKill = player.HasEntryHoldKill;
			HasEntryKill = player.HasEntryKill;
			HeadshotCount = player.HeadshotCount;
			HeGrenadeThrownCount = player.HeGrenadeThrownCount;
			IsAlive = player.IsAlive;
			IncendiaryThrownCount = player.IncendiaryThrownCount;
			IsConnected = player.IsConnected;
			IsControllingBot = player.IsControllingBot;
			JumpKillCount = player.JumpKillCount;
			KillCount = player.KillCount;
			MolotovThrownCount = player.MolotovThrownCount;
			OneKillCount = player.OneKillCount;
			RatingHltv = player.RatingHltv;
			RatingHltv2 = player.RatingHltv2;
			RoundMvpCount = player.RoundMvpCount;
			RoundPlayedCount = player.RoundPlayedCount;
			Score = player.Score;
			Side = player.Side;
			SmokeThrownCount = player.SmokeThrownCount;
			TeamKillCount = player.TeamKillCount;
			TeamName = player.TeamName;
			ThreeKillCount = player.ThreeKillCount;
			TradeDeathCount = player.TradeDeathCount;
			TradeKillCount = player.TradeKillCount;
			TwoKillCount = player.TwoKillCount;
		}
	}
}
