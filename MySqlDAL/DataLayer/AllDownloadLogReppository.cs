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
            Error = dr.GetSafe<string>("Error"),
            UserId = dr.GetSafe<int>("UserId"),
            FileHash = dr.GetSafe<string>("FileHash"),
            FileName = dr.GetSafe<string>("FileName"),
            FileSize = dr.GetSafe<int>("FileSize"),
            Height = dr.GetSafe<int>("Height"),
            Width = dr.GetSafe<int>("Width"),
            MimeType = dr.GetSafe<string>("MimeType"),
            UploadDate = dr.GetSafe<int>("UploadDate")
        };

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
        public DownloadLog GetLogById(int imageId)
        {
            return sqlH.ExecuteSingle(
                CommandType.Text,
                @"SELECT * FROM DownloadLogs where imageID = @imageId",
                c =>
                {
                    c.AddWithValue("@ImageId", imageId);
                },
                mapLogs
            );
        }
        public IEnumerable<int> GetAllLogsToComplete()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT imageId FROM DownloadLogs where UploadDate is null and Error is null",
                null,
                mapIds
            );
        }

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
                    set  [Error]    = @Error
                        ,[UserId]   = @UserId
                        ,[FileHash] = @FileHash
                        ,[FileName] = @FileName
                        ,[FileSize] = @FileSize
                        ,[Height]   = @Height
                        ,[Width]    = @Width
                        ,[MimeType] = @MimeType
                        ,[UploadDate] = @UploadDate
                    where ImageId = @ImageId                    
                   ";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@ImageId", item.ImageId);
                           c.AddWithValue("@Error", item.Error);
                           c.AddWithValue("@UserId", item.UserId);
                           c.AddWithValue("@FileHash", item.FileHash);
                           c.AddWithValue("@FileName", item.FileName);
                           c.AddWithValue("@FileSize", item.FileSize);
                           c.AddWithValue("@Height", item.Height);
                           c.AddWithValue("@Width", item.Width);
                           c.AddWithValue("@MimeType", item.MimeType);
                           c.AddWithValue("@UploadDate", item.UploadDate);
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

