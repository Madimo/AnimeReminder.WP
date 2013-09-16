using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
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
        bool animed = false;
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
                case "AddToSubscription":
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
            if (e.Content != null && e.Content.ToString() == "NewAnimeChecker.LoginPage")
                animed = false;
            else
                animed = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Pivot.UpdateLayout();
            if (settings.Contains("UserKey"))
            {
               Pivot.Title = (string)settings["UserName"];
               if (e.NavigationMode == NavigationMode.Back && settings.Contains("MustRefresh"))
               {
                   if ((bool)settings["MustRefresh"])
                   {
                       RefreshSubscription();
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
                int number = 0;
                string[] TileContent = new string[3] { "", "", "" };
                for (int i = 0; i < list.Length; ++i)
                {
                    string[] item   = list[i].Split('|');
                    if (item.Length < 6)
                        continue;
                    number++;
                    string isUpdate = item[0];
                    string id       = item[1];
                    string name     = item[2];
                    string epi      = item[3];
                    string isDone   = item[4];
                    string readed   = item[5];
                    string showEpi;
                    if (isDone == "1")
                        showEpi = "已完结，共 " + epi + " 集";
                    else
                        showEpi = "更新到第 " + epi + " 集";
                    if (readed != "0")
                        showEpi += "，看到第 " + readed + " 集";
                    System.Windows.Visibility updated;
                    if (isUpdate == "1")
                    {
                        updated = System.Windows.Visibility.Visible;
                        updatedNumber++;
                        if (updatedNumber == 1)
                        {
                            TileContent[0] = " ";
                            TileContent[1] = name + " 更新到第 " + epi + " 集";
                        }
                        else if (updatedNumber == 2)
                        {
                            TileContent[2] = name + " 更新到第 " + epi + " 集";
                        }
                    }
                    else
                    {
                        updated = System.Windows.Visibility.Collapsed;
                    }
                    App.ViewModel.SubscriptionItems.Add(new ViewModels.SubscriptionModel() { Number = number, ID = id, Name = name, Epi = epi, Readed = readed, ShowEpi = showEpi, Updated = updated });
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
                int number = 0;
                for (int i = 0; i < List.Length; ++i)
                {
                    string[] item = List[i].Split('|');
                    if (item.Length < 3)
                        return;
                    number++;
                    string time = item[0];
                    string name = item[1];
                    string id   = item[2];
                    App.ViewModel.ScheduleItems.Add(new ViewModels.ScheduleModel() { Number = number, ID = id, Name = name, Time = time });
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
                int index = App.ViewModel.SubscriptionItems.IndexOf(vi); 
                App.ViewModel.SubscriptionItems.Remove(vi);
                ((MenuItem)sender).UpdateLayout();
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
            if (e.IsMenuVisible)
                ApplicationBar.Opacity = 0.9;
            else
                ApplicationBar.Opacity = 0.5;
        }

        private void GoMarket_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask market = new MarketplaceReviewTask();
            market.Show();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/DetailPage.xaml?index=" + (((sender as Grid).DataContext as ViewModels.SubscriptionModel).Number - 1).ToString(), UriKind.Relative));
        }
        #endregion

        #region 背景图片
        private void SetBackground(ImageSource image)
        {
            BackgroundTransform.Opacity = 0;
            BackgroundTransform.Source = image;

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 1;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
            animation.BeginTime = TimeSpan.FromMilliseconds(1000);

            Storyboard.SetTarget(animation, BackgroundTransform);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Completed += AnimationCompleted;
            storyboard.Begin();
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            BackgroundImage.Source = BackgroundTransform.Source;
            BackgroundTransform.Opacity = 0;
        }

        private void ChangeBackground_Click(object sender, EventArgs e)
        {
            PhotoChooserTask photoChooser = new PhotoChooserTask();
            photoChooser.Completed += new EventHandler<PhotoResult>(PhotoChooserCompleted);
            photoChooser.ShowCamera = true;
            photoChooser.PixelWidth = (int)Application.Current.Host.Content.ActualWidth;
            photoChooser.PixelHeight = (int)Application.Current.Host.Content.ActualHeight;
            photoChooser.Show();
        }

        private void PhotoChooserCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(e.ChosenPhoto);
                App.Current.Resources.Remove("BackgroundImage");
                App.Current.Resources.Add("BackgroundImage", bitmap);
                SetBackground(bitmap);
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fileStream = isf.OpenFile("Background", FileMode.Create))
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
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists("Background"))
                {
                    BitmapImage bitmap = (BitmapImage)App.Current.Resources["DefaultBackgroundImage"];
                    App.Current.Resources.Remove("BackgroundImage");
                    App.Current.Resources.Add("BackgroundImage", bitmap);
                    isf.DeleteFile("Background");
                    SetBackground(bitmap);
                }
            }
        }

        private void Background_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundImage.Source = (BitmapImage)App.Current.Resources["BackgroundImage"];
        }
        #endregion

        #region 添加到我的订阅
        private async void AddToSubscription_Click(object sender, RoutedEventArgs e)
        {
            SetBusy("AddToSubscription");
            ViewModels.ScheduleModel sm = (ViewModels.ScheduleModel)((MenuItem)sender).DataContext;
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/add?key=" + settings["UserKey"] + "&id=" + sm.ID + "&hash=" + new Random().Next());
                if (result.Contains("ERROR_"))
                {
                    if (result == "ERROR_INVALID_KEY")
                    {
                        MessageBox.Show("", "您的帐号已在别的客户端登陆，请重新登陆", MessageBoxButton.OK);
                        settings.Remove("UserKey");
                        NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                        return;
                    }
                    if (result == "ERROR_INVALID_ANIME")
                    {
                        throw new Exception("抱歉，您选择的项目可能不会有剧集更新，无法添加");
                    }
                    throw new Exception("发生了错误，但我不知道是什么");
                }
                MessageBox.Show("", "添加成功", MessageBoxButton.OK);
                RefreshSubscription();
            }
            catch (Exception excepiton)
            {
                MessageBox.Show("", excepiton.Message, MessageBoxButton.OK);
            }
            finally
            {
                SetIdle("AddToSubscription");
            }
        }
        #endregion

        #region LongListSelector 动画
        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (animed)
            {
                (sender as StackPanel).Opacity = 1;
                return;
            }
            string type = (sender as StackPanel).DataContext.GetType().ToString();
            TimeSpan beginTime;
            double top = 0;
            if (type.Contains("Subscription"))
            {
                if (Pivot.SelectedIndex != 0 || ((sender as StackPanel).DataContext as ViewModels.SubscriptionModel).Number > 10)
                {
                    (sender as StackPanel).Opacity = 1;
                    return;
                }
                beginTime = TimeSpan.FromMilliseconds((((sender as StackPanel).DataContext as ViewModels.SubscriptionModel).Number - 1) * 60);
                top = 0;
            }
            else if (type.Contains("Schedule"))
            {
                if (Pivot.SelectedIndex != 1 || ((sender as StackPanel).DataContext as ViewModels.ScheduleModel).Number > 10)
                {
                    (sender as StackPanel).Opacity = 1;
                    return;
                }
                beginTime = TimeSpan.FromMilliseconds((((sender as StackPanel).DataContext as ViewModels.ScheduleModel).Number - 1) * 60);
                top = 12;
            }
            else
            {
                return;
            }
            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));

            Storyboard storyboard = new Storyboard();

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = 1;
            doubleAnimation.Duration = duration;
            doubleAnimation.BeginTime = beginTime;
            Storyboard.SetTarget(doubleAnimation, (StackPanel)sender);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(StackPanel.OpacityProperty));
            storyboard.Children.Add(doubleAnimation);

            ObjectAnimationUsingKeyFrames objectAnimation = new ObjectAnimationUsingKeyFrames();
            objectAnimation.Duration = duration;
            objectAnimation.BeginTime = beginTime;
            double by = (sender as StackPanel).ActualWidth / 2.0 / duration.TimeSpan.Milliseconds;
            for (double i = 1; i <= duration.TimeSpan.Milliseconds; i += duration.TimeSpan.Milliseconds / 20)
            {
                DiscreteObjectKeyFrame key = new DiscreteObjectKeyFrame();
                key.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(i));
                key.Value = new Thickness((sender as StackPanel).ActualWidth / 2.0 - i * by, 0, 0, top);
                objectAnimation.KeyFrames.Add(key);
            }
            Storyboard.SetTarget(objectAnimation, (StackPanel)sender);
            Storyboard.SetTargetProperty(objectAnimation, new PropertyPath(StackPanel.MarginProperty));
            storyboard.Children.Add(objectAnimation);

            storyboard.Begin();
        }
        #endregion
    }
}