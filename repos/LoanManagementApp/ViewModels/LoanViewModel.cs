using System;
using System.Windows.Input;
using LoanManagementApp.Models;
using LoanManagementApp.Services;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// LoanViewModel / قرض ویو ماڈل - Add new loan or merge with existing loan
    /// نیا قرض شامل کریں یا موجودہ قرض میں ملائیں
    /// </summary>
    public class LoanViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly LoanService _loanService;
        private readonly ValidationService _validationService;

        // ─── Customer Context | قرض دار سیاق ────────────────────────
        private int _customerId;
        private string _customerName = string.Empty;

        /// <summary>
        /// Customer display name | قرض دار ڈسپلے نام
        /// </summary>
        public string CustomerName
        {
            get => _customerName;
            private set => SetProperty(ref _customerName, value);
        }

        // ─── Existing Loan Info | موجودہ قرض معلومات ─────────────────

        private bool _hasExistingLoan = false;

        /// <summary>
        /// Does customer already have an active loan | کیا قرض دار کا فعال قرض موجود ہے
        /// </summary>
        public bool HasExistingLoan
        {
            get => _hasExistingLoan;
            set
            {
                SetProperty(ref _hasExistingLoan, value);
                OnPropertyChanged(nameof(MergeInfoVisible));
            }
        }

        private decimal _existingRemainingBalance;

        /// <summary>
        /// Existing loan remaining balance | موجودہ قرض کی باقی رقم
        /// </summary>
        public decimal ExistingRemainingBalance
        {
            get => _existingRemainingBalance;
            set
            {
                SetProperty(ref _existingRemainingBalance, value);
                OnPropertyChanged(nameof(MergeInfoDisplay));
            }
        }

        /// <summary>
        /// Show merge info panel | ضم معلومات پینل دکھائیں
        /// </summary>
        public bool MergeInfoVisible => HasExistingLoan;

        /// <summary>
        /// Merge info display message | ضم معلومات پیغام
        /// </summary>
        public string MergeInfoDisplay =>
            $"⚠️ Existing balance: {ExistingRemainingBalance:N0} PKR will be merged | " +
            $"موجودہ بقایا: {ExistingRemainingBalance:N0} PKR ضم ہو گا";

        // ─── Form Fields | فارم فیلڈز ────────────────────────────────

        private decimal _totalAmount;

        /// <summary>
        /// New loan total amount | نئے قرض کی کل رقم
        /// </summary>
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                SetProperty(ref _totalAmount, value);
                CalculateInstallmentAmount();
                OnPropertyChanged(nameof(MergedTotalDisplay));
            }
        }

        private int _totalInstallments;

        /// <summary>
        /// Total installments count | کل اقساط کی تعداد
        /// </summary>
        public int TotalInstallments
        {
            get => _totalInstallments;
            set
            {
                SetProperty(ref _totalInstallments, value);
                CalculateInstallmentAmount();
            }
        }

        private decimal _installmentAmount;

        /// <summary>
        /// Per installment amount | فی قسط رقم
        /// </summary>
        public decimal InstallmentAmount
        {
            get => _installmentAmount;
            set => SetProperty(ref _installmentAmount, value);
        }

        private DateTime _startDate = DateTime.Today;

        /// <summary>
        /// Loan start date | قرض شروع کی تاریخ
        /// </summary>
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today.AddMonths(12);

        /// <summary>
        /// Loan end date | قرض آخری تاریخ
        /// </summary>
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private string _notes = string.Empty;

        /// <summary>
        /// Notes / remarks | نوٹس
        /// </summary>
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // ─── Merged Total Display | ضم شدہ کل رقم ────────────────────

        /// <summary>
        /// Merged total amount display | ضم شدہ کل رقم ڈسپلے
        /// </summary>
        public string MergedTotalDisplay => HasExistingLoan
            ? $"New merged total: {ExistingRemainingBalance + TotalAmount:N0} PKR | " +
              $"نئی ضم شدہ کل: {ExistingRemainingBalance + TotalAmount:N0} PKR"
            : $"Loan total: {TotalAmount:N0} PKR | قرض کل: {TotalAmount:N0} PKR";

        // ─── Events | ایونٹس ─────────────────────────────────────────

        /// <summary>
        /// Raised on successful save | کامیاب محفوظ پر
        /// </summary>
        public event Action<int>? SaveSucceeded;

        /// <summary>
        /// Raised on cancel | منسوخی پر
        /// </summary>
        public event Action? Cancelled;

        // ─── Commands | کمانڈز ──────────────────────────────────────

        /// <summary>Save loan command | قرض محفوظ کمانڈ</summary>
        public ICommand SaveCommand { get; }

        /// <summary>Cancel command | منسوخ کمانڈ</summary>
        public ICommand CancelCommand { get; }

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public LoanViewModel()
        {
            _loanService = new LoanService();
            _validationService = new ValidationService();

            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(_ => Cancelled?.Invoke());
        }

        // ─── Initialize for Customer | قرض دار کے لیے شروع کریں ─────
        /// <summary>
        /// Set customer context and check existing loans
        /// قرض دار سیاق سیٹ کریں اور موجودہ قرضے جانچیں
        /// </summary>
        public void InitializeForCustomer(int customerId, string customerName)
        {
            _customerId = customerId;
            CustomerName = customerName;

            // Check active loan | فعال قرض جانچیں
            var activeLoan = _loanService.GetActiveLoan(customerId);
            HasExistingLoan = activeLoan != null;
            ExistingRemainingBalance = activeLoan?.RemainingAmount ?? 0;

            ResetForm();
        }

        // ─── Reset Form | فارم دوبارہ سیٹ کریں ──────────────────────
        /// <summary>
        /// Reset all form fields | تمام فارم فیلڈز دوبارہ سیٹ کریں
        /// </summary>
        public void ResetForm()
        {
            TotalAmount = 0;
            TotalInstallments = 0;
            InstallmentAmount = 0;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(12);
            Notes = string.Empty;
            ClearStatus();
        }

        // ─── Calculate Installment | قسط حساب کریں ──────────────────
        private void CalculateInstallmentAmount()
        {
            InstallmentAmount = _loanService.CalculateInstallmentAmount(TotalAmount, TotalInstallments);
        }

        // ─── Execute Save | محفوظ چلائیں ─────────────────────────────
        // ─── Execute Save | محفوظ چلائیں ─────────────────────────────
        private void ExecuteSave(object? _)
        {
            try
            {
                IsBusy = true;

                // Special case: Merge amount only (no new installments)
                if (HasExistingLoan && TotalInstallments == 0)
                {
                    // No validation needed here; the service will validate.
                    var mergeResult = _loanService.MergeLoanAmountOnly(_customerId, TotalAmount, Notes.Trim());
                    if (mergeResult.Success)
                    {
                        ShowSuccess(mergeResult.Message);
                        SaveSucceeded?.Invoke(mergeResult.LoanId);
                    }
                    else
                    {
                        ShowError(mergeResult.Message);
                    }
                    return;
                }

                // Normal case: either fresh loan or merge with new installments
                // Build loan for validation | جانچ کے لیے قرض بنائیں
                var loan = new Loan
                {
                    CustomerId = _customerId,
                    TotalAmount = TotalAmount,
                    TotalInstallments = TotalInstallments,
                    InstallmentAmount = InstallmentAmount,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Notes = Notes.Trim()
                };

                var validation = _validationService.ValidateLoan(loan);
                if (!validation.IsValid)
                {
                    ShowError(validation.Message);
                    return;
                }

                // Normal merge (may increase installments) or fresh loan
                var addResult = _loanService.AddLoanOnExisting(
                    _customerId,
                    TotalAmount,
                    TotalInstallments,
                    InstallmentAmount,
                    StartDate,
                    EndDate,
                    Notes.Trim());

                if (addResult.Success)
                {
                    ShowSuccess(addResult.Message);
                    SaveSucceeded?.Invoke(addResult.LoanId);
                }
                else
                {
                    ShowError(addResult.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Save failed | محفوظ ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanExecuteSave(object? _)
        {
            if (IsBusy) return false;
            if (TotalAmount <= 0) return false;

            // Special case: merging amount only (existing active loan, zero installments)
            if (HasExistingLoan && TotalInstallments == 0)
                return true;   // InstallmentAmount will be recalculated by service

            // Normal case: require positive installments and calculated installment amount
            return TotalInstallments > 0 && InstallmentAmount > 0;
        }
    }
}