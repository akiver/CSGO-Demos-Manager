using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Core.Models;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Manager.Messages;
using Manager.ViewModel.Shared;
using Manager.Views.Accounts;
using Manager.Views.Demos;
using Services.Interfaces;
using Services.Models.Charts;
using Services.Models.Stats;

namespace Manager.ViewModel.Accounts
{
	public class AccountOverallViewModel : BaseViewModel
	{
		#region Properties

		private readonly IAccountStatsService _accountStatsService;

		private readonly ICacheService _cacheService;

		private RelayCommand _windowLoadedCommand;

		private RelayCommand _backToHomeCommand;

		private RelayCommand _goToRankCommand;

		private RelayCommand _goToMapCommand;

		private RelayCommand _goToWeaponCommand;

		private RelayCommand _goToProgressCommand;

		private List<GenericDoubleChart> _datasMatchStats;

		private int _matchCount;

		private int _killCount;

		private int _assistCount;

		private int _deathCount;

		private int _headshotCount;

		private decimal _killDeathRatio;

		private decimal _headshotRatio;

		private int _entryKillCount;

		private int _knifeKillCount;

		private int _fiveKillCount;

		private int _fourKillCount;

		private int _threeKillCount;

		private int _twoKillCount;

		private int _bombPlantedCount;

		private int _bombDefusedCount;

		private int _bombExplodedCount;

		private int _mvpCount;

		private int _damageCount;

		private int _roundCount;

		private int _oneVersusOneCount;

		private int _oneVersusTwoCount;

		private int _oneVersusThreeCount;

		private int _oneVersusFourCount;

		private int _oneVersusFiveCount;

		private double _killPerRoundPercentage;

		private double _assistPerRoundPercentage;

		private double _deathPerRoundPercentage;

		private double _clutchWinPercentage;

		private double _averageDamagesPerRound;

		private double _hltvRating;

		private double _hltv2Rating;

		private double _eseaRws;

		private string _totalMatchTime;

		private string _averageMatchTime;

		#endregion

		#region Accessors

		public List<GenericDoubleChart> DatasMatchStats
		{
			get { return _datasMatchStats; }
			set { Set(() => DatasMatchStats, ref _datasMatchStats, value); }
		}

		public int MatchCount
		{
			get { return _matchCount; }
			set { Set(() => MatchCount, ref _matchCount, value); }
		}

		public int KillCount
		{
			get { return _killCount; }
			set { Set(() => KillCount, ref _killCount, value); }
		}

		public int AssistCount
		{
			get { return _assistCount; }
			set { Set(() => AssistCount, ref _assistCount, value); }
		}

		public int DeathCount
		{
			get { return _deathCount; }
			set { Set(() => DeathCount, ref _deathCount, value); }
		}

		public int HeadshotCount
		{
			get { return _headshotCount; }
			set { Set(() => HeadshotCount, ref _headshotCount, value); }
		}

		public decimal HeadshotRatio
		{
			get { return _headshotRatio; }
			set { Set(() => HeadshotRatio, ref _headshotRatio, value); }
		}

		public decimal KillDeathRatio
		{
			get { return _killDeathRatio; }
			set { Set(() => KillDeathRatio, ref _killDeathRatio, value); }
		}

		public int EntryKillCount
		{
			get { return _entryKillCount; }
			set { Set(() => EntryKillCount, ref _entryKillCount, value); }
		}

		public int KnifeKillCount
		{
			get { return _knifeKillCount; }
			set { Set(() => KnifeKillCount, ref _knifeKillCount, value); }
		}

		public int FiveKillCount
		{
			get { return _fiveKillCount; }
			set { Set(() => FiveKillCount, ref _fiveKillCount, value); }
		}

		public int FourKillCount
		{
			get { return _fourKillCount; }
			set { Set(() => FourKillCount, ref _fourKillCount, value); }
		}

		public int ThreeKillCount
		{
			get { return _threeKillCount; }
			set { Set(() => ThreeKillCount, ref _threeKillCount, value); }
		}

		public int TwoKillCount
		{
			get { return _twoKillCount; }
			set { Set(() => TwoKillCount, ref _twoKillCount, value); }
		}

		public int BombPlantedCount
		{
			get { return _bombPlantedCount; }
			set { Set(() => BombPlantedCount, ref _bombPlantedCount, value); }
		}

		public int BombExplodedCount
		{
			get { return _bombExplodedCount; }
			set { Set(() => BombExplodedCount, ref _bombExplodedCount, value); }
		}

		public int BombDefusedCount
		{
			get { return _bombDefusedCount; }
			set { Set(() => BombDefusedCount, ref _bombDefusedCount, value); }
		}

		public int MvpCount
		{
			get { return _mvpCount; }
			set { Set(() => MvpCount, ref _mvpCount, value); }
		}

		public int DamageCount
		{
			get { return _damageCount; }
			set { Set(() => DamageCount, ref _damageCount, value); }
		}

		public int RoundCount
		{
			get { return _roundCount; }
			set { Set(() => RoundCount, ref _roundCount, value); }
		}

