using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HttpLibrary;
using Coding4Fun.Toolkit.Controls;

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
                AnimeName.Header = subscriptionIndex.name;
                Pivot.Title = (string)settings["UserName"];
                if (subscriptionIndex.updated == System.Windows.Visibility.Visible)
                    MarkReadOrUnreadButton.Content = "标记为已读";
                else
                    MarkReadOrUnreadButton.Content = "标记为未读";
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
/*
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/del?key=" + settings["UserKey"] + "&id=" + subscriptionIndex.aid);
                if (result.Contains("ERROR_"))
                {
                    if (result == "ERROR_INVALID_KEY")
                    {
                        MessageBox.Show("", "您的帐号已在别的客户端登陆，请重新登陆", MessageBoxButton.OK);
                        settings.Remove("UserKey");
                        NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                        return;
                    }
                    if (result == "ERROR_INVALID_ID")
                    {
                        throw new Exception("您选择的内容可能已经被删除，请刷新后重试");
                    }
                    throw new Exception("发生了错误，但我不知道是什么");
                }
 */
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

        #region 标为已读/未读
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
/*
                HttpEngine httpRequest = new HttpEngine();
                string requestUrl = "";
*/
                if ((MarkReadOrUnreadButton.Content as string) == "标记为未读")
                {
                    await api.AddHighlight(subscriptionIndex.aid, "2");
/*
                    requestUrl = "http://apianime.ricter.info/add_highlight?key=" + settings["UserKey"] + "&id=" + subscriptionIndex.aid + "&hash=" + new Random().Next();
*/
                }
                else
                {
                    await api.DelHighlight(subscriptionIndex.aid);
/*
                    requestUrl = "http://apianime.ricter.info/del_highlight?key=" + settings["UserKey"] + "&id=" + subscriptionIndex.aid + "&hash=" + new Random().Next();
*/
                }
/*
                string result = await httpRequest.GetAsync(requestUrl);
                if (result.Contains("ERROR_"))
                {
                    if (result == "ERROR_INVALID_KEY")
                    {
                        MessageBox.Show("", "您的帐号已在别的客户端登陆，请重新登陆", MessageBoxButton.OK);
                        settings.Remove("UserKey");
                        NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                        return;
                    }
                    throw new Exception("发生了错误，但我不知道是什么");
                }
 */
                if (settings.Contains("MustRefresh"))
                    settings.Remove("MustRefresh");
                settings.Add("MustRefresh", true);
                settings.Save();
                ToastPrompt toast = new ToastPrompt();
                if ((MarkReadOrUnreadButton.Content as string) == "标记为未读")
                {
                    MarkReadOrUnreadButton.Content = "标记为已读";
                    toast.Title = "成功标记为未读";
                }
                else
                {
                    MarkReadOrUnreadButton.Content = "标记为未读";
                    toast.Title = "成功标记为已读";
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
/*
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/epiedit?key=" + settings["UserKey"] + "&aid=" + subscriptionIndex.aid + "&epi=" + EpiTextBox.Text + "&hash=" + new Random().Next());
                if (result.Contains("ERROR_"))
                {
                    if (result == "ERROR_INVALID_KEY")
                    {
                        MessageBox.Show("", "您的帐号已在别的客户端登陆，请重新登陆", MessageBoxButton.OK);
                        settings.Remove("UserKey");
                        NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                        return;
                    }
                    throw new Exception("发生了错误，但我不知道是什么");
                }
*/
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