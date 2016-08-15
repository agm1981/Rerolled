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
            
            
            //new UserNameExporterToMySql().Start(); // Step 1 import Users. verify on xf_users table
            
            
            
            //new ThreadExporterToMySql().AddForumId(); // Step 2 do this one to get the forum id locally. // verify on sql.Threads

            // Step 3 Run the update thread starter manual SQL if you have not, verufy the thread table


            // Step 4//
            // new ThreadExporterToMySql().InsertAllThreads();
           
            // and finally
            new PostExporterToMysql().Start();

        }
    }
}
