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
using Kazakov_KP_01._01.Services;

namespace Kazakov_KP_01._01.Pages
{
    /// <summary>
    /// Логика взаимодействия для FinancePage.xaml
    /// </summary>
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
            FinanceLogsContainer.Children.Clear();

            foreach (var fin in fins)
            {
                var finRow = new Kazakov_KP_01._01.Elements.FinanceLog();

                if (finRow != null)
                {
                    finRow.SetData(
                        fin.EventTime.ToString("HH:mm"),
                        fin.Message,
                        fin.FinanceType
                    );

                    FinanceLogsContainer.Children.Add(finRow);
                }
            }
        }
    }
}
