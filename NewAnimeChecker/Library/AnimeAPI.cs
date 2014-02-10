using HttpLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace NewAnimeChecker
{
    public class AnimeAPI
    {
        public class Anime
        {
            public int num;
            public string aid;
            public string name;
            public string status;
            public string epi;
            public string read;
            public string highlight;
            public string week;
            public string time;
        }

        public class AnimeDetail
        {
            public int id;
            public int type;
            public int copyright;
            public string versionInfo;
            public int episodeCount = 0;
            public int totalEpisodeCount = 0;
            public string title;
            public List<string> tags = new List<string>();
            public string directorName;
            public List<string> directors = new List<string>();
            public string actorName;
            public List<string> actors = new List<string>();
            public string year;
            public string poster;
            public string intro;
            public string area;
            public double score;
            public int displayType;
            public bool downloadable;

            public void Get(JObject animeDetail)
            {
                this.id          = (int)animeDetail["id"];
                this.type        = (int)animeDetail["type"];
                this.copyright   = (int)animeDetail["copyright"];
                this.versionInfo = (string)animeDetail["versionInfo"];

                if (animeDetail["episodeCount"] != null)
                    this.episodeCount = (int)animeDetail["episodeCount"];
                if (animeDetail["totalEpisodeCount"] != null)
                    this.totalEpisodeCount = (int)animeDetail["totalEpisodeCount"];

                this.title = (string)animeDetail["title"];

                foreach (string item in (animeDetail["tags"] as JArray))
                    tags.Add(item);

                this.directorName = (string)animeDetail["directorName"];
                foreach (string item in (animeDetail["directors"] as JArray))
                    directors.Add(item);

                this.actorName = (string)animeDetail["actorName"];
                foreach (string item in (animeDetail["actors"] as JArray))
                    actors.Add(item);

                this.year   = (string)animeDetail["year"];
                this.poster = (string)animeDetail["poster"];
                this.intro  = (string)animeDetail["intro"];
                this.area   = (string)animeDetail["area"];

                this.score        = (double)animeDetail["score"];
                this.displayType  = (int)animeDetail["displayType"];
                this.downloadable = (bool)animeDetail["downloadable"];

            }
        }

        public string key;
        public bool emailReminderStatus;
        public int updateNumber;
        public List<Anime> subscriptionList = new List<Anime>();
        public List<Anime> scheduleList = new List<Anime>();
        public List<Anime> searchResultList = new List<Anime>();
        public AnimeDetail animeDetail = new AnimeDetail();

        public AnimeAPI()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("UserKey"))
            {
                key = (string) IsolatedStorageSettings.ApplicationSettings["UserKey"];
            }
        }
        
        public async Task<bool> Login(string username, string password)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.PostAsync("http://api.anime.mmmoe.info/login?hash=" + new Random().Next(), "u=" + username + "&p=" + password);
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                key = (string)json["data"]["key"];
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> Register(string username, string password)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.PostAsync("http://api.anime.mmmoe.info/reg?hash=" + new Random().Next(), "u=" + username + "&p=" + password);
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                key = (string)json["data"]["key"];
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> GetSubscriptionList()
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/get_user_info?key=" + key + "&sb=Ricter&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);

                subscriptionList.Clear();
                updateNumber = 0;
                JArray subscription = json["data"]["subscription"] as JArray;
                foreach (JObject item in subscription)
                {
                    Anime anime     = new Anime();
                    anime.aid       = (string)item["id"];
                    anime.name      = (string)item["name"];
                    anime.status    = ((int)item["isover"]).ToString();
                    anime.epi       = ((int)item["episode"]).ToString();
                    anime.read      = ((int)item["watch"]).ToString();
                    anime.highlight = ((int)item["isread"]).ToString();
                    subscriptionList.Add(anime);

                    if (anime.highlight != "0")
                        updateNumber++;
                }

                subscriptionList.Sort((Anime a, Anime b) =>
                {
                    if (a.highlight != "0" && b.highlight == "0")
                        return -1;
                    return 0;
                });


                for (int i = 0; i < subscriptionList.Count; ++i)
                    subscriptionList[i].num = i + 1;

                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> AddAnime(string aid)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/add_anime?key=" + key + "&aid=" + aid + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> DelAnime(string aid)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/del_anime?key=" + key + "&aid=" + aid + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> GetEmailReminderStatus()
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/get_user_info?key=" + key + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);

                int status = (int)json["data"]["email"];
                if (status == 1)
                    emailReminderStatus = true;
                else
                    emailReminderStatus = false;
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> SetEmailReminderStatus(bool status)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string enable;
                if (status)
                    enable = "1";
                else
                    enable = "0";
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/email_reminder_set?key=" + key + "&enable=" + enable + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                emailReminderStatus = status;
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> AddHighlight(string aid, string status)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/highlight?key=" + key + "&aid=" + aid + "&status=" + status + "&method=add&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> DelHighlight(string aid)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/highlight?key=" + key + "&aid=" + aid + "&method=del&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> SetReadEpi(string aid, string epi)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/epiedit?key=" + key + "&aid=" + aid + "&epi=" + epi + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> ChangePsw(string oldPsw, string newPsw)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/changepw?key=" + key + "&oldpw=" + oldPsw + "&newpw=" + newPsw + "&hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<bool> GetUpdateSchedule()
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.anime.mmmoe.info/get_update_schedule?hash=" + new Random().Next());
                JObject json = JObject.Parse(result);
                ErrorProcessor(json);

                scheduleList.Clear();
                JArray list = json["data"]["update_list"] as JArray;
                foreach (JObject item in list)
                {
                    Anime anime = new Anime();
                    anime.name  = (string)item["name"];
                    anime.time  = (string)item["time"];

                    string url  = (string)item["url"];
                    if (url.Contains("vod"))
                    {
                        anime.aid = url.Substring(27, 5);
                    }
                    else
                    {
                        anime.aid = url.Substring(url.Length - 5, 5);
                    }

                    int week = (int)item["week"];
                    switch (week)
                    {
                        case 0:
                            anime.week = "星期天";
                            break;
                        case 1:
                            anime.week = "星期一";
                            break;
                        case 2:
                            anime.week = "星期二";
                            break;
                        case 3:
                            anime.week = "星期三";
                            break;
                        case 4:
                            anime.week = "星期四";
                            break;
                        case 5:
                            anime.week = "星期五";
                            break;
                        case 6:
                            anime.week = "星期六";
                            break;
                    }

                    scheduleList.Add(anime);
                }

                for (int i = 0; i < scheduleList.Count; ++i)
                    scheduleList[i].num = i + 1;

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> GetAnimeDetail(string aid)
        {
            try
            {
                string url = "http://data.pad.kankan.com/mobile/detail/" + aid[0] + aid[1] + "/" + aid + ".json";
                Debug.WriteLine(url);
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync(url);
                Debug.WriteLine(result);
                JObject json = JObject.Parse(result);

                animeDetail.Get(json);

                return true;
            }
            catch
            {
                throw;
            }
        }


