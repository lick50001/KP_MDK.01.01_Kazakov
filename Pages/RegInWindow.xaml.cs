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
using System.Windows.Shapes;

namespace Kazakov_KP_01._01.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegInWindow.xaml
    /// </summary>
    public partial class RegInWindow : Window
    {
        public RegInWindow()
        {
            InitializeComponent();
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

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();

            mw.Left = this.Left;
            mw.Top = this.Top;

            this.Close();
            mw.Show();
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
