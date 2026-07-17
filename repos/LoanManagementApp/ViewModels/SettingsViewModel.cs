using System;
using System.Windows.Input;
using LoanManagementApp.Models;
using LoanManagementApp.Services;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// SettingsViewModel / ترتیبات ویو ماڈل
    /// Application settings, security, email, backup, theme
    /// ایپلیکیشن ترتیبات، حفاظت، ای میل، بیک اپ، تھیم
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly AppSettingsService _settingsService;
        private readonly AuthService _authService;
        private readonly BackupService _backupService;
        private readonly EmailService _emailService;
        private readonly ValidationService _validationService;

        // ─── Active Settings Tab | فعال ترتیبات ٹیب ─────────────────
        private int _activeTab = 0;

        /// <summary>
        /// Active settings tab index | فعال ترتیبات ٹیب انڈیکس
        /// 0=General, 1=Security, 2=Email, 3=Backup
        /// </summary>
        public int ActiveTab
        {
            get => _activeTab;
            set => SetProperty(ref _activeTab, value);
        }

        // ════════════════════════════════════════════════════════════
        // SECTION 1 — General Settings | عمومی ترتیبات
        // ════════════════════════════════════════════════════════════

        private string _companyName = string.Empty;

        /// <summary>Company name | کمپنی کا نام</summary>
        public string CompanyName
        {
            get => _companyName;
            set => SetProperty(ref _companyName, value);
        }

        private string _companyNameUrdu = string.Empty;

        /// <summary>Company name in Urdu | کمپنی کا نام اردو میں</summary>
        public string CompanyNameUrdu
        {
            get => _companyNameUrdu;
            set => SetProperty(ref _companyNameUrdu, value);
        }

        private string _companyPhone = string.Empty;

        /// <summary>Company phone | کمپنی کا فون</summary>
        public string CompanyPhone
        {
            get => _companyPhone;
            set => SetProperty(ref _companyPhone, value);
        }

        private decimal _exchangeRate = 77;

        /// <summary>PKR exchange rate (not used) | PKR تبادلہ شرح (استعمال نہیں)</summary>
        public decimal ExchangeRate
        {
            get => _exchangeRate;
            set => SetProperty(ref _exchangeRate, value);
        }

        private AppTheme _currentTheme = AppTheme.Light;

        /// <summary>Current app theme | موجودہ ایپ تھیم</summary>
        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set => SetProperty(ref _currentTheme, value);
        }

        /// <summary>Is dark mode active | کیا گہرا موڈ فعال ہے</summary>
        public bool IsDarkMode
        {
            get => CurrentTheme == AppTheme.Dark;
            set
            {
                CurrentTheme = value ? AppTheme.Dark : AppTheme.Light;
                OnPropertyChanged(nameof(IsDarkMode));
                OnPropertyChanged(nameof(ThemeModeDisplay));
            }
        }

        /// <summary>Theme mode display | تھیم موڈ ڈسپلے</summary>
        public string ThemeModeDisplay => IsDarkMode
            ? "🌙 Dark Mode | گہرا موڈ"
            : "☀️ Light Mode | روشن موڈ";

        // ─── Event for theme change (MainWindow listens) | تھیم تبدیلی ایونٹ
        /// <summary>Raised when theme is changed | تھیم تبدیل ہونے پر</summary>
        public event Action<AppTheme>? ThemeChanged;

        // ════════════════════════════════════════════════════════════
        // SECTION 2 — Security Settings | حفاظتی ترتیبات
        // ════════════════════════════════════════════════════════════

        private string _currentPassword = string.Empty;

        /// <summary>Current password for verification | تصدیق کے لیے موجودہ پاس ورڈ</summary>
        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetProperty(ref _currentPassword, value);
        }

        private string _newPassword = string.Empty;

        /// <summary>New password | نیا پاس ورڈ</summary>
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _confirmNewPassword = string.Empty;

        /// <summary>Confirm new password | نئے پاس ورڈ کی تصدیق</summary>
        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set => SetProperty(ref _confirmNewPassword, value);
        }

        private string _newUsername = string.Empty;

        /// <summary>New username | نیا صارف نام</summary>
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _pinCode = string.Empty;

        /// <summary>PIN code | پن کوڈ</summary>
        public string PinCode
        {
            get => _pinCode;
            set => SetProperty(ref _pinCode, value);
        }

        private string _confirmPin = string.Empty;

        /// <summary>Confirm PIN | پن کوڈ تصدیق</summary>
        public string ConfirmPin
        {
            get => _confirmPin;
            set => SetProperty(ref _confirmPin, value);
        }

        private string _securityQuestion = string.Empty;

        /// <summary>Security question | حفاظتی سوال</summary>
        public string SecurityQuestion
        {
            get => _securityQuestion;
            set => SetProperty(ref _securityQuestion, value);
        }

        private string _securityAnswer = string.Empty;

        /// <summary>Security answer | حفاظتی جواب</summary>
        public string SecurityAnswer
        {
            get => _securityAnswer;
            set => SetProperty(ref _securityAnswer, value);
        }

        // ════════════════════════════════════════════════════════════
        // SECTION 3 — Email / SMTP Settings | ای میل ترتیبات
        // ════════════════════════════════════════════════════════════

        private string _smtpHost = "smtp.gmail.com";
        /// <summary>SMTP host | ایس ایم ٹی پی ہوسٹ</summary>
        public string SmtpHost
        {
            get => _smtpHost;
            set => SetProperty(ref _smtpHost, value);
        }

        private int _smtpPort = 587;
        /// <summary>SMTP port | ایس ایم ٹی پی پورٹ</summary>
        public int SmtpPort
        {
            get => _smtpPort;
            set => SetProperty(ref _smtpPort, value);
        }

        private string _smtpUsername = string.Empty;
        /// <summary>SMTP username | ایس ایم ٹی پی صارف نام</summary>
        public string SmtpUsername
        {
            get => _smtpUsername;
            set => SetProperty(ref _smtpUsername, value);
        }

        private string _smtpPassword = string.Empty;
        /// <summary>SMTP password | ایس ایم ٹی پی پاس ورڈ</summary>
        public string SmtpPassword
        {
            get => _smtpPassword;
            set => SetProperty(ref _smtpPassword, value);
        }

        private string _senderEmail = string.Empty;
        /// <summary>Sender email | بھیجنے والے کا ای میل</summary>
        public string SenderEmail
        {
            get => _senderEmail;
            set => SetProperty(ref _senderEmail, value);
        }

        private string _senderName = "Loan Management System";
        /// <summary>Sender name | بھیجنے والے کا نام</summary>
        public string SenderName
        {
            get => _senderName;
            set => SetProperty(ref _senderName, value);
        }

        private bool _enableSsl = true;
        /// <summary>Enable SSL | SSL فعال کریں</summary>
        public bool EnableSsl
        {
            get => _enableSsl;
            set => SetProperty(ref _enableSsl, value);
        }

        // ════════════════════════════════════════════════════════════
        // SECTION 4 — Backup Settings | بیک اپ ترتیبات
        // ════════════════════════════════════════════════════════════

        private int _maxBackupFilesToKeep = 10;
        public int MaxBackupFilesToKeep
        {
            get => _maxBackupFilesToKeep;
            set => SetProperty(ref _maxBackupFilesToKeep, value);
        }

        private string _backupFolderPath = string.Empty;
        /// <summary>Backup folder path | بیک اپ فولڈر کا راستہ</summary>
        public string BackupFolderPath
        {
            get => _backupFolderPath;
            set => SetProperty(ref _backupFolderPath, value);
        }

        private bool _autoBackupEnabled = true;
        /// <summary>Auto backup enabled | خودکار بیک اپ فعال</summary>
        public bool AutoBackupEnabled
        {
            get => _autoBackupEnabled;
            set => SetProperty(ref _autoBackupEnabled, value);
        }

        private int _autoBackupIntervalHours = 24;
        /// <summary>Auto backup interval hours | خودکار بیک اپ وقفہ گھنٹے</summary>
        public int AutoBackupIntervalHours
        {
            get => _autoBackupIntervalHours;
            set => SetProperty(ref _autoBackupIntervalHours, value);
        }

        private string _lastBackupDisplay = string.Empty;
        /// <summary>Last backup display | آخری بیک اپ ڈسپلے</summary>
        public string LastBackupDisplay
        {
            get => _lastBackupDisplay;
            set => SetProperty(ref _lastBackupDisplay, value);
        }

        // ─── Commands | کمانڈز ──────────────────────────────────────

        /// <summary>Save general settings | عمومی ترتیبات محفوظ</summary>
        public ICommand SaveGeneralCommand { get; }

        public ICommand SaveBackupSettingsCommand { get; }

        /// <summary>Change password command | پاس ورڈ تبدیل کمانڈ</summary>
        public ICommand ChangePasswordCommand { get; }

        /// <summary>Change username command | صارف نام تبدیل کمانڈ</summary>
        public ICommand ChangeUsernameCommand { get; }

        /// <summary>Set PIN command | پن سیٹ کمانڈ</summary>
        public ICommand SetPinCommand { get; }

        /// <summary>Set security question command | حفاظتی سوال سیٹ کمانڈ</summary>
        public ICommand SetSecurityQuestionCommand { get; }

        /// <summary>Save email settings command | ای میل ترتیبات محفوظ کمانڈ</summary>
        public ICommand SaveEmailCommand { get; }

        /// <summary>Test email command | ای میل ٹیسٹ کمانڈ</summary>
        public ICommand TestEmailCommand { get; }

        /// <summary>Backup now command | ابھی بیک اپ کمانڈ</summary>
        public ICommand BackupNowCommand { get; }

        /// <summary>Toggle theme command | تھیم ٹوگل کمانڈ</summary>
        public ICommand ToggleThemeCommand { get; }

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public SettingsViewModel()
        {
            _settingsService = new AppSettingsService();
            _authService = new AuthService();
            _backupService = new BackupService();
            _emailService = new EmailService();
            _validationService = new ValidationService();

            SaveGeneralCommand = new RelayCommand(ExecuteSaveGeneral);
            SaveBackupSettingsCommand = new RelayCommand(ExecuteSaveBackupSettings);
            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);
            ChangeUsernameCommand = new RelayCommand(ExecuteChangeUsername);
            SetPinCommand = new RelayCommand(ExecuteSetPin);
            SetSecurityQuestionCommand = new RelayCommand(ExecuteSetSecurityQuestion);
            SaveEmailCommand = new RelayCommand(ExecuteSaveEmail);
            TestEmailCommand = new RelayCommand(ExecuteTestEmail);
            BackupNowCommand = new RelayCommand(ExecuteBackupNow);
            ToggleThemeCommand = new RelayCommand(ExecuteToggleTheme);

            LoadSettings();
        }

        // ─── Load Settings | ترتیبات لوڈ کریں ───────────────────────
        /// <summary>
        /// Load current settings from database | ڈیٹابیس سے موجودہ ترتیبات لوڈ کریں
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                var s = _settingsService.GetSettings();

                CompanyName = s.CompanyName;
                CompanyNameUrdu = s.CompanyNameUrdu;
                CompanyPhone = s.CompanyPhone;
                ExchangeRate = s.ExchangeRate;
                CurrentTheme = s.CurrentTheme;
                SmtpHost = s.SmtpHost;
                SmtpPort = s.SmtpPort;
                SmtpUsername = s.SmtpUsername;
                SmtpPassword = s.SmtpPassword;
                SenderEmail = s.SenderEmail;
                SenderName = s.SenderName;
                EnableSsl = s.EnableSsl;
                BackupFolderPath = s.BackupFolderPath;
                AutoBackupEnabled = s.AutoBackupEnabled;
                AutoBackupIntervalHours = s.AutoBackupIntervalHours;
                LastBackupDisplay = s.LastBackupDisplay;
                MaxBackupFilesToKeep = s.MaxBackupFilesToKeep;

                OnPropertyChanged(nameof(IsDarkMode));
                OnPropertyChanged(nameof(ThemeModeDisplay));
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load settings | ترتیبات لوڈ ناکام: {ex.Message}");
            }
        }

        private void ExecuteSaveBackupSettings(object? _)
        {
            try
            {
                IsBusy = true;
                var settings = _settingsService.GetSettings();
                settings.BackupFolderPath = BackupFolderPath;
                settings.AutoBackupEnabled = AutoBackupEnabled;
                settings.MaxBackupFilesToKeep = MaxBackupFilesToKeep;
                _settingsService.UpdateSettings(settings);
                ShowSuccess("Backup settings saved | بیک اپ ترتیبات محفوظ ہوئیں");
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

        // ─── Execute Save General | عمومی ترتیبات محفوظ ─────────────
        private void ExecuteSaveGeneral(object? _)
        {
            try
            {
                IsBusy = true;
                var s = _settingsService.GetSettings();
                s.CompanyName = CompanyName.Trim();
                s.CompanyNameUrdu = CompanyNameUrdu.Trim();
                s.CompanyPhone = CompanyPhone.Trim();
                s.ExchangeRate = ExchangeRate;
                s.CurrentTheme = CurrentTheme;

                _settingsService.UpdateSettings(s);
                ShowSuccess("Settings saved | ترتیبات محفوظ ہوئیں");
                ThemeChanged?.Invoke(CurrentTheme);
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

        // ─── Execute Change Password | پاس ورڈ تبدیل ────────────────
        private void ExecuteChangePassword(object? _)
        {
            try
            {
                IsBusy = true;

                // STEP 1: Check current password first
                int userId = AuthService.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    ShowError("User not logged in | صارف لاگ ان نہیں ہے");
                    return;
                }

                // Verify current password
                var currentUser = AuthService.CurrentUser;
                bool currentPasswordOk = BCrypt.Net.BCrypt.Verify(CurrentPassword, currentUser?.PasswordHash ?? "");
                if (!currentPasswordOk)
                {
                    ShowError("Current password is incorrect | موجودہ پاس ورڈ غلط ہے");
                    return;
                }

                // STEP 2: Now validate new password
                var validation = _validationService.ValidatePassword(
                    NewPassword, ConfirmNewPassword);
                if (!validation.IsValid)
                {
                    ShowError(validation.Message);
                    return;
                }

                var result = _authService.ChangePassword(
                    userId, CurrentPassword, NewPassword, ConfirmNewPassword);

                if (result.Success)
                {
                    ShowSuccess(result.Message);
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmNewPassword = string.Empty;
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed | ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Execute Change Username | صارف نام تبدیل ────────────────
        private void ExecuteChangeUsername(object? _)
        {
            try
            {
                IsBusy = true;

                // STEP 1: UserId check | صارف شناخت جانچیں
                int userId = AuthService.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    ShowError("User not logged in | صارف لاگ ان نہیں ہے");
                    return;
                }

                // STEP 2: Verify current password | موجودہ پاس ورڈ تصدیق کریں
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    ShowError("Current password is required | موجودہ پاس ورڈ ضروری ہے");
                    return;
                }

                var currentUser = AuthService.CurrentUser;
                bool passwordOk = BCrypt.Net.BCrypt.Verify(
                    CurrentPassword, currentUser?.PasswordHash ?? "");
                if (!passwordOk)
                {
                    ShowError("Current password is incorrect | موجودہ پاس ورڈ غلط ہے");
                    return;
                }

                // STEP 3: Validate new username | نیا صارف نام جانچیں
                var usernameVal = _validationService.ValidateUsername(NewUsername);
                if (!usernameVal.IsValid)
                {
                    ShowError(usernameVal.Message);
                    return;
                }

                // STEP 4: Change username | صارف نام تبدیل کریں
                var result = _authService.ChangeUsername(userId, NewUsername.Trim());

                if (result.Success)
                {
                    ShowSuccess(result.Message);
                    NewUsername = string.Empty;
                    CurrentPassword = string.Empty;
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed | ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Execute Set PIN | پن سیٹ ────────────────────────────────
        private void ExecuteSetPin(object? _)
        {
            try
            {
                IsBusy = true;

                var pinVal = _validationService.ValidatePin(PinCode, ConfirmPin);
                if (!pinVal.IsValid)
                {
                    ShowError(pinVal.Message);
                    return;
                }

                int userId = AuthService.CurrentUser?.Id ?? 0;
                var result = _authService.SetPin(userId, PinCode, ConfirmPin);

                if (result.Success)
                {
                    ShowSuccess(result.Message);
                    PinCode = string.Empty;
                    ConfirmPin = string.Empty;
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed | ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Execute Set Security Question | حفاظتی سوال سیٹ ─────────
        private void ExecuteSetSecurityQuestion(object? _)
        {
            try
            {
                IsBusy = true;

                int userId = AuthService.CurrentUser?.Id ?? 0;
                var result = _authService.SetSecurityQuestion(
                    userId, SecurityQuestion.Trim(), SecurityAnswer.Trim());

                if (result.Success)
                {
                    ShowSuccess(result.Message);
                    SecurityAnswer = string.Empty;
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed | ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Execute Save Email | ای میل ترتیبات محفوظ ───────────────
        private void ExecuteSaveEmail(object? _)
        {
            try
            {
                IsBusy = true;

                var emailVal = _validationService.ValidateSmtpSettings(
                    SmtpHost, SmtpPort, SmtpUsername, SmtpPassword, SenderEmail);
                if (!emailVal.IsValid)
                {
                    ShowError(emailVal.Message);
                    return;
                }

                _settingsService.UpdateEmailConfig(
                    SmtpHost, SmtpPort, SmtpUsername, SmtpPassword,
                    SenderEmail, SenderName, EnableSsl);

                ShowSuccess("Email settings saved | ای میل ترتیبات محفوظ ہوئیں");
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

        // ─── Execute Test Email | ای میل ٹیسٹ ───────────────────────
        private void ExecuteTestEmail(object? _)
        {
            try
            {
                IsBusy = true;
                ShowSuccess("Sending test email... | ٹیسٹ ای میل بھیجا جا رہا ہے...");

                // Validate email first | پہلے ای میل جانچیں
                var emailVal = _validationService.ValidateEmail(SenderEmail);
                if (!emailVal.IsValid)
                {
                    ShowError("Please enter a valid sender email first | " +
                              "پہلے درست ای میل درج کریں");
                    return;
                }

                // Build settings from current form values
                // موجودہ فارم قدروں سے ترتیبات بنائیں
                var settings = _settingsService.GetSettings();
                settings.SmtpHost = SmtpHost;
                settings.SmtpPort = SmtpPort;
                settings.SmtpUsername = SmtpUsername;
                settings.SmtpPassword = SmtpPassword;
                settings.SenderEmail = SenderEmail;
                settings.SenderName = SenderName;
                settings.EnableSsl = EnableSsl;

                // Call with correct signature: (AppSettings settings, string toEmail)
                // درست signature کے ساتھ کال کریں
                var result = _emailService.SendTestEmail(settings, SenderEmail);

                if (result.Success)
                    ShowSuccess(result.Message);
                else
                    ShowError(result.Message);
            }
            catch (Exception ex)
            {
                ShowError($"Email test failed | ای میل ٹیسٹ ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Execute Backup Now | ابھی بیک اپ ───────────────────────
        private void ExecuteBackupNow(object? _)
        {
            try
            {
                IsBusy = true;
                var result = _backupService.CreateBackup();

                if (result.Success)
                {
                    ShowSuccess(result.Message);
                    LoadSettings(); // Refresh last backup display | آخری بیک اپ ریفریش
                }
                else
                {
                    // result.Message already contains full error — show once only
                    // result.Message میں پہلے سے پوری خرابی ہے — صرف ایک بار دکھائیں
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                // Only catch truly unexpected exceptions here (not BackupService errors)
                // صرف غیر متوقع استثنیٰ یہاں پکڑیں
                ShowError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Execute Toggle Theme | تھیم ٹوگل ───────────────────────
        private void ExecuteToggleTheme(object? _)
        {
            IsDarkMode = !IsDarkMode;
            ExecuteSaveGeneral(null);
        }
    }
}