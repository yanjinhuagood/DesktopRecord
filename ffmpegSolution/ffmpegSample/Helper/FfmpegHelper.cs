using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ffmpegSample.Helper
{
    public class FfmpegHelper
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
        static string ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg\\ffmpeg.exe";

        /// <summary>
        /// 功能: 开始录制
        /// </summary>
        public static void Start()
        {
            if (p == null)
            {
                p = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(ffmpegPath);
                startInfo.UseShellExecute = false;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.CreateNoWindow = true;  //不显示dos程序窗口
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;//把外部程序错误输出写到StandardError流中
                startInfo.Arguments = "-f gdigrab -framerate 30 -offset_x 0 -offset_y 0 -video_size 1600x900 -i desktop " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".mpg ";
                p.StartInfo = startInfo;
                p.Start();
                //p.BeginErrorReadLine();//开始异步读取
                //p.WaitForExit();//阻塞等待进程结束
                //p.Close();//关闭进程
                //p.Dispose();//释放资源
            }
        }

        /// <summary>
        /// 功能: 停止录制
        /// </summary>
        public static void Stop()
        {
            AttachConsole(p.Id);
            SetConsoleCtrlHandler(IntPtr.Zero, true);
            GenerateConsoleCtrlEvent(0, 0);
            FreeConsole();
            p = null;
        }
    }
}
