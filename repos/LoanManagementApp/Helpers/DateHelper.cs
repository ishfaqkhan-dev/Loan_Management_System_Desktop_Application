using System;

namespace LoanManagementApp.Helpers
{
    /// <summary>
    /// DateHelper / تاریخ مددگار
    /// Date formatting and calculation utilities
    /// تاریخ فارمیٹنگ اور حساب کے مددگار
    /// </summary>
    public static class DateHelper
    {
        // ─── Default Formats | ڈیفالٹ فارمیٹس ──────────────────────
        /// <summary>Default date format | ڈیفالٹ تاریخ فارمیٹ</summary>
        public const string DateFormat = "dd-MMM-yyyy";

        /// <summary>Default time format | ڈیفالٹ وقت فارمیٹ</summary>
        public const string TimeFormat = "hh:mm tt";

        /// <summary>Full date-time format | مکمل تاریخ-وقت فارمیٹ</summary>
        public const string DateTimeFormat = "dd-MMM-yyyy hh:mm tt";

        /// <summary>Short date format for tables | ٹیبلز کے لیے مختصر فارمیٹ</summary>
        public const string ShortFormat = "dd/MM/yy";

        // ─── Format Date | تاریخ فارمیٹ کریں ────────────────────────
        /// <summary>
        /// Format a DateTime to display string (dd-MMM-yyyy)
        /// DateTime کو ڈسپلے سٹرنگ میں فارمیٹ کریں
        /// </summary>
        public static string FormatDate(DateTime date)
            => date.ToString(DateFormat);

        // ─── Format DateTime | تاریخ-وقت فارمیٹ کریں ────────────────
        /// <summary>
        /// Format a DateTime with time (dd-MMM-yyyy hh:mm tt)
        /// وقت کے ساتھ DateTime فارمیٹ کریں
        /// </summary>
        public static string FormatDateTime(DateTime date)
            => date.ToString(DateTimeFormat);

        // ─── Format Nullable DateTime | اختیاری DateTime فارمیٹ ─────
        /// <summary>
        /// Format nullable DateTime, return fallback if null
        /// اختیاری DateTime فارمیٹ کریں، null ہو تو فال بیک دیں
        /// </summary>
        public static string FormatDateOrDefault(
            DateTime? date,
            string fallback = "---")
            => date.HasValue ? FormatDate(date.Value) : fallback;

        // ─── Days Remaining | باقی دن ────────────────────────────────
        /// <summary>
        /// Calculate days remaining until a deadline
        /// ڈیڈ لائن تک باقی دن حساب کریں
        /// </summary>
        public static int DaysRemaining(DateTime deadline)
            => (deadline.Date - DateTime.Today).Days;

        // ─── Days Overdue | گزرے ہوئے دن ────────────────────────────
        /// <summary>
        /// Calculate how many days past deadline (positive = overdue)
        /// ڈیڈ لائن کتنے دن گزری (مثبت = میعاد ختم)
        /// </summary>
        public static int DaysOverdue(DateTime deadline)
        {
            int days = (DateTime.Today - deadline.Date).Days;
            return days > 0 ? days : 0;
        }

        // ─── Is Overdue | کیا میعاد ختم ─────────────────────────────
        /// <summary>
        /// Check if a date is in the past | کیا تاریخ گزر گئی
        /// </summary>
        public static bool IsOverdue(DateTime deadline)
            => DateTime.Today > deadline.Date;

        // ─── Due Status Display | مدت حیثیت ڈسپلے ───────────────────
        /// <summary>
        /// Get a human-readable due status string
        /// پڑھنے کے قابل مدت حیثیت سٹرنگ حاصل کریں
        /// </summary>
        public static string DueStatusDisplay(DateTime deadline)
        {
            int days = DaysRemaining(deadline);

            if (days > 30)
                return $"📅 {days} days remaining | {days} دن باقی";

            if (days > 0)
                return $"⚠️ {days} days remaining | {days} دن باقی";

            if (days == 0)
                return "🔴 Due today! | آج کا آخری دن!";

            int overdue = Math.Abs(days);
            return $"❌ {overdue} days overdue | {overdue} دن گزر گئے";
        }

        // ─── Month Name | مہینے کا نام ──────────────────────────────
        /// <summary>
        /// Get formatted month-year string from YYYY-MM format
        /// YYYY-MM فارمیٹ سے فارمیٹ شدہ مہینہ-سال سٹرنگ حاصل کریں
        /// </summary>
        /// <example>
        /// FormatMonthYear("2026-02") → "Feb 2026"
        /// </example>
        public static string FormatMonthYear(string yyyyMm)
        {
            if (string.IsNullOrEmpty(yyyyMm) || yyyyMm.Length < 7)
                return yyyyMm;

            if (DateTime.TryParse(yyyyMm + "-01", out DateTime dt))
                return dt.ToString("MMM yyyy");

            return yyyyMm;
        }

        // ─── Loan Duration Display | قرض مدت ڈسپلے ──────────────────
        /// <summary>
        /// Display loan duration as "X months" or "X days"
        /// قرض مدت کو "X مہینے" یا "X دن" کے طور پر دکھائیں
        /// </summary>
        public static string LoanDurationDisplay(DateTime start, DateTime end)
        {
            TimeSpan span = end - start;
            int totalDays = (int)span.TotalDays;

            if (totalDays >= 30)
            {
                int months = (int)(totalDays / 30.44);
                return $"{months} months | {months} مہینے";
            }

            return $"{totalDays} days | {totalDays} دن";
        }

        // ─── Today's Display | آج کی تاریخ ڈسپلے ────────────────────
        /// <summary>
        /// Get today's date as a full display string
        /// آج کی تاریخ مکمل ڈسپلے سٹرنگ کے طور پر حاصل کریں
        /// </summary>
        public static string TodayDisplay =>
            $"{DateTime.Now:dddd, dd MMMM yyyy}";

        // ─── Time Ago Display | کب سے ────────────────────────────────
        /// <summary>
        /// Get "X minutes ago" / "X hours ago" style display
        /// "X منٹ پہلے" / "X گھنٹے پہلے" اسٹائل ڈسپلے حاصل کریں
        /// </summary>
        public static string TimeAgo(DateTime past)
        {
            TimeSpan diff = DateTime.Now - past;

            if (diff.TotalMinutes < 1)
                return "Just now | ابھی";

            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} min ago | منٹ پہلے";

            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hrs ago | گھنٹے پہلے";

            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago | دن پہلے";

            return FormatDate(past);
        }

        // ─── Parse Date Safe | تاریخ محفوظ طریقے سے حاصل کریں ──────
        /// <summary>
        /// Safely parse a date string, return Today if invalid
        /// تاریخ سٹرنگ محفوظ طریقے سے حاصل کریں، غلط ہو تو آج دیں
        /// </summary>
        public static DateTime ParseDateOrToday(string input)
        {
            if (DateTime.TryParse(input, out DateTime result))
                return result;
            return DateTime.Today;
        }
    }
}