using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Client.RemoteControl

{
    internal static class InputSimulator
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        // Mouse event flags
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        // Keyboard event flags
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        public static void MoveMouse(int x, int y)
        {
            try
            {
                SetCursorPos(x, y);
                Console.WriteLine($"[INPUT-SIM] Mouse moved to ({x}, {y})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error moving mouse: {ex.Message}");
            }
        }

        public static void MouseLeftDown()
        {
            try
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                Console.WriteLine("[INPUT-SIM] Mouse left down");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void MouseLeftUp()
        {
            try
            {
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                Console.WriteLine("[INPUT-SIM] Mouse left up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void MouseLeftClick()
        {
            MouseLeftDown();
            System.Threading.Thread.Sleep(10);
            MouseLeftUp();
        }

        public static void MouseRightDown()
        {
            try
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
                Console.WriteLine("[INPUT-SIM] Mouse right down");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void MouseRightUp()
        {
            try
            {
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
                Console.WriteLine("[INPUT-SIM] Mouse right up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void MouseRightClick()
        {
            MouseRightDown();
            System.Threading.Thread.Sleep(10);
            MouseRightUp();
        }

        public static void MouseMiddleDown()
        {
            try
            {
                mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);
                Console.WriteLine("[INPUT-SIM] Mouse middle down");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void MouseMiddleUp()
        {
            try
            {
                mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);
                Console.WriteLine("[INPUT-SIM] Mouse middle up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void MouseScroll(int delta)
        {
            try
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, delta, UIntPtr.Zero);
                Console.WriteLine($"[INPUT-SIM] Mouse scroll: {delta}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void KeyDown(int keyCode)
        {
            try
            {
                byte vk = (byte)keyCode;
                keybd_event(vk, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                Console.WriteLine($"[INPUT-SIM] Key down: {keyCode} (VK: 0x{keyCode:X2})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void KeyUp(int keyCode)
        {
            try
            {
                byte vk = (byte)keyCode;
                keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                Console.WriteLine($"[INPUT-SIM] Key up: {keyCode} (VK: 0x{keyCode:X2})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INPUT-SIM] Error: {ex.Message}");
            }
        }

        public static void KeyPress(int keyCode)
        {
            KeyDown(keyCode);
            System.Threading.Thread.Sleep(10);
            KeyUp(keyCode);
        }

        public static void TypeString(string text)
        {
            foreach (char c in text)
            {
                short vkCode = VkKeyScan(c);
                if (vkCode != -1)
                {
                    KeyPress(vkCode & 0xFF);
                    System.Threading.Thread.Sleep(20);
                }
            }
        }

        public static bool IsKeyPressed(int keyCode)
        {
            return (GetAsyncKeyState(keyCode) & 0x8000) != 0;
        }

        public static class VK
        {
            public const int LBUTTON = 0x01;
            public const int RBUTTON = 0x02;
            public const int CANCEL = 0x03;
            public const int MBUTTON = 0x04;
            public const int BACK = 0x08;
            public const int TAB = 0x09;
            public const int CLEAR = 0x0C;
            public const int RETURN = 0x0D;
            public const int SHIFT = 0x10;
            public const int CONTROL = 0x11;
            public const int MENU = 0x12; // ALT key
            public const int PAUSE = 0x13;
            public const int CAPITAL = 0x14;
            public const int ESCAPE = 0x1B;
            public const int SPACE = 0x20;
            public const int PRIOR = 0x21; // PAGE UP
            public const int NEXT = 0x22; // PAGE DOWN
            public const int END = 0x23;
            public const int HOME = 0x24;
            public const int LEFT = 0x25;
            public const int UP = 0x26;
            public const int RIGHT = 0x27;
            public const int DOWN = 0x28;
            public const int SELECT = 0x29;
            public const int PRINT = 0x2A;
            public const int EXECUTE = 0x2B;
            public const int SNAPSHOT = 0x2C; // PRINT SCREEN
            public const int INSERT = 0x2D;
            public const int DELETE = 0x2E;
            public const int HELP = 0x2F;

            // Number keys 0-9
            public const int KEY_0 = 0x30;
            public const int KEY_1 = 0x31;
            public const int KEY_2 = 0x32;
            public const int KEY_3 = 0x33;
            public const int KEY_4 = 0x34;
            public const int KEY_5 = 0x35;
            public const int KEY_6 = 0x36;
            public const int KEY_7 = 0x37;
            public const int KEY_8 = 0x38;
            public const int KEY_9 = 0x39;

            // Letter keys A-Z
            public const int A = 0x41;
            public const int B = 0x42;
            public const int C = 0x43;
            public const int D = 0x44;
            public const int E = 0x45;
            public const int F = 0x46;
            public const int G = 0x47;
            public const int H = 0x48;
            public const int I = 0x49;
            public const int J = 0x4A;
            public const int K = 0x4B;
            public const int L = 0x4C;
            public const int M = 0x4D;
            public const int N = 0x4E;
            public const int O = 0x4F;
            public const int P = 0x50;
            public const int Q = 0x51;
            public const int R = 0x52;
            public const int S = 0x53;
            public const int T = 0x54;
            public const int U = 0x55;
            public const int V = 0x56;
            public const int W = 0x57;
            public const int X = 0x58;
            public const int Y = 0x59;
            public const int Z = 0x5A;

            // Function keys F1-F12
            public const int F1 = 0x70;
            public const int F2 = 0x71;
            public const int F3 = 0x72;
            public const int F4 = 0x73;
            public const int F5 = 0x74;
            public const int F6 = 0x75;
            public const int F7 = 0x76;
            public const int F8 = 0x77;
            public const int F9 = 0x78;
            public const int F10 = 0x79;
            public const int F11 = 0x7A;
            public const int F12 = 0x7B;

            // Numpad keys
            public const int NUMPAD0 = 0x60;
            public const int NUMPAD1 = 0x61;
            public const int NUMPAD2 = 0x62;
            public const int NUMPAD3 = 0x63;
            public const int NUMPAD4 = 0x64;
            public const int NUMPAD5 = 0x65;
            public const int NUMPAD6 = 0x66;
            public const int NUMPAD7 = 0x67;
            public const int NUMPAD8 = 0x68;
            public const int NUMPAD9 = 0x69;
            public const int MULTIPLY = 0x6A;
            public const int ADD = 0x6B;
            public const int SEPARATOR = 0x6C;
            public const int SUBTRACT = 0x6D;
            public const int DECIMAL = 0x6E;
            public const int DIVIDE = 0x6F;
        }
    }
}
