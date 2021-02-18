using Core;
using Core.Models;
using Core.Models.Source;
using Core.Models.Steam;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls.Dialogs;
using Manager.Messages;
using Manager.Models;
using Manager.Properties;
using Manager.Services;
using Manager.ViewModel.Shared;
using Manager.Views.Demos;
using Manager.Views.Players;
using Manager.Views.Rounds;
using Services.Concrete;
using Services.Concrete.Excel;
using Services.Concrete.Maps;
using Services.Concrete.Movie;
using Services.Interfaces;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Round = Core.Models.Round;

namespace Manager.ViewModel.Demos
{
	public class DemoDetailsViewModel : BaseViewModel
	{
		#region Properties

		private Demo _demo;

		private Demo _previousDemo;

		private Demo _nextDemo;

		private Round _selectedRound;

		/// <summary>
		/// Demos sources available
		/// </summary>
		private List<Source> _sources;

		private readonly IDemosService _demosService;

		private readonly IRoundService _roundService;

		private readonly IDialogService _dialogService;

		private readonly ExcelService _excelService;

		private readonly ISteamService _steamService;

		private readonly ICacheService _cacheService;

		// player selected in the scoreboard
		private Player _selectedPlayer;

		// player selected in the combobox
		private Player _selectedPlayerStats;

		private bool _isLeftSideVisible = Settings.Default.ShowLeftPartDetails;

		private float _progress;

		private CancellationTokenSource _cts;

		private RelayCommand _windowLoadedCommand;

		private RelayCommand _backToHomeCommand;

		private RelayCommand _analyzeDemoCommand;

		private RelayCommand<int> _goToRoundCommand;

		private RelayCommand<Player> _goToPlayerCommand;

		private RelayCommand<Source> _setDemoSourceCommand;

		private RelayCommand<Demo> _heatmapCommand;

		private RelayCommand<Demo> _overviewCommand;

		private RelayCommand<Demo> _goToKillsCommand;

		private RelayCommand<Demo> _goToDemoDamagesCommand;

		private RelayCommand<Demo> _goToDemoFlashbangsCommand;

		private RelayCommand<Demo> _showDemoStuffsCommand;

		private RelayCommand<string> _saveCommentDemoCommand;

		private RelayCommand<string> _addSuspectCommand;

		private RelayCommand<string> _addPlayerToWhitelistCommand;

		private RelayCommand<Round> _watchRoundCommand;

		private RelayCommand<Player> _goToSuspectProfileCommand;

		private RelayCommand<Player> _watchPlayerCommand;

		private RelayCommand<Player> _watchHighlightsCommand;

		private RelayCommand<Player> _watchLowlightsCommand;

		private RelayCommand _exportDemoToExcelCommand;

		private RelayCommand<bool> _showAllPlayersCommand;

		private RelayCommand _toggleLeftSideCommand;

		private RelayCommand<string> _addPlayerToAccountListCommand;

		private RelayCommand<Player> _showPlayerDemosCommand;

		private RelayCommand _exportChatCommand;

		private RelayCommand _goToPreviousDemoCommand;

		private RelayCommand _goToNextDemoCommand;
		private RelayCommand<Demo> _goToMovie;

		private RelayCommand<DemoStatus> _updateDemoStatus;

		private ICollectionView _playersTeam1Collection;

		private ICollectionView _playersTeam2Collection;

		private ICollectionView _roundsCollection;

		#endregion

		#region Accessors

		public Demo Demo
		{
			get => _demo;
			set { Set(() => Demo, ref _demo, value); }
		}

		public Demo PreviousDemo
		{
			get { return _previousDemo; }
			set { Set(() => PreviousDemo, ref _previousDemo, value); }
		}

		public Demo NextDemo
		{
			get { return _nextDemo; }
			set { Set(() => NextDemo, ref _nextDemo, value); }
		}

		public Round SelectedRound
		{
			get { return _selectedRound; }
			set { Set(() => SelectedRound, ref _selectedRound, value); }
		}

		public Player SelectedPlayer
		{
			get { return _selectedPlayer; }
			set { Set(() => SelectedPlayer, ref _selectedPlayer, value); }
		}

		public List<Source> Sources
		{
			get { return _sources; }
			set { Set(() => Sources, ref _sources, value); }
		}

		public Player SelectedPlayerStats
		{
			get { return _selectedPlayerStats; }
			set
			{
				Set(() => SelectedPlayerStats, ref _selectedPlayerStats, value);
				if (value != null) UpdateRoundListStats();
			}
		}

