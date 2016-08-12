using System;
using ImportRunner;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestsCases
{
    [TestClass]
    public class WhenContentParsing
    {
        [TestMethod]
        public void ThenATagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<a href=""http://arstechnica.com/science/2012/12/nasa-will-send-second-mars-rover-in-2020-send-humans-in-2030s/"" target=""_blank"">link/</a>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(
                "a[url=http://arstechnica.com/science/2012/12/nasa-will-send-second-mars-rover-in-2020-send-humans-in-2030s/]link/[/url]b",
                outString);
        }

        [TestMethod]
        public void ThenBrTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a<br>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual($"a{Environment.NewLine}b", outString);
        }

        [TestMethod]
        public void ThenImgTagsParseProperly()
        {
            //
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<img src=""http://i.imgur.com/qDLR3.jpg"" border=""0"" alt="""">b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[img]http://i.imgur.com/qDLR3.jpg[/img]b", outString);
        }

        [TestMethod]
        public void ThenYouTubeTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">
							
<iframe class=""restrain"" title=""YouTube video player"" width=""640"" height=""390"" src=""http://www.youtube.com/embed/B5sks1-EGfw?wmode=opaque"" frameborder=""0""></iframe>

						</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("[MEDIA=youtube]B5sks1-EGfw[/MEDIA]", outString);

            content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore ""><iframe class=""restrain"" title=""YouTube video player"" width=""640"" height=""390"" src=""//www.youtube.com/embed/W2ET8ghJaBE?wmode=opaque"" frameborder=""0""></iframe><iframe class=""restrain"" title=""YouTube video player"" width=""640"" height=""390"" src=""//www.youtube.com/embed/ojpQ3LCeQTY?wmode=opaque"" frameborder=""0""></iframe></blockquote>"
            };
            outString = content.ConvertContent();
            Assert.AreEqual("[MEDIA=youtube]W2ET8ghJaBE[/MEDIA][MEDIA=youtube]ojpQ3LCeQTY[/MEDIA]", outString);
        }

        [TestMethod]
        public void ThenBTagsParseProperly()
        {
            //
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a<b>Brutal</b>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[b]Brutal[/b]b", outString);
        }

        [TestMethod]
        public void ThenVimeoVideosParseProperly()
        {
            //
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<object class=""restrain"" type=""application/x-shockwave-flash"" width=""640"" height=""360"" data=""http://vimeo.com/moogaloop.swf?clip_id=66169135"">
	<param name=""movie"" value=""http://vimeo.com/moogaloop.swf?clip_id=66169135"">
	<param name=""wmode"" value=""opaque"">
	<!--[if IE 6]>
	<embed width=""640"" height=""360"" type=""application/x-shockwave-flash"" src=""//vimeo.com/moogaloop.swf?clip_id=66169135"" />
	<![endif]--></object>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[MEDIA=vimeo]66169135[/MEDIA]b", outString);
        }


        [TestMethod]
        public void ThenUTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a<u>Brutal</u>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[u]Brutal[/u]b", outString);
        }

        [TestMethod]
        public void ThenITagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a<i>Brutal</i>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[i]Brutal[/i]b", outString);
        }

        [TestMethod]
        public void ThenQuoteWithoutAnAuthorTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a
							<div class=""bbcode_container"">
	<div class=""bbcode_quote"">
		<div class=""quote_container"">
			<div class=""bbcode_quote_container""></div>
			
				Let's lift a glass of Kanar to the best ST character of all time.
			
		</div>
	</div>
</div>I beg to differ.</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(
                "a[quote]Let's lift a glass of Kanar to the best ST character of all time.[/quote]I beg to differ.",
                outString);
           
        }


        [TestMethod]
        public void ThenQuoteWithAuthorTagsWithOutPostIdParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a<div class=""bbcode_container"">
	<div class=""bbcode_quote"">
		<div class=""quote_container"">
			<div class=""bbcode_quote_container""></div>
			
				<div class=""bbcode_postedby"">
					<img src=""images/misc/quote_icon.png"" alt=""Quote"" title=""Quote""> Originally Posted by <strong>Tuco</strong>
					
				</div>
				<div class=""message"">Lend Is the best</div>			
		</div>
	</div>
