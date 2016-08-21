using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImportRunner
{
    public class RerolledSite : Site
    {
        private string UrlLogin = @"http://www.rerolled.org/login.php?do=login";
        private static string loginCookie = "";

        private static object mylock = new object();


        protected CookieCollection GetAuthorizationCookie()
        {
            lock (mylock)
            {


                if (cookie.IsTokenValid())
                {
                    return cookie.Cookie;
                }

                var cookies = new CookieContainer();
                var handler = new HttpClientHandler
                {
                    CookieContainer = cookies,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    AllowAutoRedirect = true
                };
                HttpClient wc = SetupClient(handler);
                wc.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                wc.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                wc.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
                wc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.94 Safari/537.36");


                HttpResponseMessage result = StartSession(wc).Result;
                var data = cookies.GetCookies(new Uri("http://www.rerolled.org"));

                cookie.Cookie = data;
                return data;
            }
        }



        protected internal override HttpClient SetupClient()
        {
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true
            };
            HttpClient wc = SetupClient(handler);
            wc.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            wc.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
            wc.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
            wc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.94 Safari/537.36");
            return wc;
        }

        protected internal Task<HttpResponseMessage> StartSession(HttpClient wc)
        {
            const string username = "lendarios";
            // const string password = "8cfe9c58703de5766164d59740490e1b";

            var formContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("vb_login_username", username),
                new KeyValuePair<string, string>("vb_login_password", string.Empty),
                new KeyValuePair<string, string>("vb_login_password_hint", "Password"),
                new KeyValuePair<string, string>("s", string.Empty),
                new KeyValuePair<string, string>("securitytoken", "guest"),
                new KeyValuePair<string, string>("do", "login"),
                new KeyValuePair<string, string>("vb_login_md5password", "8cfe9c58703de5766164d59740490e1b"),
                new KeyValuePair<string, string>("vb_login_md5password_utf", "8cfe9c58703de5766164d59740490e1b")
            });
            HttpResponseMessage result = wc.PostAsync(UrlLogin, formContent).Result;
            string code = result.Content.ReadAsStringAsync().Result;
            return Task.FromResult(result);
        }

        //public TaskResult ExecutTask(string url)
        //{
        //    Task result = ExecuteAgentTask(url);
        //    result.Wait();
        //    return result
        //}

        private static CookieExpiring cookie = new CookieExpiring();

        public override async Task<TaskResult> ExecuteAgentTask(int attachmentId)
        {
            string url = $"http://www.rerolled.org/attachment.php?attachmentid={attachmentId}";
            try
            {
                
                CookieCollection cookies = GetAuthorizationCookie();
                if (cookies.Count <= 2)
                {
                    throw new Exception("Could not start session ");

                }
                // now add cookie ? ithink, maybe not
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer.Add(cookies);
                using (var wc = SetupClient(handler))
                {
                    wc.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    wc.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                    wc.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
                    wc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.94 Safari/537.36");
                    HttpResponseMessage message = await wc.GetAsync(url);
                    var result = new TaskResult
                    {
                        FileBytes = await message.Content.ReadAsByteArrayAsync(),
                        MimeType = message.Content.Headers.ContentType,
                        FileName = message.Content.Headers.ContentDisposition.FileName,
                        Success = true,
                        Errors = null,
                        Url = url
                    };

                    return result;
                }
            }
            catch (Exception ex)
            {
                return new TaskResult
                {
                    Errors = ex,
                    Success = false,
                    Url = url
                };
            }
        }
    }
}