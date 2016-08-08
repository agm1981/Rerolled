using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataLayer
{
    public class AllPostsRepository : BaseRepository
    {
        private Func<IDataReader, Post > mapPosts = dr => new Post { 
           PostId = dr.Get<int>("PostId"),
           UserName = dr.GetSafe<string>("UserName"),
           PostDate = dr.GetSafe<DateTime>("PostDate"),
            PostContent = dr.GetSafe<string>("PostContent"),
            ThreadName = dr.GetSafe<string>("PostContent"),
        };

        private Func<IDataReader, int> mapIds = dr => dr.Get<int>("PostId");

        public IEnumerable<int> GetAllPosts()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT postId FROM Posts",
                null,
                mapIds
            );
        }

        public void Save(Post item)
        {
            string sql = @"INSERT INTO [dbo].[Posts]
                    ([PostId]
                    ,[UserName]
                    ,[PostContent]
                    ,[PostDate]
                    ,[ThreadName])
                    VALUES
                      (@postId
                       ,@userName
                       ,@postContent
                       ,@postDate
                       ,@threadName)";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@postId", item.PostId);
                           c.AddWithValue("@userName", item.UserName);
                           c.AddWithValue("@postContent", item.PostId);
                           c.AddWithValue("@postDate", item.PostDate);
                           c.AddWithValue("@threadName", item.ThreadName);
                       }
                   );
        }

        public void Save(IEnumerable<Post> postToAdd)
        {
            foreach (Post post in postToAdd)
            {
                Save(post);
            }
        }
    }
}

