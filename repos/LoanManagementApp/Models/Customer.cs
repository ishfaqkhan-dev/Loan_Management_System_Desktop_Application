using System;
using System.Collections.Generic;

namespace LoanManagementApp.Models
{
    /// <summary>
    /// Customer / قرض دار - Stores complete customer profile
    /// </summary>
    public class Customer
    {
        // ─── Primary Key ───────────────────────────────────────────
        /// <summary>
        /// Unique ID | منفرد شناختی نمبر
        /// </summary>
        public int Id { get; set; }

        // ─── Personal Information | ذاتی معلومات ──────────────────
        /// <summary>
        /// Full Name | پورا نام
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Father Name | والد کا نام
        /// </summary>
        public string FatherName { get; set; } = string.Empty;

        /// <summary>
        /// Emirates ID or CNIC | اماراتی شناختی کارڈ / قومی شناختی کارڈ
        /// </summary>
        public string EmiratesIdOrCNIC { get; set; } = string.Empty;

        /// <summary>
        /// Phone Number 1 | فون نمبر ۱
        /// </summary>
        public string PhoneNumber1 { get; set; } = string.Empty;

        /// <summary>
        /// Phone Number 2 (Optional) | فون نمبر ۲ (اختیاری)
        /// </summary>
        public string PhoneNumber2 { get; set; } = string.Empty;

        /// <summary>
        /// Phone Number 3 (Optional) | فون نمبر ۳ (اختیاری)
        /// </summary>
        public string PhoneNumber3 { get; set; } = string.Empty;

        /// <summary>
        /// Address | پتہ
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// City | شہر
        /// </summary>
        public string City { get; set; } = string.Empty;

        // ─── Account / Ledger Info | کھاتہ کی معلومات ─────────────
        /// <summary>
        /// Account Number (like 6422 in ledger) | کھاتہ نمبر
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Son Of / S.O | والد کا نام (ریکارڈ کے لیے)
        /// </summary>
        public string SonOf { get; set; } = string.Empty;

        // ─── Loan Summary | قرض کا خلاصہ ──────────────────────────
        /// <summary>
        /// Total Loan Amount | کل قرض کی رقم
        /// </summary>
        public decimal TotalLoanAmount { get; set; } = 0;

        /// <summary>
        /// Total Paid Amount | کل ادا شدہ رقم
        /// </summary>
        public decimal TotalPaidAmount { get; set; } = 0;

        /// <summary>
        /// Remaining Balance | باقی رقم
        /// </summary>
        public decimal RemainingBalance { get; set; } = 0;

        // ─── Dates | تاریخیں ────────────────────────────────────────
        /// <summary>
        /// Loan Start Date | قرض شروع ہونے کی تاریخ
        /// </summary>
        public DateTime LoanStartDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Loan End Date / Deadline | قرض ختم ہونے کی آخری تاریخ
        /// </summary>
        public DateTime LoanEndDate { get; set; } = DateTime.Now.AddMonths(12);

        /// <summary>
        /// Record Created Date | ریکارڈ بنانے کی تاریخ
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last Updated Date | آخری تبدیلی کی تاریخ
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ─── Status | حیثیت ─────────────────────────────────────────
        /// <summary>
        /// Is Active Customer | کیا یہ فعال قرض دار ہے
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Notes / Remarks | نوٹس / ریمارکس
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        // ─── Navigation Properties (not stored in DB) ───────────────
        /// <summary>
        /// All Loans of this Customer | اس قرض دار کے تمام قرضے
        /// </summary>
        public List<Loan> Loans { get; set; } = new List<Loan>();

        /// <summary>
        /// All Payments of this Customer | اس قرض دار کی تمام ادائیگیاں
        /// </summary>
        public List<Payment> Payments { get; set; } = new List<Payment>();

        // ─── Display Properties (UI use only) ───────────────────────
        /// <summary>
        /// Display Name for UI | اسکرین پر نام
        /// </summary>
        public string DisplayName => $"{FullName} ({AccountNumber})";

        /// <summary>
        /// Loan Status Text in Urdu | قرض کی حیثیت اردو میں
        /// </summary>
        public string LoanStatusUrdu => RemainingBalance <= 0
            ? "✅ قرض ادا ہو گیا"
            : $"⏳ باقی رقم: {RemainingBalance:N0} PKR";

        /// <summary>
        /// Loan Status Text in English | قرض کی حیثیت انگریزی میں
        /// </summary>
        public string LoanStatusEnglish => RemainingBalance <= 0
            ? "✅ Loan Cleared"
            : $"⏳ Remaining: {RemainingBalance:N0} PKR";

        /// <summary>
        /// Is Loan Overdue | کیا قرض کی مدت گزر گئی
        /// </summary>
        public bool IsOverdue => RemainingBalance > 0 && DateTime.Now > LoanEndDate;

        /// <summary>
        /// Overdue Status in Urdu | مدت گزرنے کی حیثیت اردو میں
        /// </summary>
        public string OverdueStatusUrdu => IsOverdue
            ? $"⚠️ میعاد ختم - {(DateTime.Now - LoanEndDate).Days} دن گزر گئے"
            : "✅ وقت پر";
    }
}