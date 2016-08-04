using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomScrapper.Database;

namespace CustomScrapper
{
    public class HtmlScrapperWorker
    {
        private static object mylock = new object();


        public Task Work()
        {
            lock (mylock)
            {
                using (var context = new RerolledContext())
                {
                    // read up to 20 results.
                    var urlsToWorkWith = context.UrlstoScrape.Take(20).ToList();
                }
            }
        }
    }
}
