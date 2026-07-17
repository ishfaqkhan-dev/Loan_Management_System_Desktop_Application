using System;
using LoanManagementApp.Data;
using LoanManagementApp.Models;

namespace LoanManagementApp.Services
{
    /// <summary>
    /// AuthService / تصدیق سروس - Handles all login, security and OTP logic
    /// </summary>
    public class AuthService
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly UserRepository _userRepo;
        private readonly AppSettingsService _settingsService;

        // ─── Currently Logged In User | موجودہ لاگ ان صارف ──────────
        /// <summary>
        /// Currently logged in user | ابھی لاگ ان صارف
        /// </summary>
        public static User? CurrentUser { get; private set; }

        /// <summary>
        /// Is any user logged in | کیا کوئی صارف لاگ ان ہے
        /// </summary>
        public static bool IsLoggedIn => CurrentUser != null;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public AuthService()
        {
            _userRepo = new UserRepository();
            _settingsService = new AppSettingsService();
        }

        // ─── Login | لاگ ان ──────────────────────────────────────────
        /// <summary>
        /// Attempt login with username and password
        /// صارف نام اور پاس ورڈ سے لاگ ان کی کوشش کریں
        /// </summary>
        public (bool Success, string Message) Login(string username, string password)
        {
            try
            {
                // Validate input | ان پٹ جانچیں
                if (string.IsNullOrWhiteSpace(username))
                    return (false, "Username is required | صارف نام ضروری ہے");

                if (string.IsNullOrWhiteSpace(password))
                    return (false, "Password is required | پاس ورڈ ضروری ہے");

                // Auto unlock expired locks | میعاد ختم بندشیں کھولیں
                _userRepo.AutoUnlockExpiredAccounts();

                // Find user | صارف تلاش کریں
                var user = _userRepo.GetUserByUsername(username.Trim());
                if (user == null)
                    return (false,
                        "Invalid username or password | غلط صارف نام یا پاس ورڈ");

                // Check if account is locked | کیا اکاؤنٹ بند ہے
                if (user.IsCurrentlyLocked)
                    return (false,
                        $"Account is locked until {user.LockedUntil:hh:mm tt} | " +
                        $"اکاؤنٹ {user.LockedUntil:hh:mm tt} تک بند ہے");

                // Get settings for max attempts | زیادہ سے زیادہ کوششوں کے لیے ترتیبات
                var settings = _settingsService.GetSettings();

                // Verify password | پاس ورڈ تصدیق کریں
                bool passwordOk = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (!passwordOk)
                {
                    // Record failed attempt | ناکام کوشش ریکارڈ کریں
                    _userRepo.RecordFailedLogin(
                        user.Id,
                        settings.MaxFailedLoginAttempts,
                        settings.AccountLockDurationMinutes);

                    // Reload user to get updated attempts count
                    // اپ ڈیٹ شدہ کوششوں کی گنتی کے لیے صارف دوبارہ لوڈ کریں
                    user = _userRepo.GetUserById(user.Id)!;

                    int remaining = settings.MaxFailedLoginAttempts
                                  - user.FailedLoginAttempts;

                    if (user.IsCurrentlyLocked)
                        return (false,
                            $"Account locked for {settings.AccountLockDurationMinutes} min | " +
                            $"اکاؤنٹ {settings.AccountLockDurationMinutes} منٹ کے لیے بند ہے");

                    return (false,
                        $"Wrong password. {remaining} attempts left | " +
                        $"غلط پاس ورڈ۔ {remaining} کوششیں باقی ہیں");
                }

                // Login success | لاگ ان کامیاب
                _userRepo.RecordLoginSuccess(user.Id);

                // Reload fresh user data | تازہ صارف ڈیٹا لوڈ کریں
                CurrentUser = _userRepo.GetUserById(user.Id);

                return (true, "Login successful | لاگ ان کامیاب");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Login failed | لاگ ان ناکام: {ex.Message}");
            }
        }

