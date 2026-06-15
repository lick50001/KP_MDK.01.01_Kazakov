using Kazakov_KP_01._01.Elements;
using Kazakov_KP_01._01.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kazakov_KP_01._01.Pages
{
    public partial class Function : Page
    {
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
                    // Если окно для этой функции уже открыто — просто фокусируем его
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