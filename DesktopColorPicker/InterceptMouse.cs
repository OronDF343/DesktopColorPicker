using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace DesktopColorPicker
{
    public class InterceptMouse
    {
        private readonly NativeMethods.LowLevelMouseProc _proc;
        private IntPtr _hookId = IntPtr.Zero;
        private bool _hooked;

        public InterceptMouse()
        {
            _proc = HookCallback;
        }

        public void SetHook()
        {
            if (_hooked) throw new InvalidOperationException("Already hooked!");
            _hookId = SetHook(_proc);
            _hooked = true;
        }

        public void Unhook()
        {
            if (!_hooked) throw new InvalidOperationException("Not hooked yet!");
            if (!NativeMethods.UnhookWindowsHookEx(_hookId)) throw new Exception("Failed to unhook!");
            _hooked = false;
        }

        public bool IsHooked => _hooked;

        private static IntPtr SetHook(NativeMethods.LowLevelMouseProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, proc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = (NativeMethods.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MSLLHOOKSTRUCT));
                var m = (NativeMethods.MouseMessages)wParam;
                EventHandler<PointEventArgs> ptev = null;
                switch (m)
                {
                    case NativeMethods.MouseMessages.WM_LBUTTONDOWN:
                        ptev = MouseHookLeftButtonDown;
                        break;
                    case NativeMethods.MouseMessages.WM_MOUSEMOVE:
                        ptev = MouseHookMove;
                        break;
                        // TODO: Implement all messages
                }
                ptev?.Invoke(this, new PointEventArgs(new Point(hookStruct.pt.x, hookStruct.pt.y)));
            }
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public event EventHandler<PointEventArgs> MouseHookLeftButtonDown;
        public event EventHandler<PointEventArgs> MouseHookMove;
    }

    public class PointEventArgs : EventArgs
    {
        public PointEventArgs(Point p)
        {
            Point = p;
        }

        public Point Point { get; }
    }
}
