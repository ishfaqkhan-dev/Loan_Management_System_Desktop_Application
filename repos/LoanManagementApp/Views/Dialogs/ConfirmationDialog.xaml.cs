using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LoanManagementApp.Views.Dialogs
{
    /// <summary>
    /// ConfirmationDialog.xaml — Code Behind
    /// تصدیقی ڈائیلاگ — کوڈ بیہائنڈ
    ///
    /// Fully reusable confirm / alert dialog.
    /// مکمل دوبارہ استعمال قابل تصدیق / الرٹ ڈائیلاگ۔
    ///
    /// Usage examples | استعمال کی مثالیں:
    ///
    ///   // Simple delete confirm | سادہ حذف تصدیق
    ///   var dlg = ConfirmationDialog.AskDelete("Ali Khan");
    ///   if (dlg.ShowDialog() == true) { /* delete */ }
    ///
    ///   // Custom warning | کسٹم وارننگ
    ///   var dlg = new ConfirmationDialog
    ///   {
    ///       TitleText   = "⚠️ Warning | تنبیہ",
    ///       MessageText = "Are you sure? | کیا آپ یقینی ہیں؟",
    ///       IsWarning   = true
    ///   };
    ///   dlg.ShowDialog();
    /// </summary>
    public partial class ConfirmationDialog : Window, INotifyPropertyChanged
    {
        // ─── INotifyPropertyChanged ──────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? p = null)
        {
            if (Equals(field, value)) return false;
            field = value; OnPropertyChanged(p); return true;
        }

        // ─── Bindable Properties | قابل باندھ خصوصیات ───────────────

        private string _titleText = "Confirm | تصدیق کریں";
        public string TitleText
        {
            get => _titleText;
            set { SetField(ref _titleText, value); Title = value; }
        }

        private string _iconText = "❓";
        public string IconText
        {
            get => _iconText;
            set => SetField(ref _iconText, value);
        }

        private string _messageText = string.Empty;
        public string MessageText
        {
            get => _messageText;
            set => SetField(ref _messageText, value);
        }

        private string _subMessageText = string.Empty;
        public string SubMessageText
        {
            get => _subMessageText;
            set { SetField(ref _subMessageText, value); OnPropertyChanged(nameof(HasSubMessage)); }
        }
        public bool HasSubMessage => !string.IsNullOrWhiteSpace(_subMessageText);

        private string _detailText = string.Empty;
        public string DetailText
        {
            get => _detailText;
            set { SetField(ref _detailText, value); OnPropertyChanged(nameof(HasDetailText)); }
        }
        public bool HasDetailText => !string.IsNullOrWhiteSpace(_detailText);

        private bool _isWarning = false;
        public bool IsWarning
        {
            get => _isWarning;
            set => SetField(ref _isWarning, value);
        }

        private string _confirmButtonText = "✅ Yes, Confirm | ہاں، تصدیق کریں";
        public string ConfirmButtonText
        {
            get => _confirmButtonText;
            set => SetField(ref _confirmButtonText, value);
        }

        private string _cancelButtonText = "✖ No, Cancel | نہیں، منسوخ";
        public string CancelButtonText
        {
            get => _cancelButtonText;
            set => SetField(ref _cancelButtonText, value);
        }

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public ConfirmationDialog()
        {
            InitializeComponent();
            DataContext = this;

            BtnYes.Click += (_, _) => { DialogResult = true; Close(); };
            BtnNo.Click += (_, _) => { DialogResult = false; Close(); };
        }

        // ═══════════════════════════════════════════════════════════
        // STATIC FACTORY HELPERS | فیکٹری مددگار طریقے
        // Easy-to-use presets for common scenarios
        // عام حالات کے لیے آسان تیار ترتیبات
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Delete confirmation dialog | حذف تصدیق ڈائیلاگ
        /// </summary>
        public static ConfirmationDialog AskDelete(string itemName, string? extraDetail = null)
            => new()
            {
                TitleText = "🗑️ Delete | حذف کریں",
                IconText = "🗑️",
                MessageText = $"Delete \"{itemName}\"?\nکیا \"{itemName}\" کو حذف کریں؟",
                SubMessageText = "This action cannot be undone. | یہ عمل واپس نہیں لیا جا سکتا۔",
                DetailText = extraDetail ?? string.Empty,
                IsWarning = true,
                ConfirmButtonText = "🗑️ Yes, Delete | ہاں، حذف کریں",
                CancelButtonText = "✖ Cancel | منسوخ"
            };

        /// <summary>
        /// Logout confirmation dialog | لاگ آؤٹ تصدیق ڈائیلاگ
        /// </summary>
        public static ConfirmationDialog AskLogout()
            => new()
            {
                TitleText = "🚪 Logout | لاگ آؤٹ",
                IconText = "🚪",
                MessageText = "Are you sure you want to logout?\nکیا آپ لاگ آؤٹ کرنا چاہتے ہیں؟",
                SubMessageText = "You will need to login again. | آپ کو دوبارہ لاگ ان کرنا پڑے گا۔",
                IsWarning = false,
                ConfirmButtonText = "🚪 Yes, Logout | ہاں، لاگ آؤٹ",
                CancelButtonText = "✖ Stay | رہیں"
            };

        /// <summary>
        /// General warning / confirm | عمومی وارننگ / تصدیق
        /// </summary>
        public static ConfirmationDialog AskConfirm(
            string title,
            string message,
            string? subMessage = null,
            bool isWarning = false,
            string confirmText = "✅ Yes | ہاں",
            string cancelText = "✖ No | نہیں")
            => new()
            {
                TitleText = title,
                IconText = isWarning ? "⚠️" : "❓",
                MessageText = message,
                SubMessageText = subMessage ?? string.Empty,
                IsWarning = isWarning,
                ConfirmButtonText = confirmText,
                CancelButtonText = cancelText
            };

        /// <summary>
        /// Info / alert only (no cancel) | صرف معلومات (منسوخ نہیں)
        /// </summary>
        public static ConfirmationDialog ShowInfo(string title, string message)
        {
            var dlg = new ConfirmationDialog
            {
                TitleText = title,
                IconText = "ℹ️",
                MessageText = message,
                IsWarning = false,
                ConfirmButtonText = "✅ OK | ٹھیک ہے"
            };
            dlg.BtnNo.Visibility = Visibility.Collapsed;
            return dlg;
        }
    }
}