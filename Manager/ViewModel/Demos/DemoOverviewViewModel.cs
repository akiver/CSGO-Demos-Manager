using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using Core.Models;
using Core.Models.Events;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;
using Manager.Models;
using Manager.Properties;
using Manager.Services;
using Manager.ViewModel.Shared;
using Manager.Views.Demos;
using Services.Concrete;
using Services.Exceptions.Map;
using Services.Interfaces;
using Demo = Core.Models.Demo;
using MediaColor = System.Windows.Media.Color;
using Round = Core.Models.Round;

namespace Manager.ViewModel.Demos
{
	public class DemoOverviewViewModel : BaseViewModel
	{
		#region Properties

		private readonly IMapService _mapService;

		private Demo _demo;

		private Round _selectedRound;

		private Player _selectedPlayer;

		private bool _isPlaying;

		private bool _isPaused;

		private bool _isLogOnlyKills = Settings.Default.LogOnlyKillOnOverview;

		private int _volume = 10;

		private WriteableBitmap _writeableBitmapOverview;

		private WriteableBitmap _writeableBitmapWeapon;

		private WriteableBitmap _writeableBitmapKill;

		private WriteableBitmap _writeableBitmapSmoke;

		private WriteableBitmap _writeableBitmapFlashbang;

		private WriteableBitmap _writeableBitmapHegrenade;

		private WriteableBitmap _writeableBitmapDecoy;

		private WriteableBitmap _writeableBitmapMolotov;

		private WriteableBitmap _writeableBitmapPlayerMarker;

		private TimeSpan LastRenderTime { get; set; }

		private List<List<PositionPoint>> Points { get; set; }

		private ObservableCollection<BaseEvent> _events = new ObservableCollection<BaseEvent>();

		private List<ComboboxSelector> _teamSelectors = new List<ComboboxSelector>();

		private ObservableCollection<PlayerColor> _playersColors = new ObservableCollection<PlayerColor>();

		private ComboboxSelector _currentTeamSelector;

		private readonly IDialogService _dialogService;

		private readonly IDemosService _demoService;

		private DrawService _drawService;

		private RelayCommand _playCommand;

		private RelayCommand _stopCommand;

		private RelayCommand _pauseCommand;

		private RelayCommand<Demo> _backToDemoDetailsCommand;

		private CancellationTokenSource _cts;

		private double FrameLimiter
		{
			get
			{
				if (Demo.Tickrate > 0 && Demo.Tickrate <= 17)
				{
					return 16 * _frameLimiterMultiplier;
				}
				if (Demo.Tickrate > 17 && Demo.Tickrate <= 33)
				{
					return 47 * _frameLimiterMultiplier;
				}
				if (Demo.Tickrate > 33 && Demo.Tickrate <= 44)
				{
					if (_frameLimiterMultiplier > 1) _frameLimiterMultiplier *= 1.5;
					return 57 * _frameLimiterMultiplier;
				}
				if (_frameLimiterMultiplier > 1) _frameLimiterMultiplier *= 2;
				return 70 * _frameLimiterMultiplier;
			}
		}

		private double _frameLimiterMultiplier = 1;

		#endregion

		#region Accessors

		public Demo Demo
		{
			get => _demo;
			set { Set(() => Demo, ref _demo, value); }
		}

		public WriteableBitmap WriteableBitmapOverview
		{
			get { return _writeableBitmapOverview; }
			set { Set(() => WriteableBitmapOverview, ref _writeableBitmapOverview, value); }
		}

		public WriteableBitmap WriteableBitmapWeapon
		{
			get { return _writeableBitmapWeapon; }
			set { Set(() => WriteableBitmapWeapon, ref _writeableBitmapWeapon, value); }
		}

		public WriteableBitmap WriteableBitmapKill
		{
			get { return _writeableBitmapKill; }
			set { Set(() => WriteableBitmapKill, ref _writeableBitmapKill, value); }
		}

		public WriteableBitmap WriteableBitmapSmoke
		{
			get { return _writeableBitmapSmoke; }
			set { Set(() => WriteableBitmapSmoke, ref _writeableBitmapSmoke, value); }
		}

		public WriteableBitmap WriteableBitmapFlashbang
		{
			get { return _writeableBitmapFlashbang; }
			set { Set(() => WriteableBitmapFlashbang, ref _writeableBitmapFlashbang, value); }
		}

		public WriteableBitmap WriteableBitmapHegrenade
		{
			get { return _writeableBitmapHegrenade; }
			set { Set(() => WriteableBitmapHegrenade, ref _writeableBitmapHegrenade, value); }
		}

		public WriteableBitmap WriteableBitmapDecoy
		{
			get { return _writeableBitmapDecoy; }
			set { Set(() => WriteableBitmapDecoy, ref _writeableBitmapDecoy, value); }
		}

		public WriteableBitmap WriteableBitmapMolotov
		{
			get { return _writeableBitmapMolotov; }
			set { Set(() => WriteableBitmapMolotov, ref _writeableBitmapMolotov, value); }
		}

