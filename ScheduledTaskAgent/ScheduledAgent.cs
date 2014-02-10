using HttpMethod;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Newtonsoft.Json.Linq;


namespace ScheduledTaskAgent
{
    public class ScheduledAgent : Microsoft.Phone.Scheduler.ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent 构造函数，初始化 UnhandledException 处理程序
        /// </remarks>
        static ScheduledAgent()
        {
            // 订阅托管的异常处理程序
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// 出现未处理的异常时执行的代码
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // 出现未处理的异常；强行进入调试器
                Debugger.Break();
            }
        }

        public class Anime
        {
            public string name;
            public string epi;
            public string highlight;
        }

        /// <summary>
        /// 运行计划任务的代理
        /// </summary>
        /// <param name="task">
        /// 调用的任务
        /// </param>
        /// <remarks>
        /// 调用定期或资源密集型任务时调用此方法
        /// </remarks>
        protected override async void OnInvoke(ScheduledTask task)
        {
            //TODO: 添加用于在后台执行任务的代码
            if (IsolatedStorageSettings.ApplicationSettings.Contains("UserKey"))
            {
                try
                {
                    DateTime TimeLast = (DateTime)IsolatedStorageSettings.ApplicationSettings["LastUpdatedTime"];
                    DateTime TimeNow = DateTime.Now;
                    TimeSpan TimeOffset = TimeNow - TimeLast;
                    int TimeSet = 0;
                    switch ((int)IsolatedStorageSettings.ApplicationSettings["UpdateInterval"])
                    {
                        case 0:
                            TimeSet = 2;
                            break;
                        case 1:
                            TimeSet = 4;
                            break;
                        case 2:
                            TimeSet = 8;
                            break;
                        case 3:
                            TimeSet = 12;
                            break;
                        case 4:
                            TimeSet = 24;
                            break;
                    }
                    if (Debugger.IsAttached || TimeOffset.TotalHours >= (double)TimeSet)
                    {
                        HttpEngine httpRequest = new HttpEngine();
                        string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/get_user_info?key=" + IsolatedStorageSettings.ApplicationSettings["UserKey"] + "&sb=Ricter&hash=" + new Random().Next());
                        JObject json = JObject.Parse(result);

                        List<Anime> subscriptionList = new List<Anime>();
                        int pushNumber = 0;
                        int updatedNumber = 0;
                        JArray subscription = json["data"]["subscription"] as JArray;
                        foreach (JObject item in subscription)
                        {
                            Anime anime = new Anime();
                            anime.name = (string)item["name"];
                            anime.epi = ((int)item["episode"]).ToString();
                            anime.highlight = ((int)item["isread"]).ToString();
                            subscriptionList.Add(anime);

                            if (anime.highlight != "0")
                                updatedNumber++;
                        }

                        subscriptionList.Sort((Anime x, Anime y) =>
                        {
                            if (x.highlight != "0" && y.highlight == "0")
                                return -1;
                            if (x.highlight == "0" && y.highlight != "0")
                                return 0;
                            if (x.highlight != "0" && y.highlight != "0")
                            {
                                if (x.highlight == "1" && y.highlight == "2")
                                    return -1;
                                if (x.highlight == "2" && y.highlight == "1")
                                    return 0;
                            }
                            return -1;
                        });

                        if (Debugger.IsAttached)
                        {
                            for (int i = 0; i < subscriptionList.Count; ++i)
                            {
                                Debug.WriteLine(subscriptionList[i].name);
                                Debug.WriteLine(subscriptionList[i].epi);
                                Debug.WriteLine(subscriptionList[i].highlight);
                                Debug.WriteLine("----------------------------");
                            }
                        }

                        string[] TileContent = new string[3];
                        string showName = "";
                        if (subscriptionList.Count >= 1 && subscriptionList[0].highlight != "0")
                        {
                            TileContent[0] = "订阅更新";
                            TileContent[1] = subscriptionList[0].name + " 更新到第 " + subscriptionList[0].epi + " 集";
                            showName = subscriptionList[0].name;
                        }
                        if (subscriptionList.Count >= 2 && subscriptionList[1].highlight != "0")
                        {
                            TileContent[2] = subscriptionList[1].name + " 更新到第 " + subscriptionList[1].epi + " 集";
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
                        if (pushNumber > 0)
                        {
                            ShellToast toast = new ShellToast();
                            toast.Title = showName + " 等 " + pushNumber.ToString() + " 个订阅更新，点击查看";
                            toast.Content = "";
                            toast.Show();
                        }
                        IsolatedStorageSettings.ApplicationSettings["LastUpdatedTime"] = TimeNow;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                    }
                }
                catch
                {

                }
            }
            NotifyComplete();
        }
    }
}