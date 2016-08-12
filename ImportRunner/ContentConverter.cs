using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Common.Extensions;
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

        public string ConvertContent()
        {
            HtmlNode node = HtmlNode.CreateNode(Html);
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


            //if (node.Name == "div" && node.GetAttributeValue("class", "") == "gfyitem")
            //{
            //    //source src=""https://zippy.gfycat.com/ShrillWelcomeGeese.mp4
            //    //[MEDIA=gfycat]height=320;id=CandidImmaterialDromedary;width=640[/MEDIA]
            //    HtmlNode separatedNode = HtmlNode.CreateNode(node.OuterHtml).SelectSingleNode("//source");
            //    string src = separatedNode.GetAttributeValue("src", string.Empty);
            //    Regex reg = new Regex(@".*gfycat.com/");
            //    string toRemove = reg.Match(src).Value;
            //    src = src.Replace(toRemove, string.Empty).Replace(".mp4", string.Empty).Replace(".webm", string.Empty).Replace(".ogg", string.Empty);
            //    _data.Append($"[MEDIA=gfycat]height=320;id={src};width=640[/MEDIA]");
            //    return;
            //}
            if (node.Name == "object" && node.GetAttributeValue("data", null)?.Contains("vimeo") == true)
            {
                string data = node.GetAttributeValue("data", null);
                if (data == null)
                {
                    throw new Exception("missing Source");
                }
                data = data.Substring(data.LastIndexOf("=", StringComparison.OrdinalIgnoreCase) + 1);
                _data.Append($"[MEDIA=vimeo]{data}[/MEDIA]");

                return;
            }
            if (node.Name == "a")
            {
                string src = node.GetAttributeValue("href", null);
                if (src == null)
                {
                    throw new Exception("missing Source");
                }
                _data.Append($"[url={src}]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                _data.Append("[/url]");
                return;
            }
            if (node.Name == "b")
            {
                _data.Append("[b]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                _data.Append("[/b]");
                return;
            }
            if (node.Name == "u")
            {
                _data.Append("[u]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                _data.Append("[/u]");
                return;
            }
            if (node.Name == "i")
            {
                _data.Append("[i]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                _data.Append("[/i]");
                return;
            }
            if (node.Name == "div" && node.ChildNodes.Count > 1 && node.ChildNodes[1].InnerText.TrimSafely().Equals("Spoiler:&nbsp;"))
            {
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

            if (node.Name == "div" && node.GetAttributeValue("class", null) == "bbcode_container")
            {
                // special case. here we have to dig one level to get the post by. and also skip those from the output
                // two kinds of quote, by users and outside people
                // make that distintion. 
                HtmlNode separatedNode = HtmlNode.CreateNode(node.OuterHtml);
                HtmlNode testForInternalQuote = separatedNode.SelectSingleNode("/div/div/div/div[2]");
                if (testForInternalQuote != null && testForInternalQuote.HasChildNodes)
                {
                    string userName = HttpUtility.HtmlDecode(separatedNode.SelectSingleNode("/div/div/div/div/strong").InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely());

                    string postIdRef = node.SelectSingleNode("div/div/div[2]/a")?.GetAttributeValue("href", null);
                    if (postIdRef == null)
                    {
                        _data.Append($"[quote='{userName}']");

                        // process specidic child node
                        HtmlNode internalNodeWitHMessage = separatedNode.SelectSingleNode("/div/div/div/div[3]");
                        if (internalNodeWitHMessage != null)
                        {
                            TreeWalker(internalNodeWitHMessage);
                        }
                        _data.Append("[/quote]");
                    }
                    else
                    {
                        Regex reg = new Regex(@"p=\d*#");
                        string post = reg.Match(postIdRef).Value;
                        post = post.Replace("p=", string.Empty).Replace("#", string.Empty);
                        // [quote='Intrinsic' pid='528' dateline='1469909677']Dance, I think?[/quote]
                        _data.Append($"[quote='{userName}' pid='{post}' dateline='1']");

                        // process specidic child node
                        HtmlNode internalNodeWitHMessage = separatedNode.SelectSingleNode("/div/div/div/div[3]");
                        if (internalNodeWitHMessage != null)
                        {
                            TreeWalker(internalNodeWitHMessage);
                        }
                        _data.Append("[/quote]");
                    }
                }
                else
                {
                    // [quote]It's bummer we may never see him again.  Doubt he'll get the memo on the new site.[/quote]
                    _data.Append("[quote]");

                    // process specidic child node
                    HtmlNode internalNodeWitHMessage = separatedNode.SelectSingleNode("//div/div/div");
                    if (internalNodeWitHMessage != null)
                    {
                        TreeWalker(internalNodeWitHMessage);
                    }
                    _data.Append("[/quote]");
                }
                return;
            }
            //if (node.Name == "video")
            //{
            //    HtmlNode separatedNode = node.SelectSingleNode("//source");
            //    string src = separatedNode.GetAttributeValue("src", null);
            //    //Regex reg = new Regex(@".*gfycat.com/");
            //    //string toRemove = reg.Match(src).Value;
            //    //src = src.Replace(toRemove, string.Empty).Replace(".mp4", string.Empty).Replace(".webm", string.Empty).Replace(".ogg", string.Empty);
            //    _data.Append($"[MEDIA]height=320;id={src};width=640[/MEDIA]");
            //    return;
            //}
            string text;
            if (!node.HasChildNodes)
            {
                switch (node.Name)
                {
                    case "br":
                        _data.AppendLine();
                        break;
                    case "iframe":
                        if (node.GetAttributeValue("title", string.Empty).StartsWith("youtube", StringComparison.OrdinalIgnoreCase))
                        {
                            // <iframe class=""restrain"" title=""YouTube video player"" width=""640"" height=""390"" src=""//www.youtube.com/embed/rF9D7S480Mw?wmode=opaque"" frameborder=""0"" id=""yui-gen130""></iframe>
                            // [video=youtube]https://youtu.be/JINQh1ttao0?t=59s[/video]
                            //[MEDIA=youtube]GQQMLE4FuIQ[/MEDIA]
                            string data = node.GetAttributeValue("src", string.Empty);
                            if (data == null)
                            {
                                throw new Exception("Invalid tag " + node.Name);
                            }
                            data = data.Substring(0, data.IndexOf("?", StringComparison.OrdinalIgnoreCase));
                            int lastIndex = data.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                            data = data.Substring(lastIndex + 1);

                            string video = $"[MEDIA=youtube]{data}[/MEDIA]";
                            _data.Append(video);
                            break;
                        }

                        throw new Exception("Invalid tag " + node.Name);
                    case "img":
                        string src = node.GetAttributeValue("src", string.Empty);
                        _data.Append($"[img]{src}[/img]");
                        break;
                    case "#text":
                        text = HttpUtility.HtmlDecode(node.InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely());
                        _data.Append(text);
                        break;
                    case "div":
                        text = HttpUtility.HtmlDecode(node.InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely());
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

