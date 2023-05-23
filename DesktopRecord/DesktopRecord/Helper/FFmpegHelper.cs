using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DesktopRecord.Helper
{
    public class FFmpegHelper
    {
        #region 模拟控制台信号需要使用的api

        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        #endregion
        // ffmpeg进程
        static Process _process;

        // ffmpeg.exe实体文件路径
        static string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

        /// <summary>
        /// 功能: 开始录制
        /// </summary>
        public static bool Start()
        {
            if(!File.Exists(ffmpegPath))
                return false;
            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = "-f gdigrab -framerate 30 -offset_x 0 -offset_y 0 -video_size 1920x1080 -i desktop -c:v libx264 -preset ultrafast -crf 0 " + DateTime.Now.ToString("yyyyMMddHHmmss") + "_DesktopRecord.mp4",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            _process = new Process { StartInfo = processInfo };
            _process.Start();
            return true;
        }

        /// <summary>
        /// 功能: 停止录制
        /// </summary>
        public static void Stop()
        {
            if (_process == null) return;
            AttachConsole(_process.Id);
            SetConsoleCtrlHandler(IntPtr.Zero, true);
            GenerateConsoleCtrlEvent(0, 0);
            FreeConsole();
            _process.StandardInput.Write("q");
            if (!_process.WaitForExit(10000))
            {
                _process.Kill();
            }
        }
    }
}
