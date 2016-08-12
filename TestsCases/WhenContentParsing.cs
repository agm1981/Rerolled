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
        public void ThenFontTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a
							<font color=""#000000"">.</font>b
						</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(
                "a.b",
                outString);
        }

        [TestMethod]
        public void ThenHragsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a
							<hr>b
						</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(
                $"a----------------------------------------------------------------{Environment.NewLine}b",
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
        public void ThenYouTubeTagsInsideAtagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">						
<a href=""http://<object width="" 420""="""" height=""315""><param name=""movie"" value=""http://www.youtube.com/v/6lHHQu4CIos?version=3&amp;amp;hl=en_US%22></param><param%20name=%22allowFullScreen%22%20value=%22true%22></param><param%20name=%22allowscriptaccess%22%20value=%22always%22></param><embed%20src=%22http://www.youtube.com/v/6lHHQu4CIos?version=3&amp;amp;hl=en_US%22%20type=%22application/x-shockwave-flash%22%20width=%22420%22%20height=%22315%22%20allowscriptaccess=%22always%22%20allowfullscreen=%22true%22></embed></object>"" target=""_blank"">http://&lt;object width=&quot;420&quot; height=&quot;3...mbed&gt;&lt;/object&gt;</a>
						</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("[MEDIA=youtube]6lHHQu4CIos[/MEDIA]", outString);

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
        public void ThenFacebookVideosParseProperly()
        {
            //
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<object class=""restrain"" type=""application/x-shockwave-flash"" width=""576"" height=""432"" data=""http://www.facebook.com/v/833481713333895"">
	<param name=""movie"" value=""//www.facebook.com/v/833481713333895"">
	<param name=""wmode"" value=""opaque"">
	<!--[if IE 6]>
	<embed width=""576"" height=""432"" type=""application/x-shockwave-flash"" src=""//www.facebook.com/v/833481713333895"" />
	<![endif]--></object>b </blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[MEDIA=facebook]833481713333895[/MEDIA]b", outString);
        }

        [TestMethod]
        public void ThenDailyMotionVideosParseProperly()
        {
            //
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<object class=""restrain"" type=""application/x-shockwave-flash"" width=""420"" height=""339"" data=""http://www.dailymotion.com/swf/x10zzmb"">
	<param name=""movie"" value=""http://www.dailymotion.com/swf/x10zzmb"">
	<param name=""wmode"" value=""opaque"">
	<!--[if IE 6]>
	<embed width=""420"" height=""339"" type=""application/x-shockwave-flash"" src=""http://www.dailymotion.com/swf/x10zzmb"" />
	<![endif]--></object>b </blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[MEDIA=dailymotion]x10zzmb[/MEDIA]b", outString);
        }

        [TestMethod]
        public void ThenVideoObjectsWithWebmParseProperly()
        {
            //
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<video width=""600"" height=""472"" controls="""">
<source src=""http://i.imgur.com/tIoAL6n.webm"" type=""video/mp4"">
<source src=""http://i.imgur.com/tIoAL6n.webm"" type=""video/ogg"">
<source src=""http://i.imgur.com/tIoAL6n.webm"" type=""video/webm"">
<object data=""http://i.imgur.com/tIoAL6n.webm"" width=""600"" height=""472"">
<embed src=""http://i.imgur.com/tIoAL6n.webm"" width=""320"" height=""240"">
</object> 
</source></source></source></video>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[MEDIA=imgur]tIoAL6n[/MEDIA]b", outString);
        }

        [TestMethod]
        public void ThenMetacafeVideosParseProperly()
        {
            //
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore "">a<object class=""restrain"" type=""application/x-shockwave-flash"" width=""400"" height=""345"" data=""http://www.metacafe.com/fplayer/3594943/penguin_blew_a_seal/.swf"">
	<param name=""movie"" value=""http://www.metacafe.com/fplayer/3594943/penguin_blew_a_seal/.swf"">
	<param name=""wmode"" value=""opaque"">
	<!--[if IE 6]>
	<embed width=""400"" height=""345"" type=""application/x-shockwave-flash"" src=""http://www.metacafe.com/fplayer/3594943/penguin_blew_a_seal/.swf"" />
	<![endif]--></object>b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[MEDIA=metacafe]3594943[/MEDIA]b", outString);
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
        public void ThenImgGifyTagsParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html = @"<blockquote class=""postcontent restore "">a<img class=""gfyitem"" data-id=""ScratchyUnpleasantGoitered"">b</blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual("a[MEDIA=gfycat]height=320;id=ScratchyUnpleasantGoitered;width=640[/MEDIA]b", outString);
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
        
        [TestMethod]
        public void ThenUlSpansParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore ""><ul><li style="""">1</li><li style="""">2</li></ul></blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(@"[ul][li]1[/li][li]2[/li][/ul]", outString);
        }

        [TestMethod]
        public void ThenOlSpansParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore ""><ol><li style="""">1</li><li style="""">2</li></ol></blockquote>>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual(@"[ol][li]1[/li][li]2[/li][/ol]", outString);
        }

        [TestMethod]
        public void ThenTablesParseProperly()
        {
            ContentConverter content = new ContentConverter
            {
                Html =
                    @"<blockquote class=""postcontent restore ""><div class=""cms_table""><table width=""900"" class=""cms_table_grid"" align=""left""><tr valign=""top"" class=""cms_table_grid_tr""><td class=""cms_table_grid_td"">Name</td><td class=""cms_table_grid_td"">Type</td></tr><tr valign=""top"" class=""cms_table_grid_tr""><td class=""cms_table_grid_td""><b>WHISKEY</b></td><td class=""cms_table_grid_td""><u>$123.00</u></td></tr></table></div></blockquote>"
            };
            var outString = content.ConvertContent();
            Assert.AreEqual($@"NameType{Environment.NewLine}[b]WHISKEY[/b][u]$123.00[/u]{Environment.NewLine}", outString);
        }
    }
}
