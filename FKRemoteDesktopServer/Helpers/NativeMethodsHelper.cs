﻿using FKRemoteDesktop.DllHook;
using FKRemoteDesktop.Structs;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class NativeMethodsHelper
    {

        public const int LVM_FIRST = 0x1000;
        public const int LVM_SET_ITEM_STATE = LVM_FIRST + 43;
        public const int WM_VSCROLL = 277;

        public static readonly IntPtr SB_PAGE_BOTTOM = new IntPtr(7);

        public static int MakeWin32Long(short wLow, short wHigh)
        {
            return (int)wLow << 16 | (int)(short)wHigh;
        }

        public static void SetItemState(IntPtr handle, int itemIndex, int mask, int value)
        {
            SLvItem lvItem = new SLvItem
            {
                stateMask = mask,
                state = value
            };

            NativeMethods.SendMessageListViewItem(handle, LVM_SET_ITEM_STATE, new IntPtr(itemIndex), ref lvItem);
        }

        public static void ScrollToBottom(IntPtr handle)
        {
            NativeMethods.SendMessage(handle, WM_VSCROLL, SB_PAGE_BOTTOM, IntPtr.Zero);
        }

        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static uint GetLastInputInfoTickCount()
        {
            NativeMethods.LASTINPUTINFO lastInputInfo = new NativeMethods.LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;
            NativeMethods.GetLastInputInfo(ref lastInputInfo);
            return lastInputInfo.dwTime;
        }

        public static void DoMouseLeftClick(Point p, bool isMouseDown)
        {
            NativeMethods.INPUT[] inputs = {
                new NativeMethods.INPUT
                {
                    type = INPUT_MOUSE,
                    u = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT
                        {
                            dx = p.X,
                            dy = p.Y,
                            mouseData = 0,
                            dwFlags = isMouseDown ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP,
                            time = 0,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                }
            };
            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        public static void DoMouseRightClick(Point p, bool isMouseDown)
        {
            NativeMethods.INPUT[] inputs = {
                new NativeMethods.INPUT
                {
                    type = INPUT_MOUSE,
                    u = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT
                        {
                            dx = p.X,
                            dy = p.Y,
                            mouseData = 0,
                            dwFlags = isMouseDown ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP,
                            time = 0,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                }
            };
            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        public static void DoMouseMove(Point p)
        {
            NativeMethods.SetCursorPos(p.X, p.Y);
        }

        public static void DoMouseScroll(Point p, bool scrollDown)
        {
            NativeMethods.INPUT[] inputs = {
                new NativeMethods.INPUT
                {
                    type = INPUT_MOUSE,
                    u = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT
                        {
                            dx = p.X,
                            dy = p.Y,
                            mouseData = scrollDown ? -120 : 120,
                            dwFlags = MOUSEEVENTF_WHEEL,
                            time = 0,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                }
            };
            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        public static void DoKeyPress(byte key, bool keyDown)
        {
            NativeMethods.INPUT[] inputs = {
                new NativeMethods.INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new NativeMethods.InputUnion
                    {
                        ki = new NativeMethods.KEYBDINPUT
                        {
                            wVk = key,
                            wScan = 0,
                            dwFlags = keyDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                }
            };
            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        private const int SPI_GETSCREENSAVERRUNNING = 114;

        public static bool IsScreensaverActive()
        {
            var running = IntPtr.Zero;

            if (!NativeMethods.SystemParametersInfo(
                SPI_GETSCREENSAVERRUNNING,
                0,
                ref running,
                0))
            {
                // Something went wrong (Marshal.GetLastWin32Error)
            }
            return running != IntPtr.Zero;
        }

        private const uint DESKTOP_WRITEOBJECTS = 0x0080;
        private const uint DESKTOP_READOBJECTS = 0x0001;
        private const int WM_CLOSE = 16;
        private const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
        private const uint SPIF_SENDWININICHANGE = 0x0002;

        public static void DisableScreensaver()
        {
            var handle = NativeMethods.OpenDesktop("Screen-saver", 0,
                false, DESKTOP_READOBJECTS | DESKTOP_WRITEOBJECTS);

            if (handle != IntPtr.Zero)
            {
                NativeMethods.EnumDesktopWindows(handle, (hWnd, lParam) =>
                {
                    if (NativeMethods.IsWindowVisible(hWnd))
                        NativeMethods.PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    return true;
                },
                    IntPtr.Zero);
                NativeMethods.CloseDesktop(handle);
            }
            else
            {
                NativeMethods.PostMessage(NativeMethods.GetForegroundWindow(), WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            var dummy = IntPtr.Zero;
            NativeMethods.SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 1 /* TRUE */, ref dummy, SPIF_SENDWININICHANGE);
        }

        public static string GetForegroundWindowTitle()
        {
            StringBuilder sbTitle = new StringBuilder(1024);
            NativeMethods.GetWindowText(NativeMethods.GetForegroundWindow(), sbTitle, sbTitle.Capacity);
            return sbTitle.ToString();
        }
    }
}
