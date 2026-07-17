using System.Windows.Controls;
using LoanManagementApp.ViewModels;

namespace LoanManagementApp.Views.Pages
{
    /// <summary>
    /// DashboardPage.xaml.cs | ڈیش بورڈ پیج کوڈ بیہائنڈ
    /// Code-behind for the main dashboard page
    /// مرکزی ڈیش بورڈ پیج کا کوڈ
    /// </summary>
    public partial class DashboardPage : Page
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ───────────────────
        private DashboardViewModel? _viewModel;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public DashboardPage()
        {
            InitializeComponent();

            // Grab the ViewModel set in XAML
            _viewModel = DataContext as DashboardViewModel;

            // Reload when page is shown | پیج دکھانے پر ریلوڈ کریں
            Loaded += DashboardPage_Loaded;
        }

        // ─── Page Loaded | پیج لوڈ ہوا ──────────────────────────────
        private void DashboardPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Refresh data every time page becomes visible
            // ہر بار پیج دکھنے پر ڈیٹا ریفریش کریں
            _viewModel?.LoadDashboard();
        }
    }
}
