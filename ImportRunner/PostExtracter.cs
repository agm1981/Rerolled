using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Common;
using Common.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace ImportRunner
{
    public class PostExtracter
    {
        public string Html
        {
            get; set;
        }

        public string FileName
        {
            get;
            set;
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
                    HtmlNode separatedNode = HtmlNode.CreateNode(post.OuterHtml);
                    var item = separatedNode.Attributes["id"]?.Value;
                    
                    
                    string date = separatedNode.GetNodesByTagAndValue("span", "date", "class").First().InnerText.Trim().Replace("&nbsp;",String.Empty).Replace(",", " ");
                    //string time = node.GetNodesByTagAndValue("span", "time", "class").First().InnerText.Trim();
                    //04-14-2015 01:35 PM
                    Post p = new Post();
                    if (!date.IsNullOrEmpty())
                    {
                        date = date.Replace("Yesterday", "08-10-2016").Replace("Today", "08-11-2016");
                        DateTime createdOn = DateTime.Parse(date);
                        p.PostDate = createdOn;
                    }

                    
                    
                    string userName = separatedNode.GetNodesByTagAndValue("a", "username", "class").FirstOrDefault()?.InnerText ??
                                      separatedNode.GetNodesByTagAndValue("span", "username", "class").FirstOrDefault()?.InnerText;
                    if (userName.Length > 50)
                    {
                        // mistake getting the thing, go alternate
                        string uname =
                        separatedNode.GetNodesByTagAndValue("a", "username", "class")
                            .FirstOrDefault()?.GetAttributeValue("title", string.Empty);

                        uname = uname?.Replace("is offline", String.Empty).Replace("is online", String.Empty).TrimSafely();
                        if (!uname.IsNullOrWhiteSpace())
                        {
                            userName = uname;
                        }

                    }
                    p.UserName = userName;
                    p.PostContent = separatedNode.SelectSingleNode("//blockquote").OuterHtml;
                    p.ThreadName = Thread.ThreadName;
                    if ((item != null) && !item.IsNullOrEmpty())
                    {
                        p.PostId= int.Parse(item.Replace("post_", ""));
                    }
                    if (userName == null || p.PostContent == null || p.PostDate == DateTime.MinValue ||
                        p.PostId == default(int))
                    {
                        throw new Exception("failure parsing");
                    }
                    Posts.Add(p);
                }
            }


        }
    }
}
