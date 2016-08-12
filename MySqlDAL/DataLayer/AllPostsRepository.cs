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
            ThreadName = dr.GetSafe<string>("ThreadName"),
            NewPostContent = dr.GetSafe<string>("NewPostContent"),
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
        public IEnumerable<int> GetAllPostsToUpdate()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT postId FROM Posts where NewPostContent is null",
                null,
                mapIds
            );
        }

        public IEnumerable<Post> GetPost(IEnumerable<int> postIds)
        {
            IEnumerable<int> enumerable = postIds as int[] ?? postIds.ToArray();
            string cmd = SqlHelper.InClause(@"SELECT * FROM Posts where postId in ({0})", enumerable);
            
            return sqlH.ExecuteSet(
               CommandType.Text,
               cmd,
                c =>
                {
                     SqlHelper.InParameters(c, enumerable);
                },
               mapPosts
           );
        }

        public Post GetPost(int postId)
        {
            return sqlH.ExecuteSingle(
                CommandType.Text,
                @"SELECT * FROM Posts where postId = @postId",
                c =>
                {
                    c.AddWithValue("@postId", postId);
                },
                mapPosts
            );
        }

        public void Save(Post item)
        {
            string sql = @"

                    UPDATE Posts
                    SET
                    [UserName] = @UserName
                    ,[PostContent] = @PostContent
                    ,[PostDate] = @PostDate
                    ,[ThreadName] =  @ThreadName
                    ,[NewPostContent] = @NewPostContent
                
                    WHERE postId = @postId
        
                 if (SELECT @@ROWCOUNT) = 0
                    BEGIN

                    INSERT INTO [dbo].[Posts]
                    ([PostId]
                    ,[UserName]
                    ,[PostContent]
                    ,[PostDate]
                    ,[ThreadName]
                    ,[NewPostContent])
                    VALUES
                      (@postId
                       ,@userName
                       ,@postContent
                       ,@postDate
                       ,@threadName
                       ,@newPostContent)
                    END
                        ";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@postId", item.PostId);
                           c.AddWithValue("@userName", item.UserName);
                           c.AddWithValue("@postContent", item.PostContent);
                           c.AddWithValue("@postDate", item.PostDate);
                           c.AddWithValue("@threadName", item.ThreadName);
                           c.AddWithValue("@newPostContent", item.NewPostContent);
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

        public void SaveNewContentOnly(Post item)
        {
            string sql = @"
                    UPDATE Posts
                    SET                
                    [NewPostContent] = @NewPostContent                
                    WHERE postId = @postId
                        ";

            sqlH.ExecuteNonQuery(
                       CommandType.Text,
                       sql,
                       c =>
                       {
                           c.AddWithValue("@postId", item.PostId);
                           c.AddWithValue("@newPostContent", item.NewPostContent);
                       }
                   );
        }
    }
}

