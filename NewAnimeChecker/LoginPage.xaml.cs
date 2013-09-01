using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HttpLibrary;

namespace NewAnimeChecker
{
    public partial class LoginPage : PhoneApplicationPage
    {
        #region 构造函数
        public LoginPage()
        {
            InitializeComponent();
        }
        #endregion

        #region 导航事件处理

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Terminate();
            e.Cancel = true;
        }
        #endregion

        #region 控件事件处理
        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = (BitmapImage)App.Current.Resources["BackgroundImage"];
            LayoutRoot.Background = brush;
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("UserName"))
            {
                UserNameBox.Text = (string)settings["UserName"];
                UserNameBox.Foreground = new SolidColorBrush(Colors.Black);
                FakePasswordBox.Focus();
                settings.Remove("UserName");
                settings.Save();
            }
            else
            {
                UserNameBox.Focus();
            }
        }

        private void LoginTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "邮箱")
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            else if (textBox.Text == "密码" && ((System.Windows.Input.InputScopeName)textBox.InputScope.Names[0]).NameValue.ToString() == "Text")
            {
                PasswordBox.Visibility = System.Windows.Visibility.Visible;
                FakePasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                PasswordBox.Focus();
            }
            else if (textBox.Text != "")
            {
                textBox.SelectAll();
            }
        }

        private void RegTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "邮箱")
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            else if (textBox.Text == "密码" && ((System.Windows.Input.InputScopeName)textBox.InputScope.Names[0]).NameValue.ToString() == "Text")
            {
                RegPasswordBox.Visibility = System.Windows.Visibility.Visible;
                RegFakePasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                RegPasswordBox.Focus();
            }
            else if (textBox.Text == "密码确认" && ((System.Windows.Input.InputScopeName)textBox.InputScope.Names[0]).NameValue.ToString() == "Chat")
            {
                RegRepasswordBox.Visibility = System.Windows.Visibility.Visible;
                RegFakeRepasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                RegRepasswordBox.Focus();
            }
            else if (textBox.Text != "")
            {
                textBox.SelectAll();
            }
        }

        private void LoginTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "" && ((System.Windows.Input.InputScopeName)textBox.InputScope.Names[0]).NameValue.ToString() == "Text")
            {
                textBox.Text = "密码";
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            } 
            else if (textBox.Text == "")
            {
                textBox.Text = "邮箱";
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }

        }

        private void RegTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "" && ((System.Windows.Input.InputScopeName)textBox.InputScope.Names[0]).NameValue.ToString() == "Text")
            {
                textBox.Text = "密码";
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else if (textBox.Text == "" && ((System.Windows.Input.InputScopeName)textBox.InputScope.Names[0]).NameValue.ToString() == "Chat")
            {
                textBox.Text = "密码确认";
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else if (textBox.Text == "")
            {
                textBox.Text = "邮箱";
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }

        }

        private void LoginPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password == "")
            {
                FakePasswordBox.Visibility = System.Windows.Visibility.Visible;
                PasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void RegPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (RegPasswordBox.Password == "")
            {
                RegFakePasswordBox.Visibility = System.Windows.Visibility.Visible;
                RegPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void RegRePasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (RegRepasswordBox.Password == "")
            {
                RegFakeRepasswordBox.Visibility = System.Windows.Visibility.Visible;
                RegRepasswordBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void UserNameBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && UserNameBox.Text != "")
                FakePasswordBox.Focus();
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && PasswordBox.Password != "")
                Login();
        }

        private void RegUserNameBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && RegUserNameBox.Text != "")
                RegFakePasswordBox.Focus();
        }

        private void RegPasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && RegPasswordBox.Password != "")
                RegFakeRepasswordBox.Focus();
        }

        private void RegRepasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && RegRepasswordBox.Password != "")
                CheckBox.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            Register();
        }
        #endregion

        #region 登陆
        private async void Login()
        {
            ProgressBar.IsVisible = true;
            UserNameBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;
            LoginButton.IsEnabled = false;
            try
            {
                if (UserNameBox.Text == "邮箱")
                    throw new Exception("请输入邮箱");
                if (PasswordBox.Password == "")
                    throw new Exception("请输入密码");
                HttpEngine httpRequest = new HttpEngine();
                string UserKey = await httpRequest.GetAsync("http://apianime.ricter.info/login?u=" + UserNameBox.Text + "&p=" + PasswordBox.Password);
                if (UserKey.Contains("ERROR_"))
                {
                    if (UserKey == "ERROR_INVALID_PSW")
                        throw new Exception("邮箱或密码错误");
                    throw new Exception("发生了错误，但我不知道是什么");
                }
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                settings.Add("UserKey", UserKey);
                settings.Add("UserName", UserNameBox.Text);
                if (settings.Contains("MustRefresh"))
                    settings.Remove("MustRefresh");
                settings.Add("MustRefresh", true);
                settings.Save();
                NavigationService.GoBack();
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                UserNameBox.IsEnabled = true;
                PasswordBox.IsEnabled = true;
                LoginButton.IsEnabled = true;
                ProgressBar.IsVisible = false;
            }
        }
        #endregion

        #region 注册
        private async void Register()
        {
            ProgressBar.IsVisible = true;
            RegUserNameBox.IsEnabled = false;
            RegPasswordBox.IsEnabled = false;
            RegRepasswordBox.IsEnabled = false;
            CheckBox.IsEnabled = false;
            RegButton.IsEnabled = false;
            try
            {
                if (RegUserNameBox.Text == "邮箱")
                    throw new Exception("请填写正确的邮箱以便向您的邮箱中发送新番提醒（可选）");
                if (RegPasswordBox.Password == "")
                    throw new Exception("请填写密码");
                if (RegRepasswordBox.Password == "")
                    throw new Exception("请填写密码确认");
                if (CheckBox.IsChecked == false)
                    throw new Exception("您必须认真阅读并同意用户注册协议才能继续注册");
                Regex regex = new Regex("^[\\w-]+(\\.[\\w-]+)*@[\\w-]+(\\.[\\w-]+)+$");
                if (!regex.IsMatch(RegUserNameBox.Text))
                    throw new Exception("请填写正确的邮箱以便向您的邮箱中发送新番提醒（可选）");
                if (RegPasswordBox.Password.Length < 6 || RegPasswordBox.Password.Length > 16)
                    throw new Exception("密码的长度必须为6到16位");
                if (RegPasswordBox.Password != RegRepasswordBox.Password)
                    throw new Exception("两次输入的密码不一致");

                HttpEngine httpRequest = new HttpEngine();
                string UserKey = await httpRequest.GetAsync("http://apianime.ricter.info/reg?u=" + RegUserNameBox.Text + "&p=" + RegPasswordBox.Password);
                if (UserKey.Contains("ERROR_"))
                {
                    if (UserKey == "ERROR_EXIST_REG")
                        throw new Exception("您填写的邮箱已经被注册");
                    throw new Exception("发生了错误，但我不知道是什么");
                }
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                settings.Add("UserKey", UserKey);
                settings.Add("UserName", RegUserNameBox.Text);
                if (settings.Contains("MustRefresh"))
                    settings.Remove("MustRefresh");
                settings.Add("MustRefresh", true);
                settings.Save();
                ProgressBar.IsVisible = false;
                MessageBox.Show("", "注册成功", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                RegUserNameBox.IsEnabled = true;
                RegPasswordBox.IsEnabled = true;
                RegRepasswordBox.IsEnabled = true;
                CheckBox.IsEnabled = true;
                RegButton.IsEnabled = true;
                ProgressBar.IsVisible = false;
            }
        }
        #endregion

    }
}