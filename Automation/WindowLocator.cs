using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Находит окно целевого приложения (MarketAO) по заголовку или процессу
    /// и предоставляет его текущие экранные координаты.
    /// </summary>
    public static class WindowLocator
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        /// <summary>
        /// Находит главное окно процесса по имени (без .exe).
        /// </summary>
        public static IntPtr FindByProcessName(string processName)
        {
            var proc = Process.GetProcessesByName(processName).FirstOrDefault();
            if (proc == null) return IntPtr.Zero;

            proc.Refresh();
            return proc.MainWindowHandle;
        }

        /// <summary>
        /// Находит окно по точному заголовку (title) через Process.MainWindowTitle.
        /// </summary>
        public static IntPtr FindByTitle(string titleContains)
        {
            foreach (var proc in Process.GetProcesses())
            {
                if (proc.MainWindowHandle != IntPtr.Zero &&
                    !string.IsNullOrEmpty(proc.MainWindowTitle) &&
                    proc.MainWindowTitle.IndexOf(titleContains, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return proc.MainWindowHandle;
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Более надёжный поиск — перечисляет ВСЕ окна системы (не только
        /// MainWindowHandle процессов), ищет по заголовку. Находит окна,
        /// которые Process.MainWindowHandle не видит.
        /// </summary>
        public static IntPtr FindByTitleEnum(string titleContains)
        {
            IntPtr found = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                var sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, 256);
                string title = sb.ToString();

                if (!string.IsNullOrEmpty(title) && title.IndexOf(titleContains, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = hWnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);

            return found;
        }

        /// <summary>
        /// Диагностика: возвращает список всех видимых окон в системе с их
        /// заголовками и PID процесса-владельца. Используй для поиска точного
        /// заголовка/процесса MarketAO, если обычный поиск не срабатывает.
        /// </summary>
        public static string ListAllVisibleWindows()
        {
            var sb = new StringBuilder();

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                var titleBuilder = new StringBuilder(256);
                GetWindowText(hWnd, titleBuilder, 256);
                string title = titleBuilder.ToString();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    GetWindowThreadProcessId(hWnd, out uint pid);
                    string procName = "?";
                    try
                    {
                        procName = Process.GetProcessById((int)pid).ProcessName;
                    }
                    catch { }

                    sb.AppendLine($"Процесс: '{procName}' (PID {pid})  |  Заголовок: '{title}'");
                }

                return true;
            }, IntPtr.Zero);

            return sb.Length > 0 ? sb.ToString() : "Видимых окон с заголовком не найдено.";
        }

        /// <summary>
        /// Проверяет, что хендл окна валиден и окно ещё существует.
        /// </summary>
        public static bool IsValid(IntPtr hWnd) => hWnd != IntPtr.Zero && IsWindow(hWnd);

        /// <summary>
        /// Возвращает полные экранные координаты окна (включая рамку и заголовок).
        /// </summary>
        public static Rectangle GetWindowBounds(IntPtr hWnd)
        {
            if (!GetWindowRect(hWnd, out var rect))
                throw new InvalidOperationException("Не удалось получить координаты окна. Возможно, окно закрыто.");

            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        /// <summary>
        /// Возвращает экранные координаты КЛИЕНТСКОЙ области окна
        /// (без рамки, заголовка и системного меню).
        /// </summary>
        public static Rectangle GetClientBounds(IntPtr hWnd)
        {
            if (!GetClientRect(hWnd, out var clientRect))
                throw new InvalidOperationException("Не удалось получить клиентскую область окна.");

            var topLeft = new POINT { X = 0, Y = 0 };
            ClientToScreen(hWnd, ref topLeft);

            return new Rectangle(
                topLeft.X,
                topLeft.Y,
                clientRect.Right - clientRect.Left,
                clientRect.Bottom - clientRect.Top
            );
        }

        /// <summary>
        /// Разворачивает окно если оно свёрнуто и выводит на передний план.
        /// </summary>
        public static void RestoreAndFocus(IntPtr hWnd)
        {
            if (IsIconic(hWnd))
                ShowWindow(hWnd, SW_RESTORE);

            SetForegroundWindow(hWnd);
        }

        /// <summary>
        /// Переводит точку из системы координат окна в абсолютные экранные координаты.
        /// </summary>
        public static Point WindowToScreen(IntPtr hWnd, int relativeX, int relativeY)
        {
            var clientBounds = GetClientBounds(hWnd);
            return new Point(clientBounds.X + relativeX, clientBounds.Y + relativeY);
        }

        /// <summary>
        /// Переводит прямоугольник из системы координат окна в абсолютные экранные координаты.
        /// </summary>
        public static Rectangle WindowToScreen(IntPtr hWnd, Rectangle relativeRegion)
        {
            var clientBounds = GetClientBounds(hWnd);
            return new Rectangle(
                clientBounds.X + relativeRegion.X,
                clientBounds.Y + relativeRegion.Y,
                relativeRegion.Width,
                relativeRegion.Height
            );
        }
    }
}