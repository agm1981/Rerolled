using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CustomScrapper.Database
{
    public class UrltoScrape
    {
        public int UrltoScrapeId
        {
            get;
            set;
        }

        public string Url
        {

            get;
            set;
        }
        
    }
}
