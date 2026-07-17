using System;
using System.Windows;
using LoanManagementApp.Data;
using LoanManagementApp.Models;
using LoanManagementApp.Services;

namespace LoanManagementApp
{
    /// <summary>
    /// App.xaml.cs — Application Entry Point
    /// ایپ کا آغاز — ڈیٹابیس، تھیم، اور غلطی سنبھالنا
    /// </summary>
    public partial class App : Application
    {
        // ─── Theme Source Keys | تھیم سورس کیز ─────────────────────
        // Only DarkTheme is used (light theme is removed)
        private const string DarkThemeSource = "Themes/DarkTheme.xaml";

        // ─── OnStartup | آغاز پر ────────────────────────────────────
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ── 1. Global Exception Handlers | عالمی غلطی ───────────
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            try
            {
                // ── 2. Initialize Database | ڈیٹابیس شروع کریں ──────
                DatabaseContext.Initialize();

                // ── 3. Apply Dark Theme (only theme) | صرف گہرا تھیم لگائیں ──
                ApplyDarkTheme();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Startup Error | آغاز میں خرابی:\n\n{ex.Message}\n\n" +
                    $"The application will close. | ایپلیکیشن بند ہو جائے گی۔",
                    "Loan Management System — Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
            }
        }

        // ─── App_Startup | اسپلیش اسکرین سے شروع کریں ──────────────
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // SplashScreen will open MainWindow when done
            // اسپلیش اسکرین مکمل ہونے پر مرکزی ونڈو کھولے گی
            var splash = new LoanManagementApp.Views.SplashScreen();
            splash.Show();
        }

        // ─── OnExit | بند ہونے پر ───────────────────────────────────
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // Future cleanup can go here | مستقبل کی صفائی یہاں آ سکتی ہے
        }

        // ═══════════════════════════════════════════════════════════
        // THEME (fixed to Dark) | تھیم (صرف گہرا)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Apply Dark theme at startup (no light theme option available).
        /// ایپ شروع ہونے پر گہرا تھیم لگائیں (روشن تھیم کا آپشن نہیں)۔
        /// </summary>
        private void ApplyDarkTheme()
        {
            var app = (App)Current;
            var mergedDicts = app.Resources.MergedDictionaries;

            // Remove any existing theme dictionary
            ResourceDictionary? existingTheme = null;
            foreach (var dict in mergedDicts)
            {
                string src = dict.Source?.OriginalString ?? string.Empty;
                if (src.Contains("DarkTheme") || src.Contains("LightTheme"))
                {
                    existingTheme = dict;
                    break;
                }
            }
            if (existingTheme != null)
                mergedDicts.Remove(existingTheme);

            // Add DarkTheme at position 0
            mergedDicts.Insert(0, new ResourceDictionary
            {
                Source = new Uri(DarkThemeSource, UriKind.Relative)
            });
        }

        // Kept for backward compatibility (but now only dark theme is applied)
        public static void ApplyTheme(AppTheme theme)
        {
            // Ignore the parameter and always apply dark theme
            var app = (App)Current;
            var mergedDicts = app.Resources.MergedDictionaries;

            ResourceDictionary? existingTheme = null;
            foreach (var dict in mergedDicts)
            {
                string src = dict.Source?.OriginalString ?? string.Empty;
                if (src.Contains("DarkTheme") || src.Contains("LightTheme"))
                {
                    existingTheme = dict;
                    break;
                }
            }
            if (existingTheme != null)
                mergedDicts.Remove(existingTheme);

            mergedDicts.Insert(0, new ResourceDictionary
            {
                Source = new Uri(DarkThemeSource, UriKind.Relative)
            });
        }

        // ═══════════════════════════════════════════════════════════
        // GLOBAL EXCEPTION HANDLERS | عالمی غلطی سنبھالنے والے
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Catch unhandled exceptions on background threads
        /// پس منظر تھریڈز پر بے قابو غلطیاں پکڑیں
        /// </summary>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = e.ExceptionObject is Exception ex
                ? ex.Message
                : e.ExceptionObject?.ToString() ?? "Unknown error";

            MessageBox.Show(
                $"Unexpected Error | غیر متوقع خرابی:\n\n{message}\n\n" +
                $"Please restart the app. | براہ کرم ایپ دوبارہ شروع کریں۔",
                "Loan Management System — Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        /// <summary>
        /// Catch unhandled exceptions on the UI thread
        /// UI تھریڈ پر بے قابو غلطیاں پکڑیں
        /// </summary>
        private void OnDispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"UI Error | UI خرابی:\n\n{e.Exception.Message}\n\n" +
                $"The app will try to continue. | ایپ جاری رہنے کی کوشش کرے گی۔",
                "Loan Management System — Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            e.Handled = true; // Keep app alive | ایپ چلتی رہے
        }
    }
}