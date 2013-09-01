using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using HttpMethod;

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
                        string result = await httpRequest.GetAsync("http://apianime.ricter.info/get_subscription_list?key=" + IsolatedStorageSettings.ApplicationSettings["UserKey"] + "&hash=" + new Random().Next());
                        if (result.Contains("ERROR_"))
                        {
                            throw new Exception(result);
                        }
                        string[] list = result.Split('\n');
                        int updatedNumber = 0;
                        string[] TileContent = new string[3] { "", "", "" };
                        string ShowName = "";
                        for (int i = 0; i < list.Length; ++i)
                        {
                            string[] item = list[i].Split('|');
                            if (item.Length < 5)
                                continue;
                            string isUpdate = item[0];
                            string name     = item[2];
                            string epi      = item[3];
                            if (isUpdate == "1")
                            {
                                updatedNumber++;
                                if (updatedNumber == 1)
                                {
                                    TileContent[0] = " ";
                                    TileContent[1] = name + " 更新到第 " + epi + " 集";
                                    ShowName = name;
                                }
                                else if (updatedNumber == 2)
                                {
                                    TileContent[2] = name + " 更新到第 " + epi + " 集";
                                }
                            }
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
                        if (updatedNumber > 0)
                        {
                            ShellToast toast = new ShellToast();
                            toast.Title = ShowName + " 等 " + updatedNumber.ToString() + " 个订阅更新，点击查看";
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