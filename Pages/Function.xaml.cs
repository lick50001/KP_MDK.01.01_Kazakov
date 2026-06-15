using Kazakov_KP_01._01.Elements;
using Kazakov_KP_01._01.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kazakov_KP_01._01.Pages
{
    public partial class Function : Page
    {
        // Храним модальное окно активным, чтобы была возможность обновлять его UI из фона
        private FunctionWindow _currentOpenedWindow;

        private List<FunctionItem> _functions = new List<FunctionItem>
        {
            new FunctionItem
            {
                Title = "Авто-скупка предметов",
                Icon = "🛒",
                Description = "Автоматически отслеживает рынок и выкупает предметы по выгодной цене из списка. " +
                               "После запуска работает в фоне и логирует все операции в раздел \"Главная\"."
            }
        };

        public Function()
        {
            InitializeComponent();
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
                    // Открываем через .Show() (немодально), чтобы Frame и вкладки не блокировались!
                    _currentOpenedWindow = new FunctionWindow(item);
                    _currentOpenedWindow.Closed += (s, ev) => { _currentOpenedWindow = null; card.UpdateStatus(); };
                    _currentOpenedWindow.Show();
                };

                FunctionsContainer.Children.Add(card);
            }
        }

        // Метод фонового запуска (вызывается из Main)
        public void HandleGlobalStart()
        {
            var targetFunc = _functions[0]; // Наша первая единственная функция
            if (!targetFunc.IsRunning)
            {
                targetFunc.IsRunning = true;

                // Если открыто окно этой функции — обновляем его кнопки и статус
                _currentOpenedWindow?.UpdateStatusUI();

                // Обновляем саму карточку на странице
                RenderFunctions();

                targetFunc.OnStart?.Invoke();
            }
        }

        // Метод фоновой остановки (вызывается из Main)
        public void HandleGlobalStop()
        {
            var targetFunc = _functions[0];
            if (targetFunc.IsRunning)
            {
                targetFunc.IsRunning = false;

                _currentOpenedWindow?.UpdateStatusUI();
                RenderFunctions();

                targetFunc.OnStop?.Invoke();
            }
        }
    }
}