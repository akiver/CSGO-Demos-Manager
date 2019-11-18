using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Core;
using Core.Models;
using Core.Models.Events;
using Services.Interfaces;
using Color = System.Windows.Media.Color;
using Demo = Core.Models.Demo;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Round = Core.Models.Round;

namespace Services.Concrete
{
	public class DrawService
	{
		public IMapService MapService { get; set; }

		public WriteableBitmap OverviewLayer { get; set; }

		public WriteableBitmap WeaponLayer { get; set; }

		public WriteableBitmap KillLayer { get; set; }

		public WriteableBitmap SmokeLayer { get; set; }

		public WriteableBitmap FlashbangLayer { get; set; }

		public WriteableBitmap HegrenadeLayer { get; set; }

		public WriteableBitmap DecoyLayer { get; set; }

		public WriteableBitmap MolotovLayer { get; set; }

		public WriteableBitmap PlayerMarkerLayer { get; set; }

		public bool UseSimpleRadar { get; set; } = true;

		private readonly Color[] _colors =
		{
			Color.FromArgb(255, 255, 0, 0),
			Color.FromArgb(255, 0, 255, 0),
			Color.FromArgb(255, 0, 0, 255),
			Color.FromArgb(255, 255, 255, 0),
			Color.FromArgb(255, 0, 255, 255),
			Color.FromArgb(255, 255, 0, 255),
			Color.FromArgb(255, 255, 255, 255),
			Color.FromArgb(255, 0, 0, 0),
			Color.FromArgb(255, 48, 32, 19),
			Color.FromArgb(255, 255, 179, 255)
		};

		public DrawService(IMapService mapService)
		{
			MapService = mapService;
			OverviewLayer = mapService.GetWriteableImage(UseSimpleRadar);
			WeaponLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
			KillLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
			SmokeLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
			FlashbangLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
			HegrenadeLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
			DecoyLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
			MolotovLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
			PlayerMarkerLayer = BitmapFactory.New(MapService.Map.ResolutionX, MapService.Map.ResolutionY);
		}

