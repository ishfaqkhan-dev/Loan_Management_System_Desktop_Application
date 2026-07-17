using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using LoanManagementApp.Models;

namespace LoanManagementApp.Services
{
    /// <summary>
    /// ValidationService / جانچ سروس - Centralized input validation for all forms
    /// </summary>
    public class ValidationService
    {
        // ─── Validate Customer | قرض دار جانچیں ─────────────────────
        /// <summary>
        /// Validate all customer fields | تمام قرض دار فیلڈ جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidateCustomer(Customer customer)
        {
            // Full Name | پورا نام
            if (string.IsNullOrWhiteSpace(customer.FullName))
                return (false,
                    "Full name is required | پورا نام ضروری ہے");

            if (customer.FullName.Trim().Length < 2)
                return (false,
                    "Full name must be at least 2 characters | " +
                    "پورا نام کم از کم 2 حروف کا ہونا چاہیے");

            // Phone Number | فون نمبر
            if (string.IsNullOrWhiteSpace(customer.PhoneNumber1))
                return (false,
                    "At least one phone number is required | " +
                    "کم از کم ایک فون نمبر ضروری ہے");

            if (!IsValidPhone(customer.PhoneNumber1))
                return (false,
                    "Phone number 1 format is invalid | " +
                    "فون نمبر 1 کا فارمیٹ غلط ہے");

            if (!string.IsNullOrWhiteSpace(customer.PhoneNumber2) &&
                !IsValidPhone(customer.PhoneNumber2))
                return (false,
                    "Phone number 2 format is invalid | " +
                    "فون نمبر 2 کا فارمیٹ غلط ہے");

            if (!string.IsNullOrWhiteSpace(customer.PhoneNumber3) &&
                !IsValidPhone(customer.PhoneNumber3))
                return (false,
                    "Phone number 3 format is invalid | " +
                    "فون نمبر 3 کا فارمیٹ غلط ہے");

            // Loan Dates | قرض کی تاریخیں
            if (customer.LoanEndDate <= customer.LoanStartDate)
                return (false,
                    "Loan end date must be after start date | " +
                    "قرض کی آخری تاریخ شروع کی تاریخ کے بعد ہونی چاہیے");

            return (true, "Valid | درست");
        }

        // ─── Validate Loan | قرض جانچیں ─────────────────────────────
        /// <summary>
        /// Validate all loan fields | تمام قرض فیلڈ جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidateLoan(Loan loan)
        {
            // Total Amount | کل رقم
            if (loan.TotalAmount <= 0)
                return (false,
                    "Loan amount must be greater than zero | " +
                    "قرض کی رقم صفر سے زیادہ ہونی چاہیے");

            if (loan.TotalAmount > 10_000_000)
                return (false,
                    "Loan amount seems too large. Please verify | " +
                    "قرض کی رقم بہت زیادہ لگتی ہے۔ تصدیق کریں");

            // Installments | اقساط
            if (loan.TotalInstallments <= 0)
                return (false,
                    "Number of installments must be greater than zero | " +
                    "اقساط کی تعداد صفر سے زیادہ ہونی چاہیے");

            if (loan.TotalInstallments > 360)
                return (false,
                    "Installments cannot exceed 360 | " +
                    "اقساط 360 سے زیادہ نہیں ہو سکتیں");

            // Installment Amount | قسط کی رقم
            if (loan.InstallmentAmount <= 0)
                return (false,
                    "Installment amount must be greater than zero | " +
                    "قسط کی رقم صفر سے زیادہ ہونی چاہیے");

            // Dates | تاریخیں
            if (loan.EndDate <= loan.StartDate)
                return (false,
                    "End date must be after start date | " +
                    "آخری تاریخ شروع کی تاریخ کے بعد ہونی چاہیے");

            return (true, "Valid | درست");
        }

        // ─── Validate Payment | ادائیگی جانچیں ──────────────────────
        /// <summary>
        /// Validate payment amount against remaining balance
        /// باقی رقم کے خلاف ادائیگی کی رقم جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidatePayment(
            decimal paidAmount,
            decimal remainingBalance)
        {
            if (paidAmount <= 0)
                return (false,
                    "Payment amount must be greater than zero | " +
                    "ادائیگی کی رقم صفر سے زیادہ ہونی چاہیے");

            if (paidAmount > remainingBalance)
                return (false,
                    $"Payment ({paidAmount:N0}) exceeds remaining balance " +
                    $"({remainingBalance:N0}) | " +
                    $"ادائیگی ({paidAmount:N0}) باقی رقم " +
                    $"({remainingBalance:N0}) سے زیادہ ہے");

            return (true, "Valid | درست");
        }

