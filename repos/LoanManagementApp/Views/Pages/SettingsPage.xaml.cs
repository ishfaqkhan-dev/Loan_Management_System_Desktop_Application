using System.Windows;
using LoanManagementApp.Services;
using System.Windows.Controls;
using LoanManagementApp.ViewModels;
using Button = System.Windows.Controls.Button;

namespace LoanManagementApp.Views.Pages
{
    /// <summary>
    /// SettingsPage.xaml.cs | ترتیبات پیج کوڈ بیہائنڈ
    /// Tab switching, password change, folder browse
    /// ٹیب تبدیلی، پاس ورڈ تبدیلی، فولڈر براؤز
    ///
    /// ═══════════════════════════════════════════════════════════
    /// ✅ DOUBLE POPUP BUG FIX | ڈبل پاپ اپ بگ درست
    /// ═══════════════════════════════════════════════════════════
    /// Masla: ViewModel_PropertyChanged + BtnChangePassword_Click
    /// dono mil ke popup dikha rahe the.
    /// ViewModel_PropertyChanged mein ShowSettingsPopup blocking thi —
    /// jab popup open hoti thi to WPF dispatcher chalta rehta tha aur
    /// ClearStatus() ke PropertyChanged se dobara handler fire hota tha.
    ///
    /// Fix: ViewModel_PropertyChanged bilkul HATA DIYA.
    /// Ab har button apna result seedha check karta hai:
    ///   1. Command execute karo
    ///   2. StatusMessage aur IsError SEEDHA padho (event se nahi)
    ///   3. ClearStatus() PEHLE karo
    ///   4. Phir popup dikhao
    /// Is tarah koi event chain nahi, koi reentrancy nahi.
    /// ═══════════════════════════════════════════════════════════
    /// </summary>
    public partial class SettingsPage : Page
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ───────────────────
        private SettingsViewModel? _viewModel;

        // ─── Auth Service Reference | تصدیق سروس ─────────────────────
        private readonly AuthService _authService;

        // ─── All Tab Panels | تمام ٹیب پینلز ────────────────────────
        private Border[] _panels = null!;
        private Button[] _tabButtons = null!;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public SettingsPage()
        {
            InitializeComponent();

            _viewModel = DataContext as SettingsViewModel;
            _authService = new AuthService();

            // Group panels and buttons for easy switching
            _panels = new[] { PanelGeneral, PanelSecurity, PanelBackup };
            _tabButtons = new[] { BtnTabGeneral, BtnTabSecurity, BtnTabBackup };

            BtnChangeUsername.Click += BtnChangeUsername_Click;
            BtnChangePassword.Click += BtnChangePassword_Click;

            // Default: show general tab
            ShowTab(0);

            Loaded += SettingsPage_Loaded;

            // ── Wire Security Question and PIN buttons ──
            if (BtnSaveSecurityQuestion != null)
                BtnSaveSecurityQuestion.Click += BtnSaveSecurityQuestion_Click;
            if (BtnSavePin != null)
                BtnSavePin.Click += BtnSavePin_Click;

            // ✅ ViewModel_PropertyChanged yahan BILKUL NAHI lagana —
            // woh double-popup ka sabab tha. Har button khud result handle karta hai.
            // ViewModel_PropertyChanged کو یہاں بالکل نہ لگائیں
        }

