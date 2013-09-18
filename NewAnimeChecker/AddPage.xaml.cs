using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class AddPage : PhoneApplicationPage
    {
        #region 构造函数
        public AddPage()
        {
            InitializeComponent();
            LongListSelector.ItemsSource = App.ViewModel.SearchResultItems;
        }
        #endregion

        #region 导航事件处理
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {

        }
        #endregion

        #region 控件事件处理
        private void AddPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Title = IsolatedStorageSettings.ApplicationSettings["UserName"];
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = (BitmapImage)App.Current.Resources["BackgroundImage"];
            Pivot.Background = brush;
            SearchBox.Focus();
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "")
            {
                SearchBox.Foreground = new SolidColorBrush(Colors.Gray);
                SearchBox.Text = "搜索";
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "搜索")
            {
                SearchBox.Foreground = new SolidColorBrush(Colors.Black);
                SearchBox.Text = "";
            }
        }

        private void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                Search();
        }
        #endregion

        #region 搜索
        public async void Search()
        {
            ProgressBar.IsVisible = true;
            SearchBox.IsEnabled = false;
            LongListSelector.IsEnabled = false;
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://mediaso.xmp.kankan.xunlei.com/search.php?keyword=" + SearchBox.Text + "&hash=" + new Random().Next());
                App.ViewModel.SearchResultItems.Clear();
                string name_flag_begin = "sname=\"";
                string name_flag_end = "\",";
                string id_flag_begin = "imovieid=";
                string id_flag_end = ",";
                string type_flag_begin = "Typedesc=\"";
                string type_flag_end = "\",";
                int name_index_begin = result.IndexOf(name_flag_begin);
                while (name_index_begin != -1)
                {
                    // name
                    int name_index_end = result.IndexOf(name_flag_end, name_index_begin);
                    string name = result.Substring(name_index_begin + name_flag_begin.Length, name_index_end - name_index_begin - name_flag_begin.Length);
                    // id
                    int id_index_begin = result.IndexOf(id_flag_begin, name_index_end);
                    int id_index_end = result.IndexOf(id_flag_end, id_index_begin);
                    string id = result.Substring(id_index_begin + id_flag_begin.Length, id_index_end - id_index_begin - id_flag_begin.Length);
                    // type
                    int type_index_begin = result.IndexOf(type_flag_begin, id_index_end);
                    string type = "";
                    if (type_index_begin != -1)
                    {
                        int type_index_end = result.IndexOf(type_flag_end, type_index_begin);
                        type = result.Substring(type_index_begin + type_flag_begin.Length, type_index_end - type_index_begin - type_flag_begin.Length);
                    }
                    if (type != "电影")
                        App.ViewModel.SearchResultItems.Add(new ViewModels.SearchResultModel() { Name = name, ID = id, Type = type });
                    name_index_begin = result.IndexOf(name_flag_begin, id_index_end);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                SearchBox.IsEnabled = true;
                LongListSelector.IsEnabled = true;
                ProgressBar.IsVisible = false;
            }
        }
        #endregion

        #region 添加
        private async void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var SelectedItem = (sender as TextBlock).DataContext as ViewModels.SearchResultModel;
            if (MessageBox.Show(SelectedItem.Name, "是否添加？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            ProgressBar.IsVisible = true;
            SearchBox.IsEnabled = false;
            LongListSelector.IsEnabled = false;
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://apianime.ricter.info/add?key=" + IsolatedStorageSettings.ApplicationSettings["UserKey"] + "&id=" + SelectedItem.ID + "&hash=" + new Random().Next());
                if (result.Contains("ERROR_"))
                {
                    Debug.WriteLine(result);
                    if (result == "ERROR_INVALID_KEY")
                    {
                        MessageBox.Show("", "您的帐号已在别的客户端登陆，请重新登陆", MessageBoxButton.OK);
                        IsolatedStorageSettings.ApplicationSettings.Remove("UserKey");
                        NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                        return;
                    }
                    if (result == "ERROR_INVALID_ANIME")
                    {
                        throw new Exception("抱歉，您选择的项目可能不会有剧集更新，无法添加");
                    }
                    throw new Exception("发生了错误，但我不知道是什么");
                }
                if (IsolatedStorageSettings.ApplicationSettings.Contains("MustRefresh"))
                {
                    if (!((bool)IsolatedStorageSettings.ApplicationSettings["MustRefresh"]))
                    {
                        IsolatedStorageSettings.ApplicationSettings["MustRefresh"] = true;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                    }
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("MustRefresh", true);
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "添加成功";
                toast.FontSize = 20;
                toast.Show();
            }
            catch (Exception exception)
            {
                MessageBox.Show("", exception.Message, MessageBoxButton.OK);
            }
            finally
            {
                LongListSelector.SelectedItem = null;
                SearchBox.IsEnabled = true;
                LongListSelector.IsEnabled = true;
                ProgressBar.IsVisible = false;
            }
        }
        #endregion
    }
}