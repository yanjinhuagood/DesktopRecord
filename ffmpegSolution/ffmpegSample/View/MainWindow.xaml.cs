using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using EasyCustomControlLibrary;

namespace ffmpegSample.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : DefAultWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<string>(this, "MsgHide",new Action<string>(p=> {
                this.WindowState = WindowState.Minimized;
            }));
        }
    }
}