</div>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(
                @"a[quote='Tuco']Lend Is the best[/quote]b",
                outString);
        }

        [TestMethod]
        public void ThenQuoteWithAuthorAndPostIdTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a<div class=""bbcode_container"">
	<div class=""bbcode_quote"">
		<div class=""quote_container"">
			<div class=""bbcode_quote_container""></div>
			
				<div class=""bbcode_postedby"">
					<img src=""http://www.rerolled.org/images/misc/quote_icon.png"" alt=""Quote""> Originally Posted by <strong>hodj</strong>
					<a href=""http://www.rerolled.org/showthread.php?1610-Dragon-Age-Inquisition-(Plot-Details-in-Spoilers!)/page27&amp;p=455406#post455406#post455406"" rel=""nofollow""><img class=""inlineimg"" src=""http://www.rerolled.org/images/buttons/viewpost-right.png"" alt=""View Post""></a>
				</div>
				<div class=""message"">The primary cause of Bioware going to shit is that they are focused on appealing to as broad a segment of society as possible, which means focusing on stupid shit like romance and flashy press button win bacon gameplay. This isn't remotely debateable. They've said it themselves:</div>
			
		</div>
	</div>
</div>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(
                @"a[quote='hodj' pid='455406' dateline='1']The primary cause of Bioware going to shit is that they are focused on appealing to as broad a segment of society as possible, which means focusing on stupid shit like romance and flashy press button win bacon gameplay. This isn't remotely debateable. They've said it themselves:[/quote]b",
                outString);
        
    }


        [TestMethod]
        public void ThenSpolierTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<div style=""margin: 5px 20px 20px;""> <div class=""smallfont"" style=""margin-bottom: 2px;""><b>Spoiler:</b>&nbsp;  <input value=""Show"" style=""margin: 0px; padding: 0px; font-size: 10px;"" onclick=""if (this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display != '') { this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display = '';this.innerText = ''; this.value = 'Hide'; } else { this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display = 'none'; this.innerText = ''; this.value = 'Show'; }"" type=""button""> </div> <div class=""alt2"" style=""border: 1px inset ; margin: 0px; padding: 6px;""> <div style=""display: none;"">Snape kills dumberdole</div> </div> </div>b</blockquote>"
            };

            var outString = content.ConvertContent();
            Assert.AreEqual("a[spoiler]Snape kills dumberdole[/spoiler]b", outString);
        }

        [TestMethod]
        public void ThenTextTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a le dasd</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a le dasd", outString);
        }


        [TestMethod]
        public void ThenTextSpansParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore ""><span style=""font-family: Arial""><span style=""font-family: Book Antiqua"">&quot;Bishop owns 20 acres in Douglass, a town about 160 miles north of Houston. He used to raise poultry and goats on the land where he lives with his wife and 16-year-old daughter, he said, but sold the animals about two years ago because of the planned pipeline. Initially, the Vietnam War veteran said, he fought the company's attempt to condemn his land, but settled because he could not afford the lawyer's fees of $10,000.<br>
Bishop said he settled under &quot;duress,&quot; so he bought a law book and decided to defend himself. Since then, he has filed a lawsuit in Austin against the Texas Railroad Commission, the state agency that oversees pipelines, arguing it failed to properly investigate the pipeline and protect groundwater, public health and safety.<br>
Aware that the oil giant could have a battery of lawyers and experts at the hearing later this month, Bishop, a 64-year-old retired chemist currently in medical school, said he is determined to fight.<br>
&quot;Bring 'em on. I'm a United States Marine. I'm not afraid of anyone. I'm not afraid of them,&quot; he said. &quot;When I'm done with them, they will know that they've been in a fight. I may not win, but I'm going to hurt them.&quot;<br>
</span></span></blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(@"""Bishop owns 20 acres in Douglass, a town about 160 miles north of Houston. He used to raise poultry and goats on the land where he lives with his wife and 16-year-old daughter, he said, but sold the animals about two years ago because of the planned pipeline. Initially, the Vietnam War veteran said, he fought the company's attempt to condemn his land, but settled because he could not afford the lawyer's fees of $10,000.
Bishop said he settled under ""duress,"" so he bought a law book and decided to defend himself. Since then, he has filed a lawsuit in Austin against the Texas Railroad Commission, the state agency that oversees pipelines, arguing it failed to properly investigate the pipeline and protect groundwater, public health and safety.
Aware that the oil giant could have a battery of lawyers and experts at the hearing later this month, Bishop, a 64-year-old retired chemist currently in medical school, said he is determined to fight.
""Bring 'em on. I'm a United States Marine. I'm not afraid of anyone. I'm not afraid of them,"" he said. ""When I'm done with them, they will know that they've been in a fight. I may not win, but I'm going to hurt them.""
", outString);
        }


    }
}
