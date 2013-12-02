using Coding4Fun.Toolkit.Controls;
using HttpLibrary;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using NewAnimeChecker.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

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
        int busyNumber_;
        int busyNumber
        {
            set
            {
                busyNumber_ = value;
                if (value == 0)
                {
                    ProgressBar.IsVisible = false;
                    ProgressBar.Text = "";
                    if (!settings.Contains("FirstLaunch") || (int)settings["FirstLaunch"] != 1)
                    {
                        ToastPrompt toast = new ToastPrompt();
                        toast.Title = "提示";
                        toast.Message = "点按订阅标题来使用更多功能";
                        toast.FontSize = 20;
                        toast.Show();
                        if (!settings.Contains("FirstLaunch"))
                        {
                            settings.Add("FirstLaunch", 1);
                        }
                        else
                        {
                            settings["FirstLaunch"] = 1;
                        }
                        settings.Save();
                    }
                }
                else
                {
                    ProgressBar.IsVisible = true;
                }
            }

            get
            {
                return busyNumber_;
            }
        }

        bool IsRefreshSubBusy_;
        bool IsRefreshSubBusy
        {
            set
            {
                IsRefreshSubBusy_ = value;
                if (value)
                {
                    busyNumber++;
                    ProgressBar.Text = "正在刷新...";
                    if (Pivot.SelectedIndex == 0 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    }
                }
                else
                {
                    busyNumber--;
                    if (Pivot.SelectedIndex == 0 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;
                    }
                }
            }

            get
            {
                return IsRefreshSubBusy_;
            }
        }

        bool IsRefreshScheBusy_;
        bool IsRefreshScheBusy
        {
            set
            {
                IsRefreshScheBusy_ = value;
                if (value)
                {
                    busyNumber++;
                    ProgressBar.Text = "正在刷新...";
                    if (Pivot.SelectedIndex == 1 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    }
                } 
                else
                {
                    busyNumber--;
                    if (Pivot.SelectedIndex == 1 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                    }
                }
            }

            get
            {
                return IsRefreshScheBusy_;
            }
        }

        bool IsMarkReadBusy_;
        bool IsMarkReadBusy
        {
            set
            {
                IsMarkReadBusy_ = value;
                if (value)
                {
                    busyNumber++;
                    ProgressBar.Text = "正在执行...";
                    if (Pivot.SelectedIndex == 0 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    }
                }
                else
                {
                    busyNumber--;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;
                }
            }

            get
            {
                return IsMarkReadBusy_;
            }
        }

        bool IsAddToSubBusy_;
        bool IsAddToSubBusy
        {
            set
            {
                IsAddToSubBusy_ = value;
                if (value)
                {
                    busyNumber++;
                    ProgressBar.Text = "正在添加...";
                    if (Pivot.SelectedIndex == 1 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    }
                }
                else
                {
                    busyNumber--;
                    if (Pivot.SelectedIndex == 1 && ApplicationBar != null)
                    {
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                    }
                }
            }

            get
            {
                return IsAddToSubBusy_;
            }
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
                       settings.Save();
                   }
               }
               else
               {
                   if (!settings.Contains("MustRefresh"))
                   {
                       settings.Add("MustRefresh", false);
                       settings.Save();
                   }
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
            IsRefreshSubBusy = true;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.GetSubscriptionList();
                App.ViewModel.SubscriptionItems.Clear();
                LongListSelector.UpdateLayout();
                string[] TileContent = new string[3] { "", "", "" };
                foreach (AnimeAPI.Anime item in api.subscriptionList)
                {
                    string text;
                    string readText = "";
                    if (item.status == "1")
                        text = "已完结，共 " + item.epi + " 集";
                    else
                        text = "更新到第 " + item.epi + " 集";
                    if (item.read != "0")
                        readText = "已看 " + item.read + " 集";
                    System.Windows.Visibility updated;
                    if (item.highlight != "0")
                        updated = System.Windows.Visibility.Visible;
                    else
                        updated = System.Windows.Visibility.Collapsed;
                    App.ViewModel.SubscriptionItems.Add(new ViewModels.SubscriptionModel() 
                    {
                        num     = item.num, 
                        aid     = item.aid, 
                        name    = item.name, 
                        status  = item.status, 
                        epi     = item.epi,
                        read    = item.read, 
                        readText= readText,
                        highlight = item.highlight,
                        text    = text,
                        updated = updated
                    });
                }

                if (App.ViewModel.SubscriptionItems.Count >= 1 && App.ViewModel.SubscriptionItems[0].highlight != "0")
                {
                    TileContent[0] = "订阅更新";
                    TileContent[1] = App.ViewModel.SubscriptionItems[0].name + " 更新到第 " + App.ViewModel.SubscriptionItems[0].epi + " 集";
                }
                if (App.ViewModel.SubscriptionItems.Count >= 2 && App.ViewModel.SubscriptionItems[1].highlight != "0")
                {
                    TileContent[2] = App.ViewModel.SubscriptionItems[1].name + " 更新到第 " + App.ViewModel.SubscriptionItems[1].epi + " 集";
                }
                ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault();
                if (Tile != null)
                {
                    var TileData = new IconicTileData()
                    {
                        Title = "新番提醒",
                        Count = api.updateNumber,
                        BackgroundColor = System.Windows.Media.Colors.Transparent,
                        WideContent1 = TileContent[0],
                        WideContent2 = TileContent[1],
                        WideContent3 = TileContent[2]
                    };
                    Tile.Update(TileData);
                }

/*
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api2.ricter.info/get_subscription_list?key=" + settings["UserKey"] + "&hash=" + new Random().Next());
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
                    if (isDone == "0")
                        showEpi = "已完结，共 " + epi + " 集";
                    else
                        showEpi = "更新到第 " + epi + " 集";
                    if (readed != "0")
                        showEpi += "，看到第 " + readed + " 集";
                    System.Windows.Visibility updated;
                    if (isUpdate != "0")
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
                IsRefreshSubBusy = false;
            }
        }
        #endregion

        #region 刷新 最近更新
        public async void RefreshUpdatedSchedule()
        {
            IsRefreshScheBusy = true;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.GetUpdateSchedule();
                App.ViewModel.ScheduleItems.Clear();
                foreach (AnimeAPI.Anime item in api.scheduleList)
                {
                    string text;
                    if (item.date == "0")
                        text = "今天 " + item.time;
                    else
                        text = "明天 " + item.time;
                    App.ViewModel.ScheduleItems.Add(new ViewModels.ScheduleModel()
                    {
                        num  = item.num,
                        aid  = item.aid,
                        name = item.name,
                        time = text
                    });
                }

/*
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api2.ricter.info/get_update_schedule?hash=" + new Random().Next());
                App.ViewModel.ScheduleItems.Clear();
                string[] List = result.Split('\n');
                int number = 0;
                for (int i = 0; i < List.Length; ++i)
                {
                    string[] item = List[i].Split('|');
                    if (item.Length < 4)
                        return;
                    number++;
                    string whichDay = item[0]; 
                    string id       = item[1];
                    string name     = item[2];
                    string time     = item[3];
                    if (whichDay == "0")
                        time = "今天 " + time;
                    else if (whichDay == "1")
                        time = "明天 " + time;
                    App.ViewModel.ScheduleItems.Add(new ViewModels.ScheduleModel() { Number = number, ID = id, Name = name, Time = time });
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
                IsRefreshScheBusy = false;
            }
        }
        #endregion

        #region 删除项目
/*
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(((ViewModels.SubscriptionModel)((MenuItem)sender).DataContext).name);
            Debug.WriteLine(App.ViewModel.SubscriptionItems.Count);
            SetBusy("DeleteItem");
            try
            {
                ViewModels.SubscriptionModel vi = (ViewModels.SubscriptionModel)((MenuItem)sender).DataContext;
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api2.ricter.info/del_anime?key=" + settings["UserKey"] + "&aid=" + vi.aid);
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
 */
        #endregion

        #region 标记已读
        private async void MarkRead_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("", "确定将所有更新标记为已读？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            IsMarkReadBusy = true;
            AnimeAPI api = new AnimeAPI();
            try
            {
                foreach (ViewModels.SubscriptionModel item in App.ViewModel.SubscriptionItems)
                {
                    if (item.updated == System.Windows.Visibility.Visible)
                    {
                        await api.DelHighlight(item.aid);
                    }
                }
/*
                foreach (ViewModels.SubscriptionModel Items in App.ViewModel.SubscriptionItems)
                {
                    if (Items.updated == System.Windows.Visibility.Visible)
                    {
                        HttpEngine httpRequest = new HttpEngine();
                        string result = await httpRequest.GetAsync("http://apianime.ricter.info/del_highlight?key=" + settings["UserKey"] + "&id=" + Items.aid + "&hash=" + new Random().Next());
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
 */
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "成功将所有更新标记为已读";
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
                IsMarkReadBusy = false;
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

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/DetailPage.xaml?index=" + (((sender as Grid).DataContext as ViewModels.SubscriptionModel).num - 1).ToString(), UriKind.Relative));
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModels.ScheduleModel item = ((sender as StackPanel).DataContext as ViewModels.ScheduleModel);
            NavigationService.Navigate(new Uri("/AnimeIntroPage.xaml?aid=" + item.aid + "&title=" + item.name, UriKind.Relative));
        }
        #endregion

        #region 背景图片
        private void SetBackground(ImageBrush brush)
        {
            BackgroundTransform.Opacity = 0.4;
            BackgroundImage.Source = (Pivot.Background as ImageBrush).ImageSource;
            BackgroundTransform.Source = brush.ImageSource;
            BackgroundImage.Opacity = 1;
            BackgroundImage.Visibility = System.Windows.Visibility.Visible;
            BackgroundTransform.Visibility = System.Windows.Visibility.Visible;
            Pivot.Background = new SolidColorBrush(Colors.Transparent);

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 1;
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
            animation.BeginTime = TimeSpan.FromMilliseconds(1000);

            Storyboard.SetTarget(animation, BackgroundImage);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Completed += AnimationCompleted;
            storyboard.Begin();
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];
            BackgroundImage.Visibility = System.Windows.Visibility.Collapsed;
            BackgroundTransform.Visibility = System.Windows.Visibility.Collapsed;
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
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bitmap;
                brush.Opacity = 0.4;
                App.Current.Resources.Remove("BackgroundBrush");
                App.Current.Resources.Add("BackgroundBrush", brush);
                SetBackground(brush);
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
                    App.Current.Resources.Remove("BackgroundBrush");
                    App.Current.Resources.Add("BackgroundBrush", App.Current.Resources["DefaultBackgroundBrush"]);
                    isf.DeleteFile("Background");
                    SetBackground((ImageBrush)App.Current.Resources["DefaultBackgroundBrush"]);
                }
            }
        }
        #endregion

        #region 添加到我的订阅
        private async void AddToSubscription_Click(object sender, RoutedEventArgs e)
        {
            IsAddToSubBusy = true;
            ViewModels.ScheduleModel sm = (ViewModels.ScheduleModel)((MenuItem)sender).DataContext;
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.AddAnime(sm.aid);
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "成功将 " + sm.name + " 添加到 我的订阅";
                toast.FontSize = 20;
                toast.Show();
                RefreshSubscription();
            }
            catch (Exception excepiton)
            {
                MessageBox.Show(excepiton.Message, "错误", MessageBoxButton.OK);
                if (api.lastError == AnimeAPI.ERROR.ERROR_INVALID_KEY)
                {
                    settings.Remove("UserKey");
                    settings.Save();
                    NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
                }
            }
            finally
            {
                IsAddToSubBusy = false;
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
            if (type.Contains("Subscription"))
            {
                if (Pivot.SelectedIndex != 0 || ((sender as StackPanel).DataContext as ViewModels.SubscriptionModel).num > 10)
                {
                    (sender as StackPanel).Opacity = 1;
                    return;
                }
                beginTime = TimeSpan.FromMilliseconds((((sender as StackPanel).DataContext as ViewModels.SubscriptionModel).num - 1) * 60);
            }
            else if (type.Contains("Schedule"))
            {
                if (Pivot.SelectedIndex != 1 || ((sender as StackPanel).DataContext as ViewModels.ScheduleModel).num > 10)
                {
                    (sender as StackPanel).Opacity = 1;
                    return;
                }
                beginTime = TimeSpan.FromMilliseconds((((sender as StackPanel).DataContext as ViewModels.ScheduleModel).num - 1) * 60);
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

            (sender as StackPanel).RenderTransform = new TranslateTransform();
            DoubleAnimation transformX = new DoubleAnimation();
            transformX.Duration = duration;
            transformX.BeginTime = beginTime;
            transformX.From = (sender as StackPanel).ActualWidth / 2;
            transformX.To = 0;
            Storyboard.SetTarget(transformX, (StackPanel)sender);
            Storyboard.SetTargetProperty(transformX, new PropertyPath("(StackPanel.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(transformX);

            storyboard.Begin();
        }
        #endregion

        #region 图片加载
        // 我的订阅 图片加载
        private async void Image_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var image = sender as Image;
                var item = image.DataContext as ViewModels.SubscriptionModel;

                BitmapImage bitmap = new BitmapImage();
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = "/Cache/" + item.aid + "_" + item.epi + ".jpg";
                    if (isf.FileExists(filePath))
                    {
                        using (IsolatedStorageFileStream stream = isf.OpenFile(filePath, FileMode.Open))
                        {
                            bitmap.SetSource(stream);
                        }
                    }
                    else
                    {

                        HttpEngine httpRequest = new HttpEngine();
                        Stream stream = await httpRequest.GetAsyncForData("http://images.movie.xunlei.com/submovie_img/" + item.aid[0] + item.aid[1] + "/" + item.aid + "/" + item.epi + "_1_115x70.jpg");
                        bitmap.SetSource(stream);
                        if (!isf.DirectoryExists("/Cache"))
                        {
                            isf.CreateDirectory("/Cache");
                        }
                        using (IsolatedStorageFileStream writeStream = isf.OpenFile(filePath, FileMode.Create))
                        {
                            WriteableBitmap wb = new WriteableBitmap(bitmap);
                            wb.SaveJpeg(writeStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            writeStream.Close();
                        }
                    }
                }

                image.Source = bitmap;

                Storyboard storyboard = new Storyboard();

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 0;
                animation.To = 1;
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                Storyboard.SetTarget(animation, image);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));

                storyboard.Children.Add(animation);
                storyboard.Begin();

            }
            catch
            {

            }
        }

        // 最近更新 图片加载
        private async void Image_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var image = sender as Image;
                var item = image.DataContext as ViewModels.ScheduleModel;

                BitmapImage bitmap = new BitmapImage();
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = "/Cache/" + item.aid + "_1.jpg";
                    if (isf.FileExists(filePath))
                    {
                        using (IsolatedStorageFileStream stream = isf.OpenFile(filePath, FileMode.Open))
                        {
                            bitmap.SetSource(stream);
                        }
                    }
                    else
                    {

                        HttpEngine httpRequest = new HttpEngine();
                        Stream stream = await httpRequest.GetAsyncForData("http://images.movie.xunlei.com/submovie_img/" + item.aid[0] + item.aid[1] + "/" + item.aid + "/1_1_115x70.jpg");
                        bitmap.SetSource(stream);
                        if (!isf.DirectoryExists("/Cache"))
                        {
                            isf.CreateDirectory("/Cache");
                        }
                        using (IsolatedStorageFileStream writeStream = isf.OpenFile(filePath, FileMode.Create))
                        {
                            WriteableBitmap wb = new WriteableBitmap(bitmap);
                            wb.SaveJpeg(writeStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            writeStream.Close();
                        }
                    }
                }

                image.Source = bitmap;

                Storyboard storyboard = new Storyboard();

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 0;
                animation.To = 1;
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                Storyboard.SetTarget(animation, image);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));

                storyboard.Children.Add(animation);
                storyboard.Begin();

            }
            catch
            {

            }
        }
        #endregion
    }
}
