using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Kazakov_KP_01._01.Automation
{
    public static class KeyboardController
    {
        #region SendInput

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

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_SCANCODE = 0x0008;

        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_A = 0x41;
        private const ushort VK_DELETE = 0x2E;
        private const ushort VK_BACK = 0x08;
        private const ushort VK_RETURN = 0x0D;

        #endregion

        #region SendMessage

        [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessageWText(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT p);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        private const uint WM_CHAR = 0x0102;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_SETFOCUS = 0x0007;
        private const uint WM_SETTEXT = 0x000C;

        private const int VK_BACK_INT = 0x08;
        private const int VK_RETURN_INT = 0x0D;

        #endregion

        public static async Task TypeTextDirectAsync(IntPtr hWnd, string text, int delayBetweenCharsMs = 35)
        {
            Random rng = new Random();
            foreach (char c in text)
            {
                SendMessageW(hWnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
                await Task.Delay(delayBetweenCharsMs + rng.Next(-10, 15));
            }
        }

        public static async Task TypeTextAsync(string text, int delayBetweenCharsMs = 35)
        {
            Random rng = new Random();
            foreach (char c in text)
            {
                SendUnicodeChar(c);
                await Task.Delay(delayBetweenCharsMs + rng.Next(-10, 15));
            }
        }

        public static IntPtr GetControlAtScreenPoint(int screenX, int screenY)
        {
            POINT point = new POINT { X = screenX, Y = screenY };
            return WindowFromPoint(point);
        }

        public static void SelectAllAndDeleteDirect(IntPtr hWnd)
        {
            SendMessageW(hWnd, WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
            System.Threading.Thread.Sleep(50);
            KeyDown(VK_CONTROL);
            KeyDown(VK_A);
            KeyUp(VK_A);
            KeyUp(VK_CONTROL);
            System.Threading.Thread.Sleep(50);
            KeyDown(VK_DELETE);
            KeyUp(VK_DELETE);
            System.Threading.Thread.Sleep(30);
        }

        public static void BackspaceDirect(IntPtr hWnd, int times)
        {
            for (int i = 0; i < times; i++)
            {
                SendMessageW(hWnd, WM_KEYDOWN, (IntPtr)VK_BACK_INT, IntPtr.Zero);
                SendMessageW(hWnd, WM_KEYUP, (IntPtr)VK_BACK_INT, IntPtr.Zero);
            }
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

        public static void ClearWithBackspace(int approximateLength)
        {
            for (int i = 0; i < approximateLength + 5; i++)
            {
                KeyDown(VK_BACK);
                KeyUp(VK_BACK);
            }
        }

        public static void PressEnter()
        {
            KeyDown(VK_RETURN);
            KeyUp(VK_RETURN);
        }

        public static void PressEnterDirect(IntPtr hWnd)
        {
            SendMessageW(hWnd, WM_KEYDOWN, (IntPtr)VK_RETURN_INT, IntPtr.Zero);
            SendMessageW(hWnd, WM_KEYUP, (IntPtr)VK_RETURN_INT, IntPtr.Zero);
        }

        private static void SendUnicodeChar(char c)
        {
            INPUT down = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            INPUT up = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = c,
                        dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { down }, Marshal.SizeOf(typeof(INPUT)));
            SendInput(1, new[] { up }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyDown(ushort vk)
        {
            INPUT input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyUp(ushort vk)
        {
            INPUT input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}