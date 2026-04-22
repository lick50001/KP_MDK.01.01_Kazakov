using Kazakov_KP_01._01.Classes;
using Kazakov_KP_01._01.Services;
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
        private readonly ApiService _api = new ApiService();
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

        private async void ClickToMain(object sender, RoutedEventArgs e)
        {
            string login = tb_login.Text;
            string pass = tb_password.Text;

            string result = await _api.RegisterAsync(login, pass);
            if (result != "Success")
            {
                string finalError = string.IsNullOrEmpty(result) ? "Ошибка регистрации" : result;
                MessageBox.Show(finalError);
                return;
            }

            MainWindow mw = new MainWindow();
            mw.Left = this.Left;
            mw.Top = this.Top;
            mw.Show();
            this.Close();
        }
    }
}
