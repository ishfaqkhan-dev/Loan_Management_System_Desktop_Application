using System.Windows;
using System.Windows.Controls;
using LoanManagementApp.Models;
using LoanManagementApp.ViewModels;

namespace LoanManagementApp.Views.Pages
{
    /// <summary>
    /// AccountPage.xaml.cs | اکاؤنٹ پیج کوڈ بیہائنڈ
    /// Displays customer list and navigates to loan details page.
    /// قرض داروں کی فہرست دکھاتا ہے اور قرض کی تفصیلات والے پیج پر لے جاتا ہے۔
    /// </summary>
    public partial class AccountPage : Page
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ───────────────────
        private AccountViewModel? _viewModel;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public AccountPage()
        {
            InitializeComponent();

            _viewModel = DataContext as AccountViewModel;

            // Subscribe to ViewAccountRequested event | ایونٹ سبسکرائب کریں
            if (_viewModel != null)
                _viewModel.ViewAccountRequested += OnViewAccountRequested;

            // Back button handler | واپس بٹن
            BtnBack.Click += (s, e) => NavigationService?.GoBack();
        }

        // ─── Event Handler: View Account Requested | اکاؤنٹ دیکھنے کی درخواست ──
        /// <summary>
        /// Called when user clicks "View Account" button.
        /// Navigates to CustomerLoanPage to show detailed loan information.
        /// جب صارف "View Account" بٹن پر کلک کرے تو کال ہوتا ہے۔
        /// </summary>
        private void OnViewAccountRequested(Customer customer)
        {
            var loanPage = new CustomerLoanPage();
            loanPage.LoadCustomer(customer);
            NavigationService?.Navigate(loanPage);
        }
    }
}