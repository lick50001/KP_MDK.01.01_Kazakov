using System.Windows.Controls;
using MarketAO.Services;

namespace MarketAO.Pages
{
    public partial class HistoryPage : Page
    {
        private readonly DatabaseService _db = new DatabaseService();
        private string _currentCity;

        public HistoryPage(string city)
        {
            InitializeComponent();
            _currentCity = city;
            LoadData();
        }

        private void LoadData()
        {
            // Получаем данные из БД и привязываем к списку
            ////HistoryList.ItemsSource = _db.GetHistoryItems(_currentCity);
        }
    }
}