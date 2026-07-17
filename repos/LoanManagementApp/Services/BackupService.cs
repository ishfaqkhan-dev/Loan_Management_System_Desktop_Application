using System;
using System.IO;
using LoanManagementApp.Data;
using LoanManagementApp.Models;
using Microsoft.Data.Sqlite;

namespace LoanManagementApp.Services
{
    /// <summary>
    /// BackupService / بیک اپ سروس - Database backup and recovery management
    /// </summary>
    public class BackupService
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly AppSettingsService _settingsService;

        // ─── Backup File Name Format | بیک اپ فائل نام فارمیٹ ───────
        private const string BackupPrefix = "LoanDB_Backup_";
        private const string BackupExt = ".db";

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public BackupService()
        {
            _settingsService = new AppSettingsService();
        }

        // ─── Manual Backup | دستی بیک اپ ────────────────────────────
        /// <summary>
        /// Create backup now on user request
        /// صارف کی درخواست پر ابھی بیک اپ بنائیں
        /// </summary>
        public (bool Success, string Message, string FilePath) CreateBackup()
        {
            try
            {
                var settings = _settingsService.GetSettings();

                // ── Resolve backup folder — use Documents\LoanManagementBackups if empty or invalid ──
                // اگر بیک اپ فولڈر خالی ہو تو Documents فولڈر استعمال کریں
                string backupFolder = settings.BackupFolderPath;
                if (string.IsNullOrWhiteSpace(backupFolder) ||
                    backupFolder.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        StringComparison.OrdinalIgnoreCase))
                {
                    // AppData is protected by Controlled Folder Access — switch to Documents
                    // AppData پر Windows Security پابندی لگاتا ہے — Documents استعمال کریں
                    backupFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "LoanManagementBackups");

                    // Persist the corrected path back to settings
                    settings.BackupFolderPath = backupFolder;
                    _settingsService.UpdateSettings(settings);
                }

                // Ensure backup folder exists | بیک اپ فولڈر یقینی بنائیں
                EnsureBackupFolder(backupFolder);

                // Generate backup file name | بیک اپ فائل نام بنائیں
                string fileName = BackupPrefix +
                                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") +
                                    BackupExt;
                string backupPath = Path.Combine(backupFolder, fileName);

                // Source DB file from DatabaseContext | ماخذ ڈیٹابیس فائل
                string sourcePath = DatabaseContext.GetDatabaseFilePath();

                // Verify source file exists | ماخذ فائل کی موجودگی کی تصدیق
                if (!File.Exists(sourcePath))
                {
                    try { DatabaseContext.Initialize(); } catch { /* ignore */ }

                    if (!File.Exists(sourcePath))
                        return (false,
                            $"Database file not found at: {sourcePath}\nڈیٹابیس فائل نہیں ملی",
                            string.Empty);
                }

                // ── STRATEGY 1: SQLite Online Backup API (Best — no file lock issues) ──
                // سب سے بہتر طریقہ: SQLite کا اپنا بیک اپ API
                // NOTE: Do NOT use Mode=ReadOnly here — destination needs write access
                // ReadOnly mode مت استعمال کریں — destination کو لکھنے کی اجازت چاہیے
                Exception? primaryException = null;
                try
                {
                    using var sourceConn = new SqliteConnection($"Data Source={sourcePath};");
                    using var destConn = new SqliteConnection($"Data Source={backupPath};");

                    sourceConn.Open();
                    destConn.Open();

                    // Checkpoint WAL before backup so all data is in main file
                    // بیک اپ سے پہلے WAL چیک پوائنٹ کریں تاکہ تمام ڈیٹا main فائل میں ہو
                    using (var cmd = sourceConn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA wal_checkpoint(PASSIVE);";
                        cmd.ExecuteNonQuery();
                    }

                    sourceConn.BackupDatabase(destConn);

                    // Success — update settings and return
                    return FinalizeBackup(settings, backupPath, fileName);
                }
                catch (Exception ex)
                {
                    primaryException = ex;
                    // Fall through to Strategy 2
                }

