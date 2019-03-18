using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace HearMe.Helpers
{
        /// <summary>
        /// APPCOMMAND Keyboard Hook (because some Media Keys get translated into APP_COMMAND Windows messages), 
        /// see: http://stackoverflow.com/questions/14087873/how-to-hook-global-wm-appcommand-message
        /// </summary> 
        internal class GlobalMediaKeyHook
        {
            private static int WM_SHELLHOOKMESSAGE;
            private const int HSHELL_APPCOMMAND = 12;
            private const uint FAPPCOMMAND_MASK = 0xF000;
            private HwndSource source;
            private IntPtr hWnd;

            public enum Command
            {
                APPCOMMAND_MEDIA_NEXTTRACK = 11,
                APPCOMMAND_MEDIA_PAUSE = 47,
                APPCOMMAND_MEDIA_PLAY = 46,
                APPCOMMAND_MEDIA_PLAY_PAUSE = 14,
                APPCOMMAND_MEDIA_PREVIOUSTRACK = 12,
            }

            public event EventHandler MediaKeyNextPressed = delegate { };
            public event EventHandler MediaKeyPreviousPressed = delegate { };
            public event EventHandler MediaKeyPlayPressed = delegate { };

            public GlobalMediaKeyHook(IntPtr hWnd)
            {
                this.hWnd = hWnd;
            }

            public void SetHook()
            {
                if (this.source == null)
                {
                    this.source = HwndSource.FromHwnd(this.hWnd);

                    this.source.AddHook(WndProc);
                    WM_SHELLHOOKMESSAGE = (int)RegisterWindowMessage("SHELLHOOK");

                    RegisterShellHookWindow(this.hWnd);
                }
            }

            public void Unhook()
            {
                if (this.source != null)
                {
                    this.source.RemoveHook(WndProc);

                    if (!this.source.IsDisposed)
                    {
                        DeregisterShellHookWindow(this.hWnd);
                        this.source.Dispose();
                    }

                    this.source = null;
                }
            }

            private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == WM_SHELLHOOKMESSAGE && (int)wParam == HSHELL_APPCOMMAND)
                {
                    var command = GetAppCommandLParam(lParam);

                    switch (command)
                    {
                        case Command.APPCOMMAND_MEDIA_NEXTTRACK:
                            this.MediaKeyNextPressed(this, new EventArgs());
                            break;
                        case Command.APPCOMMAND_MEDIA_PAUSE:
                        case Command.APPCOMMAND_MEDIA_PLAY:
                        case Command.APPCOMMAND_MEDIA_PLAY_PAUSE:
                            this.MediaKeyPlayPressed(this, new EventArgs());
                            break;
                        case Command.APPCOMMAND_MEDIA_PREVIOUSTRACK:
                            this.MediaKeyPreviousPressed(this, new EventArgs());
                            break;
                        default:
                            break;
                    }

                    handled = false;
                }
                return IntPtr.Zero;
            }

            private Command GetAppCommandLParam(IntPtr lParam)
            {
                return (Command)((short)(((ushort)((((uint)lParam.ToInt64()) >> 16) & 0xffff)) & ~FAPPCOMMAND_MASK));
            }

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool RegisterShellHookWindow(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool DeregisterShellHookWindow(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern uint RegisterWindowMessage(string lpString);
        }



}
