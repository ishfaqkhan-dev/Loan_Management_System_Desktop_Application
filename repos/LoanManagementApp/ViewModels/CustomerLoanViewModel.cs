using LoanManagementApp.Data;
using LoanManagementApp.Helpers;
using LoanManagementApp.Models;
using LoanManagementApp.Services;
using LoanManagementApp.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace LoanManagementApp.ViewModels
{
    public class CustomerLoanViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly LoanService _loanService;
        private readonly CustomerRepository _customerRepo;
        private Customer? _customer;

        // ─── All loans for this customer | اس قرض دار کے تمام قرضے ──
        private ObservableCollection<Loan> _customerLoans = new ObservableCollection<Loan>();
        public ObservableCollection<Loan> CustomerLoans
        {
            get => _customerLoans;
            set => SetProperty(ref _customerLoans, value);
        }

        private int _selectedLoanId;
        public int SelectedLoanId
        {
            get => _selectedLoanId;
            set
            {
                if (SetProperty(ref _selectedLoanId, value))
                {
                    var loan = CustomerLoans.FirstOrDefault(l => l.Id == value);
                    if (loan != null && loan != _selectedLoan)
                    {
                        SelectedLoan = loan;
                    }
                }
            }
        }

        private Loan? _selectedLoan;
        public Loan? SelectedLoan
        {
            get => _selectedLoan;
            set
            {
                if (SetProperty(ref _selectedLoan, value))
                {
                    if (value != null)
                    {
                        // Keep SelectedLoanId in sync without triggering a loop
                        // SelectedLoanId کو لوپ کے بغیر ہم آہنگ رکھیں
                        if (_selectedLoanId != value.Id)
                            _selectedLoanId = value.Id;

                        LoadLoanData(value);
                        UpdateAddLoanButtonText(value);
                    }
                    else
                    {
                        _selectedLoanId = 0;
                    }
                }
            }
        }

        private string _addLoanButtonText = "➕ Add More Loan | مزید قرض شامل کریں";
        public string AddLoanButtonText
        {
            get => _addLoanButtonText;
            set => SetProperty(ref _addLoanButtonText, value);
        }

        private void UpdateAddLoanButtonText(Loan loan)
        {
            if (loan.Status == LoanStatus.Active)
                AddLoanButtonText = "➕ Add More Loan | مزید قرض شامل کریں";
            else
                AddLoanButtonText = "➕ Add New Loan | نیا قرض شامل کریں";
        }

        // ─── Properties for the currently displayed loan | موجودہ قرض کی پراپرٹیز ──
        private Loan? _activeLoan;
        private decimal _totalLoanAmount;
        private decimal _totalPaidAmount;
        private decimal _totalRemainingBalance;
        private double _progressPercentage;
        private string _loanStatusText = string.Empty;
        private string _activeLoanDisplay = string.Empty;
        private string _activeLoanDates = string.Empty;
        private bool _hasActiveLoan;

        public string TotalLoanDisplay => $"{_totalLoanAmount:N0} PKR | کل قرض";
        public string TotalPaidDisplay => $"{_totalPaidAmount:N0} PKR | کل ادا شدہ";
        public string TotalRemainingDisplay => $"{_totalRemainingBalance:N0} PKR | کل باقی";
        public double ProgressPercentage => _progressPercentage;
        public string LoanStatusText => _loanStatusText;
        public bool HasActiveLoan => _hasActiveLoan;
        public string ActiveLoanDisplay => _activeLoanDisplay;
        public string ActiveLoanDates => _activeLoanDates;

        // ─── Collections | مجموعے ────────────────────────────────────
        public ObservableCollection<InstallmentItem> Installments { get; }
        public ObservableCollection<Payment> PaymentHistory { get; }

        // ─── Commands | کمانڈز ──────────────────────────────────────
        public ICommand PayInstallmentCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand RecordCustomPaymentCommand { get; }
        public ICommand AddMoreLoanCommand { get; }
        public ICommand PrintStatementCommand { get; }
        public ICommand ViewPaymentDetailCommand { get; }

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public CustomerLoanViewModel()
        {
            _loanService = new LoanService();
            _customerRepo = new CustomerRepository();
            Installments = new ObservableCollection<InstallmentItem>();
            PaymentHistory = new ObservableCollection<Payment>();

            PayInstallmentCommand = new RelayCommand(ExecutePayInstallment);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            RecordCustomPaymentCommand = new RelayCommand(ExecuteRecordCustomPayment);
            AddMoreLoanCommand = new RelayCommand(ExecuteAddMoreLoan);
            PrintStatementCommand = new RelayCommand(ExecutePrintStatement);
            ViewPaymentDetailCommand = new RelayCommand(ExecuteViewPaymentDetail);
        }

        // ═══════════════════════════════════════════════════════════════
        // PUBLIC METHODS | عوامی طریقے
        // ═══════════════════════════════════════════════════════════════

        // ─── Load Customer Data | قرض دار کا ڈیٹا لوڈ کریں ───────────
        public void LoadCustomer(Customer? customer)
        {
            if (customer == null) return;
            _customer = _customerRepo.GetCustomerById(customer.Id);
            if (_customer == null) return;

            // Remember currently selected loan ID so we can restore it after refresh
            // موجودہ منتخب قرض کی آئی ڈی یاد رکھیں تاکہ ریفریش کے بعد بحال کر سکیں
            int previouslySelectedLoanId = _selectedLoanId;

            CustomerLoans.Clear();
            var allLoans = _loanService.GetLoansByCustomer(_customer.Id);
            foreach (var loan in allLoans.OrderByDescending(l => l.CreatedAt))
                CustomerLoans.Add(loan);

            if (CustomerLoans.Count > 0)
            {
                // If we had a loan selected before, try to restore the same selection
                // اگر پہلے کوئی قرض منتخب تھا تو اسی کو دوبارہ منتخب کریں
                if (previouslySelectedLoanId != 0 &&
                    CustomerLoans.Any(l => l.Id == previouslySelectedLoanId))
                {
                    // Restore same loan selection — do NOT switch to active loan on refresh
                    // ریفریش پر پرانا منتخب قرض بحال کریں
                    SelectedLoan = CustomerLoans.First(l => l.Id == previouslySelectedLoanId);
                }
                else
                {
                    // First load: default to active loan, or first in list
                    // پہلی بار: فعال قرض یا پہلا قرض منتخب کریں
                    var activeLoan = _loanService.GetActiveLoan(_customer.Id);
                    if (activeLoan != null)
                        SelectedLoan = CustomerLoans.FirstOrDefault(l => l.Id == activeLoan.Id)
                                       ?? CustomerLoans.First();
                    else
                        SelectedLoan = CustomerLoans.First();
                }
            }
            else
            {
                SelectedLoan = null;
                _selectedLoanId = 0;
            }

            OnPropertyChanged(nameof(PageTitle));
        }

        public string PageTitle => $"{_customer?.FullName} — Loan Details | قرض کی تفصیلات";

        // ─── Force Reload Current Loan | موجودہ قرض کو زبردستی دوبارہ لوڈ کریں ──
        private void ReloadCurrentLoan()
        {
            if (_selectedLoan == null) return;
            // Reload the loan from database
            var freshLoan = _loanService.GetLoanById(_selectedLoan.Id);
            if (freshLoan != null)
            {
                // Update the in-memory loan reference
                _selectedLoan = freshLoan;
                LoadLoanData(freshLoan);
            }
        }

        private void LoadLoanData(Loan loan)
        {
            if (loan == null) return;

            _totalLoanAmount = loan.TotalAmount;
            _totalPaidAmount = loan.PaidAmount;
            _totalRemainingBalance = loan.RemainingAmount;
            _progressPercentage = loan.TotalAmount > 0 ? (double)(loan.PaidAmount / loan.TotalAmount) * 100 : 0;
            _loanStatusText = loan.RemainingAmount > 0 ? "Active | فعال" : "Closed | بند";
            _activeLoan = loan;
            _hasActiveLoan = loan.Status == LoanStatus.Active;
            _activeLoanDisplay = $"Loan #{loan.LoanNumber} | Total: {loan.TotalAmount:N0} PKR | Remaining: {loan.RemainingAmount:N0} PKR";
            _activeLoanDates = $"Start: {loan.StartDate:dd-MMM-yyyy} | End: {loan.EndDate:dd-MMM-yyyy}";

            OnPropertyChanged(nameof(TotalLoanDisplay));
            OnPropertyChanged(nameof(TotalPaidDisplay));
            OnPropertyChanged(nameof(TotalRemainingDisplay));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(LoanStatusText));
            OnPropertyChanged(nameof(HasActiveLoan));
            OnPropertyChanged(nameof(ActiveLoanDisplay));
            OnPropertyChanged(nameof(ActiveLoanDates));

            // Load installments for this loan
            Installments.Clear();
            var paymentsForLoan = _loanService.GetPaymentsByLoan(loan.Id);
            bool isLoanClosed = loan.RemainingAmount == 0 || loan.Status == LoanStatus.Closed;

            for (int i = 1; i <= loan.TotalInstallments; i++)
            {
                var payment = paymentsForLoan.FirstOrDefault(p => p.InstallmentNumber == i);
                bool isPaid = payment != null || isLoanClosed;
                decimal displayAmount = isPaid ? (payment?.PaidAmount ?? 0) : loan.InstallmentAmount;

                if (isLoanClosed && payment == null)
                    displayAmount = 0;

                Installments.Add(new InstallmentItem
                {
                    InstallmentNumber = i,
                    Amount = displayAmount,
                    IsPaid = isPaid,
                    LoanId = loan.Id,
                    PaidDate = payment?.PaymentDate
                });
            }

            // Load payment history for this specific loan
            PaymentHistory.Clear();
            foreach (var p in paymentsForLoan.OrderBy(p => p.CreatedAt))
                PaymentHistory.Add(p);
        }

        // ═══════════════════════════════════════════════════════════════
        // PRIVATE COMMAND HANDLERS | پرائیویٹ کمانڈ ہینڈلرز
        // ═══════════════════════════════════════════════════════════════

        private void NotifyCustomerUpdated()
        {
            if (_customer != null)
                CustomerNotifier.NotifyCustomerUpdated(_customer.Id);
        }

        // ─── Execute Pay Installment (FIXED) | قسط ادا کریں (درست شدہ) ──
        private async void ExecutePayInstallment(object? parameter)
        {
            if (parameter is not InstallmentItem installment) return;
            if (installment.IsPaid) return;
            if (_selectedLoan == null) return;
            if (_customer == null) return;

            var loan = _loanService.GetLoanById(installment.LoanId);
            if (loan == null) return;

            var confirm = ConfirmationDialog.AskConfirm(
                "Pay Installment",
                $"Pay installment #{installment.InstallmentNumber} of {installment.Amount:N0} PKR?",
                isWarning: false,
                confirmText: "✅ Pay Now",
                cancelText: "Cancel");
            confirm.Owner = Application.Current.MainWindow;
            if (confirm.ShowDialog() != true) return;

            try
            {
                IsBusy = true;
                var result = _loanService.RecordPayment(
                    loan.Id, _customer.Id, installment.Amount, DateTime.Now,
                    PaymentType.Cash, $"Installment #{installment.InstallmentNumber}",
                    AuthService.CurrentUser?.Username ?? "System",
                    $"Paid installment {installment.InstallmentNumber}");

                if (result.Success)
                {
                    // Refresh only the current loan data, not all loans
                    ReloadCurrentLoan();
                    NotifyCustomerUpdated();
                    ShowSuccess($"Installment #{installment.InstallmentNumber} paid!");
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Payment failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Execute Refresh Command | ریفریش کمانڈ چلائیں ───────────
        private void ExecuteRefresh(object? _)
        {
            if (_customer != null)
            {
                LoadCustomer(_customer);
                // If a loan was selected, force reload its data (in case same ID)
                if (_selectedLoan != null)
                    ReloadCurrentLoan();
            }
        }

        private void ExecuteRecordCustomPayment(object? _)
        {
            if (_selectedLoan == null || _customer == null) return;

            var dialog = new RecordPaymentDialog(_selectedLoan, _customer.FullName);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                ExecuteRefresh(null);
                NotifyCustomerUpdated();
            }
        }

        private async void ExecuteAddMoreLoan(object? _)
        {
            if (_customer == null) return;

            // PIN verification | پن تصدیق
            var pinDialog = new PinVerificationDialog();
            pinDialog.Owner = Application.Current.MainWindow;
            if (pinDialog.ShowDialog() != true) return;

            var dialog = new AddLoanDialog(_customer.Id, _customer.FullName);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                // Reset selected loan ID so LoadCustomer treats next load as "first load"
                // and auto-selects the newly added active loan from the list.
                // منتخب قرض آئی ڈی ری سیٹ کریں تاکہ لوڈ کسٹمر نئے فعال قرض کو خود بخود منتخب کرے
                _selectedLoanId = 0;

                await System.Threading.Tasks.Task.Delay(300);
                ExecuteRefresh(null);
                NotifyCustomerUpdated();
            }
        }

        private void ExecutePrintStatement(object? _)
        {
            if (_customer == null || _selectedLoan == null) return;

            var paymentsForLoan = _loanService.GetPaymentsByLoan(_selectedLoan.Id);
            paymentsForLoan = paymentsForLoan.OrderBy(p => p.CreatedAt).ToList();

            var printWindow = new PrintStatementWindow(_customer, _selectedLoan, paymentsForLoan);
            printWindow.Owner = Application.Current.MainWindow;
            printWindow.ShowDialog();
        }

        private void ExecuteViewPaymentDetail(object? parameter)
        {
            if (parameter is Payment payment)
            {
                var detailDialog = new PaymentDetailDialog(payment);
                detailDialog.Owner = Application.Current.MainWindow;
                detailDialog.ShowDialog();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // NESTED CLASS: InstallmentItem | اقساط کے لیے اندرونی کلاس
        // ═══════════════════════════════════════════════════════════════
        public class InstallmentItem : BaseViewModel
        {
            public int InstallmentNumber { get; set; }
            public decimal Amount { get; set; }
            public bool IsPaid { get; set; }
            public int LoanId { get; set; }
            public DateTime? PaidDate { get; set; }

            public string AmountDisplay => $"{Amount:N0} PKR";
            public string ButtonText => IsPaid ? "✓ Paid" : "Pay Now";
            public Visibility ShowPayButton => IsPaid ? Visibility.Collapsed : Visibility.Visible;

            public Brush BackgroundColor => IsPaid
                ? (Brush)Application.Current.Resources["SuccessLightBrush"]
                : (Brush)Application.Current.Resources["CardBackgroundBrush"];

            public Brush BorderColor => IsPaid
                ? (Brush)Application.Current.Resources["SuccessBrush"]
                : (Brush)Application.Current.Resources["BorderBrush"];

            public Brush TextColor => IsPaid
                ? (Brush)Application.Current.Resources["SuccessForegroundBrush"]
                : (Brush)Application.Current.Resources["TextPrimaryBrush"];
        }
    }
}