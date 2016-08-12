using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.DataLayer;

namespace ImportRunner
{
    public class ConvertMessages
    {
        public void Start()
        {
            AllPostsRepository rep = new AllPostsRepository();

            HashSet<int> allPostsThatNeedToBeWorkedOn = new HashSet<int>(rep.GetAllPostsToUpdate());
            int runningCount = 0;
            bool check = true;
            while (check)
            {
                IEnumerable<int> batchIds = allPostsThatNeedToBeWorkedOn.Take(1000).ToList();
                IEnumerable<Post> batchToWork = rep.GetPost(batchIds).ToList();
                foreach (Post post in batchToWork)
                {
                    string newconntet = new ContentConverter
                    {
                        Html = post.PostContent
                    }.ConvertContent();
                    post.NewPostContent = newconntet;
                    rep.SaveNewContentOnly(post);
                }
                // check now
                allPostsThatNeedToBeWorkedOn.RemoveWhere(c => batchIds.Contains(c));
                if (allPostsThatNeedToBeWorkedOn.Count < 1)
                {
                    check = false;
                }

            }
        }
    }
}
