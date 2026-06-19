using Kazakov_KP_01._01.Automation;
using Kazakov_KP_01._01.Elements;
using Kazakov_KP_01._01.Models;
using Kazakov_KP_01._01.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Kazakov_KP_01._01.Pages
{
    public partial class Function : Page
    {
        private FunctionWindow _currentOpenedWindow;
        private AutomationContext _automationCtx;
        private List<FunctionItem> _functions;

        // Защита от гонки между быстрым Start/Stop
        private readonly object _lifecycleLock = new object();
        private bool _isStarting = false;
        private bool _isStopping = false;
        private Task _runningTask;

        public Function()
        {
            InitializeComponent();

            _functions = new List<FunctionItem>
            {
                new FunctionItem
                {
                    Title = "Авто-скупка предметов",
                    Icon = "🛒",
                    Description = "Проходит по списку предметов, для каждого вводит название в поиск, " +
                                   "распознаёт цену через OCR и покупает, если цена ниже заданного максимума.",
                    OnStart = () => StartBuyBot(),
                    OnStop = () => StopBuyBot()
                }
            };
        }

        private async void StartBuyBot()
        {
            lock (_lifecycleLock)
            {
                // Игнорируем повторный старт, если уже запущено или идёт запуск/останов
                if (_automationCtx != null || _isStarting || _isStopping)
                    return;

                _isStarting = true;
            }

            try
            {
                _automationCtx = new AutomationContext(marketProcessName: "MarketAO", tessDataPath: "./tessdata");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                lock (_lifecycleLock) { _isStarting = false; }
                return;
            }

            var api = new ApiService();
            var items = await api.GetItemAsync();

            var bot = new MarketBuyBot(_automationCtx);
            var ctxRef = _automationCtx; // локальная ссылка, чтобы не зависеть от внешнего обнуления

            _runningTask = Task.Run(async () =>
            {
                try
                {
                    await bot.RunFullCycleAsync(items);
                }
                catch (System.OperationCanceledException)
                {
                    // нормальная остановка через токен — игнорируем
                }
                catch (System.Exception)
                {
                    // глушим прочие исключения из фонового потока, чтобы не крашить процесс
                }
            }, ctxRef.Token);

            lock (_lifecycleLock) { _isStarting = false; }
        }

        private async void StopBuyBot()
        {
            Task taskToWait;
            AutomationContext ctxToDispose;

            lock (_lifecycleLock)
            {
                if (_automationCtx == null || _isStopping)
                    return;

                _isStopping = true;
                ctxToDispose = _automationCtx;
                taskToWait = _runningTask;
                _automationCtx = null; // сразу освобождаем "слот", чтобы Start не подхватил старый ctx
            }

            ctxToDispose.Stop(); // сигнал отмены, текущая итерация в боте должна это увидеть

            if (taskToWait != null)
            {
                try
                {
                    // Ждём, пока фоновый цикл реально завершится, ПЕРЕД Dispose
                    await taskToWait;
                }
                catch { /* уже обработано внутри Task.Run выше */ }
            }

            ctxToDispose.Dispose();

            lock (_lifecycleLock)
            {
                _isStopping = false;
                _runningTask = null;
            }
        }

        private void Function_Loaded(object sender, RoutedEventArgs e)
        {
            RenderFunctions();
        }

        public void RenderFunctions()
        {
            FunctionsContainer.Children.Clear();

            foreach (var func in _functions)
            {
                var card = new FunctionCard(func)
                {
                    Margin = new Thickness(0, 0, 30, 30)
                };

                card.OnOpen += (item) =>
                {
                    if (_currentOpenedWindow != null)
                    {
                        _currentOpenedWindow.Activate();
                        return;
                    }

                    _currentOpenedWindow = new FunctionWindow(item);
                    _currentOpenedWindow.Closed += (s, ev) =>
                    {
                        _currentOpenedWindow = null;
                        card.UpdateStatus();
                    };
                    _currentOpenedWindow.Show();
                };

                FunctionsContainer.Children.Add(card);
            }
        }

        
    }
}