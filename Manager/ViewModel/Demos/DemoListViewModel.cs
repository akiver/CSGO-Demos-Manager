#region Imports
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Core;
using Core.Models;
using Core.Models.Source;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls.Dialogs;
using Manager.Messages;
using Manager.Models;
using Manager.Services;
using Manager.ViewModel.Shared;
using Manager.Views.Accounts;
using Manager.Views.Demos;
using Manager.Views.Dialogs;
using Manager.Views.Suspects;
using Newtonsoft.Json;
using Services.Concrete;
using Services.Concrete.Excel;
using Services.Concrete.Movie;
using Services.Concrete.ThirdParties;
using Services.Interfaces;
using Services.Models;
using Services.Models.ThirdParties;
using Application = System.Windows.Application;
using Demo = Core.Models.Demo;
using UserControl = System.Windows.Controls.UserControl;
#endregion

namespace Manager.ViewModel.Demos
{
	public class DemoListViewModel : BaseViewModel, IDisposable
	{

		#region Properties

		private const int MAX_ANALYZE_DEMO_COUNT = 4;

		private readonly IDemosService _demosService;

		private readonly IDialogService _dialogService;

		private readonly ICacheService _cacheService;

		private readonly ISteamService _steamService;

		private readonly IAccountStatsService _accountStatsService;

		private readonly ExcelService _excelService;

		private bool _hasRing;

		private bool _isCancellable;

		private bool _isShowAllFolders;

		private bool _isMainWindowLoaded;

		private Dictionary<string, object> _demoSourcesSelectors = new Dictionary<string, object>();

		private Dictionary<string, object> _demoSourcesSelected = new Dictionary<string, object>();

		/// <summary>
		/// Keep track of demos analyze progression to update the Windows taskbar state
		/// </summary>
		private readonly Dictionary<string, float> _demoProgress = new Dictionary<string, float>();

		private ObservableCollection<Demo> _demos;

		private ObservableCollection<Demo> _selectedDemos;

		private List<DemoStatus> _statuses;

		private ICollectionView _dataGridDemosCollection;

		private string _filterDemoText;

		private RelayCommand<Demo> _showDemoDetailsCommand;

		private RelayCommand _goToAccountStatsCommand;

		private RelayCommand<ObservableCollection<Demo>> _analyzeDemosCommand;

		private RelayCommand<ObservableCollection<Demo>> _deleteDemosCommand;

		private RelayCommand<ObservableCollection<Demo>> _removeDemosFromCacheCommand;

		private RelayCommand<ObservableCollection<Demo>> _exportExcelCommand;

		private RelayCommand _showMoreDemosCommand;

		private RelayCommand _showAllDemosCommand;

		private RelayCommand<Demo> _watchDemoCommand;

		private RelayCommand<Demo> _watchHighlightCommand;

		private RelayCommand<Demo> _watchLowlightCommand;

		private RelayCommand<Demo> _browseToDemoCommand;

		private RelayCommand<Demo> _copyPlaydemoCommand;

		private RelayCommand<ObservableCollection<Demo>> _exportJsonCommand;

		private RelayCommand<Demo> _goToTickCommand;

		private RelayCommand<ObservableCollection<Demo>> _addPlayersToSuspectsListCommand;

		private RelayCommand<bool> _showAllFoldersCommand;

		private RelayCommand<bool> _showAllAccountsCommand;

		private RelayCommand<bool> _showOnlyAccountDemos;

		private RelayCommand _showSuspectsCommand;

		private RelayCommand _refreshListCommand;

		private RelayCommand<string> _saveStatusDemoCommand;

		private RelayCommand<string> _setDemoSourceCommand;

		private RelayCommand<IList> _demosSelectionChangedCommand;

		private RelayCommand _stopAnalyzeCommand;

		private RelayCommand _downloadDemosCommand;

		private RelayCommand _downloadDemoFromShareCodeCommand;

		private RelayCommand<Demo> _copyShareCodeCommand;

		private RelayCommand _showDialogThirdPartySelectionCommand;

		private RelayCommand<string> _showThirdPartyDemoCommand;

		private int _newBannedPlayerCount;

		private ObservableCollection<string> _folders;

		private string _selectedFolder;

		private RelayCommand<UserControl> _showLastUserControlCommand;

		private Rank _lastRankAccountStats;

		private CancellationTokenSource _cts = new CancellationTokenSource();

		private readonly DialogThirdPartySelection _dialogThirdPartySelection = new DialogThirdPartySelection();

		#endregion

		#region Accessors

		public bool HasRing
		{
			get { return _hasRing; }
			set { Set(() => HasRing, ref _hasRing, value); }
		}

		public bool IsCancellable
		{
			get { return _isCancellable; }
			set { Set(() => IsCancellable, ref _isCancellable, value); }
		}

		public bool IsShowAllFolders
		{
			get { return _isShowAllFolders; }
			set { Set(() => IsShowAllFolders, ref _isShowAllFolders, value); }
		}

		public bool IsShowOldDemos => Properties.Settings.Default.ShowOldDemos;

		public bool IsShowPovDemos => Properties.Settings.Default.ShowPovDemos;

		public bool IsShowPugSetupDemos => Properties.Settings.Default.ShowPugSetupDemos;

		public bool IsShowEbotDemos => Properties.Settings.Default.ShowEbotDemos;

		public bool IsShowFaceitDemos => Properties.Settings.Default.ShowFaceitDemos;

		public bool IsShowCevoDemos => Properties.Settings.Default.ShowCevoDemos;

		public bool IsShowEseaDemos => Properties.Settings.Default.ShowEseaDemos;

		public bool IsShowValveDemos => Properties.Settings.Default.ShowValveDemos;

		public bool IsShowPopFlashDemos => Properties.Settings.Default.ShowPopFlashDemos;

		public Dictionary<string, object> DemoSourcesSelectors
		{
			get { return _demoSourcesSelectors; }
			set { Set(() => DemoSourcesSelectors, ref _demoSourcesSelectors, value); }
		}

		public Dictionary<string, object> DemoSourcesSelected
		{
			get { return _demoSourcesSelected; }
			set { Set(() => DemoSourcesSelected, ref _demoSourcesSelected, value); }
		}

		public List<DemoStatus> Statuses
		{
			get { return _statuses; }
			set { Set(() => Statuses, ref _statuses, value); }
		}

		public int NewBannedPlayerCount
		{
			get { return _newBannedPlayerCount; }
			set { Set(() => NewBannedPlayerCount, ref _newBannedPlayerCount, value); }
		}

		public ObservableCollection<Demo> Demos
		{
			get { return _demos; }
			set { Set(() => Demos, ref _demos, value); }
		}

		public ObservableCollection<Demo> SelectedDemos
		{
			get { return _selectedDemos; }
			set { Set(() => SelectedDemos, ref _selectedDemos, value); }
		}

		public ICollectionView DataGridDemosCollection
		{
			get { return _dataGridDemosCollection; }
			set { Set(() => DataGridDemosCollection, ref _dataGridDemosCollection, value); }
		}

		public string FilterDemoText
		{
			get { return _filterDemoText; }
			set
			{
				Set(() => FilterDemoText, ref _filterDemoText, value);
				FilterCollection();
			}
		}

		public ObservableCollection<string> Folders
		{
			get { return _folders; }
			set { Set(() => Folders, ref _folders, value); }
		}

		public string SelectedFolder
		{
			get { return _selectedFolder; }
			set
			{
				Set(() => SelectedFolder, ref _selectedFolder, value);
				Properties.Settings.Default.LastFolder = value;
				if (Properties.Settings.Default.LimitStatsFolder) _cacheService.Filter.Folder = value;
				if (_isMainWindowLoaded)
				{
					DispatcherHelper.CheckBeginInvokeOnUI(
					async () =>
					{
						await LoadDemosHeader();
					});
				}
			}
		}

		public Rank LastRankAccountStats
		{
			get { return _lastRankAccountStats; }
			set { Set(() => LastRankAccountStats, ref _lastRankAccountStats, value); }
		}

		#endregion

		#region Filters

