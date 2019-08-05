using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DemoInfo
{
	public class HeaderParsedEventArgs : EventArgs
	{
		public DemoHeader Header { get; private set; }

		public HeaderParsedEventArgs(DemoHeader header)
		{
			this.Header = header;
		}
	}

	public class TickDoneEventArgs : EventArgs
	{
	}

	public class MatchStartedEventArgs : EventArgs
	{
	}

	public class RoundAnnounceMatchStartedEventArgs : EventArgs
	{
	}

	public class RoundEndedEventArgs : EventArgs
	{
		public RoundEndReason Reason { get; set; }

		public string Message { get; set; }

		/// <summary>
		/// The winning team. Spectate for everything that isn't CT or T. 
		/// </summary>
		public Team Winner;

	}

	public class RoundOfficiallyEndedEventArgs : EventArgs
	{
	}

	public class RoundMVPEventArgs : EventArgs
	{
		public Player Player { get; set; }

		public RoundMVPReason Reason { get; set; }
	}

	public class RoundStartedEventArgs : EventArgs
	{
		public int TimeLimit { get; set; }
		public int FragLimit { get; set; }
		public string Objective { get; set; }
	}

	public class WinPanelMatchEventArgs : EventArgs
	{
	}

	public class RoundFinalEventArgs : EventArgs
	{
	}

	public class LastRoundHalfEventArgs : EventArgs
	{
	}

	public class FreezetimeEndedEventArgs : EventArgs
	{
	}

	public class PlayerTeamEventArgs : EventArgs
	{
		public Player Swapped { get; internal set; }

		public Team NewTeam { get; internal set; }

		public Team OldTeam { get; internal set; }

		public bool Silent { get; internal set; }

		public bool IsBot { get; internal set; }
	}

	public class PlayerKilledEventArgs : EventArgs
	{
		public Equipment Weapon { get; internal set; }

		[Obsolete("Use \"Victim\" instead. This will be removed soon™", false)]
		public Player DeathPerson { get { return Victim; } }

		public Player Victim { get; internal set; }

		public Player Killer { get; internal set; }

		public Player Assister { get; internal set; }

		public int PenetratedObjects { get; internal set; }

		public bool Headshot { get; internal set; }
	}

	public class BotTakeOverEventArgs : EventArgs
	{
		public Player Taker { get; internal set; }
	}

	public class WeaponFiredEventArgs : EventArgs
	{
		public Equipment Weapon { get; internal set; }

		public Player Shooter { get; internal set; }
	}

	public class NadeEventArgs : EventArgs
	{
		public Vector Position { get; internal set; }
		public EquipmentElement NadeType { get; internal set; }
		public Player ThrownBy { get; internal set; }

		internal NadeEventArgs()
		{

		}

		internal NadeEventArgs(EquipmentElement type)
		{
			this.NadeType = type;
		}
	}

	public class FireEventArgs : NadeEventArgs
	{
		public FireEventArgs() : base(EquipmentElement.Incendiary)
		{

		}
	}
	public class SmokeEventArgs : NadeEventArgs
	{
		public SmokeEventArgs() : base(EquipmentElement.Smoke)
		{

		}
	}
	public class DecoyEventArgs : NadeEventArgs
	{
		public DecoyEventArgs() : base(EquipmentElement.Decoy)
		{

		}
	}
	public class FlashEventArgs : NadeEventArgs
	{
		//previous blind implementation
		public Player[] FlashedPlayers { get; internal set; }
		//

		public FlashEventArgs() : base(EquipmentElement.Flash)
		{

		}
	}
	public class GrenadeEventArgs : NadeEventArgs
	{
		public GrenadeEventArgs() : base(EquipmentElement.HE)
		{

		}
	}

	public class BombEventArgs : EventArgs
	{
		public Player Player { get; set; }

		public char Site { get; set; }
	}

	public class BombDefuseEventArgs : EventArgs
	{
		public Player Player { get; set; }

		public bool HasKit { get; set; }
	}

	public class PlayerHurtEventArgs : EventArgs
	{
		/// <summary>
		/// The hurt player
		/// </summary>
		public Player Player { get; set; }

		/// <summary>
		/// The attacking player
		/// </summary>
		public Player Attacker { get; set; }

		/// <summary>
		/// Remaining health points of the player
		/// </summary>
		public int Health { get; set; }

		/// <summary>
		/// Remaining armor points of the player
		/// </summary>
		public int Armor { get; set; }

		/// <summary>
		/// The Weapon used to attack. 
		/// Note: This might be not the same as the raw event
		/// we replace "hpk2000" with "usp-s" if the attacker
		/// is currently holding it - this value is originally
		/// networked "wrong". By using this property you always
		/// get the "right" weapon
		/// </summary>
		/// <value>The weapon.</value>
		public Equipment Weapon { get; set; }

		/// <summary>
		/// The original "weapon"-value from the event. 
		/// Might be wrong for USP, CZ and M4A1-S
		/// </summary>
		/// <value>The weapon string.</value>
		public string WeaponString { get; set; }

		/// <summary>
		/// The damage done to the players health
		/// </summary>
		public int HealthDamage { get; set; }

		/// <summary>
		/// The damage done to the players armor
		/// </summary>
		public int ArmorDamage { get; set; }

		/// <summary>
		/// Where the Player was hit. 
		/// </summary>
		/// <value>The hitgroup.</value>
		public Hitgroup Hitgroup { get; set; }
	}

	public class BlindEventArgs : EventArgs
	{
		public Player Player { get; set; }

		public Player Attacker { get; set; }

		public float? FlashDuration { get; set; }
	}

	public class PlayerBindEventArgs : EventArgs
	{
		public Player Player { get; set; }
	}

	public class PlayerDisconnectEventArgs : EventArgs
	{
		public Player Player {get; set; }

		public string Reason { get; set; }
	}

	/// <summary>
	/// Occurs when the server use the "say" command
	/// I don't know the purpose of IsChat and IsChatAll because they are everytime false
	/// </summary>
	public class SayTextEventArgs : EventArgs
	{
		/// <summary>
		/// Should be everytime 0 as it's a message from the server
		/// </summary>
		public int EntityIndex { get; set; }

		/// <summary>
		/// Message sent by the server
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Everytime false as the message is public
		/// </summary>
		public bool IsChat { get; set; }

		/// <summary>
		/// Everytime false as the message is public
		/// </summary>
		public bool IsChatAll { get; set; }
	}

	/// <summary>
	/// Occurs when a player use the say command
	/// Not sure about IsChat and IsChatAll, GOTV doesn't record chat team so this 2 bool are every time true
	/// </summary>
	public class SayText2EventArgs : EventArgs
	{
		/// <summary>
		/// The player who sent the message
		/// </summary>
		public Player Sender { get; set; }

		/// <summary>
		/// The message sent
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Not sure about it, maybe it's to indicate say_team or say
		/// </summary>
		public bool IsChat { get; set; }

		/// <summary>
		/// true if the message is for all players ?
		/// </summary>
		public bool IsChatAll { get; set; }
	}

	/// <summary>
	/// Occurs when the server display a player rank
	/// It occurs only with Valve demos, at the end of a Matchmaking.
	/// So for a 5v5 match there will be 10 events trigerred
	/// </summary>
	public class RankUpdateEventArgs : EventArgs
	{
		/// <summary>
		/// Player's SteamID64
		/// </summary>
		public long SteamId { get; set; }

		/// <summary>
		/// Player's rank at the beginning of the match
		/// </summary>
		public int RankOld { get; set; }

		/// <summary>
		/// Player's rank the end of the match
		/// </summary>
		public int RankNew { get; set; }

		/// <summary>
		/// Number of win that the player have
		/// </summary>
		public int WinCount { get; set; }

		/// <summary>
		/// Number of rank the player win / lost between the beggining and the end of the match
		/// </summary>
		public float RankChange { get; set; }
	}

	/// <summary>
	/// Occurs when a player leave a buy zone
	/// </summary>
	public class PlayerLeftBuyZoneEventArgs : EventArgs
	{
		/// <summary>
		/// Player who left the zone
		/// </summary>
		public Player Player { get; set; }
	}

	/// <summary>
	/// Occurs when a player's money changed
	/// </summary>
	public class PlayerMoneyChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Player that had money changes
		/// </summary>
		public Player Player { get; set; }

		/// <summary>
		/// Old account value
		/// </summary>
		public int OldAccount { get; set; }

		/// <summary>
		/// New account value
		/// </summary>
		public int NewAccount { get; set; }
	}

	/// <summary>
	/// Occurs when a player picked a weapon (buy or standard pick)
	/// </summary>
	public class PlayerPickWeaponEventArgs : EventArgs
	{
		/// <summary>
		/// The player who picked the weapon
		/// </summary>
		public Player Player { get; set; }

		/// <summary>
		/// Weapon picked
		/// </summary>
		public Equipment Weapon { get; set; }
	}

	/// <summary>
	/// Occurs when a player drop a weapon
	/// </summary>
	public class PlayerDropWeaponEventArgs : EventArgs
	{
		/// <summary>
		/// The player who dropped the weapon
		/// </summary>
		public Player Player { get; set; }

		/// <summary>
		/// Weapon dropped
		/// </summary>
		public Equipment Weapon { get; set; }
	}

	/// <summary>
	/// Occurs when a player buy an equipment
	/// </summary>
	public class PlayerBuyEventArgs : EventArgs
	{
		/// <summary>
		/// The player who bought the equipment
		/// </summary>
		public Player Player { get; set; }

		/// <summary>
		/// Equipment bought
		/// </summary>
		public Equipment Weapon { get; set; }
	}

	/// <summary>
	/// Occurs when a ConVar has changed
	/// </summary>
	public class ConVarChangeEventArgs : EventArgs
	{
		/// <summary>
		/// ConVar's name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// ConVar value
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Don't know what's that
		/// </summary>
		public UInt32 DictionaryValue { get; set; }
	}

	/// <summary>
	/// Occurs when a team's score has changed
	/// </summary>
	public class TeamScoreChangeEventArgs : EventArgs
	{
		/// <summary>
		/// Team which its score changed
		/// </summary>
		public Team Team { get; set; }

		/// <summary>
		/// Old score
		/// </summary>
		public int OldScore { get; set; }

		/// <summary>
		/// New Score
		/// </summary>
		public int NewScore { get; set; }
	}

	public class Equipment
	{
		internal int EntityID { get; set; }

		public EquipmentElement Weapon { get; set; }
		public EquipmentClass Class
		{
			get
			{
				return (EquipmentClass)(((int)Weapon / 100) + 1);
			}
		}

		/// <summary>
		/// string name from the game (eg. weapon_ak47)
		/// This names are defined in scripts/items/items_game.txt
		/// </summary>
		public string OriginalString { get; set; }

		public string SkinID { get; set; }

		public int AmmoInMagazine { get; set; }

		internal int AmmoType { get; set; }

		public Player Owner { get; set; }

		/// <summary>
		/// Previous equipment owner
		/// </summary>
		public Player PrevOwner { get; set; }

		public int ReserveAmmo {
			get {
				return (Owner != null && AmmoType != -1) ? Owner.AmmoLeft [AmmoType] : -1;
			}
		}

		internal Equipment()
		{
			this.Weapon = EquipmentElement.Unknown;
		}

		internal Equipment(string originalString)
		{
			OriginalString = originalString;

			this.Weapon = MapEquipment(originalString);
		}

		internal Equipment(string originalString, string skin)
		{
			OriginalString = originalString;

			this.Weapon = MapEquipment(originalString);

			SkinID = skin;
		}

		private const string WEAPON_PREFIX = "weapon_";
		private const string ITEM_PREFIX = "item_";

		/// <summary>
		/// Map item string coming from demos events with equipment.
		/// Used for specials equipments such as kevlar and weapons that have special name such as molotov.
		/// Others weapons detection is based on their item index definition.
		/// </summary>
		/// <param name="originalString"></param>
		/// <returns></returns>
		public static EquipmentElement MapEquipment(string originalString)
		{
			EquipmentElement weapon = EquipmentElement.Unknown;

			if (!originalString.StartsWith(ITEM_PREFIX) && !originalString.StartsWith(WEAPON_PREFIX))
			{
				originalString = WEAPON_PREFIX + originalString;
			}

			EquipmentMapping equipment = Equipments.FirstOrDefault(e => e.OriginalName == originalString);
			if (equipment.ItemIndex == 0)
			{
				switch (originalString)
				{
					case "item_kevlar":
					case "item_vest":
						weapon = EquipmentElement.Kevlar;
						break;
					case "item_assaultsuit":
					case "item_vesthelm":
						weapon = EquipmentElement.Helmet;
						break;
					case "item_defuser":
						weapon = EquipmentElement.DefuseKit;
						break;
					case "weapon_world":
					case "weapon_worldspawn":
						weapon = EquipmentElement.World;
						break;
					case "weapon_inferno":
						weapon = EquipmentElement.Incendiary;
						break;
					case "weapon_molotov_projectile":
					case "weapon_molotovgrenade":
						weapon = EquipmentElement.Molotov;
						break;
					default:
						Trace.WriteLine("Unknown weapon. " + originalString, "Equipment.MapEquipment()");
						break;
				}
			}
			else
			{
				weapon = equipment.Element;
			}

			return weapon;
		}

		/// <summary>
		/// Mapping between item index definition and EquipmentElement.
		/// Item indexes are located in the game file /csgo/scripts/items_game.txt
		/// </summary>
		public static EquipmentMapping[] Equipments =
		{
			new EquipmentMapping
			{
				ItemIndex = 1,
				OriginalName = "weapon_deagle",
				Element = EquipmentElement.Deagle,
			},
			new EquipmentMapping
			{
				ItemIndex = 2,
				OriginalName = "weapon_elite",
				Element = EquipmentElement.DualBarettas,
			},
			new EquipmentMapping
			{
				ItemIndex = 3,
				OriginalName = "weapon_fiveseven",
				Element = EquipmentElement.FiveSeven,
			},
			new EquipmentMapping
			{
				ItemIndex = 4,
				OriginalName = "weapon_glock",
				Element = EquipmentElement.Glock,
			},
			new EquipmentMapping
			{
				ItemIndex = 7,
				OriginalName = "weapon_ak47",
				Element = EquipmentElement.AK47,
			},
			new EquipmentMapping
			{
				ItemIndex = 8,
				OriginalName = "weapon_aug",
				Element = EquipmentElement.AUG,
			},
			new EquipmentMapping
			{
				ItemIndex = 9,
				OriginalName = "weapon_awp",
				Element = EquipmentElement.AWP,
			},
			new EquipmentMapping
			{
				ItemIndex = 10,
				OriginalName = "weapon_famas",
				Element = EquipmentElement.Famas,
			},
			new EquipmentMapping
			{
				ItemIndex = 11,
				OriginalName = "weapon_g3sg1",
				Element = EquipmentElement.G3SG1,
			},
			new EquipmentMapping
			{
				ItemIndex = 13,
				OriginalName = "weapon_galilar",
				Element = EquipmentElement.Gallil,
			},
			new EquipmentMapping
			{
				ItemIndex = 14,
				OriginalName = "weapon_m249",
				Element = EquipmentElement.M249,
			},
			new EquipmentMapping
			{
				ItemIndex = 16,
				OriginalName = "weapon_m4a1",
				Element = EquipmentElement.M4A4,
			},
			new EquipmentMapping
			{
				ItemIndex = 17,
				OriginalName = "weapon_mac10",
				Element = EquipmentElement.Mac10,
			},
			new EquipmentMapping
			{
				ItemIndex = 19,
				OriginalName = "weapon_p90",
				Element = EquipmentElement.P90,
			},
			new EquipmentMapping
			{
				ItemIndex = 24,
				OriginalName = "weapon_ump45",
				Element = EquipmentElement.UMP,
			},
			new EquipmentMapping
			{
				ItemIndex = 25,
				OriginalName = "weapon_xm1014",
				Element = EquipmentElement.XM1014,
			},
			new EquipmentMapping
			{
				ItemIndex = 26,
				OriginalName = "weapon_bizon",
				Element = EquipmentElement.Bizon,
			},
			new EquipmentMapping
			{
				ItemIndex = 27,
				OriginalName = "weapon_mag7",
				Element = EquipmentElement.Swag7,
			},
			new EquipmentMapping
			{
				ItemIndex = 28,
				OriginalName = "weapon_negev",
				Element = EquipmentElement.Negev,
			},
			new EquipmentMapping
			{
				ItemIndex = 29,
				OriginalName = "weapon_sawedoff",
				Element = EquipmentElement.SawedOff,
			},
			new EquipmentMapping
			{
				ItemIndex = 30,
				OriginalName = "weapon_tec9",
				Element = EquipmentElement.Tec9,
			},
			new EquipmentMapping
			{
				ItemIndex = 31,
				OriginalName = "weapon_taser",
				Element = EquipmentElement.Zeus,
			},
			new EquipmentMapping
			{
				ItemIndex = 32,
				OriginalName = "weapon_hkp2000",
				Element = EquipmentElement.P2000,
			},
			new EquipmentMapping
			{
				ItemIndex = 33,
				OriginalName = "weapon_mp7",
				Element = EquipmentElement.MP7,
			},
			new EquipmentMapping
			{
				ItemIndex = 34,
				OriginalName = "weapon_mp9",
				Element = EquipmentElement.MP9,
			},
			new EquipmentMapping
			{
				ItemIndex = 35,
				OriginalName = "weapon_nova",
				Element = EquipmentElement.Nova,
			},
			new EquipmentMapping
			{
				ItemIndex = 36,
				OriginalName = "weapon_p250",
				Element = EquipmentElement.P250,
			},
			new EquipmentMapping
			{
				ItemIndex = 38,
				OriginalName = "weapon_scar20",
				Element = EquipmentElement.Scar20,
			},
			new EquipmentMapping
			{
				ItemIndex = 39,
				OriginalName = "weapon_sg556",
				Element = EquipmentElement.SG556,
			},
			new EquipmentMapping
			{
				ItemIndex = 40,
				OriginalName = "weapon_ssg08",
				Element = EquipmentElement.Scout,
			},
			new EquipmentMapping
			{
				ItemIndex = 42,
				OriginalName = "weapon_knife",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 43,
				OriginalName = "weapon_flashbang",
				Element = EquipmentElement.Flash,
			},
			new EquipmentMapping
			{
				ItemIndex = 44,
				OriginalName = "weapon_hegrenade",
				Element = EquipmentElement.HE,
			},
			new EquipmentMapping
			{
				ItemIndex = 45,
				OriginalName = "weapon_smokegrenade",
				Element = EquipmentElement.Smoke,
			},
			new EquipmentMapping
			{
				ItemIndex = 46,
				OriginalName = "weapon_molotov",
				Element = EquipmentElement.Molotov,
			},
			new EquipmentMapping
			{
				ItemIndex = 47,
				OriginalName = "weapon_decoy",
				Element = EquipmentElement.Decoy,
			},
			new EquipmentMapping
			{
				ItemIndex = 48,
				OriginalName = "weapon_incgrenade",
				Element = EquipmentElement.Incendiary,
			},
			new EquipmentMapping
			{
				ItemIndex = 49,
				OriginalName = "weapon_c4",
				Element = EquipmentElement.Bomb,
			},
			new EquipmentMapping
			{
				ItemIndex = 59,
				OriginalName = "weapon_knife_t",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 60,
				OriginalName = "weapon_m4a1_silencer",
				Element = EquipmentElement.M4A1,
			},
			new EquipmentMapping
			{
				ItemIndex = 61,
				OriginalName = "weapon_usp_silencer",
				Element = EquipmentElement.USP,
			},
			new EquipmentMapping
			{
				ItemIndex = 63,
				OriginalName = "weapon_cz75a",
				Element = EquipmentElement.CZ,
			},
			new EquipmentMapping
			{
				ItemIndex = 64,
				OriginalName = "weapon_revolver",
				Element = EquipmentElement.Revolver,
			},
			new EquipmentMapping
			{
				ItemIndex = 23,
				OriginalName = "weapon_mp5sd",
				Element = EquipmentElement.MP5SD,
			},
			new EquipmentMapping
			{
				ItemIndex = 500,
				OriginalName = "weapon_bayonet",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 505,
				OriginalName = "weapon_knife_flip",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 506,
				OriginalName = "weapon_knife_gut",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 507,
				OriginalName = "weapon_knife_karambit",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 508,
				OriginalName = "weapon_knife_m9_bayonet",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 509,
				OriginalName = "weapon_knife_tactical",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 512,
				OriginalName = "weapon_knife_falchion",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 514,
				OriginalName = "weapon_knife_survival_bowie",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 515,
				OriginalName = "weapon_knife_butterfly",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 516,
				OriginalName = "weapon_knife_push",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 519,
				OriginalName = "weapon_knife_ursus",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 520,
				OriginalName = "weapon_knife_gypsy_jackknife",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 522,
				OriginalName = "weapon_knife_stiletto",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 523,
				OriginalName = "weapon_knife_widowmaker",
				Element = EquipmentElement.Knife,
			},
            /* danger zone items */
            new EquipmentMapping
			{
				ItemIndex = 69,
				OriginalName = "weapon_fists",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 70,
				OriginalName = "weapon_breachcharge",
				Element = EquipmentElement.Bomb,
			},
			new EquipmentMapping
			{
				ItemIndex = 75,
				OriginalName = "weapon_axe",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 76,
				OriginalName = "weapon_hammer",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 78,
				OriginalName = "weapon_spanner",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 80,
				OriginalName = "weapon_knife_ghost",
				Element = EquipmentElement.Knife,
			},
			new EquipmentMapping
			{
				ItemIndex = 83,
				OriginalName = "weapon_frag_grenade",
				Element = EquipmentElement.HE,
			},
			new EquipmentMapping
			{
				ItemIndex = 81,
				OriginalName = "weapon_firebomb",
				Element = EquipmentElement.Molotov,
			},
			new EquipmentMapping
			{
				ItemIndex = 82,
				OriginalName = "weapon_diversion",
				Element = EquipmentElement.Decoy,
			},

		};
	}

	public enum EquipmentElement
	{
		Unknown = 0,

		//Pistoles
		P2000 = 1,
		Glock = 2,
		P250 = 3,
		Deagle = 4,
		FiveSeven = 5,
		DualBarettas = 6,
		Tec9 = 7,
		CZ = 8,
		USP = 9,
		Revolver = 10,

		//SMGs
		MP7 = 101,
		MP9 = 102,
		Bizon = 103,
		Mac10 = 104,
		UMP = 105,
		P90 = 106,
		MP5SD = 107,

		//Heavy
		SawedOff = 201,
		Nova = 202,
		Swag7 = 203,
		XM1014 = 204,
		M249 = 205,
		Negev = 206,

		//Rifle
		Gallil = 301,
		Famas = 302,
		AK47 = 303,
		M4A4 = 304,
		M4A1 = 305,
		Scout = 306,
		SG556 = 307,
		AUG = 308,
		AWP = 309,
		Scar20 = 310,
		G3SG1 = 311,

		//Equipment
		Zeus = 401,
		Kevlar = 402,
		Helmet = 403,
		Bomb = 404,
		Knife = 405,
		DefuseKit = 406,
		World = 407,

		//Grenades
		Decoy = 501,
		Molotov = 502,
		Incendiary = 503,
		Flash = 504,
		Smoke = 505,
		HE = 506
	}

	public enum EquipmentClass
	{
		Unknown = 0,
		Pistol = 1,
		SMG = 2,
		Heavy = 3,
		Rifle = 4,
		Equipment = 5,
		Grenade = 6,
	}

	/// <summary>
	/// Struct to map Equipment with game item index and DemoInfo EquipmentElement API.
	/// </summary>
	public struct EquipmentMapping
	{
		/// <summary>
		/// Index defined in the game file csgo\scripts\items\items_game.txt
		/// </summary>
		public int ItemIndex;

		/// <summary>
		/// Equipment string name defined in the game file items_game.txt
		/// </summary>
		public string OriginalName;

		/// <summary>
		/// Mapping to EquipmentElement to not break current API
		/// </summary>
		public EquipmentElement Element;
	}
}
