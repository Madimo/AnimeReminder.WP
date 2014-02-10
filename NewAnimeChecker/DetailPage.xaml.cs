using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using HttpLibrary;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Net.NetworkInformation;

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
            int epiCount = int.Parse(subscriptionIndex.epi);
            if (epiCount <= 0)
                return;
            for (int i = 0; i <= epiCount / 4; ++i)
            {
                
                StackPanel stackPanel = new StackPanel();
                stackPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                for (int j = 1; j <= 4; ++j)
                {
                    if (j + i * 4 > epiCount)
                        break;
                    Button button = new Button();
                    button.Width = EpiList.RenderSize.Width / 4;
                    button.Content = (j + i * 4).ToString();
                    button.Tag = (j + i * 4).ToString();
                    button.Click += EpiButton_Click;
                    stackPanel.Children.Add(button);
                }
                if (stackPanel.Children.Count != 0)
                    EpiList.Children.Add(stackPanel);
            }
        }

        private async void EpiButton_Click(object sender, RoutedEventArgs e)
        {

            if (!DeviceNetworkInformation.IsWiFiEnabled)
            {
                MessageBoxResult result = MessageBox.Show("检测到您正在使用移动数据网络，跳转到播放页面可能会使用您的数据流量，是否继续？", "提示", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }


            LodingGrid.Visibility = System.Windows.Visibility.Visible;

            string id = null;
            string aid = subscriptionIndex.aid;

            try
            {
                string url = "http://data.pad.kankan.com/mobile/sub_detail/" + aid[0] + aid[1] + "/" + aid + ".json";
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync(url);
                JObject json = JObject.Parse(result);
                foreach (JObject item in (json["episodes"] as JArray))
                {
                    if (((int)item["index"] + 1).ToString() == ((sender as Button).Tag as string))
                    {
                        foreach (JObject part in (item["parts"] as JArray))
                        {
                            if (((int)part["index"]) == 0)
                            {
                                id = ((int)part["id"]).ToString();
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                LodingGrid.Visibility = System.Windows.Visibility.Collapsed;
                MessageBox.Show(exception.Message, "获取播放地址失败", MessageBoxButton.OK);
            }

            if (id != null)
            {
                string playUrl = "http://m.kankan.com/v/" + aid[0] + aid[1] + "/" + aid + ".shtml?subid=" + id + "&quality=2";
                Debug.WriteLine(playUrl);
                LodingGrid.Visibility = System.Windows.Visibility.Collapsed;
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri(playUrl, UriKind.Absolute);
                task.Show();
            }
            else
            {
                LodingGrid.Visibility = System.Windows.Visibility.Collapsed;
                MessageBox.Show("", "获取播放地址失败", MessageBoxButton.OK);
            }
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
                    await api.AddHighlight(subscriptionIndex.aid, "2");
                }
                else
                {
                    await api.DelHighlight(subscriptionIndex.aid);
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