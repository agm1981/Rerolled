using System.Text;
using System.Web;
using Common;

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


            //new UserNameExporterToMySql().Start(); // Step 1 import Users. verify on xf_user table



            //new ThreadExporterToMySql().AddForumId(); // Step 2 do this one to get the forum id locally. // verify on sql.Thread

            // Step 3 Run the update thread starter manual SQL if you have not, verify the thread table xf_thread


            // Step 4//
            //new ThreadExporterToMySql().InsertAllThreads(); // verify the thread table xf_thread

            // and finally
            //new PostExporterToMysql().Start();

            // Connect to reroleld and download picture
            //new RerolledImageDownloader().Start().Wait();

            // Image metadata extraector


            //new FileName().Start();
            new PictureUrlConverter().Start();
        }
    }
}
