using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoanManagementApp.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool IsNotBusy => !IsBusy;

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isStatusSuccess = true;
        public bool IsStatusSuccess
        {
            get => _isStatusSuccess;
            set => SetProperty(ref _isStatusSuccess, value);
        }

        private bool _hasStatusMessage = false;
        public bool HasStatusMessage
        {
            get => _hasStatusMessage;
            set => SetProperty(ref _hasStatusMessage, value);
        }

        // Made public so SettingsPage can use them
        public void ShowSuccess(string message)
        {
            StatusMessage = message;
            IsStatusSuccess = true;
            IsError = false;
            HasStatusMessage = true;
        }

        public void ShowError(string message)
        {
            StatusMessage = message;
            IsStatusSuccess = false;
            IsError = true;
            HasStatusMessage = true;
        }

        // Changed from protected to public to allow external calls (e.g., from SettingsPage)
        public void ClearStatus()
        {
            StatusMessage = string.Empty;
            HasStatusMessage = false;
        }

        private bool _isError = false;
        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }
    }
}