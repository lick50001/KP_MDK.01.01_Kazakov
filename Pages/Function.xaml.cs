using Kazakov_KP_01._01.Elements;
using Kazakov_KP_01._01.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kazakov_KP_01._01.Pages
{
    public partial class Function : Page
    {
        // Список функций. Для расширения — просто добавь новый элемент сюда.
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

        private void RenderFunctions()
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
                    var win = new FunctionWindow(item);
                    win.ShowDialog();
                    card.UpdateStatus(); // обновляем бейдж после закрытия окна
                };

                FunctionsContainer.Children.Add(card);
            }
        }
    }
}