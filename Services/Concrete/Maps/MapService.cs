using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Core;
using Core.Models;
using Core.Models.Maps;
using Services.Exceptions.Map;
using Services.Interfaces;

namespace Services.Concrete.Maps
{
	public class MapService : IMapService
	{
		private const string DUST2 = "de_dust2";
		private const string INFERNO = "de_inferno";
		private const string NUKE = "de_nuke";
		private const string CACHE = "de_cache";
		private const string SEASON = "de_season";
		private const string TRAIN = "de_train";
		private const string CBBLE = "de_cbble";
		private const string OVERPASS = "de_overpass";
		private const string MIRAGE = "de_mirage";
		private const string EMPIRE = "de_empire";
		private const string SANTORINI = "de_santorini";
		private const string TULIP = "de_tulip";
		private const string ROYAL = "de_royal";
		private const string CRUISE = "cs_cruise";
		private const string COAST = "de_coast";
		private const string MIKLA = "de_mikla";
		private const string CANALS = "de_canals";
		private const string AGENCY = "cs_agency";
		private const string AUSTRIA = "de_austria";
		private const string SUBZERO = "de_subzero";
		private const string BIOME = "de_biome";
		private const string OFFICE = "cs_office";
		private const string VERTIGO = "de_vertigo";

		public static string[] SimpleRadarMaps =
		{
			"de_cache",
			"de_cbble",
			"de_mirage",
			"de_new_dust2",
			"de_new_inferno",
			"de_new_nuke",
			"de_overpass",
			"de_train"
		};

		public static string[] Maps = {
			DUST2, INFERNO, NUKE, CACHE, SEASON, TRAIN, CBBLE, OVERPASS,
			MIRAGE, EMPIRE, SANTORINI, TULIP, ROYAL, CRUISE, COAST, MIKLA,
			CANALS, AGENCY, AUSTRIA, SUBZERO, BIOME, OFFICE, VERTIGO
        };

		public Map Map { get; set; }

		public void InitMap(Demo demo)
		{
			switch (demo.MapName)
			{
				case DUST2:
					if (demo.Date > new DateTime(2017, 10, 17))
					{
						Map = new NewDust2();
						break;
					}
					Map = new Dust2();
					break;
				case INFERNO:
					if (demo.Date > new DateTime(2016, 10, 12))
					{
						Map = new NewInferno();
						break;
					}
					Map = new Inferno();
					break;
				case NUKE:
					if (demo.Date > new DateTime(2016, 02, 17))
					{
						Map = new NewNuke();
						break;
					}
					Map = new Nuke();
					break;
				case CACHE:
					Map = new Cache();
					break;
				case SEASON:
					Map = new Season();
					break;
				case CBBLE:
					Map = new Cbble();
					break;
				case OVERPASS:
					Map = new Overpass();
					break;
				case MIRAGE:
					Map = new Mirage();
					break;
				case TRAIN:
					Map = new Train();
					break;
				case EMPIRE:
					Map = new Empire();
					break;
				case SANTORINI:
					Map = new Santorini();
					break;
				case TULIP:
					Map = new Tulip();
					break;
				case ROYAL:
					Map = new Royal();
					break;
				case CRUISE:
					Map = new Cruise();
					break;
				case COAST:
					Map = new Coast();
					break;
				case MIKLA:
					Map = new Mikla();
					break;
				case CANALS:
					Map = new Canals();
					break;
                case AGENCY:
                    Map = new Agency();
                    break;
                case AUSTRIA:
                    Map = new Austria();
                    break;
                case SUBZERO:
                    Map = new Subzero();
                    break;
                case BIOME:
                    Map = new Biome();
                    break;
                case OFFICE:
                    Map = new Office();
                    break;
                case VERTIGO:
                    Map = new Vertigo();
                    break;
                default:
					throw new MapUnavailableException(demo.MapName);
			}
		}

		/// <summary>
		/// Calcul X coordinate where the pixel will be draw on the image
		/// </summary>
		/// <param name="x"></param>
		public float CalculatePointToResolutionX(float x)
		{
			return (float)((x - Map.PosX) / Map.Scale);
		}

		/// <summary>
		/// Calcul Y coordinate where the pixel will be draw on the image
		/// </summary>
		/// <param name="y"></param>
		public float CalculatePointToResolutionY(float y)
		{
			return (float)((Map.PosY - y) / Map.Scale);
		}

		public WriteableBitmap GetWriteableImage(bool useSimpleRadar = true)
		{
			try
			{
				string imagesPath = AppSettings.RESOURCES_URI + "images/maps/overview/";
				if (useSimpleRadar && Array.IndexOf(SimpleRadarMaps, Map.Name) >= 0) imagesPath += "simpleradar/";
				Uri uri = new Uri(imagesPath + Map.Name + ".png", UriKind.RelativeOrAbsolute);
				StreamResourceInfo sri = System.Windows.Application.GetResourceStream(uri);
				if (sri != null)
				{
					using (Stream s = sri.Stream)
					{
						return BitmapFactory.FromStream(s);
					}
				}

				return null;
			}
			catch (FileNotFoundException)
			{
				throw new MapUnavailableException(Map.Name);
			}
		}
	}
}
