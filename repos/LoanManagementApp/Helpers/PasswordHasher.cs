using System;
using BCrypt.Net;

namespace LoanManagementApp.Helpers
{
    /// <summary>
    /// PasswordHasher / پاس ورڈ ہیشر
    /// BCrypt-based secure password hashing and verification
    /// BCrypt پر مبنی محفوظ پاس ورڈ ہیشنگ اور تصدیق
    /// All password storage must go through this class
    /// تمام پاس ورڈ ذخیرہ کاری اس کلاس سے گزرنی چاہیے
    /// </summary>
    public static class PasswordHasher
    {
        // ─── BCrypt Work Factor | BCrypt ورک فیکٹر ──────────────────
        /// <summary>
        /// BCrypt work factor — higher = slower = more secure
        /// BCrypt ورک فیکٹر — زیادہ = سست = زیادہ محفوظ
        /// 11 is a good balance for desktop apps | 11 ڈیسک ٹاپ ایپس کے لیے مناسب
        /// </summary>
        private const int WorkFactor = 11;

        // ─── Hash Password | پاس ورڈ ہیش کریں ───────────────────────
        /// <summary>
        /// Hash a plain-text password using BCrypt
        /// BCrypt سے سادہ پاس ورڈ ہیش کریں
        /// </summary>
        /// <param name="plainPassword">
        /// Raw password to hash | ہیش کرنے کا خام پاس ورڈ
        /// </param>
        /// <returns>
        /// BCrypt hash string | BCrypt ہیش سٹرنگ
        /// </returns>
        public static string Hash(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException(
                    "Password cannot be empty | پاس ورڈ خالی نہیں ہو سکتا");

            return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
        }

        // ─── Verify Password | پاس ورڈ تصدیق کریں ───────────────────
        /// <summary>
        /// Verify a plain-text password against a BCrypt hash
        /// BCrypt ہیش کے خلاف سادہ پاس ورڈ تصدیق کریں
        /// </summary>
        /// <param name="plainPassword">
        /// Password entered by user | صارف کا داخل کردہ پاس ورڈ
        /// </param>
        /// <param name="hashedPassword">
        /// Stored BCrypt hash | محفوظ BCrypt ہیش
        /// </param>
        /// <returns>
        /// True if match | میل کھائے تو True
        /// </returns>
        public static bool Verify(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword)) return false;
            if (string.IsNullOrWhiteSpace(hashedPassword)) return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
            }
            catch
            {
                // Invalid hash format | غلط ہیش فارمیٹ
                return false;
            }
        }

        // ─── Hash Security Answer | حفاظتی جواب ہیش ─────────────────
        /// <summary>
        /// Hash a security answer (case-insensitive, trimmed)
        /// حفاظتی جواب ہیش کریں (بڑے چھوٹے حروف سے قطع نظر، صاف)
        /// </summary>
        public static string HashSecurityAnswer(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
                throw new ArgumentException(
                    "Answer cannot be empty | جواب خالی نہیں ہو سکتا");

            // Normalize: lowercase + trim | نارمل: چھوٹے حروف + صاف
            return Hash(answer.Trim().ToLower());
        }

        // ─── Verify Security Answer | حفاظتی جواب تصدیق ────────────
        /// <summary>
        /// Verify a security answer against its hash
        /// جواب کو اس کے ہیش کے خلاف تصدیق کریں
        /// </summary>
        public static bool VerifySecurityAnswer(string plainAnswer, string hashedAnswer)
        {
            if (string.IsNullOrWhiteSpace(plainAnswer)) return false;
            if (string.IsNullOrWhiteSpace(hashedAnswer)) return false;

            return Verify(plainAnswer.Trim().ToLower(), hashedAnswer);
        }

        // ─── Is Strong Password | مضبوط پاس ورڈ ─────────────────────
        /// <summary>
        /// Check if a password meets minimum strength requirements
        /// کیا پاس ورڈ کم از کم مضبوطی کی ضروریات پوری کرتا ہے
        /// Minimum 4 chars (admin-style app, no complex rules needed)
        /// کم از کم 4 حروف (ایڈمن ایپ، پیچیدہ قواعد ضروری نہیں)
        /// </summary>
        public static (bool IsStrong, string Message) CheckStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required | پاس ورڈ ضروری ہے");

            if (password.Length < 4)
                return (false,
                    "Password must be at least 4 characters | " +
                    "پاس ورڈ کم از کم 4 حروف کا ہونا چاہیے");

            if (password.Length > 50)
                return (false,
                    "Password is too long (max 50) | " +
                    "پاس ورڈ بہت لمبا ہے (زیادہ سے زیادہ 50)");

            return (true, "Password is acceptable | پاس ورڈ قابل قبول ہے");
        }
    }
}