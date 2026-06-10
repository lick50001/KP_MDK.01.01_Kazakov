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
using Kazakov_KP_01._01.Models;
using Kazakov_KP_01._01.Services;
using Kazakov_KP_01._01.Pages;

namespace Kazakov_KP_01._01.Pages
{
    public partial class EditWindow : Window
    {
        private ApiService _api = new ApiService();
        private int _currentItemId = 0;
        public EditWindow()
        {
            InitializeComponent();
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;

            Title.Content = "Добавление";
            btnAction.Content = "Добавить";

            tb_Name.Text = "";
            tb_Price.Text = "";
            _currentItemId = 0;
        }

        public EditWindow(int itemid)
        {
            InitializeComponent();
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;

            _currentItemId = itemid;
            LoadItems(itemid);
        }

        public async void LoadItems(int id)
        {
            Items itm = await _api.GetItemByIdAsync(id);
            tb_Name.Text = itm.ItemName.ToString();
            tb_Price.Text = itm.MaxBuyPrice.ToString();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private async void ClickToSave(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = tb_Name.Text;
                bool isactive = true;
                string result = "";

                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Введите название предмета!");
                    return;
                }

                if (!int.TryParse(tb_Price.Text, out int price))
                {
                    MessageBox.Show("Введите корректную цену (число)!");
                    return;
                }

                if (_currentItemId == 0)
                    result = await _api.AddItemAsync(name, price, isactive);
                else
                    result = await _api.EditItemAsync(_currentItemId, name, price, isactive);

                if (result == "Success")
                {
                    MessageBox.Show(_currentItemId == 0 ? "Предмет успешно добавлен!" : "Предмет успешно обновлен!");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Ошибка операции: {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
