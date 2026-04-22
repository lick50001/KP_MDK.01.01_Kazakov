using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Kazakov_KP_01._01.Pages
{
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();

            MainFrame.Navigate(new DashboardPage());
        }

        private void Nav_Home(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Visible;
            Little_Circle.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new DashboardPage());
            UpdateUI("Btn_Home", "// ГЛАВНАЯ");
        }

        private void Nav_Func(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Visible;
            Little_Circle.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new Function()); 
            UpdateUI("Btn_Func", "// ФУНКЦИИ");
        }

        private void Nav_Price(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Visible;
            Little_Circle.Visibility = Visibility.Hidden;
            MainFrame.Navigate(new Сhecklist());
            UpdateUI("Btn_Price", "// МОНИТОР ЦЕН");
        }

        private void Nav_Finance(object sender, MouseButtonEventArgs e)
        {
            Big_Circle.Visibility = Visibility.Hidden;
            Little_Circle.Visibility = Visibility.Visible;
            MainFrame.Navigate(new FinancePage());
            UpdateUI("Btn_Finance", "// ФИНАНСЫ");
        }

        private void UpdateUI(string btnName, string headerText)
        {
            txtHeader.Text = headerText;

            Btn_Home.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
            Btn_Func.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
            Btn_Price.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
            Btn_Finance.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));

            if (this.FindName(btnName) is TextBlock tb) tb.Foreground = Brushes.White;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    }
}