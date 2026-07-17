using System;
using System.Linq;              // ✅ Added for LINQ in duplicate check
using System.Windows;          // ✅ Added for Application.Current
using System.Windows.Input;
using LoanManagementApp.Data;
using LoanManagementApp.Models;
using LoanManagementApp.Services;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// CustomerDetailViewModel / قرض دار تفصیل ویو ماڈل
    /// Add / Edit customer profile with full validation
    /// مکمل جانچ کے ساتھ قرض دار پروفائل شامل / ترمیم کریں
    /// </summary>
    public class CustomerDetailViewModel : BaseViewModel
    {
        // ─── Dependencies | انحصار ───────────────────────────────────
        private readonly CustomerRepository _customerRepo;
        private readonly ValidationService _validationService;

        // ─── Original loan totals (for edit mode) | اصل قرض کی قدریں (ترمیم موڈ کے لیے)
        private decimal _originalTotalLoanAmount;
        private decimal _originalTotalPaidAmount;
        private decimal _originalRemainingBalance;

        // ─── Active loan flag and remaining balance | فعال قرض کی علامت اور باقی رقم ──
        private bool _hasActiveLoan;
        private decimal _activeLoanRemainingBalance;

        public bool HasActiveLoan
        {
            get => _hasActiveLoan;
            set => SetProperty(ref _hasActiveLoan, value);
        }

        // ─── Mode | موڈ ──────────────────────────────────────────────
        private bool _isEditMode = false;

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                SetProperty(ref _isEditMode, value);
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(PageTitleUrdu));
                OnPropertyChanged(nameof(SaveButtonText));
            }
        }

        public string PageTitle => IsEditMode
            ? "Edit Customer | قرض دار ترمیم"
            : "Add New Customer | نیا قرض دار شامل کریں";

        public string PageTitleUrdu => IsEditMode
            ? "قرض دار ترمیم کریں"
            : "نیا قرض دار شامل کریں";

        public string SaveButtonText => IsEditMode
            ? "Update | اپ ڈیٹ کریں"
            : "Save | محفوظ کریں";

        // ─── Customer ID (for edit) | قرض دار نمبر (ترمیم کے لیے) ──
        private int _customerId = 0;

        // ─── Form Fields | فارم فیلڈز ────────────────────────────────
        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set { SetProperty(ref _fullName, value); ClearStatus(); }
        }

        private string _fatherName = string.Empty;
        public string FatherName
        {
            get => _fatherName;
            set => SetProperty(ref _fatherName, value);
        }

        private string _emiratesIdOrCnic = string.Empty;
        public string EmiratesIdOrCnic
        {
            get => _emiratesIdOrCnic;
            set => SetProperty(ref _emiratesIdOrCnic, value);
        }

        private string _phoneNumber1 = string.Empty;
        public string PhoneNumber1
        {
            get => _phoneNumber1;
            set { SetProperty(ref _phoneNumber1, value); ClearStatus(); }
        }

        private string _phoneNumber2 = string.Empty;
        public string PhoneNumber2
        {
            get => _phoneNumber2;
            set => SetProperty(ref _phoneNumber2, value);
        }

        private string _phoneNumber3 = string.Empty;
        public string PhoneNumber3
        {
            get => _phoneNumber3;
            set => SetProperty(ref _phoneNumber3, value);
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private string _city = string.Empty;
        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        private string _accountNumber = string.Empty;
        public string AccountNumber
        {
            get => _accountNumber;
            set => SetProperty(ref _accountNumber, value);
        }

        private DateTime _loanStartDate = DateTime.Today;
        public DateTime LoanStartDate
        {
            get => _loanStartDate;
            set => SetProperty(ref _loanStartDate, value);
        }

        private DateTime _loanEndDate = DateTime.Today.AddMonths(12);
        public DateTime LoanEndDate
        {
            get => _loanEndDate;
            set => SetProperty(ref _loanEndDate, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        // ─── Loan fields (for add mode) | قرض کے فیلڈز (شامل کرنے کے لیے) ──
        private decimal _loanAmount;
        public decimal LoanAmount
        {
            get => _loanAmount;
            set
            {
                if (SetProperty(ref _loanAmount, value))
                    RecalculateInstallmentAmount();
            }
        }

        private int _totalInstallments;
        public int TotalInstallments
        {
            get => _totalInstallments;
            set
            {
                if (SetProperty(ref _totalInstallments, value))
                    RecalculateInstallmentAmount();
            }
        }

        private decimal _calculatedInstallmentAmount;
        public decimal CalculatedInstallmentAmount
        {
            get => _calculatedInstallmentAmount;
            set => SetProperty(ref _calculatedInstallmentAmount, value);
        }

        // ─── Modify remaining installments fields | باقی اقساط میں تبدیلی ──
        private int _newRemainingInstallments;
        public int NewRemainingInstallments
        {
            get => _newRemainingInstallments;
            set
            {
                if (SetProperty(ref _newRemainingInstallments, value))
                    RecalculateNewInstallmentAmount();
            }
        }

        private decimal _newInstallmentAmount;
        public decimal NewInstallmentAmount
        {
            get => _newInstallmentAmount;
            set => SetProperty(ref _newInstallmentAmount, value);
        }

        // ─── Events | ایونٹس ─────────────────────────────────────────
        public event Action<Customer>? SaveSucceeded;
        public event Action? Cancelled;

        // ─── Commands | کمانڈز ──────────────────────────────────────
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public CustomerDetailViewModel()
        {
            _customerRepo = new CustomerRepository();
            _validationService = new ValidationService();

            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(_ => Cancelled?.Invoke());
        }

        // ─── Recalculate Installment Amount | قسط کی رقم دوبارہ شمار کریں ──
        private void RecalculateInstallmentAmount()
        {
            if (LoanAmount > 0 && TotalInstallments > 0)
                CalculatedInstallmentAmount = Math.Ceiling(LoanAmount / TotalInstallments);
            else
                CalculatedInstallmentAmount = 0;
        }

        // ─── Recalculate New Per‑Installment Amount | نئی فی قسط رقم خودکار ──
        private void RecalculateNewInstallmentAmount()
        {
            if (HasActiveLoan && NewRemainingInstallments > 0 && _activeLoanRemainingBalance > 0)
            {
                NewInstallmentAmount = Math.Ceiling(_activeLoanRemainingBalance / NewRemainingInstallments);
            }
            else
            {
                NewInstallmentAmount = 0;
            }
        }

        // ─── Load For Edit | ترمیم کے لیے لوڈ کریں ─────────────────
        public void LoadCustomer(Customer customer)
        {
            IsEditMode = true;
            _customerId = customer.Id;

            _originalTotalLoanAmount = customer.TotalLoanAmount;
            _originalTotalPaidAmount = customer.TotalPaidAmount;
            _originalRemainingBalance = customer.RemainingBalance;

            FullName = customer.FullName;
            FatherName = customer.FatherName;
            EmiratesIdOrCnic = customer.EmiratesIdOrCNIC;
            PhoneNumber1 = customer.PhoneNumber1;
            PhoneNumber2 = customer.PhoneNumber2;
            PhoneNumber3 = customer.PhoneNumber3;
            Address = customer.Address;
            City = customer.City;
            AccountNumber = customer.AccountNumber;
            LoanStartDate = customer.LoanStartDate;
            LoanEndDate = customer.LoanEndDate;
            Notes = customer.Notes;
            IsActive = customer.IsActive;

            try
            {
                var loanService = new LoanService();
                var activeLoan = loanService.GetActiveLoan(customer.Id);
                if (activeLoan != null)
                {
                    LoanAmount = activeLoan.TotalAmount;
                    TotalInstallments = activeLoan.TotalInstallments;
                    HasActiveLoan = true;
                    _activeLoanRemainingBalance = activeLoan.RemainingAmount;
                }
                else
                {
                    LoanAmount = 0;
                    TotalInstallments = 0;
                    HasActiveLoan = false;
                    _activeLoanRemainingBalance = 0;
                }
            }
            catch
            {
                LoanAmount = 0;
                TotalInstallments = 0;
                HasActiveLoan = false;
                _activeLoanRemainingBalance = 0;
            }
            RecalculateInstallmentAmount();
        }

        // ─── Reset | دوبارہ سیٹ کریں ─────────────────────────────────
        public void ResetForAdd()
        {
            IsEditMode = false;
            _customerId = 0;
            _originalTotalLoanAmount = 0;
            _originalTotalPaidAmount = 0;
            _originalRemainingBalance = 0;

            FullName = string.Empty;
            FatherName = string.Empty;
            EmiratesIdOrCnic = string.Empty;
            PhoneNumber1 = string.Empty;
            PhoneNumber2 = string.Empty;
            PhoneNumber3 = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            AccountNumber = GenerateAccountNumber();
            LoanStartDate = DateTime.Today;
            LoanEndDate = DateTime.Today.AddMonths(12);
            Notes = string.Empty;
            IsActive = true;
            LoanAmount = 0;
            TotalInstallments = 0;
            CalculatedInstallmentAmount = 0;
            HasActiveLoan = false;
            _activeLoanRemainingBalance = 0;
            NewRemainingInstallments = 0;
            NewInstallmentAmount = 0;
            ClearStatus();
        }

        // ─── Execute Save | محفوظ چلائیں (includes remaining installments update) ──
        private void ExecuteSave(object? _)
        {
            try
            {
                IsBusy = true;
                var customer = BuildCustomer();

                var validation = _validationService.ValidateCustomerFull(customer, !IsEditMode);
                if (!validation.IsValid)
                {
                    ShowError(validation.Message);
                    return;
                }

                if (IsEditMode)
                {
                    customer.Id = _customerId;
                    bool updated = _customerRepo.UpdateCustomer(customer);
                    if (updated)
                    {
                        // Update remaining installments if the user entered new values
                        if (HasActiveLoan && NewRemainingInstallments > 0 && NewInstallmentAmount > 0)
                        {
                            var loanService = new LoanService();
                            var activeLoan = loanService.GetActiveLoan(_customerId);
                            if (activeLoan != null)
                            {
                                bool loanUpdated = loanService.UpdateLoanRemainingInstallments(
                                    activeLoan.Id,
                                    NewRemainingInstallments,
                                    NewInstallmentAmount);
                                if (!loanUpdated)
                                    ShowError("Customer updated but loan installments failed.");
                            }
                        }
                        SaveSucceeded?.Invoke(customer);
                    }
                    else
                    {
                        ShowError("Update failed | اپ ڈیٹ ناکام");
                    }
                }
                else
                {
                    // Add new customer
                    if (LoanAmount <= 0)
                    {
                        ShowError("Loan amount is required | قرض کی رقم ضروری ہے");
                        return;
                    }
                    if (TotalInstallments <= 0)
                    {
                        ShowError("Number of installments is required | اقساط کی تعداد ضروری ہے");
                        return;
                    }

                    string cnic = EmiratesIdOrCnic.Trim();
                    if (!string.IsNullOrWhiteSpace(cnic))
                    {
                        string? existingAccount = CheckDuplicateCnic(cnic);
                        if (existingAccount != null)
                        {
                            var duplicateDialog = Views.Dialogs.ConfirmationDialog.ShowInfo(
                                "Duplicate Customer | ڈپلیکیٹ قرض دار",
                                $"A customer with Emirates ID / CNIC '{cnic}' already exists.\n" +
                                $"Their account number is: {existingAccount}\n\n" +
                                $"Cannot add duplicate record.\n\n" +
                                $"ایک قرض دار اس شناختی کارڈ کے ساتھ پہلے سے موجود ہے۔\n" +
                                $"ان کا کھاتہ نمبر ہے: {existingAccount}\n" +
                                $"ڈپلیکیٹ ریکارڈ شامل نہیں کیا جا سکتا۔");
                            duplicateDialog.Owner = Application.Current.MainWindow;
                            duplicateDialog.ShowDialog();
                            return;
                        }
                    }

                    int newId = _customerRepo.AddCustomer(customer);
                    customer.Id = newId;

                    var loanService = new LoanService();
                    decimal installmentAmount = CalculatedInstallmentAmount > 0 ? CalculatedInstallmentAmount : Math.Ceiling(LoanAmount / TotalInstallments);
                    var loan = new Loan
                    {
                        CustomerId = newId,
                        TotalAmount = LoanAmount,
                        TotalInstallments = TotalInstallments,
                        InstallmentAmount = installmentAmount,
                        StartDate = customer.LoanStartDate,
                        EndDate = customer.LoanEndDate,
                        Notes = $"Initial loan for {customer.FullName}"
                    };
                    var loanResult = loanService.AddLoan(loan);
                    if (!loanResult.Success)
                    {
                        ShowError($"Customer saved but loan failed: {loanResult.Message}");
                        return;
                    }

                    SaveSucceeded?.Invoke(customer);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Save failed | محفوظ ناکام: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanExecuteSave(object? _) =>
            !IsBusy &&
            !string.IsNullOrWhiteSpace(FullName) &&
            !string.IsNullOrWhiteSpace(PhoneNumber1);

        // ─── Build Customer | قرض دار بنائیں ────────────────────────
        private Customer BuildCustomer() => new Customer
        {
            FullName = FullName.Trim(),
            FatherName = FatherName.Trim(),
            EmiratesIdOrCNIC = EmiratesIdOrCnic.Trim(),
            PhoneNumber1 = PhoneNumber1.Trim(),
            PhoneNumber2 = PhoneNumber2.Trim(),
            PhoneNumber3 = PhoneNumber3.Trim(),
            Address = Address.Trim(),
            City = City.Trim(),
            AccountNumber = AccountNumber.Trim(),
            LoanStartDate = LoanStartDate,
            LoanEndDate = LoanEndDate,
            Notes = Notes.Trim(),
            IsActive = IsActive,
            UpdatedAt = DateTime.Now,
            TotalLoanAmount = IsEditMode ? _originalTotalLoanAmount : 0,
            TotalPaidAmount = IsEditMode ? _originalTotalPaidAmount : 0,
            RemainingBalance = IsEditMode ? _originalRemainingBalance : 0
        };

        // ─── Generate Account Number | اکاؤنٹ نمبر بنائیں ────────────
        private string GenerateAccountNumber()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            Random rand = new Random();
            string randomDigits = rand.Next(100, 999).ToString();
            return "ACC" + timestamp + randomDigits;
        }

        // ─── Duplicate CNIC Check | ڈپلیکیٹ شناختی کارڈ کی جانچ ─────
        private string? CheckDuplicateCnic(string cnic, int excludeCustomerId = 0)
        {
            if (string.IsNullOrWhiteSpace(cnic)) return null;
            if (_customerRepo.CnicExists(cnic, excludeCustomerId))
            {
                var existingCustomer = _customerRepo.GetAllCustomers()
                    .FirstOrDefault(c => c.EmiratesIdOrCNIC == cnic);
                return existingCustomer?.AccountNumber;
            }
            return null;
        }
    }
}