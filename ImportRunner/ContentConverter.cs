using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace ImportRunner
{
    public class ContentConverter
    {
      

        public string Html
        {
            get;
            set;
        }

        private object Nodes
        {
            get;
            set;
        }

        private string test = @"<blockquote class=""postcontent restore "">
break me up before yo gogo
<div style=""margin: 5px 20px 20px;""> <div class=""smallfont"" style=""margin-bottom: 2px;""><b>Spoiler:</b>&nbsp;  <input value=""Show"" style=""margin: 0px; padding: 0px; font-size: 10px;"" onclick=""if (this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display != '') { this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display = '';this.innerText = ''; this.value = 'Hide'; } else { this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display = 'none'; this.innerText = ''; this.value = 'Show'; }"" type=""button""> </div> <div class=""alt2"" style=""border: 1px inset ; margin: 0px; padding: 6px;""> <div style=""display: none;""> <img src=""https://i.sli.mg/SDDGbC.jpg"" border=""0"" alt=""""> </div> </div> </div>
						i would love yo toinigghhtt!!</blockquote>";
        public string ConvertContent()
        {
            HtmlNode node = HtmlNode.CreateNode(test);
            TreeWalker(node);
            return _data.ToString();
        }
        public ContentConverter()
        {
            _data = new StringBuilder();
        }

        private readonly StringBuilder _data;
        private void TreeWalker(HtmlNode node)
        {
            //string style = node.GetAttributeValue("style", string.Empty);
            if (node.Name == "div" && node.ChildNodes.Count>1 && node.ChildNodes[1].InnerText.TrimSafely().Equals("Spoiler:&nbsp;"))
            {
                // spoiler
                int g = 9;
                // find the node and tree walk out of here
                HtmlNode separatedNode = HtmlNode.CreateNode(node.OuterHtml);
                HtmlNode messageNode = separatedNode.SelectSingleNode("/div/div[2]/div");
                // add spoiler stuff
                if (messageNode != null)
                {
                    _data.Append("[spoiler]");
                    TreeWalker(messageNode);
                    _data.Append("[/spoiler]");
                }
                return;
            }
            else
            if (node.GetAttributeValue("class", "") == "bbcode_container")
            {
                // special case. here we have to dig one level to get the post by. and also skip those from the output
                // two kinds of quote, by users and outside people
                // make that distintion. 
                HtmlNode separatedNode = HtmlNode.CreateNode(node.OuterHtml);
                HtmlNode testForInternalQuote = separatedNode.SelectSingleNode("/div/div/div/div[2]");
                if (testForInternalQuote != null && testForInternalQuote.HasChildNodes)
                {
                    string userName = separatedNode.SelectSingleNode("/div/div/div/div/strong").InnerText;

                    string postIdRef = separatedNode.SelectSingleNode("/div/div/div/div/a").GetAttributeValue("href", string.Empty);
                    Regex reg = new Regex(@"p=\d*#");
                    string post = reg.Match(postIdRef).Value;
                    post = post.Replace("p=", string.Empty).Replace("#", string.Empty);
                    // [quote='Intrinsic' pid='528' dateline='1469909677']Dance, I think?[/quote]
                    _data.AppendLine().Append($"[quote='{userName}' pid='{post}' dateline='1']");

                    // process specidic child node
                    HtmlNode internalNodeWitHMessage = separatedNode.SelectSingleNode("/div/div/div/div[3]");
                    if (internalNodeWitHMessage != null)
                    {
                        TreeWalker(internalNodeWitHMessage);
                    }
                    _data.AppendLine("[/quote]");
                    return;


                }
                else
                {
                    // quote without from, an external more likely
                    return;
                }
            }
            else 
            if (!node.HasChildNodes)
            {
                switch (node.Name)
                {
                    case "br":
                        _data.AppendLine();
                        break;
                    case "img":
                        string src = node.GetAttributeValue("src", String.Empty);
                        _data.AppendLine($"[img]{src}[/img]");
                        break;
                    case "#text":
                        string text = node.InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely();
                        _data.Append(text);
                        break;
                    default:
                        throw new Exception("Invalid tag " + node.Name);
                }
                
            }
            
            foreach (HtmlNode childNode in node.ChildNodes)
            {
                TreeWalker(childNode);
            }
            
        }

       

       
    }
}
