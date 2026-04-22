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

namespace Kazakov_KP_01._01
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void NewReg_Click(object sender, RoutedEventArgs e)
        {
            Pages.RegInWindow RW = new Pages.RegInWindow();

            RW.Left = this.Left;
            RW.Top = this.Top;

            this.Close();
            RW.Show();
        }

        private void ClickToMain(object sender, RoutedEventArgs e)
        {
            Pages.Main mm = new Pages.Main();

            mm.Left = this.Left;
            mm.Top = this.Top;

            this.Close();
            mm.Show();
        }
    }
}
