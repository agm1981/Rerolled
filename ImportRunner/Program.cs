using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //for debugg
            //new HtmlLoader().Start();
            //new ConvertMessages().Start();

            // actual prod process
            //new UserNameExporterToMySql().Start();
            //new ThreadExporterToMySql().AddForumId(); // do this one to get the forum id locally. 
            new ThreadExporterToMySql().InsertAllThreads();

        }
    }
}
