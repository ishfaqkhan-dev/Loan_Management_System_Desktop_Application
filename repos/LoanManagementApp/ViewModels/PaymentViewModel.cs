using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LoanManagementApp.Models;
using LoanManagementApp.Services;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// PaymentViewModel / ادائیگی ویو ماڈل - Record installment payment dialog
    /// قسط ادائیگی ریکارڈ کرنے کا ڈائیلاگ
    /// </summary>
    public class PaymentViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly LoanService _loanService;
        private readonly ValidationService _validationService;

        // ─── Loan Context | قرض سیاق ─────────────────────────────────
        private int _loanId;
        private int _customerId;

        private string _customerName = string.Empty;

        /// <summary>
        /// Customer name display | قرض دار نام ڈسپلے
        /// </summary>
        public string CustomerName
        {
            get => _customerName;
            private set => SetProperty(ref _customerName, value);
        }

        private decimal _remainingBalance;

        /// <summary>
        /// Current remaining loan balance | موجودہ باقی قرض بیلنس
        /// </summary>
        public decimal RemainingBalance
        {
            get => _remainingBalance;
            private set
            {
                SetProperty(ref _remainingBalance, value);
                OnPropertyChanged(nameof(RemainingBalanceDisplay));
            }
        }

        /// <summary>
        /// Remaining balance display | باقی بیلنس ڈسپلے
        /// </summary>
        public string RemainingBalanceDisplay =>
            $"{RemainingBalance:N0} PKR | باقی رقم";

        private int _remainingInstallments;

        /// <summary>
        /// Remaining installments count | باقی اقساط کی تعداد
        /// </summary>
        public int RemainingInstallments
        {
            get => _remainingInstallments;
            private set
            {
                SetProperty(ref _remainingInstallments, value);
                OnPropertyChanged(nameof(InstallmentInfoDisplay));
            }
        }

        private decimal _suggestedAmount;

        /// <summary>
        /// Suggested installment amount | تجویز کردہ قسط رقم
        /// </summary>
        public decimal SuggestedAmount
        {
            get => _suggestedAmount;
            private set => SetProperty(ref _suggestedAmount, value);
        }

        /// <summary>
        /// Installment progress info | قسط پیشرفت معلومات
        /// </summary>
        public string InstallmentInfoDisplay =>
            $"Remaining installments: {RemainingInstallments} | " +
            $"باقی اقساط: {RemainingInstallments}";

        // ─── Form Fields | فارم فیلڈز ────────────────────────────────

        private decimal _paidAmount;

        /// <summary>
        /// Payment amount entered | درج کی گئی ادائیگی رقم
        /// </summary>
        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                SetProperty(ref _paidAmount, value);
                OnPropertyChanged(nameof(BalanceAfterPaymentDisplay));
                ClearStatus();
            }
        }

        /// <summary>
        /// Balance after this payment | اس ادائیگی کے بعد بیلنس
        /// </summary>
        public string BalanceAfterPaymentDisplay
        {
            get
            {
                decimal after = RemainingBalance - PaidAmount;
                if (after < 0) return "⚠️ Over payment! | زیادہ ادائیگی!";
                return $"Balance after: {after:N0} PKR | ادائیگی کے بعد: {after:N0} PKR";
            }
        }

        private DateTime _paymentDate = DateTime.Today;

        /// <summary>
        /// Payment date | ادائیگی کی تاریخ
        /// </summary>
        public DateTime PaymentDate
        {
            get => _paymentDate;
            set => SetProperty(ref _paymentDate, value);
        }

        private PaymentType _paymentType = PaymentType.Cash;

        /// <summary>
        /// Payment type | ادائیگی کی قسم
        /// </summary>
        public PaymentType SelectedPaymentType
        {
            get => _paymentType;
            set
            {
                SetProperty(ref _paymentType, value);
                // Update payment method options when payment type changes
                UpdatePaymentMethodOptions();
            }
        }

        private string _paymentMethod = string.Empty;

        /// <summary>
        /// Payment method description | ادائیگی طریقے کی تفصیل
        /// </summary>
        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        private string _receivedBy = string.Empty;

        /// <summary>
        /// Staff who received payment | ادائیگی وصول کرنے والا
        /// Auto-filled with current username, read-only
        /// </summary>
        public string ReceivedBy
        {
            get => _receivedBy;
            set => SetProperty(ref _receivedBy, value);
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

        // ─── Payment Method Options (dynamic based on Payment Type) ───
        private ObservableCollection<string> _paymentMethodOptions = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentMethodOptions
        {
            get => _paymentMethodOptions;
            set => SetProperty(ref _paymentMethodOptions, value);
        }

        // ─── Notes Options (predefined common notes) ─────────────────
        private ObservableCollection<string> _notesOptions = new ObservableCollection<string>();
        public ObservableCollection<string> NotesOptions
        {
            get => _notesOptions;
            set => SetProperty(ref _notesOptions, value);
        }

        // ─── Payment Types for ComboBox | ادائیگی قسم کے اختیارات ──
        /// <summary>
        /// All payment type options | تمام ادائیگی قسم کے اختیارات
        /// </summary>
        public PaymentType[] PaymentTypes =>
            (PaymentType[])Enum.GetValues(typeof(PaymentType));

        // ─── Events | ایونٹس ─────────────────────────────────────────

        /// <summary>
        /// Raised on successful payment | کامیاب ادائیگی پر
        /// </summary>
        public event Action<int>? PaymentSucceeded;

        /// <summary>
        /// Raised on cancel | منسوخی پر
        /// </summary>
        public event Action? Cancelled;

        // ─── Commands | کمانڈز ──────────────────────────────────────

        /// <summary>Record payment command | ادائیگی ریکارڈ کمانڈ</summary>
        public ICommand RecordPaymentCommand { get; }

        /// <summary>Fill suggested amount command | تجویز کردہ رقم بھریں</summary>
        public ICommand FillSuggestedCommand { get; }

        /// <summary>Cancel command | منسوخ کمانڈ</summary>
        public ICommand CancelCommand { get; }

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public PaymentViewModel()
        {
            _loanService = new LoanService();
            _validationService = new ValidationService();

            RecordPaymentCommand = new RelayCommand(ExecuteRecordPayment, CanExecuteRecord);
            FillSuggestedCommand = new RelayCommand(ExecuteFillSuggested);
            CancelCommand = new RelayCommand(_ => Cancelled?.Invoke());

            // Initialize notes options
            NotesOptions.Add("— Select or type —");
            NotesOptions.Add("Customer paid in person | گاہک نے خود ادا کیا");
            NotesOptions.Add("Payment via agent | ایجنٹ کے ذریعے ادائیگی");
            NotesOptions.Add("Partial payment agreed | قسط کی جزوی ادائیگی طے شدہ");
            NotesOptions.Add("Late payment fee applied | دیر سے ادائیگی پر چارج");
            NotesOptions.Add("Advance payment for next installment | اگلی قسط کی پیشگی ادائیگی");
        }

        // ─── Initialize for Loan | قرض کے لیے شروع کریں ─────────────
        /// <summary>
        /// Set loan context for payment dialog
        /// ادائیگی ڈائیلاگ کے لیے قرض سیاق سیٹ کریں
        /// </summary>
        public void InitializeForLoan(Loan loan, string customerName)
        {
            _loanId = loan.Id;
            _customerId = loan.CustomerId;
            CustomerName = customerName;

            RemainingBalance = loan.RemainingAmount;
            RemainingInstallments = loan.RemainingInstallments;
            SuggestedAmount = loan.InstallmentAmount;

            // Auto-fill Received By with current logged-in username (read-only)
            ReceivedBy = AuthService.CurrentUser?.Username ?? "Admin";

            // Initialize payment method options based on default payment type
            UpdatePaymentMethodOptions();

            ResetForm();
        }

        // ─── Update Payment Method Options based on selected Payment Type ──
        private void UpdatePaymentMethodOptions()
        {
            PaymentMethodOptions.Clear();

            switch (SelectedPaymentType)
            {
                case PaymentType.Cash:
                    PaymentMethodOptions.Add("— Select or type —");
                    PaymentMethodOptions.Add("Cash in hand | نقد");
                    PaymentMethodOptions.Add("Counter payment | کاؤنٹر ادائیگی");
                    PaymentMethodOptions.Add("Door collection | دروازے پر وصولی");
                    break;

                case PaymentType.Bank:
                    PaymentMethodOptions.Add("— Select or type —");
                    PaymentMethodOptions.Add("Bank Transfer | بینک ٹرانسفر");
                    PaymentMethodOptions.Add("IBFT (Inter Bank Fund Transfer)");
                    PaymentMethodOptions.Add("RTGS / NIFT");
                    PaymentMethodOptions.Add("Cheque Deposit | چیک جمع");
                    break;

                case PaymentType.Online:
                    PaymentMethodOptions.Add("— Select or type —");
                    PaymentMethodOptions.Add("JazzCash");
                    PaymentMethodOptions.Add("EasyPaisa");
                    PaymentMethodOptions.Add("Credit / Debit Card");
                    PaymentMethodOptions.Add("PayPal");
                    PaymentMethodOptions.Add("Other Online Wallet");
                    break;

                case PaymentType.Cheque:
                    PaymentMethodOptions.Add("— Select or type —");
                    PaymentMethodOptions.Add("On-Date Cheque");
                    PaymentMethodOptions.Add("Post-Dated Cheque");
                    PaymentMethodOptions.Add("Crossed Cheque");
                    break;

                default:
                    PaymentMethodOptions.Add("— Select or type —");
                    break;
            }

            // Clear current selection
            PaymentMethod = null!;
        }

        // ─── Reset Form | فارم دوبارہ سیٹ کریں ──────────────────────
        private void ResetForm()
        {
            PaidAmount = 0;
            PaymentDate = DateTime.Today;
            SelectedPaymentType = PaymentType.Cash;
            PaymentMethod = null!;
            Notes = null!;
            ClearStatus();
        }

        // ─── Fill Suggested Amount | تجویز کردہ رقم بھریں ───────────
        private void ExecuteFillSuggested(object? _)
        {
            PaidAmount = SuggestedAmount > 0
                ? Math.Min(SuggestedAmount, RemainingBalance)
                : RemainingBalance;
        }

        // ─── Execute Record Payment | ادائیگی ریکارڈ چلائیں ─────────
        private void ExecuteRecordPayment(object? _)
        {
            try
            {
                IsBusy = true;

                // Validate | جانچیں
                var validation = _validationService.ValidatePayment(
                    PaidAmount, RemainingBalance);

                if (!validation.IsValid)
                {
                    ShowError(validation.Message);
                    return;
                }

                // Record payment | ادائیگی ریکارڈ کریں
                var result = _loanService.RecordPayment(
                    _loanId,
                    _customerId,
                    PaidAmount,
                    PaymentDate,
                    SelectedPaymentType,
                    (PaymentMethod ?? string.Empty).Trim(),
                    ReceivedBy.Trim(),
                    (Notes ?? string.Empty).Trim());

                if (result.Success)
                {
                    ShowSuccess(result.Message);
                    PaymentSucceeded?.Invoke(result.PaymentId);
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Payment failed | ادائیگی ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanExecuteRecord(object? _) =>
            !IsBusy && PaidAmount > 0 && PaidAmount <= RemainingBalance;
    }
}