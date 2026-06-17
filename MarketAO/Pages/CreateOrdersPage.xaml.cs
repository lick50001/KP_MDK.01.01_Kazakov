using MarketAO.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MarketAO.Pages
{
    public partial class CreateOrdersPage : Page
    {
        private List<GameItem> _fullItemsList;
        public ObservableCollection<GameItem> FilteredItems { get; set; }

        public CreateOrdersPage(string cityName)
        {
            InitializeComponent();

            _fullItemsList = new List<GameItem>
            {
                new GameItem { ItemName = "Лук Барона", Category = "Луки", Tier = 8, Enchantment = 3, IconPath = "https://render.albiononline.com/v1/item/T8_BOW@3.png" },
                new GameItem { ItemName = "Колпак Ученого", Category = "Тканевая броня", Tier = 4, Enchantment = 0, IconPath = "https://render.albiononline.com/v1/item/T4_HEAD_CLOTH_SET1.png" },
                new GameItem { ItemName = "Яд", Category = "Эликсиры", Tier = 6, Enchantment = 1, IconPath = "https://render.albiononline.com/v1/item/T6_POTION_COOLDOWN@1.png" }
            };

            FilteredItems = new ObservableCollection<GameItem>(_fullItemsList);

            AllItemsList.ItemsSource = FilteredItems;
        }

        private void ApplyFilter()
        {
            if (_fullItemsList == null || SearchBox == null || TierFilter == null || EnchantmentFilter == null)
                return;

            string input = SearchBox.Text.ToLower().Trim();

            string selectedTier = (TierFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedEnchant = (EnchantmentFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            var tierMatch = Regex.Match(input, @"(?<=^|\s)([4-8])(?=\s|$)");
            var enchantMatch = Regex.Match(input, @"(?<=\.|[4-8]\s)([0-4])(?=\s|$)");

            if (tierMatch.Success)
            {
                selectedTier = "T" + tierMatch.Value;
            }
            if (enchantMatch.Success)
            {
                selectedEnchant = "." + enchantMatch.Value;
            }

            string nameForSearch = input;
            if (tierMatch.Success) nameForSearch = nameForSearch.Replace(tierMatch.Value, "");
            if (enchantMatch.Success) nameForSearch = nameForSearch.Replace(enchantMatch.Value, "");
            nameForSearch = nameForSearch.Replace(".", "").Trim();

            var result = _fullItemsList.Where(item =>
            {
                string[] searchWords = nameForSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool nameMatches = searchWords.Length == 0 || searchWords.All(word => item.ItemName.ToLower().Contains(word));

                bool tierMatches = (selectedTier == "Все" || item.TierString.Split('.')[0] == selectedTier);

                bool enchantMatches = (selectedEnchant == "Все" || $".{item.Enchantment}" == selectedEnchant);

                return nameMatches && tierMatches && enchantMatches;
            }).ToList();

            FilteredItems.Clear();
            foreach (var item in result)
            {
                FilteredItems.Add(item);
            }
        }

        private void ResetToComboBoxFilters()
        {
            string selectedTier = (TierFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedEnchant = (EnchantmentFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            var result = _fullItemsList.Where(item =>
                (selectedTier == "Все" || item.TierString.StartsWith(selectedTier)) &&
                (selectedEnchant == "Все" || $".{item.Enchantment}" == selectedEnchant)
            ).ToList();

            FilteredItems.Clear();
            foreach (var item in result) FilteredItems.Add(item);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }
    }
}