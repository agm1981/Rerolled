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

        private readonly  Func<IDataReader, MySqlPost> mapIncompletePosts = dr => new MySqlPost
        {
            
            PostId = Convert.ToInt32(dr.Get<uint>("post_Id")),
            Content = dr.Get<string>("message")
        };
        public IEnumerable<MySqlPost> GetAllPostsInMySql()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT Thread_Id, post_Id, username, user_id, post_date , message, position FROM xf_post",
                null,
                mapPosts
            );
        }

        public IEnumerable<MySqlPost> GetAllPostIncompleteInMySql()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT post_Id, message FROM xf_post Where message like '%www.rerolled.org/attachment.php%' order by post_Id",
                null,
                mapIncompletePosts
            );
        }
        public void UpdatePostContent(MySqlPost item)
        {
            string sql = @"Update xf_Post
                    set message = @Content
                    where post_id = @PostId                    
                   ";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@PostId", item.PostId);
                           c.AddWithValue("@Content", item.Content);
                       }
                   );
        }
        public void InsertPost(MySqlPost post)
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


