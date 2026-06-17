using MarketAO.Models;
using MarketAO.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MarketAO.Pages
{
    public enum TransactionMode { Buy, Sell }

    public partial class TransactionWindow : Window
    {
        private readonly MarketItem _item;
        private readonly TransactionMode _mode;
        private readonly DatabaseService _db;
        private readonly string _city;
        private int _quantity = 1;
        private long _pricePerUnit;

        public class MarketOrder
        {
            public long RawPrice { get; set; }
            public string PriceDisplay => RawPrice.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
            public int Amount { get; set; }
        }

        public TransactionWindow(MarketItem item, TransactionMode mode, string city, DatabaseService db = null)
        {
            InitializeComponent();
            _item = item;
            _mode = mode;
            _city = city;
            _db = db;

            string cleanPrice = item.PriceValue?.Replace(" ", "").Replace("\u00A0", "").Replace(",", "").Replace(".", "") ?? "0";
            if (!long.TryParse(cleanPrice, out _pricePerUnit)) _pricePerUnit = item.Price;
            if (_pricePerUnit <= 0) _pricePerUnit = 5000;

            ItemNameText.Text = item.ItemName ?? "—";
            ItemTierText.Text = item.Tier ?? "—";
            PriceBox.Text = _pricePerUnit.ToString();

            if (_mode == TransactionMode.Sell)
            {
                _quantity = item.Quantity > 0 ? item.Quantity : 1;
                TitleText.Text = $"Продать (В инвентаре: {item.Quantity} шт.)";
                ConfirmButton.Content = "Продать";

                TaxGrid.Visibility = Visibility.Visible;
            }
            else
            {
                _quantity = 1;
                TitleText.Text = $"Купить (Доступно: {item.Quantity} шт.)";
                ConfirmButton.Content = "Купить";

                TaxGrid.Visibility = Visibility.Collapsed;
            }

            QuantityBox.Text = _quantity.ToString();
            UpdateTotal();

            LoadCombinedMarketOrders(_pricePerUnit);
        }

        private void LoadCombinedMarketOrders(long basePrice)
        {
            if (_db == null) return;

            Random rnd = new Random();
            var allSellOrders = new List<MarketOrder>();
            var allBuyOrders = new List<MarketOrder>();

            var realMarketItems = _db.GetBuyItems(_city).Where(x => x.ItemId == _item.ItemId).ToList();
            foreach (var mItem in realMarketItems)
            {
                allSellOrders.Add(new MarketOrder
                {
                    RawPrice = mItem.Price,
                    Amount = mItem.Quantity
                });
            }

            long currentSellPrice = basePrice;
            for (int i = 0; i < 20; i++)
            {
                long rise = (long)(currentSellPrice * (0.005 + rnd.NextDouble() * 0.02));
                currentSellPrice += rise;

                allSellOrders.Add(new MarketOrder
                {
                    RawPrice = currentSellPrice,
                    Amount = rnd.Next(1, 99)
                });
            }

            for (int i = 0; i < 20; i++)
            {
                double randomFactor = 0.05 + (Math.Pow(rnd.NextDouble(), 3) * 0.65);
                long randomBuyPrice = (long)(basePrice * randomFactor);
                if (randomBuyPrice < 1) randomBuyPrice = 1;

                allBuyOrders.Add(new MarketOrder
                {
                    RawPrice = randomBuyPrice,
                    Amount = rnd.Next(5, 500)
                });
            }

            allSellOrders = allSellOrders.OrderBy(x => x.RawPrice).ToList();
            allBuyOrders = allBuyOrders.OrderByDescending(x => x.RawPrice).ToList();

            BuyOrdersList.ItemsSource = allSellOrders;
            SellOrdersList.ItemsSource = allBuyOrders;
        }

        private void UpdateTotal()
        {
            if (TotalText == null || TaxText == null) return;

            long subTotal = _pricePerUnit * _quantity;

            if (_mode == TransactionMode.Sell)
            {
                long tax = (long)(subTotal * 0.10);
                TaxText.Text = "- " + tax.ToString("N0");

                TotalLabel.Text = "ПРИБЫЛЬ:";
                TotalText.Text = (subTotal - tax).ToString("N0");
                TotalText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4ECDC4"));
            }
            else
            {
                TotalLabel.Text = "К ОПЛАТЕ:";
                TotalText.Text = subTotal.ToString("N0");
                TotalText.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD060"));
            }
        }

        private void Price_Changed(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                string cleanText = PriceBox.Text.Replace(" ", "").Replace(",", "");
                if (long.TryParse(cleanText, out long p))
                {
                    _pricePerUnit = p < 0 ? 0 : p;
                    UpdateTotal();
                }
            }
        }

        private void Quantity_Changed(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded && int.TryParse(QuantityBox.Text, out int q))
            {
                _quantity = q < 0 ? 0 : q;

                if (_quantity > _item.Quantity)
                {
                    _quantity = _item.Quantity;
                    QuantityBox.Text = _quantity.ToString();
                }

                UpdateTotal();
            }
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            if (_quantity >= _item.Quantity) return;
            _quantity++;
            QuantityBox.Text = _quantity.ToString();
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            if (_quantity > 1) { _quantity--; QuantityBox.Text = _quantity.ToString(); }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            long totalCost = _pricePerUnit * _quantity;
            _item.Price = _pricePerUnit;

            if (_mode == TransactionMode.Buy)
            {
                if (!BalanceService.Instance.CanAfford(totalCost))
                {
                    MessageBox.Show("Недостаточно серебра для завершения сделки!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _db?.BuyItem(_item, _quantity, _city);
            }
            else
            {
                _db?.SellItem(_item, _quantity);
            }

            this.DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}
