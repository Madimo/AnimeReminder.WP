using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using NewAnimeChecker.Resources;
using System.Diagnostics;
using HttpLibrary;

namespace NewAnimeChecker
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region 变量定义
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        #endregion

        #region 构造函数
        public MainPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }
        #endregion

        #region 状态设置
        int BusyNumber = 0;
        bool IsRefreshSubBusy = false;
        bool IsRefreshScheBusy = false;
        bool IsMarkReadBusy = false;

        public void SetIdle(string name)
        {
            switch (name)
            {
                case "DeleteItem":
                case "MarkRead":
                case "RefreshSubscription":
                    IsRefreshSubBusy = false;
                    if (Pivot.SelectedIndex == 0 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;
                    }
                    break;
                case "RefreshUpdatedSchedule":
                    IsRefreshScheBusy = false;
                    if (Pivot.SelectedIndex == 1 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                    }
                    break;
            }
            BusyNumber--;
            if (BusyNumber == 0)
                ProgressBar.IsVisible = false;
        }

        public void SetBusy(string name)
        {
            if (BusyNumber == 0)
                ProgressBar.IsVisible = true;
            switch (name)
            {
                case "DeleteItem":
                case "MarkRead":
                case "RefreshSubscription":
                    IsRefreshSubBusy = true;
                    if (Pivot.SelectedIndex == 0 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    }
                    break;
                case "RefreshUpdatedSchedule":
                    IsRefreshScheBusy = true;
                    if (Pivot.SelectedIndex == 1 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    }
                    break;
            }
            BusyNumber++;
        }
        #endregion

        #region 导航事件处理
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (settings.Contains("UserKey"))
            {
               Pivot.Title = (string)settings["UserName"];
               if (e.NavigationMode == NavigationMode.Back && settings.Contains("MustRefresh"))
               {
                   if ((bool)settings["MustRefresh"])
                   {
                       RefreshSubscription();
                       RefreshUpdatedSchedule();
                       settings["MustRefresh"] = false;
                   }
               }
               else
               {
                   if (!settings.Contains("MustRefresh"))
                       settings.Add("MustRefresh", false);
                   RefreshSubscription();
                   RefreshUpdatedSchedule();
               }
            }
            else
            {
                NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Terminate();
            e.Cancel = true;
        }
        #endregion

        #region 刷新 我的订阅
        public async void RefreshSubscription()
        {
            SetBusy("RefreshSubscription");
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/get_subscription_list?key=" + settings["UserKey"] + "&hash=" + new Random().Next());
                if (result.IndexOf("ERROR_") != -1)
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
                App.ViewModel.SubscriptionItems.Clear();
                string[] list = result.Split('\n');
                int updatedNumber = 0;
                string[] TileContent = new string[3] { "", "", "" };
                for (int i = 0; i < list.Length; ++i)
                {
                    string[] item   = list[i].Split('|');
                    if (item.Length < 5)
                        continue;
                    string isUpdate = item[0];
                    string id       = item[1];
                    string name     = item[2];
                    string epi      = item[3];
                    string isDone   = item[4];
                    if (isDone == "1")
                        epi = "已完结，共 " + item[3] + " 集";
                    else
                        epi = "更新到第 " + item[3] + " 集";
                    System.Windows.Visibility updated;
                    if (isUpdate == "1")
                    {
                        updated = System.Windows.Visibility.Visible;
                        updatedNumber++;
                        if (updatedNumber == 1)
                        {
                            TileContent[0] = " ";
                            TileContent[1] = name + " 更新到第 " + item[3] + " 集";
                        }
                        else if (updatedNumber == 2)
                        {
                            TileContent[2] = name + " 更新到第 " + item[3] + " 集";
                        }
                    }
                    else
                    {
                        updated = System.Windows.Visibility.Collapsed;
                    }
                    App.ViewModel.SubscriptionItems.Add(new ViewModels.SubscriptionModel() { ID = id, Name = name, Epi = epi, Updated = updated });
                }
                ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault();
                if (Tile != null)
                {
                    var TileData = new IconicTileData()
                    {
                        Title = "新番提醒",
                        Count = updatedNumber,
                        BackgroundColor = System.Windows.Media.Colors.Transparent,
                        WideContent1 = TileContent[0],
                        WideContent2 = TileContent[1],
                        WideContent3 = TileContent[2]
                    };
                    Tile.Update(TileData);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                SetIdle("RefreshSubscription");
            }
        }
        #endregion

        #region 刷新 最近更新
        public async void RefreshUpdatedSchedule()
        {
            SetBusy("RefreshUpdatedSchedule");
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/get_update_schedule?hash=" + new Random().Next());
                App.ViewModel.ScheduleItems.Clear();
                string[] List = result.Split('\n');
                for (int i = 0; i < List.Length; ++i)
                {
                    string[] item = List[i].Split('|');
                    if (item.Length < 3)
                        return;
                    string time = item[0];
                    string name = item[1];
                    string id   = item[2];
                    App.ViewModel.ScheduleItems.Add(new ViewModels.ScheduleModel() { ID = id, Name = name, Time = time });
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                SetIdle("RefreshUpdatedSchedule");
            }
        }
        #endregion

        #region 删除项目
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(((ViewModels.SubscriptionModel)((MenuItem)sender).DataContext).Name);
            Debug.WriteLine(App.ViewModel.SubscriptionItems.Count);
            SetBusy("DeleteItem");
            try
            {
                ViewModels.SubscriptionModel vi = (ViewModels.SubscriptionModel)((MenuItem)sender).DataContext;
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/del?key=" + settings["UserKey"] + "&id=" + vi.ID);
                if (result.IndexOf("ERROR_") != -1)
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
                int index = App.ViewModel.SubscriptionItems.IndexOf(vi); 
                App.ViewModel.SubscriptionItems.Remove(vi);
                ((MenuItem)sender).ClearValue(FrameworkElement.DataContextProperty);
/*                if (App.ViewModel.EpiItems.Count - 1 < index)
                    ((MenuItem)sender).DataContext = null;
                else
                    ((MenuItem)sender).DataContext = App.ViewModel.EpiItems[index];*/
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                SetIdle("DeleteItem");
            }
        }
        #endregion

        #region 标记已读
        private async void MarkRead_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("", "确定将所有更新标记为已看？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            SetBusy("MarkRead");
            try
            {
                foreach (ViewModels.SubscriptionModel Items in App.ViewModel.SubscriptionItems)
                {
                    if (Items.Updated == System.Windows.Visibility.Visible)
                    {
                        HttpEngine httpRequest = new HttpEngine();
                        string result = await httpRequest.GetAsync("http://apianime.ricter.info/del_highlight?key=" + settings["UserKey"] + "&id=" + Items.ID + "&hash=" + new Random().Next());
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
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                SetIdle("MarkRead");
                RefreshSubscription();
            }
        }
        #endregion

        #region 控件事件处理
        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists("Background.jpg"))
                {
                    using (IsolatedStorageFileStream file = isf.OpenFile("Background.jpg", FileMode.Open))
                    {
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.SetSource(file);
                            ImageBrush brush = new ImageBrush();
                            brush.ImageSource = bitmap;            
                            Pivot.Background = brush;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            if (Pivot.SelectedIndex == 0)
                RefreshSubscription();
            else
                RefreshUpdatedSchedule(); 
        }

        private void Add_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddPage.xaml", UriKind.Relative));
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Pivot.SelectedIndex)
            {
                case 0:
                    ApplicationBar = (ApplicationBar)Resources["AppBar0"];
                    if (IsRefreshSubBusy)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    }
                    else
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;
                    }
                    break;
                case 1:
                    ApplicationBar = (ApplicationBar)Resources["AppBar1"];
                    if (IsRefreshScheBusy)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    }
                    else
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                    }
                    break;
            }
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            if (e.IsMenuVisible == true)
                ApplicationBar.Opacity = 0.9;
            else
                ApplicationBar.Opacity = 0.5;
        }

        private void GoMarket_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask market = new MarketplaceReviewTask();
            market.Show();
        }
        #endregion

        #region 更换背景
        private void ChangeBackground_Click(object sender, EventArgs e)
        {
            PhotoChooserTask photoChooser = new PhotoChooserTask();
            photoChooser.Completed += new EventHandler<PhotoResult>(PhotoChooserCompleted);
            photoChooser.ShowCamera = true;
            photoChooser.PixelWidth = (int)Application.Current.Host.Content.ActualWidth;
            photoChooser.PixelHeight = (int)Application.Current.Host.Content.ActualHeight;
            photoChooser.Show();
        }

        public void PhotoChooserCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(e.ChosenPhoto);
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bitmap;
                Pivot.Background = brush;
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fileStream = isf.OpenFile("Background.jpg", FileMode.Create))
                    {
                        Image image = new Image();
                        image.Source = bitmap;
                        WriteableBitmap wb = new WriteableBitmap(image, null);
                        Extensions.SaveJpeg(wb, fileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                    }
                }
            }
        }

        private void RestoreBackground_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("", "恢复默认背景？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists("Background.jpg"))
                isf.DeleteFile("Background.jpg");
            Pivot.Background = new SolidColorBrush(Colors.Black);
        }
        #endregion
    }

}