/*
 *      - 错误判断
 *      
 */

        public enum ERROR
        {
            ERROR_NONE = 0,
            ERROR_INVALID_PSW,
            ERROR_INVALID_KEY,
            ERROR_INVALID_ANIME,
            ERROR_INVALID_EPI,
            ERROR_EXIST_EMAIL,
            ERROR_EXIST_ANIME,
            ERROR_UNKNOWN
        };

        public ERROR lastError;

        private void ErrorProcessor(JObject json)
        {
            Debug.WriteLine(json);

            int status = (int)json["status"];
            if (status == 200) 
            {
                lastError = ERROR.ERROR_NONE;
                return;
            }

            string message;
            switch (status)
            {
                case 400:
                    lastError = ERROR.ERROR_INVALID_KEY;
                    message   = "您的帐号授权已过期，请重新登陆";
                    break;
                case 401:
                    lastError = ERROR.ERROR_INVALID_PSW;
                    message   = "密码错误";
                    break;
                case 407:
                    lastError = ERROR.ERROR_EXIST_EMAIL;
                    message   = "您填写的邮箱已经被注册，请直接登陆或换个邮箱重试";
                    break;
                case 501:
                    lastError = ERROR.ERROR_INVALID_ANIME;
                    message   = "抱歉，您添加的内容不存在于片库中";
                    break;
                case 502:
                    lastError = ERROR.ERROR_INVALID_EPI;
                    message   = "集数不符合规范";
                    break;
                case 503:
                    lastError = ERROR.ERROR_EXIST_ANIME;
                    message   = "此订阅已经在您的订阅列表中，请不要重复添加";
                    break;
                default:
                    lastError = ERROR.ERROR_UNKNOWN;
                    message   = "发生了错误，请重试";
                    break;
            }

            message += "\nerrorcode: " + status.ToString();

            throw new Exception(message);
        }
    }
}
