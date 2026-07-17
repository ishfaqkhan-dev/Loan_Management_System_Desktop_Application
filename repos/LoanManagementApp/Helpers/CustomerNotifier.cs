using System;

namespace LoanManagementApp.Helpers
{
    /// <summary>
    /// Static notifier for customer data changes.
    /// Any ViewModel can raise this event, and others can subscribe to refresh.
    /// </summary>
    public static class CustomerNotifier
    {
        /// <summary>
        /// Raised when a customer's data (balance, loan status) changes.
        /// </summary>
        public static event Action<int>? CustomerUpdated;

        public static void NotifyCustomerUpdated(int customerId)
        {
            CustomerUpdated?.Invoke(customerId);
        }
    }
}