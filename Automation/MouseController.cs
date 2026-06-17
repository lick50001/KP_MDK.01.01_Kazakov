using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Управление мышью: плавные перемещения по кривой Безье и клики через SendInput.
    /// Поддерживает как абсолютные экранные координаты, так и координаты
    /// относительно окна MarketAO через MarketWindow.
    /// </summary>
    public static class MouseController
    {
        #region Win32

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const int INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        #endregion

        private static readonly Random _rng = new Random();

        /// <summary>
        /// Текущая позиция курсора.
        /// </summary>
        public static Point GetPosition()
        {
            GetCursorPos(out var p);
            return new Point(p.X, p.Y);
        }

        /// <summary>
        /// Мгновенно ставит курсор в точку (без анимации).
        /// </summary>
        public static void SetPosition(int x, int y) => SetCursorPos(x, y);

        /// <summary>
        /// Плавно перемещает курсор из текущей позиции в точку (x, y)
        /// по кривой Безье с небольшим случайным дрожанием.
        /// </summary>
        public static async Task MoveSmoothAsync(int x, int y, int durationMs = 400, int steps = 40)
        {
            var start = GetPosition();
            var end = new Point(x, y);

            double dist = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            double offsetMagnitude = Math.Min(dist * 0.25, 80);

            var control1 = new Point(
                start.X + (int)((end.X - start.X) * 0.3) + RandomOffset(offsetMagnitude),
                start.Y + (int)((end.Y - start.Y) * 0.3) + RandomOffset(offsetMagnitude)
            );

            var control2 = new Point(
                start.X + (int)((end.X - start.X) * 0.7) + RandomOffset(offsetMagnitude),
                start.Y + (int)((end.Y - start.Y) * 0.7) + RandomOffset(offsetMagnitude)
            );

            int stepDelay = Math.Max(1, durationMs / steps);

            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                double easedT = t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

                var point = CubicBezier(start, control1, control2, end, easedT);
                SetCursorPos(point.X, point.Y);

                await Task.Delay(stepDelay);
            }

            SetCursorPos(x, y);
        }

        public static Task MoveSmoothAsync(Point target, int durationMs = 400, int steps = 40)
            => MoveSmoothAsync(target.X, target.Y, durationMs, steps);

        /// <summary>
        /// Перемещает курсор в центр заданного прямоугольника (абсолютные координаты).
        /// </summary>
        public static Task MoveToRegionCenterAsync(Rectangle region, int durationMs = 400, int steps = 40)
        {
            int centerX = region.X + region.Width / 2;
            int centerY = region.Y + region.Height / 2;
            return MoveSmoothAsync(centerX, centerY, durationMs, steps);
        }

        public static async Task LeftClickAsync(int delayBetweenDownUpMs = 60)
        {
            SendMouseInput(MOUSEEVENTF_LEFTDOWN);
            await Task.Delay(delayBetweenDownUpMs);
            SendMouseInput(MOUSEEVENTF_LEFTUP);
        }

        public static async Task RightClickAsync(int delayBetweenDownUpMs = 60)
        {
            SendMouseInput(MOUSEEVENTF_RIGHTDOWN);
            await Task.Delay(delayBetweenDownUpMs);
            SendMouseInput(MOUSEEVENTF_RIGHTUP);
        }

        /// <summary>
        /// Перемещается в точку (x, y) и кликает левой кнопкой (абсолютные координаты).
        /// </summary>
        public static async Task ClickAtAsync(int x, int y, int moveDurationMs = 400, int clickDelayMs = 60)
        {
            await MoveSmoothAsync(x, y, moveDurationMs);
            await Task.Delay(_rng.Next(40, 120));
            await LeftClickAsync(clickDelayMs);
        }

        /// <summary>
        /// Перемещается в центр области и кликает левой кнопкой (абсолютные координаты).
        /// </summary>
        public static async Task ClickRegionAsync(Rectangle region, int moveDurationMs = 400, int clickDelayMs = 60)
        {
            int centerX = region.X + region.Width / 2;
            int centerY = region.Y + region.Height / 2;
            await ClickAtAsync(centerX, centerY, moveDurationMs, clickDelayMs);
        }

        public static async Task DoubleClickAtAsync(int x, int y, int moveDurationMs = 400)
        {
            await MoveSmoothAsync(x, y, moveDurationMs);
            await Task.Delay(_rng.Next(40, 100));
            await LeftClickAsync();
            await Task.Delay(_rng.Next(60, 120));
            await LeftClickAsync();
        }

        // ===== Относительно окна MarketAO (основной способ использования) =====

        /// <summary>
        /// Кликает по точке, заданной координатами относительно окна MarketAO.
        /// </summary>
        public static async Task ClickInWindowAsync(MarketWindow market, int relativeX, int relativeY, int moveDurationMs = 400, int clickDelayMs = 60)
        {
            var screenPoint = market.ToScreen(relativeX, relativeY);
            await ClickAtAsync(screenPoint.X, screenPoint.Y, moveDurationMs, clickDelayMs);
        }

        /// <summary>
        /// Кликает по центру области, заданной координатами относительно окна MarketAO.
        /// </summary>
        public static async Task ClickRegionInWindowAsync(MarketWindow market, Rectangle relativeRegion, int moveDurationMs = 400, int clickDelayMs = 60)
        {
            var screenRegion = market.ToScreen(relativeRegion);
            await ClickRegionAsync(screenRegion, moveDurationMs, clickDelayMs);
        }

        /// <summary>
        /// Перемещает курсор (без клика) в точку относительно окна MarketAO.
        /// </summary>
        public static async Task MoveInWindowAsync(MarketWindow market, int relativeX, int relativeY, int durationMs = 400, int steps = 40)
        {
            var screenPoint = market.ToScreen(relativeX, relativeY);
            await MoveSmoothAsync(screenPoint.X, screenPoint.Y, durationMs, steps);
        }

        #region Внутренние хелперы

        private static void SendMouseInput(uint flags)
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static int RandomOffset(double magnitude)
            => (int)(_rng.NextDouble() * magnitude * 2 - magnitude);

        private static Point CubicBezier(Point p0, Point p1, Point p2, Point p3, double t)
        {
            double u = 1 - t;
            double x = u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X;
            double y = u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y;
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        #endregion
    }
}