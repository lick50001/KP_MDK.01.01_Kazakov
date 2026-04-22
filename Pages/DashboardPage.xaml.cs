using Kazakov_KP_01._01.Services;
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

namespace Kazakov_KP_01._01.Pages
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml    
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) => LoadLogs();
        }

        public async Task LoadLogs()
        {
            ApiService _api = new ApiService();
            var logs = await _api.GetLogAsync();
            LogsContainer.Children.Clear();

            foreach (var log in logs)
            {
                var logRow = new Kazakov_KP_01._01.Elements.Log();

                if (logRow != null)
                {
                    logRow.SetData(
                        log.EventTime.ToString("HH:mm"),
                        log.Message,
                        log.LogType
                    );

                    LogsContainer.Children.Add(logRow);
                }
            }
        }
    }
}
