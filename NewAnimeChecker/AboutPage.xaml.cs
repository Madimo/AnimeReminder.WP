using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Media;

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