using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NewAnimeChecker
{
    public partial class AboutPage : PhoneApplicationPage
    {
        #region 构造函数
        public AboutPage()
        {
            InitializeComponent();
        }
        #endregion

        #region 控件事件处理
        private void AboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
        }
        #endregion
    }
}