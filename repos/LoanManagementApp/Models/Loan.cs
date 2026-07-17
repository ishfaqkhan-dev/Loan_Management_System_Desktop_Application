using System;
using System.Collections.Generic;

namespace LoanManagementApp.Models
{
    /// <summary>
    /// Loan / قرض - Stores complete loan details for a customer
    /// </summary>
    public class Loan
    {
        // ─── Primary Key ───────────────────────────────────────────
        /// <summary>
        /// Unique Loan ID | منفرد قرض نمبر
        /// </summary>
        public int Id { get; set; }

        // ─── Foreign Key ───────────────────────────────────────────
        /// <summary>
        /// Customer ID | قرض دار کا نمبر
        /// </summary>
        public int CustomerId { get; set; }

        // ─── Loan Amount Info | قرض کی رقم کی معلومات ─────────────
        /// <summary>
        /// Total Loan Amount | کل قرض کی رقم
        /// </summary>
        public decimal TotalAmount { get; set; } = 0;

        /// <summary>
        /// Remaining Loan Amount | باقی قرض کی رقم
        /// </summary>
        public decimal RemainingAmount { get; set; } = 0;

        /// <summary>
        /// Total Paid Amount | کل ادا شدہ رقم
        /// </summary>
        public decimal PaidAmount { get; set; } = 0;

        // ─── Installment Info | قسط کی معلومات ────────────────────
        /// <summary>
        /// Total Number of Installments | اقساط کی کل تعداد
        /// </summary>
        public int TotalInstallments { get; set; } = 0;

        /// <summary>
        /// Remaining Installments | باقی اقساط
        /// </summary>
        public int RemainingInstallments { get; set; } = 0;

        /// <summary>
        /// Paid Installments Count | ادا شدہ اقساط کی تعداد
        /// </summary>
        public int PaidInstallments { get; set; } = 0;

        /// <summary>
        /// Per Installment Amount | فی قسط رقم
        /// </summary>
        public decimal InstallmentAmount { get; set; } = 0;

        // ─── Dates | تاریخیں ────────────────────────────────────────
        /// <summary>
        /// Loan Start Date | قرض شروع ہونے کی تاریخ
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Loan End Date / Deadline | قرض ختم ہونے کی آخری تاریخ
        /// </summary>
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(12);

        /// <summary>
        /// Loan Created Date | قرض بنانے کی تاریخ
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last Updated Date | آخری تبدیلی کی تاریخ
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ─── Loan Status | قرض کی حیثیت ────────────────────────────
        /// <summary>
        /// Loan Status | قرض کی حیثیت
        /// Active=فعال, Closed=بند, Overdue=میعاد ختم, Merged=ضم شدہ
        /// </summary>
        public LoanStatus Status { get; set; } = LoanStatus.Active;

        /// <summary>
        /// Loan Number (e.g. Loan #1, Loan #2 for same customer)
        /// قرض نمبر (ایک قرض دار کے متعدد قرضوں کے لیے)
        /// </summary>
        public int LoanNumber { get; set; } = 1;

        /// <summary>
        /// Is this loan merged with another | کیا یہ قرض کسی اور میں ضم ہوا
        /// </summary>
        public bool IsMerged { get; set; } = false;

        /// <summary>
        /// Notes / Remarks | نوٹس / ریمارکس
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        // ─── Navigation Properties (not stored in DB) ───────────────
        /// <summary>
        /// Customer Reference | قرض دار کا حوالہ
        /// </summary>
        public Customer? Customer { get; set; }

        /// <summary>
        /// All Payments for this Loan | اس قرض کی تمام ادائیگیاں
        /// </summary>
        public List<Payment> Payments { get; set; } = new List<Payment>();

        // ─── Display Properties (not stored in DB) ─────────────────────────
        public string DateRange { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Loan # {LoanNumber}";
        }

        // ─── Display / Computed Properties ──────────────────────────
        /// <summary>
        /// Loan Progress Percentage | قرض کی ادائیگی کا فیصد
        /// </summary>
        public double ProgressPercentage =>
            TotalAmount > 0 ? (double)(PaidAmount / TotalAmount) * 100 : 0;

        /// <summary>
        /// Status in Urdu | حیثیت اردو میں
        /// </summary>
        public string StatusUrdu => Status switch
        {
            LoanStatus.Active => "✅ فعال",
            LoanStatus.Closed => "🔒 بند",
            LoanStatus.Overdue => "⚠️ میعاد ختم",
            LoanStatus.Merged => "🔗 ضم شدہ",
            _ => "نامعلوم"
        };

        /// <summary>
        /// Status in English | حیثیت انگریزی میں
        /// </summary>
        public string StatusEnglish => Status switch
        {
            LoanStatus.Active => "✅ Active",
            LoanStatus.Closed => "🔒 Closed",
            LoanStatus.Overdue => "⚠️ Overdue",
            LoanStatus.Merged => "🔗 Merged",
            _ => "Unknown"
        };

        /// <summary>
        /// Is Loan Overdue | کیا قرض کی مدت گزر گئی
        /// </summary>
        public bool IsOverdue =>
            RemainingAmount > 0 && DateTime.Now > EndDate;

        /// <summary>
        /// Days Remaining or Overdue | باقی دن یا گزرے ہوئے دن
        /// </summary>
        public int DaysRemaining =>
            (EndDate - DateTime.Now).Days;

        /// <summary>
        /// Due Status in Urdu | مدت کی حیثیت اردو میں
        /// </summary>
        public string DueStatusUrdu => DaysRemaining > 0
            ? $"📅 {DaysRemaining} دن باقی"
            : $"⚠️ {Math.Abs(DaysRemaining)} دن گزر گئے";

        /// <summary>
        /// Due Status in English | مدت کی حیثیت انگریزی میں
        /// </summary>
        public string DueStatusEnglish => DaysRemaining > 0
            ? $"📅 {DaysRemaining} days remaining"
            : $"⚠️ {Math.Abs(DaysRemaining)} days overdue";

        /// <summary>
        /// Formatted Total Amount | کل رقم فارمیٹ کے ساتھ
        /// </summary>
        public string TotalAmountDisplay =>
            $"{TotalAmount:N0} PKR | کل رقم";

        /// <summary>
        /// Formatted Remaining Amount | باقی رقم فارمیٹ کے ساتھ
        /// </summary>
        public string RemainingAmountDisplay =>
            $"{RemainingAmount:N0} PKR | باقی رقم";
    }

    // ─── Loan Status Enum | قرض کی حیثیت ───────────────────────────
    /// <summary>
    /// Loan Status Options | قرض کی حیثیت کے اختیارات
    /// </summary>
    public enum LoanStatus
    {
        /// <summary>Active / فعال</summary>
        Active = 1,

        /// <summary>Closed / بند</summary>
        Closed = 2,

        /// <summary>Overdue / میعاد ختم</summary>
        Overdue = 3,

        /// <summary>Merged / ضم شدہ</summary>
        Merged = 4
    }
}