using System;

namespace LoanManagementApp.Helpers
{
    /// <summary>
    /// CurrencyConverter / کرنسی کنورٹر
    /// AED ↔ PKR currency conversion and formatting
    /// AED ↔ PKR کرنسی تبدیلی اور فارمیٹنگ
    /// </summary>
    public static class CurrencyConverter
    {
        // ─── Default Rate | ڈیفالٹ شرح ──────────────────────────────
        /// <summary>
        /// Default exchange rate 1 AED = 77 PKR | ڈیفالٹ شرح
        /// Updated at runtime from AppSettings
        /// رن ٹائم پر AppSettings سے اپ ڈیٹ ہوتی ہے
        /// </summary>
        public static decimal ExchangeRate { get; set; } = 77m;

        // ─── AED to PKR | AED سے PKR ─────────────────────────────────
        /// <summary>
        /// Convert AED amount to PKR | AED رقم کو PKR میں تبدیل کریں
        /// </summary>
        public static decimal AedToPkr(decimal aedAmount)
            => Math.Round(aedAmount * ExchangeRate, 2);

        // ─── PKR to AED | PKR سے AED ─────────────────────────────────
        /// <summary>
        /// Convert PKR amount to AED | PKR رقم کو AED میں تبدیل کریں
        /// </summary>
        public static decimal PkrToAed(decimal pkrAmount)
        {
            if (ExchangeRate == 0) return 0;
            return Math.Round(pkrAmount / ExchangeRate, 2);
        }

        // ─── Format AED | AED فارمیٹ ─────────────────────────────────
        /// <summary>
        /// Format amount as AED display string
        /// رقم کو AED ڈسپلے سٹرنگ کے طور پر فارمیٹ کریں
        /// </summary>
        /// <example>FormatAed(5000) → "5,000 AED"</example>
        public static string FormatAed(decimal amount)
            => $"{amount:N0} PKR";

        // ─── Format PKR | PKR فارمیٹ ─────────────────────────────────
        /// <summary>
        /// Format amount as PKR display string
        /// رقم کو PKR ڈسپلے سٹرنگ کے طور پر فارمیٹ کریں
        /// </summary>
        /// <example>FormatPkr(385000) → "3,85,000 PKR"</example>
        public static string FormatPkr(decimal amount)
            => $"{amount:N0} PKR";

        // ─── Dual Display | دو کرنسی ڈسپلے ──────────────────────────
        /// <summary>
        /// Show amount in both AED and PKR on one line
        /// ایک لائن پر AED اور PKR دونوں میں رقم دکھائیں
        /// </summary>
        /// <example>
        /// DualDisplay(5000) → "5,000 AED | 3,85,000 PKR"
        /// </example>
        public static string DualDisplay(decimal aedAmount)
        {
            decimal pkr = AedToPkr(aedAmount);
            return $"{aedAmount:N0} PKR | {pkr:N0} PKR";
        }

        // ─── Dual Display with Urdu | اردو کے ساتھ دو کرنسی ─────────
        /// <summary>
        /// Show amount in AED + PKR with Urdu labels
        /// AED + PKR دونوں میں اردو لیبل کے ساتھ دکھائیں
        /// </summary>
        public static string DualDisplayUrdu(decimal aedAmount)
        {
            decimal pkr = AedToPkr(aedAmount);
            return $"{aedAmount:N0} PKR | {pkr:N0} روپے";
        }

        // ─── Exchange Rate Display | شرح ڈسپلے ───────────────────────
        /// <summary>
        /// Display current exchange rate string
        /// موجودہ تبادلہ شرح سٹرنگ دکھائیں
        /// </summary>
        public static string RateDisplay =>
            $"1 PKR = {ExchangeRate:N0} PKR | تبادلہ شرح";

        // ─── Parse Amount from String | سٹرنگ سے رقم حاصل کریں ──────
        /// <summary>
        /// Safely parse a decimal amount from user input
        /// صارف ان پٹ سے محفوظ طریقے سے رقم حاصل کریں
        /// </summary>
        public static decimal ParseSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;

            // Remove commas and spaces | کامے اور خالی جگہ ہٹائیں
            string cleaned = input.Replace(",", "").Replace(" ", "").Trim();

            return decimal.TryParse(cleaned, out decimal result) ? result : 0;
        }

        // ─── Round to Nearest 50 | قریبی 50 پر گول کریں ────────────
        /// <summary>
        /// Round installment amount to nearest 50 for cleaner numbers
        /// قسط کی رقم کو صاف ہندسوں کے لیے قریبی 50 پر گول کریں
        /// </summary>
        public static decimal RoundToNearest50(decimal amount)
        {
            return Math.Ceiling(amount / 50) * 50;
        }

        // ─── Format with Symbol | علامت کے ساتھ فارمیٹ ──────────────
        /// <summary>
        /// Format with a custom currency symbol
        /// مخصوص کرنسی علامت کے ساتھ فارمیٹ کریں
        /// </summary>
        public static string FormatWithSymbol(decimal amount, string symbol = "PKR")
            => $"{amount:N0} {symbol}";
    }
}