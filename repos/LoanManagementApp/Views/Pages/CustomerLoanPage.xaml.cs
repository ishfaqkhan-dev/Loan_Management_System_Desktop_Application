using System.Windows;
using System.Windows.Controls;
using LoanManagementApp.Models;
using LoanManagementApp.ViewModels;

namespace LoanManagementApp.Views.Pages
{
    /// <summary>
    /// CustomerLoanPage.xaml.cs | قرض دار کے قرض کی تفصیلات پیج کوڈ بیہائنڈ
    /// Displays detailed loan information for a selected customer.
    /// منتخب قرض دار کے قرض کی مکمل تفصیلات دکھاتا ہے۔
    /// </summary>
    public partial class CustomerLoanPage : Page
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ───────────────────
        private CustomerLoanViewModel? _viewModel;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public CustomerLoanPage()
        {
            InitializeComponent();

            _viewModel = DataContext as CustomerLoanViewModel;

            // Back button handler | واپس بٹن
            BtnBack.Click += (s, e) => NavigationService?.GoBack();
        }

        // ─── Load Customer Data | قرض دار کا ڈیٹا لوڈ کریں ───────────
        /// <summary>
        /// Loads the selected customer's loan details.
        /// منتخب قرض دار کے قرض کی تفصیلات لوڈ کریں۔
        /// </summary>
        /// <param name="customer">Customer object | قرض دار کا آبجیکٹ</param>
        public void LoadCustomer(Customer customer)
        {
            if (customer == null) return;
            _viewModel?.LoadCustomer(customer);
        }
    }
}