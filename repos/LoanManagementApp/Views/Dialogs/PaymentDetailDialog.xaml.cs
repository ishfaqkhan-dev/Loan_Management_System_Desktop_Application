using System.Windows;
using LoanManagementApp.Models;

namespace LoanManagementApp.Views.Dialogs
{
    public partial class PaymentDetailDialog : Window
    {
        public PaymentDetailDialog(Payment payment)
        {
            InitializeComponent();
            DataContext = this;
            LoadPaymentData(payment);
        }

        public int InstallmentNumber { get; set; }
        public string PaymentDateDisplay { get; set; } = string.Empty;
        public string PaidAmountDisplay { get; set; } = string.Empty;
        public string BalanceBeforeDisplay { get; set; } = string.Empty;
        public string BalanceAfterDisplay { get; set; } = string.Empty;
        public string PaymentTypeDisplay { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string ReceivedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string VerifiedStatus { get; set; } = string.Empty;

        private void LoadPaymentData(Payment payment)
        {
            InstallmentNumber = payment.InstallmentNumber;
            PaymentDateDisplay = payment.PaymentDate.ToString("dd-MMM-yyyy hh:mm tt");
            PaidAmountDisplay = $"{payment.PaidAmount:N0} PKR";
            BalanceBeforeDisplay = $"{payment.BalanceBeforePayment:N0} PKR";
            BalanceAfterDisplay = $"{payment.RemainingBalanceAfterPayment:N0} PKR";
            PaymentTypeDisplay = payment.PaymentTypeUrdu;
            PaymentMethod = string.IsNullOrWhiteSpace(payment.PaymentMethod) ? "—" : payment.PaymentMethod;
            ReceivedBy = string.IsNullOrWhiteSpace(payment.ReceivedBy) ? "—" : payment.ReceivedBy;
            Notes = string.IsNullOrWhiteSpace(payment.Notes) ? "—" : payment.Notes;
            VerifiedStatus = payment.IsVerified ? "✅ Verified | تصدیق شدہ" : "❌ Not Verified | غیر تصدیق شدہ";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}