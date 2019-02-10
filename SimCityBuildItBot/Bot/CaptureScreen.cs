using Common.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SimCityBuildItBot.Bot
{
    public class CaptureScreen
    {
        private readonly ILog log;
        public Touch touch;
        public CaptureScreen(ILog log,Touch touch)
        {
            this.log = log;
            this.touch = touch;
        }
        public CaptureScreen(ILog log)
        {
            this.log = log;
            
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public void LeftMouseDown(int xpos, int ypos)
        {

            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
        }
        public void LeftMouseUp(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
        public void LeftMouseMoveTo(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
        }


        public void LeftMouseClick(int xpos, int ypos)
        {
            //SetCursorPos(1298, 290);
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
        public Bitmap SnapShot(int x, int y, Size size,string thing="Unknown")
        {

            // New version by ngocbd


            return touch.Crop(x, y, size.Width, size.Height, thing);




            /*

            Process proc = MemuChooser.GetMemuProcess();
            if (proc == null)
            {
                System.Diagnostics.Debug.WriteLine("Can't find process: ");
                log.Debug("Can't find process: ");
                return null;
            }
           
            Rect rect = new Rect();
            IntPtr error = GetWindowRect(proc.MainWindowHandle, ref rect);
            SetWindowPos(proc.MainWindowHandle, 0, 0, 0, 1320, 754, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
            // adb shell am display-size 1320x754

            if ((rect.bottom - rect.top) != 754)
            {
                log.Debug("height of window must be 754, it is " + (rect.bottom - rect.top));
            }

            if ((rect.right - rect.left) != 1320)
            {
                log.Debug("width of window must be 1320, it is " + ( rect.right - rect.left));
            }

            // sometimes it gives error.
            while (error == (IntPtr)0)
            {
                error = GetWindowRect(proc.MainWindowHandle, ref rect);
            }

            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            Graphics.FromImage(bmp).CopyFromScreen(rect.left + x, rect.top + y, 0, 0, size, CopyPixelOperation.SourceCopy);
            
            bmp.Save(@"E:\botscreensave\"+ thing + ".png", ImageFormat.Png);
            return bmp;*/
        }
    }
}