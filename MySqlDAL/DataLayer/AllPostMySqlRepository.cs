using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace Common.DataLayer
{
    public class AllPostMySqlRepository : MySqlBaseRepository
    {
        private Func<IDataReader, MySqlPost> mapPosts = dr => new MySqlPost
        {
            ThreadId = dr.Get<int>("Thread_Id"),
            PostId = dr.Get<int>("post_Id"),
            UserName = dr.Get<string>("username"),
            UserId = dr.Get<int>("user_id"),
            PostDate = dr.Get<int>("post_date"),
            Content = dr.Get<string>("message"),
            Position = dr.Get<int>("position"),
            
        };

        //private Func<IDataReader, int> mapIds = dr => dr.Get<int>("PostId");

        public IEnumerable<MySqlPost> GetAllThreadsInMySql()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT Thread_Id, post_Id, username, user_id, post_date , message, position FROM xf_post",
                null,
                mapPosts
            );
        }
        
        public void InsertThread(MySqlPost post)
        {
            
            //`InsertPost`(in threadId int, in userId int, in userna varchar(50), in postDate int, in content mediumtext, in position int, out postId int )
            string sql = "InsertPost";
            int postId = sqlH.ExecuteNonQuery<int>(
                CommandType.StoredProcedure,
                sql,
                c =>
                {
                    c.AddWithValue("@threadId", post.ThreadId);
                    c.AddWithValue("@userId", post.UserId);
                    c.AddWithValue("@userna", post.UserName);
                    c.AddWithValue("@postDate", post.PostDate);
                    c.AddWithValue("@content", post.Content);
                    c.AddWithValue("@position", post.Position);
                    

                    c.Add(new MySqlParameter("postId", MySqlDbType.Int32));
                    c["@postId"].Direction = ParameterDirection.Output;
                },
                c => Convert.ToInt32(c.Parameters["@postId"].Value)

            );
            post.PostId = postId;
        }
    }

}


