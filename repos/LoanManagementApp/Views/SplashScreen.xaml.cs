using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace LoanManagementApp.Views
{
    /// <summary>
    /// SplashScreen.xaml.cs | اسپلیش اسکرین کوڈ بیہائنڈ
    /// Shows animated splash for ~3 seconds, then opens MainWindow.
    /// تقریباً 3 سیکنڈ انیمیشن دکھا کر مرکزی ونڈو کھولتا ہے۔
    /// </summary>
    public partial class SplashScreen : Window
    {
        // Total display time in milliseconds | کل ڈسپلے وقت ملی سیکنڈ میں
        private const int SplashDurationMs = 5000;

        // ─── Constructor | کنسٹرکٹر ──────────────────────────────────
        public SplashScreen()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        // ─── On Window Loaded | ونڈو لوڈ ہونے پر ─────────────────────
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Wait for the full splash duration (animations run in parallel via XAML)
            // انیمیشن XAML میں چل رہی ہیں، ہم صرف وقت کا انتظار کریں
            await Task.Delay(SplashDurationMs);

            // ── Fade out smoothly before closing | بند ہونے سے پہلے آہستہ غائب ہوں ──
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                FillBehavior = FillBehavior.Stop
            };

            fadeOut.Completed += (_, _) =>
            {
                // Open MainWindow and close splash | مرکزی ونڈو کھولیں اور اسپلیش بند کریں
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                Close();
            };

            BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}