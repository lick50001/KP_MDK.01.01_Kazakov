using Kazakov_KP_01._01.Classes;
using Kazakov_KP_01._01.Models;
using Kazakov_KP_01._01.Services;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Kazakov_KP_01._01.Pages
{
    public partial class Main : Window
    {
        private ApiService _api = new ApiService();
        public Users _currentUser;

        #region Win32 API для фонового отслеживания F6 и F7
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_F6_ID = 9006;
        private const int HOTKEY_F7_ID = 9007;

        private const uint VK_F6 = 0x75; // Код клавиши F6
        private const uint VK_F7 = 0x76; // Код клавиши F7
        private const uint MOD_NONE = 0x0000;

        private IntPtr _windowHandle;
        private HwndSource _source;
        #endregion

        public Main()
        {
            InitializeComponent();
            Loaded += Main_Loaded;
            MainFrame.Navigate(new DashboardPage());
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            // Регистрируем глобальные клавиши в системе Windows
            RegisterHotKey(_windowHandle, HOTKEY_F6_ID, MOD_NONE, VK_F6);
            RegisterHotKey(_windowHandle, HOTKEY_F7_ID, MOD_NONE, VK_F7);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();

                // Если во Frame сейчас открыта страница функций, транслируем нажатие туда
                if (MainFrame.Content is Function functionPage)
                {
                    if (id == HOTKEY_F6_ID)
                    {
                        functionPage.HandleGlobalStart(); // Вызываем старт на странице
                        handled = true;
                    }
                    else if (id == HOTKEY_F7_ID)
                    {
                        functionPage.HandleGlobalStop(); // Вызываем стоп на странице
                        handled = true;
                    }
                }
            }
            return IntPtr.Zero;
        }

        private async void Main_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataUserAsync();
        }

        private async Task LoadDataUserAsync()
        {
            _currentUser = await _api.GetCurrentUserAsync();
            if (_currentUser != null)
            {
                tb_Role.Text = $"{_currentUser.LevelRoot}: {_currentUser.UserName}";
                string role = SessionManager.CurrentRole ?? _currentUser.LevelRoot ?? _currentUser.Level ?? "";
                if (role.ToLower() == "admin")
                    Btn_Admin.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Role.Text = "Не авторизован";
            }
        }

        private void Nav_Home(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Visible;
            Little_Circle.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new DashboardPage());
            UpdateUI("Btn_Home", "// ГЛАВНАЯ");
        }

        private void Nav_Func(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Visible;
            Little_Circle.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new Function());
            UpdateUI("Btn_Func", "// ФУНКЦИИ");
        }

        private void Nav_Price(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Visible;
            Little_Circle.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new Сhecklist());
            UpdateUI("Btn_Price", "// МОНИТОР ЦЕН");
        }

        private void Nav_Finance(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Hidden;
            Little_Circle.Visibility = Visibility.Visible;
            MainFrame.Navigate(new FinancePage());
            UpdateUI("Btn_Finance", "// ФИНАНСЫ");
        }

        private void Nav_Admin(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Visible;
            Little_Circle.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new AdminPage());
            UpdateUI("Btn_Admin", "// АДМИНКА");
        }

        private void UpdateUI(string btnName, string headerText)
        {
            txtHeader.Text = headerText;
            Btn_Home.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
            Btn_Func.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
            Btn_Price.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
            Btn_Finance.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
            Btn_Admin.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));

            if (this.FindName(btnName) is TextBlock tb) tb.Foreground = Brushes.White;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        protected override void OnClosed(EventArgs e)
        {
            // Чистим за собой глобальные хуки при выходе
            _source?.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_F6_ID);
            UnregisterHotKey(_windowHandle, HOTKEY_F7_ID);
            base.OnClosed(e);
        }
    }
}