        // ─── Logout | لاگ آؤٹ ────────────────────────────────────────
        /// <summary>
        /// Log out current user | موجودہ صارف کو لاگ آؤٹ کریں
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
        }

        // ─── Change Password | پاس ورڈ تبدیل کریں ───────────────────
        /// <summary>
        /// Change password after verifying old password
        /// پرانا پاس ورڈ تصدیق کے بعد نیا پاس ورڈ سیٹ کریں
        /// </summary>
        public (bool Success, string Message) ChangePassword(
            int userId,
            string oldPassword,
            string newPassword,
            string confirmPassword)
        {
            try
            {
                // Validate | جانچیں
                if (string.IsNullOrWhiteSpace(newPassword))
                    return (false, "New password is required | نیا پاس ورڈ ضروری ہے");

                if (newPassword.Length < 4)
                    return (false,
                        "Password must be at least 4 characters | " +
                        "پاس ورڈ کم از کم 4 حروف کا ہونا چاہیے");

                if (newPassword != confirmPassword)
                    return (false,
                        "Passwords do not match | پاس ورڈ میل نہیں کھاتے");

                // Get user | صارف حاصل کریں
                var user = _userRepo.GetUserById(userId);
                if (user == null)
                    return (false, "User not found | صارف نہیں ملا");

                // Verify old password | پرانا پاس ورڈ تصدیق کریں
                if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                    return (false,
                        "Old password is incorrect | پرانا پاس ورڈ غلط ہے");

                // Hash and save new password | نیا پاس ورڈ ہیش کر کے محفوظ کریں
                string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                bool updated = _userRepo.UpdatePassword(userId, newHash);

                if (updated && CurrentUser?.Id == userId)
                    CurrentUser = _userRepo.GetUserById(userId);

                return updated
                    ? (true, "Password changed successfully | پاس ورڈ کامیابی سے تبدیل ہوا")
                    : (false, "Failed to change password | پاس ورڈ تبدیل کرنے میں ناکامی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Change password failed | پاس ورڈ تبدیل ناکام: {ex.Message}");
            }
        }

        // ─── Change Username | صارف نام تبدیل کریں ──────────────────
        /// <summary>
        /// Change username after PIN or security question verification
        /// پن یا حفاظتی سوال کی تصدیق کے بعد صارف نام تبدیل کریں
        /// </summary>
        public (bool Success, string Message) ChangeUsername(
            int userId,
            string newUsername)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newUsername))
                    return (false, "Username is required | صارف نام ضروری ہے");

                if (newUsername.Trim().Length < 3)
                    return (false,
                        "Username must be at least 3 characters | " +
                        "صارف نام کم از کم 3 حروف کا ہونا چاہیے");

                // Check duplicate | ڈپلیکیٹ چیک کریں
                if (_userRepo.UsernameExists(newUsername.Trim(), userId))
                    return (false,
                        "Username already taken | یہ صارف نام پہلے سے موجود ہے");

                bool updated = _userRepo.UpdateUsername(userId, newUsername.Trim());

                if (updated && CurrentUser?.Id == userId)
                    CurrentUser = _userRepo.GetUserById(userId);

                return updated
                    ? (true, "Username changed successfully | صارف نام کامیابی سے تبدیل ہوا")
                    : (false, "Failed to change username | صارف نام تبدیل کرنے میں ناکامی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Change username failed | صارف نام تبدیل ناکام: {ex.Message}");
            }
        }

        // ─── Set PIN | پن کوڈ سیٹ کریں ──────────────────────────────
        /// <summary>
        /// Set or update PIN code | پن کوڈ سیٹ یا اپ ڈیٹ کریں
        /// </summary>
        public (bool Success, string Message) SetPin(
            int userId,
            string pin,
            string confirmPin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pin))
                    return (false, "PIN is required | پن ضروری ہے");

                if (pin.Length < 4 || pin.Length > 6)
                    return (false,
                        "PIN must be 4 to 6 digits | پن 4 سے 6 ہندسوں کا ہونا چاہیے");

                if (!IsNumeric(pin))
                    return (false,
                        "PIN must contain digits only | پن صرف ہندسوں پر مشتمل ہو");

                if (pin != confirmPin)
                    return (false, "PINs do not match | پن میل نہیں کھاتے");

                bool updated = _userRepo.UpdatePin(userId, pin);

                if (updated && CurrentUser?.Id == userId)
                    CurrentUser = _userRepo.GetUserById(userId);

                return updated
                    ? (true, "PIN set successfully | پن کامیابی سے سیٹ ہوا")
                    : (false, "Failed to set PIN | پن سیٹ کرنے میں ناکامی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Set PIN failed | پن سیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Verify PIN | پن تصدیق کریں ─────────────────────────────
        /// <summary>
        /// Verify entered PIN against stored PIN
        /// درج کردہ پن کو محفوظ پن سے تصدیق کریں
        /// </summary>
        public bool VerifyPin(int userId, string enteredPin)
        {
            try
            {
                var user = _userRepo.GetUserById(userId);
                if (user == null) return false;

                return !string.IsNullOrEmpty(user.PinCode) &&
                       user.PinCode == enteredPin;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"PIN verification failed | پن تصدیق ناکام: {ex.Message}");
            }
        }

        // ─── Set Security Question | حفاظتی سوال سیٹ کریں ──────────
        /// <summary>
        /// Set security question and answer
        /// حفاظتی سوال اور جواب سیٹ کریں
        /// </summary>
        public (bool Success, string Message) SetSecurityQuestion(
            int userId,
            string question,
            string answer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question))
                    return (false,
                        "Security question is required | حفاظتی سوال ضروری ہے");

                if (string.IsNullOrWhiteSpace(answer))
                    return (false,
                        "Security answer is required | حفاظتی جواب ضروری ہے");

                // Hash the answer | جواب ہیش کریں
                string answerHash = BCrypt.Net.BCrypt.HashPassword(
                    answer.Trim().ToLower());

                bool updated = _userRepo.UpdateSecurityQuestion(
                    userId, question.Trim(), answerHash);

                if (updated && CurrentUser?.Id == userId)
                    CurrentUser = _userRepo.GetUserById(userId);

                return updated
                    ? (true, "Security question saved | حفاظتی سوال محفوظ ہوا")
                    : (false, "Failed to save | محفوظ کرنے میں ناکامی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Set security question failed | حفاظتی سوال سیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Verify Security Answer | حفاظتی جواب تصدیق کریں ────────
        /// <summary>
        /// Verify security question answer
        /// حفاظتی سوال کا جواب تصدیق کریں
        /// </summary>
        public bool VerifySecurityAnswer(int userId, string answer)
        {
            try
            {
                var user = _userRepo.GetUserById(userId);
                if (user == null) return false;

                if (string.IsNullOrEmpty(user.SecurityAnswerHash))
                    return false;

                return BCrypt.Net.BCrypt.Verify(
                    answer.Trim().ToLower(),
                    user.SecurityAnswerHash);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Security answer verification failed | جواب تصدیق ناکام: {ex.Message}");
            }
        }

        // ─── Reset Password Via Security | حفاظتی سوال سے پاس ورڈ ──
        /// <summary>
        /// Reset password using security answer verification
        /// حفاظتی جواب کی تصدیق کے بعد پاس ورڈ ریسیٹ کریں
        /// </summary>
        public (bool Success, string Message) ResetPasswordViaSecurityAnswer(
            int userId,
            string securityAnswer,
            string newPassword,
            string confirmPassword)
        {
            try
            {
                if (!VerifySecurityAnswer(userId, securityAnswer))
                    return (false,
                        "Security answer is incorrect | حفاظتی جواب غلط ہے");

                if (string.IsNullOrWhiteSpace(newPassword))
                    return (false,
                        "New password is required | نیا پاس ورڈ ضروری ہے");

                if (newPassword.Length < 4)
                    return (false,
                        "Password must be at least 4 characters | " +
                        "پاس ورڈ کم از کم 4 حروف کا ہونا چاہیے");

                if (newPassword != confirmPassword)
                    return (false,
                        "Passwords do not match | پاس ورڈ میل نہیں کھاتے");

                string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                bool updated = _userRepo.UpdatePassword(userId, newHash);

                return updated
                    ? (true, "Password reset successful | پاس ورڈ ریسیٹ کامیاب")
                    : (false, "Failed to reset password | پاس ورڈ ریسیٹ ناکام");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Reset password failed | پاس ورڈ ریسیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Update Theme | تھیم اپ ڈیٹ کریں ───────────────────────
        /// <summary>
        /// Update current user preferred theme
        /// موجودہ صارف کا پسندیدہ تھیم اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateTheme(AppTheme theme)
        {
            try
            {
                if (CurrentUser == null) return false;

                bool updated = _userRepo.UpdateTheme(CurrentUser.Id, theme);

                if (updated)
                    CurrentUser = _userRepo.GetUserById(CurrentUser.Id);

                return updated;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Theme update failed | تھیم اپ ڈیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Get Current User | موجودہ صارف حاصل کریں ───────────────
        /// <summary>
        /// Get fresh current user data from database
        /// ڈیٹابیس سے تازہ موجودہ صارف ڈیٹا حاصل کریں
        /// </summary>
        public User? GetCurrentUser()
        {
            try
            {
                if (CurrentUser == null) return null;

                CurrentUser = _userRepo.GetUserById(CurrentUser.Id);
                return CurrentUser;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get current user | موجودہ صارف حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Has PIN Set | کیا پن سیٹ ہے ────────────────────────────
        /// <summary>
        /// Check if user has PIN set | کیا صارف کا پن سیٹ ہے
        /// </summary>
        public bool HasPinSet(int userId)
        {
            try
            {
                var user = _userRepo.GetUserById(userId);
                return user != null && !string.IsNullOrEmpty(user.PinCode);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to check PIN | پن جانچنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Has Security Question Set | کیا حفاظتی سوال سیٹ ہے ─────
        /// <summary>
        /// Check if user has security question set
        /// کیا صارف کا حفاظتی سوال سیٹ ہے
        /// </summary>
        public bool HasSecurityQuestionSet(int userId)
        {
            try
            {
                var user = _userRepo.GetUserById(userId);
                return user != null &&
                       !string.IsNullOrEmpty(user.SecurityQuestion) &&
                       !string.IsNullOrEmpty(user.SecurityAnswerHash);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to check security question | حفاظتی سوال جانچنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Private Helpers ─────────────────────────────────────────

        /// <summary>
        /// Check if string contains digits only | کیا سٹرنگ صرف ہندسوں پر مشتمل ہے
        /// </summary>
        private bool IsNumeric(string value)
        {
            foreach (char c in value)
                if (!char.IsDigit(c)) return false;
            return true;
        }
    }
}