		/// <summary>
		/// Return PositionPoint for each players determined by selection
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="teamSelector"></param>
		/// <param name="selectedPlayer"></param>
		/// <param name="round"></param>
		/// <returns></returns>
		public async Task<List<List<PositionPoint>>> GetPoints(Demo demo, string teamSelector, Player selectedPlayer, Round round)
		{
			List<List<PositionPoint>> points = new List<List<PositionPoint>>();

			if (teamSelector != null)
			{
				switch (teamSelector)
				{
					case "CT":
						demo.PositionPoints.Reverse();

						foreach (Player playerExtended in demo.Players)
						{
							List<PositionPoint> playerPoints = new List<PositionPoint>();

							for (int i = demo.PositionPoints.Count - 1; i >= 0; i--)
							{
								if (demo.PositionPoints[i].RoundNumber != round.Number) continue;

								// Keep kills from terrorists
								if (demo.PositionPoints[i].Event != null
									&& demo.PositionPoints[i].Team == Side.Terrorist
									&& demo.PositionPoints[i].Event.GetType() == typeof(KillEvent))
								{
									KillEvent e = (KillEvent)demo.PositionPoints[i].Event;
									if (e.KillerSteamId == playerExtended.SteamId)
									{
										playerPoints.Add(demo.PositionPoints[i]);
										demo.PositionPoints.RemoveAt(i);
										continue;
									}
								}

								if (demo.PositionPoints[i].Team != Side.CounterTerrorist) continue;

								// Molotov started
								if (demo.PositionPoints[i].Event != null
									&& demo.PositionPoints[i].Event.GetType() == typeof(MolotovFireStartedEvent))
								{
									MolotovFireStartedEvent e = (MolotovFireStartedEvent)demo.PositionPoints[i].Event;
									if (e.ThrowerSteamId == playerExtended.SteamId)
									{
										playerPoints.Add(demo.PositionPoints[i]);
										demo.PositionPoints.RemoveAt(i);
										continue;
									}
								}

								// Molotov ended
								if (demo.PositionPoints[i].Event != null
									&& demo.PositionPoints[i].Event.GetType() == typeof(MolotovFireEndedEvent))
								{
									MolotovFireEndedEvent e = (MolotovFireEndedEvent)demo.PositionPoints[i].Event;
									if (e.ThrowerSteamId == playerExtended.SteamId)
									{
										playerPoints.Add(demo.PositionPoints[i]);
										demo.PositionPoints.RemoveAt(i);
										continue;
									}
								}

								if (demo.PositionPoints[i].PlayerSteamId != 0
								&& demo.PositionPoints[i].PlayerSteamId == playerExtended.SteamId)
								{
									playerPoints.Add(demo.PositionPoints[i]);
									demo.PositionPoints.RemoveAt(i);
								}
							}
							if (playerPoints.Any()) points.Add(playerPoints);
						}
						break;
					case "T":
						demo.PositionPoints.Reverse();

						foreach (Player playerExtended in demo.Players)
						{
							List<PositionPoint> playerPoints = new List<PositionPoint>();

							for (int i = demo.PositionPoints.Count - 1; i >= 0; i--)
							{
								if (demo.PositionPoints[i].RoundNumber != round.Number) continue;

								// Keep kills from CT
								if (demo.PositionPoints[i].Event != null
									&& demo.PositionPoints[i].Team == Side.CounterTerrorist
									&& demo.PositionPoints[i].Event.GetType() == typeof(KillEvent))
								{
									KillEvent e = (KillEvent)demo.PositionPoints[i].Event;
									if (e.KilledSteamId == playerExtended.SteamId)
									{
										playerPoints.Add(demo.PositionPoints[i]);
										demo.PositionPoints.RemoveAt(i);
										continue;
									}
								}

								if (demo.PositionPoints[i].Team != Side.Terrorist) continue;

								// Molotov started
								if (demo.PositionPoints[i].Event != null
									&& demo.PositionPoints[i].Event.GetType() == typeof(MolotovFireStartedEvent))
								{
									MolotovFireStartedEvent e = (MolotovFireStartedEvent)demo.PositionPoints[i].Event;
									if (e.ThrowerSteamId == playerExtended.SteamId)
									{
										playerPoints.Add(demo.PositionPoints[i]);
										demo.PositionPoints.RemoveAt(i);
										continue;
									}
								}

								// Molotov ended
								if (demo.PositionPoints[i].Event != null
									&& demo.PositionPoints[i].Event.GetType() == typeof(MolotovFireEndedEvent))
								{
									MolotovFireEndedEvent e = (MolotovFireEndedEvent)demo.PositionPoints[i].Event;
									if (e.ThrowerSteamId == playerExtended.SteamId)
									{
										playerPoints.Add(demo.PositionPoints[i]);
										demo.PositionPoints.RemoveAt(i);
										continue;
									}
								}

								if (demo.PositionPoints[i].PlayerSteamId == playerExtended.SteamId)
								{
									playerPoints.Add(demo.PositionPoints[i]);
									demo.PositionPoints.RemoveAt(i);
								}
							}
							if (playerPoints.Any()) points.Add(playerPoints);
						}
						break;
					case "BOTH":
						points.AddRange(
							demo.Players.Select(
								playerExtended => demo.PositionPoints.Where(
									point => point.RoundNumber == round.Number
									&& point.PlayerSteamId == playerExtended.SteamId).ToList())
									.Where(pts => pts.Any()));
						break;
				}
			}

			if (selectedPlayer != null)
			{
				await Task.Run(delegate
				{
					List<PositionPoint> pt = demo.PositionPoints.ToList().Where(
						positionPoint => positionPoint.PlayerSteamId == selectedPlayer.SteamId
						&& positionPoint.RoundNumber == round.Number
						|| (positionPoint.Event != null
						&& positionPoint.Event.GetType() == typeof(KillEvent))
						&& positionPoint.RoundNumber == round.Number).ToList();
					if (pt.Any()) points.Add(pt);
				});
			}

			// Set players color
			await Task.Run(delegate
			{
				int index = 0;
				foreach (List<PositionPoint> positionPoints in points.ToList())
				{
					foreach (PositionPoint positionPoint in positionPoints)
					{
						positionPoint.X = MapService.CalculatePointToResolutionX(positionPoint.X);
						positionPoint.Y = MapService.CalculatePointToResolutionY(positionPoint.Y);
						positionPoint.Color = ColorToInt(_colors[index]);
					}
					index++;
				}
			});

			return points;
		}

		/// <summary>
		/// Draw a pixel on main overview
		/// </summary>
		/// <param name="positionPoint"></param>
		public void DrawPixel(PositionPoint positionPoint)
		{
			OverviewLayer.SetPixel((int)positionPoint.X, (int)positionPoint.Y, positionPoint.Color);
		}

