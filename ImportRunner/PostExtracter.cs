using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using MySqlDAL;
using MySqlDAL.Extensions;

namespace ImportRunner
{
    public class PostExtracter
    {
        public string Html
        {
            get; set;
        }

        public Thread Thread
        {
            get;
            set;
        }

        public HashSet<Post> Posts
        {
            get; set;
        }

        private void BuildThread(HtmlNode node)
        {
            HtmlNode breadCrumb = HtmlNode.CreateNode(node.GetNodesByTagAndValue("div", "breadcrumb").First().OuterHtml);
            string threadName = breadCrumb.SelectNodes("//li").Last().InnerText.Trim();
            Thread = new Thread
            {
                ThreadName = threadName
            };

        }

        public void ExtractPosts()
        {
            HtmlDocument doc = new HtmlDocument();
            Posts = new HashSet<Post>();
            doc.LoadHtml(Html);

            BuildThread(doc.DocumentNode);

            HtmlNodeCollection posts = doc.DocumentNode.SelectNodes("//div[@id='postlist']/ol/li");
            if (posts != null)
            {
                foreach (HtmlNode post in posts)
                {
                    HtmlNode node = HtmlNode.CreateNode(post.OuterHtml); // clear the old stuff
                    string date = node.GetNodesByTagAndValue("span", "date", "class").First().InnerText.Trim().Replace("&nbsp;",String.Empty).Replace(",", " ");
                    //string time = node.GetNodesByTagAndValue("span", "time", "class").First().InnerText.Trim();
                    //04-14-2015 01:35 PM
                    Post p = new Post();
                    if (!date.IsNullOrEmpty())
                    {
                        DateTime createdOn = DateTime.Parse(date);
                        p.PostDate = createdOn;
                    }
                    string userName = post.GetNodesByTagAndValue("a", "username", "class").First().InnerText;
                    p.UserName = userName;
                    p.PostContent = post.SelectSingleNode("//blockquote").OuterHtml;
                    p.Thread = Thread;
                    Posts.Add(p);
                }
            }


        }
    }
}
