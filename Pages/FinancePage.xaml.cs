using Kazakov_KP_01._01.Classes;
using Kazakov_KP_01._01.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Kazakov_KP_01._01.Pages
{
    public partial class FinancePage : Page
    {
        public FinancePage()
        {
            InitializeComponent();
        }

        private async void FinancePage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadFinanceLogs();
        }

        public async Task LoadFinanceLogs()
        {
            ApiService _api = new ApiService();
            var fins = await _api.GetFinanceLogAsync();

            var summary = FinanceCalculator.Calculate(fins);
            TxtTotalProfit.Text = FinanceCalculator.FormatMoney(summary.TotalProfit);

            FinanceLogsContainer.Children.Clear();

            foreach (var fin in fins.OrderByDescending(f => f.EventTime))
            {
                var finRow = new Kazakov_KP_01._01.Elements.FinanceLog();

                finRow.SetData(
                    fin.EventTime.ToLocalTime().ToString("HH:mm"),
                    $"{fin.Message}  ({FinanceCalculator.FormatMoney(fin.Amount)})",
                    fin.FinanceType
                );

                FinanceLogsContainer.Children.Add(finRow);
            }
        }
    }
}