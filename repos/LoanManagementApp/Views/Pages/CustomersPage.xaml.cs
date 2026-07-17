using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LoanManagementApp.Models;
using LoanManagementApp.ViewModels;
using LoanManagementApp.Views.Dialogs;

namespace LoanManagementApp.Views.Pages
{
    /// <summary>
    /// CustomersPage.xaml.cs | قرض داروں کا پیج کوڈ بیہائنڈ
    /// Handles navigation events, double-click, add/view buttons
    /// نیویگیشن ایونٹس، ڈبل کلک، شامل/دیکھنے کے بٹن
    /// </summary>
    public partial class CustomersPage : Page
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ───────────────────
        private CustomerListViewModel? _viewModel;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public CustomersPage()
        {
            InitializeComponent();

            _viewModel = DataContext as CustomerListViewModel;

            // Wire up ViewModel navigation events
            // ویو ماڈل نیویگیشن ایونٹس کو جوڑیں
            if (_viewModel != null)
            {
                _viewModel.NavigateToDetail += OnNavigateToDetail;
                _viewModel.NavigateToAdd += OnNavigateToAdd;
            }

            // Wire up header Add button
            BtnAddCustomer.Click += BtnAddCustomer_Click;

            // Reload on page shown
            Loaded += CustomersPage_Loaded;
        }

        // ─── Page Loaded | پیج لوڈ ہوا ──────────────────────────────
        private void CustomersPage_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.LoadCustomers();
        }

        // ─── Double Click on Row → View Detail ───────────────────────
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is Customer customer)
                OpenCustomerDetail(customer);
        }

        // ─── View Button Click | دیکھیں بٹن کلک ─────────────────────
        private void BtnViewCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Customer customer)
                OpenCustomerDetail(customer);
        }

        // ─── Add Customer Button Click | قرض دار شامل کریں ──────────
        internal void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            OpenAddCustomerDetail();
        }

        // ─── Navigate to Customer Detail (View/Edit) ─────────────────
        private void OpenCustomerDetail(Customer customer)
        {
            var detailPage = new CustomerDetailPage();
            detailPage.LoadForEdit(customer);
            NavigationService?.Navigate(detailPage);
        }

        // ─── Navigate to Add New Customer ────────────────────────────
        private void OpenAddCustomerDetail()
        {
            var detailPage = new CustomerDetailPage();
            detailPage.LoadForAdd();
            NavigationService?.Navigate(detailPage);
        }

        // ─── ViewModel Navigation Callbacks | ویو ماڈل نیویگیشن ─────
        private void OnNavigateToDetail(Customer customer)
            => OpenCustomerDetail(customer);

        private void OnNavigateToAdd()
            => OpenAddCustomerDetail();
    }
}
