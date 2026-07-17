using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LoanManagementApp.Models;
using LoanManagementApp.Services;
using LoanManagementApp.Data;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// DashboardViewModel / ڈیش بورڈ ویو ماڈل - Main dashboard summary and statistics
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly LoanService _loanService;
        private readonly BackupService _backupService;
        private readonly PaymentRepository _paymentRepo;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public DashboardViewModel()
        {
            _loanService = new LoanService();
            _backupService = new BackupService();
            _paymentRepo = new PaymentRepository();

            RefreshCommand = new RelayCommand(ExecuteRefresh);
            BackupNowCommand = new RelayCommand(ExecuteBackupNow);
            LoadChartCommand = new RelayCommand(ExecuteLoadChart);

            OverdueLoans = new ObservableCollection<Loan>();
            MonthlyChartData = new ObservableCollection<MonthlyData>();

            // Static year range: from 2000 to 2100 (no future updates needed)
            int startYear = 2000;
            int endYear = 2100;
            YearsList = Enumerable.Range(startYear, endYear - startYear + 1).ToList();

            SelectedYear = DateTime.Today.Year;
            SelectedMonth = DateTime.Today.Month;
            SelectedMonthIndex = DateTime.Today.Month - 1;

            LoadDashboard();
        }

        // ─── Summary Cards | خلاصہ کارڈز ────────────────────────────

        private decimal _totalLoaned;
        public decimal TotalLoaned
        {
            get => _totalLoaned;
            set { SetProperty(ref _totalLoaned, value); OnPropertyChanged(nameof(TotalLoanedDisplay)); }
        }

        private decimal _totalRemaining;
        public decimal TotalRemaining
        {
            get => _totalRemaining;
            set { SetProperty(ref _totalRemaining, value); OnPropertyChanged(nameof(TotalRemainingDisplay)); }
        }

        private decimal _totalCollected;
        public decimal TotalCollected
        {
            get => _totalCollected;
            set { SetProperty(ref _totalCollected, value); OnPropertyChanged(nameof(TotalCollectedDisplay)); }
        }

        private int _totalActiveLoans;
        public int TotalActiveLoans
        {
            get => _totalActiveLoans;
            set => SetProperty(ref _totalActiveLoans, value);
        }

        private int _overdueCount;
        public int OverdueCount
        {
            get => _overdueCount;
            set => SetProperty(ref _overdueCount, value);
        }

        // ─── Display Formatters | ڈسپلے فارمیٹرز ────────────────────
        public string TotalLoanedDisplay => $"{TotalLoaned:N0} PKR | کل قرض";
        public string TotalRemainingDisplay => $"{TotalRemaining:N0} PKR | باقی وصولی";
        public string TotalCollectedDisplay => $"{TotalCollected:N0} PKR | وصول شدہ";

        // ─── Collections | مجموعے ────────────────────────────────────
        public ObservableCollection<Loan> OverdueLoans { get; }
        public ObservableCollection<MonthlyData> MonthlyChartData { get; }

        // ─── Last Backup Info | آخری بیک اپ معلومات ─────────────────
        private string _lastBackupDisplay = string.Empty;
        public string LastBackupDisplay
        {
            get => _lastBackupDisplay;
            set => SetProperty(ref _lastBackupDisplay, value);
        }

        // ─── Current User Info | موجودہ صارف ────────────────────────
        public string CurrentUsername => AuthService.CurrentUser?.Username ?? "Admin";
        public string WelcomeMessage => $"Welcome back, {CurrentUsername}! | خوش آمدید";
        public string TodayDisplay => $"{DateTime.Now:dddd, dd MMMM yyyy} | آج";

        // ─── Month/Year Selection for Chart | چارٹ کے لیے مہینہ/سال ──
        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set => SetProperty(ref _selectedYear, value);
        }

        private int _selectedMonth;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set => SetProperty(ref _selectedMonth, value);
        }

        private int _selectedMonthIndex;
        public int SelectedMonthIndex
        {
            get => _selectedMonthIndex;
            set
            {
                SetProperty(ref _selectedMonthIndex, value);
                // Convert index to month number (0-based index -> 1-based month)
                SelectedMonth = value + 1;
            }
        }

        public List<int> YearsList { get; }

        // ─── Commands | کمانڈز ──────────────────────────────────────
        public ICommand RefreshCommand { get; }
        public ICommand BackupNowCommand { get; }
        public ICommand LoadChartCommand { get; }

        // ─── Load Dashboard | ڈیش بورڈ لوڈ کریں ─────────────────────
        public void LoadDashboard()
        {
            try
            {
                IsBusy = true;

                var summary = _loanService.GetDashboardSummary();
                TotalLoaned = summary.TotalLoaned;
                TotalRemaining = summary.TotalRemaining;
                TotalCollected = summary.TotalCollected;
                TotalActiveLoans = summary.TotalLoans;

                OverdueLoans.Clear();
                var overdue = _loanService.GetOverdueLoans();
                foreach (var loan in overdue)
                    OverdueLoans.Add(loan);
                OverdueCount = OverdueLoans.Count;

                // Load chart for current selected month/year
                LoadChartData();

                _backupService.RunAutoBackupIfDue();
                ClearStatus();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load dashboard | ڈیش بورڈ لوڈ ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteRefresh(object? _) => LoadDashboard();

        private void ExecuteBackupNow(object? _)
        {
            try
            {
                IsBusy = true;
                var result = _backupService.CreateBackup();
                if (result.Success)
                    ShowSuccess(result.Message);
                else
                    ShowError(result.Message);
            }
            catch (Exception ex)
            {
                ShowError($"Backup failed | بیک اپ ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteLoadChart(object? _) => LoadChartData();

        /// <summary>
        /// Load daily collection data for the selected month/year and create bars for all days.
        /// منتخب مہینے/سال کے لیے روزانہ وصولی کا ڈیٹا لوڈ کریں اور تمام دنوں کے لیے بارز بنائیں۔
        /// </summary>
        private void LoadChartData()
        {
            try
            {
                int year = SelectedYear;
                int month = SelectedMonth;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                var dailyPayments = _paymentRepo.GetDailyCollectionForMonth(year, month);
                var paymentDict = dailyPayments.ToDictionary(d => d.Day, d => d.Amount);

                // Get max amount to scale bars
                decimal maxAmount = dailyPayments.Count > 0 ? dailyPayments.Max(d => d.Amount) : 1;

                MonthlyChartData.Clear();
                for (int day = 1; day <= daysInMonth; day++)
                {
                    decimal amount = paymentDict.ContainsKey(day) ? paymentDict[day] : 0;
                    // Calculate relative height as percentage of max (capped at 160px)
                    double relativeHeight = maxAmount > 0 ? (double)(amount / maxAmount) * 160 : 0;
                    relativeHeight = Math.Max(4, Math.Min(160, relativeHeight)); // min 4px, max 160px

                    MonthlyChartData.Add(new MonthlyData
                    {
                        Month = day.ToString(),
                        Amount = amount,
                        RelativeHeight = relativeHeight
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load chart data | چارٹ ڈیٹا لوڈ ناکام: {ex.Message}");
            }
        }
    }

    // ─── Monthly Chart Data Helper | ماہانہ چارٹ مددگار ────────────
    public class MonthlyData
    {
        /// <summary>Month name or day number | مہینے کا نام یا دن کا نمبر</summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>Collection amount | وصولی کی رقم</summary>
        public decimal Amount { get; set; }

        /// <summary>Amount display | رقم فارمیٹ</summary>
        public string AmountDisplay => $"{Amount:N0}";

        /// <summary>Relative height for bar (0-160) | بار کی نسبتاً اونچائی</summary>
        public double RelativeHeight { get; set; }
    }
}