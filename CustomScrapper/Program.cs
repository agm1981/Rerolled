using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomScrapper
{
    class Program
    {
        /// <summary>
        /// This application is designed in thee separate parts
        /// Part 1 Thread reads from the table "UrlToScrape" in DB, the urls to scrape. It connects and gets the HTML
        /// It parses the HTML, and detects the links. It elimininates badly formed links, or links that go outside the forum,
        /// and it checks the table "UrlsScraped", if not there it adds the URLs to the first table "UrlToScrape" and saves the Html into the file system.
        /// rinse and repeat
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
        }
    }
}
