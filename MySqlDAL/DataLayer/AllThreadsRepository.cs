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
        private Func<IDataReader, Thread > mapThreads = dr => new Thread
        { 
           ThreadId = dr.Get<int>("ThreadId"),
           ThreadName = dr.GetSafe<string>("ThreadName"),
           OldForumName = dr.GetSafe<string>("OldForumName"),
            NewForumName = dr.GetSafe<string>("NewForumName"),
            UserNameStarter = dr.GetSafe<string>("UserNameStarter"),
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

        public void Save(Thread item)
        {
            string sql = @"INSERT INTO [dbo].[Threads]
                    ([ThreadName]
                       ,[OldForumName]
                       ,[NewForumName]
                       ,[UserNameStarter])
                    VALUES
                      (@ThreadName
                       ,@OldForumName
                       ,@NewForumName
                       ,@UserNameStarter)";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@ThreadName", item.ThreadName);
                           c.AddWithValue("@OldForumName", item.OldForumName);
                           c.AddWithValue("@NewForumName", item.NewForumName);
                           c.AddWithValue("@UserNameStarter", item.UserNameStarter);
                           
                       }
                   );
        }

        public void Save(IEnumerable<Thread> postToAdd)
        {
            foreach (Thread thread in postToAdd)
            {
                Save(thread);
            }
        }

        
    }
}