        // ─── Page Loaded | پیج لوڈ ─────────────────────────────────
        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.LoadSettings();
        }

        // ─── Show Popup | پاپ اپ دکھائیں ───────────────────────────
        /// <summary>
        /// NotificationDialog — logout dialog jaise style mein
        /// لاگ آؤٹ ڈائیلاگ جیسے انداز میں اطلاع ڈائیلاگ
        /// </summary>
        private void ShowPopup(string message, bool isError)
        {
            var owner = Window.GetWindow(this);
            if (isError)
                Views.Dialogs.NotificationDialog.ShowError(owner!, message);
            else
                Views.Dialogs.NotificationDialog.ShowSuccess(owner!, message);
        }

        // ─── Tab Button Click | ٹیب بٹن کلک ─────────────────────────
        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tagStr &&
                int.TryParse(tagStr, out int tabIndex))
            {
                ShowTab(tabIndex);
                _viewModel!.ActiveTab = tabIndex;
            }
        }

        // ─── Show Tab Panel | ٹیب پینل دکھائیں ──────────────────────
        private void ShowTab(int index)
        {
            for (int i = 0; i < _panels.Length; i++)
            {
                _panels[i].Visibility = i == index
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                _tabButtons[i].Tag = i.ToString();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // CHANGE PASSWORD | پاس ورڈ تبدیل کریں
        // ═══════════════════════════════════════════════════════════

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            string currentPwd = PbCurrentPassword.Password;
            string newPwd = PbNewPassword.Password;
            string confirmPwd = PbConfirmPassword.Password;

            // ── Validation — command se pehle | کمانڈ سے پہلے جانچ ──
            if (string.IsNullOrEmpty(currentPwd))
            {
                ShowPopup(
                    "موجودہ پاس ورڈ درج کریں | Please enter your current password",
                    isError: true);
                PbCurrentPassword.Focus();
                return;
            }
            if (string.IsNullOrEmpty(newPwd))
            {
                ShowPopup(
                    "نیا پاس ورڈ درج کریں | Please enter a new password",
                    isError: true);
                PbNewPassword.Focus();
                return;
            }
            if (string.IsNullOrEmpty(confirmPwd))
            {
                ShowPopup(
                    "پاس ورڈ دوبارہ درج کریں | Please confirm your new password",
                    isError: true);
                PbConfirmPassword.Focus();
                return;
            }

            // ── ViewModel mein values set karo | ویو ماڈل میں قدریں سیٹ کریں ──
            _viewModel.CurrentPassword = currentPwd;
            _viewModel.NewPassword = newPwd;
            _viewModel.ConfirmNewPassword = confirmPwd;

            // ── Command execute karo | کمانڈ چلائیں ──
            if (_viewModel.ChangePasswordCommand.CanExecute(null))
                _viewModel.ChangePasswordCommand.Execute(null);

            // ✅ SEEDHA result padho — event se nahi, koi reentrancy nahi
            // براہ راست نتیجہ پڑھیں — ایونٹ سے نہیں، کوئی ری اینٹرینسی نہیں
            string msg = _viewModel.StatusMessage;
            bool isErr = _viewModel.IsError;

            // ✅ ClearStatus PEHLE — phir popup
            // پہلے اسٹیٹس صاف کریں — پھر پاپ اپ
            _viewModel.ClearStatus();

            // ✅ Sirf success par boxes saaf karo
            // صرف کامیابی پر خانے صاف کریں
            if (!isErr)
            {
                PbCurrentPassword.Clear();
                PbNewPassword.Clear();
                PbConfirmPassword.Clear();
            }

            // ✅ Ab popup dikhao — ClearStatus ke BAAD
            // اب پاپ اپ دکھائیں — اسٹیٹس صاف کرنے کے بعد
            if (!string.IsNullOrEmpty(msg))
                ShowPopup(msg, isErr);
        }

        // ═══════════════════════════════════════════════════════════
        // CHANGE USERNAME | صارف نام تبدیل کریں
        // ═══════════════════════════════════════════════════════════

        private void BtnChangeUsername_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = PbCurrentForUsername.Password;
            string newUsername = TxtNewUsername.Text.Trim();

            if (string.IsNullOrEmpty(currentPassword))
            {
                ShowPopup(
                    "موجودہ پاس ورڈ ضروری ہے | Current password is required",
                    isError: true);
                return;
            }
            if (string.IsNullOrEmpty(newUsername))
            {
                ShowPopup(
                    "نیا صارف نام ضروری ہے | New username is required",
                    isError: true);
                return;
            }
            if (newUsername.Length < 3)
            {
                ShowPopup(
                    "صارف نام کم از کم 3 حروف | Username must be at least 3 characters",
                    isError: true);
                return;
            }

            var currentUser = AuthService.CurrentUser;
            if (currentUser == null)
            {
                ShowPopup("صارف نہیں ملا | User not found", isError: true);
                return;
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(currentPassword, currentUser.PasswordHash);
            if (!passwordOk)
            {
                ShowPopup(
                    "موجودہ پاس ورڈ غلط ہے | Current password is incorrect",
                    isError: true);
                return;
            }

            var result = _authService.ChangeUsername(currentUser.Id, newUsername);
            if (result.Success)
            {
                ShowPopup(result.Message, isError: false);
                PbCurrentForUsername.Clear();
                TxtNewUsername.Clear();
            }
            else
                ShowPopup(result.Message, isError: true);
        }

        // ─── Browse Backup Folder | بیک اپ فولڈر براؤز ──────────────
        private void BtnBrowseBackup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog()
            {
                Title = "Select Backup Folder | بیک اپ فولڈر منتخب کریں",
                Multiselect = false
            };

            if (!string.IsNullOrWhiteSpace(_viewModel?.BackupFolderPath))
                dialog.InitialDirectory = _viewModel.BackupFolderPath;

            if (dialog.ShowDialog() == true && _viewModel != null)
                _viewModel.BackupFolderPath = dialog.FolderName;
        }

        // ═══════════════════════════════════════════════════════════
        // SECURITY QUESTION | حفاظتی سوال
        // ═══════════════════════════════════════════════════════════

        private void BtnSaveSecurityQuestion_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = PbCurrentForSecurity.Password;
            string newQuestion = TxtNewSecurityQuestion.Text.Trim();
            string newAnswer = TxtNewSecurityAnswer.Text.Trim();

            if (string.IsNullOrEmpty(currentPassword))
            {
                ShowPopup(
                    "موجودہ پاس ورڈ ضروری ہے | Current password is required",
                    isError: true);
                return;
            }
            if (string.IsNullOrEmpty(newQuestion))
            {
                ShowPopup(
                    "حفاظتی سوال ضروری ہے | Security question is required",
                    isError: true);
                return;
            }
            if (string.IsNullOrEmpty(newAnswer))
            {
                ShowPopup(
                    "جواب ضروری ہے | Security answer is required",
                    isError: true);
                return;
            }

            var currentUser = AuthService.CurrentUser;
            if (currentUser == null)
            {
                ShowPopup("صارف نہیں ملا | User not found", isError: true);
                return;
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(currentPassword, currentUser.PasswordHash);
            if (!passwordOk)
            {
                ShowPopup(
                    "موجودہ پاس ورڈ غلط ہے | Current password is incorrect",
                    isError: true);
                return;
            }

            var result = _authService.SetSecurityQuestion(currentUser.Id, newQuestion, newAnswer);
            if (result.Success)
            {
                ShowPopup(result.Message, isError: false);
                PbCurrentForSecurity.Clear();
                TxtNewSecurityQuestion.Clear();
                TxtNewSecurityAnswer.Clear();
            }
            else
                ShowPopup(result.Message, isError: true);
        }

        // ═══════════════════════════════════════════════════════════
        // PIN | پن کوڈ
        // ═══════════════════════════════════════════════════════════

        private void BtnSavePin_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = PbCurrentForPin.Password;
            string newPin = PwdNewPin.Password;
            string confirmPin = PwdConfirmPin.Password;

            if (string.IsNullOrEmpty(currentPassword))
            {
                ShowPopup(
                    "موجودہ پاس ورڈ ضروری ہے | Current password is required",
                    isError: true);
                return;
            }
            if (string.IsNullOrEmpty(newPin))
            {
                ShowPopup("پن ضروری ہے | PIN is required", isError: true);
                return;
            }

            var currentUser = AuthService.CurrentUser;
            if (currentUser == null)
            {
                ShowPopup("صارف نہیں ملا | User not found", isError: true);
                return;
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(currentPassword, currentUser.PasswordHash);
            if (!passwordOk)
            {
                ShowPopup(
                    "موجودہ پاس ورڈ غلط ہے | Current password is incorrect",
                    isError: true);
                return;
            }

            var result = _authService.SetPin(currentUser.Id, newPin, confirmPin);
            if (result.Success)
            {
                ShowPopup(result.Message, isError: false);
                PbCurrentForPin.Clear();
                PwdNewPin.Clear();
                PwdConfirmPin.Clear();
            }
            else
                ShowPopup(result.Message, isError: true);
        }
    }
}