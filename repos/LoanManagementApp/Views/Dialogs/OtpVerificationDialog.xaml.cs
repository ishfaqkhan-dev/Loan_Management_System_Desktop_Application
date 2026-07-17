using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using LoanManagementApp.Services;

namespace LoanManagementApp.Views.Dialogs
{
    /// <summary>
    /// OtpVerificationDialog.xaml — Code Behind
    /// او ٹی پی تصدیق ڈائیلاگ — کوڈ بیہائنڈ
    /// Handles OTP sending, countdown timer, verification
    /// او ٹی پی بھیجنا، الٹی گنتی، تصدیق
    /// </summary>
    public partial class OtpVerificationDialog : Window, INotifyPropertyChanged
    {
        // ─── INotifyPropertyChanged | پراپرٹی تبدیلی اطلاع ──────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? p = null)
        {
            if (Equals(field, value)) return false;
            field = value; OnPropertyChanged(p); return true;
        }

        // ─── Services | سروسز ────────────────────────────────────────
        private readonly EmailService _emailService;

        // ─── Config | ترتیب ─────────────────────────────────────────
        private readonly int _userId;
        private readonly string _purpose;

        // ─── Countdown Timer | الٹی گنتی ─────────────────────────────
        private DispatcherTimer? _timer;
        private int _secondsLeft = 120;   // 2 min default | 2 منٹ

        // ─── Public Result | نتیجہ ──────────────────────────────────
        /// <summary>True when OTP verified successfully | کامیاب تصدیق</summary>
        public bool IsVerified { get; private set; } = false;

        // ─── Bindable Properties | قابل باندھ خصوصیات ───────────────

        private string _otpCode = string.Empty;
        public string OtpCode
        {
            get => _otpCode;
            set { SetField(ref _otpCode, value); ClearStatus(); }
        }

        private string _subTitle = string.Empty;
        public string SubTitle
        {
            get => _subTitle;
            private set => SetField(ref _subTitle, value);
        }

        private string _infoMessage = string.Empty;
        public string InfoMessage
        {
            get => _infoMessage;
            private set => SetField(ref _infoMessage, value);
        }

        private string _countdownDisplay = string.Empty;
        public string CountdownDisplay
        {
            get => _countdownDisplay;
            private set => SetField(ref _countdownDisplay, value);
        }

        private bool _isCountingDown = true;
        public bool IsCountingDown
        {
            get => _isCountingDown;
            private set => SetField(ref _isCountingDown, value);
        }

        private bool _canResend = false;
        public bool CanResend
        {
            get => _canResend;
            private set => SetField(ref _canResend, value);
        }

        // ─── Status | حیثیت ─────────────────────────────────────────
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => SetField(ref _isBusy, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetField(ref _statusMessage, value);
        }

        private bool _isStatusSuccess = true;
        public bool IsStatusSuccess
        {
            get => _isStatusSuccess;
            private set => SetField(ref _isStatusSuccess, value);
        }

        private bool _hasStatusMessage = false;
        public bool HasStatusMessage
        {
            get => _hasStatusMessage;
            private set => SetField(ref _hasStatusMessage, value);
        }

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        /// <param name="userId">Logged-in user ID | لاگ ان صارف کا نمبر</param>
        /// <param name="purpose">Use OtpPurpose constants | OtpPurpose ثابت قدریں استعمال کریں</param>
        public OtpVerificationDialog(int userId, string purpose)
        {
            InitializeComponent();
            DataContext = this;

            _emailService = new EmailService();
            _userId = userId;
            _purpose = purpose;

            SubTitle = $"Purpose: {purpose}";
            InfoMessage = "Sending OTP to your registered email...\n" +
                          "آپ کے رجسٹرڈ ای میل پر او ٹی پی بھیجا جا رہا ہے...";

            // Wire button clicks | بٹن کلک جوڑیں
            BtnVerify.Click += OnVerifyClicked;
            BtnCancel.Click += (_, _) => { DialogResult = false; Close(); };
            BtnResend.Click += (_, _) => SendOtpAndStartTimer();

            Loaded += (_, _) =>
            {
                SendOtpAndStartTimer();
                TxtOtp.Focus();
            };
        }