		public bool Filter(object obj)
		{
			var data = obj as Demo;
			if (data != null)
			{
				// Text filter
				if (!string.IsNullOrEmpty(_filterDemoText))
				{
					return data.Name.Contains(_filterDemoText) || data.MapName.Contains(_filterDemoText)
						|| data.Comment.Contains(_filterDemoText) || data.Hostname.Contains(_filterDemoText)
						|| data.ClientName.Contains(_filterDemoText) || data.TeamCT.Name.Contains(_filterDemoText)
						|| data.TeamT.Name.Contains(_filterDemoText) || data.SourceName.Contains(_filterDemoText)
						|| data.DateAsString.Contains(_filterDemoText);
				}

				// POV filter
				if (!IsShowPovDemos && data.SourceName == Pov.NAME) return false;

				// PugSetup filter
				if (!IsShowPugSetupDemos && data.SourceName == PugSetup.NAME) return false;

				// eBot filter
				if (!IsShowEbotDemos && data.SourceName == Ebot.NAME) return false;

				// ESEA filter
				if (!IsShowEseaDemos && data.SourceName == Esea.NAME) return false;

				// Valve filter
				if (!IsShowValveDemos && data.SourceName == Valve.NAME) return false;

				// Faceit filter
				if (!IsShowFaceitDemos && data.SourceName == Faceit.NAME) return false;

				// Cevo filter
				if (!IsShowCevoDemos && data.SourceName == Cevo.NAME) return false;

				// PopFlash filter
				if (!IsShowPopFlashDemos && data.SourceName == PopFlash.NAME) return false;

				// No analyzable demos filter
				if (!IsShowOldDemos && data.Status == DemoStatus.NAME_DEMO_STATUS_ERROR) return false;

				return true;
			}
			return false;
		}

		private void FilterCollection()
		{
			_dataGridDemosCollection?.Refresh();
		}

		#endregion

		#region Commands

		/// <summary>
		/// Command to start demo(s) analysis
		/// </summary>
		public RelayCommand<ObservableCollection<Demo>> AnalyzeDemosCommand
		{
			get
			{
				return _analyzeDemosCommand
					?? (_analyzeDemosCommand = new RelayCommand<ObservableCollection<Demo>>(
					async demos =>
					{
						Demo hasValveDemo = SelectedDemos.FirstOrDefault(d => d.Source.GetType() == typeof(Valve));
						await UpdateSelectedDemosAsync();
						await AnalyzeDemosAsync();
						if (hasValveDemo != null) await RefreshLastRankAccount();
					},
					demos => SelectedDemos != null && SelectedDemos.Count > 0 && SelectedDemos.Count(d => d.Source.GetType() == typeof(Pov)) == 0 && !IsBusy));
			}
		}

		/// <summary>
		/// Command to delete demo(s)
		/// </summary>
		public RelayCommand<ObservableCollection<Demo>> DeleteDemosCommand
		{
			get
			{
				return _deleteDemosCommand
					?? (_deleteDemosCommand = new RelayCommand<ObservableCollection<Demo>>(
					async demos =>
					{
						var delete = await _dialogService.ShowMessageAsync(Properties.Resources.DialogSendToRecycleBinConfimation, MessageDialogStyle.AffirmativeAndNegative);
						if (delete == MessageDialogResult.Negative) return;

						List<Demo> demosNotFound = new List<Demo>();
						foreach (Demo demo in demos)
						{
							bool isDeleted = await _demosService.DeleteDemo(demo);
							if (!isDeleted) demosNotFound.Add(demo);
						}

						if (demosNotFound.Any())
						{
							await _dialogService.ShowDemosNotFoundAsync(demosNotFound);
						}
						else
						{
							await _dialogService.ShowMessageAsync(string.Format(Properties.Resources.DialogDemosSentToRecycleBin, demos.Count), MessageDialogStyle.Affirmative);
						}

						DispatcherHelper.CheckBeginInvokeOnUI(
						async () =>
						{
							await LoadDemosHeader();
						});
					},
					demos => SelectedDemos != null && SelectedDemos.Any() && !IsBusy));
			}
		}

		/// <summary>
		/// Command to remove demo(s) from the cache
		/// </summary>
		public RelayCommand<ObservableCollection<Demo>> RemoveDemosFromCacheCommand
		{
			get
			{
				return _removeDemosFromCacheCommand
					?? (_removeDemosFromCacheCommand = new RelayCommand<ObservableCollection<Demo>>(
					async demos =>
					{
						var delete = await _dialogService.ShowMessageAsync(Properties.Resources.DialogRemoveDemosFromCacheConfirmation, MessageDialogStyle.AffirmativeAndNegative);
						if (delete == MessageDialogResult.Negative) return;

						List<Demo> demosNotFound = new List<Demo>();
						foreach (Demo demo in demos)
						{
							bool isDeleted = await _cacheService.RemoveDemo(demo.Id);
							if (!isDeleted) demosNotFound.Add(demo);
						}

						if (demosNotFound.Any())
						{
							await _dialogService.ShowDemosNotFoundAsync(demosNotFound);
						}
						else
						{
							await _dialogService.ShowMessageAsync(string.Format(Properties.Resources.DialogDemosRemovedFromCache, demos.Count), MessageDialogStyle.Affirmative);
						}

						DispatcherHelper.CheckBeginInvokeOnUI(
						async () =>
						{
							await LoadDemosHeader();
						});
					},
					demos => SelectedDemos != null && SelectedDemos.Any() && !IsBusy));
			}
		}

