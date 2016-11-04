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
using Core;
using Core.Models;
using Core.Models.Source;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls.Dialogs;
using Manager.Messages;
using Manager.Services;
using Manager.Views.Accounts;
using Manager.Views.Demos;
using Manager.Views.Suspects;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Services.Concrete.Excel;
using Services.Interfaces;
using Application = System.Windows.Application;
using Demo = Core.Models.Demo;
using UserControl = System.Windows.Controls.UserControl;

namespace Manager.ViewModel.Demos
{
	public class DemoListViewModel : ViewModelBase, IDisposable
	{

		#region Properties

		private const int MAX_ANALYZE_DEMO_COUNT = 4;

		private readonly IDemosService _demosService;

		private readonly DialogService _dialogService;

		private readonly ICacheService _cacheService;

		private readonly ISteamService _steamService;

		private readonly IAccountStatsService _accountStatsService;

		private readonly ExcelService _excelService;

		private bool _isBusy;

		private bool _hasRing;

		private bool _isCancellable;

		private bool _hasNotification;

		private bool _isShowAllFolders;

		private bool _isMainWindowLoaded;

		private bool _isShowPovDemos = Properties.Settings.Default.ShowPovDemos;

		private bool _isShowEbotDemos = Properties.Settings.Default.ShowEbotDemos;

		private bool _isShowFaceitDemos = Properties.Settings.Default.ShowFaceitDemos;

		private bool _isShowEseaDemos = Properties.Settings.Default.ShowEseaDemos;

		private bool _isShowCevoDemos = Properties.Settings.Default.ShowCevoDemos;

		private bool _isShowValveDemos = Properties.Settings.Default.ShowValveDemos;

		private bool _isShowPopFlashDemos = Properties.Settings.Default.ShowPopFlashDemos;

		private bool _isShowOldDemos = Properties.Settings.Default.ShowOldDemos;

		private string _notificationMessage;

		private ObservableCollection<Demo> _demos;

		private ObservableCollection<Demo> _selectedDemos;

		private ICollectionView _dataGridDemosCollection;

		private string _filterDemoText;

		private Demo _selectedDemo;

		private RelayCommand<Demo> _showDemoDetailsCommand;

		private RelayCommand _goToAccountStatsCommand;

		private RelayCommand<ObservableCollection<Demo>> _analyzeDemosCommand;

		private RelayCommand<ObservableCollection<Demo>> _deleteDemosCommand;

		private RelayCommand<ObservableCollection<Demo>> _removeDemosFromCacheCommand;

		private RelayCommand<ObservableCollection<Demo>> _exportExcelCommand;

		private RelayCommand _showMoreDemosCommand;

		private RelayCommand<Demo> _watchDemoCommand;

		private RelayCommand<Demo> _watchHighlightCommand;

		private RelayCommand<Demo> _watchLowlightCommand;

		private RelayCommand<Demo> _browseToDemoCommand;

		private RelayCommand<Demo> _copyPlaydemoCommand;

		private RelayCommand<ObservableCollection<Demo>> _exportJsonCommand;

		private RelayCommand<Demo> _goToTickCommand;

		private RelayCommand<ObservableCollection<Demo>> _addPlayersToSuspectsListCommand;

		private RelayCommand<bool> _showAllFoldersCommand;

		private RelayCommand<bool> _showPovDemosCommand;

		private RelayCommand<bool> _showEbotDemosCommand;

		private RelayCommand<bool> _showEseaDemosCommand;

		private RelayCommand<bool> _showValveDemosCommand;

		private RelayCommand<bool> _showOldDemosCommand;

		private RelayCommand<bool> _showFaceitDemosCommand;

		private RelayCommand<bool> _showCevoDemosCommand;

		private RelayCommand<bool> _showPopFlashDemosCommand;

		private RelayCommand<bool> _showAllAccountsCommand;

		private RelayCommand<bool> _showOnlyAccountDemos;

		private RelayCommand _showSuspectsCommand;

		private RelayCommand _refreshListCommand;

		private RelayCommand<string> _saveStatusDemoCommand;

		private RelayCommand<string> _setDemoSourceCommand;

		private RelayCommand<IList> _demosSelectionChangedCommand;

		private RelayCommand _stopAnalyzeCommand;

