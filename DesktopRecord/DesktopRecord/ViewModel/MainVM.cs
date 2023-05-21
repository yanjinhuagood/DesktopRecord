﻿using DesktopRecord.Helper;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using WPFDevelopers.Controls;
using WPFDevelopers.Helpers;

namespace DesktopRecord.ViewModel
{
    public class MainVM : ViewModelBase
    {
        private DispatcherTimer tm = new DispatcherTimer();
       
        public int currentCount = 0;
       
        private string myTime = "开始录制";
       
        public string  MyTime
        {
            get { return myTime; }
            set
            {
                myTime = value;
                NotifyPropertyChange("MyTime");
            }
        }

       
        private bool isStart = true;
       
        public bool IsStart
        {
            get { return isStart; }
            set
            {
                isStart = value;
                NotifyPropertyChange("IsStart");
            }
        }


        private bool _isShow;
      
        public bool IsShow
        {
            get { return _isShow; }
            set
            {
                _isShow = value;
                NotifyPropertyChange("IsShow");
            }
        }

        private ICommand myStart;
       
        public ICommand MyStart
        {
            get
            {
                return myStart ?? (myStart = new RelayCommand(p =>
                           {
                               App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
                               if (!FFmpegHelper.Start())
                               {
                                   App.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                                   MessageBox.Show("未找到 【ffmpeg.exe】,请下载", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                                   return;
                               }
                               tm.Tick += tm_Tick;
                               tm.Interval = TimeSpan.FromSeconds(1);
                               tm.Start();
                               IsStart = false;
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
                return myStop ?? (myStop = new RelayCommand(p =>
                           {
                               IsStart = true;
                               FFmpegHelper.Stop();
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
        public ICommand RecordCommand { get;}
        public ICommand RecordStopCommand { get; }
        public MainVM()
        {
            RecordCommand = new RelayCommand(Record,CanExecuteRecordCommand);
            RecordStopCommand = new RelayCommand(RecordStop);
        }
        void Record(object parameter)
        {
            App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
            Win32.Start();
            IsStart = false;
        }

        private bool CanExecuteRecordCommand(object parameter)
        {
            return IsStart;
        }
        void RecordStop(object parameter)
        {
            Win32.Stop();
            var task = new Task(() => 
            {
                IsShow = true;
                Win32.Save($"DesktopRecord_{DateTime.Now.ToString("yyyyMMddHHmmss")}.gif");
            });
            task.ContinueWith(previousTask =>
            {
                IsShow = false;
                IsStart = true;
                Process.Start(AppDomain.CurrentDomain.BaseDirectory);
            }, TaskScheduler.FromCurrentSynchronizationContext());
            task.Start();
        }
       
    }
}