		public WriteableBitmap WriteableBitmapPlayerMarker
		{
			get { return _writeableBitmapPlayerMarker; }
			set { Set(() => WriteableBitmapPlayerMarker, ref _writeableBitmapPlayerMarker, value); }
		}

		public ObservableCollection<BaseEvent> Events
		{
			get { return _events; }
			set { Set(() => Events, ref _events, value); }
		}

		public ObservableCollection<PlayerColor> PlayersColor
		{
			get { return _playersColors; }
			set { Set(() => PlayersColor, ref _playersColors, value); }
		}

		public Round SelectedRound
		{
			get { return _selectedRound; }
			set { Set(() => SelectedRound, ref _selectedRound, value); }
		}

		public Player SelectedPlayer
		{
			get { return _selectedPlayer; }
			set
			{
				Set(() => SelectedPlayer, ref _selectedPlayer, value);
				if (value == null) return;
				if (CurrentTeamSelector != null) CurrentTeamSelector = null;
			}
		}

		public ComboboxSelector CurrentTeamSelector
		{
			get { return _currentTeamSelector; }
			set
			{
				Set(() => CurrentTeamSelector, ref _currentTeamSelector, value);
				if (value == null) return;
				if (SelectedPlayer != null) SelectedPlayer = null;
			}
		}

		public List<ComboboxSelector> TeamSelectors
		{
			get { return _teamSelectors; }
			set { Set(() => TeamSelectors, ref _teamSelectors, value); }
		}

		public int Volume
		{
			get { return _volume; }
			set
			{
				Set(() => Volume, ref _volume, value);
				SoundService.SetVolume(value);
			}
		}

		public double FrameLimiterMultiplier
		{
			get { return _frameLimiterMultiplier; }
			set
			{
				Set(() => FrameLimiterMultiplier, ref _frameLimiterMultiplier, value);
				RaisePropertyChanged(() => FrameLimiter);
			}
		}

		public bool IsPlaying
		{
			get { return _isPlaying; }
			set { Set(() => IsPlaying, ref _isPlaying, value); }
		}

		public bool IsPaused
		{
			get { return _isPaused; }
			set { Set(() => IsPaused, ref _isPaused, value); }
		}

		public bool IsLogOnlyKills
		{
			get { return _isLogOnlyKills; }
			set
			{
				Settings.Default.LogOnlyKillOnOverview = value;
				Set(() => IsLogOnlyKills, ref _isLogOnlyKills, value);
			}
		}

		#endregion

		#region Commands

		/// <summary>
		/// Command to start animation
		/// </summary>
		public RelayCommand PlayCommand
		{
			get
			{
				return _playCommand
					?? (_playCommand = new RelayCommand(
					async () =>
					{
						if (IsPaused)
						{
							Notification = Properties.Resources.NotificationPlaying;
							IsPlaying = true;
							IsPaused = false;

							// Resume render loop
							CompositionTarget.Rendering += CompositionTarget_Rendering;
							return;
						}

						IsBusy = true;
						Notification = Properties.Resources.NotificationGeneratingData;

						try
						{
							if (_cts == null)
							{
								_cts = new CancellationTokenSource();
							}

							PlayersColor.Clear();
							Events.Clear();

							_mapService.InitMap(Demo);
							_drawService = new DrawService(_mapService)
							{
								UseSimpleRadar = Settings.Default.UseSimpleRadar,
							};

							Player player = SelectedPlayer;
							Round round = SelectedRound;

							// Analyze demos to get player's positions
							Demo = await _demoService.AnalyzePlayersPosition(Demo, _cts.Token);

							// Get back selection
							SelectedPlayer = player;
							SelectedRound = round;

							// Generate points to draw
							Points = await _drawService.GetPoints(Demo, CurrentTeamSelector.Id, player, round);

							// Set players colors on UI
							foreach (List<PositionPoint> positionPoints in Points)
							{
								System.Drawing.Color col = System.Drawing.Color.FromArgb(positionPoints.First().Color);
								PlayerColor playerColor = new PlayerColor
								{
									Name = positionPoints.First().PlayerName,
                                    Team = positionPoints.First().Team.AsString(),
									Color = new SolidColorBrush(MediaColor.FromRgb(col.R, col.G, col.B))
								};
								if (!PlayersColor.Contains(playerColor)) PlayersColor.Add(playerColor);
							}

                            if (!Points.Any())
							{
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogNoPointsFound, MessageDialogStyle.Affirmative);
								IsBusy = false;
								return;
							}

							InitLayers();

							Notification = Properties.Resources.NotificationPlaying;
							IsPlaying = true;

							CommandManager.InvalidateRequerySuggested();

							// Start render loop
							CompositionTarget.Rendering += CompositionTarget_Rendering;
						}
						catch (Exception e)
						{
							IsPlaying = false;
							IsBusy = false;

							if (!(e is MapException)) Logger.Instance.Log(e);
							if (e is MapException)
							{
								await _dialogService.ShowErrorAsync(e.Message, MessageDialogStyle.Affirmative).ConfigureAwait(false);
							}
						}

					}, () => (!IsBusy && !IsPaused || IsBusy && IsPaused) && (SelectedPlayer != null || CurrentTeamSelector != null) && SelectedRound != null));
			}
		}