		public bool IsLeftSideVisible
		{
			get { return _isLeftSideVisible; }
			set
			{
				Settings.Default.ShowLeftPartDetails = value;
				Set(() => IsLeftSideVisible, ref _isLeftSideVisible, value);
			}
		}

		public ICollectionView PlayersTeam1Collection
		{
			get { return _playersTeam1Collection; }
			set { Set(() => PlayersTeam1Collection, ref _playersTeam1Collection, value); }
		}

		public ICollectionView RoundsCollection
		{
			get { return _roundsCollection; }
			set { Set(() => RoundsCollection, ref _roundsCollection, value); }
		}

		public ICollectionView PlayersTeam2Collection
		{
			get { return _playersTeam2Collection; }
			set { Set(() => PlayersTeam2Collection, ref _playersTeam2Collection, value); }
		}

		#endregion

		#region Commands

		public RelayCommand WindowLoaded
		{
			get
			{
				return _windowLoadedCommand
					?? (_windowLoadedCommand = new RelayCommand(
					async () =>
					{
						var currentPage = new ViewModelLocator().Main.CurrentPage.CurrentPage;
						IsBusy = true;
						Notification = Properties.Resources.NotificationLoading;
						HasNotification = true;
						// reload whole demo data if an account was selected and the current page was the demos list
						if (Settings.Default.SelectedStatsAccountSteamID != 0 && currentPage is DemoListView && _cacheService.HasDemoInCache(Demo.Id))
							Demo = await _cacheService.GetDemoDataFromCache(Demo.Id);
						await UpdateDemoFromAppArgument();
						await LoadData();
						CommandManager.InvalidateRequerySuggested();
					}));
			}
		}

		/// <summary>
		/// Update the demo's source (from the combobox)
		/// </summary>
		public RelayCommand<Source> SetDemoSourceCommand
		{
			get
			{
				return _setDemoSourceCommand
					?? (_setDemoSourceCommand = new RelayCommand<Source>(
					async source =>
					{
						await _demosService.SetSource(Demo, source.Name);
					}, source => Demo != null && source != null && source.Name != Demo.SourceName));
			}
		}

		/// <summary>
		/// Command to back to the home page
		/// </summary>
		public RelayCommand BackToHomeCommand
		{
			get
			{
				return _backToHomeCommand
					?? (_backToHomeCommand = new RelayCommand(
					() =>
					{
						if (SelectedPlayerStats != null && SelectedPlayerStats.SteamId != 0)
						{
							var settingsViewModel = new ViewModelLocator().Settings;
							settingsViewModel.IsShowAllPlayers = true;
						}
						var mainViewModel = new ViewModelLocator().Main;
						Application.Current.Properties["LastPageViewed"] = mainViewModel.CurrentPage.CurrentPage;
						DemoListView demoListView = new DemoListView();
						mainViewModel.CurrentPage.ShowPage(demoListView);
						Cleanup();
					}));
			}
		}

		/// <summary>
		/// Command to go to round control
		/// </summary>
		public RelayCommand<int> ShowRoundCommand
		{
			get
			{
				return _goToRoundCommand
					?? (_goToRoundCommand = new RelayCommand<int>(
					roundNumber =>
					{
						var roundViewModel = new ViewModelLocator().RoundDetails;
						roundViewModel.RoundNumber = roundNumber;
						roundViewModel.Demo = Demo;
						RoundDetailsView roundView = new RoundDetailsView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(roundView);
					}, roundNumber => !IsBusy && Demo != null
					&& Demo.Source.GetType() != typeof(Pov) && SelectedRound != null));
			}
		}

		/// <summary>
		/// Command to go to player control
		/// </summary>
		public RelayCommand<Player> ShowPlayerCommand
		{
			get
			{
				return _goToPlayerCommand
					?? (_goToPlayerCommand = new RelayCommand<Player>(
					player =>
					{
						var playerViewModel = new ViewModelLocator().PlayerDetails;
						playerViewModel.CurrentPlayer = player;
						playerViewModel.Demo = Demo;
						PlayerDetailsView playerView = new PlayerDetailsView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(playerView);
					}, player => !IsBusy && Demo != null
					&& Demo.Source.GetType() != typeof(Pov) && SelectedPlayer != null));
			}
		}