		private RelayCommand _downloadDemosCommand;

		private RelayCommand _copyShareCodeCommand;

		private int _newBannedPlayerCount;

		private ObservableCollection<string> _folders;

		private string _selectedFolder;

		private RelayCommand<UserControl> _showLastUserControlCommand;

		private Rank _lastRankAccountStats;

		private CancellationTokenSource _cts = new CancellationTokenSource();

		#endregion

		#region Accessors

		public bool HasRing
		{
			get { return _hasRing; }
			set { Set(() => HasRing, ref _hasRing, value); }
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			set { Set(() => IsBusy, ref _isBusy, value); }
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

		public bool IsShowOldDemos
		{
			get { return _isShowOldDemos; }
			set { Set(() => IsShowOldDemos, ref _isShowOldDemos, value); }
		}

		public bool IsShowPovDemos
		{
			get { return _isShowPovDemos; }
			set
			{
				Set(() => IsShowPovDemos, ref _isShowPovDemos, value);
				FilterCollection();
			}
		}

		public bool IsShowEbotDemos
		{
			get { return _isShowEbotDemos; }
			set
			{
				Set(() => IsShowEbotDemos, ref _isShowEbotDemos, value);
				FilterCollection();
			}
		}

		public bool IsShowFaceitDemos
		{
			get { return _isShowFaceitDemos; }
			set
			{
				Set(() => IsShowFaceitDemos, ref _isShowFaceitDemos, value);
				FilterCollection();
			}
		}

		public bool IsShowCevoDemos
		{
			get { return _isShowCevoDemos; }
			set
			{
				Set(() => IsShowCevoDemos, ref _isShowCevoDemos, value);
				FilterCollection();
			}
		}

		public bool IsShowEseaDemos
		{
			get { return _isShowEseaDemos; }
			set
			{
				Set(() => IsShowEseaDemos, ref _isShowEseaDemos, value);
				FilterCollection();
			}
		}

		public bool IsShowValveDemos
		{
			get { return _isShowValveDemos; }
			set
			{
				Set(() => IsShowValveDemos, ref _isShowValveDemos, value);
				FilterCollection();
			}
		}

		public bool IsShowPopFlashDemos
		{
			get { return _isShowPopFlashDemos; }
			set
			{
				Set(() => IsShowPopFlashDemos, ref _isShowPopFlashDemos, value);
				FilterCollection();
			}
		}

		public int NewBannedPlayerCount
		{
			get { return _newBannedPlayerCount; }
			set
			{
				Set(() => NewBannedPlayerCount, ref _newBannedPlayerCount, value);
				RaisePropertyChanged(() => NewBannedPlayerCountAsString);
			}
		}

		public string NewBannedPlayerCountAsString => string.Format(" ({0})", _newBannedPlayerCount);

		public bool HasNotification
		{
			get { return _hasNotification; }
			set { Set(() => HasNotification, ref _hasNotification, value); }
		}

		public string NotificationMessage
		{
			get { return _notificationMessage; }
			set { Set(() => NotificationMessage, ref _notificationMessage, value); }
		}

		public Demo SelectedDemo
		{
			get { return _selectedDemo; }
			set { Set(() => SelectedDemo, ref _selectedDemo, value); }
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
				Properties.Settings.Default.Save();
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
				if (!IsShowPovDemos && data.SourceName == "pov") return false;

				// eBot filter
				if (!IsShowEbotDemos && data.SourceName == "ebot") return false;

				// ESEA filter
				if (!IsShowEseaDemos && data.SourceName == "esea") return false;

				// Valve filter
				if (!IsShowValveDemos && data.SourceName == "valve") return false;

				// Faceit filter
				if (!IsShowFaceitDemos && data.SourceName == "faceit") return false;

				// Cevo filter
				if (!IsShowCevoDemos && data.SourceName == "cevo") return false;

				// PopFlash filter
				if (!IsShowPopFlashDemos && data.SourceName == "popflash") return false;

				// No analyzable demos filter
				if (!IsShowOldDemos && data.Status == "old") return false;

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
						bool result = await RefreshSelectedDemos();
						if (result && hasValveDemo != null) await RefreshLastRankAccount();
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
											NotificationMessage = Properties.Resources.NotificationAnalyzingDemosForExport;
											IsCancellable = true;
											if (_cts == null) _cts = new CancellationTokenSource();

											List<Demo> demoList = demos.ToList();
											while (demoList.Any() && _cts != null)
											{
												Task[] tasks = demoList.ToList().Take(MAX_ANALYZE_DEMO_COUNT).Select(async demo =>
												{
													if (!_cacheService.HasDemoInCache(demo.Id))
													{
														await AnalyzeDemoAsync(demo, _cts.Token);
													}
													demo.WeaponFired = await _cacheService.GetDemoWeaponFiredAsync(demo);
													demoList.Remove(demo);
												}).ToArray();
												await Task.WhenAny(Task.WhenAll(tasks), _cts.Token.AsTask());
											}
											if (_cts != null) await _excelService.GenerateXls(SelectedDemos.ToList(), saveExportFileDialog.FileName, Properties.Settings.Default.SelectedStatsAccountSteamID);
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
											if (_cts == null) _cts = new CancellationTokenSource();

											try
											{
												IsBusy = true;
												HasRing = true;
												HasNotification = true;
												NotificationMessage = Properties.Resources.NotificationAnalyzingDemosForExport;
												IsCancellable = true;

												List<Demo> demoList = demos.ToList();
												while (demoList.Any() && _cts != null)
												{
													Task[] tasks = demoList.ToList().Take(MAX_ANALYZE_DEMO_COUNT).Select(async demo =>
													{
														string exportFilePath = directoryPath + Path.DirectorySeparatorChar + demo.Name.Substring(0, demo.Name.Length - 4) + "-export.xlsx";
														if (!_cacheService.HasDemoInCache(demo.Id))
														{
															int analyzeResult = await AnalyzeDemoAsync(demo, _cts.Token);
															if (analyzeResult == 1 && _cts != null)
															{
																NotificationMessage = string.Format(Properties.Resources.NotificationExportingDemo, demo.Name);
																demo.WeaponFired = await _cacheService.GetDemoWeaponFiredAsync(demo);
																await _excelService.GenerateXls(demo, exportFilePath);
															}
														}
														else
														{
															NotificationMessage = string.Format(Properties.Resources.NotificationExportingDemo, demo.Name);
															demo.WeaponFired = await _cacheService.GetDemoWeaponFiredAsync(demo);
															await _excelService.GenerateXls(demo, exportFilePath);
														}
														demoList.Remove(demo);
													}).ToArray();
													await Task.WhenAny(Task.WhenAll(tasks), _cts.Token.AsTask());
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
							SaveFileDialog saveExportDialog = new SaveFileDialog
							{
								FileName = SelectedDemo.Name.Substring(0, SelectedDemo.Name.Length - 4) + "-export.xlsx",
								Filter = "XLSX file (*.xlsx)|*.xlsx"
							};

							if (saveExportDialog.ShowDialog() == DialogResult.OK)
							{
								try
								{
									IsBusy = true;
									HasRing = true;
									HasNotification = true;
									if (!_cacheService.HasDemoInCache(SelectedDemo.Id))
									{
										NotificationMessage = string.Format(Properties.Resources.NotificationAnalyzingDemoForExport, SelectedDemo.Name);
										IsCancellable = true;
										if (_cts == null) _cts = new CancellationTokenSource();
										await AnalyzeDemoAsync(SelectedDemo, _cts.Token);
									}
									if (_cts != null)
									{
										NotificationMessage = string.Format(Properties.Resources.NotificationAnalyzingDemoForExport, SelectedDemo.Name);
										await _excelService.GenerateXls(SelectedDemo, saveExportDialog.FileName);
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
							detailsViewModel.CurrentDemo = demo;

							// Display the UserControl
							var mainViewModel = new ViewModelLocator().Main;
							DemoDetailsView detailsView = new DemoDetailsView();
							mainViewModel.CurrentPage.ShowPage(detailsView);
						},
						demo => SelectedDemo != null));
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
							NotificationMessage = Properties.Resources.NotificationPlayDemoCommandCopied;
							await Task.Delay(3000);
							HasNotification = false;
							IsBusy = false;
							CommandManager.InvalidateRequerySuggested();
						},
						demo => SelectedDemo != null));
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
									GameLauncher launcher = new GameLauncher(SelectedDemo);
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
						demo => SelectedDemo != null));
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

