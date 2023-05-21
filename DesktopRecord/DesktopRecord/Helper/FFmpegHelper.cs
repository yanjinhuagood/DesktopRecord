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
        static Process p;

        // ffmpeg.exe实体文件路径
        static string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

        /// <summary>
        /// 功能: 开始录制
        /// </summary>
        public static bool Start()
        {
            if(!File.Exists(ffmpegPath))
                return false;
            p = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(ffmpegPath);
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.CreateNoWindow = true;  //不显示dos程序窗口
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;//把外部程序错误输出写到StandardError流中
            startInfo.Arguments = "-f gdigrab -framerate 30 -offset_x 0 -offset_y 0 -video_size 1600x900 -i desktop " + DateTime.Now.ToString("yyyyMMddHHmmss") + "_DesktopRecord.mpg ";
            p.StartInfo = startInfo;
            p.Start();
            return true;
            //p.BeginErrorReadLine();//开始异步读取
            //p.WaitForExit();//阻塞等待进程结束
            //p.Close();//关闭进程
            //p.Dispose();//释放资源
        }

        /// <summary>
        /// 功能: 停止录制
        /// </summary>
        public static void Stop()
        {
            if (p == null) return;
            AttachConsole(p.Id);
            SetConsoleCtrlHandler(IntPtr.Zero, true);
            GenerateConsoleCtrlEvent(0, 0);
            FreeConsole();
            p = null;
        }
    }
}
