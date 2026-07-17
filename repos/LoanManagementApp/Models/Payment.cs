using System;

namespace LoanManagementApp.Models
{
    /// <summary>
    /// Payment / ادائیگی - Stores complete payment record for each installment
    /// </summary>
    public class Payment
    {
        // ─── Primary Key ───────────────────────────────────────────
        /// <summary>
        /// Unique Payment ID | منفرد ادائیگی نمبر
        /// </summary>
        public int Id { get; set; }

        // ─── Foreign Keys ──────────────────────────────────────────
        /// <summary>
        /// Loan ID | قرض نمبر
        /// </summary>
        public int LoanId { get; set; }

        /// <summary>
        /// Customer ID | قرض دار کا نمبر
        /// </summary>
        public int CustomerId { get; set; }

        // ─── Payment Amount Info | ادائیگی کی رقم کی معلومات ──────
        /// <summary>
        /// Paid Amount | ادا کی گئی رقم
        /// </summary>
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Remaining Balance After Payment | ادائیگی کے بعد باقی رقم
        /// </summary>
        public decimal RemainingBalanceAfterPayment { get; set; } = 0;

        /// <summary>
        /// Balance Before Payment | ادائیگی سے پہلے کی رقم
        /// </summary>
        public decimal BalanceBeforePayment { get; set; } = 0;

        // ─── Installment Info | قسط کی معلومات ────────────────────
        /// <summary>
        /// Installment Number (e.g. 1st, 2nd, 3rd) | قسط نمبر
        /// </summary>
        public int InstallmentNumber { get; set; } = 0;

        /// <summary>
        /// Remaining Installments After This Payment | اس ادائیگی کے بعد باقی اقساط
        /// </summary>
        public int RemainingInstallmentsAfterPayment { get; set; } = 0;

        /// <summary>
        /// Total Installments of Loan | قرض کی کل اقساط
        /// </summary>
        public int TotalInstallments { get; set; } = 0;

        // ─── Payment Date & Time | ادائیگی کی تاریخ اور وقت ────────
        /// <summary>
        /// Payment Date | ادائیگی کی تاریخ
        /// </summary>
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Record Created Date | ریکارڈ بنانے کی تاریخ
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ─── Payment Type | ادائیگی کی قسم ────────────────────────
        /// <summary>
        /// Payment Type | ادائیگی کی قسم
        /// Cash=نقد, Bank=بینک, Online=آن لائن
        /// </summary>
        public PaymentType PaymentType { get; set; } = PaymentType.Cash;

        /// <summary>
        /// Payment Method Description | ادائیگی کے طریقے کی تفصیل
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        // ─── Transaction Info | لین دین کی معلومات ─────────────────
        /// <summary>
        /// Transaction / Voucher Number | لین دین / وصولی نمبر
        /// </summary>
        public string VoucherNumber { get; set; } = string.Empty;

        /// <summary>
        /// Received By (Staff Name) | وصول کرنے والے کا نام
        /// </summary>
        public string ReceivedBy { get; set; } = string.Empty;

        /// <summary>
        /// Notes / Remarks | نوٹس / ریمارکس
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Is Payment Verified | کیا ادائیگی تصدیق شدہ ہے
        /// </summary>
        public bool IsVerified { get; set; } = true;

        // ─── Navigation Properties (not stored in DB) ───────────────
        /// <summary>
        /// Loan Reference | قرض کا حوالہ
        /// </summary>
        public Loan? Loan { get; set; }

        /// <summary>
        /// Customer Reference | قرض دار کا حوالہ
        /// </summary>
        public Customer? Customer { get; set; }

        // ─── Display / Computed Properties ──────────────────────────
        /// <summary>
        /// Payment Date Formatted | ادائیگی کی تاریخ فارمیٹ کے ساتھ
        /// </summary>
        public string PaymentDateDisplay =>
            PaymentDate.ToString("dd-MMM-yyyy hh:mm tt");

        /// <summary>
        /// Paid Amount Display | ادا کی گئی رقم فارمیٹ کے ساتھ
        /// </summary>
        public string PaidAmountDisplay =>
            $"{PaidAmount:N0} PKR | ادا شدہ رقم";

        /// <summary>
        /// Remaining Balance Display | باقی رقم فارمیٹ کے ساتھ
        /// </summary>
        public string RemainingBalanceDisplay =>
            $"{RemainingBalanceAfterPayment:N0} PKR | باقی رقم";

        /// <summary>
        /// Installment Progress Display | قسط کی پیشرفت
        /// </summary>
        public string InstallmentProgressDisplay =>
            $"Installment {InstallmentNumber} of {TotalInstallments} | " +
            $"قسط {InstallmentNumber} از {TotalInstallments}";

        /// <summary>
        /// Payment Type in Urdu | ادائیگی کی قسم اردو میں
        /// </summary>
        public string PaymentTypeUrdu => PaymentType switch
        {
            PaymentType.Cash => "💵 نقد",
            PaymentType.Bank => "🏦 بینک",
            PaymentType.Online => "📱 آن لائن",
            PaymentType.Cheque => "📋 چیک",
            _ => "نامعلوم"
        };

        /// <summary>
        /// Payment Type in English | ادائیگی کی قسم انگریزی میں
        /// </summary>
        public string PaymentTypeEnglish => PaymentType switch
        {
            PaymentType.Cash => "💵 Cash",
            PaymentType.Bank => "🏦 Bank Transfer",
            PaymentType.Online => "📱 Online",
            PaymentType.Cheque => "📋 Cheque",
            _ => "Unknown"
        };

        /// <summary>
        /// Short Summary of Payment | ادائیگی کا مختصر خلاصہ
        /// </summary>
        public string PaymentSummary =>
            $"{PaymentDate:dd-MMM-yyyy} | " +
            $"Paid: {PaidAmount:N0} | ادا: {PaidAmount:N0} | " +
            $"Balance: {RemainingBalanceAfterPayment:N0} | باقی: {RemainingBalanceAfterPayment:N0}";
    }

    // ─── Payment Type Enum | ادائیگی کی قسم ────────────────────────
    /// <summary>
    /// Payment Type Options | ادائیگی کی قسم کے اختیارات
    /// </summary>
    public enum PaymentType
    {
        /// <summary>Cash / نقد</summary>
        Cash = 1,

        /// <summary>Bank Transfer / بینک ٹرانسفر</summary>
        Bank = 2,

        /// <summary>Online / آن لائن</summary>
        Online = 3,

        /// <summary>Cheque / چیک</summary>
        Cheque = 4
    }
}