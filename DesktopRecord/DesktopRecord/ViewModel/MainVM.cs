using DesktopRecord.Helper;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using WPFDevelopers.Controls;
using WPFDevelopers.Helpers;
using Win32 = DesktopRecord.Helper.Win32;

namespace DesktopRecord.ViewModel
{
    public class MainVM : ViewModelBase
    {
        private DispatcherTimer tm = new DispatcherTimer();

        public int currentCount = 0;

        private RecordEnums _recordEnums;
        public RecordEnums RecordEnums
        {
            get { return _recordEnums; }
            set
            {
                _recordEnums = value;
                NotifyPropertyChange(nameof(RecordEnums));
            }
        }

        private string myTime = "开始录制";

        public string MyTime
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
                        Message.Push("未找到 【ffmpeg.exe】,请下载", System.Windows.MessageBoxImage.Error);
                        return;
                    }
                    tm.Tick += tm_Tick;
                    tm.Interval = TimeSpan.FromSeconds(1);
                    tm.Start();
                    IsStart = false;
                }, a =>
                 {
                     return IsStart;
                 }));
            }
        }
        private void tm_Tick(object sender, EventArgs e)
        {
            currentCount++;
            MyTime = "录制中(" + currentCount + "s)";
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
                               var task = new Task(() =>
                               {
                                   FFmpegHelper.Stop();
                                   MyTime = "开始录制";
                                   tm.Stop();
                                   currentCount = 0;
                                   IsShow = true;
                               });
                               task.ContinueWith(previousTask =>
                               {
                                   IsShow = false;
                                   IsStart = true;
                                   Process.Start(AppDomain.CurrentDomain.BaseDirectory);
                               }, TaskScheduler.FromCurrentSynchronizationContext());
                               task.Start();
                           }, a =>
            {
                return !IsStart;
            }));
            }
        }
        public ICommand RecordCommand { get; }
        public ICommand RecordStopCommand { get; }

        public MainVM()
        {
            RecordCommand = new RelayCommand(Record, CanExecuteRecordCommand);
            RecordStopCommand = new RelayCommand(RecordStop);
        }
        void Record(object parameter)
        {
            App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
            switch (RecordEnums)
            {
                case RecordEnums.FFmpeg:
                    break;
                case RecordEnums.WindowsAPI:
                    Win32.Start();
                    break;
                case RecordEnums.Accord:
                    AccordHelper.Start();
                    break;
                default:
                    break;
            }
            IsStart = false;
        }

        private bool CanExecuteRecordCommand(object parameter)
        {
            return IsStart;
        }
        void RecordStop(object parameter)
        {
            var task = new Task(() =>
            {
                switch (RecordEnums)
                {
                    case RecordEnums.FFmpeg:
                        break;
                    case RecordEnums.WindowsAPI:
                        Win32.Stop();
                        Win32.Save($"DesktopRecord_{DateTime.Now.ToString("yyyyMMddHHmmss")}.gif");
                        break;
                    case RecordEnums.Accord:
                        AccordHelper.Stop();
                        break;
                    default:
                        break;
                }
                IsShow = true;
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
