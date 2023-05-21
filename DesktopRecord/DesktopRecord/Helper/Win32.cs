using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopRecord.Helper
{
    public class Win32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, System.Int32 dwRop);

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr handle);

        private static string basePath = AppDomain.CurrentDomain.BaseDirectory;


        private static string tempDir = Path.Combine(Path.GetTempPath(), "DesktopRecord");
        private static Thread _thread = null;

        public static bool IsRunning = false;
        static int screenWidth = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
        static int screenHeight = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);

        private static BitmapSource GetCursorIcon()
        {
            var cursorInfo = new CURSORINFO { cbSize = Marshal.SizeOf(typeof(CURSORINFO)) };
            if (GetCursorInfo(out cursorInfo) && cursorInfo.hCursor != IntPtr.Zero)
            {
                try
                {
                    return Imaging.CreateBitmapSourceFromHIcon(cursorInfo.hCursor, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DestroyIcon(cursorInfo.hCursor);
                }
            }

            return null;
        }
        private static BitmapSource CaptureScreen()
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            IntPtr memdc = CreateCompatibleDC(dc);
            IntPtr bitmap = CreateCompatibleBitmap(dc, screenWidth, screenHeight);
            SelectObject(memdc, bitmap);
            BitBlt(memdc, 0, 0, screenWidth, screenHeight, dc, 0, 0, 0xCC0020);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(bitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ReleaseDC(desk, dc);
            return source;
        }

        public static void Start()
        {
            if (_thread == null)
            {
                IsRunning = true;
                _thread = new Thread(Record);
                _thread.Start();
            }
        }
        public static void Stop()
        {
            if (_thread != null)
            {
                IsRunning = false;
                _thread = null;
            }
        }
        private static void Record()
        {
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
            else
            {
                foreach (string file in Directory.GetFiles(tempDir))
                    File.Delete(file);
            }
            int num = 0;
            while (IsRunning)
            {
                num += 1;
                Thread.Sleep(50);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    var drawingVisual = new DrawingVisual();
                    POINT mousePosition;
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(CaptureScreen(),
                            new Rect(new Point(),
                            new Size(screenWidth, screenHeight)));

                        if (GetCursorPos(out mousePosition))
                        {
                            var cursorSize = 30;
                            var cursorHalfSize = cursorSize / 2;
                            var cursorCenterX = mousePosition.X - SystemParameters.VirtualScreenLeft;
                            var cursorCenterY = mousePosition.Y - SystemParameters.VirtualScreenTop;
                            drawingContext.DrawImage(GetCursorIcon(),
                                new Rect(new Point(cursorCenterX, cursorCenterY),
                                new Size(cursorSize, cursorSize)));

                        }
                    }

                    var png = Path.Combine(tempDir, $"{num}.jpg");
                    using (FileStream stream = new FileStream(png, FileMode.Create))
                    {
                        var bitmap = new RenderTargetBitmap((int)screenWidth, (int)screenHeight, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(drawingVisual);
                        var bitmapEncoder = BitmapFrame.Create(bitmap);
                        var encoder = new JpegBitmapEncoder();
                        encoder.QualityLevel = 50;
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(stream);
                        encoder.Frames.Clear();
                        GC.Collect();
                    }
                }));
            }
            
        }

        public static void ClearRecording()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);
        }
       
        public static void Save(string Output)
        {
            try
            {
                Output = Path.Combine(basePath, Output);
                using (var gifFileStream = new FileStream(Output, FileMode.Create))
                {
                    var gifBitmapEncoder = new GifBitmapEncoder();
                    var jpgs = Directory.GetFiles(tempDir, "*.jpg", SearchOption.TopDirectoryOnly);
                    if (jpgs.Length == 0) return;
                    foreach (string file in jpgs)
                    {
                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            decoder.Frames[0].Freeze();
                            gifBitmapEncoder.Frames.Add(decoder.Frames[0]);
                            GC.Collect();
                        }
                    }
                    gifBitmapEncoder.Save(gifFileStream);
                    gifBitmapEncoder.Frames.Clear();
                    gifBitmapEncoder = null;
                    GC.Collect();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }




                //using (var stream = new FileStream(Output, FileMode.Create))
                //{
                //    var encoder = new GifBitmapEncoder();
                //    string[] imageFiles = Directory.GetFiles(tempDir, "*.jpg");
                //    foreach (string imageFile in imageFiles)
                //    {
                //        using (var fileStream = new FileStream(imageFile, FileMode.Open))
                //        {
                //            var bitmap = new BitmapImage();
                //            bitmap.DecodePixelWidth = 200;
                //            bitmap.DecodePixelHeight = 200;
                //            bitmap.BeginInit();
                //            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                //            bitmap.StreamSource = fileStream;
                //            bitmap.EndInit();
                //            var frame = BitmapFrame.Create(bitmap);
                //            encoder.Frames.Add(frame);
                //            frame = null;
                //            bitmap = null;
                //            GC.Collect();
                //            fileStream.Dispose();
                //        }
                //    }
                //    encoder.Save(stream);
                //    encoder.Frames.Clear();
                //    encoder = null;
                //    GC.Collect();
                //    GC.WaitForPendingFinalizers();
                //    encoder = new GifBitmapEncoder();
                //}
            }
            catch
            {
                throw;
            }

        }

    }
}
