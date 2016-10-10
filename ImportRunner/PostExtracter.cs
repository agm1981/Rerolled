using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private void BuildThreadFoh()
        {

            string filenameA = Path.GetFileNameWithoutExtension(FileName);
            Regex regex = new Regex(@"\d+$");
            if (regex.Match(filenameA).Success)
            {
                // ends with digit.
                
                filenameA = filenameA.Substring(0, filenameA.LastIndexOf("-", StringComparison.OrdinalIgnoreCase));
            }
            Thread = new Thread
            {
                ThreadName = filenameA
            };





        }

        public void ExtractRerolledPosts()
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


                    string date = separatedNode.GetNodesByTagAndValue("span", "date", "class").First().InnerText.Trim().Replace("&nbsp;", String.Empty).Replace(",", " ");
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
                        p.PostId = int.Parse(item.Replace("post_", ""));
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
    
        public void ExtractFoHPosts()
        {
            HtmlDocument doc = new HtmlDocument();
            Posts = new HashSet<Post>();
            doc.LoadHtml(Html.Replace("'","\""));
            Regex rg = new Regex(@"<body>.*<\/body>", RegexOptions.Singleline|RegexOptions.IgnoreCase);
            string body = rg.Match(Html.Replace("'", "\"")).Value;

            HtmlNode node = HtmlNode.CreateNode(body);

            BuildThreadFoh();

            var posts = node.GetNodesByTagAndValue("div", "post", "class").ToList();
            if (posts.Any())
            {
                foreach (HtmlNode post in posts)
                {
                    HtmlNode separatedNode = HtmlNode.CreateNode(post.OuterHtml);
                    var item = separatedNode.GetNodesByTagAndValue("div", "header", "class").First().FirstChild.InnerText;

                    // format is "11-15-2004, 05:03 PM"

                    string date = item.Trim();
                   
                    Post p = new Post();
                    if (!date.IsNullOrEmpty())
                    {
                        date = date.Replace("Yesterday", "06-05-2013").Replace("Today", "06-06-2013");
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
                    p.PostContent = separatedNode.GetNodesByTagAndValue("span", "content", "class").First().InnerHtml;
                    p.ThreadName = Thread.ThreadName;
                    string postIdNode = separatedNode.GetNodesByTagAndValue("div", "header", "class").First().Children().Skip(1).First().InnerText.Trim();
                    if (!item.IsNullOrEmpty())
                    {
                        string id = postIdNode.Split(' ').First();
                        p.PostId= int.Parse(id.Replace("#", ""));
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
