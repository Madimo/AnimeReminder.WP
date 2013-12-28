using HttpLibrary;
using Newtonsoft.Json; 
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
                string result = await httpRequest.PostAsync("http://api.ricter.info/login", "u=" + username + "&p=" + password + "&hash=" + new Random().Next());
                JsonData data = ErrorProcessor(result);
                key = data.key;
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
                string result = await httpRequest.PostAsync("http://api.ricter.info/reg", "u=" + username + "&p=" + password + "&hash=" + new Random().Next());
                JsonData data = ErrorProcessor(result);
                key = data.key ;
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
                string result = await httpRequest.GetAsync("http://api.ricter.info/get_user_info?key=" + key + "&hash=" + new Random().Next());
                JsonData data = ErrorProcessor(result);
                subscriptionList.Clear();
                foreach (JsonData animeItem in data.subscription)
                {
                    Anime anime = new Anime();
                    anime.aid = animeItem.id.ToString();
                    anime.name = animeItem.name;
                    anime.status = animeItem.isover.ToString();
                    anime.epi = animeItem.episode.ToString();
                    anime.read = animeItem.watch.ToString();
                    anime.highlight = animeItem.isread.ToString();
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
                string result = await httpRequest.GetAsync("http://api.ricter.info/add_anime?key=" + key + "&aid=" + aid + "&hash=" + new Random().Next());
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
                string result = await httpRequest.GetAsync("http://api.ricter.info/del_anime?key=" + key + "&aid=" + aid + "&hash=" + new Random().Next());
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
                string result = await httpRequest.GetAsync("http://api.ricter.info/get_user_info?key=" + key + "&hash=" + new Random().Next());
                JsonData data = ErrorProcessor(result);
                if (data.email == 0)
                    emailReminderStatus = false;
                else if (data.email == 1)
                    emailReminderStatus = true;
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
                string result = await httpRequest.GetAsync("http://api.ricter.info/email_reminder_set?key=" + key + "&enable=" + enable + "&hash=" + new Random().Next());
                ErrorProcessor(result);
                emailReminderStatus = status;
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Highlight(string aid, string method)
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.ricter.info/highlight?key=" + key + "&aid=" + aid + "&method=" + method + "&hash=" + new Random().Next());
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
                string result = await httpRequest.GetAsync("http://api.ricter.info/epiedit?key=" + key + "&aid=" + aid + "&epi=" + epi + "&hash=" + new Random().Next());
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
                string result = await httpRequest.PostAsync("http://api.ricter.info/changepw", "key=" + key + "&oldpw=" + oldPsw + "&newpw=" + newPsw + "&hash=" + new Random().Next());
                ErrorProcessor(result);
                return true;
            }
            catch
            {
                throw;
            }
        }
        
        //未完
        public async Task<bool> GetUpdateSchedule()
        {
            try
            {
                HttpEngine httpRequest = new HttpEngine();
                string result = await httpRequest.GetAsync("http://api.ricter.info/get_update_schedule?hash=" + new Random().Next());
                JsonData data = ErrorProcessor(result);
                scheduleList.Clear();

                foreach (JsonData animeItem in data.update_list)
                {
                    Anime anime = new Anime();
                    anime.date = animeItem.week;
                    anime.aid = animeItem.url.Split('/')[4];
                    anime.name  = animeItem.name;
                    anime.time = animeItem.time;
                    //这里加一个IF判断今天是周几，取week以及week+1的
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
            ERROR_INVALID_ANIME,
            ERROR_INVALID_EPI,
            ERROR_SYSTEM,
            ERROR_UNKNOWN,
            METHOD_NOT_ALLOW
        };

        public ERROR lastError;

        private JsonData ErrorProcessor(string result)
        {
            JsonData json = JsonConvert.DeserializeObject<JsonData>(result);
            if (json.status == 500)
                lastError = ERROR.ERROR_UNKNOWN;
            if (json.status == 400)
                lastError = ERROR.ERROR_INVALID_KEY;
            if (json.status == 501)
                lastError = ERROR.ERROR_INVALID_ANIME;
            if (json.status == 502)
                lastError = ERROR.ERROR_INVALID_EPI;
            if (json.status == 503)
                lastError = ERROR.ERROR_EXIST_ANIME;
            if (json.status == 403)
                lastError = ERROR.METHOD_NOT_ALLOW;
            if (json.status == 401)
                lastError = ERROR.ERROR_INVALID_PSW;
            if (json.status == 407)
                lastError = ERROR.ERROR_EXIST_EMAIL;

            if (json.status != 500)
            {
                throw new Exception(json.message);
            };
            return json.data;
        }

        public class JsonData
        {
            public int status
            {
                get;
                set;
            }

            public string message
            {
                get;
                set;
            }

            public JsonData data
            {
                get;
                set;
            }

            public string key
            {
                get;
                set;
            }

            public int email
            {
                get;
                set;
            }

            public List<JsonData> subscription
            {
                get;
                set;
            }

            public int isover
            {
                get;
                set;
            }

            public int episode
            {
                get;
                set;
            }

            public int watch
            {
                get;
                set;
            }

            public int isread
            {
                get;
                set;
            }

            public int id
            {
                get;
                set;
            }

            public string name
            {
                get;
                set;
            }

            public List<JsonData> update_list
            {
                get;
                set;
            }

            public string week
            {
                get;
                set;
            }

            public string time
            {
                get;
                set;
            }

            public string url
            {
                get;
                set;
            }

        }
    }
}


