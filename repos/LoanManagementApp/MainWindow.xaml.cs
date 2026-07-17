using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LoanManagementApp.Models;
using LoanManagementApp.Services;
using LoanManagementApp.ViewModels;
using LoanManagementApp.Views.Pages;

namespace LoanManagementApp
{
    /// <summary>
    /// MainWindow.xaml.cs — Code Behind
    /// مرکزی ونڈو — کوڈ بیہائنڈ
    ///
    /// Responsibilities | ذمہ داریاں:
    /// 1. Show Login overlay — switch to Shell on success
    /// 2. Wire PasswordBox (WPF security requirement)
    /// 3. Handle sidebar navigation — navigate Frame to Pages
    /// 4. Handle Logout — clear user, return to Login
    /// 5. Apply theme changes from SettingsPage
    /// </summary>
    public partial class MainWindow : Window
    {
        // ─── ViewModel | ویو ماڈل ────────────────────────────────────
        private readonly LoginViewModel _loginVm;

        // ─── Page Instances (lazy-created, reused) | پیج انسٹینسز ──
        private DashboardPage? _dashboardPage;
        private CustomersPage? _customersPage;
        private SettingsPage? _settingsPage;

        // ─── Currently active nav button | فعال نیویگیشن بٹن ────────
        private Button? _activeNavBtn;

        // ─── Constructor | کنسٹرکٹر ─────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();

            // Setup LoginViewModel | لاگ ان ویو ماڈل سیٹ کریں
            _loginVm = new LoginViewModel();
            DataContext = _loginVm;

            // Wire events | ایونٹس جوڑیں
            _loginVm.LoginSucceeded += OnLoginSucceeded;

            TxtForgotPassword.MouseLeftButtonDown += (s, e) => OpenForgotPasswordDialog();

            // Wire PasswordBox (can't bind directly in WPF)
            // PasswordBox براہ راست bind نہیں ہو سکتا
            PwdPassword.PasswordChanged += (_, _) =>
                _loginVm.Password = PwdPassword.Password;

            // Wire nav buttons | نیویگیشن بٹن جوڑیں
            BtnNavDashboard.Click += (_, _) => NavigateTo("Dashboard");
            BtnNavCustomers.Click += (_, _) => NavigateTo("Customers");
            BtnNavAccount.Click += (_, _) => NavigateTo("Account");
            BtnNavSettings.Click += (_, _) => NavigateTo("Settings");
            BtnLogout.Click += (_, _) => OnLogoutClicked();

