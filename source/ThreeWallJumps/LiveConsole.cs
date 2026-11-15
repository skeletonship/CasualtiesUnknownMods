using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ThreeWallJumps
{
    // Uses a private CONOUT$ handle without redirecting the game's Console.Out.
    internal sealed class LiveConsole : IDisposable
    {
        private readonly IntPtr output;
        private readonly bool owned;
        private readonly ushort originalColor;
        private readonly object sync = new object();
        private LiveConsole(IntPtr handle, bool own, ushort color)
        { output = handle; owned = own; originalColor = color; }

        internal static LiveConsole Open()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException("The live console is available on Windows.");
            bool own = false;
            if (GetConsoleWindow() == IntPtr.Zero)
            {
                if (!AllocConsole()) throw new Win32Exception(Marshal.GetLastWin32Error());
                own = true;
            }
            IntPtr handle = CreateFileW("CONOUT$", 0xC0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            {
                int error = Marshal.GetLastWin32Error();
                if (own) FreeConsole();
                throw new Win32Exception(error);
            }
            ushort color = 7;
            if (GetConsoleScreenBufferInfo(handle, out ConsoleInfo info)) color = info.Attributes;
            if (own)
            {
                IntPtr window = GetConsoleWindow();
                IntPtr menu = GetSystemMenu(window, false);
                // Prevent a console close event from terminating the game process.
                if (menu != IntPtr.Zero) DeleteMenu(menu, 0xF060, 0);
                DrawMenuBar(window);
            }
            return new LiveConsole(handle, own, color);
        }

        internal void Write(string message, ushort color)
        {
            lock (sync)
            {
                SetConsoleTextAttribute(output, color);
                try
                {
                    string line = message.Replace("\n", "\r\n") + "\r\n";
                    if (!WriteConsoleW(output, line, (uint)line.Length, out uint written, IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally { SetConsoleTextAttribute(output, originalColor); }
            }
        }
        internal void SetStatus(string status)
        { if (owned) SetConsoleTitleW("Three Wall Jumps " + Plugin.Version + " | " + status); }
        public void Dispose()
        { CloseHandle(output); if (owned) FreeConsole(); }

        [StructLayout(LayoutKind.Sequential)] private struct Coord { public short X, Y; }
        [StructLayout(LayoutKind.Sequential)] private struct SmallRect { public short Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] private struct ConsoleInfo
        { public Coord Size, CursorPosition; public ushort Attributes; public SmallRect Window; public Coord MaximumSize; }
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool AllocConsole();
        [DllImport("kernel32.dll")] private static extern bool FreeConsole();
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)] private static extern IntPtr CreateFileW(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)] private static extern bool WriteConsoleW(IntPtr handle, string text, uint length, out uint written, IntPtr reserved);
        [DllImport("kernel32.dll")] private static extern bool SetConsoleTextAttribute(IntPtr handle, ushort attributes);
        [DllImport("kernel32.dll")] private static extern bool GetConsoleScreenBufferInfo(IntPtr handle, out ConsoleInfo info);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)] private static extern bool SetConsoleTitleW(string title);
        [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
        [DllImport("user32.dll")] private static extern IntPtr GetSystemMenu(IntPtr window, bool revert);
        [DllImport("user32.dll")] private static extern bool DeleteMenu(IntPtr menu, uint position, uint flags);
        [DllImport("user32.dll")] private static extern bool DrawMenuBar(IntPtr window);
    }
}
