using System;
using System.Windows;
using System.Windows.Media;
using LoanManagementApp.Data;
using LoanManagementApp.Models;
using LoanManagementApp.Services;

namespace LoanManagementApp.Views.Dialogs
{
    public partial class ForgotPasswordDialog : Window
    {
        private readonly UserRepository _userRepo;
        private readonly AuthService _authService;
        private User? _currentUser;

        public ForgotPasswordDialog()
        {
            InitializeComponent();
            _userRepo = new UserRepository();
            _authService = new AuthService();

            // Wire events
            BtnVerifyUser.Click += BtnVerifyUser_Click;
            BtnProceed.Click += BtnProceed_Click;
            BtnResetPassword.Click += BtnResetPassword_Click;
            BtnCancel.Click += (s, e) => DialogResult = false;

            // Show/hide panels based on radio selection
            RbSecurityQuestion.Checked += (s, e) => ToggleRecoveryPanels();
            RbPin.Checked += (s, e) => ToggleRecoveryPanels();
        }

        private void ToggleRecoveryPanels()
        {
            SecurityQuestionPanel.Visibility = RbSecurityQuestion.IsChecked == true
                ? Visibility.Visible : Visibility.Collapsed;
            PinPanel.Visibility = RbPin.IsChecked == true
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnVerifyUser_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowStatus(false, "Please enter username | براہ کرم صارف نام درج کریں");
                return;
            }

            _currentUser = _userRepo.GetUserByUsername(username);
            if (_currentUser == null)
            {
                ShowStatus(false, "User not found | صارف نہیں ملا");
                return;
            }

            ShowStatus(true, $"User verified: {username} | صارف تصدیق شدہ");

            // Show step 2, hide step 1
            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Visible;

            // Populate security question if exists (using null-conditional operator to avoid warnings)
            TxtSecurityQuestion.Text = _currentUser.SecurityQuestion ?? string.Empty;
            if (string.IsNullOrEmpty(_currentUser.SecurityQuestion))
                RbSecurityQuestion.IsEnabled = false;

            if (string.IsNullOrEmpty(_currentUser.PinCode))
                RbPin.IsEnabled = false;
        }

        private void BtnProceed_Click(object sender, RoutedEventArgs e)
        {
            // Ensure user is not null
            if (_currentUser == null)
            {
                ShowStatus(false, "User not found | صارف نہیں ملا");
                return;
            }

            if (RbSecurityQuestion.IsChecked == true)
            {
                string answer = TxtSecurityAnswer.Text.Trim();
                if (string.IsNullOrWhiteSpace(answer))
                {
                    ShowStatus(false, "Please enter security answer | حفاظتی جواب درج کریں");
                    return;
                }

                bool verified = _authService.VerifySecurityAnswer(_currentUser.Id, answer);
                if (!verified)
                {
                    ShowStatus(false, "Incorrect answer | جواب غلط ہے");
                    return;
                }
            }
            else if (RbPin.IsChecked == true)
            {
                string pin = PwdPin.Password.Trim();
                if (string.IsNullOrWhiteSpace(pin))
                {
                    ShowStatus(false, "Please enter PIN | پن درج کریں");
                    return;
                }

                bool verified = _authService.VerifyPin(_currentUser.Id, pin);
                if (!verified)
                {
                    ShowStatus(false, "Incorrect PIN | پن غلط ہے");
                    return;
                }
            }
            else
            {
                ShowStatus(false, "Please select a recovery method | کوئی طریقہ منتخب کریں");
                return;
            }

            // Verification passed - show step 3
            Step2Panel.Visibility = Visibility.Collapsed;
            Step3Panel.Visibility = Visibility.Visible;
            ShowStatus(true, "Verification successful | تصدیق کامیاب");
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            // Ensure user is not null
            if (_currentUser == null)
            {
                ShowStatus(false, "User not found | صارف نہیں ملا");
                return;
            }

            string newPassword = PwdNewPassword.Password;
            string confirm = PwdConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ShowStatus(false, "New password required | نیا پاس ورڈ ضروری ہے");
                return;
            }
            if (newPassword.Length < 4)
            {
                ShowStatus(false, "Password must be at least 4 characters | پاس ورڈ کم از کم 4 حروف");
                return;
            }
            if (newPassword != confirm)
            {
                ShowStatus(false, "Passwords do not match | پاس ورڈ میل نہیں کھاتے");
                return;
            }

            string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            bool updated = _userRepo.UpdatePassword(_currentUser.Id, newHash);

            if (updated)
            {
                MessageBox.Show("Password reset successfully!\nپاس ورڈ کامیابی سے ری سیٹ ہو گیا!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                ShowStatus(false, "Failed to reset password | پاس ورڈ ری سیٹ ناکام");
            }
        }

        private void ShowStatus(bool success, string message)
        {
            StatusBorder.Visibility = Visibility.Visible;
            TxtStatus.Text = message;

            if (success)
            {
                StatusBorder.Background = new SolidColorBrush(Color.FromRgb(27, 58, 30));
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(154, 230, 180));
            }
            else
            {
                StatusBorder.Background = new SolidColorBrush(Color.FromRgb(59, 18, 18));
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(252, 129, 129));
            }
        }
    }
}