		/// <summary>
		/// Command to stop animation
		/// </summary>
		public RelayCommand StopCommand
		{
			get
			{
				return _stopCommand
					?? (_stopCommand = new RelayCommand(
					() =>
					{
						CompositionTarget.Rendering -= CompositionTarget_Rendering;
						IsBusy = false;
						IsPlaying = false;
						IsPaused = false;
						Demo.PositionPoints.Clear();
						Points.Clear();
					}, () => IsBusy && (SelectedPlayer != null || CurrentTeamSelector != null) && SelectedRound != null));
			}
		}

		/// <summary>
		/// Command to pause animation
		/// </summary>
		public RelayCommand PauseCommand
		{
			get
			{
				return _pauseCommand
					?? (_pauseCommand = new RelayCommand(
					() =>
					{
						Notification = Properties.Resources.NotificationPaused;
						IsPlaying = false;
						IsPaused = true;
						CompositionTarget.Rendering -= CompositionTarget_Rendering;
					}, () => IsPlaying));
			}
		}

		/// <summary>
		/// Command to back to details view
		/// </summary>
		public RelayCommand<Demo> BackToDemoDetailsCommand
		{
			get
			{
				return _backToDemoDetailsCommand
					?? (_backToDemoDetailsCommand = new RelayCommand<Demo>(
						demo =>
						{
							var detailsViewModel = new ViewModelLocator().DemoDetails;
							detailsViewModel.Demo = demo;
							var mainViewModel = new ViewModelLocator().Main;
							DemoDetailsView detailsView = new DemoDetailsView();
							mainViewModel.CurrentPage.ShowPage(detailsView);
							Cleanup();
						},
						demo => Demo != null && !IsBusy));
			}
		}

		#endregion

		public DemoOverviewViewModel(IDialogService dialogService, IDemosService demoService, IMapService mapService)
		{
			_dialogService = dialogService;
			_demoService = demoService;
			_mapService = mapService;
			TeamSelectors.Add(new ComboboxSelector("CT", Properties.Resources.CounterTerrorists));
			TeamSelectors.Add(new ComboboxSelector("T", Properties.Resources.Terrorists));
			TeamSelectors.Add(new ComboboxSelector("BOTH", Properties.Resources.Both));
		}

		/// <summary>
		/// Render loop
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			TimeSpan timeSinceLastRender = DateTime.Now.TimeOfDay - LastRenderTime;

			if (timeSinceLastRender.TotalSeconds < (1.0 / FrameLimiter)) return;

			LastRenderTime = DateTime.Now.TimeOfDay;

			// Stop animation if there is no more points
			if (!Points.Any())
			{
				CompositionTarget.Rendering -= CompositionTarget_Rendering;
				IsBusy = false;
				IsPlaying = false;
				Demo.PositionPoints.Clear();
				Points.Clear();
				CommandManager.InvalidateRequerySuggested();
				return;
			}

			// Draw points for each player
			foreach (List<PositionPoint> positionPoints in Points)
			{
				if (positionPoints.Any())
				{
					// get the first point of the list
					PositionPoint positionPoint = positionPoints.First();

					// If there is an event at this point, draw it
					if (positionPoint.Event != null)
					{
						if (!string.IsNullOrWhiteSpace(positionPoint.Event.Message))
						{
							_events.Add(positionPoint.Event);
						}

						_drawService.DrawEvent(positionPoint);
					}
					else
					{
						// Draw it
						_drawService.DrawPixel(positionPoint);
						_drawService.DrawPlayerMarker(positionPoint);
					}

					// Remove this point
					positionPoints.Remove(positionPoint);

					// If there is no more points remove the list from all the lists
					if (!positionPoints.Any())
					{
						Points.Remove(positionPoints);
						break;
					}
				}
			}
		}

		public override void Cleanup()
		{
			base.Cleanup();
			WriteableBitmapDecoy = null;
			WriteableBitmapFlashbang = null;
			WriteableBitmapHegrenade = null;
			WriteableBitmapKill = null;
			WriteableBitmapMolotov = null;
			WriteableBitmapOverview = null;
			WriteableBitmapPlayerMarker = null;
			WriteableBitmapSmoke = null;
			WriteableBitmapWeapon = null;
			FrameLimiterMultiplier = 1;
			PlayersColor.Clear();
			Events.Clear();
			SoundService.CloseSounds();
		}

		private void InitLayers()
		{
			WriteableBitmapOverview = _drawService.OverviewLayer;
			WriteableBitmapWeapon = _drawService.WeaponLayer;
			WriteableBitmapKill = _drawService.KillLayer;
			WriteableBitmapSmoke = _drawService.SmokeLayer;
			WriteableBitmapFlashbang = _drawService.FlashbangLayer;
			WriteableBitmapDecoy = _drawService.DecoyLayer;
			WriteableBitmapHegrenade = _drawService.HegrenadeLayer;
			WriteableBitmapMolotov = _drawService.MolotovLayer;
			WriteableBitmapPlayerMarker = _drawService.PlayerMarkerLayer;
		}
	}
}
