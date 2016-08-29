using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Common;
using Common.DataLayer;

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
            //new MetaDataExtrator().Start();

            //new FileName().Start();
        }
    }

    internal class FileName
    {
        public void Start()
        {
            string prefix = "rrr_img_";
            string folder = @"e:\rr_images";
            HashSet<FileInfo> fileNamess = new HashSet<FileInfo>(new DirectoryInfo(folder).GetFiles());
            foreach (FileInfo fileInfo in fileNamess)
            {
                fileInfo.MoveTo($@"{folder}\{prefix}{fileInfo.Name}");
            }
        }
    }

    public class MetaDataExtrator
    {

        private HashSet<int> workListItems = new HashSet<int>();
        /// <summary>
        /// here we loop on the pictures and we
        /// </summary>
        public void Start()
        {
            AllDownloadLogReppository rep =new AllDownloadLogReppository();

            workListItems = new HashSet<int>(rep.GetAllLogsToComplete());

            IEnumerable<int> batch = workListItems.Take(1000);
            DirectoryInfo dir = new DirectoryInfo(ConfigurationManager.AppSettings["OutputFolder"]);
            foreach (int imageId in batch)
            {
                // Load from th efile ssytem,
                FileInfo fileData = dir.GetFiles($"{imageId}.*").First();

                DownloadLog log = rep.GetLogById(imageId);
                log.CompleteFields(fileData);

            }

        }
    }
}