		public int OneVersusOneCount
		{
			get { return _oneVersusOneCount; }
			set { Set(() => OneVersusOneCount, ref _oneVersusOneCount, value); }
		}

		public int OneVersusTwoCount
		{
			get { return _oneVersusTwoCount; }
			set { Set(() => OneVersusTwoCount, ref _oneVersusTwoCount, value); }
		}

		public int OneVersusThreeCount
		{
			get { return _oneVersusThreeCount; }
			set { Set(() => OneVersusThreeCount, ref _oneVersusThreeCount, value); }
		}

		public int OneVersusFourCount
		{
			get { return _oneVersusFourCount; }
			set { Set(() => OneVersusFourCount, ref _oneVersusFourCount, value); }
		}

		public int OneVersusFiveCount
		{
			get { return _oneVersusFiveCount; }
			set { Set(() => OneVersusFiveCount, ref _oneVersusFiveCount, value); }
		}

		public double KillPerRoundPercentage
		{
			get { return _killPerRoundPercentage; }
			set { Set(() => KillPerRoundPercentage, ref _killPerRoundPercentage, value); }
		}

		public double AssistPerRoundPercentage
		{
			get { return _assistPerRoundPercentage; }
			set { Set(() => AssistPerRoundPercentage, ref _assistPerRoundPercentage, value); }
		}

		public double DeathPerRoundPercentage
		{
			get { return _deathPerRoundPercentage; }
			set { Set(() => DeathPerRoundPercentage, ref _deathPerRoundPercentage, value); }
		}

		public double ClutchWinPercentage
		{
			get { return _clutchWinPercentage; }
			set { Set(() => ClutchWinPercentage, ref _clutchWinPercentage, value); }
		}

		public double AverageDamagesPerRound
		{
			get { return _averageDamagesPerRound; }
			set { Set(() => AverageDamagesPerRound, ref _averageDamagesPerRound, value); }
		}

		public double HltvRating
		{
			get { return _hltvRating; }
			set { Set(() => HltvRating, ref _hltvRating, value); }
		}

		public double Hltv2Rating
		{
			get { return _hltv2Rating; }
			set { Set(() => Hltv2Rating, ref _hltv2Rating, value); }
		}

		public double EseaRws
		{
			get { return _eseaRws; }
			set { Set(() => EseaRws, ref _eseaRws, value); }
		}

		public string TotalMatchTime
		{
			get { return _totalMatchTime; }
			set { Set(() => TotalMatchTime, ref _totalMatchTime, value); }
		}

