using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace LoanManagementApp.Data
{
    /// <summary>
    /// DatabaseContext / ڈیٹابیس کنکشن - Manages SQLite connection and table creation
    /// </summary>
    public class DatabaseContext
    {
        // ─── Database Path | ڈیٹابیس کا راستہ ─────────────────────
        private static readonly string DbFolderPath =
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
                "LoanManagementApp");

        private static readonly string DbFilePath =
            Path.Combine(DbFolderPath, "LoanManagement.db");

        /// <summary>
        /// Connection String | کنکشن سٹرنگ
        /// </summary>
        public static string ConnectionString =>
            $"Data Source={DbFilePath};";

        // ─── Initialize Database | ڈیٹابیس شروع کریں ───────────────
        /// <summary>
        /// Initialize Database - Create folder, file and all tables
        /// ڈیٹابیس شروع کریں - فولڈر، فائل اور تمام ٹیبل بنائیں
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Create folder if not exists | فولڈر بنائیں اگر موجود نہ ہو
                if (!Directory.Exists(DbFolderPath))
                    Directory.CreateDirectory(DbFolderPath);

                // Create tables | ٹیبل بنائیں
                using var connection = GetConnection();
                CreateTables(connection);
                SeedDefaultData(connection);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Database initialization failed | ڈیٹابیس شروع کرنے میں ناکامی: {ex.Message}");
            }
        }

        // ─── Get Connection | کنکشن حاصل کریں ──────────────────────
        /// <summary>
        /// Get open SQLite connection | کھلا کنکشن حاصل کریں
        /// </summary>
        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // Enable WAL mode for better performance | بہتر کارکردگی کے لیے
            using var pragmaCmd = connection.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
            pragmaCmd.ExecuteNonQuery();

            return connection;
        }

        // ─── Create All Tables | تمام ٹیبل بنائیں ──────────────────
        private static void CreateTables(SqliteConnection connection)
        {
            using var cmd = connection.CreateCommand();

            // ── Users Table | صارفین کا ٹیبل ──────────────────────
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username                TEXT    NOT NULL UNIQUE,
                    PasswordHash            TEXT    NOT NULL,
                    PinCode                 TEXT    NOT NULL DEFAULT '',
                    SecurityQuestion        TEXT    NOT NULL DEFAULT '',
                    SecurityAnswerHash      TEXT    NOT NULL DEFAULT '',
                    Email                   TEXT    NOT NULL DEFAULT '',
                    IsEmailVerified         INTEGER NOT NULL DEFAULT 0,
                    OtpCode                 TEXT    NOT NULL DEFAULT '',
                    OtpExpiryTime           TEXT,
                    OtpPurpose              TEXT    NOT NULL DEFAULT '',
                    LastLoginDate           TEXT,
                    FailedLoginAttempts     INTEGER NOT NULL DEFAULT 0,
                    IsLocked                INTEGER NOT NULL DEFAULT 0,
                    LockedUntil             TEXT,
                    Role                    INTEGER NOT NULL DEFAULT 1,
                    IsActive                INTEGER NOT NULL DEFAULT 1,
                    PreferredTheme          INTEGER NOT NULL DEFAULT 1,
                    CreatedAt               TEXT    NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt               TEXT    NOT NULL DEFAULT (datetime('now'))
                );";
            cmd.ExecuteNonQuery();

            // ── Customers Table | قرض داروں کا ٹیبل ───────────────
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName            TEXT    NOT NULL,
                    FatherName          TEXT    NOT NULL DEFAULT '',
                    EmiratesIdOrCNIC    TEXT    NOT NULL DEFAULT '',
                    PhoneNumber1        TEXT    NOT NULL DEFAULT '',
                    PhoneNumber2        TEXT    NOT NULL DEFAULT '',
                    PhoneNumber3        TEXT    NOT NULL DEFAULT '',
                    Address             TEXT    NOT NULL DEFAULT '',
                    City                TEXT    NOT NULL DEFAULT '',
                    AccountNumber       TEXT    NOT NULL DEFAULT '',
                    SonOf               TEXT    NOT NULL DEFAULT '',
                    TotalLoanAmount     REAL    NOT NULL DEFAULT 0,
                    TotalPaidAmount     REAL    NOT NULL DEFAULT 0,
                    RemainingBalance    REAL    NOT NULL DEFAULT 0,
                    LoanStartDate       TEXT    NOT NULL DEFAULT (datetime('now')),
                    LoanEndDate         TEXT    NOT NULL DEFAULT (datetime('now')),
                    IsActive            INTEGER NOT NULL DEFAULT 1,
                    Notes               TEXT    NOT NULL DEFAULT '',
                    CreatedAt           TEXT    NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt           TEXT    NOT NULL DEFAULT (datetime('now'))
                );";
            cmd.ExecuteNonQuery();

            // ── Loans Table | قرضوں کا ٹیبل ────────────────────────
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Loans (
                    Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerId              INTEGER NOT NULL,
                    TotalAmount             REAL    NOT NULL DEFAULT 0,
                    RemainingAmount         REAL    NOT NULL DEFAULT 0,
                    PaidAmount              REAL    NOT NULL DEFAULT 0,
                    TotalInstallments       INTEGER NOT NULL DEFAULT 0,
                    RemainingInstallments   INTEGER NOT NULL DEFAULT 0,
                    PaidInstallments        INTEGER NOT NULL DEFAULT 0,
                    InstallmentAmount       REAL    NOT NULL DEFAULT 0,
                    StartDate               TEXT    NOT NULL DEFAULT (datetime('now')),
                    EndDate                 TEXT    NOT NULL DEFAULT (datetime('now')),
                    Status                  INTEGER NOT NULL DEFAULT 1,
                    LoanNumber              INTEGER NOT NULL DEFAULT 1,
                    IsMerged                INTEGER NOT NULL DEFAULT 0,
                    Notes                   TEXT    NOT NULL DEFAULT '',
                    CreatedAt               TEXT    NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt               TEXT    NOT NULL DEFAULT (datetime('now')),
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
                );";
            cmd.ExecuteNonQuery();

            // ── Payments Table | ادائیگیوں کا ٹیبل ─────────────────
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    Id                              INTEGER PRIMARY KEY AUTOINCREMENT,
                    LoanId                          INTEGER NOT NULL,
                    CustomerId                      INTEGER NOT NULL,
                    PaidAmount                      REAL    NOT NULL DEFAULT 0,
                    RemainingBalanceAfterPayment    REAL    NOT NULL DEFAULT 0,
                    BalanceBeforePayment            REAL    NOT NULL DEFAULT 0,
                    InstallmentNumber               INTEGER NOT NULL DEFAULT 0,
                    RemainingInstallmentsAfterPayment INTEGER NOT NULL DEFAULT 0,
                    TotalInstallments               INTEGER NOT NULL DEFAULT 0,
                    PaymentDate                     TEXT    NOT NULL DEFAULT (datetime('now')),
                    PaymentType                     INTEGER NOT NULL DEFAULT 1,
                    PaymentMethod                   TEXT    NOT NULL DEFAULT '',
                    VoucherNumber                   TEXT    NOT NULL DEFAULT '',
                    ReceivedBy                      TEXT    NOT NULL DEFAULT '',
                    Notes                           TEXT    NOT NULL DEFAULT '',
                    IsVerified                      INTEGER NOT NULL DEFAULT 1,
                    CreatedAt                       TEXT    NOT NULL DEFAULT (datetime('now')),
                    FOREIGN KEY (LoanId)      REFERENCES Loans(Id)     ON DELETE CASCADE,
                    FOREIGN KEY (CustomerId)  REFERENCES Customers(Id) ON DELETE CASCADE
                );";
            cmd.ExecuteNonQuery();

            // ── AppSettings Table | ایپ ترتیبات کا ٹیبل ───────────
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS AppSettings (
                    Id                          INTEGER PRIMARY KEY AUTOINCREMENT,
                    CurrentTheme                INTEGER NOT NULL DEFAULT 1,
                    ThemeLastChanged            TEXT    NOT NULL DEFAULT (datetime('now')),
                    DefaultCurrency             TEXT    NOT NULL DEFAULT 'PKR',
                    CurrencySymbol              TEXT    NOT NULL DEFAULT 'PKR',
                    SecondaryCurrency           TEXT    NOT NULL DEFAULT 'PKR',
                    ExchangeRate                REAL    NOT NULL DEFAULT 77,
                    ExchangeRateLastUpdated     TEXT    NOT NULL DEFAULT (datetime('now')),
                    SmtpHost                    TEXT    NOT NULL DEFAULT 'smtp.gmail.com',
                    SmtpPort                    INTEGER NOT NULL DEFAULT 587,
                    SmtpUsername                TEXT    NOT NULL DEFAULT '',
                    SmtpPassword                TEXT    NOT NULL DEFAULT '',
                    SenderEmail                 TEXT    NOT NULL DEFAULT '',
                    SenderName                  TEXT    NOT NULL DEFAULT 'Loan Management System',
                    IsEmailConfigured           INTEGER NOT NULL DEFAULT 0,
                    EnableSsl                   INTEGER NOT NULL DEFAULT 1,
                    BackupFolderPath            TEXT    NOT NULL DEFAULT '',
                    AutoBackupEnabled           INTEGER NOT NULL DEFAULT 1,
                    AutoBackupIntervalHours     INTEGER NOT NULL DEFAULT 24,
                    LastBackupDate              TEXT,
                    LastBackupFilePath          TEXT    NOT NULL DEFAULT '',
                    MaxBackupFilesToKeep        INTEGER NOT NULL DEFAULT 10,
                    AppName                     TEXT    NOT NULL DEFAULT 'Loan Management System',
                    AppNameUrdu                 TEXT    NOT NULL DEFAULT 'قرض مینجمنٹ سسٹم',
                    CompanyName                 TEXT    NOT NULL DEFAULT '',
                    CompanyNameUrdu             TEXT    NOT NULL DEFAULT '',
                    CompanyPhone                TEXT    NOT NULL DEFAULT '',
                    CompanyAddress              TEXT    NOT NULL DEFAULT '',
                    OtpExpiryMinutes            INTEGER NOT NULL DEFAULT 5,
                    OtpLength                   INTEGER NOT NULL DEFAULT 6,
                    MaxFailedLoginAttempts      INTEGER NOT NULL DEFAULT 5,
                    AccountLockDurationMinutes  INTEGER NOT NULL DEFAULT 30,
                    RequirePinForSensitiveActions INTEGER NOT NULL DEFAULT 1,
                    DateFormat                  TEXT    NOT NULL DEFAULT 'dd-MMM-yyyy',
                    TimeFormat                  TEXT    NOT NULL DEFAULT 'hh:mm tt',
                    UpdatedAt                   TEXT    NOT NULL DEFAULT (datetime('now'))
                );";
            cmd.ExecuteNonQuery();
        }

        // ─── Seed Default Data | ڈیفالٹ ڈیٹا ڈالیں ─────────────────
        /// <summary>
        /// Insert default admin user and settings if not exist
        /// ڈیفالٹ ایڈمن اور ترتیبات ڈالیں اگر موجود نہ ہوں
        /// </summary>
        private static void SeedDefaultData(SqliteConnection connection)
        {
            using var cmd = connection.CreateCommand();

            // Check if admin exists by Role | ایڈمن کردار سے چیک کریں
            cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Role = 1;";
            var count = Convert.ToInt32(cmd.ExecuteScalar());

            if (count == 0)
            {
                // Insert default admin | ڈیفالٹ ایڈمن ڈالیں
                // Default: Username=admin, Password=admin
                string defaultPasswordHash =
                    BCrypt.Net.BCrypt.HashPassword("admin");

                cmd.CommandText = @"
                    INSERT INTO Users 
                    (Username, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
                    VALUES 
                    ('admin', @hash, 1, 1, datetime('now'), datetime('now'));";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@hash", defaultPasswordHash);
                cmd.ExecuteNonQuery();

                // ─── Get admin ID and set default security question ───
                // حفاظتی سوال اور جواب ڈیفالٹ سیٹ کریں
                cmd.CommandText = "SELECT Id FROM Users WHERE Username = 'admin';";
                cmd.Parameters.Clear();
                int adminId = Convert.ToInt32(cmd.ExecuteScalar());

                string defaultQuestion = "What is your name?";
                string defaultAnswerHash = BCrypt.Net.BCrypt.HashPassword("ishfaq");

                cmd.CommandText = @"
                    UPDATE Users SET 
                        SecurityQuestion = @Question,
                        SecurityAnswerHash = @AnswerHash
                    WHERE Id = @Id;";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Question", defaultQuestion);
                cmd.Parameters.AddWithValue("@AnswerHash", defaultAnswerHash);
                cmd.Parameters.AddWithValue("@Id", adminId);
                cmd.ExecuteNonQuery();
            }

            // Check if settings exist | ترتیبات موجود ہیں یا نہیں
            cmd.CommandText = "SELECT COUNT(*) FROM AppSettings;";
            cmd.Parameters.Clear();
            var settingsCount = Convert.ToInt32(cmd.ExecuteScalar());

            if (settingsCount == 0)
            {
                // Insert default settings | ڈیفالٹ ترتیبات ڈالیں
                string defaultBackupPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "LoanManagementBackups");

                cmd.CommandText = @"
                    INSERT INTO AppSettings 
                    (BackupFolderPath, UpdatedAt)
                    VALUES 
                    (@backupPath, datetime('now'));";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@backupPath", defaultBackupPath);
                cmd.ExecuteNonQuery();
            }
        }

        // ─── Database Info | ڈیٹابیس کی معلومات ────────────────────
        /// <summary>
        /// Get Database File Path | ڈیٹابیس فائل کا راستہ حاصل کریں
        /// </summary>
        public static string GetDatabaseFilePath() => DbFilePath;

        /// <summary>
        /// Get Database Folder Path | ڈیٹابیس فولڈر کا راستہ حاصل کریں
        /// </summary>
        public static string GetDatabaseFolderPath() => DbFolderPath;

        /// <summary>
        /// Check if Database Exists | کیا ڈیٹابیس موجود ہے
        /// </summary>
        public static bool DatabaseExists() => File.Exists(DbFilePath);

        /// <summary>
        /// Get Database File Size | ڈیٹابیس فائل کا سائز
        /// </summary>
        public static string GetDatabaseSize()
        {
            if (!DatabaseExists()) return "0 KB | صفر";
            var size = new FileInfo(DbFilePath).Length;
            return size < 1024
                ? $"{size} B"
                : size < 1048576
                    ? $"{size / 1024.0:F1} KB"
                    : $"{size / 1048576.0:F1} MB";
        }
    }
}