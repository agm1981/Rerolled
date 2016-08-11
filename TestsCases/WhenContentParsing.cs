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
            ContentConverter content = new ContentConverter();
            content.Html = @"<blockquote class=""postcontent restore "">
							NASA will send second Mars rover in 2020, send humans in 2030s - <a href=""http://arstechnica.com/science/2012/12/nasa-will-send-second-mars-rover-in-2020-send-humans-in-2030s/"" target=""_blank"">http://arstechnica.com/science/2012/...mans-in-2030s/</a>
						</blockquote>";
            var outString = content.ConvertContent();
        }

        [TestMethod]
        public void ThenBrTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenImgTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenYouTubeTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenBTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenUTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenITagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenQuoteWithoutAnAuthorTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenQuoteWithAnAuthorTagsParseProperly()
        {
        }
        [TestMethod]
        public void ThenQuoteWithAuthorTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenSpolierTagsParseProperly()
        {
        }

        [TestMethod]
        public void ThenTextTagsParseProperly()
        {
        }

         
    }
}
