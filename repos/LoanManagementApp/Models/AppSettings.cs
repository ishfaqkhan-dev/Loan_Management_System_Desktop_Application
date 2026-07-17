using LoanManagementApp.Models;
using System;
using System.Windows.Controls;

namespace LoanManagementApp.Models
{
    /// <summary>
    /// AppSettings / ایپ کی ترتیبات - Stores all application configuration
    /// </summary>
    public class AppSettings
    {
        // ─── Primary Key ───────────────────────────────────────────
        /// <summary>
        /// Unique Settings ID | منفرد ترتیبات نمبر
        /// </summary>
        public int Id { get; set; }

        // ─── Theme Settings | تھیم کی ترتیبات ──────────────────────
        /// <summary>
        /// Current App Theme | موجودہ ایپ تھیم
        /// Dark=گہرا, Light=روشن
        /// </summary>
        public AppTheme CurrentTheme { get; set; } = AppTheme.Light;

        /// <summary>
        /// Theme Last Changed Date | تھیم آخری بار تبدیل کرنے کی تاریخ
        /// </summary>
        public DateTime ThemeLastChanged { get; set; } = DateTime.Now;

        // ─── Currency Settings | کرنسی کی ترتیبات ──────────────────
        /// <summary>
        /// Default Currency | ڈیفالٹ کرنسی
        /// e.g. AED, PKR, USD
        /// </summary>
        public string DefaultCurrency { get; set; } = "PKR";

        /// <summary>
        /// Currency Symbol | کرنسی کی علامت
        /// </summary>
        public string CurrencySymbol { get; set; } = "PKR";

        /// <summary>
        /// Secondary Currency | ثانوی کرنسی
        /// </summary>
        public string SecondaryCurrency { get; set; } = "PKR";

        /// <summary>
        /// Exchange Rate (1 AED = ? PKR) | تبادلہ شرح
        /// </summary>
        public decimal ExchangeRate { get; set; } = 77;

        /// <summary>
        /// Exchange Rate Last Updated | تبادلہ شرح آخری بار اپ ڈیٹ کی تاریخ
        /// </summary>
        public DateTime ExchangeRateLastUpdated { get; set; } = DateTime.Now;

        // ─── Email / SMTP Settings | ای میل کی ترتیبات ─────────────
        /// <summary>
        /// SMTP Host | ایس ایم ٹی پی ہوسٹ
        /// e.g. smtp.gmail.com
        /// </summary>
        public string SmtpHost { get; set; } = "smtp.gmail.com";

        /// <summary>
        /// SMTP Port | ایس ایم ٹی پی پورٹ
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// SMTP Username / Email | ایس ایم ٹی پی صارف نام
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;

        /// <summary>
        /// SMTP Password | ایس ایم ٹی پی پاس ورڈ
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// Sender Email Address | بھیجنے والے کا ای میل
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// Sender Display Name | بھیجنے والے کا نام
        /// </summary>
        public string SenderName { get; set; } = "Loan Management System";

        /// <summary>
        /// Is Email Configured | کیا ای میل ترتیب دی گئی ہے
        /// </summary>
        public bool IsEmailConfigured { get; set; } = false;

        /// <summary>
        /// Enable SSL for Email | ای میل کے لیے SSL فعال کریں
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        // ─── Backup Settings | بیک اپ کی ترتیبات ───────────────────
        /// <summary>
        /// Backup Folder Path | بیک اپ فولڈر کا راستہ
        /// </summary>
        public string BackupFolderPath { get; set; } =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            + @"\LoanManagementBackups";

        /// <summary>
        /// Auto Backup Enabled | خودکار بیک اپ فعال ہے
        /// </summary>
        public bool AutoBackupEnabled { get; set; } = true;

        /// <summary>
        /// Auto Backup Interval in Hours | خودکار بیک اپ وقفہ گھنٹوں میں
        /// </summary>
        public int AutoBackupIntervalHours { get; set; } = 24;

        /// <summary>
        /// Last Backup Date | آخری بیک اپ کی تاریخ
        /// </summary>
        public DateTime? LastBackupDate { get; set; }

        /// <summary>
        /// Last Backup File Path | آخری بیک اپ فائل کا راستہ
        /// </summary>
        public string LastBackupFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Max Backup Files to Keep | زیادہ سے زیادہ بیک اپ فائلیں
        /// </summary>
        public int MaxBackupFilesToKeep { get; set; } = 10;

