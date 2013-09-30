using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using HttpLibrary;

namespace NewAnimeChecker
{
    public partial class NotificationSettingsPage : PhoneApplicationPage
    {
        #region 变量定义
        public IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        string agentName = "NewAnimeChecker_ScheduledTask";
        bool isLoaded = false;
        #endregion

        #region 构造函数
        public NotificationSettingsPage()
        {
            InitializeComponent();
        }
        #endregion

        #region 控件事件处理
        private void NotificationSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Title = (string)settings["UserName"];
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
        }

        private async void SetLockscreen_Click(object sender, RoutedEventArgs e)
        {
            var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
        }
        #endregion

        #region 邮件提醒开关
        private async void EmailReminderSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar.Text = "获取中...";
            ProgressBar.IsVisible = true;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.GetEmailReminderStatus();
/*
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/email_reminder_get?key=" + IsolatedStorageSettings.ApplicationSettings["UserKey"] + "&hash=" + new Random().Next());
                if (result.Contains("ERROR_"))
                {
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
                }
                if (result == "1")
                    EmailReminderSwitch.IsChecked = true;
                else
                    EmailReminderSwitch.IsChecked = false;
 */
                EmailReminderSwitch.IsChecked = api.emailReminderStatus;
                EmailReminderSwitch.IsEnabled = true;
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
                ProgressBar.Text = "";
            }

        }

        private async void EmailReminderSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ProgressBar.Text = "设置中...";
            ProgressBar.IsVisible = true;
            EmailReminderSwitch.IsEnabled = false;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.SetEmailReminderStatus(true);
/*
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/email_reminder_set?key=" + IsolatedStorageSettings.ApplicationSettings["UserKey"] + "&enable=1&hash=" + new Random().Next());
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
                EmailReminderSwitch.IsEnabled = true;
                ProgressBar.IsVisible = false;
                ProgressBar.Text = "";
            }
        }

        private async void EmailReminderSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ProgressBar.Text = "设置中...";
            ProgressBar.IsVisible = true;
            EmailReminderSwitch.IsEnabled = false;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.SetEmailReminderStatus(false);
/*
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/email_reminder_set?key=" + IsolatedStorageSettings.ApplicationSettings["UserKey"] + "&enable=0&hash=" + new Random().Next());
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
                EmailReminderSwitch.IsEnabled = true;
                ProgressBar.IsVisible = false;
                ProgressBar.Text = "";
            }
        }
        #endregion

        #region 后台代理开关
        private void TaskAgentSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            var UpdateScheduledTask = ScheduledActionService.Find("NewAnimeChecker_ScheduledTask") as PeriodicTask;
            if (UpdateScheduledTask != null && UpdateScheduledTask.IsScheduled == true)
            {
                TaskAgentSwitch.IsChecked = true;
                SetLockscreenButton.IsEnabled = true;
            }
            else
            {
                TaskAgentSwitch.IsChecked = false;
                SetLockscreenButton.IsEnabled = false;
            }
        }

        private void TaskAgentSwitch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var updateScheduledTask = ScheduledActionService.Find(agentName) as PeriodicTask;
                if (updateScheduledTask != null)
                    ScheduledActionService.Remove(agentName);
                updateScheduledTask = new PeriodicTask(agentName);
                updateScheduledTask.Description = "开启后新番提醒会定时检查您的订阅是否有更新，如果有更新将第一时间通知您";
                ScheduledActionService.Add(updateScheduledTask);
                if (settings.Contains("LastUpdatedTime"))
                    settings.Remove("LastUpdatedTime");
                settings.Add("LastUpdatedTime", DateTime.Now);
                if (!settings.Contains("UpdateInterval"))
                    settings.Add("UpdateInterval", 0);
                settings.Save();
                ListPicker.IsEnabled = true;
                SetLockscreenButton.IsEnabled = true;
                if (Debugger.IsAttached)
                {
                    ScheduledActionService.LaunchForTest(agentName, TimeSpan.FromSeconds(30));
                }
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show("本程序的后台任务被禁用，无法后台检查订阅更新，请从 系统设置-应用程序-后台任务 中开启本程序的后台任务后重试", "错误", MessageBoxButton.OK);
                }
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    MessageBox.Show("手机的后台任务数量已经达到上限，请从 系统设置-应用程序-后台任务 中关闭不需要的其它程序的后台任务后重试", "错误", MessageBoxButton.OK);
                }
                TaskAgentSwitch.IsChecked = false;
            }
            catch (SchedulerServiceException)
            {
                MessageBox.Show("启动后台任务失败", "错误", MessageBoxButton.OK);
                TaskAgentSwitch.IsChecked = false;
            }
        }

        private void TaskAgentSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            var UpdateScheduledTask = ScheduledActionService.Find(agentName) as PeriodicTask;
            if (UpdateScheduledTask != null)
                ScheduledActionService.Remove(agentName);
            ListPicker.IsEnabled = false;
            SetLockscreenButton.IsEnabled = false;
        }
        #endregion

        #region 后台代理间隔选择
        private void ListPicker_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            var UpdateScheduledTask = ScheduledActionService.Find("NewAnimeChecker_ScheduledTask") as PeriodicTask;
            if (UpdateScheduledTask != null && UpdateScheduledTask.IsScheduled == true)
            {
                ListPicker.IsEnabled = true;
            }
            else
            {
                ListPicker.IsEnabled = false;
            }
            if (settings.Contains("UpdateInterval"))
                ListPicker.SelectedIndex = (int)settings["UpdateInterval"];
            else
                ListPicker.SelectedIndex = 0;
        }

        private void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoaded)
            {
                if (settings.Contains("UpdateInterval"))
                    settings.Remove("UpdateInterval");
                settings.Add("UpdateInterval", ListPicker.SelectedIndex);
                settings.Save();
            }
        }
        #endregion
    }
}