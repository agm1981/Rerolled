using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Common;
using Common.DataLayer;

namespace ImportRunner
{
    public class ThreadExporterToMySql
    {
        private List<IdValue> FohList = new List<IdValue>
        {
            new IdValue(1, "Gaming Related Forum"),
            new IdValue(2, "MMORPG General Discussion"),
            new IdValue(3, "PC & Console Gaming"),
            new IdValue(3, "Other Games"),
            new IdValue(4, "Screenshots: NSFW Forum - No post minimum"),
            new IdValue(5, "Screenshots"),
            new IdValue(6, "Retard Rickshaw"),
            new IdValue(7, "Entertainment Forums"),
            new IdValue(8, "Movie House"),
            new IdValue(9, "TV House"),
            new IdValue(10, "Music House"),
            new IdValue(10, "Music &amp; Podcast House"),
            new IdValue(11, "Book House"),
            new IdValue(11, "Book of the Month Archive"),
            new IdValue(12, "General Forums"),
            new IdValue(13, "General"),
            new IdValue(14, "Gravy's Sports Stadium"),
            new IdValue(15, "Grown Up Stuff"),
            new IdValue(15, "Game, Web, Software Development"),
            
            new IdValue(16, "Technology and Gadgets"),
            new IdValue(16, "Technology, Electronics and Gadgets"),
            new IdValue(17, "Travel"),
            new IdValue(18, "Business, Finance, and Careers"),
            new IdValue(18, "Business, Finance &amp; Career Development"),
            new IdValue(18, "Job Listings and Employment Opportunities"),
            new IdValue(19, "Product Reviews"),
            //
            new IdValue(20, "Comments / Suggestions"),
            new IdValue(20, "Comments/Suggestions"),
            new IdValue(21, "Special Access for Donors"),
            new IdValue(22, "Moderation Discussion"),
            new IdValue(23, "Special Access"),
            new IdValue(24, "Mobile Gaming"),
            new IdValue(25, "Tabletop Gaming"),
            new IdValue(26, "Moderator Campaigning and Voting"),
            new IdValue(27, "Moderation and Comments"),
        };

        public ThreadExporterToMySql()
        {

        }

        public void AddForumId()
        {
            // Load ll null
            AllThreadsRepository rep = new AllThreadsRepository();
            List<Thread> threads = rep.GetAllThreads().Where(c => c.NewForumId == null).ToList();
            foreach (Thread thread in threads)
            {
                thread.NewForumId = FohList.FirstOrDefault(c => c.Value == thread.OldForumName)?.Id;
                if (thread.NewForumId == null)
                {
                    continue;
                }
                rep.UpdateForumId(thread);
            }
        }

        public void InsertAllThreads()
        {
            AllThreadsRepository rep = new AllThreadsRepository();
            AllThreadsMySqlRepository mySqlRep = new AllThreadsMySqlRepository();
            AllUserMySqlRepository mySqlUserRep = new AllUserMySqlRepository();
            HashSet<MySqlUser> allMySqlUsers = new HashSet<MySqlUser>(mySqlUserRep.GetAllUsersInMySql());


            List<Thread> threads = rep.GetAllThreadsToExport().ToList().Where(c => c.NewForumId.HasValue).ToList();

            foreach (Thread thread in threads)
            {
                MySqlThread trad = new MySqlThread
                {
                    Title = HttpUtility.HtmlDecode(thread.ThreadName) + "_imported",
                    NodeId = thread.NewForumId.Value,
                    StarterUserName = thread.UserNameStarter,
                    StarterUserId = allMySqlUsers.First(c => c.UserName == thread.UserNameStarter + UserNameExporterToMySql.name_suffix).UserId
                };
                mySqlRep.InsertThread(trad);
                thread.NewThreadId = trad.ThreadId;
                rep.UpdateFohThreadId(thread);

            }
        }

        public class IdValue
        {
            public int Id
            {
                get;
                set;
            }

            public string Value
            {
                get;
                set;
            }

            public IdValue(int id, string value)
            {
                Id = id;
                Value = value;
            }
        }


    }

}


