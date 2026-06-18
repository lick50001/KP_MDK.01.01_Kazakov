using System;
using System.Drawing;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Представляет открытое окно MarketAO (главное или модальное, например
    /// окно сделки). Все координаты OCR и кликов задаются относительно
    /// конкретного экземпляра этого класса.
    /// </summary>
    public class MarketWindow
    {
        public IntPtr Handle { get; private set; }

        private MarketWindow(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Пытается найти главное окно MarketAO тремя способами по очереди.
        /// </summary>
        public static MarketWindow Find(string processName = "MarketAO", string titleFallback = "MarketAO")
        {
            IntPtr handle = WindowLocator.FindByTitleEnum(titleFallback);

            if (handle == IntPtr.Zero)
                handle = WindowLocator.FindByProcessName(processName);

            if (handle == IntPtr.Zero)
                handle = WindowLocator.FindByTitle(titleFallback);

            if (handle == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Окно '" + processName + "' не найдено. Убедись, что приложение MarketAO запущено.");

            return new MarketWindow(handle);
        }

        /// <summary>
        /// Находит окно того же процесса по характерному размеру клиентской
        /// области (например, окно сделки TransactionWindow 850x450).
        /// Используй, когда у модального окна нет видимого системного
        /// заголовка (WindowStyle="None") и обычный поиск по титулу не работает.
        /// </summary>
        public static MarketWindow FindBySize(string processName, int width, int height, int tolerance = 5)
        {
            IntPtr handle = WindowLocator.FindByProcessAndSize(processName, width, height, tolerance);

            if (handle == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Окно процесса '" + processName + "' с размером " + width + "x" + height + " не найдено.");

            return new MarketWindow(handle);
        }

        /// <summary>
        /// true, если окно всё ещё существует (не было закрыто пользователем).
        /// </summary>
        public bool IsAlive => WindowLocator.IsValid(Handle);

        /// <summary>
        /// Текущие экранные координаты клиентской области окна.
        /// </summary>
        public Rectangle Bounds => WindowLocator.GetClientBounds(Handle);

        /// <summary>
        /// Разворачивает окно (если свёрнуто) и выводит на передний план.
        /// </summary>
        public void Activate() => WindowLocator.RestoreAndFocus(Handle);

        /// <summary>
        /// Переводит точку из системы координат окна в абсолютные экранные координаты.
        /// </summary>
        public Point ToScreen(int relativeX, int relativeY) => WindowLocator.WindowToScreen(Handle, relativeX, relativeY);

        /// <summary>
        /// Переводит прямоугольник из системы координат окна в абсолютные экранные координаты.
        /// </summary>
        public Rectangle ToScreen(Rectangle relativeRegion) => WindowLocator.WindowToScreen(Handle, relativeRegion);
    }
}