using System;
using System.IO;
using LoanManagementApp.Data;
using LoanManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace LoanManagementApp.Services
{
    /// <summary>
    /// AppSettingsService / ترتیبات سروس - Manages all application settings
    /// </summary>
    public class AppSettingsService
    {
        // ─── Get Settings | ترتیبات حاصل کریں ───────────────────────
        /// <summary>
        /// Get current application settings from database
        /// ڈیٹابیس سے موجودہ ایپلیکیشن ترتیبات حاصل کریں
        /// </summary>
        public AppSettings GetSettings()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "SELECT * FROM AppSettings LIMIT 1;";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapSettings(reader);

                // Return default if none found | نہ ملے تو ڈیفالٹ واپس کریں
                return new AppSettings();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get settings | ترتیبات حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Settings | ترتیبات اپ ڈیٹ کریں ─────────────────
        /// <summary>
        /// Update all settings in database | ڈیٹابیس میں تمام ترتیبات اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateSettings(AppSettings settings)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                settings.UpdatedAt = DateTime.Now;

                cmd.CommandText = @"
                    UPDATE AppSettings SET
                        CurrentTheme                  = @CurrentTheme,
                        ThemeLastChanged              = @ThemeLastChanged,
                        DefaultCurrency               = @DefaultCurrency,
                        CurrencySymbol                = @CurrencySymbol,
                        SecondaryCurrency             = @SecondaryCurrency,
                        ExchangeRate                  = @ExchangeRate,
                        ExchangeRateLastUpdated       = @ExchangeRateLastUpdated,
                        SmtpHost                      = @SmtpHost,
                        SmtpPort                      = @SmtpPort,
                        SmtpUsername                  = @SmtpUsername,
                        SmtpPassword                  = @SmtpPassword,
                        SenderEmail                   = @SenderEmail,
                        SenderName                    = @SenderName,
                        IsEmailConfigured             = @IsEmailConfigured,
                        EnableSsl                     = @EnableSsl,
                        BackupFolderPath              = @BackupFolderPath,
                        AutoBackupEnabled             = @AutoBackupEnabled,
                        AutoBackupIntervalHours       = @AutoBackupIntervalHours,
                        LastBackupDate                = @LastBackupDate,
                        LastBackupFilePath            = @LastBackupFilePath,
                        MaxBackupFilesToKeep          = @MaxBackupFilesToKeep,
                        AppName                       = @AppName,
                        AppNameUrdu                   = @AppNameUrdu,
                        CompanyName                   = @CompanyName,
                        CompanyNameUrdu               = @CompanyNameUrdu,
                        CompanyPhone                  = @CompanyPhone,
                        CompanyAddress                = @CompanyAddress,
                        OtpExpiryMinutes              = @OtpExpiryMinutes,
                        OtpLength                     = @OtpLength,
                        MaxFailedLoginAttempts        = @MaxFailedLoginAttempts,
                        AccountLockDurationMinutes    = @AccountLockDurationMinutes,
                        RequirePinForSensitiveActions = @RequirePinForSensitiveActions,
                        DateFormat                    = @DateFormat,
                        TimeFormat                    = @TimeFormat,
                        UpdatedAt                     = @UpdatedAt
                    WHERE Id = @Id;";

                BindSettingsParameters(cmd, settings);
                cmd.Parameters.AddWithValue("@Id", settings.Id);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update settings | ترتیبات اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Theme | تھیم اپ ڈیٹ کریں ───────────────────────
        /// <summary>
        /// Update only the theme setting | صرف تھیم ترتیب اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateTheme(AppTheme theme)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE AppSettings SET
                        CurrentTheme     = @CurrentTheme,
                        ThemeLastChanged = datetime('now'),
                        UpdatedAt        = datetime('now');";

                cmd.Parameters.AddWithValue("@CurrentTheme", (int)theme);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update theme | تھیم اپ ڈیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Update Email Config | ای میل ترتیب اپ ڈیٹ ─────────────
        /// <summary>
        /// Update SMTP email configuration | SMTP ای میل ترتیب اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateEmailConfig(
            string smtpHost,
            int smtpPort,
            string smtpUsername,
            string smtpPassword,
            string senderEmail,
            string senderName,
            bool enableSsl)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE AppSettings SET
                        SmtpHost          = @SmtpHost,
                        SmtpPort          = @SmtpPort,
                        SmtpUsername      = @SmtpUsername,
                        SmtpPassword      = @SmtpPassword,
                        SenderEmail       = @SenderEmail,
                        SenderName        = @SenderName,
                        EnableSsl         = @EnableSsl,
                        IsEmailConfigured = @IsEmailConfigured,
                        UpdatedAt         = datetime('now');";

                cmd.Parameters.AddWithValue("@SmtpHost", smtpHost);
                cmd.Parameters.AddWithValue("@SmtpPort", smtpPort);
                cmd.Parameters.AddWithValue("@SmtpUsername", smtpUsername);
                cmd.Parameters.AddWithValue("@SmtpPassword", smtpPassword);
                cmd.Parameters.AddWithValue("@SenderEmail", senderEmail);
                cmd.Parameters.AddWithValue("@SenderName", senderName);
                cmd.Parameters.AddWithValue("@EnableSsl", enableSsl ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsEmailConfigured",
                    !string.IsNullOrWhiteSpace(smtpUsername) ? 1 : 0);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update email config | ای میل ترتیب اپ ڈیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Update Exchange Rate | تبادلہ شرح اپ ڈیٹ کریں ──────────
        /// <summary>
        /// Update currency exchange rate | کرنسی تبادلہ شرح اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateExchangeRate(decimal rate)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE AppSettings SET
                        ExchangeRate            = @ExchangeRate,
                        ExchangeRateLastUpdated = datetime('now'),
                        UpdatedAt               = datetime('now');";

                cmd.Parameters.AddWithValue("@ExchangeRate", rate);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update exchange rate | تبادلہ شرح اپ ڈیٹ ناکام: {ex.Message}");
            }
        }

        // ─── Private Helpers ─────────────────────────────────────────

        /// <summary>
        /// Bind settings parameters to command | کمانڈ میں پیرامیٹر ڈالیں
        /// </summary>
        private void BindSettingsParameters(SqliteCommand cmd, AppSettings s)
        {
            cmd.Parameters.AddWithValue("@CurrentTheme",
                (int)s.CurrentTheme);
            cmd.Parameters.AddWithValue("@ThemeLastChanged",
                s.ThemeLastChanged.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@DefaultCurrency", s.DefaultCurrency);
            cmd.Parameters.AddWithValue("@CurrencySymbol", s.CurrencySymbol);
            cmd.Parameters.AddWithValue("@SecondaryCurrency", s.SecondaryCurrency);
            cmd.Parameters.AddWithValue("@ExchangeRate", s.ExchangeRate);
            cmd.Parameters.AddWithValue("@ExchangeRateLastUpdated",
                s.ExchangeRateLastUpdated.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@SmtpHost", s.SmtpHost);
            cmd.Parameters.AddWithValue("@SmtpPort", s.SmtpPort);
            cmd.Parameters.AddWithValue("@SmtpUsername", s.SmtpUsername);
            cmd.Parameters.AddWithValue("@SmtpPassword", s.SmtpPassword);
            cmd.Parameters.AddWithValue("@SenderEmail", s.SenderEmail);
            cmd.Parameters.AddWithValue("@SenderName", s.SenderName);
            cmd.Parameters.AddWithValue("@IsEmailConfigured", s.IsEmailConfigured ? 1 : 0);
            cmd.Parameters.AddWithValue("@EnableSsl", s.EnableSsl ? 1 : 0);
            cmd.Parameters.AddWithValue("@BackupFolderPath", s.BackupFolderPath);
            cmd.Parameters.AddWithValue("@AutoBackupEnabled", s.AutoBackupEnabled ? 1 : 0);
            cmd.Parameters.AddWithValue("@AutoBackupIntervalHours", s.AutoBackupIntervalHours);
            cmd.Parameters.AddWithValue("@LastBackupDate",
                s.LastBackupDate.HasValue
                    ? s.LastBackupDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LastBackupFilePath", s.LastBackupFilePath);
            cmd.Parameters.AddWithValue("@MaxBackupFilesToKeep", s.MaxBackupFilesToKeep);
            cmd.Parameters.AddWithValue("@AppName", s.AppName);
            cmd.Parameters.AddWithValue("@AppNameUrdu", s.AppNameUrdu);
            cmd.Parameters.AddWithValue("@CompanyName", s.CompanyName);
            cmd.Parameters.AddWithValue("@CompanyNameUrdu", s.CompanyNameUrdu);
            cmd.Parameters.AddWithValue("@CompanyPhone", s.CompanyPhone);
            cmd.Parameters.AddWithValue("@CompanyAddress", s.CompanyAddress);
            cmd.Parameters.AddWithValue("@OtpExpiryMinutes", s.OtpExpiryMinutes);
            cmd.Parameters.AddWithValue("@OtpLength", s.OtpLength);
            cmd.Parameters.AddWithValue("@MaxFailedLoginAttempts", s.MaxFailedLoginAttempts);
            cmd.Parameters.AddWithValue("@AccountLockDurationMinutes", s.AccountLockDurationMinutes);
            cmd.Parameters.AddWithValue("@RequirePinForSensitiveActions",
                s.RequirePinForSensitiveActions ? 1 : 0);
            cmd.Parameters.AddWithValue("@DateFormat", s.DateFormat);
            cmd.Parameters.AddWithValue("@TimeFormat", s.TimeFormat);
            cmd.Parameters.AddWithValue("@UpdatedAt",
                s.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Map database reader to AppSettings object
        /// ریڈر سے ترتیبات آبجیکٹ بنائیں
        /// </summary>
        private AppSettings MapSettings(SqliteDataReader reader)
        {
            return new AppSettings
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CurrentTheme = (AppTheme)reader.GetInt32(
                                                   reader.GetOrdinal("CurrentTheme")),
                ThemeLastChanged = DateTime.Parse(
                                                   reader.GetString(
                                                       reader.GetOrdinal("ThemeLastChanged"))),
                DefaultCurrency = reader.GetString(reader.GetOrdinal("DefaultCurrency")),
                CurrencySymbol = reader.GetString(reader.GetOrdinal("CurrencySymbol")),
                SecondaryCurrency = reader.GetString(reader.GetOrdinal("SecondaryCurrency")),
                ExchangeRate = reader.GetDecimal(reader.GetOrdinal("ExchangeRate")),
                ExchangeRateLastUpdated = DateTime.Parse(
                                                   reader.GetString(
                                                       reader.GetOrdinal("ExchangeRateLastUpdated"))),
                SmtpHost = reader.GetString(reader.GetOrdinal("SmtpHost")),
                SmtpPort = reader.GetInt32(reader.GetOrdinal("SmtpPort")),
                SmtpUsername = reader.GetString(reader.GetOrdinal("SmtpUsername")),
                SmtpPassword = reader.GetString(reader.GetOrdinal("SmtpPassword")),
                SenderEmail = reader.GetString(reader.GetOrdinal("SenderEmail")),
                SenderName = reader.GetString(reader.GetOrdinal("SenderName")),
                IsEmailConfigured = reader.GetInt32(
                                                   reader.GetOrdinal("IsEmailConfigured")) == 1,
                EnableSsl = reader.GetInt32(
                                                   reader.GetOrdinal("EnableSsl")) == 1,
                BackupFolderPath = reader.GetString(reader.GetOrdinal("BackupFolderPath")),
                AutoBackupEnabled = reader.GetInt32(
                                                   reader.GetOrdinal("AutoBackupEnabled")) == 1,
                AutoBackupIntervalHours = reader.GetInt32(
                                                   reader.GetOrdinal("AutoBackupIntervalHours")),
                LastBackupDate = reader.IsDBNull(reader.GetOrdinal("LastBackupDate"))
                                                   ? null
                                                   : DateTime.Parse(reader.GetString(
                                                       reader.GetOrdinal("LastBackupDate"))),
                LastBackupFilePath = reader.GetString(reader.GetOrdinal("LastBackupFilePath")),
                MaxBackupFilesToKeep = reader.GetInt32(
                                                   reader.GetOrdinal("MaxBackupFilesToKeep")),
                AppName = reader.GetString(reader.GetOrdinal("AppName")),
                AppNameUrdu = reader.GetString(reader.GetOrdinal("AppNameUrdu")),
                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                CompanyNameUrdu = reader.GetString(reader.GetOrdinal("CompanyNameUrdu")),
                CompanyPhone = reader.GetString(reader.GetOrdinal("CompanyPhone")),
                CompanyAddress = reader.GetString(reader.GetOrdinal("CompanyAddress")),
                OtpExpiryMinutes = reader.GetInt32(reader.GetOrdinal("OtpExpiryMinutes")),
                OtpLength = reader.GetInt32(reader.GetOrdinal("OtpLength")),
                MaxFailedLoginAttempts = reader.GetInt32(
                                                   reader.GetOrdinal("MaxFailedLoginAttempts")),
                AccountLockDurationMinutes = reader.GetInt32(
                                                   reader.GetOrdinal("AccountLockDurationMinutes")),
                RequirePinForSensitiveActions = reader.GetInt32(
                                                   reader.GetOrdinal("RequirePinForSensitiveActions")) == 1,
                DateFormat = reader.GetString(reader.GetOrdinal("DateFormat")),
                TimeFormat = reader.GetString(reader.GetOrdinal("TimeFormat")),
                UpdatedAt = DateTime.Parse(
                                                   reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }
    }
}