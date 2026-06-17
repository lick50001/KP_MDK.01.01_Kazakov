using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarketAO.Pages;

namespace MarketAO
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try { this.DragMove(); } catch { }
        }

        private void City_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedBorder = sender as Border;

            if (clickedBorder != null && clickedBorder.Tag != null)
            {
                string cityName = clickedBorder.Tag.ToString();

                AlbionMarketWindow aoM = new AlbionMarketWindow(cityName);
                aoM.Owner = this;
                aoM.Show();

                this.Hide();
            }
        }
    }
}