using System;
using System.Collections.Generic;
using System.Data;

namespace Common.DataLayer
{
    public class AllDownloadLogReppository : BaseRepository
    {
        private readonly Func<IDataReader, DownloadLog> mapLogs = dr => new DownloadLog
        {
            ImageId = dr.Get<int>("ImageId"),
            Error = dr.GetSafe<string>("Error")};

        private Func<IDataReader, int> mapIds = dr => dr.Get<int>("ImageId");

        public IEnumerable<DownloadLog> GetAllLogs()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT * FROM DownloadLogs",
                null,
                mapLogs
            );
        }
        //public IEnumerable<Thread> GetAllLogThreadsToExport()
        //{
        //    return sqlH.ExecuteSet(
        //        CommandType.Text,
        //        @"SELECT * FROM Threads where newThreadId is null and newForumid is not null",
        //        null,
        //        mapThreads
        //    );
        //}

        public void InsertLog(DownloadLog item)
        {
            string sql = @"INSERT INTO [dbo].[DownloadLogs]
                    (  [ImageId]    ,[Error])
                    VALUES
                      (@ImageId     ,@Error)";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@ImageId", item.ImageId);
                           c.AddWithValue("@Error", item.Error);
                       }
                   );
        }


        public void UpdateLog(DownloadLog item)
        {
            string sql = @"Update [dbo].[DownloadLogs]
                    set Error = @Error
                    where ImageId = @ImageId                    
                   ";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@ImageId", item.ImageId);
                           c.AddWithValue("@Error", item.Error);
                       }
                   );
        }

      

        public void InsertLog(IEnumerable<DownloadLog> logToAdd)
        {
            foreach (DownloadLog log in logToAdd)
            {
                InsertLog(log);
            }
        }


    }
}

