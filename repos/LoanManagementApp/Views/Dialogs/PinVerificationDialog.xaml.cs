using System.Windows;
using LoanManagementApp.Services;

namespace LoanManagementApp.Views.Dialogs
{
    /// <summary>
    /// PinVerificationDialog.xaml.cs | پن تصدیق ڈائیلاگ
    /// Prompts user for PIN before sensitive actions.
    /// حساس کارروائیوں سے پہلے صارف سے پن مانگتا ہے۔
    /// </summary>
    public partial class PinVerificationDialog : Window
    {
        // ─── Result | نتیجہ ─────────────────────────────────────────
        /// <summary>
        /// True if PIN verified successfully | پن درست ہونے پر True
        /// </summary>
        public bool IsVerified { get; private set; } = false;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public PinVerificationDialog()
        {
            InitializeComponent();
        }

        // ─── Verify Button Click | تصدیق بٹن ─────────────────────────
        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredPin = PinPasswordBox.Password;
            var currentUser = AuthService.CurrentUser;

            // Check if user is logged in
            if (currentUser == null)
            {
                ErrorText.Text = "User not logged in. Please login again. | صارف لاگ ان نہیں ہے۔ براہ کرم دوبارہ لاگ ان کریں۔";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            // Check if PIN is set
            if (string.IsNullOrEmpty(currentUser.PinCode))
            {
                ErrorText.Text = "PIN is not set. Please set a PIN in Settings first. | پن سیٹ نہیں ہے۔ پہلے ترتیبات میں پن سیٹ کریں۔";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            // Verify PIN
            if (currentUser.PinCode == enteredPin)
            {
                IsVerified = true;
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Text = "Incorrect PIN. Please try again. | غلط پن۔ دوبارہ کوشش کریں۔";
                ErrorText.Visibility = Visibility.Visible;
                PinPasswordBox.Clear();
            }
        }

        // ─── Cancel Button Click | منسوخ بٹن ─────────────────────────
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}