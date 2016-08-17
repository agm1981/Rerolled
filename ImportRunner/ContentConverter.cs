using System;
using System.Security.Cryptography;
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
            return data.ToString();
        }
        public ContentConverter()
        {
            data = new StringBuilder();
        }

        private readonly StringBuilder data;

        private void TreeWalker(HtmlNode node)
        {
            string src;
            //string style = node.GetAttributeValue("style", string.Empty);

            //<div class=""cms_table"">
            if (node.Name == "td")
            {
                if (!node.HasChildNodes)
                {
                    string text = HttpUtility.HtmlDecode(node.InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely());
                    data.Append(text);
                    return;
                }
                else
                {
                    foreach (HtmlNode childNode in node.ChildNodes)
                    {
                        TreeWalker(childNode);
                    }
                }
                return;
            }
            if (node.Name == "div" && node.GetAttributeValue("class", "") == "cms_table")
            {
                // da fuc tables
                /*
                 * <blockquote class="postcontent restore ">						
                    <div class="cms_table">
                    <table width="900" class="cms_table_grid" align="left">
                        <tr valign="top" class="cms_table_grid_tr">
                            <td class="cms_table_grid_td">Name</td>
                            <td class="cms_table_grid_td">Type</td>
                        </tr>
                        <tr valign="top" class="cms_table_grid_tr">
                            <td class="cms_table_grid_td"><b>WHISKEY</b></td>
                            <td class="cms_table_grid_td"></td>
                        </tr>
                    </table></div></blockquote>
                 */
                HtmlNode tableNode = HtmlNode.CreateNode(node.SelectSingleNode("table").OuterHtml);

                foreach (HtmlNode tr in tableNode.ChildNodes)
                {
                    // go inside the tr
                    foreach (HtmlNode td in tr.ChildNodes)
                    {
                        TreeWalker(td);
                    }

                    data.AppendLine(); // at the end of the Tr we insert new line
                }


                return;
            }
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
                string dataAttr = node.GetAttributeValue("data", null);
                if (dataAttr == null)
                {
                    throw new Exception("missing Source");
                }
                dataAttr = dataAttr.Substring(dataAttr.LastIndexOf("=", StringComparison.OrdinalIgnoreCase) + 1);
                data.Append($"[MEDIA=vimeo]{dataAttr}[/MEDIA]");

                return;
            }
            if (node.Name == "object" && node.GetAttributeValue("data", null)?.Contains("metacafe") == true)
            {
                //data="http://www.metacafe.com/fplayer/3594943/penguin_blew_a_seal/.swf">
                string dataAttr = node.GetAttributeValue("data", null);
                if (dataAttr == null)
                {
                    throw new Exception("missing Source");
                }
                Regex numbers = new Regex("\\d+");

                string onlyNumber = numbers.Match(dataAttr).Value;
                data.Append($"[MEDIA=metacafe]{onlyNumber}[/MEDIA]");

                return;
            }
            if (node.Name == "object" && node.GetAttributeValue("data", null)?.Contains("facebook.com/v") == true)
            {
                string dataAttr = node.GetAttributeValue("data", null);
                if (dataAttr == null)
                {
                    throw new Exception("missing Source");
                }
                dataAttr = dataAttr.Substring(dataAttr.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                data.Append($"[MEDIA=facebook]{dataAttr}[/MEDIA]");

                return;
            }
            if (node.Name == "object" && node.GetAttributeValue("data", null)?.Contains("dailymotion.com") == true)
            {
                string dataAttr = node.GetAttributeValue("data", null);
                if (dataAttr == null)
                {
                    throw new Exception("missing Source");
                }
                dataAttr = dataAttr.Substring(dataAttr.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                data.Append($"[MEDIA=dailymotion]{dataAttr}[/MEDIA]");

                return;
            }

            if (node.Name == "a")
            {
                src = node.GetAttributeValue("href", null);
                if (src == "http://<object width=")
                {
                    HtmlNode paramNode = node.FirstChild;
                    if (paramNode.Name == "param" && paramNode.GetAttributeValue("name", null) == "movie" && paramNode.GetAttributeValue("value", string.Empty).Contains("youtube"))
                    {
                        // fucked up case. its a youtube video inside the link WFT people
                        string dataValue = paramNode.GetAttributeValue("value", string.Empty);
                        dataValue = dataValue.Substring(0, dataValue.IndexOf("?", StringComparison.OrdinalIgnoreCase));
                        dataValue = dataValue.Substring(dataValue.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        string video = $"[MEDIA=youtube]{dataValue}[/MEDIA]";
                        data.Append(video);
                    }

                    return;
                }

                if (src == null)
                {
                    throw new Exception("missing Source");
                }
                data.Append($"[url={src}]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                data.Append("[/url]");
                return;
            }
            if (node.Name == "b")
            {
                data.Append("[b]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                data.Append("[/b]");
                return;
            }
            if (node.Name == "ul")
            {
                data.Append("[ul]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                data.Append("[/ul]");
                return;
            }

            if (node.Name == "ol")
            {
                data.Append("[ol]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                data.Append("[/ol]");
                return;
            }

            if (node.Name == "li")
            {
                data.Append("[li]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                data.Append("[/li]");
                return;
            }


            if (node.Name == "font")
            {
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                return;
            }
            if (node.Name == "u")
            {
                data.Append("[u]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                data.Append("[/u]");
                return;
            }
            if (node.Name == "i")
            {
                data.Append("[i]");
                foreach (HtmlNode childNode in node.ChildNodes)
                {
                    TreeWalker(childNode);
                }
                data.Append("[/i]");
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
                    data.Append("[spoiler]");
                    TreeWalker(messageNode);
                    data.Append("[/spoiler]");
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
                if (testForInternalQuote != null && testForInternalQuote.HasChildNodes && testForInternalQuote.GetAttributeValue("class", null) == "bbcode_postedby")
                {
                    string userName = HttpUtility.HtmlDecode(separatedNode.SelectSingleNode("/div/div/div/div/strong").InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely());

                    string postIdRef = separatedNode.SelectSingleNode("div/div/div[2]/a")?.GetAttributeValue("href", null);
                    if (postIdRef == null)
                    {
                        data.Append($@"[QUOTE=""{userName}, member: MEMBERID""]");

                        // process specidic child node
                        HtmlNode internalNodeWitHMessage = separatedNode.SelectSingleNode("/div/div/div/div[3]");
                        if (internalNodeWitHMessage != null)
                        {
                            TreeWalker(internalNodeWitHMessage);
                        }
                        data.Append("[/QUOTE]");
                    }
                    else
                    {
                        Regex reg = new Regex(@"p=\d*#");
                        string post = reg.Match(postIdRef).Value;
                        post = post.Replace("p=", string.Empty).Replace("#", string.Empty);
                        //[QUOTE="hodj, post: 1531, member: 9"]
                        data.Append($@"[QUOTE=""{userName}, post: {post}, member: MEMBERID""]");

                        // process specidic child node
                        HtmlNode internalNodeWitHMessage = separatedNode.SelectSingleNode("/div/div/div/div[3]");
                        if (internalNodeWitHMessage != null)
                        {
                            TreeWalker(internalNodeWitHMessage);
                        }
                        data.Append("[/QUOTE]");
                    }
                }
                else
                {
                    // [quote]It's bummer we may never see him again.  Doubt he'll get the memo on the new site.[/quote]
                    data.Append("[QUOTE]");

                    // process specidic child node
                    HtmlNode internalNodeWitHMessage = separatedNode.SelectSingleNode("//div/div/div");
                    if (internalNodeWitHMessage != null)
                    {
                        TreeWalker(internalNodeWitHMessage);
                    }
                    data.Append("[/QUOTE]");
                }
                return;
            }
            if (node.Name == "video" && node.InnerHtml.Contains("i.imgur.com"))
            {
                // vide object inside for imgur
                HtmlNode sour = HtmlNode.CreateNode(node.SelectSingleNode("source").OuterHtml);
                if (sour == null)
                {
                    throw new Exception("Missing source");
                }

                string attSrc = sour.GetAttributeValue("src", null);
                if (attSrc == null)
                {
                    throw new Exception("Missing source attrib");
                }
                // just output it
                attSrc = attSrc.Replace(".webm", string.Empty); // for those webm thingy
                attSrc = attSrc.Substring(attSrc.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                string video = $"[MEDIA=imgur]{attSrc}[/MEDIA]";
               
                data.Append(video);

                return;
            }//video.webmfiles.org
            if (node.Name == "video" && node.InnerHtml.Contains("video.webmfiles.org"))
            {
                // vide object inside for imgur
                HtmlNode sour = HtmlNode.CreateNode(node.SelectSingleNode("source").OuterHtml);
                if (sour == null)
                {
                    throw new Exception("Missing source");
                }

                string attSrc = sour.GetAttributeValue("src", null);
                if (attSrc == null)
                {
                    throw new Exception("Missing source attrib");
                }
                string video = $"[url]{attSrc}[/url]";

                data.Append(video);

                return;
            }
            if (node.Name == "video" && node.InnerHtml.Contains("webmbassy.com"))
            {
                // vide object inside for imgur
                HtmlNode sour = HtmlNode.CreateNode(node.SelectSingleNode("source").OuterHtml);
                if (sour == null)
                {
                    throw new Exception("Missing source");
                }

                string attSrc = sour.GetAttributeValue("src", null);
                if (attSrc == null)
                {
                    throw new Exception("Missing source attrib");
                }
                string video = $"[url]{attSrc}[/url]";

                data.Append(video);

                return;
            }

            if (node.Name == "video" && node.InnerHtml.Contains("i.4cdn.org/"))
            {
                // vide object inside for imgur
                HtmlNode sour = HtmlNode.CreateNode(node.SelectSingleNode("source").OuterHtml);
                if (sour == null)
                {
                    throw new Exception("Missing source");
                }
                string attSrc = sour.GetAttributeValue("src", null);
                if (attSrc == null)
                {
                    throw new Exception("Missing source attrib");
                }

                string video = $"[url]{attSrc}[/url]";
                data.Append(video);

                return;
            }

            if (node.Name == "video" && node.InnerHtml.Contains("cdn.streamable.com"))
            {
                // vide object inside for imgur
                HtmlNode sour = HtmlNode.CreateNode(node.SelectSingleNode("source").OuterHtml);
                if (sour == null)
                {
                    throw new Exception("Missing source");
                }
                string attSrc = sour.GetAttributeValue("src", null);
                if (attSrc == null)
                {
                    throw new Exception("Missing source attrib");
                }

                string video = $"[url]{attSrc}[/url]";
                data.Append(video);

                return;
            }
            if (node.Name == "video" && node.InnerHtml.Contains(".gfycat.com"))
            {
                HtmlNode separatedNode = node.SelectSingleNode("source");
                src = separatedNode.GetAttributeValue("src", null);
                Regex reg = new Regex(@".*gfycat.com/");
                string toRemove = reg.Match(src).Value;
                src = src.Replace(toRemove, string.Empty).Replace(".mp4", string.Empty).Replace(".webm", string.Empty).Replace(".ogg", string.Empty);
                data.Append($"[MEDIA=gfycat]height=320;id={src};width=640[/MEDIA]");
                return;
            }

            if (!node.HasChildNodes)
            {
                string text;
                switch (node.Name)
                {
                    case "hr":
                        data.Append("----------------------------------------------------------------").AppendLine();
                        break;
                    case "br":
                        data.AppendLine();
                        break;
                    case "iframe":
                        if (node.GetAttributeValue("title", string.Empty).StartsWith("youtube", StringComparison.OrdinalIgnoreCase))
                        {
                            // <iframe class=""restrain"" title=""YouTube video player"" width=""640"" height=""390"" src=""//www.youtube.com/embed/rF9D7S480Mw?wmode=opaque"" frameborder=""0"" id=""yui-gen130""></iframe>
                            // [video=youtube]https://youtu.be/JINQh1ttao0?t=59s[/video]
                            //[MEDIA=youtube]GQQMLE4FuIQ[/MEDIA]
                            src = node.GetAttributeValue("src", null);
                            if (src == null)
                            {
                                throw new Exception("Invalid tag " + node.Name);
                            }
                            src = src.Substring(0, src.IndexOf("?", StringComparison.OrdinalIgnoreCase));
                            int lastIndex = src.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                            src = src.Substring(lastIndex + 1);

                            string video = $"[MEDIA=youtube]{src}[/MEDIA]";
                            data.Append(video);
                            break;
                        }

                        throw new Exception("Invalid tag " + node.Name);
                    case "img":
                        src = node.GetAttributeValue("src", null);
                        string gfyitem = node.GetAttributeValue("class", null);
                        if (gfyitem == "gfyitem")
                        {
                            // "gfyitem" img 


                            string dataId = node.GetAttributeValue("data-id", null);
                            data.Append($"[MEDIA=gfycat]height=320;id={dataId};width=640[/MEDIA]");
                            break;
                        }
                        if (src == null)
                        {
                            throw new Exception("missing source");
                        }
                        data.Append($"[img]{src}[/img]");
                        break;
                    case "#text":
                        text = HttpUtility.HtmlDecode(node.InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely());
                        data.Append(text);
                        break;
                    case "div":
                        text = HttpUtility.HtmlDecode(node.InnerText.RemoveEndOfLineCharacter().ReplaceTabsForSingleWhiteSpace().TrimSafely());
                        data.Append(text);
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

