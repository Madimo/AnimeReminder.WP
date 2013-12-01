using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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
    }
}