            // Enter key on password = login | پاس ورڈ میں Enter = لاگ ان
            PwdPassword.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter &&
                    _loginVm.LoginCommand.CanExecute(null))
                    _loginVm.LoginCommand.Execute(null);
            };

            // Enter on username = focus password
            TxtUsername.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    PwdPassword.Focus();
            };

            // Show/hide busy indicator via ViewModel
            _loginVm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.IsBusy))
                    TxtLoginBusy.Visibility = _loginVm.IsBusy
                        ? Visibility.Visible : Visibility.Collapsed;

                // Handle status message changes (for login errors)
                if (e.PropertyName == nameof(LoginViewModel.HasStatusMessage) ||
                    e.PropertyName == nameof(LoginViewModel.StatusMessage))
                {
                    UpdateLoginStatus();
                }
            };

            // Focus username field on load
            Loaded += (_, _) => TxtUsername.Focus();
        }

        // ═══════════════════════════════════════════════════════════
        // LOGIN | لاگ ان
        // ═══════════════════════════════════════════════════════════

        private void OnLoginSucceeded()
        {
            string username = AuthService.CurrentUser?.Username ?? "Admin";
            TxtCurrentUser.Text = username;
            TxtUserInitial.Text = username.Length > 0
                ? username[0].ToString().ToUpper() : "A";

            LoginOverlay.Visibility = Visibility.Collapsed;
            ShellGrid.Visibility = Visibility.Visible;

            PwdPassword.Clear();
            _loginVm.ResetForm();
            LoginStatusBorder.Visibility = Visibility.Collapsed;

            // ✅ Make Account button visible after login
            BtnNavAccount.Visibility = Visibility.Visible;

            NavigateTo("Dashboard");
        }

        private void OpenForgotPasswordDialog()
        {
            var dialog = new Views.Dialogs.ForgotPasswordDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void UpdateLoginStatus()
        {
            if (!_loginVm.HasStatusMessage)
            {
                LoginStatusBorder.Visibility = Visibility.Collapsed;
                return;
            }

            bool isSuccess = _loginVm.IsStatusSuccess;
            LoginStatusBorder.Visibility = Visibility.Visible;
            TxtLoginStatus.Text = _loginVm.StatusMessage;

            if (isSuccess)
            {
                LoginStatusBorder.Background =
                    new SolidColorBrush(Color.FromRgb(27, 58, 30));
                LoginStatusBorder.BorderBrush =
                    new SolidColorBrush(Color.FromRgb(39, 103, 73));
                TxtLoginStatus.Foreground =
                    new SolidColorBrush(Color.FromRgb(154, 230, 180));
            }
            else
            {
                LoginStatusBorder.Background =
                    new SolidColorBrush(Color.FromRgb(59, 18, 18));
                LoginStatusBorder.BorderBrush =
                    new SolidColorBrush(Color.FromRgb(197, 48, 48));
                TxtLoginStatus.Foreground =
                    new SolidColorBrush(Color.FromRgb(252, 129, 129));
            }
        }

        // ═══════════════════════════════════════════════════════════
        // NAVIGATION | نیویگیشن
        // ═══════════════════════════════════════════════════════════

        public void NavigateTo(string page, object? parameter = null)
        {
            switch (page)
            {
                case "Dashboard":
                    _dashboardPage ??= new DashboardPage();
                    MainFrame.Navigate(_dashboardPage);
                    SetActiveNav(BtnNavDashboard);
                    break;

                case "Customers":
                    _customersPage ??= new CustomersPage();
                    MainFrame.Navigate(_customersPage);
                    SetActiveNav(BtnNavCustomers);
                    break;

                case "CustomerDetail":
                    if (parameter is Customer customerForDetail)
                    {
                        var detailPage = new CustomerDetailPage();
                        detailPage.LoadForEdit(customerForDetail);
                        detailPage.SaveCompleted += (_) => NavigateTo("Customers");
                        detailPage.Cancelled += () => NavigateTo("Customers");
                        MainFrame.Navigate(detailPage);
                        BtnNavAccount.Visibility = Visibility.Visible;
                        SetActiveNav(null);
                    }
                    break;

                case "Account":
                    // The new AccountPage has its own customer list; just navigate.
                    var accountPage = new AccountPage();
                    MainFrame.Navigate(accountPage);
                    SetActiveNav(BtnNavAccount);
                    break;

                case "Settings":
                    if (_settingsPage == null)
                    {
                        _settingsPage = new SettingsPage();
                        if (_settingsPage.DataContext is SettingsViewModel settingsVm)
                            settingsVm.ThemeChanged += App.ApplyTheme;
                    }
                    MainFrame.Navigate(_settingsPage);
                    SetActiveNav(BtnNavSettings);
                    break;
            }
        }

        private void SetActiveNav(Button? btn)
        {
            if (_activeNavBtn != null)
                _activeNavBtn.Background = Brushes.Transparent;

            _activeNavBtn = btn;

            if (_activeNavBtn != null)
                _activeNavBtn.Background =
                    (Brush)Application.Current.Resources["SidebarActiveBrush"];
        }

        // ═══════════════════════════════════════════════════════════
        // LOGOUT | لاگ آؤٹ
        // ═══════════════════════════════════════════════════════════

        private void OnLogoutClicked()
        {
            var dlg = Views.Dialogs.ConfirmationDialog.AskLogout();
            dlg.Owner = this;

            if (dlg.ShowDialog() != true) return;

            new AuthService().Logout();

            _dashboardPage = null;
            _customersPage = null;
            _settingsPage = null;

            MainFrame.Content = null;
            BtnNavAccount.Visibility = Visibility.Collapsed;
            SetActiveNav(null);

            _loginVm.ResetForm();
            PwdPassword.Clear();

            ShellGrid.Visibility = Visibility.Collapsed;
            LoginOverlay.Visibility = Visibility.Visible;
            TxtUsername.Focus();
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        { WindowState = WindowState.Minimized; }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        { WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        { Close(); }
    }
}