		/// <summary>
		/// Draw player's marker
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawPlayerMarker(PositionPoint positionPoint)
		{
			try
			{
                if (positionPoint.Team == Side.CounterTerrorist)
                    PlayerMarkerLayer.FillEllipseCentered((int)positionPoint.X, (int)positionPoint.Y, 5, 5, positionPoint.Color);
                else
                    PlayerMarkerLayer.DrawEllipseCentered((int)positionPoint.X, (int)positionPoint.Y, 5, 5, positionPoint.Color);
                //PlayerMarkerLayer.FillQuad((int)positionPoint.X - 2, (int)positionPoint.Y - 2, (int)positionPoint.X + 2, (int)positionPoint.Y - 2, (int)positionPoint.X - 2, (int)positionPoint.Y + 2, (int)positionPoint.X + 2, (int)positionPoint.Y + 2, positionPoint.Color);

                Bitmap icon = null;
				if (positionPoint.PlayerHasBomb)
				{
					icon = GetIconBitmapFromStream("bomb_overview");
					DrawIcon(PlayerMarkerLayer, icon, positionPoint);
				}

				await Task.Delay(200);

                if (positionPoint.Team == Side.CounterTerrorist)
                    PlayerMarkerLayer.FillEllipseCentered((int)positionPoint.X, (int)positionPoint.Y, 5, 5, Colors.Transparent);
                else
                    PlayerMarkerLayer.DrawEllipseCentered((int)positionPoint.X, (int)positionPoint.Y, 5, 5, Colors.Transparent);
                //PlayerMarkerLayer.FillQuad((int)positionPoint.X - 2, (int)positionPoint.Y - 2, (int)positionPoint.X + 2, (int)positionPoint.Y - 2, (int)positionPoint.X - 2, (int)positionPoint.Y + 2, (int)positionPoint.X + 2, (int)positionPoint.Y + 2, Colors.Transparent);

                if (positionPoint.PlayerHasBomb)
				{
					await Task.Delay(200);

					ClearIcon(PlayerMarkerLayer, icon, positionPoint);
					icon?.Dispose();
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw weapon icon
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawWeapon(PositionPoint positionPoint)
		{
			var ev = (WeaponFireEvent)positionPoint.Event;
			Bitmap icon = null;
			switch (ev.Weapon.Name)
			{
				case "Flashbang":
					icon = GetIconBitmapFromStream("flashbang");
					break;
				case "He Grenade":
					icon = GetIconBitmapFromStream("hegrenade");
					break;
				case "Smoke":
					icon = GetIconBitmapFromStream("smokegrenade");
					break;
				case "Decoy":
					icon = GetIconBitmapFromStream("decoy");
					break;
				case "Molotov":
					icon = GetIconBitmapFromStream("molotov");
					break;
				case "Incendiary":
					icon = GetIconBitmapFromStream("incendiary");
					break;
			}

			if (icon != null)
			{
				try
				{
					DrawIcon(WeaponLayer, icon, positionPoint);

					await Task.Delay(2000);

					ClearIcon(WeaponLayer, icon, positionPoint);

					icon.Dispose();
				}
				catch (Exception e)
				{
					Logger.Instance.Log(e);
				}
			}
		}

		/// <summary>
		/// Draw icon when a flashbang exploded
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawFlashbangExploded(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("flashbang_exploded");
				DrawIcon(FlashbangLayer, icon, positionPoint);

				await Task.Delay(2000);

				ClearIcon(FlashbangLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw successive icons to animate smoke started
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawSmokeStarted(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("smoke_start");
				DrawIcon(SmokeLayer, icon, positionPoint);

				await Task.Delay(1000);

				icon = GetIconBitmapFromStream("smoke");
				DrawIcon(SmokeLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Clear smoke icon when a smoke ended
		/// </summary>
		/// <param name="positionPoint"></param>
		public void DrawSmokeEnded(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("smoke");
				ClearIcon(SmokeLayer, icon, positionPoint);
				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw burn icon when a molo is started burn
		/// </summary>
		/// <param name="positionPoint"></param>
		public void DrawMolotovStarted(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("molotov_burning");
				DrawIcon(MolotovLayer, icon, positionPoint);
				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Clear burn icon when a molotov is no more burning
		/// </summary>
		/// <param name="positionPoint"></param>
		public void DrawMolotovEnded(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("molotov_burning");
				ClearIcon(MolotovLayer, icon, positionPoint);
				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw icon when a hegrenade exploded
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawExplosiveNadeExploded(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("he_exploded");
				DrawIcon(HegrenadeLayer, icon, positionPoint);

				await Task.Delay(2000);

				ClearIcon(HegrenadeLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw icon when a player has been killed
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawKill(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("kill");
				DrawIcon(KillLayer, icon, positionPoint);

				await Task.Delay(2000);

				ClearIcon(KillLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw icon when the bomb has been planted
		/// </summary>
		/// <param name="positionPoint"></param>
		public void DrawBombPlanted(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("bomb");
				DrawIcon(WeaponLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw icon when the bomb has exploded
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawBombExploded(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("bomb");

				ClearIcon(WeaponLayer, icon, positionPoint);

				icon = GetIconBitmapFromStream("bomb_exploded");
				DrawIcon(WeaponLayer, icon, positionPoint);

				await Task.Delay(3000);

				ClearIcon(WeaponLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw icon when the bomb has been defused
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawBombDefused(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("bomb");

				ClearIcon(WeaponLayer, icon, positionPoint);

				icon = GetIconBitmapFromStream("defuser");
				DrawIcon(WeaponLayer, icon, positionPoint);

				await Task.Delay(3000);

				ClearIcon(WeaponLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Draw icon when a decoy start "screaming"
		/// </summary>
		/// <param name="positionPoint"></param>
		public void DrawDecoyStarted(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("decoy_screaming");
				DrawIcon(DecoyLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Undraw an icon when a decoy is over
		/// </summary>
		/// <param name="positionPoint"></param>
		public async void DrawDecoyEnded(PositionPoint positionPoint)
		{
			try
			{
				Bitmap icon = GetIconBitmapFromStream("decoy_screaming");
				ClearIcon(DecoyLayer, icon, positionPoint);

				icon = GetIconBitmapFromStream("decoy_exploded");
				DrawIcon(DecoyLayer, icon, positionPoint);

				await Task.Delay(1000);

				ClearIcon(DecoyLayer, icon, positionPoint);

				icon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		/// <summary>
		/// Undraw a specific icon from the WriteableBitmap layer
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="icon"></param>
		/// <param name="positionPoint"></param>
		private static void ClearIcon(WriteableBitmap layer, Bitmap icon, PositionPoint positionPoint)
		{
			BitmapData data = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height),
			ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

			IntPtr ptr = data.Scan0;

			// Declare an array to hold the bytes of the bitmap. 
			int bytes = Math.Abs(data.Stride) * icon.Height;
			byte[] rgbValues = new byte[bytes];

			// Copy the RGB values into the array.
			Marshal.Copy(ptr, rgbValues, 0, bytes);

			// Clear data
			Array.Clear(rgbValues, 0, rgbValues.Length);

			layer.WritePixels(
				new Int32Rect(0, 0, icon.Width, icon.Height),
				rgbValues,
				data.Stride,
				(int)positionPoint.X - icon.Width / 2,
				(int)positionPoint.Y - icon.Height / 2);

			icon.UnlockBits(data);
		}

		/// <summary>
		/// Draw a specific icon on WriteableBitmapEvents layer
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="icon"></param>
		/// <param name="positionPoint"></param>
		private static void DrawIcon(WriteableBitmap layer, Bitmap icon, PositionPoint positionPoint)
		{
			BitmapData data = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height),
			ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

			IntPtr ptr = data.Scan0;

			// Declare an array to hold the bytes of the bitmap.
			int bytes = Math.Abs(data.Stride) * icon.Height;
			byte[] rgbValues = new byte[bytes];

			// Copy the RGB values into the array.
			Marshal.Copy(ptr, rgbValues, 0, bytes);

			int x = (int)positionPoint.X - icon.Width / 2;
			int y = (int)positionPoint.Y - icon.Height / 2;
			int maxX = (int)layer.Width - icon.Width;
			int maxY = (int)layer.Height - icon.Height;

			if (x > maxX) x = maxX;
			if (y > maxY) y = maxY;

			layer.WritePixels(new Int32Rect(0, 0, icon.Width, icon.Height), rgbValues, data.Stride, x, y);

			icon.UnlockBits(data);
		}

		/// <summary>
		/// Convert System.Windows.Media.Color to int
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		private static int ColorToInt(Color color)
		{
			return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
		}

		public void DrawEvent(PositionPoint positionPoint)
		{
			Type type = positionPoint.Event.GetType();
			if (type == typeof(WeaponFireEvent))
			{
				DrawWeapon(positionPoint);
				WeaponFireEvent e = (WeaponFireEvent)positionPoint.Event;
				SoundService.PlayWeaponFired(positionPoint.Team, e);
			}
			else if (type == typeof(FlashbangExplodedEvent))
			{
				DrawFlashbangExploded(positionPoint);
				SoundService.PlayFlashbangExploded();
			}
			else if (type == typeof(SmokeNadeStartedEvent))
			{
				DrawSmokeStarted(positionPoint);
				SoundService.PlaySmokeDetonated();
			}
			else if (type == typeof(SmokeNadeEndedEvent))
			{
				DrawSmokeEnded(positionPoint);
			}
			else if (type == typeof(ExplosiveNadeExplodedEvent))
			{
				DrawExplosiveNadeExploded(positionPoint);
				SoundService.PlayHeExploded();
			}
			else if (type == typeof(MolotovFireStartedEvent))
			{
				DrawMolotovStarted(positionPoint);
				SoundService.PlayMolotovExploded();
			}
			else if (type == typeof(MolotovFireEndedEvent))
			{
				DrawMolotovEnded(positionPoint);
			}
			else if (type == typeof(KillEvent))
			{
				DrawKill(positionPoint);
				SoundService.PlayPlayerKilled(positionPoint.Team);
			}
			else if (type == typeof(DecoyStartedEvent))
			{
				DrawDecoyStarted(positionPoint);
			}
			else if (type == typeof(DecoyEndedEvent))
			{
				DrawDecoyEnded(positionPoint);
				SoundService.PlayDecoyExploded();
			}
			else if (type == typeof(BombPlantedEvent))
			{
				DrawBombPlanted(positionPoint);
				SoundService.PlayBombPlanted();
			}
			else if (type == typeof(BombExplodedEvent))
			{
				DrawBombExploded(positionPoint);
				SoundService.PlayBombExploded();
			}
			else if (type == typeof(BombDefusedEvent))
			{
				DrawBombDefused(positionPoint);
				SoundService.PlayBombDefused();
			}
		}

		public void DrawStuff(Stuff stuff)
		{
			try
			{
				SmokeLayer.Clear();
				float startX = MapService.CalculatePointToResolutionX(stuff.StartX);
				float startY = MapService.CalculatePointToResolutionY(stuff.StartY);
				float endX = MapService.CalculatePointToResolutionX(stuff.EndX);
				float endY = MapService.CalculatePointToResolutionY(stuff.EndY);

				Bitmap startIcon = null;
				Bitmap endIcon = null;

				switch (stuff.Type)
				{
					case StuffType.SMOKE:
						startIcon = GetIconBitmapFromStream("smokegrenade");
						endIcon = GetIconBitmapFromStream("smoke");
						break;
					case StuffType.FLASHBANG:
						startIcon = GetIconBitmapFromStream("flashbang");
						endIcon = GetIconBitmapFromStream("flashbang_exploded");
						break;
					case StuffType.HE:
						startIcon = GetIconBitmapFromStream("hegrenade");
						endIcon = GetIconBitmapFromStream("he_exploded");
						break;
					case StuffType.MOLOTOV:
					case StuffType.INCENDIARY:
						startIcon = GetIconBitmapFromStream("molotov");
						endIcon = GetIconBitmapFromStream("molotov_burning");
						break;
					case StuffType.DECOY:
						startIcon = GetIconBitmapFromStream("decoy");
						endIcon = GetIconBitmapFromStream("decoy_screaming");
						break;
				}

				if (startIcon == null || endIcon == null) return;

				DrawIcon(SmokeLayer, startIcon, new PositionPoint
				{
					X = startX,
					Y = startY
				});
				DrawIcon(SmokeLayer, endIcon, new PositionPoint
				{
					X = endX,
					Y = endY
				});
				SmokeLayer.DrawLineAa((int)startX, (int)startY, (int)endX, (int)endY, ColorToInt(_colors[0]));

				startIcon.Dispose();
				endIcon.Dispose();
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
		}

		private static Bitmap GetIconBitmapFromStream(string iconName)
		{
			try
			{
				StreamResourceInfo sri = Application.GetResourceStream(new Uri(AppSettings.RESOURCES_URI + "images/icons/" + iconName + ".png", UriKind.RelativeOrAbsolute));
				if (sri != null)
				{
					using (Stream s = sri.Stream)
					{
						return new Bitmap(s);
					}
				}

				return null;
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				return null;
			}
		}
	}
}