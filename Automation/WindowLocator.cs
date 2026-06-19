using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kazakov_KP_01._01.Automation
{
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

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        public static IntPtr FindByProcessName(string processName)
        {
            Process proc = Process.GetProcessesByName(processName).FirstOrDefault();
            if (proc == null) return IntPtr.Zero;

            proc.Refresh();
            return proc.MainWindowHandle;
        }

        public static IntPtr FindByTitle(string titleContains)
        {
            foreach (Process proc in Process.GetProcesses())
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

        public static IntPtr FindByTitleEnum(string titleContains)
        {
            IntPtr found = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                StringBuilder sb = new StringBuilder(256);
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

        public static IntPtr FindByProcessAndSize(string processName, int expectedWidth, int expectedHeight, int tolerance = 5)
        {
            IntPtr found = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);

                string procName = "";
                try
                {
                    procName = Process.GetProcessById((int)pid).ProcessName;
                }
                catch
                {
                    return true;
                }

                if (!string.Equals(procName, processName, StringComparison.OrdinalIgnoreCase))
                    return true;

                RECT rect;
                if (!GetClientRect(hWnd, out rect)) return true;

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                bool widthMatch = Math.Abs(width - expectedWidth) <= tolerance;
                bool heightMatch = Math.Abs(height - expectedHeight) <= tolerance;

                if (widthMatch && heightMatch)
                {
                    found = hWnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);

            return found;
        }

        public static List<IntPtr> FindAllByProcessName(string processName)
        {
            List<IntPtr> result = new List<IntPtr>();

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);

                string procName = "";
                try
                {
                    procName = Process.GetProcessById((int)pid).ProcessName;
                }
                catch
                {
                    return true;
                }

                if (string.Equals(procName, processName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(hWnd);
                }

                return true;
            }, IntPtr.Zero);

            return result;
        }

        public static string ListAllVisibleWindows()
        {
            StringBuilder sb = new StringBuilder();

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;

                StringBuilder titleBuilder = new StringBuilder(256);
                GetWindowText(hWnd, titleBuilder, 256);
                string title = titleBuilder.ToString();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    uint pid;
                    GetWindowThreadProcessId(hWnd, out pid);
                    string procName = "?";
                    try
                    {
                        procName = Process.GetProcessById((int)pid).ProcessName;
                    }
                    catch { }

                    RECT rect;
                    string sizeInfo = "?";
                    if (GetClientRect(hWnd, out rect))
                    {
                        sizeInfo = (rect.Right - rect.Left) + "x" + (rect.Bottom - rect.Top);
                    }

                    sb.AppendLine("Процесс: '" + procName + "' (PID " + pid + ")  |  Заголовок: '" + title + "'  |  Размер: " + sizeInfo);
                }

                return true;
            }, IntPtr.Zero);

            return sb.Length > 0 ? sb.ToString() : "Видимых окон с заголовком не найдено.";
        }

        public static bool IsValid(IntPtr hWnd) => hWnd != IntPtr.Zero && IsWindow(hWnd);

        public static Rectangle GetWindowBounds(IntPtr hWnd)
        {
            RECT rect;
            if (!GetWindowRect(hWnd, out rect))
                throw new InvalidOperationException("Не удалось получить координаты окна. Возможно, окно закрыто.");

            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public static Rectangle GetClientBounds(IntPtr hWnd)
        {
            RECT clientRect;
            if (!GetClientRect(hWnd, out clientRect))
                throw new InvalidOperationException("Не удалось получить клиентскую область окна.");

            POINT topLeft = new POINT { X = 0, Y = 0 };
            ClientToScreen(hWnd, ref topLeft);

            return new Rectangle(
                topLeft.X,
                topLeft.Y,
                clientRect.Right - clientRect.Left,
                clientRect.Bottom - clientRect.Top
            );
        }

        public static void RestoreAndFocus(IntPtr hWnd)
        {
            if (IsIconic(hWnd))
                ShowWindow(hWnd, SW_RESTORE);

            IntPtr foreground = GetForegroundWindow();
            uint foregroundThreadId = GetWindowThreadProcessId(foreground, out _);
            uint targetThreadId = GetWindowThreadProcessId(hWnd, out _);
            uint currentThreadId = GetCurrentThreadId();

            bool attached = false;

            if (foregroundThreadId != targetThreadId)
            {
                attached = AttachThreadInput(currentThreadId, targetThreadId, true);
            }

            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);

            if (attached)
            {
                AttachThreadInput(currentThreadId, targetThreadId, false);
            }
        }

        public static Point WindowToScreen(IntPtr hWnd, int relativeX, int relativeY)
        {
            Rectangle clientBounds = GetClientBounds(hWnd);
            return new Point(clientBounds.X + relativeX, clientBounds.Y + relativeY);
        }

        public static Rectangle WindowToScreen(IntPtr hWnd, Rectangle relativeRegion)
        {
            Rectangle clientBounds = GetClientBounds(hWnd);
            return new Rectangle(
                clientBounds.X + relativeRegion.X,
                clientBounds.Y + relativeRegion.Y,
                relativeRegion.Width,
                relativeRegion.Height
            );
        }
    }
}