        // ─── Send OTP & Start Timer | او ٹی پی بھیجیں اور ٹائمر شروع ─
        /// <summary>
        /// Calls EmailService.SendOtp(userId, purpose) — matches actual method signature.
        /// اصل طریقے کے دستخط کے مطابق EmailService.SendOtp(userId, purpose) کال کرتا ہے۔
        /// </summary>
        private void SendOtpAndStartTimer()
        {
            IsBusy = true;
            CanResend = false;
            ClearStatus();

            try
            {
                // ✅ Correct: SendOtp(int userId, string purpose)
                var result = _emailService.SendOtp(_userId, _purpose);

                if (result.Success)
                {
                    InfoMessage = result.Message +
                                     "\nEnter the 6-digit code below | نیچے 6 ہندسوں کا کوڈ درج کریں";
                    _secondsLeft = 120;
                    IsCountingDown = true;
                    StartCountdown();
                }
                else
                {
                    ShowStatus(false, result.Message);
                    CanResend = true;
                }
            }
            catch (Exception ex)
            {
                ShowStatus(false,
                    $"Failed to send OTP | او ٹی پی بھیجنے میں ناکامی: {ex.Message}");
                CanResend = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Countdown Timer | الٹی گنتی ─────────────────────────────
        private void StartCountdown()
        {
            _timer?.Stop();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) =>
            {
                _secondsLeft--;
                int mins = _secondsLeft / 60;
                int secs = _secondsLeft % 60;
                CountdownDisplay = $"⏱ Expires in: {mins:D2}:{secs:D2} | میعاد: {mins:D2}:{secs:D2}";

                if (_secondsLeft <= 0)
                {
                    _timer.Stop();
                    IsCountingDown = false;
                    CanResend = true;
                    CountdownDisplay = string.Empty;
                    ShowStatus(false,
                        "OTP expired. Please resend. | او ٹی پی کی مدت ختم۔ دوبارہ بھیجیں۔");
                }
            };
            _timer.Start();
        }

        // ─── Verify OTP | او ٹی پی تصدیق کریں ──────────────────────
        /// <summary>
        /// Calls EmailService.VerifyOtp(userId, enteredOtp, purpose).
        /// </summary>
        private async void OnVerifyClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OtpCode))
            {
                ShowStatus(false,
                    "Please enter the OTP | براہ کرم او ٹی پی درج کریں");
                return;
            }

            if (_secondsLeft <= 0)
            {
                ShowStatus(false,
                    "OTP expired. Please resend. | او ٹی پی کی مدت ختم۔ دوبارہ بھیجیں۔");
                return;
            }

            IsBusy = true;
            ClearStatus();

            await System.Threading.Tasks.Task.Delay(300);

            try
            {
                // ✅ Correct: VerifyOtp(int userId, string enteredOtp, string purpose)
                var result = _emailService.VerifyOtp(_userId, OtpCode.Trim(), _purpose);

                if (result.Success)
                {
                    _timer?.Stop();
                    IsVerified = true;
                    ShowStatus(true,
                        "✅ OTP Verified Successfully! | او ٹی پی کامیابی سے تصدیق ہو گیا!");

                    await System.Threading.Tasks.Task.Delay(700);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowStatus(false, result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowStatus(false,
                    $"Verification failed | تصدیق ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Helpers | مددگار ────────────────────────────────────────
        private void ShowStatus(bool success, string message)
        {
            IsStatusSuccess = success;
            StatusMessage = message;
            HasStatusMessage = true;
        }

        private void ClearStatus()
        {
            StatusMessage = string.Empty;
            HasStatusMessage = false;
        }

        // ─── Cleanup on close | بند ہونے پر صفائی ──────────────────
        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}