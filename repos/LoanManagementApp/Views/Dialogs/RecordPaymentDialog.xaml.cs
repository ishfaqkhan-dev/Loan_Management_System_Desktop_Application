using System.Windows;
using LoanManagementApp.Models;
using LoanManagementApp.ViewModels;

namespace LoanManagementApp.Views.Dialogs
{
    /// <summary>
    /// RecordPaymentDialog.xaml — Code Behind
    /// ادائیگی ریکارڈ ڈائیلاگ — کوڈ بیہائنڈ
    /// </summary>
    public partial class RecordPaymentDialog : Window
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ──────────────────
        private readonly PaymentViewModel _vm;

        // ─── Result: saved payment ID | نتیجہ: محفوظ ادائیگی نمبر ──
        /// <summary>
        /// Payment ID after successful record — 0 if cancelled
        /// کامیاب ریکارڈ کے بعد ادائیگی نمبر — منسوخی پر 0
        /// </summary>
        public int SavedPaymentId { get; private set; } = 0;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public RecordPaymentDialog(Loan loan, string customerName)
        {
            InitializeComponent();

            _vm = (PaymentViewModel)DataContext;

            // Initialize for this loan | اس قرض کے لیے شروع کریں
            _vm.InitializeForLoan(loan, customerName);

            // Wire events | ایونٹس جوڑیں
            _vm.PaymentSucceeded += OnPaymentSucceeded;
            _vm.Cancelled += OnCancelled;

            // Focus amount field | رقم فیلڈ پر فوکس
            Loaded += (_, _) => TxtPaidAmount.Focus();

            // Fix ComboBox selection display | کمبو باکس سلیکشن ڈسپلے درست کریں
            CmbPaymentMethod.SelectionChanged += (_, _) =>
                _vm.PaymentMethod = CmbPaymentMethod.SelectedItem as string ?? string.Empty;

            CmbNotes.SelectionChanged += (_, _) =>
                _vm.Notes = CmbNotes.SelectedItem as string ?? string.Empty;
        }

        // ─── Payment Succeeded | کامیاب ادائیگی ─────────────────────
        private void OnPaymentSucceeded(int paymentId)
        {
            SavedPaymentId = paymentId;

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(900);
                DialogResult = true;
                Close();
            });
        }

        // ─── Cancelled | منسوخی ─────────────────────────────────────
        private void OnCancelled()
        {
            DialogResult = false;
            Close();
        }
    }
}