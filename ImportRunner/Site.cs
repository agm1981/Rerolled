using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace ImportRunner
{
    [Serializable]
    public abstract class Site
    {



        [NotMapped]
        public Encoding SiteEncoding
        {
            get;
            set;

        }

        public Site()
        {
            SiteEncoding = Encoding.UTF8;
        }


        public virtual  Task<TaskResult> ExecuteAgentTask(int id)
        {
            return null;
        }



        // Agents just grab HTML
        public virtual async Task<TaskResult> ExecuteAgentTask(string url)
        {

            try
            {
                using (var wc = this.SetupClient())
                {
                    HttpResponseMessage message = await wc.GetAsync(url);
                    var result = new TaskResult
                    {
                        FileBytes = await message.Content.ReadAsByteArrayAsync(),
                        MimeType = message.Content.Headers.ContentType,
                        FileName = message.Content.Headers.ContentDisposition.FileName,
                        Success = true,
                        Errors = null
                    };

                    return result;
                }
            }
            catch (Exception ex)
            {
                return new TaskResult
                {
                    Errors = ex,
                    Success = false
                };
            }
        }

        protected internal virtual HttpClient SetupClient()
        {
            return new HttpClient();
        }

        protected virtual HttpClient SetupClient(HttpClientHandler h)
        {
            return new HttpClient(h);
        }

        /// <summary>
        /// Use this for User name / password combination
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected virtual HttpClient SetupClient(string userName, string password)
        {
            HttpClientHandler credentials = new HttpClientHandler
            {
                Credentials = new NetworkCredential(userName, password)
            };
            HttpClient wc = new HttpClient(credentials);
            return wc;
        }

        /// <summary>
        /// Use this when u have to send a bunch of cookies to the server
        /// </summary>
        /// <param name="cookieFromServer"></param>
        /// <param name="uriBase"></param>
        /// <returns></returns>
        protected virtual HttpClient SetupClient(string cookieFromServer, Uri uriBase)
        {
            string[] cookiesSplited = cookieFromServer.Split(';');
            Func<string, KeyValuePair<string, string>> parseCookie = x =>
            {
                if (x.Contains('='))
                {
                    string leftSide = x.Substring(0, x.IndexOf('='));
                    string rightSide = x.Substring(x.IndexOf('=') + 1);
                    return new KeyValuePair<string, string>(leftSide.Trim(), rightSide.Trim());
                }
                else
                {
                    return new KeyValuePair<string, string>();
                }
            };
            List<KeyValuePair<string, string>> cookiesSeparated = cookiesSplited.Select(cookie => parseCookie(cookie)).ToList();


            var cookies = new CookieContainer();

            foreach (KeyValuePair<string, string> pair in cookiesSeparated.Where(c => !string.IsNullOrWhiteSpace(c.Value)))
            {
                cookies.Add(uriBase, new Cookie(pair.Key, pair.Value));
            }

            var handler = new HttpClientHandler()
            {
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var wc = new HttpClient(handler);
            wc.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            wc.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
            wc.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
            wc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.86 Safari/537.36");
            return wc;
        }

        // Grab HTML (using ExecuteAgentTask) and then diff it for changes









    }
}
