using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace NewAnimeChecker
{
    public partial class DetailPage : PhoneApplicationPage
    {
        #region 变量定义
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        ViewModels.SubscriptionModel subscriptionIndex;
        #endregion

        #region 构造函数
        public DetailPage()
        {
            InitializeComponent();
        }
        #endregion

        #region 导航事件处理
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string index;
            if (NavigationContext.QueryString.TryGetValue("index", out index))
            {
                subscriptionIndex = App.ViewModel.SubscriptionItems[int.Parse(index)];
                Pivot.Title = (string)settings["UserName"];
                if (subscriptionIndex.highlight != "0")
                    MarkReadOrUnreadButton.Content = "标记为未更新";
                else
                    MarkReadOrUnreadButton.Content = "标记为更新";
                EpiTextBox.Text = subscriptionIndex.read;
                EpiTextBlock.Text = "此订阅目前共 " + subscriptionIndex.epi + " 集";
            }
            else
            {
                MessageBox.Show("订阅信息读取失败，请重试", "错误", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }
        #endregion

        #region 控件事件处理
        private void DetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (int.Parse(EpiTextBox.Text) < int.Parse(subscriptionIndex.epi))
                EpiTextBox.Text = (int.Parse(EpiTextBox.Text) + 1).ToString();
        }

        private void Sub_Click(object sender, RoutedEventArgs e)
        {
            if (int.Parse(EpiTextBox.Text) > 0)
                EpiTextBox.Text = (int.Parse(EpiTextBox.Text) - 1).ToString();
        }

        private void EpiTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EpiTextBox.SelectAll();
        }

        private void EpiTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EpiTextBox.Text == "")
                EpiTextBox.Text = subscriptionIndex.read;
            else if (int.Parse(EpiTextBox.Text) > int.Parse(subscriptionIndex.epi))
                EpiTextBox.Text = subscriptionIndex.epi;
            else if (int.Parse(EpiTextBox.Text) < 0)
                EpiTextBox.Text = "0";
            else
                EpiTextBox.Text = int.Parse(EpiTextBox.Text).ToString();
        }

        private void EpiTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Unknown || EpiTextBox.Text.Length >= 6)
            {
                e.Handled = true;
            }
        }

        private void EpiTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EpiTextBox.Text == ".")
                EpiTextBox.Text = "";
        }
        #endregion

        #region 删除订阅
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("", "确定删除此订阅？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            ProgressBar.Text = "删除中...";
            ProgressBar.IsVisible = true;
            UpdateButton.IsEnabled = false;
            MarkReadOrUnreadButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.DelAnime(subscriptionIndex.aid);
                App.ViewModel.SubscriptionItems.Remove(subscriptionIndex);
                if (settings.Contains("MustRefresh"))
                    settings.Remove("MustRefresh");
                settings.Add("MustRefresh", true);
                settings.Save();
                NavigationService.GoBack();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "错误", MessageBoxButton.OK);
                if (api.lastError == AnimeAPI.ERROR.ERROR_INVALID_KEY)
                {
                    settings.Remove("UserKey");
                    settings.Save();
                    NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                }
            }
            finally
            {
                UpdateButton.IsEnabled = true;
                MarkReadOrUnreadButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                ProgressBar.IsVisible = false;
                ProgressBar.Text = "";
            }
        }
        #endregion

        #region 标为未更新/更新
        private async void MarkReadOrUnread_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Text = "执行中...";
            ProgressBar.IsVisible = true;
            UpdateButton.IsEnabled = false;
            MarkReadOrUnreadButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            AnimeAPI api = new AnimeAPI();
            try
            {
                if ((MarkReadOrUnreadButton.Content as string) == "标记为更新")
                {
                    await api.Highlight(subscriptionIndex.aid, "add");
                }
                else
                {
                    await api.Highlight(subscriptionIndex.aid, "del");
                }
                if (settings.Contains("MustRefresh"))
                    settings.Remove("MustRefresh");
                settings.Add("MustRefresh", true);
                settings.Save();
                ToastPrompt toast = new ToastPrompt();
                if ((MarkReadOrUnreadButton.Content as string) == "标记为更新")
                {
                    MarkReadOrUnreadButton.Content = "标记为未更新";
                    toast.Title = "成功标记为更新";
                }
                else
                {
                    MarkReadOrUnreadButton.Content = "标记为更新";
                    toast.Title = "成功标记为未更新";
                }
                toast.FontSize = 20;
                toast.Show();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "错误", MessageBoxButton.OK);
                if (api.lastError == AnimeAPI.ERROR.ERROR_INVALID_KEY)
                {
                    settings.Remove("UserKey");
                    settings.Save();
                    NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                }
            }
            finally
            {
                UpdateButton.IsEnabled = true;
                MarkReadOrUnreadButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                ProgressBar.IsVisible = false;
                ProgressBar.Text = "";
            }
        }
        #endregion

        #region 更新已看集数
        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Text = "更新中...";
            ProgressBar.IsVisible = true;
            UpdateButton.IsEnabled = false;
            MarkReadOrUnreadButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.SetReadEpi(subscriptionIndex.aid, EpiTextBox.Text);
                if (settings.Contains("MustRefresh"))
                    settings.Remove("MustRefresh");
                settings.Add("MustRefresh", true);
                settings.Save();
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "更新成功";
                toast.FontSize = 20;
                toast.Show();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "错误", MessageBoxButton.OK);
                if (api.lastError == AnimeAPI.ERROR.ERROR_INVALID_KEY)
                {
                    settings.Remove("UserKey");
                    settings.Save();
                    NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                }
            }
            finally
            {
                ProgressBar.IsVisible = false;
                UpdateButton.IsEnabled = true;
                MarkReadOrUnreadButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                ProgressBar.Text = "";
            }
        }
        #endregion
    }
}