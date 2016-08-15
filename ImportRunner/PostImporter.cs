using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.DataLayer;

namespace ImportRunner
{
    public class PostExporterToMysql
    {
        public void Start()
        {
            AllPostsRepository repPost = new AllPostsRepository();
            AllPostMySqlRepository mySqlPostRep = new AllPostMySqlRepository();
            AllUserMySqlRepository mySqlUserRep = new AllUserMySqlRepository();

            AllThreadsMySqlRepository mySqlThreadsRep = new AllThreadsMySqlRepository();

            HashSet<MySqlThread> mySqlThreads = new HashSet<MySqlThread>(mySqlThreadsRep.GetAllThreadsInMySql());
            HashSet<MySqlUser> mySqlUsers = new HashSet<MySqlUser>(mySqlUserRep.GetAllUsersInMySql());
            
            HashSet<PostImported> postsImported = new HashSet<PostImported>(repPost.GetAllExportedPosts());
            IEnumerable<int> postsPending = repPost.GetPostsToExport().ToList();
            foreach (int postid in postsPending)
            {
                // here we take an old post
                // load it, then find the thread it belongs too.
                // find the user it made it
                // figure out the position on the thread.
                // figure out if we need to switch the quote to the new item
                // save it to mySql,
                // update back the post Id in SQL
                

                Post post = repPost.GetPost(postid);
                int mySqlThreadId = mySqlThreads.First(c => c.Title == post.ThreadName + "_imported").ThreadId;
                MySqlPost sqlPost = new MySqlPost
                {
                    Content = ConvertQuotes(post.NewPostContent, postsImported),
                    PostDate = DateTimeToUnixTimestamp(post.PostDate),
                    ThreadId = mySqlThreadId,
                    UserName = post.UserName + UserNameExporterToMySql.name_suffix,
                    UserId =
                        mySqlUsers.First(c => c.UserName == post.UserName + UserNameExporterToMySql.name_suffix).UserId,
                    Position = GetPositionInThread(postsImported, mySqlThreadId),
                };
                mySqlPostRep.InsertPost(sqlPost);
                post.NewPostId = sqlPost.PostId;
                post.NewThreadId = mySqlThreadId;

                postsImported.Add(new PostImported
                {
                    OldPostId = postid,
                    NewPostId = sqlPost.PostId,
                    NewThreadId = mySqlThreadId
                });
                repPost.SaveNewPostIdNewThreadIdOnly(post);



            }
        }

        private int GetPositionInThread(HashSet<PostImported> postsImported, int mySqlThreadId)
        {
            return postsImported.Count(c => c.NewThreadId == mySqlThreadId);
        }

        private string ConvertQuotes(string postContent, HashSet<PostImported> postsImported)
        {

            // the idea is to search the post for quotes and then replace them for the new post ID of the quote.
            // also search for the name and replace it for the new name, that is easy

            return postContent;
        }

        public static int DateTimeToUnixTimestamp(DateTime dateTimeInUtc)
        {
            return Convert.ToInt32((dateTimeInUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        }


    }
}
