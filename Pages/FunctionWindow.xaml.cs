using Kazakov_KP_01._01.Models;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Kazakov_KP_01._01.Pages
{
    public partial class FunctionWindow : Window
    {
        private FunctionItem _item;

        #region Win32 API для глобальных хоткеев F6/F7
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_START_ID = 9006;
        private const int HOTKEY_STOP_ID = 9007;

        private const uint VK_F6 = 0x75;
        private const uint VK_F7 = 0x76;
        private const uint MOD_NONE = 0x0000;

        private IntPtr _windowHandle;
        private HwndSource _source;
        #endregion

        public FunctionWindow(FunctionItem item)
        {
            InitializeComponent();
            _item = item;

            TxtIcon.Text = _item.Icon;
            TxtTitle.Text = _item.Title;
            TxtDescription.Text = _item.Description;
            TxtHotkeys.Text = $"Запуск: {_item.StartKeyHint}   |   Стоп: {_item.StopKeyHint}";

            UpdateStatusUI();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_START_ID, MOD_NONE, VK_F6);
            RegisterHotKey(_windowHandle, HOTKEY_STOP_ID, MOD_NONE, VK_F7);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();

                if (id == HOTKEY_START_ID)
                {
                    StartFunction();
                    handled = true;
                }
                else if (id == HOTKEY_STOP_ID)
                {
                    StopFunction();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void StartFunction()
        {
            if (_item.IsRunning) return;
            _item.IsRunning = true;
            UpdateStatusUI();
            _item.OnStart?.Invoke();
        }

        private void StopFunction()
        {
            if (!_item.IsRunning) return;
            _item.IsRunning = false;
            UpdateStatusUI();
            _item.OnStop?.Invoke();
        }

        public void UpdateStatusUI()
        {
            if (_item.IsRunning)
            {
                StatusBadge.Background = new SolidColorBrush(Color.FromArgb(40, 0, 255, 194));
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 194));
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 194));
                TxtStatus.Text = "Запущено";

                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = true;
            }
            else
            {
                StatusBadge.Background = new SolidColorBrush(Color.FromArgb(40, 255, 77, 77));
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                TxtStatus.Text = "Остановлено";

                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) => StartFunction();
        private void BtnStop_Click(object sender, RoutedEventArgs e) => StopFunction();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

        protected override void OnClosed(EventArgs e)
        {
            _source?.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_START_ID);
            UnregisterHotKey(_windowHandle, HOTKEY_STOP_ID);
            base.OnClosed(e);
        }
    }
}