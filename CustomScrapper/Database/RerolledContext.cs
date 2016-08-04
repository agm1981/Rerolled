using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomScrapper.Database
{
   public  class RerolledContext : DbContext
    {
        
        public DbSet<UrltoScrape> UrlstoScrape
        {
            get;
            set;
        }
        public DbSet<UrlScraped> UrlsScraped
        {
            get;
            set;
        }


        public RerolledContext():base("RerolledContext")
        {
            Configuration.ProxyCreationEnabled = false;
        }
    }
}
