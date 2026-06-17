using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Automation
{
    /// <summary>
    /// Ввод текста с клавиатуры. Поддерживает два режима:
    /// 1) SendInput — глобальная эмуляция клавиатуры через систему (работает,
    ///    если окно реально в фокусе ОС).
    /// 2) WM_CHAR через SendMessage — отправка сообщений прямо в хендл окна,
    ///    минуя фокус ОС. Часто надёжнее для WPF/WinForms текстовых полей,
    ///    которые могут игнорировать синтетический SendInput.
    /// </summary>
    public static class KeyboardController
    {
        #region SendInput (глобальный ввод)

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;

        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_A = 0x41;
        private const ushort VK_DELETE = 0x2E;
        private const ushort VK_BACK = 0x08;

        #endregion

        #region SendMessage (прямой ввод в окно)

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT p);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        private const uint WM_CHAR = 0x0102;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_SETFOCUS = 0x0007;

        #endregion

        /// <summary>
        /// Печатает текст через SendInput (глобальный ввод). Требует,
        /// чтобы целевое окно реально было в фокусе ОС.
        /// </summary>
        public static async Task TypeTextAsync(string text, int delayBetweenCharsMs = 35)
        {
            var rng = new Random();

            foreach (char c in text)
            {
                SendUnicodeChar(c);
                await Task.Delay(delayBetweenCharsMs + rng.Next(-10, 15));
            }
        }

        /// <summary>
        /// Печатает текст напрямую в указанное окно через WM_CHAR,
        /// минуя системный фокус. Используй это, если SendInput не работает
        /// для конкретного текстового поля (частая ситуация с кастомными контролами).
        /// </summary>
        /// <param name="hWnd">Хендл окна или конкретного контрола, куда нужно ввести текст</param>
        public static async Task TypeTextDirectAsync(IntPtr hWnd, string text, int delayBetweenCharsMs = 35)
        {
            var rng = new Random();

            foreach (char c in text)
            {
                SendMessage(hWnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
                await Task.Delay(delayBetweenCharsMs + rng.Next(-10, 15));
            }
        }

        /// <summary>
        /// Находит самый "глубокий" дочерний контрол окна в указанной экранной точке.
        /// Полезно, чтобы получить хендл именно текстового поля (а не всего окна),
        /// для точного WM_CHAR-ввода.
        /// </summary>
        public static IntPtr GetControlAtScreenPoint(int screenX, int screenY)
        {
            var point = new POINT { X = screenX, Y = screenY };
            return WindowFromPoint(point);
        }

        public static void SelectAllAndDelete()
        {
            KeyDown(VK_CONTROL);
            KeyDown(VK_A);
            KeyUp(VK_A);
            KeyUp(VK_CONTROL);

            KeyDown(VK_DELETE);
            KeyUp(VK_DELETE);
        }

        /// <summary>
        /// Очистка поля напрямую в хендл контрола через WM_KEYDOWN (Ctrl+A, Delete).
        /// </summary>
        public static void SelectAllAndDeleteDirect(IntPtr hWnd)
        {
            const int VK_A_INT = 0x41;
            const int VK_DELETE_INT = 0x2E;
            const int VK_CONTROL_INT = 0x11;

            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_CONTROL_INT, IntPtr.Zero);
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_A_INT, IntPtr.Zero);
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_A_INT, IntPtr.Zero);
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_CONTROL_INT, IntPtr.Zero);

            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_DELETE_INT, IntPtr.Zero);
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_DELETE_INT, IntPtr.Zero);
        }

        public static void ClearWithBackspace(int approximateLength)
        {
            for (int i = 0; i < approximateLength + 5; i++)
            {
                KeyDown(VK_BACK);
                KeyUp(VK_BACK);
            }
        }

        private static void SendUnicodeChar(char c)
        {
            var down = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = c, dwFlags = KEYEVENTF_UNICODE, time = 0, dwExtraInfo = IntPtr.Zero } }
            };

            var up = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion { ki = new KEYBDINPUT { wVk = 0, wScan = c, dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero } }
            };

            SendInput(1, new[] { down }, Marshal.SizeOf(typeof(INPUT)));
            SendInput(1, new[] { up }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void KeyDown(ushort vk)
        {
            var input = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = 0, dwFlags = 0, time = 0, dwExtraInfo = IntPtr.Zero } } };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void KeyUp(ushort vk)
        {
            var input = new INPUT { type = INPUT_KEYBOARD, U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, wScan = 0, dwFlags = KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero } } };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}