							IsBusy = true;
							HasRing = true;
							HasNotification = true;
							List<Demo> demosFailed = new List<Demo>();
							for (int i = 0; i < demos.Count; i++)
							{
								if (!demos[i].Players.Any())
								{
									try
									{
										if (_cts == null)
										{
											_cts = new CancellationTokenSource();
										}
										NotificationMessage = string.Format(Properties.Resources.NotificationAnalyzingDemo, demos[i].Name);
										demos[i] = await _demosService.AnalyzeDemo(demos[i], _cts.Token);
										if (AppSettings.IsInternetConnectionAvailable())
										{
											await _demosService.AnalyzeBannedPlayersAsync(demos[i]);
										}
										await _cacheService.WriteDemoDataCache(demos[i]);
									}
									catch (Exception e)
									{
										Logger.Instance.Log(e);
										demos[i].Status = "old";
										demosFailed.Add(demos[i]);
										await _cacheService.WriteDemoDataCache(demos[i]);
									}

								}
								if (demos[i].Players.Any())
								{
									foreach (Player playerExtended in demos[i].Players)
									{
										NotificationMessage = Properties.Resources.NotificationAddingSuspects;
										await _cacheService.AddSuspectToCache(playerExtended.SteamId.ToString());
									}
								}
							}

							if (demosFailed.Any())
							{
								await _dialogService.ShowDemosFailedAsync(demosFailed);
							}

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
							Properties.Settings.Default.Save();
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
							NotificationMessage = Properties.Resources.NotificationLoading;
							await LoadDemosHeader();
							IsBusy = false;
							HasRing = false;
						},
						isChecked => !IsBusy && new ViewModelLocator().Settings.SelectedStatsAccount != null));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle old demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowOldDemosCommand
		{
			get
			{
				return _showOldDemosCommand
					?? (_showOldDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowOldDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowOldDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle Faceit demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowFaceitDemosCommand
		{
			get
			{
				return _showFaceitDemosCommand
					?? (_showFaceitDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowFaceitDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowFaceitDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle CEVO demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowCevoDemosCommand
		{
			get
			{
				return _showCevoDemosCommand
					?? (_showCevoDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowCevoDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowCevoDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle PopFlash demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowPopFlashDemosCommand
		{
			get
			{
				return _showPopFlashDemosCommand
					?? (_showPopFlashDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowPopFlashDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowPopFlashDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle POV demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowPovDemosCommand
		{
			get
			{
				return _showPovDemosCommand
					?? (_showPovDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowPovDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowPovDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle eBot demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowEbotDemosCommand
		{
			get
			{
				return _showEbotDemosCommand
					?? (_showEbotDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowEbotDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowEbotDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle ESEA demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowEseaDemosCommand
		{
			get
			{
				return _showEseaDemosCommand
					?? (_showEseaDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowEseaDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowEseaDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
			}
		}

		/// <summary>
		/// Command when the checkbox to toggle Valve demos is clicked
		/// </summary>
		public RelayCommand<bool> ShowValveDemosCommand
		{
			get
			{
				return _showValveDemosCommand
					?? (_showValveDemosCommand = new RelayCommand<bool>(
						isChecked =>
						{
							IsShowValveDemos = isChecked;
							DataGridDemosCollection.Refresh();
							Properties.Settings.Default.ShowValveDemos = isChecked;
							Properties.Settings.Default.Save();
						},
						isChecked => !IsBusy));
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
						demo => SelectedDemo != null));
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
							GameLauncher launcher = new GameLauncher(SelectedDemo);
							launcher.WatchDemo();
						}
						catch (Exception e)
						{
							Logger.Instance.Log(e);
							await _dialogService.ShowErrorAsync(e.Message, MessageDialogStyle.Affirmative);
						}
					},
					demo => SelectedDemo != null));
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
						if (AppSettings.SteamExePath() == null)
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
							return;
						}
						if (Properties.Settings.Default.WatchAccountSteamId == 0)
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogSetAccountToFocus, MessageDialogStyle.Affirmative);
							return;
						}
						try
						{
							GameLauncher launcher = new GameLauncher(SelectedDemo);
							var isPlayerPerspective = await _dialogService.ShowHighLowWatchAsync();
							if (isPlayerPerspective == MessageDialogResult.FirstAuxiliary) return;
							launcher.WatchHighlightDemo(isPlayerPerspective == MessageDialogResult.Affirmative);
						}
						catch (Exception e)
						{
							Logger.Instance.Log(e);
							await _dialogService.ShowErrorAsync(e.Message, MessageDialogStyle.Affirmative);
						}
					},
					demo => SelectedDemo != null));
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
						if (AppSettings.SteamExePath() == null)
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogSteamNotFound, MessageDialogStyle.Affirmative);
							return;
						}
						if (Properties.Settings.Default.WatchAccountSteamId == 0)
						{
							await _dialogService.ShowMessageAsync(Properties.Resources.DialogSetAccountToFocus, MessageDialogStyle.Affirmative);
							return;
						}
						try
						{
							GameLauncher launcher = new GameLauncher(SelectedDemo);
							var isPlayerPerspective = await _dialogService.ShowHighLowWatchAsync();
							if (isPlayerPerspective == MessageDialogResult.FirstAuxiliary) return;
							launcher.WatchLowlightDemo(isPlayerPerspective == MessageDialogResult.Affirmative);
						}
						catch (Exception e)
						{
							Logger.Instance.Log(e);
							await _dialogService.ShowErrorAsync(e.Message, MessageDialogStyle.Affirmative);
						}
					},
					demo => SelectedDemo != null));
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
						await _demosService.SetSource(SelectedDemos, source);
					},
					source => SelectedDemos != null && SelectedDemos.Count > 0));
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
							{
								SelectedDemos.Add(demo);
							}
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
						() =>
						{
							if (_cts != null)
							{
								_cts.Cancel();
								_cts = null;
								NotificationMessage = Properties.Resources.NotificationCancelling;
								IsCancellable = false;
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
						NotificationMessage = Properties.Resources.NotificationLoadingMoreDemos;
						IsBusy = true;
						HasRing = true;
						HasNotification = true;
						List<string> folders = new List<string>();
						if (SelectedFolder != null)
						{
							folders.Add(SelectedFolder);
						}
						else
						{
							folders = Folders.ToList();
						}

						List<Demo> demos = await _demosService.GetDemosHeader(folders, Demos.ToList(), true);

						foreach (Demo demo in demos)
						{
							await _accountStatsService.MapSelectedAccountValues(demo, Properties.Settings.Default.SelectedStatsAccountSteamID);
							Demos.Add(demo);
						}

						IsBusy = false;
						HasNotification = false;
						CommandManager.InvalidateRequerySuggested();
					},
					() => !IsBusy));
			}
		}

		/// <summary>
		/// Command to copy demo's share code
		/// </summary>
		public RelayCommand CopyShareCodeCommand
		{
			get
			{
				return _copyShareCodeCommand
					?? (_copyShareCodeCommand = new RelayCommand(
						async () =>
						{
							string shareCode = await _demosService.GetShareCode(SelectedDemo);
							if (shareCode == string.Empty)
							{
								await _dialogService.ShowErrorAsync(Properties.Resources.DialogDemoShareCodeUnavailable, MessageDialogStyle.Affirmative);
								return;
							}
							Clipboard.SetText(shareCode);
							IsBusy = true;
							HasRing = false;
							HasNotification = true;
							NotificationMessage = Properties.Resources.NotificationDemoShareCodeCopied;
							await Task.Delay(3000);
							HasNotification = false;
							IsBusy = false;
							CommandManager.InvalidateRequerySuggested();
						},
						() => SelectedDemo != null));
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
						if (!AppSettings.IsInternetConnectionAvailable())
						{
							await _dialogService.ShowNoInternetConnectionAsync();
							return;
						}
						if (!Directory.Exists(Properties.Settings.Default.DownloadFolder))
						{
							await _dialogService.ShowErrorAsync(Properties.Resources.DialogSetFolderForDownload, MessageDialogStyle.Affirmative);
							return;
						}

						try
						{
							IsBusy = true;
							HasNotification = true;
							HasRing = true;
							IsCancellable = true;
							NotificationMessage = Properties.Resources.NotificationRetrievingMatchesData;
							if (_cts == null) _cts = new CancellationTokenSource();
							int result = await _steamService.GenerateMatchListFile(_cts.Token);
							switch (result)
							{
								case 1:
									await _dialogService.ShowErrorAsync(Properties.Resources.DialogBoilerNotFound, MessageDialogStyle.Affirmative);
									break;
								case 2:
									await _dialogService.ShowErrorAsync(Properties.Resources.DialogBoilerIncorrect, MessageDialogStyle.Affirmative);
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
									await
										_dialogService.ShowErrorAsync(string.Format(Properties.Resources.DialogErrorWhileRetrievingMatchesData, result), MessageDialogStyle.Affirmative);
									break;
								case 0:
									await ProcessDemosDownloaded();
									break;
							}
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
							string path = Path.GetFullPath(folderDialog.SelectedPath).ToLower();

							try
							{
								IsBusy = true;
								HasRing = true;
								HasNotification = true;
								NotificationMessage = Properties.Resources.NotificationAnalyzingForJsonExport;
								IsCancellable = true;
								if (_cts == null) _cts = new CancellationTokenSource();

								List<Demo> demoList = demos.ToList();
								while (demoList.Any() && _cts != null)
								{
									Task[] tasks = demoList.ToList().Take(MAX_ANALYZE_DEMO_COUNT).Select(async demo =>
									{
										await AnalyzeDemoAsync(demo, _cts.Token, false);
										await _cacheService.GenerateJsonAsync(demo, path);
										demoList.Remove(demo);
									}).ToArray();
									await Task.WhenAny(Task.WhenAll(tasks), _cts.Token.AsTask());
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
			IDemosService demosService, DialogService dialogService, ISteamService steamService,
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

			Demos = new ObservableCollection<Demo>();
			SelectedDemos = new ObservableCollection<Demo>();
			DataGridDemosCollection = CollectionViewSource.GetDefaultView(Demos);
			DataGridDemosCollection.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
			DataGridDemosCollection.Filter = Filter;

			Messenger.Default.Register<MainWindowLoadedMessage>(this, HandleMainWindowLoadedMessage);
		}

		private void HandleMainWindowLoadedMessage(MainWindowLoadedMessage msg)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(
			async () =>
			{
				HasNotification = true;
				IsBusy = true;
				HasRing = true;
				NotificationMessage = Properties.Resources.NotificationInitCache;
				await _cacheService.InitDemoBasicDataList();
				await _cacheService.InitDemoCache();
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

				// Notify the bot that the app is loaded
				Win32Utils.SendMessageToBot(Win32Utils.WM_CSGO_DM_LOADED);

				// Start downloading demos if the app has been started with "download" argument
				if (App.StartUpWindow == "download")
					DownloadDemosCommand.Execute(null);

				_isMainWindowLoaded = true;
			});
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
			NotificationMessage = string.Format(Properties.Resources.NotificationSuspectsHaveBeenBanned, NewBannedPlayerCount);
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
				NotificationMessage = Properties.Resources.NotificationSearchingLastRank;
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

		private async Task<bool> RefreshSelectedDemos()
		{
			if (SelectedDemos.Count == Demos.Count && Demos.Count == AppSettings.DEMO_PAGE_COUNT)
			{
				var isAllAnalyze = await _dialogService.ShowAnalyzeAllDemosAsync();
				if (isAllAnalyze == MessageDialogResult.FirstAuxiliary) return false;
				IsBusy = true;
				HasRing = true;
				HasNotification = true;
				if (isAllAnalyze == MessageDialogResult.Negative)
				{
					NotificationMessage = Properties.Resources.NotificationLoadingAllDemos;
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

			IsBusy = true;
			HasRing = true;
			HasNotification = true;
			IsCancellable = true;
			NotificationMessage = Properties.Resources.NotificationAnalyzingMultipleDemos;
			CommandManager.InvalidateRequerySuggested();
			if (SelectedDemos.Count == 1) NotificationMessage = string.Format(Properties.Resources.NotificationAnalyzingDemo, SelectedDemos[0].Name);

			List<Demo> demosFailed = new List<Demo>();
			List<Demo> demosNotFound = new List<Demo>();

			try
			{
				if (_cts == null) _cts = new CancellationTokenSource();
				List<Demo> demos = SelectedDemos.ToList();
				while (demos.Any() && _cts != null)
				{
					Task[] tasks = demos.ToList().Take(MAX_ANALYZE_DEMO_COUNT).Select(async demo =>
					{
						int result = await AnalyzeDemoAsync(demo, _cts.Token);
						switch (result)
						{
							case -1:
								demosFailed.Add(demo);
								break;
							case -2:
								demosNotFound.Add(demo);
								break;
						}
						demos.Remove(demo);
					}).ToArray();

					await Task.WhenAny(Task.WhenAll(tasks), _cts.Token.AsTask());
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Log(e);
				await _dialogService.ShowErrorAsync(Properties.Resources.DialogErrorAnalyzingDemos, MessageDialogStyle.Affirmative);
			}
			finally
			{
				IsBusy = false;
				HasNotification = false;
				IsCancellable = false;
				CommandManager.InvalidateRequerySuggested();
				if (demosNotFound.Any()) await _dialogService.ShowDemosNotFoundAsync(demosNotFound);
				if (demosFailed.Any()) await _dialogService.ShowDemosFailedAsync(demosFailed);
			}

			return true;
		}

		/// <summary>
		/// Process analyze for 1 demo
		/// </summary>
		/// <param name="demo"></param>
		/// <param name="token"></param>
		/// <param name="writeToCache"></param>
		/// <returns></returns>
		private async Task<int> AnalyzeDemoAsync(Demo demo, CancellationToken token, bool writeToCache = true)
		{
			if (!File.Exists(demo.Path)) return -2;

			try
			{
				await _demosService.AnalyzeDemo(demo, token);
				if (_cts != null)
				{
					if (AppSettings.IsInternetConnectionAvailable())
					{
						await _demosService.AnalyzeBannedPlayersAsync(demo);
					}

					if (writeToCache)
					{
						await _cacheService.WriteDemoDataCache(demo);
						await _cacheService.UpdateRankInfoAsync(demo, Properties.Settings.Default.SelectedStatsAccountSteamID);
					}
					await _accountStatsService.MapSelectedAccountValues(demo, Properties.Settings.Default.SelectedStatsAccountSteamID);
				}
			}
			catch (Exception e)
			{
				if (e is TaskCanceledException || e is JsonSerializationException) return -1;
				Logger.Instance.Log(e);
				demo.Status = "old";
				await _cacheService.WriteDemoDataCache(demo);
				return -1;
			}
			finally
			{
				if (_cts != null && demo.Status == "old")
				{
					demo.Status = "none";
					await _cacheService.WriteDemoDataCache(demo);
				}
			}

			return 1;
		}

		private async Task RefreshBannedPlayerCount()
		{
			try
			{
				HasNotification = true;
				IsBusy = true;
				HasRing = true;
				IsCancellable = false;
				NotificationMessage = Properties.Resources.NotificationCheckingNewBanned;
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
				NotificationMessage = Properties.Resources.NotificationLoadingDemos;
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

				List<Demo> demos = await _demosService.GetDemosHeader(folders, Demos.ToList(), true);
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
				NotificationMessage = Properties.Settings.Default.SelectedStatsAccountSteamID != 0
					? Properties.Resources.NotificationNoDemosFoundForAccount
					: Properties.Resources.NotificationNoDemosFound;
				if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFolder)) NotificationMessage += " " + Properties.Resources.NotificationInThisFolder;
			}
			else
			{
				IsBusy = false;
				HasNotification = false;
				HasRing = false;
			}
			CommandManager.InvalidateRequerySuggested();
		}

		private async Task ProcessDemosDownloaded()
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
						NotificationMessage = string.Format(Properties.Resources.NotificationDownloadingDemo, i, demoDownloadList.Count);
						await _demosService.DownloadDemo(demoUrl, demoName);
						if (ct.IsCancellationRequested) return;
						NotificationMessage = string.Format(Properties.Resources.NotificationExtractingDemo, i, demoDownloadList.Count);
						await _demosService.DecompressDemoArchive(demoName);
						demoDownloadedCount++;
						if (ct.IsCancellationRequested) return;
					}
				}
				else
				{
					await _dialogService.ShowMessageAsync(Properties.Resources.DialogNoNewerDemo, MessageDialogStyle.Affirmative);
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

		public void Dispose()
		{
			_cts.Dispose();
		}
	}
}
