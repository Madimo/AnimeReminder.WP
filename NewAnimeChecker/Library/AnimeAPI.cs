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
            public string date;
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/login?u=" + username + "&p=" + password + "&hash=" + new Random().Next());
                ErrorProcessor(result);
                key = result;
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/reg?u=" + username + "&p=" + password + "&hash=" + new Random().Next());
                ErrorProcessor(result);
                key = result;
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/get_subscription_list?key=" + key + "&sb=Ricter&hash=" + new Random().Next());
                ErrorProcessor(result);

                subscriptionList.Clear();
                updateNumber = 0;
                string[] list = result.Split('\n');
                for (int i = 0; i < list.Length; ++i)
                {
                    string[] item = list[i].Split('|');
                    if (item.Length < 6)
                        continue;
                    Anime anime     = new Anime();
                    anime.aid       = item[0];
                    anime.name      = item[1];
                    anime.status    = item[2];
                    anime.epi       = item[3];
                    anime.read      = item[4];
                    anime.highlight = item[5];
                    subscriptionList.Add(anime);

                    if (anime.highlight != "0")
                        updateNumber++;
                }
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/add_anime?key=" + key + "&aid=" + aid + "&hash=" + new Random().Next());
                ErrorProcessor(result);
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/del_anime?key=" + key + "&aid=" + aid + "&hash=" + new Random().Next());
                ErrorProcessor(result);
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/email_reminder_get?key=" + key + "&hash=" + new Random().Next());
                ErrorProcessor(result);
                if (result == "0")
                    emailReminderStatus = false;
                else if (result == "1")
                    emailReminderStatus = true;
                else
                    ErrorProcessor("ERROR_UNKNOWN");
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/email_reminder_set?key=" + key + "&enable=" + enable + "&hash=" + new Random().Next());
                ErrorProcessor(result);
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/add_highlight?key=" + key + "&aid=" + aid + "&status=" + status + "&hash=" + new Random().Next());
                ErrorProcessor(result);
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/del_highlight?key=" + key + "&aid=" + aid + "&hash=" + new Random().Next());
                ErrorProcessor(result);
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/epiedit?key=" + key + "&aid=" + aid + "&epi=" + epi + "&hash=" + new Random().Next());
                ErrorProcessor(result);
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/changepw?key=" + key + "&oldpw=" + oldPsw + "&newpw=" + newPsw + "&hash=" + new Random().Next());
                ErrorProcessor(result);
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
                string result = await httpRequest.GetAsync("http://api2.ricter.info/get_update_schedule?hash=" + new Random().Next());

                scheduleList.Clear();
                string[] list = result.Split('\n');
                for (int i = 0; i < list.Length; ++i)
                {
                    string[] item = list[i].Split('|');
                    if (item.Length < 4)
                        continue;
                    Anime anime = new Anime();
                    anime.date  = item[0];
                    anime.aid   = item[1];
                    anime.name  = item[2];
                    anime.time  = item[3];
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

        public async Task<bool> SearchAnime(string keyword)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("");

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
 *      ERROR_INVALID_DATA    数据提交错误，指参数不足或其他错误
 *      ERROR_INVALID_PSW     密码错误，在登陆以及修改密码时候可能遇到
 *      ERROR_INVALID_KEY     KEY错误，指KEY不存在，常见错误
 *      ERROR_INVALID_AID     AID错误，AID指的是订阅新番的ID号码，返回此错误原因是订阅可能不存在或者是电影
 *      ERROR_INVALID_NEWPW   新密码错误，指修改密码时新密码过长或过短
 *      ERROR_EXIST_EMAIL     EMAIL已被注册
 *      ERROR_EXIST_ANIME     订阅重复添加时的错误
 *      ERROR_INVALID_EPI     用户看到的集数错误，可能是大于总集数或小于0
 *      ERROR_SYSTEM          系统错误，和APP无关
 */
        public enum ERROR
        {
            ERROR_NONE = 0,
            ERROR_INVALID_PSW,
            ERROR_INVALID_KEY,
            ERROR_EXIST_EMAIL,
            ERROR_EXIST_ANIME,
            ERROR_SYSTEM,
            ERROR_UNKNOWN
        };

        public ERROR lastError;

        private void ErrorProcessor(string result)
        {
            if (result.Contains("ERROR_"))
            {
                if (result.Contains("ERROR_INVALID_PSW"))
                {
                    lastError = ERROR.ERROR_INVALID_PSW;
                    throw new Exception("密码错误");
                }
                if (result.Contains("ERROR_INVALID_KEY"))
                {
                    lastError = ERROR.ERROR_INVALID_KEY;
                    throw new Exception("您的帐号授权已过期，请重新登陆");
                }
                if (result.Contains("ERROR_EXIST_EMAIL"))
                {
                    lastError = ERROR.ERROR_EXIST_EMAIL;
                    throw new Exception("您填写的邮箱已经被注册，请直接登陆或换个邮箱重试");
                }
                if (result.Contains("ERROR_EXIST_ANIME"))
                {
                    lastError = ERROR.ERROR_EXIST_ANIME;
                    throw new Exception("此订阅已经在您的订阅列表中，请不要重复添加");
                }
                if (result.Contains("ERROR_SYSTEM"))
                {
                    lastError = ERROR.ERROR_SYSTEM;
                    throw new Exception("服务器内部错误，请稍后重试");
                }

                lastError = ERROR.ERROR_UNKNOWN;
                throw new Exception("发生了错误，请重试");
            }
        }
    }
}
