using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
testtest
							<div class=""bbcode_container"">
	<div class=""bbcode_quote"">
		<div class=""quote_container"">
			<div class=""bbcode_quote_container""></div>
			
				<div class=""bbcode_postedby"">
					<img src=""http://www.rerolled.org/images/misc/quote_icon.png"" alt=""Quote""> Originally Posted by <strong>Asmadai</strong>
					<a href=""http://www.rerolled.org/showthread.php?3-testttest&amp;p=7#post7#post7"" rel=""nofollow""><img class=""inlineimg"" src=""http://www.rerolled.org/images/buttons/viewpost-right.png"" alt=""View Post""></a>
				</div>
				<div class=""message"">Responseeeeee</div>
			
		</div>
	</div>
</div>quote tesssttttttt</blockquote>";
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
            if (node.GetAttributeValue("class", "") == "bbcode_container")
            {
                // special case. here we have to dig one level to get the post by. and also skip those from the output
                // two kinds of quote, by users and outside people
                // make that distintion. 
                HtmlNode separatedNode = HtmlNode.CreateNode(node.OuterHtml);
                var userName = separatedNode.SelectSingleNode("/div/div/div/div/strong").InnerText;
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
                        _data.Append(node.InnerText);
                        break;
                    case "div":
                        // Quote logic here
                        // end result is a 
                        // [quote='Intrinsic' pid='528' dateline='1469909677']I had to reference the Dramatis Personae and shit so often the thought of trying to audiobook this on a first read through is terrifying, hah.Sucks that they changed talent though. Sane thing happened with ASoIaF with Dance, I think?[/quote]
                       
                        break;
                    default:
                        if (node.GetAttributeValue("value", string.Empty).Equals("Show", StringComparison.OrdinalIgnoreCase))
                        {
                            return; // show button for spoiler
                        }
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
