using System;
using System.Collections.Generic;
using LoanManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace LoanManagementApp.Data
{
    /// <summary>
    /// UserRepository / صارف ریپوزٹری - All CRUD operations for Users
    /// </summary>
    public class UserRepository
    {
        // ─── Get User By ID | شناخت سے صارف حاصل کریں ──────────────
        /// <summary>
        /// Get single user by ID | ایک صارف شناخت سے حاصل کریں
        /// </summary>
        public User? GetUserById(int userId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Users
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", userId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapUser(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get user | صارف حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get User By Username | صارف نام سے صارف حاصل کریں ──────
        /// <summary>
        /// Get user by username | صارف نام کے ذریعے صارف حاصل کریں
        /// </summary>
        public User? GetUserByUsername(string username)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Users
                    WHERE Username = @Username
                      AND IsActive  = 1;";
                cmd.Parameters.AddWithValue("@Username", username);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapUser(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get user by username | صارف نام سے صارف حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get All Users | تمام صارفین حاصل کریں ─────────────────
        /// <summary>
        /// Get all active users | تمام فعال صارفین حاصل کریں
        /// </summary>
        public List<User> GetAllUsers()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Users
                    WHERE IsActive = 1
                    ORDER BY CreatedAt ASC;";

                using var reader = cmd.ExecuteReader();
                var users = new List<User>();

                while (reader.Read())
                    users.Add(MapUser(reader));

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get all users | تمام صارفین حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Password | پاس ورڈ اپ ڈیٹ کریں ─────────────────
        /// <summary>
        /// Update user password hash | صارف کا پاس ورڈ ہیش اپ ڈیٹ کریں
        /// </summary>
        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        PasswordHash = @PasswordHash,
                        UpdatedAt    = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update password | پاس ورڈ اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Username | صارف نام اپ ڈیٹ کریں ────────────────
        /// <summary>
        /// Update username | صارف نام اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateUsername(int userId, string newUsername)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        Username  = @Username,
                        UpdatedAt = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@Username", newUsername);
                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update username | صارف نام اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update PIN | پن کوڈ اپ ڈیٹ کریں ───────────────────────
        /// <summary>
        /// Update user PIN code | صارف کا پن کوڈ اپ ڈیٹ کریں
        /// </summary>
        public bool UpdatePin(int userId, string newPin)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        PinCode   = @PinCode,
                        UpdatedAt = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@PinCode", newPin);
                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update PIN | پن کوڈ اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Security Question | حفاظتی سوال اپ ڈیٹ کریں ────
        /// <summary>
        /// Update security question and answer hash
        /// حفاظتی سوال اور جواب ہیش اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateSecurityQuestion(
            int userId,
            string question,
            string answerHash)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        SecurityQuestion   = @SecurityQuestion,
                        SecurityAnswerHash = @SecurityAnswerHash,
                        UpdatedAt          = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@SecurityQuestion", question);
                cmd.Parameters.AddWithValue("@SecurityAnswerHash", answerHash);
                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update security question | حفاظتی سوال اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Email | ای میل اپ ڈیٹ کریں ─────────────────────
        /// <summary>
        /// Update user email address | صارف کا ای میل ایڈریس اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateEmail(int userId, string email, bool isVerified = false)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        Email           = @Email,
                        IsEmailVerified = @IsEmailVerified,
                        UpdatedAt       = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@IsEmailVerified", isVerified ? 1 : 0);
                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update email | ای میل اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Save OTP | او ٹی پی محفوظ کریں ─────────────────────────
        /// <summary>
        /// Save OTP code with expiry and purpose
        /// او ٹی پی کوڈ مدت اور مقصد کے ساتھ محفوظ کریں
        /// </summary>
        public bool SaveOtp(int userId, string otpCode, DateTime expiryTime, string purpose)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        OtpCode       = @OtpCode,
                        OtpExpiryTime = @OtpExpiryTime,
                        OtpPurpose    = @OtpPurpose,
                        UpdatedAt     = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@OtpCode", otpCode);
                cmd.Parameters.AddWithValue("@OtpExpiryTime",
                    expiryTime.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@OtpPurpose", purpose);
                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to save OTP | او ٹی پی محفوظ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Clear OTP | او ٹی پی صاف کریں ─────────────────────────
        /// <summary>
        /// Clear OTP after use or expiry
        /// استعمال یا میعاد ختم ہونے کے بعد او ٹی پی صاف کریں
        /// </summary>
        public bool ClearOtp(int userId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        OtpCode       = '',
                        OtpExpiryTime = NULL,
                        OtpPurpose    = '',
                        UpdatedAt     = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to clear OTP | او ٹی پی صاف کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Record Login Success | کامیاب لاگ ان ریکارڈ کریں ───────
        /// <summary>
        /// Record successful login, reset failed attempts
        /// کامیاب لاگ ان ریکارڈ کریں، ناکام کوششیں ریسیٹ کریں
        /// </summary>
        public bool RecordLoginSuccess(int userId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        LastLoginDate       = datetime('now'),
                        FailedLoginAttempts = 0,
                        IsLocked            = 0,
                        LockedUntil         = NULL,
                        UpdatedAt           = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to record login | لاگ ان ریکارڈ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Record Failed Login | ناکام لاگ ان ریکارڈ کریں ─────────
        /// <summary>
        /// Increment failed login attempts, lock account if limit reached
        /// ناکام لاگ ان گنتی بڑھائیں، حد پر اکاؤنٹ بند کریں
        /// </summary>
        public bool RecordFailedLogin(int userId, int maxAttempts, int lockDurationMinutes)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                // First increment the counter | پہلے گنتی بڑھائیں
                cmd.CommandText = @"
                    UPDATE Users SET
                        FailedLoginAttempts = FailedLoginAttempts + 1,
                        UpdatedAt           = datetime('now')
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", userId);
                cmd.ExecuteNonQuery();

                // Check if limit reached | حد پہنچی یا نہیں چیک کریں
                cmd.Parameters.Clear();
                cmd.CommandText = @"
                    SELECT FailedLoginAttempts FROM Users
                    WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", userId);

                var attempts = Convert.ToInt32(cmd.ExecuteScalar());

                // Lock account if max attempts reached | حد پر اکاؤنٹ بند کریں
                if (attempts >= maxAttempts)
                {
                    var lockUntil = DateTime.Now.AddMinutes(lockDurationMinutes);

                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                        UPDATE Users SET
                            IsLocked    = 1,
                            LockedUntil = @LockedUntil,
                            UpdatedAt   = datetime('now')
                        WHERE Id = @Id;";

                    cmd.Parameters.AddWithValue("@LockedUntil",
                        lockUntil.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Id", userId);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to record failed login | ناکام لاگ ان ریکارڈ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Unlock Account | اکاؤنٹ کھولیں ─────────────────────────
        /// <summary>
        /// Manually unlock a locked account | بند اکاؤنٹ دستی طور پر کھولیں
        /// </summary>
        public bool UnlockAccount(int userId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        IsLocked            = 0,
                        LockedUntil         = NULL,
                        FailedLoginAttempts = 0,
                        UpdatedAt           = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to unlock account | اکاؤنٹ کھولنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Theme | تھیم اپ ڈیٹ کریں ───────────────────────
        /// <summary>
        /// Update user preferred theme | صارف کا پسندیدہ تھیم اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateTheme(int userId, AppTheme theme)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        PreferredTheme = @PreferredTheme,
                        UpdatedAt      = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@PreferredTheme", (int)theme);
                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update theme | تھیم اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Update Full User | مکمل صارف اپ ڈیٹ کریں ──────────────
        /// <summary>
        /// Update complete user record | مکمل صارف ریکارڈ اپ ڈیٹ کریں
        /// </summary>
        public bool UpdateUser(User user)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                user.UpdatedAt = DateTime.Now;

                cmd.CommandText = @"
                    UPDATE Users SET
                        Username           = @Username,
                        PasswordHash       = @PasswordHash,
                        PinCode            = @PinCode,
                        SecurityQuestion   = @SecurityQuestion,
                        SecurityAnswerHash = @SecurityAnswerHash,
                        Email              = @Email,
                        IsEmailVerified    = @IsEmailVerified,
                        OtpCode            = @OtpCode,
                        OtpExpiryTime      = @OtpExpiryTime,
                        OtpPurpose         = @OtpPurpose,
                        LastLoginDate      = @LastLoginDate,
                        FailedLoginAttempts= @FailedLoginAttempts,
                        IsLocked           = @IsLocked,
                        LockedUntil        = @LockedUntil,
                        Role               = @Role,
                        IsActive           = @IsActive,
                        PreferredTheme     = @PreferredTheme,
                        UpdatedAt          = @UpdatedAt
                    WHERE Id = @Id;";

                BindUserParameters(cmd, user);
                cmd.Parameters.AddWithValue("@Id", user.Id);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to update user | صارف اپ ڈیٹ کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Check Username Exists | صارف نام موجود ہے یا نہیں ──────
        /// <summary>
        /// Check if username already exists | کیا صارف نام پہلے سے موجود ہے
        /// </summary>
        public bool UsernameExists(string username, int excludeUserId = 0)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT COUNT(*) FROM Users
                    WHERE Username = @Username
                      AND Id      != @ExcludeId
                      AND IsActive = 1;";
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@ExcludeId", excludeUserId);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to check username | صارف نام جانچنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Admin User | ایڈمن صارف حاصل کریں ─────────────────
        /// <summary>
        /// Get the default admin user | ڈیفالٹ ایڈمن صارف حاصل کریں
        /// </summary>
        public User? GetAdminUser()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    SELECT * FROM Users
                    WHERE Role     = 1
                      AND IsActive = 1
                    ORDER BY Id ASC
                    LIMIT 1;";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return MapUser(reader);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to get admin user | ایڈمن صارف حاصل کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Mark Email Verified | ای میل تصدیق شدہ کریں ───────────
        /// <summary>
        /// Mark user email as verified | صارف کا ای میل تصدیق شدہ کریں
        /// </summary>
        public bool MarkEmailVerified(int userId)
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        IsEmailVerified = 1,
                        UpdatedAt       = datetime('now')
                    WHERE Id = @Id;";

                cmd.Parameters.AddWithValue("@Id", userId);

                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to verify email | ای میل تصدیق کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Auto Unlock Expired Locks | میعاد ختم بندش کھولیں ──────
        /// <summary>
        /// Auto unlock accounts whose lock period has expired
        /// جن اکاؤنٹس کی بندش کی مدت گزر گئی انہیں خود بخود کھولیں
        /// </summary>
        public int AutoUnlockExpiredAccounts()
        {
            try
            {
                using var connection = DatabaseContext.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                    UPDATE Users SET
                        IsLocked            = 0,
                        LockedUntil         = NULL,
                        FailedLoginAttempts = 0,
                        UpdatedAt           = datetime('now')
                    WHERE IsLocked   = 1
                      AND LockedUntil < datetime('now');";

                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to auto unlock accounts | خودکار اکاؤنٹ کھولنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Private Helper Methods ──────────────────────────────────

        /// <summary>
        /// Bind user parameters to command | کمانڈ میں پیرامیٹر ڈالیں
        /// </summary>
        private void BindUserParameters(SqliteCommand cmd, User user)
        {
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@PinCode", user.PinCode);
            cmd.Parameters.AddWithValue("@SecurityQuestion", user.SecurityQuestion);
            cmd.Parameters.AddWithValue("@SecurityAnswerHash", user.SecurityAnswerHash);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@IsEmailVerified", user.IsEmailVerified ? 1 : 0);
            cmd.Parameters.AddWithValue("@OtpCode", user.OtpCode);
            cmd.Parameters.AddWithValue("@OtpExpiryTime",
                user.OtpExpiryTime.HasValue
                    ? user.OtpExpiryTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@OtpPurpose", user.OtpPurpose);
            cmd.Parameters.AddWithValue("@LastLoginDate",
                user.LastLoginDate.HasValue
                    ? user.LastLoginDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FailedLoginAttempts", user.FailedLoginAttempts);
            cmd.Parameters.AddWithValue("@IsLocked", user.IsLocked ? 1 : 0);
            cmd.Parameters.AddWithValue("@LockedUntil",
                user.LockedUntil.HasValue
                    ? user.LockedUntil.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Role", (int)user.Role);
            cmd.Parameters.AddWithValue("@IsActive", user.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@PreferredTheme", (int)user.PreferredTheme);
            cmd.Parameters.AddWithValue("@CreatedAt",
                user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@UpdatedAt",
                user.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Map database reader to User object | ریڈر سے صارف آبجیکٹ بنائیں
        /// </summary>
        private User MapUser(SqliteDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                PinCode = reader.GetString(reader.GetOrdinal("PinCode")),
                SecurityQuestion = reader.GetString(reader.GetOrdinal("SecurityQuestion")),
                SecurityAnswerHash = reader.GetString(reader.GetOrdinal("SecurityAnswerHash")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                IsEmailVerified = reader.GetInt32(reader.GetOrdinal("IsEmailVerified")) == 1,
                OtpCode = reader.GetString(reader.GetOrdinal("OtpCode")),
                OtpExpiryTime = reader.IsDBNull(reader.GetOrdinal("OtpExpiryTime"))
                                         ? null
                                         : DateTime.Parse(reader.GetString(
                                               reader.GetOrdinal("OtpExpiryTime"))),
                OtpPurpose = reader.GetString(reader.GetOrdinal("OtpPurpose")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate"))
                                         ? null
                                         : DateTime.Parse(reader.GetString(
                                               reader.GetOrdinal("LastLoginDate"))),
                FailedLoginAttempts = reader.GetInt32(reader.GetOrdinal("FailedLoginAttempts")),
                IsLocked = reader.GetInt32(reader.GetOrdinal("IsLocked")) == 1,
                LockedUntil = reader.IsDBNull(reader.GetOrdinal("LockedUntil"))
                                         ? null
                                         : DateTime.Parse(reader.GetString(
                                               reader.GetOrdinal("LockedUntil"))),
                Role = (UserRole)reader.GetInt32(reader.GetOrdinal("Role")),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                PreferredTheme = (AppTheme)reader.GetInt32(reader.GetOrdinal("PreferredTheme")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }
    }
}