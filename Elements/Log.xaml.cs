using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kazakov_KP_01._01.Elements
{
    /// <summary>
    /// Логика взаимодействия для Log.xaml
    /// </summary>
    public partial class Log : UserControl
    {
        public Log()
        {
            InitializeComponent();
            this.MouseEnter += (s, e) => this.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
            this.MouseLeave += (s, e) => this.Background = Brushes.Transparent;
        }
        public void SetData(string time, string message, string logType)
        {
            txtTime.Text = time ?? "--:--:--";
            txtMessage.Text = message ?? "(нет сообщения)";
            txtType.Text = logType ?? "info";

            SolidColorBrush color;
            switch (logType != null ? logType.ToLower() : "")
            {
                case "error":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4B4B"));
                    break;
                case "warning":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA500"));
                    break;
                case "success":
                    color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFC2"));
                    break;
                default:
                    color = new SolidColorBrush(Colors.White);
                    break;
            }
            txtType.Foreground = color;
        }
    }
}
