using LoanManagementApp.Data;
using LoanManagementApp.Helpers;
using LoanManagementApp.Models;
using System;
using System.Net;
using System.Net.Mail;

namespace LoanManagementApp.Services
{
    /// <summary>
    /// EmailService / ای میل سروس - Handles SMTP email sending and OTP delivery
    /// </summary>
    public class EmailService
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly UserRepository _userRepo;
        private readonly AppSettingsService _settingsService;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public EmailService()
        {
            _userRepo = new UserRepository();
            _settingsService = new AppSettingsService();
        }

        // ─── Send OTP Email | او ٹی پی ای میل بھیجیں ────────────────
        /// <summary>
        /// Generate OTP, save to DB, and send via email
        /// او ٹی پی بنائیں، ڈیٹابیس میں محفوظ کریں اور ای میل بھیجیں
        /// </summary>
        public (bool Success, string Message) SendOtp(
            int userId,
            string purpose)
        {
            try
            {
                // Get user | صارف حاصل کریں
                var user = _userRepo.GetUserById(userId);
                if (user == null)
                    return (false, "User not found | صارف نہیں ملا");

                if (string.IsNullOrWhiteSpace(user.Email))
                    return (false,
                        "No email address set. Please add email first | " +
                        "ای میل ایڈریس نہیں ہے۔ پہلے ای میل شامل کریں");

                // Get settings | ترتیبات حاصل کریں
                var settings = _settingsService.GetSettings();

                if (!settings.IsEmailConfigured)
                    return (false,
                        "Email is not configured in Settings | " +
                        "ای میل ترتیبات میں سیٹ نہیں ہے");

                // Generate OTP | او ٹی پی بنائیں
                string otp = OtpGenerator.Generate(settings.OtpLength);
                var expiryTime = DateTime.Now.AddMinutes(settings.OtpExpiryMinutes);

                // Save OTP to DB | او ٹی پی ڈیٹابیس میں محفوظ کریں
                _userRepo.SaveOtp(userId, otp, expiryTime, purpose);

                // Send email | ای میل بھیجیں
                string subject = BuildSubject(purpose);
                string body = BuildOtpEmailBody(
                    user.Username, otp, settings.OtpExpiryMinutes, purpose);

                SendEmail(settings, user.Email, subject, body);

                return (true,
                    $"OTP sent to {user.Email} | " +
                    $"او ٹی پی {user.Email} پر بھیجا گیا");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to send OTP | او ٹی پی بھیجنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Verify OTP | او ٹی پی تصدیق کریں ──────────────────────
        /// <summary>
        /// Verify entered OTP against saved OTP
        /// درج کردہ او ٹی پی کو محفوظ او ٹی پی سے تصدیق کریں
        /// </summary>
        public (bool Success, string Message) VerifyOtp(
            int userId,
            string enteredOtp,
            string purpose)
        {
            try
            {
                var user = _userRepo.GetUserById(userId);
                if (user == null)
                    return (false, "User not found | صارف نہیں ملا");

                // Check OTP exists | او ٹی پی موجود ہے یا نہیں
                if (string.IsNullOrEmpty(user.OtpCode))
                    return (false,
                        "No OTP found. Please request a new one | " +
                        "کوئی او ٹی پی نہیں۔ نیا او ٹی پی مانگیں");

                // Check purpose | مقصد جانچیں
                if (user.OtpPurpose != purpose)
                    return (false,
                        "OTP purpose does not match | او ٹی پی کا مقصد میل نہیں کھاتا");

                // Check expiry | میعاد جانچیں
                if (!user.IsOtpValid)
                {
                    _userRepo.ClearOtp(userId);
                    return (false,
                        "OTP has expired. Please request a new one | " +
                        "او ٹی پی ختم ہو گیا۔ نیا او ٹی پی مانگیں");
                }

                // Check code | کوڈ جانچیں
                if (user.OtpCode != enteredOtp.Trim())
                    return (false,
                        "Incorrect OTP code | غلط او ٹی پی کوڈ");

                // Clear OTP after successful verify | تصدیق کے بعد صاف کریں
                _userRepo.ClearOtp(userId);

                return (true, "OTP verified successfully | او ٹی پی کامیابی سے تصدیق ہوا");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"OTP verification failed | او ٹی پی تصدیق ناکام: {ex.Message}");
            }
        }

        // ─── Test Email Connection | ای میل کنکشن جانچیں ────────────
        /// <summary>
        /// Send a test email to verify SMTP settings
        /// SMTP ترتیبات جانچنے کے لیے ٹیسٹ ای میل بھیجیں
        /// </summary>
        public (bool Success, string Message) SendTestEmail(
            AppSettings settings,
            string toEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail))
                    return (false,
                        "Recipient email is required | وصول کنندہ کا ای میل ضروری ہے");

