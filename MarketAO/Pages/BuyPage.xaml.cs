using MarketAO.Models;
using MarketAO.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MarketAO.Pages
{
    public partial class BuyPage : Page
    {
        private readonly DatabaseService _db = new DatabaseService();
        private string _currentCity;
        private ObservableCollection<MarketItem> _allItems;
        public ObservableCollection<MarketItem> FilteredItems { get; set; }

        public BuyPage(string city)
        {
            InitializeComponent();
            _currentCity = city;
            FilteredItems = new ObservableCollection<MarketItem>();
            BuyList.ItemsSource = FilteredItems;
            LoadBuyData();
        }

        private void LoadBuyData()
        {
            _allItems = _db.GetBuyItems(_currentCity);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allItems == null) return;

            string input = SearchBox.Text.ToLower().Trim();
            string selectedTier = (TierFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            var tierMatch = Regex.Match(input, @"(?<=^|\s)([4-8])(?=\s|$)");
            var enchantMatch = Regex.Match(input, @"(?<=\.|[4-8]\s)([0-4])(?=\s|$)");

            if (tierMatch.Success) selectedTier = "T" + tierMatch.Value;

            string nameForSearch = input;
            if (tierMatch.Success) nameForSearch = nameForSearch.Replace(tierMatch.Value, "");
            if (enchantMatch.Success) nameForSearch = nameForSearch.Replace(enchantMatch.Value, "");

            nameForSearch = nameForSearch.Replace(".", "").Trim();

            var result = _allItems.Where(item =>
            {
                string[] words = nameForSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool nameOk = words.Length == 0 || words.All(w => (item.ItemName ?? "").ToLower().Contains(w));
                bool tierOk = selectedTier == "Все" || $"T{item.TierInt}" == selectedTier;
                return nameOk && tierOk;
            }).ToList();

            FilteredItems.Clear();
            foreach (var it in result) FilteredItems.Add(it);
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as MarketItem;
            if (item == null) return;

            // ПЕРЕДАЕМ ГОРОД _currentCity
            var dialog = new TransactionWindow(item, TransactionMode.Buy, _currentCity, _db)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                LoadBuyData(); // Обновляем рынок после покупки
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();
    }
}