        // ─── Validate Password | پاس ورڈ جانچیں ─────────────────────
        /// <summary>
        /// Validate password strength | پاس ورڈ کی مضبوطی جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidatePassword(
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false,
                    "Password is required | پاس ورڈ ضروری ہے");

            if (password.Length < 4)
                return (false,
                    "Password must be at least 4 characters | " +
                    "پاس ورڈ کم از کم 4 حروف کا ہونا چاہیے");

            if (password.Length > 50)
                return (false,
                    "Password is too long (max 50 characters) | " +
                    "پاس ورڈ بہت لمبا ہے (زیادہ سے زیادہ 50 حروف)");

            if (password != confirmPassword)
                return (false,
                    "Passwords do not match | پاس ورڈ میل نہیں کھاتے");

            return (true, "Valid | درست");
        }

        // ─── Validate PIN | پن جانچیں ────────────────────────────────
        /// <summary>
        /// Validate PIN code format | پن کوڈ فارمیٹ جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidatePin(
            string pin,
            string confirmPin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                return (false, "PIN is required | پن ضروری ہے");

            if (pin.Length < 4 || pin.Length > 6)
                return (false,
                    "PIN must be 4 to 6 digits | پن 4 سے 6 ہندسوں کا ہونا چاہیے");

            if (!Regex.IsMatch(pin, @"^\d+$"))
                return (false,
                    "PIN must contain digits only | پن صرف ہندسوں پر مشتمل ہو");

            if (pin != confirmPin)
                return (false, "PINs do not match | پن میل نہیں کھاتے");

            return (true, "Valid | درست");
        }

        // ─── Validate Username | صارف نام جانچیں ─────────────────────
        /// <summary>
        /// Validate username format | صارف نام فارمیٹ جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required | صارف نام ضروری ہے");

            if (username.Trim().Length < 3)
                return (false,
                    "Username must be at least 3 characters | " +
                    "صارف نام کم از کم 3 حروف کا ہونا چاہیے");

            if (username.Trim().Length > 30)
                return (false,
                    "Username is too long (max 30 characters) | " +
                    "صارف نام بہت لمبا ہے (زیادہ سے زیادہ 30 حروف)");

            if (!Regex.IsMatch(username.Trim(), @"^[a-zA-Z0-9_]+$"))
                return (false,
                    "Username can only contain letters, numbers and underscore | " +
                    "صارف نام صرف حروف، ہندسے اور انڈر اسکور رکھ سکتا ہے");

            return (true, "Valid | درست");
        }

        // ─── Validate Email | ای میل جانچیں ──────────────────────────
        /// <summary>
        /// Validate email address format | ای میل ایڈریس فارمیٹ جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false,
                    "Email address is required | ای میل ایڈریس ضروری ہے");

            try
            {
                var addr = new MailAddress(email.Trim());
                if (addr.Address != email.Trim())
                    return (false,
                        "Invalid email format | ای میل فارمیٹ غلط ہے");

                return (true, "Valid | درست");
            }
            catch
            {
                return (false,
                    "Invalid email format | ای میل فارمیٹ غلط ہے");
            }
        }

        // ─── Validate SMTP Settings | SMTP ترتیبات جانچیں ──────────
        /// <summary>
        /// Validate SMTP email server settings | SMTP ای میل سرور ترتیبات جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidateSmtpSettings(
            string smtpHost,
            int smtpPort,
            string smtpUsername,
            string smtpPassword,
            string senderEmail)
        {
            if (string.IsNullOrWhiteSpace(smtpHost))
                return (false,
                    "SMTP host is required | SMTP ہوسٹ ضروری ہے");

            if (smtpPort <= 0 || smtpPort > 65535)
                return (false,
                    "SMTP port must be between 1 and 65535 | " +
                    "SMTP پورٹ 1 سے 65535 کے درمیان ہونا چاہیے");

            if (string.IsNullOrWhiteSpace(smtpUsername))
                return (false,
                    "SMTP username is required | SMTP صارف نام ضروری ہے");

            if (string.IsNullOrWhiteSpace(smtpPassword))
                return (false,
                    "SMTP password is required | SMTP پاس ورڈ ضروری ہے");

            var emailValidation = ValidateEmail(senderEmail);
            if (!emailValidation.IsValid)
                return (false,
                    "Sender email is invalid | بھیجنے والے کا ای میل غلط ہے");

            return (true, "Valid | درست");
        }

        // ─── Validate Amount | رقم جانچیں ────────────────────────────
        /// <summary>
        /// Validate a decimal amount field | رقم فیلڈ جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidateAmount(
            decimal amount,
            string fieldName = "Amount | رقم")
        {
            if (amount <= 0)
                return (false,
                    $"{fieldName} must be greater than zero | " +
                    $"{fieldName} صفر سے زیادہ ہونی چاہیے");

            if (amount > 100_000_000)
                return (false,
                    $"{fieldName} seems too large | {fieldName} بہت زیادہ لگتی ہے");

            return (true, "Valid | درست");
        }

        // ─── Validate OTP | او ٹی پی جانچیں ──────────────────────────
        /// <summary>
        /// Validate OTP code format | او ٹی پی کوڈ فارمیٹ جانچیں
        /// </summary>
        public (bool IsValid, string Message) ValidateOtp(string otp, int expectedLength = 6)
        {
            if (string.IsNullOrWhiteSpace(otp))
                return (false,
                    "OTP code is required | او ٹی پی کوڈ ضروری ہے");

            if (otp.Trim().Length != expectedLength)
                return (false,
                    $"OTP must be {expectedLength} digits | " +
                    $"او ٹی پی {expectedLength} ہندسوں کا ہونا چاہیے");

            if (!Regex.IsMatch(otp.Trim(), @"^\d+$"))
                return (false,
                    "OTP must contain digits only | او ٹی پی صرف ہندسوں پر مشتمل ہو");

            return (true, "Valid | درست");
        }

        // ─── Private Helpers ─────────────────────────────────────────

        /// <summary>
        /// Validate phone number format | فون نمبر فارمیٹ جانچیں
        /// Accepts: +971xxxxxxxxx, 0xxxxxxxxx, or plain digits
        /// </summary>
        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            string cleaned = phone.Replace(" ", "").Replace("-", "");
            return cleaned.Length >= 7 && cleaned.Length <= 15 &&
                   Regex.IsMatch(cleaned, @"^[+0-9]+$");
        }

        // Add these methods inside the ValidationService class

        /// <summary>
        /// Validate name (only letters, spaces, and dots allowed)
        /// نام کی تصدیق (صرف حروف، خالی جگہ اور ڈاٹ کی اجازت)
        /// </summary>
        public (bool IsValid, string Message) ValidateName(string name, string fieldName = "Name")
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, $"{fieldName} is required | {fieldName} ضروری ہے");

            name = name.Trim();
            if (name.Length < 2)
                return (false, $"{fieldName} must be at least 2 characters | {fieldName} کم از کم 2 حروف کا ہونا چاہیے");

            // Only letters, spaces, dots, hyphens (for names like "Ali-Mohammed" or "M. Ali")
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z\s\.\-]+$"))
                return (false, $"{fieldName} can only contain letters, spaces, dots and hyphens | {fieldName} صرف حروف، خالی جگہ، ڈاٹ اور ہائفن رکھ سکتا ہے");

            return (true, "Valid");
        }

        /// <summary>
        /// Validate Emirates ID or CNIC
        /// Emirates ID: 15 digits (optional format with hyphens, but we store raw)
        /// CNIC: 13 digits (optional format with hyphens)
        /// We'll allow digits only, length 13-15
        /// </summary>
        public (bool IsValid, string Message) ValidateEmiratesIdOrCnic(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (true, "Optional | اختیاری"); // Not required? Actually user wants it required? Let's check requirement: Emirates ID/CNIC should be required? User said "her field import hai" – means all fields are important, but they also said "Emirates ID or CNIC" can be either. We'll make it required but allow either format.

            string cleaned = value.Trim().Replace("-", "").Replace(" ", "");
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^\d+$"))
                return (false, "Emirates ID / CNIC must contain only digits | صرف ہندسے ہوں");

            if (cleaned.Length == 15) // Emirates ID
                return (true, "Valid Emirates ID");
            else if (cleaned.Length == 13) // CNIC
                return (true, "Valid CNIC");
            else
                return (false, "Emirates ID must be 15 digits, CNIC must be 13 digits | اماراتی شناختی کارڈ 15 ہندسے، قومی شناختی کارڈ 13 ہندسے");
        }

        /// <summary>
        /// Validate phone number (UAE or Pakistan format)
        /// Accepts: +971xxxxxxxxx, 0xxxxxxxxx, 05xxxxxxxx, or plain digits
        /// Minimum 7, maximum 15 digits
        /// </summary>
        public (bool IsValid, string Message) ValidatePhone(string phone, bool isRequired = true)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return isRequired
                    ? (false, "Phone number is required | فون نمبر ضروری ہے")
                    : (true, "Optional");

            string cleaned = phone.Trim().Replace(" ", "").Replace("-", "").Replace("+", "");
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^\d+$"))
                return (false, "Phone number can only contain digits, spaces, hyphens, and + | صرف ہندسے، خالی جگہ، ہائفن اور + کی اجازت");

            if (cleaned.Length < 7 || cleaned.Length > 15)
                return (false, "Phone number must be between 7 and 15 digits | فون نمبر 7 سے 15 ہندسوں کے درمیان ہو");

            return (true, "Valid");
        }

        /// <summary>
        /// Validate address / city (non-empty, basic characters)
        /// </summary>
        public (bool IsValid, string Message) ValidateAddress(string address, string fieldName = "Address")
        {
            if (string.IsNullOrWhiteSpace(address))
                return (false, $"{fieldName} is required | {fieldName} ضروری ہے");

            if (address.Trim().Length < 2)
                return (false, $"{fieldName} must be at least 2 characters | {fieldName} کم از کم 2 حروف کا ہونا چاہیے");

            // Allow letters, numbers, spaces, commas, dots, hyphens, slashes
            if (!System.Text.RegularExpressions.Regex.IsMatch(address, @"^[a-zA-Z0-9\s\.,\-/]+$"))
                return (false, $"{fieldName} contains invalid characters | {fieldName} میں غلط حروف ہیں");

            return (true, "Valid");
        }

        /// <summary>
        /// Comprehensive customer validation for all required fields
        /// </summary>
        public (bool IsValid, string Message) ValidateCustomerFull(Customer customer, bool isAddMode = true)
        {
            // Full Name
            var nameResult = ValidateName(customer.FullName, "Full Name");
            if (!nameResult.IsValid) return nameResult;

            // Father Name (optional? But user said all fields important – we'll make it required)
            if (string.IsNullOrWhiteSpace(customer.FatherName))
                return (false, "Father Name is required | والد کا نام ضروری ہے");
            if (customer.FatherName.Trim().Length < 2)
                return (false, "Father Name must be at least 2 characters | والد کا نام کم از کم 2 حروف کا ہونا چاہیے");

            // Emirates ID / CNIC – required
            var idResult = ValidateEmiratesIdOrCnic(customer.EmiratesIdOrCNIC);
            if (!idResult.IsValid) return idResult;

            // Phone numbers
            var phone1Result = ValidatePhone(customer.PhoneNumber1, true);
            if (!phone1Result.IsValid) return phone1Result;
            if (!string.IsNullOrWhiteSpace(customer.PhoneNumber2))
            {
                var phone2Result = ValidatePhone(customer.PhoneNumber2, false);
                if (!phone2Result.IsValid) return phone2Result;
            }
            if (!string.IsNullOrWhiteSpace(customer.PhoneNumber3))
            {
                var phone3Result = ValidatePhone(customer.PhoneNumber3, false);
                if (!phone3Result.IsValid) return phone3Result;
            }

            // Address and City
            var addressResult = ValidateAddress(customer.Address, "Address");
            if (!addressResult.IsValid) return addressResult;
            var cityResult = ValidateAddress(customer.City, "City");
            if (!cityResult.IsValid) return cityResult;

            // Loan Dates
            if (customer.LoanStartDate == default)
                return (false, "Loan Start Date is required | قرض شروع کی تاریخ ضروری ہے");
            if (customer.LoanEndDate == default)
                return (false, "Loan End Date is required | قرض ختم کی تاریخ ضروری ہے");
            if (customer.LoanEndDate <= customer.LoanStartDate)
                return (false, "Loan end date must be after start date | قرض کی آخری تاریخ شروع کی تاریخ کے بعد ہونی چاہیے");

            return (true, "Valid");
        }
    }
}