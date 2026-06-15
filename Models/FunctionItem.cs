using System;
using System.Windows;

namespace Kazakov_KP_01._01.Models
{
    public class FunctionItem
    {
        public string Title { get; set; }
        public string Icon { get; set; } = "⚙";
        public string Description { get; set; }
        public bool IsRunning { get; set; } = false;

        // Клавиши управления для вывода в интерфейс
        public string StartKeyHint { get; set; } = "F5";
        public string StopKeyHint { get; set; } = "F6";

        // Действия по умолчанию выводят сообщения
        public Action OnStart { get; set; } = () => MessageBox.Show("Функция запущена!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
        public Action OnStop { get; set; } = () => MessageBox.Show("Функция остановлена!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}