                string subject = "Test Email - Loan Management System | ٹیسٹ ای میل";
                string body = BuildTestEmailBody(settings.AppName);

                SendEmail(settings, toEmail, subject, body);

                return (true,
                    $"Test email sent to {toEmail} | " +
                    $"ٹیسٹ ای میل {toEmail} پر بھیجا گیا");
            }
            catch (Exception ex)
            {
                return (false,
                    $"Failed to send test email | ٹیسٹ ای میل ناکام: {ex.Message}");
            }
        }

        // ─── Update User Email | صارف کا ای میل اپ ڈیٹ کریں ─────────
        /// <summary>
        /// Update user email and send verification OTP
        /// صارف کا ای میل اپ ڈیٹ کریں اور تصدیقی او ٹی پی بھیجیں
        /// </summary>
        public (bool Success, string Message) UpdateAndVerifyEmail(
            int userId,
            string newEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newEmail))
                    return (false,
                        "Email address is required | ای میل ایڈریس ضروری ہے");

                if (!IsValidEmail(newEmail))
                    return (false,
                        "Invalid email format | ای میل فارمیٹ غلط ہے");

                // Save unverified email | غیر تصدیق شدہ ای میل محفوظ کریں
                _userRepo.UpdateEmail(userId, newEmail.Trim(), false);

                // Send OTP for verification | تصدیق کے لیے او ٹی پی بھیجیں
                return SendOtp(userId, OtpPurpose.VerifyEmail);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update email | ای میل اپ ڈیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Confirm Email With OTP | او ٹی پی سے ای میل تصدیق کریں ─
        /// <summary>
        /// Confirm email after OTP verification
        /// او ٹی پی تصدیق کے بعد ای میل تصدیق شدہ کریں
        /// </summary>
        public (bool Success, string Message) ConfirmEmailWithOtp(
            int userId,
            string enteredOtp)
        {
            try
            {
                var result = VerifyOtp(userId, enteredOtp, OtpPurpose.VerifyEmail);

                if (!result.Success)
                    return result;

                // Mark email as verified | ای میل تصدیق شدہ کریں
                _userRepo.MarkEmailVerified(userId);

                return (true,
                    "Email verified successfully | ای میل کامیابی سے تصدیق ہوئی");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Email confirmation failed | ای میل تصدیق ناکام: {ex.Message}");
            }
        }

        // ─── Core Send Email | ای میل بھیجنے کا مرکزی طریقہ ─────────
        /// <summary>
        /// Core SMTP email sending method
        /// SMTP ای میل بھیجنے کا مرکزی طریقہ
        /// </summary>
        private void SendEmail(
            AppSettings settings,
            string toEmail,
            string subject,
            string body)
        {
            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                EnableSsl = settings.EnableSsl,
                Credentials = new NetworkCredential(
                    settings.SmtpUsername,
                    settings.SmtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15000   // 15 seconds | 15 سیکنڈ
            };

            var from = new MailAddress(settings.SenderEmail, settings.SenderName);
            var to = new MailAddress(toEmail);

            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            client.Send(message);
        }

        // ─── Email Body Builders | ای میل باڈی بنانے والے ──────────

        /// <summary>
        /// Build OTP email body | او ٹی پی ای میل باڈی بنائیں
        /// </summary>
        private string BuildOtpEmailBody(
            string username,
            string otp,
            int expiryMinutes,
            string purpose)
        {
            string purposeText = purpose switch
            {
                OtpPurpose.ChangePassword => "Change Password | پاس ورڈ تبدیل",
                OtpPurpose.ChangeUsername => "Change Username | صارف نام تبدیل",
                OtpPurpose.VerifyEmail => "Verify Email | ای میل تصدیق",
                _ => "Security Verification | حفاظتی تصدیق"
            };

            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; direction: ltr;'>
                <div style='max-width: 500px; margin: auto; border: 1px solid #ddd;
                            border-radius: 8px; padding: 30px;'>

                    <h2 style='color: #2c3e50; text-align: center;'>
                        🔐 Loan Management System
                    </h2>
                    <h3 style='color: #7f8c8d; text-align: center;'>
                        قرض مینجمنٹ سسٹم
                    </h3>

                    <hr style='border: 1px solid #ecf0f1;'/>

                    <p>Dear <strong>{username}</strong>,</p>
                    <p>آپ کا <strong>{purposeText}</strong> کا او ٹی پی کوڈ یہ ہے:</p>

                    <div style='text-align: center; margin: 20px 0;'>
                        <span style='font-size: 36px; font-weight: bold;
                                     letter-spacing: 10px; color: #2980b9;
                                     background: #ecf0f1; padding: 10px 20px;
                                     border-radius: 8px;'>
                            {otp}
                        </span>
                    </div>

                    <p style='color: #e74c3c; text-align: center;'>
                        ⏳ This code expires in <strong>{expiryMinutes} minutes</strong><br/>
                        یہ کوڈ <strong>{expiryMinutes} منٹ</strong> میں ختم ہو جائے گا
                    </p>

                    <hr style='border: 1px solid #ecf0f1;'/>

                    <p style='color: #95a5a6; font-size: 12px; text-align: center;'>
                        If you did not request this, please ignore this email.<br/>
                        اگر آپ نے یہ نہیں مانگا تو اس ای میل کو نظرانداز کریں۔
                    </p>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Build test email body | ٹیسٹ ای میل باڈی بنائیں
        /// </summary>
        private string BuildTestEmailBody(string appName)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 500px; margin: auto; border: 1px solid #ddd;
                            border-radius: 8px; padding: 30px; text-align: center;'>
                    <h2 style='color: #27ae60;'>✅ Email Configuration Successful</h2>
                    <h3 style='color: #7f8c8d;'>ای میل ترتیب کامیاب</h3>
                    <p>Your SMTP settings are working correctly for <strong>{appName}</strong>.</p>
                    <p>آپ کی SMTP ترتیبات درست طریقے سے کام کر رہی ہیں۔</p>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Build email subject based on purpose | مقصد کے مطابق موضوع بنائیں
        /// </summary>
        private string BuildSubject(string purpose)
        {
            return purpose switch
            {
                OtpPurpose.ChangePassword =>
                    "OTP: Change Password | پاس ورڈ تبدیل کریں",
                OtpPurpose.ChangeUsername =>
                    "OTP: Change Username | صارف نام تبدیل کریں",
                OtpPurpose.VerifyEmail =>
                    "OTP: Verify Email | ای میل تصدیق کریں",
                _ =>
                    "OTP: Security Verification | حفاظتی تصدیق"
            };
        }

        // ─── Private Helpers ─────────────────────────────────────────

        /// <summary>
        /// Validate email format | ای میل فارمیٹ جانچیں
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }
    }

    // ─── OTP Purpose Constants | او ٹی پی مقصد کی ثابت قدریں ────────
    /// <summary>
    /// OTP Purpose string constants | او ٹی پی مقصد کی ثابت قدریں
    /// </summary>
    public static class OtpPurpose
    {
        /// <summary>Change Password / پاس ورڈ تبدیل</summary>
        public const string ChangePassword = "ChangePassword";

        /// <summary>Change Username / صارف نام تبدیل</summary>
        public const string ChangeUsername = "ChangeUsername";

        /// <summary>Verify Email / ای میل تصدیق</summary>
        public const string VerifyEmail = "VerifyEmail";
    }
}