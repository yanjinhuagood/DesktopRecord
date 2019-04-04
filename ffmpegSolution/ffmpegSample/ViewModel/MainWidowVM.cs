using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using ffmpegSample.Helper;
using System.Windows.Threading;
using System.Diagnostics;

namespace ffmpegSample.ViewModel
{
    public class MainWidowVM : ViewModelBase
    {
        private DispatcherTimer tm = new DispatcherTimer();
        //定义全局变量
        public int currentCount = 0;
        /// <summary>
        /// 开始录制计时
        /// </summary>
        private string myTime = "开始录制";
        /// <summary>
        /// 开始录制计时
        /// </summary>
        public string  MyTime
        {
            get { return myTime; }
            set
            {
                myTime = value;
                RaisePropertyChanged(() => MyTime);
            }
        }

        /// <summary>
        /// 获取或设置
        /// </summary>
        private bool isStart = true;
        /// <summary>
        /// 获取或设置
        /// </summary>
        public bool IsStart
        {
            get { return isStart; }
            set
            {
                isStart = value;
                RaisePropertyChanged(() => IsStart);
            }
        }


        /// <summary>
        /// 开始录制
        /// </summary>
        private ICommand myStart;
        /// <summary>
        /// 开始录制
        /// </summary>
        public ICommand MyStart
        {
            get
            {
                return myStart ?? (myStart = new RelayCommand<object>(p =>
                           {
                               IsStart = false;
                               Messenger.Default.Send<string>("MsgHide", "MsgHide");
                               FfmpegHelper.Start();
                               tm.Tick += tm_Tick;
                               tm.Interval = TimeSpan.FromSeconds(0.05);
                               tm.Start();
                           }, a =>
            {
                               return true;
                           }));
            }
        }
        private void tm_Tick(object sender, EventArgs e)
        {
            currentCount++;
            MyTime = "开始录制(" + currentCount + "s)";
        }
        /// <summary>
        /// 获取或设置
        /// </summary>
        private ICommand myStop;
        /// <summary>
        /// 获取或设置
        /// </summary>
        public ICommand MyStop
        {
            get
            {
                return myStop ?? (myStop = new RelayCommand<object>(p =>
                           {
                               IsStart = true;
                               FfmpegHelper.Stop();
                               MyTime = "开始录制";
                               tm.Stop();
                               currentCount = 0;
                               Process.Start(AppDomain.CurrentDomain.BaseDirectory);
                           }, a =>
            {
                               return true;
                           }));
            }
        }

        public MainWidowVM()
        {

        }
        
    }
}
