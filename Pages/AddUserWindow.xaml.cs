using Kazakov_KP_01._01.Services;
using System.Windows;
using System.Windows.Input;

namespace Kazakov_KP_01._01.Pages
{
    public partial class AddUserWindow : Window
    {
        private ApiService _api = new ApiService();

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
            => Close();

        private async void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = tb_Name.Text.Trim();
            var pass = tb_Pass.Password;
            var role = (cb_Role.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "User";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            var result = await _api.RegisterAsync(name, pass, role);
            if (result == "Success")
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show($"Ошибка: {result}");
            }
        }
    }
}