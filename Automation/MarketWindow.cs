using System;
using System.Drawing;

namespace Kazakov_KP_01._01.Automation
{
    public class MarketWindow
    {
        public IntPtr Handle { get; private set; }

        private MarketWindow(IntPtr handle)
        {
            Handle = handle;
        }

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

        public static MarketWindow FindBySize(string processName, int width, int height, int tolerance = 5)
        {
            IntPtr handle = WindowLocator.FindByProcessAndSize(processName, width, height, tolerance);

            if (handle == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Окно процесса '" + processName + "' с размером " + width + "x" + height + " не найдено.");

            return new MarketWindow(handle);
        }

        public bool IsAlive => WindowLocator.IsValid(Handle);

        public Rectangle Bounds => WindowLocator.GetClientBounds(Handle);

        public void Activate() => WindowLocator.RestoreAndFocus(Handle);

        public Point ToScreen(int relativeX, int relativeY) => WindowLocator.WindowToScreen(Handle, relativeX, relativeY);

        public Rectangle ToScreen(Rectangle relativeRegion) => WindowLocator.WindowToScreen(Handle, relativeRegion);
    }
}