using System;
using System.Drawing;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Представляет открытое окно MarketAO. Все координаты OCR и кликов
    /// задаются относительно этого окна.
    /// </summary>
    public class MarketWindow
    {
        public IntPtr Handle { get; }

        private MarketWindow(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Пытается найти окно MarketAO тремя способами по очереди:
        /// по имени процесса, по Process.MainWindowTitle, и через EnumWindows
        /// (самый надёжный способ, находит окна, которые остальные методы не видят).
        /// </summary>
        public static MarketWindow Find(string processName = "MarketAO", string titleFallback = "MarketAO")
        {
            // Начинаем с самого надёжного метода — EnumWindows точно находит окно,
            // как показала диагностика
            var handle = WindowLocator.FindByTitleEnum(titleFallback);

            if (handle == IntPtr.Zero)
                handle = WindowLocator.FindByProcessName(processName);

            if (handle == IntPtr.Zero)
                handle = WindowLocator.FindByTitle(titleFallback);

            if (handle == IntPtr.Zero)
                throw new InvalidOperationException(
                    $"Окно '{processName}' не найдено. Убедись, что приложение MarketAO запущено.");

            return new MarketWindow(handle);
        }

        public bool IsAlive => WindowLocator.IsValid(Handle);
        public Rectangle Bounds => WindowLocator.GetClientBounds(Handle);
        public void Activate() => WindowLocator.RestoreAndFocus(Handle);
        public Point ToScreen(int relativeX, int relativeY) => WindowLocator.WindowToScreen(Handle, relativeX, relativeY);
        public Rectangle ToScreen(Rectangle relativeRegion) => WindowLocator.WindowToScreen(Handle, relativeRegion);
    }
}