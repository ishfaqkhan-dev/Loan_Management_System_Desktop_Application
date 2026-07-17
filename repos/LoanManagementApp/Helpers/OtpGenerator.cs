using System;
using System.Text;

namespace LoanManagementApp.Helpers
{
    /// <summary>
    /// OtpGenerator / او ٹی پی بنانے والا
    /// Generates secure numeric OTP codes for email verification
    /// ای میل تصدیق کے لیے محفوظ عددی او ٹی پی کوڈ بناتا ہے
    /// </summary>
    public static class OtpGenerator
    {
        // ─── Default OTP Length | ڈیفالٹ او ٹی پی لمبائی ──────────
        private const int DefaultLength = 6;

        // ─── Secure Random | محفوظ رینڈم ────────────────────────────
        private static readonly Random _random = new Random();

        // ─── Generate OTP | او ٹی پی بنائیں ─────────────────────────
        /// <summary>
        /// Generate a numeric OTP of the given length
        /// دی گئی لمبائی کا عددی او ٹی پی بنائیں
        /// </summary>
        /// <param name="length">
        /// Number of digits (4-8) | ہندسوں کی تعداد
        /// </param>
        /// <returns>
        /// OTP string | او ٹی پی سٹرنگ
        /// </returns>
        public static string Generate(int length = DefaultLength)
        {
            // Clamp length between 4 and 8 | لمبائی 4 سے 8 کے درمیان
            if (length < 4) length = 4;
            if (length > 8) length = 8;

            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(_random.Next(0, 10)); // 0–9 digit | 0–9 ہندسہ

            return sb.ToString();
        }

        // ─── Generate With Expiry | میعاد کے ساتھ بنائیں ────────────
        /// <summary>
        /// Generate OTP and calculate its expiry time
        /// او ٹی پی بنائیں اور اس کی میعاد کا وقت حساب کریں
        /// </summary>
        /// <param name="expiryMinutes">
        /// Minutes until OTP expires | او ٹی پی ختم ہونے کے منٹ
        /// </param>
        /// <param name="length">
        /// OTP digit count | او ٹی پی ہندسوں کی تعداد
        /// </param>
        /// <returns>
        /// (OTP code, Expiry DateTime) | (او ٹی پی کوڈ، میعاد تاریخ)
        /// </returns>
        public static (string Otp, DateTime ExpiryTime) GenerateWithExpiry(
            int expiryMinutes = 5,
            int length = DefaultLength)
        {
            string otp = Generate(length);
            var expiry = DateTime.Now.AddMinutes(expiryMinutes);
            return (otp, expiry);
        }

        // ─── Is OTP Expired | کیا او ٹی پی ختم ہو گیا ────────────────
        /// <summary>
        /// Check if an OTP has expired | جانچیں کیا او ٹی پی ختم ہو گیا
        /// </summary>
        public static bool IsExpired(DateTime expiryTime)
            => DateTime.Now > expiryTime;

        // ─── Mask OTP (for display) | او ٹی پی چھپانا ────────────────
        /// <summary>
        /// Mask part of OTP for safe display (e.g. "12**56")
        /// محفوظ ڈسپلے کے لیے او ٹی پی کا حصہ چھپائیں
        /// </summary>
        public static string Mask(string otp)
        {
            if (string.IsNullOrEmpty(otp) || otp.Length < 4)
                return "****";

            int visibleChars = 2;
            string visible = otp.Substring(0, visibleChars);
            string masked = new string('*', otp.Length - visibleChars * 2);
            string ending = otp.Substring(otp.Length - visibleChars);

            return visible + masked + ending;
        }
    }
}