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
        public static int DateTimeToUnixTimestamp(DateTime dateTimeInUtc)
        {
            return Convert.ToInt32((dateTimeInUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        }

        private readonly Func<IDataReader, Post> mapPosts = dr => new Post
        {
            Id = dr.Get<int>("Id"),
            PostId = dr.Get<int>("PostId"),
            UserName = dr.GetSafe<string>("UserName"),
            PostDate = dr.GetSafe<DateTime>("PostDate"),
            PostContent = dr.GetSafe<string>("PostContent"),
            ThreadName = dr.GetSafe<string>("ThreadName"),
            NewPostContent = dr.GetSafe<string>("NewPostContent"),
            NewPostId = dr.GetSafe<int?>("NewPostId"),
            NewThreadId = dr.GetSafe<int?>("NewThreadId")
        };

        private readonly Func<IDataReader, int> mapIds = dr => dr.Get<int>("Id");

        private readonly Func<IDataReader, PostImported> mapPostsImported = dr => new PostImported
        {
            NewPostId = dr.Get<int>("NewPostId"),
            OldPostId = dr.Get<int>("Id"),
            NewThreadId = dr.Get<int>("NewThreadId"),
            PostDate = DateTimeToUnixTimestamp(dr.Get<DateTime>("PostDate")),
        };
        public IEnumerable<PostImported> GetAllExportedPosts()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"select id, PostId,  NewPostId, NewThreadId, PostDate from Posts  where NewPostId is not null  order by threadname, postid",
                null,
                mapPostsImported
            );
        }
        public IEnumerable<int> GetPostsToExport()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"select distinct p.Id from Posts p
                    INNER JOIN Threads t on t.threadname = p.threadname
                where NewPostId is null and t.newForumId is not null",
                null,
                mapIds
            );
        }



        public IEnumerable<string> GetAllUsersFromPostTable()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"select distinct username from Posts ",
                null,
                c=> c.Get<string>("UserName")
            );
        }

        public IEnumerable<int> GetAllPosts()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT Id FROM Posts  order by threadname, postid",
                null,
                mapIds
            );
        }
        public IEnumerable<int> GetAllPostsToUpdate()
        {
            return sqlH.ExecuteSet(
                CommandType.Text,
                @"SELECT Id FROM Posts where NewPostContent is null order by threadname, postid",
                null,
                mapIds
            );
        }

        public IEnumerable<Post> GetPost(IEnumerable<int> ids)
        {
            IEnumerable<int> enumerable = ids as int[] ?? ids.ToArray();
            string cmd = SqlHelper.InClause(@"SELECT * FROM Posts where Id in ({0})  order by threadname, postid", enumerable);

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

        public Post GetPost(int id)
        {
            return sqlH.ExecuteSingle(
                CommandType.Text,
                @"SELECT * FROM Posts where Id = @Id",
                c =>
                {
                    c.AddWithValue("@id", id);
                },
                mapPosts
            );
        }

        public void Save(Post item)
        {
            string sql = @"
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

        public void UpdateNewThreadContentOnly(Post item)
        {
            string sql = @"
                UPDATE Posts
                SET                
                [NewPostContent] = @NewPostContent                
                WHERE Id = @id
                    ";

            sqlH.ExecuteNonQuery(
                CommandType.Text,
                sql,
                c =>
                {
                    c.AddWithValue("@id", item.Id);
                    c.AddWithValue("@newPostContent", item.NewPostContent);
                }
            );
        }
        public void SaveNewPostIdNewThreadIdOnly(Post item)
        {
            string sql = @"
                UPDATE Posts
                SET                
                [NewPostId] = @NewPostId,
                [NewThreadId] = @NewThreadId
                WHERE Id = @Id
                    ";

            sqlH.ExecuteNonQuery(
                CommandType.Text,
                sql,
                c =>
                {
                    c.AddWithValue("@id", item.Id);
                    c.AddWithValue("@NewPostId", item.NewPostId);
                    c.AddWithValue("@NewThreadId", item.NewThreadId);
                }
            );
        }
    }
}


