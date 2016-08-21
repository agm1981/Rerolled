using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using Common;
using Common.DataLayer;

namespace ImportRunner
{
    public class RerolledImageDownloader
    {

        private ConcurrentDictionary<int, bool> ItemsToDownload
        {
            get;
            set;
        }


        public RerolledImageDownloader()
        {
            ItemsToDownload = new ConcurrentDictionary<int, bool>();

        }



        public async Task Start()
        {
            // Load List of saved oned

            AllDownloadLogReppository rep = new AllDownloadLogReppository();

            HashSet<int> logsSaved = new HashSet<int>(rep.GetAllLogs().Select(c => c.ImageId));

            HashSet<int> allItemsToadd = new HashSet<int>();
            for (int i = 1; i <= 137301; i++)
            {
                allItemsToadd.Add(i);
            }
            allItemsToadd.RemoveWhere(c => logsSaved.Contains(c));
            foreach (int i in allItemsToadd)
            {
                ItemsToDownload.TryAdd(i, true);
            }

            while (ItemsToDownload.Any())
            {
                HashSet<int> batchToGet = new HashSet<int>(ItemsToDownload.Take(100).Select(c => c.Key));
                Parallel.ForEach(batchToGet, attachmentId =>
                {
                    RerolledSite site = new RerolledSite();

                    TaskResult result = site.ExecuteAgentTask(attachmentId).Result;
                    
                    SaveResultToDbAndFileSystem(result);
                    
                });
                Console.WriteLine($"{ItemsToDownload.Count}");
            }





            int g = 0;
        }


        private void SaveResultToDbAndFileSystem(TaskResult result)
        {
            string error = null;
            AllDownloadLogReppository rep = new AllDownloadLogReppository();
            if (!result.Success)
            {
                DownloadLog logError = new DownloadLog
                {
                    Error = result.Errors.Message,
                    ImageId = result.ImageId
                };
                rep.InsertLog(logError);
                bool dd;
                ItemsToDownload.TryRemove(result.ImageId, out dd);
                return;
            }
            try
            {
                var fileName = result.GetFileName();
                string filePath = ConfigurationManager.AppSettings["OutputFolder"] + $@"\{fileName}";
                File.WriteAllBytes(filePath, result.FileBytes);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            DownloadLog log = new DownloadLog
            {
                Error = error,
                ImageId = result.ImageId
            };
            
            rep.InsertLog(log);
            bool d;
            ItemsToDownload.TryRemove(result.ImageId, out d);
        }

    }


}
