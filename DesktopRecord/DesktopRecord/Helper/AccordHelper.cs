using Accord.Math;
using Accord.Video;
using Accord.Video.FFMPEG;
using System;
using System.Windows;

namespace DesktopRecord.Helper
{
    public class AccordHelper
    {
        static ScreenCaptureStream screenStream;
        static VideoFileWriter videoWriter;
        public static void Start()
        {
            var workArea = SystemParameters.WorkArea.Size;
            var width = (int)workArea.Width;
            var height = (int)workArea.Height;
            var rectangle = new System.Drawing.Rectangle(0, 0, width, height);
            screenStream = new ScreenCaptureStream(rectangle);
            videoWriter = new VideoFileWriter();
            var filePath = $"{Environment.CurrentDirectory}/DesktopRecord_{DateTime.Now.ToString("yyyyMMddHHmmss")}.avi";
            var framerate = new Rational(1000, screenStream.FrameInterval);
            var videoBitRate = 1200 * 1000;
            videoWriter.Open(filePath, width, height, framerate, VideoCodec.MSMPEG4v3, videoBitRate);
            screenStream.FrameInterval = 40;
            screenStream.NewFrame += ScreenStream_NewFrame;
            screenStream.Start();
        }

        private static void ScreenStream_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (videoWriter == null) return;
            videoWriter.WriteVideoFrame(eventArgs.Frame);
        }

        public static void Stop()
        {
            if (screenStream != null)
            {
                screenStream.Stop();
                screenStream = null;
            }
            if (videoWriter != null)
            {
                videoWriter.Close();
                videoWriter.Dispose();
                videoWriter = null;
            }
        }
    }
}