                // ── STRATEGY 2: WAL Checkpoint + Raw File Copy (Fallback) ──
                // دوسرا طریقہ: WAL چیک پوائنٹ پھر فائل کاپی
                Exception? fallbackException = null;
                try
                {
                    // Checkpoint to flush WAL into main .db file | WAL کو flush کریں
                    using (var conn = new SqliteConnection($"Data Source={sourcePath};"))
                    {
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                        cmd.ExecuteNonQuery();
                    }

                    // Read all bytes and write to destination — avoids any file-lock bypass issues
                    // تمام بائٹس پڑھیں اور destination پر لکھیں
                    byte[] dbBytes = File.ReadAllBytes(sourcePath);
                    File.WriteAllBytes(backupPath, dbBytes);

                    return FinalizeBackup(settings, backupPath, fileName);
                }
                catch (Exception ex)
                {
                    fallbackException = ex;
                }

                // Both strategies failed
                return (false,
                    $"Backup failed:\n" +
                    $"• Primary: {primaryException?.Message}\n" +
                    $"• Fallback: {fallbackException?.Message}\n\n" +
                    "Fix: Go to Windows Security → Ransomware Protection → Allow an app → Add LoanManagementApp.exe\n" +
                    "حل: Windows Security میں اپنی App کو Controlled Folder Access سے استثنیٰ دیں",
                    string.Empty);
            }
            catch (Exception ex)
            {
                return (false,
                    $"Backup failed | بیک اپ ناکام: {ex.Message}",
                    string.Empty);
            }
        }

        // ─── Finalize Backup (common post-success steps) ────────────
        private (bool Success, string Message, string FilePath) FinalizeBackup(
            dynamic settings, string backupPath, string fileName)
        {
            // Update last backup info | آخری بیک اپ معلومات اپ ڈیٹ کریں
            settings.LastBackupDate = DateTime.Now;
            settings.LastBackupFilePath = backupPath;
            _settingsService.UpdateSettings(settings);

            // Clean old backups | پرانے بیک اپ صاف کریں
            CleanOldBackups(settings.BackupFolderPath, settings.MaxBackupFilesToKeep);

            return (true,
                $"Backup created successfully | بیک اپ کامیابی سے بنا:\n{fileName}\n📁 {settings.BackupFolderPath}",
                backupPath);
        }

        // ─── Auto Backup | خودکار بیک اپ ────────────────────────────
        /// <summary>
        /// Run auto backup if interval has passed
        /// وقفہ گزرنے پر خودکار بیک اپ چلائیں
        /// </summary>
        public (bool Ran, string Message) RunAutoBackupIfDue()
        {
            try
            {
                var settings = _settingsService.GetSettings();

                if (!settings.AutoBackupEnabled)
                    return (false, "Auto backup is disabled | خودکار بیک اپ غیر فعال ہے");

                if (!settings.IsBackupDue)
                    return (false, $"Backup not due yet. {settings.NextBackupDisplay}");

                var result = CreateBackup();
                return result.Success
                    ? (true, result.Message)
                    : (false, result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Auto backup error | خودکار بیک اپ خرابی: {ex.Message}");
            }
        }

        // ─── Restore Backup | بیک اپ بحال کریں ──────────────────────
        public (bool Success, string Message) RestoreBackup(string backupFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(backupFilePath))
                    return (false, "Backup file path is required | بیک اپ فائل کا راستہ ضروری ہے");

                if (!File.Exists(backupFilePath))
                    return (false, "Backup file not found | بیک اپ فائل نہیں ملی");

                if (!backupFilePath.EndsWith(BackupExt, StringComparison.OrdinalIgnoreCase))
                    return (false, "Invalid backup file format | بیک اپ فائل فارمیٹ غلط ہے");

                string sourcePath = DatabaseContext.GetDatabaseFilePath();

                // Safety backup before restore | بحالی سے پہلے حفاظتی بیک اپ
                string safetyBackupPath = sourcePath + ".before_restore_" +
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".bak";
                if (File.Exists(sourcePath))
                    File.Copy(sourcePath, safetyBackupPath, overwrite: true);

                File.Copy(backupFilePath, sourcePath, overwrite: true);

                return (true,
                    "Database restored successfully. Please restart the application | " +
                    "ڈیٹابیس کامیابی سے بحال ہوا۔ براہ کرم ایپلیکیشن دوبارہ شروع کریں");
            }
            catch (Exception ex)
            {
                return (false, $"Restore failed | بحالی ناکام: {ex.Message}");
            }
        }

        // ─── Get All Backups | تمام بیک اپ ──────────────────────────
        public (bool Success, string[] Files) GetAllBackups()
        {
            try
            {
                var settings = _settingsService.GetSettings();

                if (!Directory.Exists(settings.BackupFolderPath))
                    return (true, Array.Empty<string>());

                var files = Directory.GetFiles(
                    settings.BackupFolderPath, BackupPrefix + "*" + BackupExt);

                Array.Sort(files, (a, b) => string.Compare(b, a, StringComparison.Ordinal));
                return (true, files);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get backups | بیک اپ فہرست ناکام: {ex.Message}");
            }
        }

        // ─── Change Backup Folder ────────────────────────────────────
        public (bool Success, string Message) ChangeBackupFolder(string newFolderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newFolderPath))
                    return (false, "Folder path is required | فولڈر کا راستہ ضروری ہے");

                EnsureBackupFolder(newFolderPath);

                var settings = _settingsService.GetSettings();
                settings.BackupFolderPath = newFolderPath;
                _settingsService.UpdateSettings(settings);

                return (true, $"Backup folder updated | بیک اپ فولڈر اپ ڈیٹ ہوا: {newFolderPath}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change backup folder | فولڈر تبدیل ناکام: {ex.Message}");
            }
        }

        // ─── Get Backup File Info ────────────────────────────────────
        public string GetBackupFileInfo(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath)) return "File not found | فائل نہیں ملی";

                var info = new FileInfo(backupFilePath);
                string size = info.Length < 1024
                    ? $"{info.Length} B"
                    : info.Length < 1048576
                        ? $"{info.Length / 1024.0:F1} KB"
                        : $"{info.Length / 1048576.0:F1} MB";

                return $"{info.Name} | {size} | {info.CreationTime:dd-MMM-yyyy hh:mm tt}";
            }
            catch { return "Error reading file info | فائل معلومات خرابی"; }
        }

        // ─── Toggle Auto Backup ──────────────────────────────────────
        public (bool Success, string Message) ToggleAutoBackup(bool enable)
        {
            try
            {
                var settings = _settingsService.GetSettings();
                settings.AutoBackupEnabled = enable;
                _settingsService.UpdateSettings(settings);

                return (true, enable
                    ? "Auto backup enabled | خودکار بیک اپ فعال کیا گیا"
                    : "Auto backup disabled | خودکار بیک اپ غیر فعال کیا گیا");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to toggle auto backup | خودکار بیک اپ ٹوگل ناکام: {ex.Message}");
            }
        }

        // ─── Private Helpers ─────────────────────────────────────────

        private void EnsureBackupFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        private void CleanOldBackups(string folderPath, int maxToKeep)
        {
            try
            {
                if (!Directory.Exists(folderPath)) return;

                var files = Directory.GetFiles(folderPath, BackupPrefix + "*" + BackupExt);
                Array.Sort(files, (a, b) => string.Compare(a, b, StringComparison.Ordinal));

                int toDelete = files.Length - maxToKeep;
                for (int i = 0; i < toDelete; i++)
                {
                    try { File.Delete(files[i]); } catch { /* skip */ }
                }
            }
            catch { /* Non-critical cleanup */ }
        }
    }
}