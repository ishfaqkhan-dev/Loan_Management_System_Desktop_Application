using System.Collections.Generic;
using System.Windows;
using LoanManagementApp.Models;

namespace LoanManagementApp.Views.Dialogs
{
    public partial class LoanSelectionDialog : Window
    {
        public Loan? SelectedLoan { get; private set; }

        public LoanSelectionDialog(List<Loan> loans)
        {
            InitializeComponent();
            DataContext = this;
            LoansListBox.ItemsSource = loans;
        }

        public List<Loan> Loans { get; set; } = new List<Loan>();

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedLoan = LoansListBox.SelectedItem as Loan;
            if (SelectedLoan == null)
            {
                MessageBox.Show("Please select a loan first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}