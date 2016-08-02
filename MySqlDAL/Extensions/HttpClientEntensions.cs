using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace MySqlDAL.Extensions
{
    public static class HttpClientEntensions
    {
        /// <summary>
        /// Sets up an agent similar to Chrome and sends the cookie provided to the server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieStr"></param>
        /// <returns></returns>
        public static HttpClient SetupClientWithCookie(string url,string cookieStr)
        {
            string[] cookiesSplited = cookieStr.Split(';');
            Func<string, KeyValuePair<string, string>> parseCookie = x =>
            {
                if (x.Contains('='))
                {
                    string leftSide = x.Substring(0, x.IndexOf('='));
                    string rightSide = x.Substring(x.IndexOf('=') + 1);
                    return new KeyValuePair<string, string>(leftSide.Trim(), rightSide.Trim());
                }
                return new KeyValuePair<string, string>();
            };
            List<KeyValuePair<string, string>> cookiesSeparated = cookiesSplited.Select(cookie => parseCookie(cookie)).ToList();


            var cookies = new CookieContainer();
            Uri uriBase = new Uri(url);
            foreach (KeyValuePair<string, string> pair in cookiesSeparated.Where(c => !string.IsNullOrWhiteSpace(c.Value)))
            {
                cookies.Add(uriBase, new Cookie(pair.Key, pair.Value));
            }

            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var wc = new HttpClient(handler);
            wc.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            wc.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
            wc.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
            wc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.94 Safari/537.36");
            return wc;
        }
    }
}
