using System;
using System.Windows.Input;

namespace LoanManagementApp.ViewModels
{
    /// <summary>
    /// RelayCommand / ریلے کمانڈ - ICommand implementation for MVVM button bindings
    /// </summary>
    public class RelayCommand : ICommand
    {
        // ─── Private Fields | نجی فیلڈز ─────────────────────────────
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────

        /// <summary>
        /// Create command with execute action | ایکشن کے ساتھ کمانڈ بنائیں
        /// </summary>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute
                ?? throw new ArgumentNullException(nameof(execute),
                    "Execute action cannot be null | ایکشن خالی نہیں ہو سکتا");
            _canExecute = canExecute;
        }

        /// <summary>
        /// Convenience constructor with no parameter | بغیر پیرامیٹر کا کنسٹرکٹر
        /// </summary>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(
                _ => execute(),
                canExecute == null ? null : _ => canExecute())
        { }

        // ─── ICommand Members | ICommand اراکین ─────────────────────

        /// <summary>
        /// CanExecuteChanged event for UI refresh | UI ریفریش کے لیے ایونٹ
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Can command execute now | کیا کمانڈ ابھی چل سکتی ہے
        /// </summary>
        public bool CanExecute(object? parameter) =>
            _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// Execute the command | کمانڈ چلائیں
        /// </summary>
        public void Execute(object? parameter) => _execute(parameter);

        // ─── Manual Refresh | دستی ریفریش ────────────────────────────

        /// <summary>
        /// Force UI to re-evaluate CanExecute | UI کو CanExecute دوبارہ جانچنے پر مجبور کریں
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}