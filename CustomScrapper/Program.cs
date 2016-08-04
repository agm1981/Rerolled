using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CustomScrapper.Database;

namespace CustomScrapper
{
    class Program
    {
        private static readonly ConcurrentDictionary<string, string> UrlsToScrape = new ConcurrentDictionary<string,string>();

        private static readonly ConcurrentDictionary<string, string> UrlsScraped = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// This application is designed in thee separate parts
        /// Part 1 Thread reads up to 20 records from the table "UrlToScrape" in DB, the urls to scrape. It connects and gets the HTML
        /// It parses the HTML, and detects the links. It elimininates badly formed links, or links that go outside the forum,
        /// and it checks the table "UrlsScraped", if not there it adds the URLs to the first table "UrlToScrape" and saves the Html into the file system.
        /// rinse and repeat
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            LoadData();
            //Task ReadUrlFromDbAndFetchHtml
        }

        private static void LoadData()
        {
            List<string> urlstoScrape = new List<string>();
            List<string> urlsScraped = new List<string>();
            using (var context = new RerolledContext())
            {
                urlstoScrape.AddRange(context.UrlstoScrape.AsNoTracking().Select(c=>c.Url).ToList());
                urlsScraped.AddRange(context.UrlsScraped.AsNoTracking().Select(c => c.Url).ToList());
            }
            foreach (string url in urlstoScrape)
            {
                UrlsToScrape.TryAdd(url, url);
            }
            foreach (string url in urlsScraped)
            {
                UrlsScraped.TryAdd(url, url);
            }
        }
    }
}