        // ─── Application Info | ایپلیکیشن کی معلومات ───────────────
        /// <summary>
        /// Application Name | ایپلیکیشن کا نام
        /// </summary>
        public string AppName { get; set; } = "Loan Management System";

        /// <summary>
        /// Application Name in Urdu | ایپلیکیشن کا نام اردو میں
        /// </summary>
        public string AppNameUrdu { get; set; } = "قرض مینجمنٹ سسٹم";

        /// <summary>
        /// Company / Shop Name | کمپنی / دکان کا نام
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Company Name in Urdu | کمپنی کا نام اردو میں
        /// </summary>
        public string CompanyNameUrdu { get; set; } = string.Empty;

        /// <summary>
        /// Company Phone | کمپنی کا فون
        /// </summary>
        public string CompanyPhone { get; set; } = string.Empty;

        /// <summary>
        /// Company Address | کمپنی کا پتہ
        /// </summary>
        public string CompanyAddress { get; set; } = string.Empty;

        // ─── OTP Settings | او ٹی پی کی ترتیبات ────────────────────
        /// <summary>
        /// OTP Expiry Minutes | او ٹی پی ختم ہونے کے منٹ
        /// </summary>
        public int OtpExpiryMinutes { get; set; } = 5;

        /// <summary>
        /// OTP Length | او ٹی پی کی لمبائی
        /// </summary>
        public int OtpLength { get; set; } = 6;

        // ─── Security Settings | حفاظتی ترتیبات ────────────────────
        /// <summary>
        /// Max Failed Login Attempts | زیادہ سے زیادہ ناکام لاگ ان کوششیں
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// Account Lock Duration in Minutes | اکاؤنٹ بندش کا وقت منٹوں میں
        /// </summary>
        public int AccountLockDurationMinutes { get; set; } = 30;

        /// <summary>
        /// Require PIN for Sensitive Actions | حساس کاموں کے لیے پن ضروری ہے
        /// </summary>
        public bool RequirePinForSensitiveActions { get; set; } = true;

        // ─── Date & Time Settings | تاریخ اور وقت کی ترتیبات ───────
        /// <summary>
        /// Date Format | تاریخ کی شکل
        /// </summary>
        public string DateFormat { get; set; } = "dd-MMM-yyyy";

        /// <summary>
        /// Time Format | وقت کی شکل
        /// </summary>
        public string TimeFormat { get; set; } = "hh:mm tt";

        /// <summary>
        /// Settings Last Updated | ترتیبات آخری بار اپ ڈیٹ کی تاریخ
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ─── Computed Properties ────────────────────────────────────
        /// <summary>
        /// Last Backup Display | آخری بیک اپ فارمیٹ کے ساتھ
        /// </summary>
        public string LastBackupDisplay => LastBackupDate.HasValue
            ? $"{LastBackupDate:dd-MMM-yyyy hh:mm tt} | آخری بیک اپ"
            : "No Backup Yet | ابھی تک کوئی بیک اپ نہیں";

        /// <summary>
        /// Is Backup Due | کیا بیک اپ کا وقت ہو گیا
        /// </summary>
        public bool IsBackupDue => AutoBackupEnabled && (
            !LastBackupDate.HasValue ||
            DateTime.Now >= LastBackupDate.Value.AddHours(AutoBackupIntervalHours));

        /// <summary>
        /// Next Backup Time | اگلے بیک اپ کا وقت
        /// </summary>
        public string NextBackupDisplay => LastBackupDate.HasValue
            ? $"Next: {LastBackupDate.Value.AddHours(AutoBackupIntervalHours):dd-MMM-yyyy hh:mm tt} | اگلا بیک اپ"
            : "Pending | باقی ہے";

        /// <summary>
        /// Exchange Rate Display | تبادلہ شرح فارمیٹ کے ساتھ
        /// </summary>
        public string ExchangeRateDisplay =>
            $"1 {DefaultCurrency} = {ExchangeRate} {SecondaryCurrency} | تبادلہ شرح";

        /// <summary>
        /// Theme Display in Urdu | تھیم اردو میں
        /// </summary>
        public string ThemeDisplayUrdu => CurrentTheme == AppTheme.Dark
            ? "🌙 گہرا موڈ"
            : "☀️ روشن موڈ";

        /// <summary>
        /// Theme Display in English | تھیم انگریزی میں
        /// </summary>
        public string ThemeDisplayEnglish => CurrentTheme == AppTheme.Dark
            ? "🌙 Dark Mode"
            : "☀️ Light Mode";
    }
}
