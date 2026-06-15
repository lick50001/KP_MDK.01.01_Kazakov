using Kazakov_KP_01._01.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Kazakov_KP_01._01.Pages
{
    public partial class FunctionWindow : Window
    {
        private FunctionItem _item;

        public FunctionWindow(FunctionItem item)
        {
            InitializeComponent();
            _item = item;

            TxtIcon.Text = _item.Icon;
            TxtTitle.Text = _item.Title;
            TxtDescription.Text = _item.Description;

            UpdateStatusUI();
        }

        private void UpdateStatusUI()
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

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // TODO: здесь будет запуск бота-автоматизации для рынка
            _item.IsRunning = true;
            _item.OnStart?.Invoke();
            UpdateStatusUI();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            // TODO: здесь будет остановка бота-автоматизации
            _item.IsRunning = false;
            _item.OnStop?.Invoke();
            UpdateStatusUI();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
    }
}