		public string AverageMatchTime
		{
			get { return _averageMatchTime; }
			set { Set(() => AverageMatchTime, ref _averageMatchTime, value); }
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
						await LoadDatas();
						Messenger.Default.Register<SettingsFlyoutClosed>(this, HandleSettingsFlyoutClosedMessage);
					}));
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
						var mainViewModel = new ViewModelLocator().Main;
						Application.Current.Properties["LastPageViewed"] = mainViewModel.CurrentPage.CurrentPage;
						DemoListView homeView = new DemoListView();
						mainViewModel.CurrentPage.ShowPage(homeView);
						Cleanup();
					}));
			}
		}

		/// <summary>
		/// Command to go to the rank stats page
		/// </summary>
		public RelayCommand GoToRankCommand
		{
			get
			{
				return _goToRankCommand
					?? (_goToRankCommand = new RelayCommand(
					() =>
					{
						var mainViewModel = new ViewModelLocator().Main;
						Application.Current.Properties["LastPageViewed"] = mainViewModel.CurrentPage.CurrentPage;
						AccountRankView rankView = new AccountRankView();
						mainViewModel.CurrentPage.ShowPage(rankView);
						Cleanup();
					}));
			}
		}

		/// <summary>
		/// Command to go to the maps stats page
		/// </summary>
		public RelayCommand GoToMapCommand
		{
			get
			{
				return _goToMapCommand
					?? (_goToMapCommand = new RelayCommand(
					() =>
					{
						var mainViewModel = new ViewModelLocator().Main;
						Application.Current.Properties["LastPageViewed"] = mainViewModel.CurrentPage.CurrentPage;
						AccountMapsView mapsView = new AccountMapsView();
						mainViewModel.CurrentPage.ShowPage(mapsView);
						Cleanup();
					}));
			}
		}

		/// <summary>
		/// Command to go to the weapon stats page
		/// </summary>
		public RelayCommand GoToWeaponCommand
		{
			get
			{
				return _goToWeaponCommand
					?? (_goToWeaponCommand = new RelayCommand(
					() =>
					{
						var mainViewModel = new ViewModelLocator().Main;
						Application.Current.Properties["LastPageViewed"] = mainViewModel.CurrentPage.CurrentPage;
						AccountWeaponsView weaponsView = new AccountWeaponsView();
						mainViewModel.CurrentPage.ShowPage(weaponsView);
						Cleanup();
					}));
			}
		}

		/// <summary>
		/// Command to go to the progression stats page
		/// </summary>
		public RelayCommand GoToProgressCommand
		{
			get
			{
				return _goToProgressCommand
					?? (_goToProgressCommand = new RelayCommand(
					() =>
					{
						var mainViewModel = new ViewModelLocator().Main;
						Application.Current.Properties["LastPageViewed"] = mainViewModel.CurrentPage.CurrentPage;
						AccountProgressView progressView = new AccountProgressView();
						mainViewModel.CurrentPage.ShowPage(progressView);
						Cleanup();
					}));
			}
		}

		#endregion

		private async Task LoadDatas()
		{
			IsBusy = true;
			Notification = Properties.Resources.NotificationLoading;
			List<Demo> demos = await _cacheService.GetFilteredDemoListAsync();
			OverallStats datas = await _accountStatsService.GetGeneralAccountStatsAsync(demos);
			MatchCount = datas.MatchCount;
			KillCount = datas.KillCount;
			AssistCount = datas.AssistCount;
			DeathCount = datas.DeathCount;
			HeadshotCount = datas.HeadshotCount;
			HeadshotRatio = datas.HeadshotRatio;
			KnifeKillCount = datas.KnifeKillCount;
			FiveKillCount = datas.FiveKillCount;
			FourKillCount = datas.FourKillCount;
			ThreeKillCount = datas.ThreeKillCount;
			TwoKillCount = datas.TwoKillCount;
			EntryKillCount = datas.EntryKillCount;
			KillDeathRatio = datas.KillDeathRatio;
			BombDefusedCount = datas.BombDefusedCount;
			BombExplodedCount = datas.BombExplodedCount;
			BombPlantedCount = datas.BombPlantedCount;
			KillPerRoundPercentage = datas.KillPerRoundPercentage;
			AssistPerRoundPercentage = datas.AssistPerRoundPercentage;
			DeathPerRoundPercentage = datas.DeathPerRoundPercentage;
			ClutchWinPercentage = datas.ClutchWinPercentage;
			AverageDamagesPerRound = datas.AverageDamagesPerRound;
			MvpCount = datas.MvpCount;
			DamageCount = datas.DamageCount;
			RoundCount = datas.RoundCount;
			HltvRating = datas.HltvRating;
			Hltv2Rating = datas.Hltv2Rating;
			EseaRws = datas.EseaRws;
			OneVersusOneCount = datas.OneVersusOneCount;
			OneVersusTwoCount = datas.OneVersusTwoCount;
			OneVersusThreeCount = datas.OneVersusThreeCount;
			OneVersusFourCount = datas.OneVersusFourCount;
			OneVersusFiveCount = datas.OneVersusFiveCount;
			TimeSpan totalTime = TimeSpan.FromSeconds(datas.TotalMatchesDuration);
			TotalMatchTime = string.Format(Properties.Resources.TotalMatchTimeValue, totalTime.Hours, totalTime.Minutes);
			TimeSpan avgTotalTime = TimeSpan.FromSeconds(datas.AverageMatchDuration);
			AverageMatchTime = string.Format(Properties.Resources.AverageMatchTimeValue, avgTotalTime.Minutes);
			DatasMatchStats = new List<GenericDoubleChart>
			{
				new GenericDoubleChart
				{
					Label = Properties.Resources.Win,
					Value = datas.MatchWinCount
				},
				new GenericDoubleChart
				{
					Label = Properties.Resources.Loss,
					Value = datas.MatchLossCount
				},
				new GenericDoubleChart
				{
					Label = Properties.Resources.Draw,
					Value = datas.MatchDrawCount
				}
			};
			IsBusy = false;
		}

		public AccountOverallViewModel(IAccountStatsService accountStatsService, ICacheService cacherService)
		{
			_accountStatsService = accountStatsService;
			_cacheService = cacherService;

			if (IsInDesignMode)
			{
				Application.Current.Dispatcher.Invoke(async () =>
				{
					await LoadDatas();
				});
			}
		}

		private void HandleSettingsFlyoutClosedMessage(SettingsFlyoutClosed msg)
		{
			DispatcherHelper.CheckBeginInvokeOnUI(
				async () =>
				{
					await LoadDatas();
				});
		}

		public override void Cleanup()
		{
			base.Cleanup();
			AssistCount = 0;
			BombDefusedCount = 0;
			BombExplodedCount = 0;
			BombPlantedCount = 0;
			DamageCount = 0;
			DatasMatchStats = null;
			DeathCount = 0;
			EntryKillCount = 0;
			FiveKillCount = 0;
			FourKillCount = 0;
			HeadshotCount = 0;
			HeadshotRatio = 0;
			KillCount = 0;
			KillDeathRatio = 0;
			TwoKillCount = 0;
			KnifeKillCount = 0;
			MvpCount = 0;
			MatchCount = 0;
			ThreeKillCount = 0;
			ClutchWinPercentage = 0;
			AssistPerRoundPercentage = 0;
			DeathPerRoundPercentage = 0;
			KillPerRoundPercentage = 0;
			AverageDamagesPerRound = 0;
			RoundCount = 0;
			EseaRws = 0;
			OneVersusFiveCount = 0;
			OneVersusFourCount = 0;
			OneVersusThreeCount = 0;
			OneVersusTwoCount = 0;
			OneVersusOneCount = 0;
		}
	}
}
