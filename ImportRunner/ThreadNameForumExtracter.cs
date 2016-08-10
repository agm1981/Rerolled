using System.Linq;
using Common;
using Common.Extensions;
using HtmlAgilityPack;

namespace ImportRunner
{
    public class ThreadNameForumExtracter
    {
        public string Html
        {
            get;
            set;
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


        private void ExtractThreadName(HtmlNode node)
        {
            HtmlNode breadCrumb = HtmlNode.CreateNode(node.GetNodesByTagAndValue("div", "breadcrumb").First().OuterHtml);
            Thread.ThreadName = breadCrumb.SelectNodes("//li").Last().InnerText.Trim();
            Thread.OldForumName = breadCrumb.SelectNodes("//li")[breadCrumb.SelectNodes("//li").Count-2].InnerText.Trim();
        }


        public void ExtractThreads()
        {
            HtmlDocument doc = new HtmlDocument();
            Thread = new Thread();
            doc.LoadHtml(Html);

            ExtractThreadName(doc.DocumentNode);

        }
    }
}