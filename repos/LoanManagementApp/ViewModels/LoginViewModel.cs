using System;
using System.Windows.Input;
using System.Windows.Threading;
using LoanManagementApp.Services;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// LoginViewModel / لاگ ان ویو ماڈل - Handles login screen logic
    ///
    /// ✅ FIX 1: _isExecutingLogin flag — Password setter mein ClearStatus()
    ///           finally block mein Password="" karne par error clear na kare
    ///
    /// ✅ FIX 2: Account Lockout Timer — jab account lock ho toh Login button
    ///           30 minute tak disable rahe. Countdown message dikhao.
    ///           Sirf AuthService ka result dekho — timer se enforce karo.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly AuthService _authService;

        // ─── Internal flag to skip ClearStatus during login execution
        // لاگ ان کے دوران status clear نہ ہو اس لیے flag
        private bool _isExecutingLogin = false;

        // ─── Lockout Timer | لاک آؤٹ ٹائمر ─────────────────────────
        // Har second countdown update karne ke liye | ہر سیکنڈ اپڈیٹ
        private DispatcherTimer? _lockoutTimer;

        // Jab tak lock hai tab tak Login button disable rahe
        // جب تک لاک ہے لاگ ان بٹن غیر فعال رہے
        private DateTime _lockoutUntil = DateTime.MinValue;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);

            // ✅ App start par database se lockout check karo
            // ایپ شروع ہوتے ہی ڈیٹا بیس سے لاک آؤٹ جانچیں
            CheckDatabaseLockoutOnStartup();
        }

        /// <summary>
        /// App open hone par database se LockedUntil check karo
        /// agar abhi bhi locked hai to remaining time ka timer shuru karo
        /// ایپ کھلنے پر ڈیٹا بیس سے LockedUntil جانچیں
        /// </summary>
        private void CheckDatabaseLockoutOnStartup()
        {
            try
            {
                var userRepo = new LoanManagementApp.Data.UserRepository();
                var adminUser = userRepo.GetAdminUser();

                if (adminUser != null &&
                    adminUser.IsCurrentlyLocked &&
                    adminUser.LockedUntil.HasValue)
                {
                    var remaining = adminUser.LockedUntil.Value - DateTime.Now;
                    if (remaining > TimeSpan.Zero)
                    {
                        // Remaining time se timer shuru karo
                        // باقی وقت سے ٹائمر شروع کریں
                        StartLockoutTimerUntil(adminUser.LockedUntil.Value);
                    }
                }
            }
            catch
            {
                // Startup check fail hone par kuch nahi karna
                // اسٹارٹ اپ چیک ناکام ہو تو کچھ نہ کریں
            }
        }

        // ─── Properties | پراپرٹیز ──────────────────────────────────

        private string _username = string.Empty;

        /// <summary>Username input | صارف نام ان پٹ</summary>
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                // Login execute ہوتے وقت status clear نہ کریں
                if (!_isExecutingLogin) ClearStatus();
            }
        }

        private string _password = string.Empty;

        /// <summary>
        /// Password input | پاس ورڈ ان پٹ
        /// (Set from code-behind via PasswordBox.Password)
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                // ✅ FIX 1: finally block mein Password="" se error clear na ho
                // فائنلی بلاک میں Password="" سے error clear نہ ہو
                if (!_isExecutingLogin) ClearStatus();
            }
        }

        private bool _rememberMe = false;

        /// <summary>Remember me checkbox | مجھے یاد رکھیں</summary>
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        // ─── IsLocked property | لاک اسٹیٹس ─────────────────────────
        private bool _isLocked = false;

        /// <summary>
        /// True when account is locked out | جب اکاؤنٹ لاک ہو
        /// Login button is disabled while this is true
        /// </summary>
        public bool IsLocked
        {
            get => _isLocked;
            private set
            {
                SetProperty(ref _isLocked, value);
                // ✅ WPF standard approach — sabhi commands ka CanExecute refresh karo
                // تمام کمانڈز کا CanExecute ریفریش کریں
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        // ─── Login Result Event | لاگ ان نتیجہ ایونٹ ───────────────

        /// <summary>
        /// Raised when login succeeds — View navigates to Dashboard
        /// لاگ ان کامیاب ہونے پر — ویو ڈیش بورڈ پر جائے
        /// </summary>
        public event Action? LoginSucceeded;

        // ─── Commands | کمانڈز ──────────────────────────────────────

        /// <summary>Login command | لاگ ان کمانڈ</summary>
        public ICommand LoginCommand { get; }

        // ─── Execute Login | لاگ ان چلائیں ──────────────────────────
        private void ExecuteLogin(object? parameter)
        {
            // ✅ Extra guard — agar kisi wajah se CanExecute bypass ho
            // اضافی حفاظت — اگر کسی وجہ سے CanExecute bypass ہو
            if (IsLocked) return;

            _isExecutingLogin = true; // Error clear hone se rokhne ke liye flag
            try
            {
                IsBusy = true;
                ClearStatus();

                var result = _authService.Login(Username.Trim(), Password);

                if (result.Success)
                {
                    // ✅ Login kamyab — lockout clear karo
                    StopLockoutTimer();
                    LoginSucceeded?.Invoke();
                }
                else
                {
                    // ✅ FIX 2: AuthService se lockout info nikalo
                    // Check karo ke kya message lockout waala hai
                    // AuthService "locked" ya "30 minutes" ya "lock" likhta hai
                    if (IsLockoutMessage(result.Message))
                    {
                        // ✅ 30 minute ka timer shuru karo
                        StartLockoutTimer(minutes: 30);
                    }

                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"خرابی | Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                // Password صاف کریں — _isExecutingLogin=true ہے
                // تو Password setter ClearStatus() نہیں چلائے گا
                Password = string.Empty;

                // ✅ Flag reset karo — error message ab mehfooz hai
                _isExecutingLogin = false;
            }
        }

        /// <summary>
        /// Check karo ke AuthService ka message lockout message hai ya nahi
        /// AuthService کا پیغام لاک آؤٹ پیغام ہے یا نہیں
        /// </summary>
        private static bool IsLockoutMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return false;

            string lower = message.ToLowerInvariant();
            // AuthService mein jo bhi lockout message hai — in keywords se detect karo
            return lower.Contains("lock") ||
                   lower.Contains("لاک") ||
                   lower.Contains("30") ||
                   lower.Contains("minute") ||
                   lower.Contains("منٹ") ||
                   lower.Contains("disabled") ||
                   lower.Contains("blocked");
        }

        // ─── CanExecuteLogin | لاگ ان کر سکتے ہیں ──────────────────
        private bool CanExecuteLogin(object? parameter) =>
            !IsBusy &&
            !IsLocked &&   // ✅ FIX 2: Locked hone par button disable
            !string.IsNullOrWhiteSpace(Username);

        // ─── Lockout Timer Methods | لاک آؤٹ ٹائمر ─────────────────

        /// <summary>
        /// 30 minute (ya jo bhi) ka lockout timer shuru karo
        /// 30 منٹ (یا جتنا بھی) کا لاک آؤٹ ٹائمر شروع کریں
        /// </summary>
        private void StartLockoutTimer(int minutes = 30)
        {
            // DateTime.Now + minutes se StartLockoutTimerUntil call karo
            StartLockoutTimerUntil(DateTime.Now.AddMinutes(minutes));
        }

        /// <summary>
        /// Exact lockUntil DateTime tak timer chalaao
        /// app restart ke baad bhi remaining time sahi rahe
        /// مخصوص وقت تک ٹائمر چلاؤ — ریسٹارٹ کے بعد بھی صحیح
        /// </summary>
        private void StartLockoutTimerUntil(DateTime lockUntil)
        {
            // Pehla timer band karo agar chal raha ho
            StopLockoutTimer();

            _lockoutUntil = lockUntil;
            IsLocked = true;

            // Countdown message foran dikhao
            UpdateLockoutMessage();

            // Har second update karo | ہر سیکنڈ اپڈیٹ کریں
            _lockoutTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _lockoutTimer.Tick += LockoutTimer_Tick;
            _lockoutTimer.Start();
        }

        /// <summary>
        /// Timer tick — countdown update karo ya unlock karo
        /// ٹائمر ٹِک — الٹی گنتی اپڈیٹ یا انلاک کریں
        /// </summary>
        private void LockoutTimer_Tick(object? sender, EventArgs e)
        {
            if (DateTime.Now >= _lockoutUntil)
            {
                // ✅ Time khatam — unlock karo
                StopLockoutTimer();
                IsLocked = false;
                ShowError(
                    "آپ دوبارہ لاگ ان کر سکتے ہیں | You may try logging in again");
            }
            else
            {
                // Countdown update karo
                UpdateLockoutMessage();
            }
        }

        /// <summary>
        /// Remaining time message dikhao | باقی وقت کا پیغام دکھائیں
        /// </summary>
        private void UpdateLockoutMessage()
        {
            var remaining = _lockoutUntil - DateTime.Now;
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

            int mins = (int)remaining.TotalMinutes;
            int secs = remaining.Seconds;

            ShowError(
                $"اکاؤنٹ لاک ہے — {mins:D2}:{secs:D2} باقی | " +
                $"Account locked — {mins:D2}:{secs:D2} remaining");
        }

        /// <summary>
        /// Timer band karo | ٹائمر بند کریں
        /// </summary>
        private void StopLockoutTimer()
        {
            if (_lockoutTimer != null)
            {
                _lockoutTimer.Stop();
                _lockoutTimer.Tick -= LockoutTimer_Tick;
                _lockoutTimer = null;
            }
            _lockoutUntil = DateTime.MinValue;
            IsLocked = false;
        }

        // ─── Clear Fields | فیلڈز صاف کریں ──────────────────────────
        /// <summary>Reset login form | لاگ ان فارم دوبارہ سیٹ کریں</summary>
        public void ResetForm()
        {
            StopLockoutTimer();
            Username = string.Empty;
            Password = string.Empty;
            RememberMe = false;
            ClearStatus();
        }
    }
}