		public RelayCommand<ObservableCollection<Demo>> ExportExcelCommand
		{
			get
			{
				return _exportExcelCommand
					?? (_exportExcelCommand = new RelayCommand<ObservableCollection<Demo>>(
					async demos =>
					{
						if (Properties.Settings.Default.SelectedStatsAccountSteamID != 0)
						{
							var settingsViewModel = new ViewModelLocator().Settings;
							var isExportFocusedOnPlayer = await _dialogService.ShowExportPlayerStatsAsync(settingsViewModel.SelectedStatsAccount.Name);
							if (isExportFocusedOnPlayer == MessageDialogResult.Negative) return;
						}

						if (demos.Count > 1)
						{
							var isMultipleExport = await _dialogService.ShowExportDemosAsync();
							switch (isMultipleExport)
							{
								case MessageDialogResult.FirstAuxiliary:
									return;
								case MessageDialogResult.Affirmative:
									{
										SaveFileDialog saveExportFileDialog = new SaveFileDialog
										{
											FileName = "export-" + DateTime.Now.ToString("yy-MM-dd-hh-mm-ss") + ".xlsx",
											Filter = "XLSX file (*.xlsx)|*.xlsx"
										};

										if (saveExportFileDialog.ShowDialog() != DialogResult.OK) return;

										try
										{
											IsBusy = true;
											HasRing = true;
											HasNotification = true;
											Notification = Properties.Resources.NotificationAnalyzingDemosForExport;
											IsCancellable = true;

											await AnalyzeDemosAsync(true);
											if (_cts != null)
											{
												foreach (Demo demo in demos)
												{
													if (demo.SourceName == Pov.NAME) continue;
													if (demo.WeaponFired.Count == 0)
														demo.WeaponFired = await _cacheService.GetDemoWeaponFiredAsync(demo);
												}
												await _excelService.GenerateXls(demos.ToList(), saveExportFileDialog.FileName, Properties.Settings.Default.SelectedStatsAccountSteamID);
											}
										}
										catch (Exception e)
										{
											Logger.Instance.Log(e);
											await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorWhileExportingDemos, MessageDialogStyle.Affirmative);
										}
										finally
										{
											IsBusy = false;
											HasNotification = false;
										}
									}
									break;
								default:
									{
										SaveFileDialog saveExportFolderDialog = new SaveFileDialog
										{
											FileName = Properties.Resources.SaveHere,
											OverwritePrompt = false
										};

										DialogResult result = saveExportFolderDialog.ShowDialog();
										if (result != DialogResult.OK) return;
										string directoryPath = Path.GetDirectoryName(saveExportFolderDialog.FileName);
										if (directoryPath != null)
										{
											try
											{
												IsBusy = true;
												HasRing = true;
												HasNotification = true;
												Notification = Properties.Resources.NotificationAnalyzingDemosForExport;
												IsCancellable = true;

												await AnalyzeDemosAsync(true);
												if (_cts != null)
												{
													foreach (Demo demo in demos)
													{
														Notification = string.Format(Properties.Resources.NotificationExportingDemo, demo.Name);
														if (demo.WeaponFired.Count == 0)
															demo.WeaponFired = await _cacheService.GetDemoWeaponFiredAsync(demo);

														string exportFilePath = $"{directoryPath}{Path.DirectorySeparatorChar}{demo.Name.Substring(0, demo.Name.Length - 4)}-export.xlsx";
														await _excelService.GenerateXls(demo, exportFilePath);
													}
												}
											}
											catch (Exception e)
											{
												Logger.Instance.Log(e);
												await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorWhileExportingDemos, MessageDialogStyle.Affirmative);
											}
											finally
											{
												IsBusy = false;
												HasNotification = false;
												IsCancellable = false;
											}
										}
									}
									break;
							}
						}
						else
						{
							Demo demo = demos.First();
							SaveFileDialog saveExportDialog = new SaveFileDialog
							{
								FileName = demo.Name.Substring(0, demo.Name.Length - 4) + "-export.xlsx",
								Filter = "XLSX file (*.xlsx)|*.xlsx"
							};

							if (saveExportDialog.ShowDialog() == DialogResult.OK)
							{
								try
								{
									IsBusy = true;
									HasRing = true;
									HasNotification = true;
									IsCancellable = true;
									Notification = string.Format(Properties.Resources.NotificationAnalyzingDemoForExport, demo.Name);

									await AnalyzeDemosAsync(true);
									if (_cts != null)
									{
										Notification = string.Format(Properties.Resources.NotificationAnalyzingDemoForExport, demo.Name);
										await _excelService.GenerateXls(demo, saveExportDialog.FileName);
									}
								}
								catch (Exception e)
								{
									Logger.Instance.Log(e);
									await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorWhileExportingDemo, MessageDialogStyle.Affirmative);
								}
								finally
								{
									IsBusy = false;
									HasNotification = false;
									IsCancellable = false;
								}
							}
						}
						CommandManager.InvalidateRequerySuggested();
					},
					demos => SelectedDemos != null && SelectedDemos.Any() && !IsBusy));
			}
		}

		/// <summary>
		/// Command to show details view
		/// </summary>
		public RelayCommand<Demo> ShowDemoDetailsCommand
		{
			get
			{
				return _showDemoDetailsCommand
					?? (_showDemoDetailsCommand = new RelayCommand<Demo>(
						async demo =>
						{
							if (!File.Exists(demo.Path))
							{
								await _dialogService.ShowErrorAsync(string.Format(Properties.Resources.DialogErrorDemoNotFound, demo.Name), MessageDialogStyle.Affirmative);
								return;
							}

							// Set the demo
							var detailsViewModel = new ViewModelLocator().DemoDetails;
							detailsViewModel.Demo = demo;

							// Display the UserControl
							var mainViewModel = new ViewModelLocator().Main;
							DemoDetailsView detailsView = new DemoDetailsView();
							mainViewModel.CurrentPage.ShowPage(detailsView);
						},
						demo => demo != null));
			}
		}

		/// <summary>
		/// Command to show account stats view
		/// </summary>
		public RelayCommand GoToAccountStatsCommand
		{
			get
			{
				return _goToAccountStatsCommand
					?? (_goToAccountStatsCommand = new RelayCommand(
						async () =>
						{
							if (Properties.Settings.Default.SelectedStatsAccountSteamID == 0)
							{
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogSelectAccountFirst, MessageDialogStyle.Affirmative);
								return;
							}
							var mainViewModel = new ViewModelLocator().Main;
							AccountOverallView overallView = new AccountOverallView();
							mainViewModel.CurrentPage.ShowPage(overallView);
						}, () => !IsBusy));
			}
		}

		/// <summary>
		/// Command to copy watch command to clipboard
		/// </summary>
		public RelayCommand<Demo> CopyPlaydemoCommand
		{
			get
			{
				return _copyPlaydemoCommand
					?? (_copyPlaydemoCommand = new RelayCommand<Demo>(
						async demo =>
						{
							Clipboard.SetText("playdemo \"" + demo.Path + "\"");
							IsBusy = true;
							HasRing = false;
							HasNotification = true;
							Notification = Properties.Resources.NotificationPlayDemoCommandCopied;
							await Task.Delay(3000);
							HasNotification = false;
							IsBusy = false;
							CommandManager.InvalidateRequerySuggested();
						},
						demo => demo != null));
			}
		}

		/// <summary>
		/// Command to go to a specific tick
		/// </summary>
		public RelayCommand<Demo> GoToTickCommand
		{
			get
			{
				return _goToTickCommand
					?? (_goToTickCommand = new RelayCommand<Demo>(
						async demo =>
						{
							if (AppSettings.SteamExePath() == null)
							{
								await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
								return;
							}
							var result = await _dialogService.ShowInputAsync(Properties.Resources.DialogGoToTick, Properties.Resources.DialogEnterTick);
							if (string.IsNullOrEmpty(result)) return;
							int tick;
							bool isInt = int.TryParse(result, out tick);

							if (isInt)
							{
								try
								{
									GameLauncherConfiguration config = new GameLauncherConfiguration(demo)
									{
										SteamExePath = AppSettings.SteamExePath(),
										Width = Properties.Settings.Default.ResolutionWidth,
										Height = Properties.Settings.Default.ResolutionHeight,
										Fullscreen = Properties.Settings.Default.IsFullscreen,
										EnableHlae = Properties.Settings.Default.EnableHlae,
										CsgoExePath = Properties.Settings.Default.CsgoExePath,
										EnableHlaeConfigParent = Properties.Settings.Default.EnableHlaeConfigParent,
										HlaeConfigParentFolderPath = Properties.Settings.Default.HlaeConfigParentFolderPath,
										HlaeExePath = HlaeService.GetHlaeExePath(),
										LaunchParameters = Properties.Settings.Default.LaunchParameters,
										UseCustomActionsGeneration = Properties.Settings.Default.UseCustomActionsGeneration,
										FocusPlayerSteamId = Properties.Settings.Default.WatchAccountSteamId,
									};
									GameLauncher launcher = new GameLauncher(config);
									launcher.WatchDemoAt(tick);
								}
								catch (Exception e)
								{
									Logger.Instance.Log(e);
									await _dialogService.ShowErrorAsync(e.Message, MessageDialogStyle.Affirmative);
								}
							}
							else
							{
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogInvalidTick, MessageDialogStyle.Affirmative);
							}
						},
						demo => demo != null));
			}
		}

		/// <summary>
		/// Command to add all players from selected demos to suspects list
		/// </summary>
		public RelayCommand<ObservableCollection<Demo>> AddPlayersToSuspectsListCommand
		{
			get
			{
				return _addPlayersToSuspectsListCommand
					?? (_addPlayersToSuspectsListCommand = new RelayCommand<ObservableCollection<Demo>>(
						async demos =>
						{
							if (!AppSettings.IsInternetConnectionAvailable())
							{
								await _dialogService.ShowNoInternetConnectionAsync();
								return;
							}

							await AnalyzeDemosAsync(true);

							IsBusy = true;
							HasNotification = true;
							Notification = Properties.Resources.NotificationAddingSuspects;
							for (int i = 0; i < demos.Count; i++)
							{
								if (demos[i].Players.Any())
								{
									foreach (Player playerExtended in demos[i].Players)
									{
										await _cacheService.AddSuspectToCache(playerExtended.SteamId.ToString());
									}
								}
							}
							IsBusy = false;
							HasNotification = false;

							await RefreshBannedPlayerCount();
						},
						demos => SelectedDemos != null && SelectedDemos.Count > 0 && SelectedDemos.Count(d => d.Source.GetType() == typeof(Pov)) == 0 && !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to display demos from all folders is clicked
		/// </summary>
		public RelayCommand<bool> ShowAllFoldersCommand
		{
			get
			{
				return _showAllFoldersCommand
					?? (_showAllFoldersCommand = new RelayCommand<bool>(
						isChecked =>
						{
							if (isChecked)
							{
								SelectedFolder = null;
							}
							else
							{
								if (Folders.Count > 0)
								{
									if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastFolder))
									{
										SelectedFolder = Properties.Settings.Default.LastFolder;
									}
									else
									{
										SelectedFolder = Folders.ElementAt(0);
									}
								}

							}
							Properties.Settings.Default.ShowAllFolders = isChecked;
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to show all account stats is clicked
		/// </summary>
		public RelayCommand<bool> ShowAllAccountsCommand
		{
			get
			{
				return _showAllAccountsCommand
					?? (_showAllAccountsCommand = new RelayCommand<bool>(
						isChecked =>
						{
							var settingsViewModel = new ViewModelLocator().Settings;
							settingsViewModel.SelectedStatsAccount = !isChecked ? settingsViewModel.Accounts[0] : null;
						},
						isChecked => !IsBusy && new ViewModelLocator().Settings.Accounts.Any()));
			}
		}

		/// <summary>
		/// Command to show / hide only the demos for the selected account
		/// </summary>
		public RelayCommand<bool> ShowOnlyAccountDemos
		{
			get
			{
				return _showOnlyAccountDemos
					?? (_showOnlyAccountDemos = new RelayCommand<bool>(
						async isChecked =>
						{
							new ViewModelLocator().Settings.IsShowOnlyAccountDemos = isChecked;
							_demosService.ShowOnlyAccountDemos = isChecked;
							IsBusy = true;
							HasRing = true;
							IsCancellable = false;
							Notification = Properties.Resources.NotificationLoading;
							await LoadDemosHeader();
							IsBusy = false;
							HasRing = false;
						},
						isChecked => !IsBusy && new ViewModelLocator().Settings.SelectedStatsAccount != null));
			}
		}

		/// <summary>
		/// Browse to demo command
		/// </summary>
		public RelayCommand<Demo> BrowseToDemoCommand
		{
			get
			{
				return _browseToDemoCommand
					?? (_browseToDemoCommand = new RelayCommand<Demo>(
						async demo =>
						{
							if (!File.Exists(demo.Path))
							{
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogDemoNotFound, MessageDialogStyle.Affirmative);
								return;
							}

							string argument = "/select, \"" + demo.Path + "\"";
							Process.Start("explorer.exe", argument);
						},
						demo => demo != null));
			}
		}

		/// <summary>
		/// Command to refresh demos list
		/// </summary>
		public RelayCommand RefreshListCommand
		{
			get
			{
				return _refreshListCommand
					?? (_refreshListCommand = new RelayCommand(
						() =>
						{
							DispatcherHelper.CheckBeginInvokeOnUI(
							async () =>
							{
								await LoadDemosHeader();
							});
						}, () => !IsBusy));
			}
		}

		/// <summary>
		/// Command to show suspects view
		/// </summary>
		public RelayCommand ShowSuspectsCommand
		{
			get
			{
				return _showSuspectsCommand
					?? (_showSuspectsCommand = new RelayCommand(
						async () =>
						{
							if (!AppSettings.IsInternetConnectionAvailable())
							{
								await _dialogService.ShowNoInternetConnectionAsync();
								return;
							}

							var mainViewModel = new ViewModelLocator().Main;
							SuspectListView suspectsView = new SuspectListView();
							mainViewModel.CurrentPage.ShowPage(suspectsView);
							NewBannedPlayerCount = 0;
						}));
			}
		}

		/// <summary>
		/// Command to watch a demo
		/// </summary>
		public RelayCommand<Demo> WatchDemoCommand
		{
			get
			{
				return _watchDemoCommand
					?? (_watchDemoCommand = new RelayCommand<Demo>(
					async demo =>
					{
						if (AppSettings.SteamExePath() == null)
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
							return;
						}
						try
						{
							GameLauncherConfiguration config = new GameLauncherConfiguration(demo)
							{
								SteamExePath = AppSettings.SteamExePath(),
								Width = Properties.Settings.Default.ResolutionWidth,
								Height = Properties.Settings.Default.ResolutionHeight,
								Fullscreen = Properties.Settings.Default.IsFullscreen,
								EnableHlae = Properties.Settings.Default.EnableHlae,
								CsgoExePath = Properties.Settings.Default.CsgoExePath,
								EnableHlaeConfigParent = Properties.Settings.Default.EnableHlaeConfigParent,
								HlaeConfigParentFolderPath = Properties.Settings.Default.HlaeConfigParentFolderPath,
								HlaeExePath = HlaeService.GetHlaeExePath(),
								LaunchParameters = Properties.Settings.Default.LaunchParameters,
								UseCustomActionsGeneration = Properties.Settings.Default.UseCustomActionsGeneration,
								FocusPlayerSteamId = Properties.Settings.Default.WatchAccountSteamId,
							};
							GameLauncher launcher = new GameLauncher(config);
							await launcher.StartGame();
						}
						catch (Exception e)
						{
							Logger.Instance.Log(e);
							await _dialogService.ShowErrorAsync(e.Message, MessageDialogStyle.Affirmative);
						}
					},
					demo => demo != null));
			}
		}

		/// <summary>
		/// Command to watch player's highlights
		/// </summary>
		public RelayCommand<Demo> WatchHighlightCommand
		{
			get
			{
				return _watchHighlightCommand
					?? (_watchHighlightCommand = new RelayCommand<Demo>(
					async demo =>
					{
						await ProcessWatchHighOrLow(demo);
					},
					demo => demo != null));
			}
		}

		/// <summary>
		/// Command to watch player's lowlights
		/// </summary>
		public RelayCommand<Demo> WatchLowlightCommand
		{
			get
			{
				return _watchLowlightCommand
					?? (_watchLowlightCommand = new RelayCommand<Demo>(
					async demo =>
					{
						await ProcessWatchHighOrLow(demo, false);
					},
					demo => demo != null));
			}
		}

		/// <summary>
		/// Command to set the demo's status
		/// </summary>
		public RelayCommand<string> SaveStatusDemoCommand
		{
			get
			{
				return _saveStatusDemoCommand
					?? (_saveStatusDemoCommand = new RelayCommand<string>(
					async status =>
					{
						foreach (Demo demo in SelectedDemos)
						{
							await _demosService.SaveStatus(demo, status);
						}
					},
					status => SelectedDemos != null && SelectedDemos.Count > 0));
			}
		}

		/// <summary>
		/// Command to set the demo's source
		/// </summary>
		public RelayCommand<string> SetDemoSourceCommand
		{
			get
			{
				return _setDemoSourceCommand
					?? (_setDemoSourceCommand = new RelayCommand<string>(
					async source =>
					{
						SelectedDemos = await _demosService.SetSource(SelectedDemos, source);
					},
					source => SelectedDemos != null && SelectedDemos.Count > 0 && !IsBusy));
			}
		}

		/// <summary>
		/// Show the last window viewed
		/// </summary>
		public RelayCommand<UserControl> ShowLastUserControlCommand
		{
			get
			{
				return _showLastUserControlCommand
					?? (_showLastUserControlCommand = new RelayCommand<UserControl>(
					userControl =>
					{
						UserControl lastUserControl = (UserControl)Application.Current.Properties["LastPageViewed"];
						if (lastUserControl != null)
						{
							var mainViewModel = new ViewModelLocator().Main;
							mainViewModel.CurrentPage.ShowPage(lastUserControl);
							Application.Current.Properties["LastPageViewed"] = userControl;
						}
					}));
			}
		}

		/// <summary>
		/// Command fired when a demo selection is done
		/// </summary>
		public RelayCommand<IList> DemosSelectionChangedCommand
		{
			get
			{
				return _demosSelectionChangedCommand
					?? (_demosSelectionChangedCommand = new RelayCommand<IList>(
						demos =>
						{
							if (demos == null) return;
							SelectedDemos.Clear();
							foreach (Demo demo in demos)
								SelectedDemos.Add(demo);
						}));
			}
		}

		/// <summary>
		/// Command to stop current analyze
		/// </summary>
		public RelayCommand StopAnalyzeCommand
		{
			get
			{
				return _stopAnalyzeCommand
					?? (_stopAnalyzeCommand = new RelayCommand(
						async () =>
						{
							if (_cts != null)
							{
								_cts.Cancel();
								_cts = null;
								Notification = Properties.Resources.NotificationCancelling;
								IsCancellable = false;
								// small delay to be sure to send the msg after the last progress event trigerred
								await Task.Delay(100);
								SendTaskbarProgressMessage(0);
								_demoProgress.Clear();
							}
						}, () => IsBusy));
			}
		}

		public RelayCommand ShowMoreDemosCommand
		{
			get
			{
				return _showMoreDemosCommand
					?? (_showMoreDemosCommand = new RelayCommand(
					async () =>
					{
						await PaginateDemos(Properties.Settings.Default.DemosListSize);
					},
					() => !IsBusy));
			}
		}

		public RelayCommand ShowAllDemosCommand
		{
			get
			{
				return _showAllDemosCommand
					?? (_showAllDemosCommand = new RelayCommand(
					async () =>
					{
						await PaginateDemos();
					},
					() => !IsBusy));
			}
		}

		/// <summary>
		/// Command to copy demo's share code
		/// </summary>
		public RelayCommand<Demo> CopyShareCodeCommand
		{
			get
			{
				return _copyShareCodeCommand
					?? (_copyShareCodeCommand = new RelayCommand<Demo>(
						async demo =>
						{
							string shareCode = await _demosService.GetShareCode(demo);
							if (shareCode == string.Empty)
							{
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogDemoShareCodeUnavailable, MessageDialogStyle.Affirmative);
								return;
							}
							Clipboard.SetText(shareCode);
							IsBusy = true;
							HasRing = false;
							HasNotification = true;
							Notification = Properties.Resources.NotificationDemoShareCodeCopied;
							await Task.Delay(3000);
							HasNotification = false;
							IsBusy = false;
							CommandManager.InvalidateRequerySuggested();
						},
						(demo) => demo != null));
			}
		}

		/// <summary>
		/// Command to show dialog third party selection
		/// </summary>
		public RelayCommand ShowDialogThirdPartySelectionCommand
		{
			get
			{
				return _showDialogThirdPartySelectionCommand
					?? (_showDialogThirdPartySelectionCommand = new RelayCommand(
						async () =>
						{
							await _dialogService.ShowCustomDialogAsync(_dialogThirdPartySelection);
						},
						() => SelectedDemos.Count > 0));
			}
		}

		/// <summary>
		/// Command to download last MM demos for the current Steam account
		/// </summary>
		public RelayCommand DownloadDemosCommand
		{
			get
			{
				return _downloadDemosCommand
					?? (_downloadDemosCommand = new RelayCommand(
					async () =>
					{
						bool downloadAllowed = await PreProcessDemoDownload();
						if (!downloadAllowed) return;

						try
						{
							IsBusy = true;
							HasNotification = true;
							HasRing = true;
							IsCancellable = true;
							Notification = Properties.Resources.NotificationRetrievingMatchesData;
							if (_cts == null) _cts = new CancellationTokenSource();
							int result = await _steamService.GenerateMatchListFile(_cts.Token);
							await HandleBoilerResult(result);
						}
						catch (Exception e)
						{
							if (!(e is TaskCanceledException))
							{
								Logger.Instance.Log(e);
							}
						}
						finally
						{
							IsBusy = false;
							HasNotification = false;
							HasRing = false;
							IsCancellable = false;
							CommandManager.InvalidateRequerySuggested();
						}
					},
					() => !IsBusy));
			}
		}

		/// <summary>
		/// Command to download a demo from its share code
		/// </summary>
		public RelayCommand DownloadDemoFromShareCodeCommand
		{
			get
			{
				return _downloadDemoFromShareCodeCommand
					?? (_downloadDemoFromShareCodeCommand = new RelayCommand(
					async () =>
					{
						bool downloadAllowed = await PreProcessDemoDownload();
						if (!downloadAllowed) return;

						string shareCode = await _dialogService.ShowInputAsync(Properties.Resources.DialogTitleDownloadDemoFromShareCode, Properties.Resources.DialogMessageDownloadDemoFromShareCode);
						if (string.IsNullOrEmpty(shareCode)) return;

						try
						{
							CommandManager.InvalidateRequerySuggested();
							IsBusy = true;
							HasNotification = true;
							HasRing = true;
							IsCancellable = true;
							Notification = Properties.Resources.NotificationRetrievingDemoFromShareCode;
							if (_cts == null) _cts = new CancellationTokenSource();

							int result = await _steamService.DownloadDemoFromShareCode(shareCode, _cts.Token);

							await HandleBoilerResult(result, false);
						}
						catch (Exception e)
						{
							if (e is ShareCode.ShareCodePatternException)
							{
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorInvalidShareCode, MessageDialogStyle.Affirmative);
							}
							if (!(e is TaskCanceledException) && !(e is ShareCode.ShareCodePatternException))
							{
								Logger.Instance.Log(e);
							}
						}
						finally
						{
							IsBusy = false;
							HasNotification = false;
							HasRing = false;
							IsCancellable = false;
							CommandManager.InvalidateRequerySuggested();
						}
					},
					() => !IsBusy));
			}
		}

		/// <summary>
		/// Command to export demo data to a JSON file
		/// </summary>
		public RelayCommand<ObservableCollection<Demo>> ExportJsonCommand
		{
			get
			{
				return _exportJsonCommand
					?? (_exportJsonCommand = new RelayCommand<ObservableCollection<Demo>>(
						async demos =>
						{
							FolderBrowserDialog folderDialog = new FolderBrowserDialog
							{
								SelectedPath = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))
							};
							DialogResult result = folderDialog.ShowDialog();
							if (result != DialogResult.OK) return;

							try
							{
								IsBusy = true;
								HasRing = true;
								HasNotification = true;
								Notification = Properties.Resources.NotificationAnalyzingForJsonExport;
								IsCancellable = true;

								string path = Path.GetFullPath(folderDialog.SelectedPath).ToLower();
								await AnalyzeDemosAsync(true);
								if (_cts != null)
								{
									foreach (Demo demo in demos)
										await _cacheService.GenerateJsonAsync(demo, path);
								}
							}
							catch (Exception e)
							{
								Logger.Instance.Log(e);
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorWhileExportingDemos, MessageDialogStyle.Affirmative);
							}
							finally
							{
								IsBusy = false;
								HasRing = false;
								HasNotification = false;
								IsCancellable = false;
							}
						},
						demos => SelectedDemos != null && SelectedDemos.Any()));
			}
		}

		#endregion

		public DemoListViewModel(
			IDemosService demosService, IDialogService dialogService, ISteamService steamService,
			ICacheService cacheService, ExcelService excelService, IAccountStatsService accountStatsService)
		{
			_demosService = demosService;
			_dialogService = dialogService;
			_steamService = steamService;
			_cacheService = cacheService;
			_excelService = excelService;
			_accountStatsService = accountStatsService;
			_demosService.ShowOnlyAccountDemos = Properties.Settings.Default.ShowOnlyAccountDemos;

			if (IsInDesignModeStatic)
			{
				DispatcherHelper.Initialize();
			}

			Statuses = new List<DemoStatus>(DemoStatus.DefaultStatus.Where(s => s.Name != DemoStatus.NAME_DEMO_STATUS_NONE));
			Demos = new ObservableCollection<Demo>();
			SelectedDemos = new ObservableCollection<Demo>();
			DataGridDemosCollection = CollectionViewSource.GetDefaultView(Demos);
			DataGridDemosCollection.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
			DataGridDemosCollection.Filter = Filter;

			Messenger.Default.Register<MainWindowLoadedMessage>(this, HandleMainWindowLoadedMessage);
			Messenger.Default.Register<ComboBoxMultiClosedMessage>(this, HandleComboBoxMultiClosedMessage);
		}

		private void HandleMainWindowLoadedMessage(MainWindowLoadedMessage msg)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(
			async () =>
			{
				LoadDemoSourcesSelection();

				HasNotification = true;
				IsBusy = true;
				HasRing = true;
				Notification = Properties.Resources.NotificationInitCache;
				await _cacheService.InitDemoBasicDataList();
				HasNotification = false;
				IsBusy = false;
				HasRing = false;
				await RefreshFolders();

				IsShowAllFolders = Properties.Settings.Default.ShowAllFolders;

				if (IsShowAllFolders)
				{
					SelectedFolder = null;
				}
				else if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastFolder))
				{
					SelectedFolder = Properties.Settings.Default.LastFolder;
				}
				else if (Folders.Count > 0)
				{
					SelectedFolder = Folders.ElementAt(0);
				}

				await LoadDemosHeader();
				await RefreshLastRankAccount();
				// Refresh suspect banned only if the bot isn't running since it will notify for new bans
				if (AppSettings.IsInternetConnectionAvailable()
				&& !Process.GetProcesses().Where(p => p.ProcessName.StartsWith(AppSettings.BOT_PROCESS_NAME)).ToList().Any())
				{
					await RefreshBannedPlayerCount();
				}

				Messenger.Default.Register<SelectedAccountChangedMessage>(this, HandleSelectedAccountChangedMessage);
				Messenger.Default.Register<SettingsFlyoutClosed>(this, HandleSettingsFlyoutClosedMessage);
				Messenger.Default.Register<UpdateSuspectBannedCountMessage>(this, HandleUpdateSuspectBannedCountMessage);
				Messenger.Default.Register<DownloadDemosMessage>(this, HandleDownloadDemosMessage);
				Messenger.Default.Register<ThirdPartySelected>(this, HandleThirdPartySelectedMessage);

				// Notify the bot that the app is loaded
				Win32Utils.SendMessageToBot(Win32Utils.WM_CSGO_DM_LOADED);

				// Start downloading demos if the app has been started with "download" argument
				if (App.StartUpWindow == "download")
					DownloadDemosCommand.Execute(null);

				_isMainWindowLoaded = true;
			});
		}

		private void LoadDemoSourcesSelection()
		{
			DemoSourcesSelectors = new Dictionary<string, object>
				{
					{Valve.NAME, "Valve"},
					{Ebot.NAME, "eBot"},
					{Cevo.NAME, "CEVO"},
					{Faceit.NAME, "FaceIt"},
					{Esea.NAME, "ESEA"},
					{PopFlash.NAME, "Popflash"},
					{Pov.NAME, "POV"},
					{PugSetup.NAME, "PugSetup"},
					{DemoStatus.NAME_DEMO_STATUS_ERROR, Properties.Resources.NoAnalyzableDemos}
				};

			if (Properties.Settings.Default.ShowValveDemos)
			{
				_demoSourcesSelected.Add(Valve.NAME, "Valve");
			}
			if (Properties.Settings.Default.ShowEbotDemos)
			{
				_demoSourcesSelected.Add(Ebot.NAME, "eBot");
			}
			if (Properties.Settings.Default.ShowCevoDemos)
			{
				_demoSourcesSelected.Add(Cevo.NAME, "CEVO");
			}
			if (Properties.Settings.Default.ShowEseaDemos)
			{
				_demoSourcesSelected.Add(Esea.NAME, "ESEA");
			}
			if (Properties.Settings.Default.ShowFaceitDemos)
			{
				_demoSourcesSelected.Add(Faceit.NAME, "FaceIt");
			}
			if (Properties.Settings.Default.ShowPopFlashDemos)
			{
				_demoSourcesSelected.Add(PopFlash.NAME, "PopFlash");
			}
			if (Properties.Settings.Default.ShowPovDemos)
			{
				_demoSourcesSelected.Add(Pov.NAME, "POV");
			}
			if (Properties.Settings.Default.ShowPugSetupDemos)
			{
				_demoSourcesSelected.Add(PugSetup.NAME, "PugSetup");
			}
			if (Properties.Settings.Default.ShowOldDemos)
			{
				_demoSourcesSelected.Add(DemoStatus.NAME_DEMO_STATUS_ERROR, Properties.Resources.NoAnalyzableDemos);
			}

			DemoSourcesSelected = _demoSourcesSelected;
		}

		private async Task PaginateDemos(int size = 0)
		{
			Notification = Properties.Resources.NotificationLoadingMoreDemos;
			IsBusy = true;
			HasRing = true;
			HasNotification = true;
			List<string> folders = new List<string>();
			if (SelectedFolder != null)
				folders.Add(SelectedFolder);
			else
				folders = Folders.ToList();

			List<Demo> demos = await _demosService.GetDemosHeader(folders, Demos.ToList(), size);
			foreach (Demo demo in demos)
			{
				await _accountStatsService.MapSelectedAccountValues(demo, Properties.Settings.Default.SelectedStatsAccountSteamID);
				Demos.Add(demo);
			}

			IsBusy = false;
			HasNotification = false;
			CommandManager.InvalidateRequerySuggested();
		}

		private static void UpdateDemoSourceSettings(Dictionary<string, object> items, string name)
		{
			switch (name)
			{
				case Valve.NAME:
					Properties.Settings.Default.ShowValveDemos = items.ContainsKey(name);
					break;
				case Ebot.NAME:
					Properties.Settings.Default.ShowEbotDemos = items.ContainsKey(name);
					break;
				case Esea.NAME:
					Properties.Settings.Default.ShowEseaDemos = items.ContainsKey(name);
					break;
				case Cevo.NAME:
					Properties.Settings.Default.ShowCevoDemos = items.ContainsKey(name);
					break;
				case Faceit.NAME:
					Properties.Settings.Default.ShowFaceitDemos = items.ContainsKey(name);
					break;
				case PopFlash.NAME:
					Properties.Settings.Default.ShowPopFlashDemos = items.ContainsKey(name);
					break;
				case Pov.NAME:
					Properties.Settings.Default.ShowPovDemos = items.ContainsKey(name);
					break;
				case PugSetup.NAME:
					Properties.Settings.Default.ShowPugSetupDemos = items.ContainsKey(name);
					break;
				case DemoStatus.NAME_DEMO_STATUS_ERROR:
					Properties.Settings.Default.ShowOldDemos = items.ContainsKey(name);
					break;
			}
		}

		private async void HandleComboBoxMultiClosedMessage(ComboBoxMultiClosedMessage msg)
		{
			foreach (KeyValuePair<string, object> demoSelector in DemoSourcesSelectors)
			{
				UpdateDemoSourceSettings(msg.SelectedItems, demoSelector.Key);
			}

			await LoadDemosHeader();
		}

		/// <summary>
		/// Handle third party selection from dialog
		/// </summary>
		/// <param name="msg"></param>
		private void HandleThirdPartySelectedMessage(ThirdPartySelected msg)
		{
			ProcessSendShareCode(msg.Name);
		}

		/// <summary>
		/// Command to show demo's details on a third party application
		/// </summary>
		public RelayCommand<string> ShowThirdPartyDemoCommand
		{
			get
			{
				return _showThirdPartyDemoCommand
					?? (_showThirdPartyDemoCommand = new RelayCommand<string>(
						async thirdPartyName =>
						{
							// dialog confirmation
							if (Properties.Settings.Default.AskThirdPartyConfirmation)
							{
								var send = await _dialogService.ShowSendShareCodeToThirdPartyConfirmationAsync(thirdPartyName);
								if (send == MessageDialogResult.Negative) return;
								if (send == MessageDialogResult.FirstAuxiliary)
								{
									Properties.Settings.Default.AskThirdPartyConfirmation = false;
									Properties.Settings.Default.Save();
								}
							}

							ProcessSendShareCode(thirdPartyName);
						},
						thirdPartyName => SelectedDemos.Count > 0));
			}
		}

		private void HandleDownloadDemosMessage(DownloadDemosMessage obj)
		{
			DownloadDemosCommand.Execute(null);
		}

		private void HandleUpdateSuspectBannedCountMessage(UpdateSuspectBannedCountMessage m)
		{
			UpdateSuspectBannedCount(m.Count);
		}

		private async void UpdateSuspectBannedCount(int count)
		{
			HasNotification = true;
			HasRing = false;
			IsBusy = true;
			NewBannedPlayerCount += count;
			Notification = string.Format(Properties.Resources.NotificationSuspectsHaveBeenBanned, NewBannedPlayerCount);
			await Task.Delay(5000);
			HasNotification = false;
			IsBusy = false;
		}

		private void HandleSelectedAccountChangedMessage(SelectedAccountChangedMessage msg)
		{
			if (IsBusy) return;
			DispatcherHelper.CheckBeginInvokeOnUI(
			async () =>
			{
				try
				{
					await LoadDemosHeader();
					await RefreshLastRankAccount();
				}
				catch (Exception e)
				{
					Logger.Instance.Log(e);
					await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorWhileRefreshingLastRank, MessageDialogStyle.Affirmative);
				}
			});
		}

		private void HandleSettingsFlyoutClosedMessage(SettingsFlyoutClosed msg)
		{
			if (IsBusy) return;
			DispatcherHelper.CheckBeginInvokeOnUI(
			async () =>
			{
				await RefreshFolders();
				await LoadDemosHeader();
				await RefreshLastRankAccount();
			});
		}

		private async Task RefreshFolders()
		{
			List<string> folders = await _cacheService.GetFoldersAsync();
			Folders = new ObservableCollection<string>(folders);
		}

		private async Task RefreshLastRankAccount()
		{
			if (Properties.Settings.Default.SelectedStatsAccountSteamID != 0)
			{
				HasNotification = true;
				IsBusy = true;
				HasRing = true;
				Notification = Properties.Resources.NotificationSearchingLastRank;
				long steamId = Properties.Settings.Default.SelectedStatsAccountSteamID;
				Rank lastRank = await _cacheService.GetLastRankAsync(steamId);
				if (lastRank == null)
				{
					// if the rank is not in the cache we try to get it from demos
					LastRankAccountStats = await _demosService.GetLastRankAccountStatsAsync(steamId);
				}
				else
				{
					LastRankAccountStats = lastRank;
				}
				UpdateNotificationStatus();
			}
			CommandManager.InvalidateRequerySuggested();
		}

		/// <summary>
		/// Handle share code sending / response to a third party
		/// </summary>
		/// <param name="thirdPartyName"></param>
		private async void ProcessSendShareCode(string thirdPartyName)
		{
			if (!AppSettings.IsInternetConnectionAvailable())
			{
				await _dialogService.ShowNoInternetConnectionAsync();
				return;
			}

			IThirdPartyInterface thirdPartyService = ThirdPartiesServiceFactory.Factory(thirdPartyName);
			foreach (Demo demo in SelectedDemos)
			{
				string shareCode = await _demosService.GetShareCode(demo);
				if (shareCode == string.Empty)
				{
					await _dialogService.ShowErrorAsync(Properties.Resources.DialogShareCodeNotAvailable, MessageDialogStyle.Affirmative);
					continue;
				}
				ThirdPartyData data = await thirdPartyService.SendShareCode(demo, shareCode);
				if (!data.Success)
				{
					await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorWhileSendingShareCode, MessageDialogStyle.Affirmative);
					continue;
				}

				Process.Start(data.DemoUrl);
			}
		}

		private async Task UpdateSelectedDemosAsync()
		{
			if (SelectedDemos.Count == Demos.Count && Demos.Count == Properties.Settings.Default.DemosListSize)
			{
				MessageDialogResult isAllAnalyze = await _dialogService.ShowAnalyzeAllDemosAsync();
				if (isAllAnalyze == MessageDialogResult.FirstAuxiliary) return;
				IsBusy = true;
				HasRing = true;
				HasNotification = true;
				if (isAllAnalyze == MessageDialogResult.Negative)
				{
					Notification = Properties.Resources.NotificationLoadingAllDemos;
					List<string> folders = new List<string>();
					if (SelectedFolder != null)
					{
						folders.Add(SelectedFolder);
					}
					else
					{
						folders = Folders.ToList();
					}
					List<Demo> allDemos = await _demosService.GetDemosHeader(folders);
					foreach (Demo demo in allDemos)
					{
						if (!SelectedDemos.Contains(demo))
						{
							SelectedDemos.Add(demo);
						}
					}
				}
			}
		}

		/// <summary>
		/// Process to analyze multiple demos
		/// </summary>
		/// <returns></returns>
		private async Task AnalyzeDemosAsync(bool checkExists = false)
		{
			List<Demo> corruptedDemos = SelectedDemos.Where(d => d.Status == DemoStatus.NAME_DEMO_STATUS_CORRUPTED).ToList();
			if (corruptedDemos.Count > 0)
				await _dialogService.ShowDemosCorruptedWarningAsync(corruptedDemos);

			CommandManager.InvalidateRequerySuggested();
			_demoProgress.Clear();
			IsBusy = true;
			HasRing = true;
			HasNotification = true;
			IsCancellable = true;
			if (SelectedDemos.Count == 1)
				Notification = string.Format(Properties.Resources.NotificationAnalyzingDemo, SelectedDemos[0].Name);
			else
				Notification = Properties.Resources.NotificationAnalyzingMultipleDemos;

			List<Demo> demosFailed = new List<Demo>();
			List<Demo> demosNotFound = new List<Demo>();

			try
			{
				if (_cts == null) _cts = new CancellationTokenSource();
				List<Task> tasks = new List<Task>();
				SemaphoreSlim throttler = new SemaphoreSlim(MAX_ANALYZE_DEMO_COUNT);
				List<Demo> demos = new List<Demo>(SelectedDemos.ToList());

				// init progresses before to have the right number of demos in the dict
				foreach (Demo demo in demos)
					_demoProgress.Add(demo.Id, 0);

				foreach (Demo demo in demos)
				{
					await throttler.WaitAsync().ConfigureAwait(false);

					tasks.Add(Task.Run(async () =>
					{
						if (_cts != null)
						{
							if (!File.Exists(demo.Path))
							{
								demosNotFound.Add(demo);
							}
							else
							{
								bool analyze = !checkExists || !_cacheService.HasDemoInCache(demo.Id);
								if (analyze)
								{
									bool result = await AnalyzeDemoAsync(demo, _cts.Token);
									if (!result) demosFailed.Add(demo);
								}
							}
						}
						throttler.Release();
					}));
				}

				await Task.WhenAll(tasks);
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
			finally
			{
				IsBusy = false;
				HasNotification = false;
				IsCancellable = false;
				if (demosNotFound.Any()) DispatcherHelper.CheckBeginInvokeOnUI(async () => await _dialogService.ShowDemosNotFoundAsync(demosNotFound));
				if (demosFailed.Any()) DispatcherHelper.CheckBeginInvokeOnUI(async () => await _dialogService.ShowDemosFailedAsync(demosFailed));
				CommandManager.InvalidateRequerySuggested();
			}
		}

		/// <summary>
		/// Process analyze for 1 demo
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task<bool> AnalyzeDemoAsync(Demo demo, CancellationToken token)
		{
			bool result = false;
			try
			{
				await _demosService.AnalyzeDemo(demo, token, HandleAnalyzeProgress);
				if (demo.Status == DemoStatus.NAME_DEMO_STATUS_ERROR) demo.Status = DemoStatus.NAME_DEMO_STATUS_NONE;
				if (_cts != null)
				{
					if (AppSettings.IsInternetConnectionAvailable())
					{
						await _demosService.AnalyzeBannedPlayersAsync(demo);
					}

					await _cacheService.WriteDemoDataCache(demo);
					await _cacheService.UpdateRankInfoAsync(demo, Properties.Settings.Default.SelectedStatsAccountSteamID);
					await _accountStatsService.MapSelectedAccountValues(demo, Properties.Settings.Default.SelectedStatsAccountSteamID);
				}
				result = true;
			}
			catch (Exception e)
			{
				// remove it from the dict to ignore it during progress calculation
				_demoProgress.Remove(demo.Id);
				if (!(e is TaskCanceledException) && !(e is JsonSerializationException))
				{
					try
					{
						Logger.Instance.Log(e);
						if (demo.Duration == 0.0) // invalid header
							demo.Status = DemoStatus.NAME_DEMO_STATUS_CORRUPTED;
						else
							demo.Status = DemoStatus.NAME_DEMO_STATUS_ERROR;
						await _cacheService.WriteDemoDataCache(demo);
					}
					catch (Exception)
					{
						// ignore JsonSerializationException
					}
				}
			}

			return result;
		}

		private async Task RefreshBannedPlayerCount()
		{
			try
			{
				HasNotification = true;
				IsBusy = true;
				HasRing = true;
				IsCancellable = false;
				Notification = Properties.Resources.NotificationCheckingNewBanned;
				List<string> suspectIdList = await _cacheService.GetSuspectsListFromCache();
				List<string> bannedIdList = await _cacheService.GetSuspectsBannedList();
				List<Suspect> newSuspectBannedList = await _steamService.GetNewSuspectBannedList(suspectIdList, bannedIdList);
				if (newSuspectBannedList.Any())
				{
					UpdateSuspectBannedCount(newSuspectBannedList.Count);
					// Add new banned suspects to banned list
					foreach (Suspect suspectBanned in newSuspectBannedList)
					{
						await _cacheService.AddSteamIdToBannedList(suspectBanned.SteamId);
					}
				}
			}
			catch (Exception e)
			{
				await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorGettingSuspectsData, MessageDialogStyle.Affirmative);
				Logger.Instance.Log(e);
			}
			finally
			{
				UpdateNotificationStatus();
				CommandManager.InvalidateRequerySuggested();
			}
		}

		private async Task LoadDemosHeader()
		{
			try
			{
				Notification = Properties.Resources.NotificationLoadingDemos;
				IsBusy = true;
				HasRing = true;
				HasNotification = true;
				IsCancellable = false;
				List<string> folders = new List<string>();

				if (SelectedFolder != null)
				{
					folders.Add(SelectedFolder);
				}
				else
				{
					folders = Folders.ToList();
					IsShowAllFolders = true;
				}

				Demos.Clear();

				List<Demo> demos = await _demosService.GetDemosHeader(folders, Demos.ToList(), Properties.Settings.Default.DemosListSize);
				foreach (Demo demo in demos)
				{
					await _accountStatsService.MapSelectedAccountValues(demo, Properties.Settings.Default.SelectedStatsAccountSteamID);
					Demos.Add(demo);
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
			finally
			{
				UpdateNotificationStatus();
			}
		}

		private void UpdateNotificationStatus()
		{
			if (Demos.Count == 0)
			{
				HasNotification = true;
				IsBusy = false;
				HasRing = false;
				Notification = Properties.Settings.Default.SelectedStatsAccountSteamID != 0
					? Properties.Resources.NotificationNoDemosFoundForAccount
					: Properties.Resources.NotificationNoDemosFound;
				if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFolder)) Notification += " " + Properties.Resources.NotificationInThisFolder;
			}
			else
			{
				IsBusy = false;
				HasNotification = false;
				HasRing = false;
			}
			CommandManager.InvalidateRequerySuggested();
		}

		private async Task<bool> PreProcessDemoDownload()
		{
			if (!AppSettings.IsInternetConnectionAvailable())
			{
				await _dialogService.ShowNoInternetConnectionAsync();
				return false;
			}
			if (!Directory.Exists(Properties.Settings.Default.DownloadFolder))
			{
				await _dialogService.ShowErrorAsync(Properties.Resources.DialogSetFolderForDownload, MessageDialogStyle.Affirmative);
				return false;
			}

			return true;
		}

		private async Task HandleBoilerResult(int result, bool isRecentMatches = true)
		{
			switch (result)
			{
				case 1:
					await _dialogService.ShowErrorAsync(Properties.Resources.DialogBoilerNotFound, MessageDialogStyle.Affirmative);
					break;
				case 2:
					await _dialogService.ShowErrorAsync(Properties.Resources.DialogBoilerIncorrect, MessageDialogStyle.Affirmative);
					break;
				case -1:
					await _dialogService.ShowErrorAsync("Invalid arguments", MessageDialogStyle.Affirmative);
					break;
				case -2:
					await _dialogService.ShowErrorAsync(Properties.Resources.DialogRestartSteam, MessageDialogStyle.Affirmative);
					break;
				case -3:
				case -4:
					await
						_dialogService.ShowErrorAsync(Properties.Resources.DialogSteamNotRunningOrNotLoggedIn, MessageDialogStyle.Affirmative);
					break;
				case -5:
				case -6:
				case -7:
					string msg = isRecentMatches
						? string.Format(Properties.Resources.DialogErrorWhileRetrievingMatchesData, result)
						: Properties.Resources.DialogErrorWhileRetrievingDemoData;
					await _dialogService.ShowErrorAsync(msg, MessageDialogStyle.Affirmative);
					break;
				case -8:
					string demoNotFoundMessage = isRecentMatches
						? Properties.Resources.DialogNoNewerDemo
						: Properties.Resources.DialogDemoFromShareCodeNotAvailable;
					await _dialogService.ShowMessageAsync(demoNotFoundMessage, MessageDialogStyle.Affirmative);
					break;
				case 0:
					await ProcessDemosDownloaded(isRecentMatches);
					break;
				default:
					await
						_dialogService.ShowErrorAsync("Unknown error", MessageDialogStyle.Affirmative);
					break;
			}
		}

		private async Task ProcessDemosDownloaded(bool isRecentMatches = true)
		{
			_cts = new CancellationTokenSource();
			CancellationToken ct = _cts.Token;
			int demoDownloadedCount = 0;
			try
			{
				_demosService.DownloadFolderPath = Properties.Settings.Default.DownloadFolder;
				Dictionary<string, string> demoDownloadList = await _demosService.GetDemoListUrl();
				if (ct.IsCancellationRequested) return;
				if (demoDownloadList.Count > 0)
				{
					for (int i = 1; i < demoDownloadList.Count + 1; i++)
					{
						string demoName = demoDownloadList.ElementAt(i - 1).Key;
						string demoUrl = demoDownloadList.ElementAt(i - 1).Value;
						Notification = string.Format(Properties.Resources.NotificationDownloadingDemo, i, demoDownloadList.Count);
						await _demosService.DownloadDemo(demoUrl, demoName);
						if (ct.IsCancellationRequested) return;
						Notification = string.Format(Properties.Resources.NotificationExtractingDemo, i, demoDownloadList.Count);
						await _demosService.DecompressDemoArchive(demoName);
						demoDownloadedCount++;
						if (ct.IsCancellationRequested) return;
					}
				}
				else
				{
					string demoNotFoundMessage = isRecentMatches
						? Properties.Resources.DialogNoNewerDemo
						: Properties.Resources.DialogDemoFromShareCodeNotAvailable;
					await _dialogService.ShowMessageAsync(demoNotFoundMessage, MessageDialogStyle.Affirmative);
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
			}
			finally
			{
				if (demoDownloadedCount > 0)
				{
					await _dialogService.ShowMessageAsync(string.Format(Properties.Resources.DialogDemosHaveBeenDownloaded, demoDownloadedCount), MessageDialogStyle.Affirmative);
					await LoadDemosHeader();
				}
			}
		}

		/// <summary>
		/// Check if the user is able to use the highlight / lowlight from the demos list
		/// - Steam is required
		/// - An account has to be selected
		/// - The selected account must be in the selected demo
		/// </summary>
		/// <returns></returns>
		private async Task<bool> ValidateHighLowWatch(Demo demo)
		{
			if (AppSettings.SteamExePath() == null)
			{
				await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
				return false;
			}
			if (Properties.Settings.Default.WatchAccountSteamId == 0)
			{
				await _dialogService.ShowMessageAsync(Properties.Resources.DialogSetAccountToFocus, MessageDialogStyle.Affirmative);
				return false;

			}
			Player player = demo.Players.FirstOrDefault(p => p.SteamId == Properties.Settings.Default.WatchAccountSteamId);
			if (player == null)
			{
				await _dialogService.ShowMessageAsync(Properties.Resources.DialogPlayerFocusedNotFound, MessageDialogStyle.Affirmative);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Launch highlights or lowlights
		/// </summary>
		/// <param name="isHighlight"></param>
		private async void StartWatchHighOrLow(Demo demo, bool isHighlight = true)
		{
			GameLauncherConfiguration config = new GameLauncherConfiguration(demo)
			{
				SteamExePath = AppSettings.SteamExePath(),
				Width = Properties.Settings.Default.ResolutionWidth,
				Height = Properties.Settings.Default.ResolutionHeight,
				Fullscreen = Properties.Settings.Default.IsFullscreen,
				EnableHlae = Properties.Settings.Default.EnableHlae,
				CsgoExePath = Properties.Settings.Default.CsgoExePath,
				EnableHlaeConfigParent = Properties.Settings.Default.EnableHlaeConfigParent,
				HlaeConfigParentFolderPath = Properties.Settings.Default.HlaeConfigParentFolderPath,
				HlaeExePath = HlaeService.GetHlaeExePath(),
				LaunchParameters = Properties.Settings.Default.LaunchParameters,
				UseCustomActionsGeneration = Properties.Settings.Default.UseCustomActionsGeneration,
				FocusPlayerSteamId = Properties.Settings.Default.WatchAccountSteamId,
			};
			GameLauncher launcher = new GameLauncher(config);
			var isPlayerPerspective = await _dialogService.ShowHighLowWatchAsync();
			if (isPlayerPerspective == MessageDialogResult.FirstAuxiliary) return;
			if (isHighlight)
			{
				launcher.WatchHighlightDemo(isPlayerPerspective == MessageDialogResult.Affirmative);
			}
			else
			{
				launcher.WatchLowlightDemo(isPlayerPerspective == MessageDialogResult.Affirmative);
			}
		}

		/// <summary>
		/// Process when the user want to watch highlights or lowlights
		/// </summary>
		private async Task ProcessWatchHighOrLow(Demo demo, bool isHighlight = true)
		{
			bool isWatchValidated = await ValidateHighLowWatch(demo);
			try
			{
				if (isWatchValidated) StartWatchHighOrLow(demo, isHighlight);
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				await _dialogService.ShowErrorAsync(e.Message, MessageDialogStyle.Affirmative);
			}
		}

		private void HandleAnalyzeProgress(string demoId, float value)
		{
			// it's time consuming, we don't want to update at each events only when the rounded value has changed
			if (value < 0 || value > 1) return;
			value = (float)Math.Round(value, 2);
			if (value <= _demoProgress[demoId]) return;
			_demoProgress[demoId] = value;

			float total = _demoProgress.ToList().Sum(kvp => kvp.Value);
			value = _demoProgress.Count > 0 ? total / _demoProgress.Count : 0;
			SendTaskbarProgressMessage(value);
		}

		private static void SendTaskbarProgressMessage(float value)
		{
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
			{
				UpdateTaskbarProgressMessage msg = new UpdateTaskbarProgressMessage
				{
					Value = value,
				};
				Messenger.Default.Send(msg);
			}));
		}

		public void Dispose()
		{
			_cts.Dispose();
		}
	}
}
