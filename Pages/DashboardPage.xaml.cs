using Kazakov_KP_01._01.Classes;
using Kazakov_KP_01._01.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Kazakov_KP_01._01.Pages
{
    public partial class DashboardPage : Page
    {
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private DispatcherTimer _timer;
        private DispatcherTimer _financeRefreshTimer;

        public DashboardPage()
        {
            InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                await LoadLogs();
                await LoadFinanceSummary();
            };

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateTimeLabel();

            _financeRefreshTimer = new DispatcherTimer();
            _financeRefreshTimer.Interval = TimeSpan.FromSeconds(30);
            _financeRefreshTimer.Tick += async (s, e) => await LoadFinanceSummary();
            _financeRefreshTimer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTimeLabel();
        }

        private void UpdateTimeLabel()
        {
            TimeSpan ts = _stopwatch.Elapsed;
            lTimeWork.Text = ts.ToString(@"hh\:mm\:ss");
        }

        public async Task LoadLogs()
        {
            ApiService _api = new ApiService();
            var logs = await _api.GetLogAsync();
            LogsContainer.Children.Clear();

            foreach (var log in logs)
            {
                var logRow = new Kazakov_KP_01._01.Elements.Log();
                logRow.SetData(
                    log.EventTime.ToString("HH:mm"),
                    log.Message,
                    log.LogType
                );
                LogsContainer.Children.Add(logRow);
            }
        }

        public async Task LoadFinanceSummary()
        {
            ApiService _api = new ApiService();
            var fins = await _api.GetFinanceLogAsync();

            var summary = FinanceCalculator.Calculate(fins);

            TxtProfit24h.Text = FinanceCalculator.FormatMoneyPlain(summary.Profit24h);
            TxtProfitSession.Text = FinanceCalculator.FormatMoneyPlain(summary.ProfitSession);
        }
    }
}