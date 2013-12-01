using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
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

namespace NewAnimeChecker
{
    public partial class AnimeIntroPage : PhoneApplicationPage
    {
        string aid;
        string title;

        public AnimeIntroPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.TryGetValue("title", out title))
            {
                Content.Header = title;
            }

            if (NavigationContext.QueryString.TryGetValue("aid", out aid))
            {
                try
                {
                    AnimeAPI api = new AnimeAPI();
                    await api.GetAnimeDetail(aid);

                    Loading.Visibility = System.Windows.Visibility.Collapsed;
                    LoadingProgress.Visibility = System.Windows.Visibility.Collapsed;

                    Content.Header = api.animeDetail.title;

                    Directors.Text = api.animeDetail.directorName + "：";
                    foreach (string item in api.animeDetail.directors)
                        Directors.Text += item + " ";

                    Actors.Text = api.animeDetail.actorName + "：";
                    foreach (string item in api.animeDetail.actors)
                        Actors.Text += item + " ";

                    Tags.Text = "标签：";
                    foreach (string item in api.animeDetail.tags)
                        Tags.Text += item + " ";

                    Year.Text = "上映：" + api.animeDetail.year;

                    Aera.Text = "地区：" + api.animeDetail.area;

                    Score.Text = "评分：" + api.animeDetail.score.ToString();

                    if (api.animeDetail.totalEpisodeCount == 0)
                    {
                        State.Text = "状态：更新到第 " + api.animeDetail.episodeCount + " 集";
                    }
                    else
                    {
                        State.Text = "状态：已完结，共 " + api.animeDetail.totalEpisodeCount + " 集";
                    }

                    Intro.Text = api.animeDetail.intro;

                    
                    
                    if (DeviceNetworkInformation.IsWiFiEnabled || !IsolatedStorageSettings.ApplicationSettings.Contains("ShowImage") || (bool)IsolatedStorageSettings.ApplicationSettings["ShowImage"])
                    {
                        try
                        {
                            BitmapImage image = new BitmapImage();
                            string filePath = "/Cache/" + api.animeDetail.id + "_intro.jpg";
                            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                if (isf.FileExists(filePath))
                                {
                                    using (IsolatedStorageFileStream iss = isf.OpenFile(filePath, FileMode.Open))
                                    {
                                        image.SetSource(iss);
                                    }
                                }
                                else
                                {
                                    HttpLibrary.HttpEngine httpRequest = new HttpLibrary.HttpEngine();
                                    Stream stream = await httpRequest.GetAsyncForData("http://images.movie.xunlei.com/gallery" + api.animeDetail.poster);
                                    image.SetSource(stream);
                                    if (!isf.DirectoryExists("/Cache"))
                                    {
                                        isf.CreateDirectory("/Cache");
                                    }
                                    using (IsolatedStorageFileStream iss = isf.OpenFile(filePath, FileMode.Create))
                                    {
                                        WriteableBitmap bitmap = new WriteableBitmap(image);
                                        bitmap.SaveJpeg(iss, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
                                        iss.Close();
                                    }
                                }
                            }

                            Background.Source = ((ImageBrush)App.Current.Resources["BackgroundBrush"]).ImageSource;
                            Background.Opacity = 0.4;
                            Background.Visibility = System.Windows.Visibility.Visible;
                            Pivot.Background = new SolidColorBrush(Colors.Transparent);
                            Transform.Source = image;
                            Transform.Opacity = 0.4;
                            Transform.Visibility = System.Windows.Visibility.Visible;

                            DoubleAnimation animation = new DoubleAnimation();
                            animation.From = 0.4;
                            animation.To = 0;
                            animation.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
                            animation.BeginTime = TimeSpan.FromMilliseconds(500);
                            animation.Completed += AnimationCompleted;

                            Storyboard.SetTarget(animation, Background);
                            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));

                            DoubleAnimation animationTwo = new DoubleAnimation();
                            animationTwo.From = 1;
                            animationTwo.To = 0;
                            animationTwo.Duration = new Duration(TimeSpan.FromMilliseconds(1500));
                            animationTwo.BeginTime = TimeSpan.FromMilliseconds(500);

                            Storyboard.SetTarget(animationTwo, Rect);
                            Storyboard.SetTargetProperty(animationTwo, new PropertyPath(Image.OpacityProperty));

                            Storyboard storyboard = new Storyboard();
                            storyboard.Children.Add(animation);
                            storyboard.Children.Add(animationTwo);
                            storyboard.Begin();
                        }
                        catch
                        {
                            // do nothing
                            
                        }
                    }
                }
                catch
                {
                    Loading.Visibility = System.Windows.Visibility.Collapsed;
                    LoadingProgress.Visibility = System.Windows.Visibility.Collapsed;
                    IntroPanel.Visibility = System.Windows.Visibility.Collapsed;
                    Error.Visibility = System.Windows.Visibility.Visible;
                }
            }
            
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            ImageBrush brush      = new ImageBrush();
            brush.ImageSource     = Transform.Source;
            brush.Opacity         = 0.4;
            Pivot.Background      = brush;
            Transform.Visibility  = System.Windows.Visibility.Collapsed;
            Background.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.Title = (string)IsolatedStorageSettings.ApplicationSettings["UserName"];
            Pivot.Background = (ImageBrush)App.Current.Resources["BackgroundBrush"];

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.IsVisible = true;
            ProgressBar.Text = "正在添加...";
            AnimeAPI api = new AnimeAPI();
            try
            {
                await api.AddAnime(aid);
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "成功将添加到我的订阅";
                toast.FontSize = 20;
                toast.Show();
            }
            catch (Exception excepiton)
            {
                MessageBox.Show(excepiton.Message, "错误", MessageBoxButton.OK);
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
                ProgressBar.Text = "";
            }
        }
        
    }
}