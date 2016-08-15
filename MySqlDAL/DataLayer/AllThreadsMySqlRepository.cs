using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace Common.DataLayer
{
    public class AllThreadsMySqlRepository : MySqlBaseRepository
    {
        private Func<IDataReader, MySqlThread> mapThreads = dr => new MySqlThread
        {
            ThreadId = Convert.ToInt32(dr.Get<uint>("Thread_Id")),
            Title = dr.Get<string>("title"),
            NodeId = Convert.ToInt32(dr.Get<uint>("node_id")),
            StarterUserId = Convert.ToInt32(dr.Get<uint>("user_id")),
            StarterUserName = dr.Get<string>("username"),
        };

        //private Func<IDataReader, int> mapIds = dr => dr.Get<int>("PostId");

        public IEnumerable<MySqlThread> GetAllThreadsInMySql()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT Thread_Id, title, node_id, user_id, username FROM xf_Thread",
                null,
                mapThreads
            );
        }

        public void InsertThread(MySqlThread thread)
        {
            //call InsertThread(1, 'titsdsle',3, 'harfle', @tid);
            // CALL `xenforo`.`InsertThread`(<{in nodeId int}>, <{in titleThread varchar(50)}>, <{in userId int}>, <{in username varchar(50)}>, <{out threadId int}>);
            string sql = "InsertThread";
            int threadId = sqlH.ExecuteNonQuery<int>(
                CommandType.StoredProcedure,
                sql,
                c =>
                {
                    c.AddWithValue("@nodeId", thread.NodeId);
                    c.AddWithValue("@titleThread", thread.Title);
                    c.AddWithValue("@userId", thread.StarterUserId);
                    c.AddWithValue("@username", thread.StarterUserName);

                    c.Add(new MySqlParameter("threadId", MySqlDbType.Int32));
                    c["@threadId"].Direction = ParameterDirection.Output;
                },
                c => Convert.ToInt32(c.Parameters["@threadId"].Value)

            );
            thread.ThreadId = threadId;
        }
    }

}


