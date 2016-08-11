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

            HashSet<int> allPosts = new HashSet<int>(rep.GetAllPosts());
            foreach (int postId in allPosts)
            {
                Post post = rep.GetPost(postId);
                string newconntet = new ContentConverter
                {
                    Html = post.PostContent
                }.ConvertContent();
                post.NewPostContent = newconntet;
                rep.Save(post);
            }
        }
    }
}
