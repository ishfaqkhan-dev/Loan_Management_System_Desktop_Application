using System.Windows;
using LoanManagementApp.ViewModels;

namespace LoanManagementApp.Views.Dialogs
{
    /// <summary>
    /// AddLoanDialog.xaml.cs | قرض شامل ڈائیلاگ کوڈ بیہائنڈ
    /// Handles dialog result and passes customer context to ViewModel.
    /// </summary>
    public partial class AddLoanDialog : Window
    {
        // ─── ViewModel Reference | ویو ماڈل حوالہ ───────────────────
        private LoanViewModel? _viewModel;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public AddLoanDialog(int customerId, string customerName)
        {
            InitializeComponent();

            _viewModel = DataContext as LoanViewModel;
            if (_viewModel != null)
            {
                _viewModel.InitializeForCustomer(customerId, customerName);

                // When save succeeds, set dialog result to true (refresh caller)
                _viewModel.SaveSucceeded += (loanId) =>
                {
                    DialogResult = true;
                    Close();
                };

                // When cancel, close without refreshing
                _viewModel.Cancelled += () =>
                {
                    DialogResult = false;
                    Close();
                };
            }
        }
    }
}