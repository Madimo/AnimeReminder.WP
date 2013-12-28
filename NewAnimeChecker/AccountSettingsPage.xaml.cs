using Microsoft.Phone.Controls;
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace NewAnimeChecker
{
    public partial class AccountSettingsPage : PhoneApplicationPage
    {
        #region 构造函数
        public AccountSettingsPage()
        {
            InitializeComponent();
        }
        #endregion

        #region 控件事件处理
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("", "确认注销当前帐号？", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                IsolatedStorageSettings.ApplicationSettings.Remove("UserKey");
                IsolatedStorageSettings.ApplicationSettings.Save();
                NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            }
        }

        private void AccountSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Title = (string)IsolatedStorageSettings.ApplicationSettings["UserName"];
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
        }

        private void OldPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (OldPasswordBox.Password == "")
            {
                FakeOldPasswordBox.Visibility = System.Windows.Visibility.Visible;
                OldPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void FakeOldPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            OldPasswordBox.Visibility = System.Windows.Visibility.Visible;
            FakeOldPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            OldPasswordBox.Focus();
        }

        private void NewPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NewPasswordBox.Password == "")
            {
                FakeNewPasswordBox.Visibility = System.Windows.Visibility.Visible;
                NewPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void FakeNewRepasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NewPasswordBox.Visibility = System.Windows.Visibility.Visible;
            FakeNewPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            NewPasswordBox.Focus();
        }

        private void RepasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (RepasswordBox.Password == "")
            {
                FakeRepasswordBox.Visibility = System.Windows.Visibility.Visible;
                RepasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void FakeRepasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            RepasswordBox.Visibility = System.Windows.Visibility.Visible;
            FakeRepasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            RepasswordBox.Focus();
        }
        #endregion

        #region 修改密码
        private void ChangePsw_Click(object sender, RoutedEventArgs e)
        {
            if ((ChangePsw.Content as string) == "修改密码")
            {
                FakeOldPasswordBox.Visibility = System.Windows.Visibility.Visible;
                FakeNewPasswordBox.Visibility = System.Windows.Visibility.Visible;
                FakeRepasswordBox.Visibility  = System.Windows.Visibility.Visible;

                Storyboard storyboard = new Storyboard();

                Duration duration = new Duration(TimeSpan.FromMilliseconds(200));

                ChangePsw.RenderTransform = new TranslateTransform();
                To.RenderTransform = new TranslateTransform();

                DoubleAnimation transformY = new DoubleAnimation();
                transformY.Duration = duration;
                transformY.From = ChangePsw.Margin.Bottom;
                transformY.To = To.Margin.Top - ChangePsw.Margin.Top;
                Storyboard.SetTarget(transformY, ChangePsw);
                Storyboard.SetTargetProperty(transformY, new PropertyPath("(Button.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(transformY);

                DoubleAnimation doubleAnimationOne = new DoubleAnimation();
                doubleAnimationOne.From = 0;
                doubleAnimationOne.To = 1;
                doubleAnimationOne.Duration = duration;
                Storyboard.SetTarget(doubleAnimationOne, FakeOldPasswordBox);
                Storyboard.SetTargetProperty(doubleAnimationOne, new PropertyPath(TextBox.OpacityProperty));
                storyboard.Children.Add(doubleAnimationOne);

                DoubleAnimation doubleAnimationTwo = new DoubleAnimation();
                doubleAnimationTwo.From = 0;
                doubleAnimationTwo.To = 1;
                doubleAnimationTwo.Duration = duration;
                Storyboard.SetTarget(doubleAnimationTwo, FakeNewPasswordBox);
                Storyboard.SetTargetProperty(doubleAnimationTwo, new PropertyPath(TextBox.OpacityProperty));
                storyboard.Children.Add(doubleAnimationTwo);

                DoubleAnimation doubleAnimationThree = new DoubleAnimation();
                doubleAnimationThree.From = 0;
                doubleAnimationThree.To = 1;
                doubleAnimationThree.Duration = duration;
                Storyboard.SetTarget(doubleAnimationThree, FakeRepasswordBox);
                Storyboard.SetTargetProperty(doubleAnimationThree, new PropertyPath(TextBox.OpacityProperty));
                storyboard.Children.Add(doubleAnimationThree);

                storyboard.Begin();

                ChangePsw.Content = "修改";
            }
            else
            {
                ChangePassword();
            }
        }

        private async void ChangePassword()
        {
            ProgressBar.IsVisible = true;
            Logout.IsEnabled = false;
            ChangePsw.IsEnabled = false;

            AnimeAPI api = new AnimeAPI();
            try
            {
                if (OldPasswordBox.Password == "")
                    throw new Exception("请输入原密码");
                if (NewPasswordBox.Password == "")
                    throw new Exception("请输入新密码");
                if (RepasswordBox.Password == "")
                    throw new Exception("请输入密码确认");
                if (OldPasswordBox.Password.Length < 6 || OldPasswordBox.Password.Length > 16)
                    throw new Exception("密码的长度必须在 6 到 16 之间");
                if (NewPasswordBox.Password.Length < 6 || NewPasswordBox.Password.Length > 16)
                    throw new Exception("密码的长度必须在 6 到 16 之间");
                if (RepasswordBox.Password.Length < 6 || RepasswordBox.Password.Length > 16)
                    throw new Exception("密码的长度必须在 6 到 16 之间");
                if (NewPasswordBox.Password != RepasswordBox.Password)
                    throw new Exception("两次输入的密码不一致");

                await api.ChangePsw(OldPasswordBox.Password, NewPasswordBox.Password);
                IsolatedStorageSettings.ApplicationSettings.Remove("UserKey");
                IsolatedStorageSettings.ApplicationSettings.Save();
                MessageBox.Show("", "修改密码成功，请重新登陆", MessageBoxButton.OK);
                NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "错误", MessageBoxButton.OK);
                if (api.lastError == AnimeAPI.ERROR.ERROR_INVALID_KEY)
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove("UserKey");
                    IsolatedStorageSettings.ApplicationSettings.Save();
                    NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                }
            }
            finally
            {
                ProgressBar.IsVisible = false;
                Logout.IsEnabled = true;
                ChangePsw.IsEnabled = true;
            }
        }
        #endregion
    }
}
