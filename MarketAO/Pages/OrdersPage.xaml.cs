using MarketAO.Services;
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

namespace MarketAO.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private string _currentCity;
        public OrdersPage(string cityName)
        {
            InitializeComponent();
            _currentCity = cityName;
            LoadOrders(cityName);
        }

        private void LoadOrders(string cityName)
        {
            var db = new DatabaseService();

            //// Обязательно загружаем данные в ОБА списка!
            //BuyOrdersList.ItemsSource = db.GetBuyOrders(_currentCity);
            //SellOrdersList.ItemsSource = db.GetSellOrders(_currentCity);
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            //// Получаем данные ордера, на который нажали
            //var button = sender as Button;
            ////var order = button.DataContext as MarketOrder; // MarketOrder — ваш класс данных

            //if (order != null)
            //{
            //    // Здесь можно открыть новое окно или диалог для ввода новой цены
            //    MessageBox.Show($"Изменение ордера для: {order.ItemName}");
            //}
        }
    }
}
