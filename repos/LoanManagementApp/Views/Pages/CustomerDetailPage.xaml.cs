using System;
using System.Windows;
using System.Windows.Controls;
using LoanManagementApp.Models;
using LoanManagementApp.ViewModels;
using LoanManagementApp.Views.Dialogs;

namespace LoanManagementApp.Views.Pages
{
    /// <summary>
    /// CustomerDetailPage.xaml.cs | قرض دار تفصیل پیج کوڈ بیہائنڈ
    /// Handles add/edit mode switching and save/cancel events
    /// شامل کرنے / ترمیم موڈ اور محفوظ / منسوخ ایونٹس
    /// </summary>
    public partial class CustomerDetailPage : Page
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ───────────────────
        private CustomerDetailViewModel? _viewModel;

        // ─── Events for parent page callback ─────────────────────────
        /// <summary>
        /// Fires when save succeeds | محفوظ کامیاب ہونے پر
        /// </summary>
        public event Action<Customer>? SaveCompleted;

        /// <summary>
        /// Fires when user cancels | منسوخ کرنے پر
        /// </summary>
        public event Action? Cancelled;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public CustomerDetailPage()
        {
            InitializeComponent();

            _viewModel = DataContext as CustomerDetailViewModel;

            if (_viewModel != null)
            {
                // When ViewModel saves successfully, show popup then fire event
                _viewModel.SaveSucceeded += OnSaveSucceeded;

                // When ViewModel cancels, fire our event
                _viewModel.Cancelled += () =>
                {
                    Cancelled?.Invoke();
                    NavigationService?.GoBack();
                };
            }

            // Back button
            BtnBack.Click += (s, e) =>
            {
                Cancelled?.Invoke();
                NavigationService?.GoBack();
            };
        }

        // ─── Save Succeeded Handler | محفوظ کامیاب ہونے پر ──────────
        private void OnSaveSucceeded(Customer customer)
        {
            // Show confirmation popup
            var dialog = ConfirmationDialog.ShowInfo(
                "Success | کامیابی",
                $"Customer '{customer.FullName}' has been added successfully!\n\nقرض دار '{customer.FullName}' کامیابی سے شامل ہو گیا!");
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            // Fire external event
            SaveCompleted?.Invoke(customer);

            // Navigate back to Customers page
            NavigationService?.GoBack();
        }

        // ─── Load for Add Mode | شامل کرنے کے موڈ کے لیے لوڈ ────────
        /// <summary>
        /// Call this to open page in Add New Customer mode
        /// نیا قرض دار موڈ میں پیج کھولنے کے لیے کال کریں
        /// </summary>
        public void LoadForAdd()
        {
            _viewModel?.ResetForAdd();
        }

        // ─── Load for Edit Mode | ترمیم موڈ کے لیے لوڈ ─────────────
        /// <summary>
        /// Call this to open page in Edit mode with existing customer
        /// موجودہ قرض دار کے ساتھ ترمیم موڈ میں پیج کھولنے کے لیے
        /// </summary>
        public void LoadForEdit(Customer customer)
        {
            _viewModel?.LoadCustomer(customer);
        }
    }
}