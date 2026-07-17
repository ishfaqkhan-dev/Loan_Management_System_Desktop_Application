using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LoanManagementApp.Views.Dialogs
{
    /// <summary>
    /// NotificationDialog.xaml.cs | اطلاع ڈائیلاگ کوڈ بیہائنڈ
    ///
    /// Logout dialog jaise style mein success/error popup
    /// لاگ آؤٹ ڈائیلاگ جیسے انداز میں کامیابی/خرابی پاپ اپ
    ///
    /// Usage | استعمال:
    ///   NotificationDialog.ShowSuccess(this, "Password changed!");
    ///   NotificationDialog.ShowError(this, "Current password is wrong.");
    /// </summary>
    public partial class NotificationDialog : Window
    {
        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        private NotificationDialog()
        {
            InitializeComponent();
        }

        // ═══════════════════════════════════════════════════════════
        // STATIC FACTORY METHODS | اسٹیٹک فیکٹری میتھڈز
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Show success notification popup | کامیابی کا پاپ اپ دکھائیں
        /// </summary>
        public static void ShowSuccess(Window owner, string message)
        {
            var dlg = new NotificationDialog();
            dlg.Owner = owner;
            dlg.ConfigureSuccess(message);
            dlg.ShowDialog();
        }

        /// <summary>
        /// Show error notification popup | خرابی کا پاپ اپ دکھائیں
        /// </summary>
        public static void ShowError(Window owner, string message)
        {
            var dlg = new NotificationDialog();
            dlg.Owner = owner;
            dlg.ConfigureError(message);
            dlg.ShowDialog();
        }

        // ═══════════════════════════════════════════════════════════
        // CONFIGURE MODES | موڈ ترتیب
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Configure as success | کامیابی موڈ ترتیب دیں
        /// </summary>
        private void ConfigureSuccess(string message)
        {
            TxtTitle.Text = "✅ کامیاب | Success";
            TxtMessage.Text = message;

            // ── Green icon and circle | سبز آئیکن اور دائرہ ──
            IconPath.Data = (Geometry)FindResource("IconCheck");
            IconPathLarge.Data = (Geometry)FindResource("IconCheck");

            IconPath.Fill = (Brush)FindResource("SuccessBrush");
            IconPathLarge.Fill = (Brush)FindResource("SuccessBrush");
            IconCircle.Background = (Brush)FindResource("SuccessLightBrush");

            // ── OK button green | سبز بٹن ──
            BtnOk.Style = (Style)FindResource("SuccessButtonStyle");
            BtnOk.Content = "✓  ٹھیک ہے | OK";
        }

        /// <summary>
        /// Configure as error | خرابی موڈ ترتیب دیں
        /// </summary>
        private void ConfigureError(string message)
        {
            TxtTitle.Text = "❌ خرابی | Error";
            TxtMessage.Text = message;

            // ── Red/Warning icon and circle | سرخ آئیکن اور دائرہ ──
            IconPath.Data = (Geometry)FindResource("IconWarning");
            IconPathLarge.Data = (Geometry)FindResource("IconWarning");

            IconPath.Fill = (Brush)FindResource("DangerBrush");
            IconPathLarge.Fill = (Brush)FindResource("DangerBrush");
            IconCircle.Background = (Brush)FindResource("DangerLightBrush");

            // ── OK button red | سرخ بٹن ──
            BtnOk.Style = (Style)FindResource("DangerButtonStyle");
            BtnOk.Content = "✓  ٹھیک ہے | OK";
        }

        // ═══════════════════════════════════════════════════════════
        // EVENT HANDLERS | ایونٹ ہینڈلرز
        // ═══════════════════════════════════════════════════════════

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseDown(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }
    }
}