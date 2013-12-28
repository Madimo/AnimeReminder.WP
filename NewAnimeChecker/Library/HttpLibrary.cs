using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// PM> Install-Package Microsoft.Bcl.Async

namespace HttpLibrary
{
    public class HttpEngine
    {
        public virtual async Task<Stream> PostAsync(string RequestUrl, string Context)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(RequestUrl, UriKind.Absolute));
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";

                using (Stream stream = await httpWebRequest.GetRequestStreamAsync())
                {
                    byte[] entryBytes = Encoding.UTF8.GetBytes(Context);
                    stream.Write(entryBytes, 0, entryBytes.Length);
                }

                WebResponse response = await httpWebRequest.GetResponseAsync();
                return response.GetResponseStream();
            }
            catch
            {
                throw new Exception("网络错误，请检查网络连接或重试");
            }
        }

        public virtual async Task<string> GetAsync(string RequestUrl)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(RequestUrl, UriKind.Absolute));
                httpWebRequest.Method = "GET";
                httpWebRequest.UserAgent = "NewAnimeChecker.Mobile.WindowsPhone.Ver1~3";
                WebResponse response = await httpWebRequest.GetResponseAsync();
                Stream streamResult = response.GetResponseStream();
                StreamReader sr = new StreamReader(streamResult, Encoding.UTF8);
                string returnValue = sr.ReadToEnd();
                return returnValue;
            }
            catch
            {
                throw new Exception("网络错误，请检查网络连接或重试");
            }
        }

        public virtual async Task<Stream> GetAsyncForData(string RequestUrl)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(RequestUrl, UriKind.Absolute));
                httpWebRequest.Method = "GET";
                WebResponse response = await httpWebRequest.GetResponseAsync();
                Stream streamResult = response.GetResponseStream();
                return streamResult;
            }
            catch
            {
                throw new Exception("网络错误，请检查网络连接或重试");
            }
        }
    }
}
