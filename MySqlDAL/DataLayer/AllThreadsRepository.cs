using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataLayer
{
    public class AllThreadsRepository : BaseRepository
    {
        private Func<IDataReader, Thread> mapThreads = dr => new Thread
        {
            ThreadId = dr.Get<int>("ThreadId"),
            ThreadName = dr.Get<string>("ThreadName"),
            OldForumName = dr.GetSafe<string>("OldForumName"),
            NewForumId = dr.GetSafe<int?>("NewForumId"),
            UserNameStarter = dr.GetSafe<string>("UserNameStarter"),
            NewThreadId = dr.GetSafe<int?>("NewThreadId"),
        };

        private Func<IDataReader, int> mapIds = dr => dr.Get<int>("PostId");

        public IEnumerable<Thread> GetAllThreads()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT * FROM Threads",
                null,
                mapThreads
            );
        }
        public IEnumerable<Thread> GetAllThreadsToExport()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT * FROM Threads where newThreadId is null and newForumid is not null",
                null,
                mapThreads
            );
        }

        public void InsertThread(Thread item)
        {
            string sql = @"INSERT INTO [dbo].[Threads]
                    ([ThreadName]
                       ,[OldForumName]
                       ,[NewForumId]
                       ,[UserNameStarter])
                    VALUES
                      (@ThreadName
                       ,@OldForumName
                       ,@NewForumId
                       ,@UserNameStarter)";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@ThreadName", item.ThreadName);
                           c.AddWithValue("@OldForumName", item.OldForumName);
                           c.AddWithValue("@NewForumId", item.NewForumId);
                           c.AddWithValue("@UserNameStarter", item.UserNameStarter);
                       }
                   );
        }


        public void UpdateForumId(Thread item)
        {
            string sql = @"Update [dbo].[Threads]
                    set NewForumId = @NewForumId
                    where ThreadId = @ThreadId                    
                   ";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@NewForumId", item.NewForumId);
                           c.AddWithValue("@ThreadId", item.ThreadId);
                       }
                   );
        }

        public void UpdateFohThreadId(Thread item)
        {
            string sql = @"Update [dbo].[Threads]
                    set NewThreadId = @NewThreadId
                    where ThreadId = @ThreadId                    
                   ";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@NewThreadId", item.NewThreadId);
                           c.AddWithValue("@ThreadId", item.ThreadId);
                       }
                   );
        }

        public void InsertThread(IEnumerable<Thread> postToAdd)
        {
            foreach (Thread thread in postToAdd)
            {
                InsertThread(thread);
            }
        }


    }
}

