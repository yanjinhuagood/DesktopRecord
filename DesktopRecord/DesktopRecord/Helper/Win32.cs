using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            Task.Factory.StartNew(() => 
            {
                while (IsRunning)
                {
                    Thread.Sleep(20);
                    num += 1;
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
                            bitmapEncoder.Freeze();
                            var encoder = new JpegBitmapEncoder();
                            encoder.QualityLevel = 50;
                            encoder.Frames.Add(bitmapEncoder);
                            encoder.Save(stream);
                            encoder.Frames.Clear();
                            GC.Collect();
                        }
                    }));
                }
            });
            
            
        }

        public static void ClearRecording()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);
        }
       
        public static void Save(string output)
        {
            try
            {
                output = Path.Combine(basePath, output);
                var imagePaths = Directory.GetFiles(tempDir, "*.jpg", SearchOption.TopDirectoryOnly);
                if (imagePaths.Length == 0) return;

                #region GC不释放，暂时弃用

                //using (var gifFileStream = new FileStream(Output, FileMode.Create))
                //{
                //    var gifBitmapEncoder = new GifBitmapEncoder();
                //    var jpgs = Directory.GetFiles(tempDir, "*.jpg", SearchOption.TopDirectoryOnly);
                //    if (jpgs.Length == 0) return;
                //    foreach (string file in jpgs)
                //    {
                //        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                //        {
                //            var bitmapDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                //            var bitmapFrame = bitmapDecoder.Frames[0];
                //            bitmapDecoder.Frames[0].Freeze();
                //            gifBitmapEncoder.Frames.Add(bitmapFrame);
                //            bitmapFrame = null;
                //            bitmapDecoder = null;
                //            GC.Collect();
                //            stream.Dispose();
                //        }
                //    }
                //    gifBitmapEncoder.Save(gifFileStream);
                //    gifBitmapEncoder.Frames.Clear();
                //    gifBitmapEncoder = null;
                //    GC.Collect();
                //    GC.WaitForPendingFinalizers();
                //} 
                #endregion


                var bitmapFrames = new List<BitmapFrame>();
                foreach (string imagePath in imagePaths)
                {
                    var frame = BitmapFrame.Create(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                    bitmapFrames.Add(frame);
                }
                using (var gifStream = new MemoryStream())
                {
                    using (var encoder = new GifEncoder(gifStream))
                    {
                        foreach (var imagePath in imagePaths)
                        {
                            var image = System.Drawing.Image.FromFile(imagePath);
                            encoder.AddFrame(image, 0, 0, TimeSpan.FromSeconds(0));
                        }
                    }
                    gifStream.Position = 0;
                    using (var fileStream = new FileStream(output, FileMode.Create))
                    {
                        fileStream.Write(gifStream.ToArray(), 0, gifStream.ToArray().Length);
                    }
                }


            }
            catch
            {
                throw;
            }

        }

    }
}