		/// <summary>
		/// Command to go to heatmap control
		/// </summary>
		public RelayCommand<Demo> HeatmapCommand
		{
			get
			{
				return _heatmapCommand
					?? (_heatmapCommand = new RelayCommand<Demo>(
					async demo =>
					{
						if (!MapService.Maps.Contains(demo.MapName))
						{
							await _dialogService.ShowErrorAsync(Properties.Resources.DialogMapNotSupported, MessageDialogStyle.Affirmative);
							return;
						}
						var heatmapViewModel = new ViewModelLocator().DemoHeatmap;
						heatmapViewModel.Demo = demo;
						DemoHeatmapView heatmapView = new DemoHeatmapView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(heatmapView);
					}, demo => !IsBusy && Demo != null && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		/// <summary>
		/// Command to go to overview control
		/// </summary>
		public RelayCommand<Demo> OverviewCommand
		{
			get
			{
				return _overviewCommand
					?? (_overviewCommand = new RelayCommand<Demo>(
					demo =>
					{
						var overviewViewModel = new ViewModelLocator().DemoOverview;
						overviewViewModel.Demo = demo;
						DemoOverviewView overviewView = new DemoOverviewView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(overviewView);
					}, demo => !IsBusy && Demo != null && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		/// <summary>
		/// Command to go to kills control
		/// </summary>
		public RelayCommand<Demo> GoToKillsCommand
		{
			get
			{
				return _goToKillsCommand
					?? (_goToKillsCommand = new RelayCommand<Demo>(
					demo =>
					{
						var entryKillsViewModel = new ViewModelLocator().DemoKills;
						entryKillsViewModel.Demo = demo;
						DemoKillsView killsView = new DemoKillsView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(killsView);
					}, demo => !IsBusy && Demo != null && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		/// <summary>
		/// Command to go to demo damages control
		/// </summary>
		public RelayCommand<Demo> GoToDemoDamagesCommand
		{
			get
			{
				return _goToDemoDamagesCommand
					?? (_goToDemoDamagesCommand = new RelayCommand<Demo>(
					demo =>
					{
						var demoDamagesViewModel = new ViewModelLocator().DemoDamages;
						demoDamagesViewModel.Demo = demo;
						DemoDamagesView demoDamagesView = new DemoDamagesView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(demoDamagesView);
					}, demo => !IsBusy && Demo != null && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		/// <summary>
		/// Command to go to demo flashbangs stats control
		/// </summary>
		public RelayCommand<Demo> GoToDemoFlashbangsCommand
		{
			get
			{
				return _goToDemoFlashbangsCommand
					?? (_goToDemoFlashbangsCommand = new RelayCommand<Demo>(
					async demo =>
					{
						if (!_cacheService.HasDemoInCache(demo.Id))
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogAnalyzeRequired, MessageDialogStyle.Affirmative);
							return;
						}
						var demoFlashbangsViewModel = new ViewModelLocator().DemoFlashbangs;
						demoFlashbangsViewModel.Demo = demo;
						DemoFlashbangsView demoFlashbangsView = new DemoFlashbangsView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(demoFlashbangsView);
					}, demo => !IsBusy && Demo != null && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		/// <summary>
		/// Command to show movie view
		/// </summary>
		public RelayCommand<Demo> GoToMovie
		{
			get
			{
				return _goToMovie
					   ?? (_goToMovie = new RelayCommand<Demo>(
						   demo =>
						   {
							   DemoMovieViewModel movieViewModel = new ViewModelLocator().DemoMovie;
							   movieViewModel.Demo = demo;
							   var mainViewModel = new ViewModelLocator().Main;
							   DemoMovieView demoMovieView = new DemoMovieView();
							   mainViewModel.CurrentPage.ShowPage(demoMovieView);
						   },
						   demo => Demo != null && !IsBusy));
			}
		}

		public RelayCommand<Demo> ShowDemoStuffsCommand
		{
			get
			{
				return _showDemoStuffsCommand
					?? (_showDemoStuffsCommand = new RelayCommand<Demo>(
					async demo =>
					{
						if (!_cacheService.HasDemoInCache(demo.Id))
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogAnalyzeRequired, MessageDialogStyle.Affirmative);
							return;
						}
						if (!MapService.Maps.Contains(demo.MapName))
						{
							await _dialogService.ShowErrorAsync(Properties.Resources.DialogMapNotSupported, MessageDialogStyle.Affirmative);
							return;
						}
						var demoStuffsViewModel = new ViewModelLocator().DemoStuffs;
						demoStuffsViewModel.Demo = demo;
						DemoStuffsView demoStuffsView = new DemoStuffsView();
						var mainViewModel = new ViewModelLocator().Main;
						mainViewModel.CurrentPage.ShowPage(demoStuffsView);
					}, demo => !IsBusy && Demo != null && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		public RelayCommand<Player> GoToSuspectProfileCommand
		{
			get
			{
				return _goToSuspectProfileCommand
					?? (_goToSuspectProfileCommand = new RelayCommand<Player>(
						player =>
						{
							Process.Start(string.Format(AppSettings.STEAM_COMMUNITY_URL, player.SteamId));
						},
						suspect => SelectedPlayer != null));
			}
		}

		public RelayCommand<Player> WatchPlayerCommand
		{
			get
			{
				return _watchPlayerCommand
					?? (_watchPlayerCommand = new RelayCommand<Player>(
						async player =>
						{
							if (AppSettings.SteamExePath() == null)
							{
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
								return;
							}
							Round firstRound = Demo.Rounds.FirstOrDefault();
							if (firstRound == null) return;
							GameLauncherConfiguration config = Config.BuildGameLauncherConfiguration(Demo);
							config.FocusPlayerSteamId = player.SteamId;
							GameLauncher launcher = new GameLauncher(config);
							launcher.WatchPlayer();
						},
						suspect => SelectedPlayer != null));
			}
		}

		public RelayCommand<Player> WatchHighlights
		{
			get
			{
				return _watchHighlightsCommand
					?? (_watchHighlightsCommand = new RelayCommand<Player>(
						async player =>
						{
							if (AppSettings.SteamExePath() == null)
							{
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
								return;
							}
							GameLauncherConfiguration config = Config.BuildGameLauncherConfiguration(Demo);
							config.FocusPlayerSteamId = player.SteamId;
							GameLauncher launcher = new GameLauncher(config);
							var isPlayerPerspective = await _dialogService.ShowHighLowWatchAsync();
							if (isPlayerPerspective == MessageDialogResult.FirstAuxiliary) return;
							launcher.WatchHighlightDemo(isPlayerPerspective == MessageDialogResult.Affirmative);
						},
						suspect => SelectedPlayer != null));
			}
		}

		public RelayCommand<Player> WatchLowlights
		{
			get
			{
				return _watchLowlightsCommand
					?? (_watchLowlightsCommand = new RelayCommand<Player>(
						async player =>
						{
							if (AppSettings.SteamExePath() == null)
							{
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
								return;
							}
							GameLauncherConfiguration config = Config.BuildGameLauncherConfiguration(Demo);
							config.FocusPlayerSteamId = player.SteamId;
							GameLauncher launcher = new GameLauncher(config);
							var isPlayerPerspective = await _dialogService.ShowHighLowWatchAsync();
							if (isPlayerPerspective == MessageDialogResult.FirstAuxiliary) return;
							launcher.WatchLowlightDemo(isPlayerPerspective == MessageDialogResult.Affirmative);
						},
						suspect => SelectedPlayer != null));
			}
		}

		public RelayCommand ExportDemoToExcelCommand
		{
			get
			{
				return _exportDemoToExcelCommand
					?? (_exportDemoToExcelCommand = new RelayCommand(
						async () =>
						{
							if (Demo.Status == DemoStatus.NAME_DEMO_STATUS_CORRUPTED)
								await _dialogService.ShowDemosCorruptedWarningAsync(new List<Demo> { Demo });

							if (SelectedPlayerStats != null && SelectedPlayerStats.SteamId != 0)
							{
								var isExportFocusedOnPlayer = await _dialogService.ShowExportPlayerStatsAsync(SelectedPlayerStats.Name);
								if (isExportFocusedOnPlayer == MessageDialogResult.Negative) return;
							}

							SaveFileDialog exportDialog = new SaveFileDialog
							{
								FileName = Demo.Name.Substring(0, Demo.Name.Length - 4) + "-export.xlsx",
								Filter = "XLSX file (*.xlsx)|*.xlsx"
							};

							if (exportDialog.ShowDialog() == DialogResult.OK)
							{
								try
								{
									if (!_cacheService.HasDemoInCache(Demo.Id))
									{
										IsBusy = true;
										HasNotification = true;
										Notification = string.Format(Properties.Resources.NotificationAnalyzingDemoForExport, Demo.Name);
										await _demosService.AnalyzeDemo(Demo, CancellationToken.None);
									}
									await _excelService.GenerateXls(Demo, exportDialog.FileName);
								}
								catch (Exception e)
								{
									await HandleAnalyzeException(e);
								}
								finally
								{
									IsBusy = false;
									HasNotification = false;
								}
							}

						},
						() => Demo != null && !IsBusy && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		/// <summary>
		/// Command to start current demo analysis
		/// </summary>
		public RelayCommand AnalyzeDemoCommand
		{
			get
			{
				return _analyzeDemoCommand
					?? (_analyzeDemoCommand = new RelayCommand(
					async () =>
					{
						if (Demo.Status == DemoStatus.NAME_DEMO_STATUS_CORRUPTED)
							await _dialogService.ShowDemosCorruptedWarningAsync(new List<Demo> { Demo });

						Notification = Properties.Resources.NotificationAnalyzing;
						IsBusy = true;
						HasNotification = true;
						new ViewModelLocator().Settings.IsShowAllPlayers = true;

						try
						{
							if (_cts == null)
							{
								_cts = new CancellationTokenSource();
							}

							_progress = 0;
							Demo = await _demosService.AnalyzeDemo(Demo, _cts.Token, HandleAnalyzeProgress);

							await FetchPlayersAvatar();
							UpdatePlayersSort();

							if (AppSettings.IsInternetConnectionAvailable())
							{
								await _demosService.AnalyzeBannedPlayersAsync(Demo);
							}
							await _cacheService.WriteDemoDataCache(Demo);
							await _cacheService.UpdateRankInfoAsync(Demo, Settings.Default.SelectedStatsAccountSteamID);
						}
						catch (Exception e)
						{
							await HandleAnalyzeException(e);
						}

						IsBusy = false;
						HasNotification = false;
						CommandManager.InvalidateRequerySuggested();
					},
					() => !IsBusy && Demo != null && Demo.Source.GetType() != typeof(Pov)));
			}
		}

		/// <summary>
		/// Command to save demo's comment
		/// </summary>
		public RelayCommand<string> SaveCommentDemoCommand
		{
			get
			{
				return _saveCommentDemoCommand
					?? (_saveCommentDemoCommand = new RelayCommand<string>(
					async comment =>
					{
						await _demosService.SaveComment(Demo, comment);
						HasNotification = true;
						Notification = Properties.Resources.NotificationCommentSaved;
						await Task.Delay(5000);
						HasNotification = false;
					}));
			}
		}

		/// <summary>
		/// Command to watch a specific round
		/// </summary>
		public RelayCommand<Round> WatchRoundCommand
		{
			get
			{
				return _watchRoundCommand
					?? (_watchRoundCommand = new RelayCommand<Round>(
					async round =>
					{
						if (AppSettings.SteamExePath() == null)
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
							return;
						}
						GameLauncherConfiguration config = Config.BuildGameLauncherConfiguration(Demo);
						config.FocusPlayerSteamId = Settings.Default.WatchAccountSteamId;
						GameLauncher launcher = new GameLauncher(config);
						launcher.WatchDemoAt(round.Tick);
					},
					round => Demo != null && SelectedRound != null));
			}
		}

		/// <summary>
		/// Command to add a suspect to the list
		/// </summary>
		public RelayCommand<string> AddSuspectCommand
		{
			get
			{
				return _addSuspectCommand
					?? (_addSuspectCommand = new RelayCommand<string>(
						async steamId =>
						{
							Notification = Properties.Resources.NotificationAddingPlayerToSuspectsList;
							HasNotification = true;
							IsBusy = true;

							bool added = await _cacheService.AddSuspectToCache(steamId);
							IsBusy = false;
							if (!added)
							{
								HasNotification = false;
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogPlayerAlreadyInSuspectsList, MessageDialogStyle.Affirmative);
							}

							Notification = Properties.Resources.NotificationPlayedAddedToSuspectsList;
							CommandManager.InvalidateRequerySuggested();
							await Task.Delay(5000);
							HasNotification = false;
						}));
			}
		}

		/// <summary>
		/// Command to add a player to the whitelist
		/// </summary>
		public RelayCommand<string> AddPlayerToWhitelistCommand
		{
			get
			{
				return _addPlayerToWhitelistCommand
					?? (_addPlayerToWhitelistCommand = new RelayCommand<string>(
						async steamId =>
						{
							HasNotification = true;
							IsBusy = true;
							Notification = Properties.Resources.NotificationAddingPlayerToWhitelist;

							bool added = await _cacheService.AddPlayerToWhitelist(steamId);
							IsBusy = false;
							if (!added)
							{
								HasNotification = false;
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogPlayerAlreadyInSuspectWhitelist, MessageDialogStyle.Affirmative);
							}

							Notification = Properties.Resources.NotificationPlayerAddedToWhitelist;
							CommandManager.InvalidateRequerySuggested();
							await Task.Delay(5000);
							HasNotification = false;
						}));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle specific player's stats is clicked
		/// </summary>
		public RelayCommand<bool> ShowAllPlayersCommand
		{
			get
			{
				return _showAllPlayersCommand
					?? (_showAllPlayersCommand = new RelayCommand<bool>(
						isChecked =>
						{
							var settingsViewModel = new ViewModelLocator().Settings;
							if (!isChecked)
								SelectedPlayerStats = Demo.Players[0];
							else
								SelectedPlayerStats = null;
							settingsViewModel.IsShowAllPlayers = isChecked;
						},
						isChecked => !IsBusy && Demo != null && Demo.Players.Any()));
			}
		}

		/// <summary>
		/// Command to go to toggle the left side of the view
		/// </summary>
		public RelayCommand ToggleLeftSideCommand
		{
			get
			{
				return _toggleLeftSideCommand
					?? (_toggleLeftSideCommand = new RelayCommand(
					() =>
					{
						IsLeftSideVisible = !IsLeftSideVisible;
					}));
			}
		}

		/// <summary>
		/// Command to add a player to accounts list
		/// </summary>
		public RelayCommand<string> AddPlayerToAccountListCommand
		{
			get
			{
				return _addPlayerToAccountListCommand
					?? (_addPlayerToAccountListCommand = new RelayCommand<string>(
					async steamId =>
					{
						Notification = Properties.Resources.NotificationAddingPlayerToAccountsList;
						HasNotification = true;
						IsBusy = true;

						bool added = false;
						try
						{
							Account account = new Account
							{
								SteamId = steamId
							};

							if (AppSettings.IsInternetConnectionAvailable())
							{
								Suspect player = await _steamService.GetBanStatusForUser(steamId);
								account.Name = player != null ? player.Nickname : steamId;
							}
							else
							{
								account.Name = steamId;
							}

							added = await _cacheService.AddAccountAsync(account);
							IsBusy = false;
							if (!added)
							{
								HasNotification = false;
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogPlayerAlreadyInAccountsList, MessageDialogStyle.Affirmative);
							}
							else
							{
								var settingsViewModel = new ViewModelLocator().Settings;
								settingsViewModel.Accounts.Add(account);
							}
						}
						catch (Exception e)
						{
							Logger.Instance.Log(e);
							await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorWhileRetrievingPlayerInformation, MessageDialogStyle.Affirmative);
						}

						if (added) Notification = Properties.Resources.NotificationPlayerAddedToAccountsList;
						CommandManager.InvalidateRequerySuggested();
						if (added) await Task.Delay(5000);
						HasNotification = false;
					}));
			}
		}

		/// <summary>
		/// Command to display demos within selected player has played
		/// </summary>
		public RelayCommand<Player> ShowPlayerDemosCommand
		{
			get
			{
				return _showPlayerDemosCommand
					?? (_showPlayerDemosCommand = new RelayCommand<Player>(
						async player =>
						{
							IsBusy = true;
							HasNotification = true;
							Notification = Properties.Resources.NotificationSearchingDemosForPlayer;
							List<Demo> demos = await _demosService.GetDemosPlayer(player.SteamId.ToString());
							IsBusy = false;
							HasNotification = false;
							if (!demos.Any())
							{
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogNoDemosFoundForPlayer, MessageDialogStyle.Affirmative);
								return;
							}

							var demoListViewModel = new ViewModelLocator().DemoList;
							demoListViewModel.SelectedDemos.Clear();
							demoListViewModel.Demos.Clear();
							foreach (Demo demo in demos)
							{
								demoListViewModel.Demos.Add(demo);
							}
							demoListViewModel.DataGridDemosCollection.Refresh();

							var mainViewModel = new ViewModelLocator().Main;
							Application.Current.Properties["LastPageViewed"] = mainViewModel.CurrentPage.CurrentPage;
							DemoListView demoListView = new DemoListView();
							mainViewModel.CurrentPage.ShowPage(demoListView);
						},
						player => SelectedPlayer != null));
			}
		}

		public RelayCommand GoToPreviousDemoCommand
		{
			get
			{
				return _goToPreviousDemoCommand
					?? (_goToPreviousDemoCommand = new RelayCommand(
						async () =>
						{
							Demo = PreviousDemo;
							SelectedPlayerStats = null;
							await LoadData();
						},
						() => PreviousDemo != null && !IsBusy));
			}
		}

		public RelayCommand GoToNextDemoCommand
		{
			get
			{
				return _goToNextDemoCommand
					?? (_goToNextDemoCommand = new RelayCommand(
						async () =>
						{
							Demo = NextDemo;
							SelectedPlayerStats = null;
							await LoadData();
						},
						() => NextDemo != null && !IsBusy));
			}
		}

		public RelayCommand ExportChatCommand
		{
			get
			{
				return _exportChatCommand
					?? (_exportChatCommand = new RelayCommand(
					async () =>
					{
						if (Demo.Status == DemoStatus.NAME_DEMO_STATUS_CORRUPTED)
							await _dialogService.ShowDemosCorruptedWarningAsync(new List<Demo> { Demo });

						SaveFileDialog exportDialog = new SaveFileDialog
						{
							FileName = Demo.Name.Substring(0, Demo.Name.Length - 4) + "-chat.txt",
							Filter = "Text file (*.txt)|*.txt"
						};

						if (exportDialog.ShowDialog() == DialogResult.OK)
						{
							if (!_cacheService.HasDemoInCache(Demo.Id))
							{
								try
								{
									Notification = Properties.Resources.NotificationAnalyzing;
									IsBusy = true;
									HasNotification = true;

									if (_cts == null)
									{
										_cts = new CancellationTokenSource();
									}
									await _demosService.AnalyzeDemo(Demo, _cts.Token);

									if (AppSettings.IsInternetConnectionAvailable())
									{
										await _demosService.AnalyzeBannedPlayersAsync(Demo);
									}
									await _cacheService.WriteDemoDataCache(Demo);
								}
								catch (Exception e)
								{
									await HandleAnalyzeException(e);
								}
								finally
								{
									IsBusy = false;
									HasNotification = false;
								}
							}

							if (Demo.ChatMessageList.Any())
							{
								_demosService.WriteChatFile(Demo, exportDialog.FileName);
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogChatFileCreated, MessageDialogStyle.Affirmative);
							}
							else
							{
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogNoChatFound, MessageDialogStyle.Affirmative);
							}
						}
					}));
			}
		}

		/// <summary>
		/// Update the demo's status (from the combobox).
		/// </summary>
		public RelayCommand<DemoStatus> UpdateDemoStatus
		{
			get
			{
				return _updateDemoStatus
					   ?? (_updateDemoStatus = new RelayCommand<DemoStatus>(
						   async status =>
						   {
							   await _demosService.SaveStatus(Demo, status.Name);
						   }, (status) => Demo != null && status != null && status.Name != Demo.Status));
			}
		}

		#endregion

		public DemoDetailsViewModel(
			IDemosService demosService, IDialogService dialogService, ISteamService steamService,
			ICacheService cacheService, ExcelService excelService, IRoundService roundService)
		{
			_demosService = demosService;
			_dialogService = dialogService;
			_steamService = steamService;
			_cacheService = cacheService;
			_excelService = excelService;
			_roundService = roundService;

			Sources = Source.Sources;

			if (IsInDesignMode)
			{
				DispatcherHelper.CheckBeginInvokeOnUI(async () =>
				{
					Demo = await _cacheService.GetDemoDataFromCache(string.Empty);
					PlayersTeam1Collection = CollectionViewSource.GetDefaultView(Demo.TeamCT.Players);
					PlayersTeam2Collection = CollectionViewSource.GetDefaultView(Demo.TeamT.Players);
					RoundsCollection = CollectionViewSource.GetDefaultView(Demo.Rounds);
				});
			}

			Messenger.Default.Register<LoadDemoFromAppArgument>(this, HandleLoadFromArgumentMessage);
		}

		private async void HandleLoadFromArgumentMessage(LoadDemoFromAppArgument m)
		{
			await UpdateDemoFromAppArgument();
		}

		private async Task HandleAnalyzeException(Exception e)
		{
			Logger.Instance.Log(e);
			await _dialogService.ShowErrorAsync(string.Format(Properties.Resources.DialogErrorWhileAnalyzingDemo, Demo.Name, AppSettings.APP_WEBSITE), MessageDialogStyle.Affirmative);
			if (Demo.Duration == 0.0) // invalid header
				Demo.Status = DemoStatus.NAME_DEMO_STATUS_CORRUPTED;
			else
				Demo.Status = DemoStatus.NAME_DEMO_STATUS_ERROR;
			await _cacheService.WriteDemoDataCache(Demo);
		}

		public override void Cleanup()
		{
			base.Cleanup();
			SelectedPlayer = null;
			PlayersTeam1Collection = null;
			PlayersTeam2Collection = null;
			RoundsCollection = null;
			SelectedRound = null;
			SelectedPlayerStats = null;
		}

		private async void UpdateRoundListStats()
		{
			HasNotification = true;
			IsBusy = true;
			Notification = Properties.Resources.NotificationLoading;
			if (SelectedPlayerStats == null && _cacheService.HasDemoInCache(Demo.Id))
			{
				Demo demo = await _cacheService.GetDemoDataFromCache(Demo.Id);
				Demo.Rounds.Clear();
				foreach (Round round in demo.Rounds)
				{
					Demo.Rounds.Add(round);
				}
			}
			else
			{
				foreach (Round round in Demo.Rounds)
				{
					await _roundService.MapRoundValuesToSelectedPlayer(Demo, round, SelectedPlayerStats.SteamId);
				}
			}
			HasNotification = false;
			IsBusy = false;
		}

		private void UpdateDemosPagination()
		{
			ObservableCollection<Demo> demos = new ViewModelLocator().DemoList.Demos;
			int demoIndex = demos.IndexOf(Demo);
			int indexPrevious = demoIndex - 1;
			int indexNext = demoIndex + 1;
			PreviousDemo = demos.ElementAtOrDefault(indexPrevious);
			NextDemo = demos.ElementAtOrDefault(indexNext);
		}

		private async Task LoadData()
		{
			IsBusy = true;
			HasNotification = true;
			Notification = Properties.Resources.NotificationLoading;
			PlayersTeam1Collection = CollectionViewSource.GetDefaultView(Demo.TeamCT.Players);
			PlayersTeam2Collection = CollectionViewSource.GetDefaultView(Demo.TeamT.Players);
			UpdatePlayersSort();
			RoundsCollection = CollectionViewSource.GetDefaultView(Demo.Rounds);
			await FetchPlayersAvatar();
			new ViewModelLocator().Settings.IsShowAllPlayers = true;
			UpdateDemosPagination();
			IsBusy = false;
			HasNotification = false;
		}

		private async Task FetchPlayersAvatar()
		{
			if (AppSettings.IsInternetConnectionAvailable() && Demo.Players.Any())
			{
				IEnumerable<string> steamIdList = Demo.Players.Select(p => p.SteamId.ToString()).Distinct();
				List<PlayerSummary> playerSummaryList = await _steamService.GetUserSummaryAsync(steamIdList.ToList());
				foreach (PlayerSummary playerSummary in playerSummaryList)
				{
					Player player = Demo.Players.FirstOrDefault(p => p.SteamId.ToString() == playerSummary.SteamId);
					if (player != null) player.AvatarUrl = playerSummary.AvatarFull;
				}
			}
		}

		private void UpdatePlayersSort()
		{
			PlayersTeam1Collection.SortDescriptions.Add(new SortDescription("RatingHltv", ListSortDirection.Descending));
			PlayersTeam1Collection.SortDescriptions.Add(new SortDescription("RatingHltv2", ListSortDirection.Descending));
			PlayersTeam2Collection.SortDescriptions.Add(new SortDescription("RatingHltv", ListSortDirection.Descending));
			PlayersTeam2Collection.SortDescriptions.Add(new SortDescription("RatingHltv2", ListSortDirection.Descending));
		}

		/// <summary>
		/// Handle the demo path provided as argument
		/// If a .dem file is added to the application arguments, it should be triggered to update the current demo displayed
		/// </summary>
		/// <returns></returns>
		private async Task UpdateDemoFromAppArgument()
		{
			if (!string.IsNullOrEmpty(App.DemoFilePath))
			{
				Demo = await _demosService.GetDemoHeaderAsync(App.DemoFilePath);
				if (_cacheService.HasDemoInCache(Demo.Id))
				{
					_demo = await _cacheService.GetDemoDataFromCache(Demo.Id);
				}
			}
		}

		private void HandleAnalyzeProgress(string demoId, float value)
		{
			// it's time consuming, we don't want to update at each events only when the rounded value has changed
			if (value < 0 || value > 1) return;
			value = (float)Math.Round(value, 2);
			if (value <= _progress) return;
			_progress = value;

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
			{
				UpdateTaskbarProgressMessage msg = new UpdateTaskbarProgressMessage
				{
					Value = value,
				};
				Messenger.Default.Send(msg);
			}));
		}
	}
}
