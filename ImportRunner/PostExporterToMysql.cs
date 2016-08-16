using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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
            long i=0;
            foreach (int postid in postsPending)
            {
                i++;
                // here we take an old post
                // load it, then find the thread it belongs too.
                // find the user it made it
                // figure out the position on the thread.
                // figure out if we need to switch the quote to the new item
                // save it to mySql,
                // update back the post Id in SQL


                Post post = repPost.GetPost(postid);
                int mySqlThreadId = mySqlThreads.First(c => c.Title.Equals(HttpUtility.HtmlDecode(post.ThreadName) + "_imported", StringComparison.OrdinalIgnoreCase)).ThreadId;
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
                    NewThreadId = mySqlThreadId,
                    PostDate = sqlPost.PostDate
                });
                repPost.SaveNewPostIdNewThreadIdOnly(post);
                if (i % 100 == 0)
                {
                    Console.WriteLine(post.PostId);
                }


            }
        }

        private int GetPositionInThread(HashSet<PostImported> postsImported, int mySqlThreadId)
        {
            return postsImported.Count(c => c.NewThreadId == mySqlThreadId);
        }

        private string ConvertQuotes(string postContent, HashSet<PostImported> postsImported)
        {

            // this will select the whoile tag header 
            //[quote='Troll' pid='15' dateline='1']I see what you did here.[/quote]Looks good.  Checking how quotes look... [quote='Eyashusa' pid='113' dateline='1']McConnell filibustering himself was funnier I think [quote='Adam12']Lol why do you keep saying
            // it will select [quote='Troll' pid='15' dateline='1'] and [quote='Eyashusa' pid='113' dateline='1'] and [quote='Adam12']

            string returnValue = postContent;
            Regex reg = new Regex(@"\[quote='.+'\]");
            Regex regex = new Regex("quote='(.*?)'");
            Regex innerREgex = new Regex("'(.*?)'");
            foreach (Match match in reg.Matches(postContent))
            {
                string foundPartial = match.Value;
                // now from here we get the name 
                //string[] data = foundPartial.Split(' '); // either first one has it in the acse of [quote='Adam12']

                string quoteLine = regex.Match(foundPartial).Value;
                // now we have [quote='Adam12' or [quote='Troll' or [quote='Eyashusa'

                string userName = innerREgex.Match(quoteLine).Value.Replace("'", string.Empty);
                string oldQuote = quoteLine;
                if (userName.Length > 0) // fuck you guy named " "
                {
                    quoteLine = quoteLine.Replace(userName, userName + UserNameExporterToMySql.name_suffix);
                }
                else
                {
                    quoteLine = quoteLine.Replace("=''", $"='{UserNameExporterToMySql.name_suffix}'");
                }
                returnValue = returnValue.Replace(oldQuote, quoteLine);

            }

            // now the PID
            reg = new Regex(@"\[quote='.+' pid='.+'\]");
            Regex innerREgex3 = new Regex("'(.*?)'");
            foreach (Match match in reg.Matches(postContent))
            {
                string foundPartial = match.Value;
                // now from here we get the name 
                string[] data = foundPartial.Split(' ');
                string postIdLine = data.First(c => c.Contains("pid="));


                int postId = int.Parse(innerREgex3.Match(postIdLine).Value.Replace("'", string.Empty));
                string oldQuote = postIdLine;
                // here we find the old post
                if (postsImported.Any(c => c.OldPostId == postId))
                {
                    int newPostId = postsImported.First(c => c.OldPostId == postId).NewPostId;
                    postIdLine = postIdLine.Replace(postId.ToString(), newPostId.ToString());
                    returnValue = returnValue.Replace(oldQuote, postIdLine);
                }
                else
                {
                    // post is not there, it may been deleted.

                    returnValue = returnValue.Replace(oldQuote, "");

                }

            }

            // and the date
            reg = new Regex(@"\[quote='.+' dateline='.+'\]");
            Regex innerREgex2 = new Regex("'(.*?)'");
            foreach (Match match in reg.Matches(postContent))
            {
                string foundPartial = match.Value;
                // now from here we get the name 
                string[] data = foundPartial.Split(' ');
                string postIdLine = data.First(c => c.Contains("pid="));
                string datelineLine = data.First(c => c.Contains("dateline="));


                int postId = int.Parse(innerREgex2.Match(postIdLine).Value.Replace("'", string.Empty));
                //int datelineIdId = int.Parse(innerREgex.Match(postIdLine).Value.Replace("'", string.Empty));
                string oldQuote = datelineLine;
                // here we find the old post
                if (postsImported.Any(c => c.OldPostId == postId))
                {
                    int newDateline = postsImported.First(c => c.OldPostId == postId).PostDate;
                    datelineLine = datelineLine.Replace("1", newDateline.ToString());
                    returnValue = returnValue.Replace(oldQuote, datelineLine);
                }
                else
                {
                    returnValue = returnValue.Replace(oldQuote, "]"); // respect end of tag, its always at the end anyways
                }



            }

            // the idea is to search the post for quotes and then replace them for the new post ID of the quote.
            // also search for the name and replace it for the new name, that is easy

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(returnValue);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            returnValue = iso.GetString(isoBytes);


            //byte[] bytes = Encoding.Default.GetBytes(returnValue);
            //Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            //returnValue = Encoding.UTF8.GetString(bytes);
            //string test = Encoding.ASCII.GetString(bytes);
            return returnValue;
        }

        public static int DateTimeToUnixTimestamp(DateTime dateTimeInUtc)
        {
            return Convert.ToInt32((dateTimeInUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        }


    }
}
