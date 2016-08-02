using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MySqlDAL.Extensions
{
    public static class HtmlNodeExtensions
    {
        public static IEnumerable<HtmlNode> GetNodesByTagAndValue(this string source, string tagName, string value, string attribute = "id")
        {
            HtmlNode srcNode = HtmlNode.CreateNode(source);
            return GetNodesByTagAndValue(srcNode, tagName, value, attribute);
        }

        /// <summary>
        /// This method gives you the Separated Html NOde that matches the query. It is not bound to the doc root, so you can do "//" without a problem.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tagName"></param>
        /// <param name="value"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static IEnumerable<HtmlNode> GetNodesByTagAndValue(this HtmlNode source, string tagName, string value, string attribute = "id")
        {
            List<HtmlNode> values = new List<HtmlNode>();
            if (source == null || tagName.IsNullOrWhiteSpace() || value.IsNullOrWhiteSpace() || attribute.IsNullOrWhiteSpace())
            {
                return values;
            }
            string src = source.OuterHtml;


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(src);

            HtmlNode srcNode = doc.DocumentNode;

            if (srcNode == null)
            {
                return values;
            }
            //div[@id='container']"

            // put the class exception
            if (attribute.ToLower() == "class")
            {

                // well use //tagName[contains(@class, 'class1') and contains(@class, 'class2')]
                // GetQuerySelctor does nto work for multiple classes
                StringBuilder bl = new StringBuilder();
                bl.AppendFormat("//{0}[", tagName);
                
                List<string> cssClasses = value.Split(' ').ToList();
                bool first = true;
                foreach (string cssClass in cssClasses.Where(x => !x.IsNullOrWhiteSpace()))
                {
                    if (first)
                    {
                        bl.AppendFormat("contains(@class, '{0}') ", cssClass);
                        first = false;
                        continue;
                    }

                    bl.AppendFormat("and contains(@class, '{0}') ", cssClass);


                }

                bl.Append("]");


                string xpath = bl.ToString();

                IEnumerable<HtmlNode> results = srcNode.SelectNodes(xpath);
                if (results == null)
                {
                    return values;
                }
                foreach (HtmlNode node in results)
                {
                    // so we split all the nodes individually
                    values.Add(HtmlNode.CreateNode(node.OuterHtml));
                }
                return values;
            }
            else
            {

                string searchString = String.Format("//{0}[@{1}='{2}']", tagName, attribute, value);

                HtmlNodeCollection results = srcNode.SelectNodes(searchString);

                return results == null ? values : results.ToList();
            }
        }

        public static void RemoveCommentNodes(this HtmlNode @this)
        {
            @this.Descendants().Where(n =>
                n.NodeType == HtmlNodeType.Comment
                || n.NodeType == HtmlNodeType.Text && String.IsNullOrWhiteSpace(n.InnerText.HtmlTrim())
                ).ToList().ForEach(n => n.Remove());
        }

        public static void RemoveScriptAndStyleNodes(this HtmlNode @this)
        {
            @this.Descendants()
                        .Where(n => n.Name == "script" || n.Name == "style")
                        .ToList()
                        .ForEach(n => n.Remove());
        }
        public static void RemoveScriptNodes(this HtmlNode @this)
        {
            @this.Descendants()
                        .Where(n =>  n.Name == "style")
                        .ToList()
                        .ForEach(n => n.Remove());
        }


        public static string ConvertHtmlToText(this HtmlNode htmlNode)
        {
            StringWriter sw = new StringWriter();
            ConvertTo(htmlNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        public static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write(Environment.NewLine);
                            break;
                        case "br":
                            // treat paragraphs as crlf
                            outText.Write(Environment.NewLine);
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }

    }
}
