using System;

namespace LoanManagementApp.Models
{
    /// <summary>
    /// User / صارف - Stores authentication and security details
    /// </summary>
    public class User
    {
        // ─── Primary Key ───────────────────────────────────────────
        /// <summary>
        /// Unique User ID | منفرد صارف نمبر
        /// </summary>
        public int Id { get; set; }

        // ─── Login Credentials | لاگ ان کی معلومات ────────────────
        /// <summary>
        /// Username | صارف نام
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password Hash (BCrypt) | خفیہ پاس ورڈ
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        // ─── Security Info | حفاظتی معلومات ────────────────────────
        /// <summary>
        /// PIN Code (4-6 digits) | پن کوڈ
        /// </summary>
        public string PinCode { get; set; } = string.Empty;

        /// <summary>
        /// Security Question | حفاظتی سوال
        /// </summary>
        public string SecurityQuestion { get; set; } = string.Empty;

        /// <summary>
        /// Security Answer (Hashed) | حفاظتی جواب
        /// </summary>
        public string SecurityAnswerHash { get; set; } = string.Empty;

        // ─── Email Info | ای میل کی معلومات ────────────────────────
        /// <summary>
        /// Email Address for OTP | ای میل ایڈریس
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Is Email Verified | کیا ای میل تصدیق شدہ ہے
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;

        // ─── OTP Info | او ٹی پی کی معلومات ────────────────────────
        /// <summary>
        /// Current OTP Code | موجودہ او ٹی پی کوڈ
        /// </summary>
        public string OtpCode { get; set; } = string.Empty;

        /// <summary>
        /// OTP Expiry Time | او ٹی پی ختم ہونے کا وقت
        /// </summary>
        public DateTime? OtpExpiryTime { get; set; }

        /// <summary>
        /// OTP Purpose | او ٹی پی کا مقصد
        /// ChangePassword=پاس ورڈ تبدیل, ChangeUsername=صارف نام تبدیل
        /// </summary>
        public string OtpPurpose { get; set; } = string.Empty;

        // ─── Login Tracking | لاگ ان ٹریکنگ ───────────────────────
        /// <summary>
        /// Last Login Date | آخری لاگ ان کی تاریخ
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Failed Login Attempts | ناکام لاگ ان کوششیں
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// Is Account Locked | کیا اکاؤنٹ بند ہے
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Account Locked Until | اکاؤنٹ کب تک بند ہے
        /// </summary>
        public DateTime? LockedUntil { get; set; }

        // ─── Role & Status | کردار اور حیثیت ───────────────────────
        /// <summary>
        /// User Role | صارف کا کردار
        /// </summary>
        public UserRole Role { get; set; } = UserRole.Admin;

        /// <summary>
        /// Is Active | کیا فعال ہے
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ─── Dates | تاریخیں ────────────────────────────────────────
        /// <summary>
        /// Account Created Date | اکاؤنٹ بنانے کی تاریخ
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last Updated Date | آخری تبدیلی کی تاریخ
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ─── Theme Preference | تھیم کی ترجیح ──────────────────────
        /// <summary>
        /// Preferred Theme | پسندیدہ تھیم
        /// Dark=گہرا, Light=روشن
        /// </summary>
        public AppTheme PreferredTheme { get; set; } = AppTheme.Light;

        // ─── Computed Properties ────────────────────────────────────
        /// <summary>
        /// Is OTP Valid | کیا او ٹی پی درست ہے
        /// </summary>
        public bool IsOtpValid =>
            !string.IsNullOrEmpty(OtpCode) &&
            OtpExpiryTime.HasValue &&
            DateTime.Now <= OtpExpiryTime.Value;

        /// <summary>
        /// Is Account Currently Locked | کیا اکاؤنٹ ابھی بند ہے
        /// </summary>
        public bool IsCurrentlyLocked =>
            IsLocked &&
            LockedUntil.HasValue &&
            DateTime.Now <= LockedUntil.Value;

        /// <summary>
        /// Lock Status in Urdu | بندش کی حیثیت اردو میں
        /// </summary>
        public string LockStatusUrdu => IsCurrentlyLocked
            ? $"🔒 اکاؤنٹ بند ہے - {LockedUntil:hh:mm tt} تک"
            : "✅ اکاؤنٹ فعال ہے";

        /// <summary>
        /// Lock Status in English | بندش کی حیثیت انگریزی میں
        /// </summary>
        public string LockStatusEnglish => IsCurrentlyLocked
            ? $"🔒 Account Locked until {LockedUntil:hh:mm tt}"
            : "✅ Account Active";

        /// <summary>
        /// Role Display in Urdu | کردار اردو میں
        /// </summary>
        public string RoleUrdu => Role switch
        {
            UserRole.Admin => "👑 ایڈمن",
            UserRole.Manager => "👤 مینیجر",
            UserRole.Viewer => "👁️ دیکھنے والا",
            _ => "نامعلوم"
        };

        /// <summary>
        /// Role Display in English | کردار انگریزی میں
        /// </summary>
        public string RoleEnglish => Role switch
        {
            UserRole.Admin => "👑 Admin",
            UserRole.Manager => "👤 Manager",
            UserRole.Viewer => "👁️ Viewer",
            _ => "Unknown"
        };

        /// <summary>
        /// Last Login Display | آخری لاگ ان فارمیٹ کے ساتھ
        /// </summary>
        public string LastLoginDisplay => LastLoginDate.HasValue
            ? $"{LastLoginDate:dd-MMM-yyyy hh:mm tt} | آخری لاگ ان"
            : "Never Logged In | کبھی لاگ ان نہیں ہوا";
    }

    // ─── User Role Enum | صارف کا کردار ────────────────────────────
    /// <summary>
    /// User Role Options | صارف کے کردار کے اختیارات
    /// </summary>
    public enum UserRole
    {
        /// <summary>Admin / ایڈمن</summary>
        Admin = 1,

        /// <summary>Manager / مینیجر</summary>
        Manager = 2,

        /// <summary>Viewer / دیکھنے والا</summary>
        Viewer = 3
    }

    // ─── App Theme Enum | ایپ تھیم ──────────────────────────────────
    /// <summary>
    /// Application Theme Options | ایپلیکیشن تھیم کے اختیارات
    /// </summary>
    public enum AppTheme
    {
        /// <summary>Light Mode / روشن موڈ</summary>
        Light = 1,

        /// <summary>Dark Mode / گہرا موڈ</summary>
        Dark = 2
    }
}