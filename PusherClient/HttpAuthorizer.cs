using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace PusherClient
{
    public class HttpAuthorizer: IAuthorizer
    {
        private Uri _authEndpoint;
        public HttpAuthorizer(string authEndpoint)
        {
            _authEndpoint = new Uri(authEndpoint);
        }
        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }
        private string GetCookie()
        {
            CookieContainer reqCookies = new CookieContainer();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://kuna.io/markets/btcusdt");
            req.Referer = "https://kuna.io/markets/btcusdt";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.80 Safari/537.36";
           // req.Headers.Add("referer", "https://kuna.io/markets/btcusdt");

         //   req.Method = "GET";
          //  req.ContentType = "application/x-www-form-urlencoded";
          //  req.ContentLength = data.Length;
            req.AllowAutoRedirect = true;
           // req.CookieContainer = Cookies;

           // Stream reqst = req.GetRequestStream(); // add form data to request stream

            //reqst.Write(data, 0, data.Length);
           // reqst.Close();

            //HttpWebResponse res = (HttpWebResponse)req.GetResponse(); // send request,get response
            using (var resp = req.GetResponse())
            {
                var trtr = resp.Headers.Get("Set-Cookie");
                return trtr;
                //var html = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            }
            //res.Cookies = req.CookieContainer.GetCookies(req.RequestUri);
            //foreach (Cookie c in res.Cookies)
            //{
            //  //  Cookies.Add(c);
            //}
        }
        private static void fixCookies(HttpWebRequest request, HttpWebResponse response)
        {
            for (int i = 0; i < response.Headers.Count; i++)
            {
                string name = response.Headers.GetKey(i);
                if (name != "Set-Cookie")
                    continue;
                string value = response.Headers.Get(i);
                foreach (var singleCookie in value.Split(','))
                {
                    Match match = Regex.Match(singleCookie, "(.+?)=(.+?);");
                    if (match.Captures.Count == 0)
                        continue;
                    response.Cookies.Add(
                        new Cookie(
                            match.Groups[1].ToString(),
                            match.Groups[2].ToString(),
                            "/",
                            request.Host.Split(':')[0]));
                }
            }
        }
        public string Authorize(string channelName, string socketId)
        {
            string authToken = null;
            
            using (var webClient = new MyWebClient())
            {
                
                webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.80 Safari/537.36";
                //webClient.Headers[HttpRequestHeader.Cookie] = "__cfduid=d9808aaa6b26f84fa4248883c72c653261552920587; _ga=GA1.2.1479391552.1552920587; _ym_uid=1553155035451434974; _ym_d=1553155035; sound=true; lang=ru; redirect_to=%2Fkeep-hope-alive%3F; _kuna_session=3b7f9dbdb6e354ceee84e0060bab3c62; XSRF-TOKEN=QRDLnCkddRIp7aXwHv%2B3yi%2FhgzFpby8LAvrMS%2BdQm%2BQ%3D; _gid=GA1.2.21850888.1560261630; market_id=btcusdt; _gat=1";
                string cook = GetCookie();
                webClient.Headers[HttpRequestHeader.Cookie] = cook;

                webClient.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                webClient.Headers[HttpRequestHeader.AcceptLanguage] = "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,uk;q=0.6";
                webClient.Headers["Origin"] = "https://kuna.io";
                webClient.Headers[HttpRequestHeader.Referer] = "https://kuna.io/markets/btcusdt";

                string data = String.Format("channel_name={0}&socket_id={1}", channelName, socketId);
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                authToken = webClient.UploadString(_authEndpoint, "POST", data);
            }
            return authToken;
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789%_!";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
