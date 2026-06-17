using System.Windows;
using System.Windows.Input;
// Убедитесь, что пространство имен совпадает с x:Class в XAML
namespace MarketAO.Pages
{
    public partial class AlbionMarketWindow : Window
    {
        private string _currentCity;
        public AlbionMarketWindow(string cityName)
        {
            InitializeComponent();

            _currentCity = cityName;
            CityTitle.Text = $"{_currentCity} Рынок";

            if (MainFrame != null)
            {
                MainFrame.Navigate(new BuyPage(_currentCity));
            }
        }

        private void NavBuy_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new BuyPage(_currentCity));
        private void NavSell_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SellPage(_currentCity));
        private void NavOrders_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OrdersPage(_currentCity));
        private void NavHistory_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new HistoryPage(_currentCity));
        private void NavCreateOrdersl_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CreateOrdersPage(_currentCity));

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void BackToMap_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Show();
                    break;
                }
            }

            this.Close();
        }
    }
}