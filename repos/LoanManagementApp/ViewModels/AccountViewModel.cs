using LoanManagementApp.Data;
using LoanManagementApp.Helpers;      // ✅ Added for CustomerNotifier
using LoanManagementApp.Models;
using LoanManagementApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// AccountViewModel / اکاؤنٹ ویو ماڈل
    /// Customer list with search/filter and navigation to loan details.
    /// </summary>
    public class AccountViewModel : BaseViewModel
    {
        private readonly CustomerRepository _customerRepo;

        public AccountViewModel()
        {
            _customerRepo = new CustomerRepository();
            AllCustomers = new ObservableCollection<Customer>();
            FilteredCustomers = new ObservableCollection<Customer>();

            LoadCustomersCommand = new RelayCommand(ExecuteLoadCustomers);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            RefreshAllCommand = new RelayCommand(ExecuteRefreshAll);
            ViewAccountCommand = new RelayCommand(ExecuteViewAccount);

            // Subscribe to customer update notifications (from CustomerLoanPage)
            CustomerNotifier.CustomerUpdated += OnCustomerUpdated;

            LoadCustomers();
        }

        ~AccountViewModel()
        {
            CustomerNotifier.CustomerUpdated -= OnCustomerUpdated;
        }

        public ObservableCollection<Customer> AllCustomers { get; }
        public ObservableCollection<Customer> FilteredCustomers { get; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ApplyFilters(); }
        }

        private bool _showActiveOnly = false;
        public bool ShowActiveOnly
        {
            get => _showActiveOnly;
            set { SetProperty(ref _showActiveOnly, value); ApplyFilters(); }
        }

        private bool _showOverdueOnly = false;
        public bool ShowOverdueOnly
        {
            get => _showOverdueOnly;
            set { SetProperty(ref _showOverdueOnly, value); ApplyFilters(); }
        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set { SetProperty(ref _totalCount, value); OnPropertyChanged(nameof(CountDisplay)); }
        }

        private int _filteredCount;
        public int FilteredCount
        {
            get => _filteredCount;
            set { SetProperty(ref _filteredCount, value); OnPropertyChanged(nameof(CountDisplay)); }
        }

        public string CountDisplay => $"Showing {FilteredCount} of {TotalCount} customers | {TotalCount} میں سے {FilteredCount} قرض دار";

        public ICommand LoadCustomersCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand RefreshAllCommand { get; }
        public ICommand ViewAccountCommand { get; }

        public event Action<Customer>? ViewAccountRequested;

        public void LoadCustomers()
        {
            try
            {
                IsBusy = true;
                AllCustomers.Clear();
                var list = _customerRepo.GetAllCustomers();
                foreach (var c in list)
                    AllCustomers.Add(c);
                TotalCount = AllCustomers.Count;
                ApplyFilters();
                ClearStatus();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load customers: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            FilteredCustomers.Clear();
            var query = AllCustomers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(search) ||
                    (c.PhoneNumber1?.ToLower().Contains(search) ?? false) ||
                    (c.AccountNumber?.ToLower().Contains(search) ?? false) ||
                    (c.EmiratesIdOrCNIC?.ToLower().Contains(search) ?? false));
            }

            if (ShowActiveOnly)
                query = query.Where(c => c.IsActive && c.RemainingBalance > 0);

            if (ShowOverdueOnly)
                query = query.Where(c => c.IsOverdue);

            foreach (var c in query)
                FilteredCustomers.Add(c);

            FilteredCount = FilteredCustomers.Count;
        }

        private void ExecuteLoadCustomers(object? _) => LoadCustomers();
        private void ExecuteClearFilters(object? _)
        {
            SearchText = string.Empty;
            ShowActiveOnly = false;
            ShowOverdueOnly = false;
        }
        private void ExecuteRefreshAll(object? _) => LoadCustomers();

        private void ExecuteViewAccount(object? parameter)
        {
            if (parameter is Customer customer)
            {
                ViewAccountRequested?.Invoke(customer);
            }
        }

        private void OnCustomerUpdated(int customerId)
        {
            // Reload the entire customer list to reflect updated status (Closed/Overdue)
            LoadCustomers();
            // Note: No need to reload account details because AccountPage only shows the list.
            // The updated status will appear after the list refresh.
        }
    }
}