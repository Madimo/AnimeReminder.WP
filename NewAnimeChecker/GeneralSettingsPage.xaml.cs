using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using Coding4Fun.Toolkit.Controls;

namespace NewAnimeChecker
{
    public partial class GeneralSettingsPage : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public GeneralSettingsPage()
        {
            InitializeComponent();
        }

        private void ShowImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!settings.Contains("ShowImage"))
            {
                settings.Add("ShowImage", true);
                settings.Save();
            }
            ShowImage.IsChecked = (bool)settings["ShowImage"];
        }

        private void ShowImage_Checked(object sender, RoutedEventArgs e)
        {
            settings["ShowImage"] = true;
            settings.Save();
        }

        private void ShowImage_Unchecked(object sender, RoutedEventArgs e)
        {
            settings["ShowImage"] = false;
            settings.Save();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
        }

        #region 清除图片缓存
        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("", "确定清除图片缓存？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    string[] files = isf.GetFileNames("/Cache/*.jpg");
                    foreach (string file in files)
                    {
                        isf.DeleteFile("/Cache/" + file);
                    }
                    ToastPrompt toast = new ToastPrompt()
                    {
                        Title = "成功清除图片缓存",
                        FontSize = 20
                    };
                    toast.Show();
                }
                catch
                {
                    MessageBox.Show("部分缓存删除失败");
                }
            }
        }
        #endregion

    }
}