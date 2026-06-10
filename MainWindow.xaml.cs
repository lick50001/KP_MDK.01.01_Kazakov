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
using Kazakov_KP_01._01.Classes;
using Kazakov_KP_01._01.Services;

namespace Kazakov_KP_01._01
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApiService _api = new ApiService();
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

        private async void ClickToMain(object sender, RoutedEventArgs e)
        {
            string login = tb_login.Text;
            string pass = pb_password.Password;

            string token = await _api.LoginAsync(login, pass);
            if (token == null)
            {
                MessageBox.Show("Неверный логин или пароль!");
                return;
            }
            SessionManager.Token = token;

            Pages.Main mm = new Pages.Main();
            mm.Left = this.Left;
            mm.Top = this.Top;
            this.Close();
            mm.Show();
        }

        private void Pb_Password_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder(sender as PasswordBox, true);
        }

        private void Pb_Password_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder(sender as PasswordBox, false);
        }

        private void Pb_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb != null && !pb.IsFocused)
            {
                UpdatePasswordPlaceholder(pb, false);
            }
        }

        private void UpdatePasswordPlaceholder(PasswordBox passwordBox, bool isFocused)
        {
            if (passwordBox == null) return;

            var placeholder = passwordBox.Template.FindName("PlaceholderText", passwordBox) as System.Windows.Controls.TextBlock;

            if (placeholder != null)
            {
                if (isFocused)
                {
                    placeholder.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (string.IsNullOrEmpty(passwordBox.Password))
                    {
                        placeholder.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        placeholder.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
