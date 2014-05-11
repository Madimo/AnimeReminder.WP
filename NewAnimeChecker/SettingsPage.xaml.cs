using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace NewAnimeChecker
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
        }

        private void Account_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AccountSettingsPage.xaml", UriKind.Relative));
        }

        private void Notification_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NotificationSettingsPage.xaml", UriKind.Relative));
        }

        private void GoMarket_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MarketplaceReviewTask market = new MarketplaceReviewTask();
            market.Show();
        }

        private void Donate_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void About_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void Feedback_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "新番提醒WindowsPhone 反馈";
            emailComposeTask.To = "MadimoZhang@gmail.com";
            emailComposeTask.Body = "请在下面写下您的意见或bug反馈：\n\n\n\n\n\n下面是我们需要的信息，不涉及您的隐私\nApplicationVersion:2.1\nCurrentMemoryUsage: " + DeviceStatus.ApplicationCurrentMemoryUsage
                + "\nMemoryUsageLimit: " + DeviceStatus.ApplicationMemoryUsageLimit + "\nPeakMemoryUsage: " + DeviceStatus.ApplicationPeakMemoryUsage + "\nFirmwareVersion: "
                + DeviceStatus.DeviceFirmwareVersion + "\nHardwareVersion: " + DeviceStatus.DeviceHardwareVersion + "\nManufacturer: " + DeviceStatus.DeviceManufacturer + "\nDeviceName: "
                + DeviceStatus.DeviceName + "\nTotalMemory: " + DeviceStatus.DeviceTotalMemory;
            emailComposeTask.Show();
        }

        private void General_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GeneralSettingsPage.xaml", UriKind.Relative));
        }
    }
}
