using MarketAO.Models;
using MarketAO.Services;
using System.Windows;
using System.Windows.Controls;

namespace MarketAO.Pages
{
    public partial class SellPage : Page
    {
        private readonly string _cityName;
        private readonly DatabaseService _db = new DatabaseService();

        public SellPage(string cityName)
        {
            InitializeComponent();
            _cityName = cityName;

            // ГАРАНТИЯ ОБНОВЛЕНИЯ ИНВЕНТАРЯ ПРИ ОТКРЫТИИ СТРАНИЦЫ
            this.Loaded += (s, e) => LoadInventory();
        }

        private void LoadInventory()
        {
            InventoryList.ItemsSource = _db.GetInventoryItems(_cityName);
        }

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as MarketItem;
            if (item == null) return;

            var dialog = new TransactionWindow(item, TransactionMode.Sell, _cityName, _db)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                LoadInventory();
            }
        }
    }
}
