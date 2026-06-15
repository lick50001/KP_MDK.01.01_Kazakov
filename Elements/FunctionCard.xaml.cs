using Kazakov_KP_01._01.Models;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Kazakov_KP_01._01.Elements
{
    public partial class FunctionCard : UserControl
    {
        private FunctionItem _item;
        public event Action<FunctionItem> OnOpen;

        public FunctionCard(FunctionItem item)
        {
            InitializeComponent();
            _item = item;
            Render();
        }

        private void Render()
        {
            TxtIcon.Text = _item.Icon;
            TxtTitle.Text = _item.Title;
            TxtDescription.Text = _item.Description;
            UpdateStatus();
        }

        public void UpdateStatus()
        {
            if (_item.IsRunning)
            {
                StatusBadge.Background = new SolidColorBrush(Color.FromArgb(40, 0, 255, 194));
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 194));
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 194));
                TxtStatus.Text = "Запущено";
            }
            else
            {
                StatusBadge.Background = new SolidColorBrush(Color.FromArgb(40, 255, 77, 77));
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                TxtStatus.Text = "Остановлено";
            }
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
            => OnOpen?.Invoke(_item);
    }
}