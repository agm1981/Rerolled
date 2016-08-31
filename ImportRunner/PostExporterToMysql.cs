using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Common;
using Common.DataLayer;
using Common.Extensions;

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

            HashSet<int> postsPending = new HashSet<int>(repPost.GetPostsToExport());
            long i = 0;
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
                int? mySqlThreadId = mySqlThreads.FirstOrDefault(c => c.Title.Equals(HttpUtility.HtmlDecode(post.ThreadName) + "_imported", StringComparison.OrdinalIgnoreCase))?.ThreadId;
                if (mySqlThreadId == null && post.ThreadName.StartsWith("Father Hires In-Game", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 6375;
                }
                if (mySqlThreadId == null && post.ThreadName.Contains("ALLAH AKBAR WE JIHAD FOR MARIO", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 3450;
                }
                if (mySqlThreadId == null && post.ThreadName.EndsWith("are fucking stupid and should go away.", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 3463;
                }


                if (mySqlThreadId == null && post.ThreadName.StartsWith("Blue Is The Warmest Color", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 5900;
                }

                if (mySqlThreadId == null && post.ThreadName.StartsWith("Gabriel Garc", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 4772;
                }

                if (mySqlThreadId == null && post.ThreadName.StartsWith("Kajiimagi", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 3531;
                }

                if (mySqlThreadId == null && post.ThreadName.StartsWith("Nude Beach Blow Job Jet Ski Fight Leads to W", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 6065;
                }
                if (mySqlThreadId == null && post.ThreadName.EndsWith("[Kickstarter - PC voxel-based open world RPG]", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 1357;
                }

                if (mySqlThreadId == null && post.ThreadName.StartsWith("Santander", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 4267;
                }

                if (mySqlThreadId == null && post.ThreadName.StartsWith("Twitch Plays Pok", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 5913;
                }

                if (mySqlThreadId == null && post.ThreadName.EndsWith("Series Picked Up By Syfy", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 2773;
                }

                if (mySqlThreadId == null && post.ThreadName.StartsWith("NBA 2013", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 1029;
                }
                if (mySqlThreadId == null && post.ThreadName.StartsWith("Scarlett Johansson Named", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 909;
                }
                if (mySqlThreadId == null && post.ThreadName.StartsWith("May Book of the Month: The Shadow of the Wind by Carlos Ruiz", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 4125;
                }
                if (mySqlThreadId == null && post.ThreadName.StartsWith("France!  Jtai cass", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 4399;
                }

                if (mySqlThreadId == null && post.ThreadName.StartsWith("Happy Chinese New Year", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 3352;
                }

                if (mySqlThreadId == null && post.ThreadName.EndsWith("tal Legend", StringComparison.OrdinalIgnoreCase))
                {
                    mySqlThreadId = 6118;
                }
                if (mySqlThreadId == null)
                {
                    continue;
                    throw new Exception($" need case for {post.ThreadName}");
                }
                MySqlPost sqlPost = new MySqlPost
                {
                    Content = ConvertQuotes(post.NewPostContent, postsImported, mySqlUsers),
                    PostDate = DateTimeToUnixTimestamp(post.PostDate),
                    ThreadId = mySqlThreadId.Value,
                    UserName = post.UserName + UserNameExporterToMySql.name_suffix,
                    UserId =
                        mySqlUsers.First(c => c.UserName == post.UserName + UserNameExporterToMySql.name_suffix).UserId,
                    Position = GetPositionInThread(postsImported, mySqlThreadId.Value),
                };
                mySqlPostRep.InsertPost(sqlPost);
                post.NewPostId = sqlPost.PostId;
                post.NewThreadId = mySqlThreadId;

                postsImported.Add(new PostImported
                {
                    OldPostId = postid,
                    NewPostId = sqlPost.PostId,
                    NewThreadId = mySqlThreadId.Value,
                    PostDate = sqlPost.PostDate
                });
                repPost.SaveNewPostIdNewThreadIdOnly(post);
                if (i % 100 == 0)
                {
                    Console.WriteLine($"{post.PostId} left: {postsPending.Count - i} ");
                }


            }
        }

        private int GetPositionInThread(HashSet<PostImported> postsImported, int mySqlThreadId)
        {
            return postsImported.Count(c => c.NewThreadId == mySqlThreadId);
        }

        private string ConvertQuotes(string postContent, HashSet<PostImported> postsImported, HashSet<MySqlUser> usersImported)
        {

            // this will select the whoile tag header 
            //[QUOTE="Asmadai, post: 7, member: MEMBERID"]Responseeeeee[/QUOTE]quote tesssttttttt

            string returnValue = postContent;
            Regex regForWholeQuoteBLock = new Regex(@"\[QUOTE="".*? MEMBERID""\]", RegexOptions.Compiled); // to find the whole Quote tag
            Regex regexForUserNamePlusComma = new Regex(@"QUOTE=""(.*?),", RegexOptions.Compiled); // to find the quote user separated by ,
            // Regex innerREgex = new Regex(@"""(.*?)""");
            foreach (Match match in regForWholeQuoteBLock.Matches(postContent))
            {
                string wholeQuote = match.Value;

                string userNamePlusQuote = regexForUserNamePlusComma.Match(wholeQuote).Value;

                //[QUOTE="Asmadai, post: 7, member: MEMBERID"]Responseeeeee[/QUOTE]quote tesssttttttt
                string userName = userNamePlusQuote.Split('"')[1].Replace(",", string.Empty).TrimSafely();
                string oldQuote = wholeQuote;
                if (userName.Length > 0) // fuck you guy named " "
                {
                    wholeQuote = wholeQuote.Replace(userName, userName + UserNameExporterToMySql.name_suffix);
                }
                else
                {
                    wholeQuote = wholeQuote.Replace(@""",", $@"""{UserNameExporterToMySql.name_suffix},");
                }
                returnValue = returnValue.Replace(oldQuote, wholeQuote);

            }

            // now the PID
            Regex regForWholeQuoteBLockSecond = new Regex(@"\[QUOTE="".*? post: \d+, member: MEMBERID""\]", RegexOptions.Compiled);
            Regex postIdFinder = new Regex(@"post: \d+,", RegexOptions.Compiled);
            foreach (Match match in regForWholeQuoteBLockSecond.Matches(returnValue))
            {
                string wholeQuote = match.Value;
                // now from here we get the name 


                string postIdLine = postIdFinder.Match(wholeQuote).Value;
                int postId = int.Parse(postIdLine.Replace("post: ", string.Empty).Replace(",", string.Empty).TrimSafely());

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

            // and the member Id

            //Regex innerREgex2 = new Regex("'(.*?)'"); // MEMBERID
            foreach (Match match in regForWholeQuoteBLock.Matches(returnValue))
            {

                string wholeQuote = match.Value;

                string userNamePlusQuote = regexForUserNamePlusComma.Match(wholeQuote).Value;

                //[QUOTE="Asmadai_sl, post: 7, member: MEMBERID"]Responseeeeee[/QUOTE]quote tesssttttttt
                string userName = userNamePlusQuote.Split('"')[1].Replace(",", string.Empty).TrimSafely();
                string oldQuote = wholeQuote;


                if (usersImported.Any(c => c.UserName == userName))
                {
                    string newUserNameId = usersImported.First(c => c.UserName == userName).UserId.ToString();
                    wholeQuote = wholeQuote.Replace("MEMBERID", newUserNameId);
                    returnValue = returnValue.Replace(oldQuote, wholeQuote);
                }
                else
                {
                    // post is not there, it may been deleted.

                    returnValue = returnValue.Replace(", member: MEMBERID", "");

                }






                //string foundPartial = match.Value;
                //// now from here we get the name 
                //string[] data = foundPartial.Split(' ');
                //string postIdLine = data.First(c => c.Contains("pid="));
                //string datelineLine = data.First(c => c.Contains("dateline="));


                //int postId = int.Parse(innerREgex2.Match(postIdLine).Value.Replace("'", string.Empty));
                ////int datelineIdId = int.Parse(innerREgex.Match(postIdLine).Value.Replace("'", string.Empty));
                //string oldQuote = datelineLine;
                //// here we find the old post
                //if (postsImported.Any(c => c.OldPostId == postId))
                //{
                //    int newDateline = postsImported.First(c => c.OldPostId == postId).PostDate;
                //    datelineLine = datelineLine.Replace("1", newDateline.ToString());
                //    returnValue = returnValue.Replace(oldQuote, datelineLine);
                //}
                //else
                //{
                //    returnValue = returnValue.Replace(oldQuote, "]"); // respect end of tag, its always at the end anyways
                //}



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
