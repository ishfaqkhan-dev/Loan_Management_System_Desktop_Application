using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LoanManagementApp.Data;
using LoanManagementApp.Models;
using LoanManagementApp.Views.Dialogs;
using System.Windows;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// CustomerListViewModel / قرض داروں کی فہرست ویو ماڈل
    /// Manages the customers list page with search and filter
    /// تلاش اور فلٹر کے ساتھ قرض داروں کی فہرست پیج
    /// </summary>
    public class CustomerListViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly CustomerRepository _customerRepo;

        // ─── All Customers Cache | تمام قرض دار کیش ─────────────────
        private ObservableCollection<Customer> _allCustomers = new();

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public CustomerListViewModel()
        {
            _customerRepo = new CustomerRepository();
            FilteredCustomers = new ObservableCollection<Customer>();

            LoadCustomersCommand = new RelayCommand(ExecuteLoadCustomers);
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch);
            DeleteCustomerCommand = new RelayCommand(ExecuteDeleteCustomer);

            LoadCustomers();
        }

        // ─── Customers Collection | قرض داروں کا مجموعہ ─────────────
        public ObservableCollection<Customer> FilteredCustomers { get; }

        // ─── Search | تلاش ───────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        // ─── Filter Options | فلٹر اختیارات ──────────────────────────
        private bool _showActiveOnly = false;
        public bool ShowActiveOnly
        {
            get => _showActiveOnly;
            set
            {
                SetProperty(ref _showActiveOnly, value);
                ApplyFilter();
            }
        }

        private bool _showOverdueOnly = false;
        public bool ShowOverdueOnly
        {
            get => _showOverdueOnly;
            set
            {
                SetProperty(ref _showOverdueOnly, value);
                ApplyFilter();
            }
        }

        // ─── Selected Customer | منتخب قرض دار ──────────────────────
        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                SetProperty(ref _selectedCustomer, value);
                OnPropertyChanged(nameof(IsCustomerSelected));
            }
        }
        public bool IsCustomerSelected => SelectedCustomer != null;

        // ─── Count Info | گنتی معلومات ───────────────────────────────
        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                SetProperty(ref _totalCount, value);
                OnPropertyChanged(nameof(CountDisplay));
            }
        }
        private int _filteredCount;
        public int FilteredCount
        {
            get => _filteredCount;
            set
            {
                SetProperty(ref _filteredCount, value);
                OnPropertyChanged(nameof(CountDisplay));
            }
        }
        public string CountDisplay => $"Showing {FilteredCount} of {TotalCount} customers | {TotalCount} میں سے {FilteredCount} قرض دار";

        // ─── Navigation Events | نیویگیشن ایونٹس ────────────────────
        public event Action<Customer>? NavigateToDetail;
        public event Action? NavigateToAdd;

        // ─── Commands | کمانڈز ──────────────────────────────────────
        public ICommand LoadCustomersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        // ─── Load Customers | قرض دار لوڈ کریں ──────────────────────
        public void LoadCustomers()
        {
            try
            {
                IsBusy = true;
                _allCustomers.Clear();
                var list = _customerRepo.GetAllCustomers();
                foreach (var c in list)
                    _allCustomers.Add(c);
                TotalCount = _allCustomers.Count;
                ApplyFilter();
                ClearStatus();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load customers | قرض دار لوڈ ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Apply Filter | فلٹر لگائیں ──────────────────────────────
        private void ApplyFilter()
        {
            FilteredCustomers.Clear();
            var query = _allCustomers.AsEnumerable();

            // Text search | متن تلاش
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(search) ||
                    (c.PhoneNumber1?.ToLower().Contains(search) ?? false) ||
                    (c.AccountNumber?.ToLower().Contains(search) ?? false) ||
                    (c.EmiratesIdOrCNIC?.ToLower().Contains(search) ?? false) ||
                    (c.City?.ToLower().Contains(search) ?? false));
            }

            // Active filter | فعال فلٹر
            if (ShowActiveOnly)
                query = query.Where(c => c.IsActive && c.RemainingBalance > 0);

            // Overdue filter | میعاد ختم فلٹر
            if (ShowOverdueOnly)
                query = query.Where(c => c.IsOverdue);

            foreach (var c in query)
                FilteredCustomers.Add(c);

            FilteredCount = FilteredCustomers.Count;
        }

        // ─── Command Handlers | کمانڈ ہینڈلرز ──────────────────────
        private void ExecuteLoadCustomers(object? _) => LoadCustomers();
        private void ExecuteSearch(object? _) => ApplyFilter();
        private void ExecuteClearSearch(object? _)
        {
            SearchText = string.Empty;
            ShowActiveOnly = false;
            ShowOverdueOnly = false;
        }

        private async void ExecuteDeleteCustomer(object? parameter)
        {
            var customer = parameter as Customer ?? SelectedCustomer;
            if (customer == null) return;

            // Check if loan is fully paid (RemainingBalance == 0)
            if (customer.RemainingBalance > 0)
            {
                var errorDialog = ConfirmationDialog.ShowInfo(
                    "Cannot Delete",
                    $"Customer '{customer.FullName}' has an active loan of {customer.RemainingBalance:N0} PKR.\nPlease clear the loan first.\n\nقرض دار کا فعال قرض ہے۔ پہلے قرض ادا کروائیں۔");
                errorDialog.Owner = Application.Current.MainWindow;
                errorDialog.ShowDialog();
                return;
            }

            // ── PIN Verification | پن تصدیق ──────────────────────────
            var pinDialog = new PinVerificationDialog();
            pinDialog.Owner = Application.Current.MainWindow;
            if (pinDialog.ShowDialog() != true) return;

            var confirm = ConfirmationDialog.AskConfirm(
                "Delete Customer",
                $"Are you sure you want to delete '{customer.FullName}'?\nThis action cannot be undone.\n\nکیا آپ واقعی '{customer.FullName}' کو حذف کرنا چاہتے ہیں؟ یہ عمل واپس نہیں لیا جا سکتا۔",
                isWarning: true,
                confirmText: "🗑️ Yes, Delete | ہاں، حذف کریں",
                cancelText: "Cancel | منسوخ");
            confirm.Owner = Application.Current.MainWindow;

            if (confirm.ShowDialog() != true) return;

            try
            {
                IsBusy = true;
                bool deleted = _customerRepo.DeleteCustomer(customer.Id);
                if (deleted)
                {
                    LoadCustomers();
                }
                else
                {
                    ShowError("Delete failed | حذف ناکام");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Delete error | حذف خرابی: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ─── Navigation Methods | نیویگیشن طریقے ────────────────────
        public void OpenCustomerDetail(Customer customer)
        {
            SelectedCustomer = customer;
            NavigateToDetail?.Invoke(customer);
        }

        public void OpenAddCustomer()
        {
            NavigateToAdd?.Invoke();
        }
    }
}