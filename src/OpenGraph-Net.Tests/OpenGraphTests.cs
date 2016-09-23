// <copyright file="OpenGraphTests.cs" company="SHHH Innovations LLC">
// Copyright SHHH Innovations LLC
// </copyright>

namespace OpenGraph_Net.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// The open graph test fixture
    /// </summary>
    [TestFixture]
    public class OpenGraphTests
    {
        /// <summary>
        /// The valid sample content
        /// </summary>
        private string validSampleContent = @"<!DOCTYPE HTML>
<html>
<head>
    <meta property=""og:type"" content=""product"" />
    <meta property=""og:title"" cOntent=""Product Title"" />
    <meta name=""og:image"" content=""http://www.test.com/test.png""/>
    <meta propErty=""og:uRl"" content=""http://www.test.com"" />
    <meta property=""og:description"" content=""My Description""/>
    <meta property=""og:site_Name"" content=""Test Site"">
</head>
<body>
</body>
</html>";

        /// <summary>
        /// The invalid sample content
        /// </summary>
        private string invalidSampleContent = @"<!DOCTYPE HTML>
<html>
<head>
    <meta property=""og:title"" cOntent=""Product Title"" />
    <meta name=""og:image"" content=""http://www.test.com/test.png""/>
    <meta propErty=""og:uRl"" content=""http://www.test.com"" />
    <meta property=""og:description"" content=""My Description""/>
    <meta property=""og:site_Name"" content=""Test Site"">
    <meta property=""og:mistake"" value=""not included"">
</head>
<body>
</body>
</html>";

        /// <summary>
        /// The invalid missing required URLs
        /// </summary>
        private string invalidMissingRequiredUrls = @"<!DOCTYPE HTML>
<html>
<head>
    <meta property=""og:type"" content=""product"" />
    <meta property=""og:title"" cOntent=""Product Title"" />
    <meta property=""og:description"" content=""My Description""/>
    <meta property=""og:site_Name"" content=""Test Site"">
</head>
<body>
</body>
</html>";

        /// <summary>
        /// The invalid missing all meta
        /// </summary>
        private string invalidMissingAllMeta = @"<!DOCTYPE HTML>
<html>
<head>
    <title>some title</title>
</head>
<body>
</body>
</html>";

        /// <summary>
        /// Tests calling <c>MakeOpenGraph</c> method
        /// </summary>
        public void MakeOpenGraph_Test()
        {
            var title = "some title";
            var type = "website";
            var image = "http://www.go.com/someimg.jpg";
            var url = "http://www.go.com/";
            var description = "some description";
            var siteName = "my site";
            var graph = OpenGraph.MakeGraph(title, type, image, url, description, siteName);

            Assert.AreEqual(title, graph.Title);
            Assert.AreEqual(type, graph.Type);
            Assert.AreEqual(image, graph.Image.ToString());
            Assert.AreEqual(url, graph.Url.ToString());
            Assert.AreEqual(description, graph["description"]);
            Assert.AreEqual(siteName, graph["site_name"]);

            var expected = "<meta property=\"og:title\" content=\"some title\">" +
                "<meta property=\"og:type\" content=\"website\">" +
                "<meta property=\"og:image\" content=\"http://www.go.com/someimg.jpg\">" +
                "<meta property=\"og:url\" content=\"http://www.go.com/\">" +
                "<meta property=\"og:description\" content=\"some description\">" +
                "<meta property=\"og:site_name\" content=\"my site\">";
            Assert.AreEqual(expected, graph.ToString());
        }

        /// <summary>
        /// Tests parsing the HTML
        /// </summary>
        [Test]
        public void ParseHtml_ValidGraphParsing_Test()
        {
            OpenGraph graph = OpenGraph.ParseHtml(this.validSampleContent, true);

            Assert.AreEqual("product", graph.Type);
            Assert.AreEqual("Product Title", graph.Title);
            Assert.AreEqual("http://www.test.com/test.png", graph.Image.ToString());
            Assert.AreEqual("http://www.test.com/", graph.Url.ToString());
            Assert.AreEqual("My Description", graph["description"]);
            Assert.AreEqual("Test Site", graph["site_name"]);
        }

        /// <summary>
        /// Tests parsing the HTML that is missing URLs
        /// </summary>
        [Test]
        public void ParseHtml_HtmlMissingUrls_Test()
        {
            OpenGraph graph = OpenGraph.ParseHtml(this.invalidMissingRequiredUrls, false);

            Assert.AreEqual("product", graph.Type);
            Assert.AreEqual("Product Title", graph.Title);
            Assert.IsNull(graph.Image);
            Assert.IsNull(graph.Url);
            Assert.AreEqual("My Description", graph["description"]);
            Assert.AreEqual("Test Site", graph["site_name"]);
        }

        /// <summary>
        /// Test that parsing the HTML with invalid graph specification throws an exception
        /// </summary>
        [Test, ExpectedException(typeof(InvalidSpecificationException))]
        public void ParseHtml_InvaidGraphParsing_Test()
        {
            OpenGraph graph = OpenGraph.ParseHtml(this.invalidSampleContent, true);
        }

        /// <summary>
        /// Test that parsing the HTML with invalid graph specification throws an exception
        /// </summary>
        [Test, ExpectedException(typeof(InvalidSpecificationException))]
        public void ParseHtml_InvaidGraphParsingMissingAllMeta_Test()
        {
            OpenGraph graph = OpenGraph.ParseHtml(this.invalidMissingAllMeta, true);
        }

        /// <summary>
        /// Test that parsing the HTML with invalid graph specification passes when validate specification boolean is off
        /// </summary>
        [Test]
        public void ParseHtml_InvalidGraphParsingWithoutCheck_Test()
        {
            OpenGraph graph = OpenGraph.ParseHtml(this.invalidSampleContent);

            Assert.AreEqual(string.Empty, graph.Type);
            Assert.IsFalse(graph.ContainsKey("mistake"));
            Assert.AreEqual("Product Title", graph.Title);
            Assert.AreEqual("http://www.test.com/test.png", graph.Image.ToString());
            Assert.AreEqual("http://www.test.com/", graph.Url.ToString());
            Assert.AreEqual("My Description", graph["description"]);
            Assert.AreEqual("Test Site", graph["site_name"]);
        }

        /// <summary>
        /// Test parsing a URL
        /// </summary>
        [Test]
        public void ParseUrl_AmazonUrl_Test()
        {
            OpenGraph graph = OpenGraph.ParseUrl("http://www.amazon.com/Spaced-Complete-Simon-Pegg/dp/B0019MFY3Q");

            Assert.AreEqual("http://www.amazon.com/dp/B0019MFY3Q/ref=tsm_1_fb_lk", graph.Url.ToString());
            Assert.IsTrue(graph.Title.StartsWith("Spaced: The Complete Series"));
            Assert.IsTrue(graph["description"].Contains("Spaced"));
            Assert.IsTrue(graph.Image.ToString().StartsWith("https://images-na.ssl-images-amazon.com/images/I"));
            Assert.AreEqual("movie", graph.Type);
            Assert.AreEqual("Amazon.com", graph["site_name"]);
        }


        [Test]
        public void ParseUrl_Vk_Test()
        {
            var graph = ParseUrl("https://vk.com/wall-41600377_66756");
            Assert.AreEqual(graph["description"], "Создайте себе горное настроение с нашим первым фан-китом по игре #SteepGame&amp;#33; -&amp;gt; http://ubi.li/u8w9n");
        }

        [Test]
        public void ParseUrl_Periscope_Encoded_Test()
        {
            var graph = ParseUrl("https://www.periscope.tv/w/1DXxyZZZVykKM");
            Assert.AreEqual(graph["image"], "https://tn.periscope.tv/lXc5gSh6UPaWdc37LtVCb3UdtSfvj2QNutojPK2du5YWrNchfI4wXpwwHKTyfDhmfT2ibsBZV4doQeWlhSvI4A==/chunk_314.jpg?Expires=1781852253&Signature=U5OY3Y2HRb4ETmakQAPwMcv~bqu6KygIxriooa41rk64RcDfjww~qpVgMR-T1iX4S9NxfvXHLMT3pEckBDEOicsNO7oUAo4NieH9GRB2Sv0EA7swxLojD~Zn98ThNWTF5fSzv6SSPjyvctsqBiRmvAN6x7fmMH6l3vzx8ePSCgdEm8-31lUAz7lReBNZQjYSi~C8AwqZVI0Mx6y8lNKklL~m0e6RTGdvr~-KIDewU3wpjSdX7AgpaXXjahk4x-ceUUKcH3T1j--ZjaY7nqPO9fbMZFNPs502A32mrcmaZCzvaD~AuoH~u3y44mJVjzHRrpTxHIBklqHxAgc7dzverg__&Key-Pair-Id=APKAIHCXHHQVRTVSFRWQ");
        }

        [Test]
        public void ParseUrl_EncodingError_Test()
        {
            var ogs = ParseUrl("http://www.telerama.fr/cinema/realite-virtuelle-360-de-bonheur-a-ameliorer,144339.php?utm_medium=Social&utm_source=Twitter&utm_campaign=Echobox&utm_term=Autofeed#link_time=1466595239");
            Assert.AreEqual(ogs["title"], "Réalité virtuelle : 360° de bonheur à améliorer");
            Assert.AreEqual(ogs["description"], "Le cinéma à 360° a désormais son festival. Organisé par le Forum des images, le premier Paris Virtual Film Festival a donc vu le jour....");
        }


        [Test]
        public void ParseUrl_TwitterMetas_Test()
        {
            var ogs = ParseUrl("http://www.sociabble.com/blog/sociabble-shares-social-selling-expertise-microsoft-experiences-hubforum-paris/");
            Assert.AreEqual(ogs.TwitterCard, "summary_large_image");
            Assert.AreEqual(ogs.TwitterDescription, "Microsoft experiences is teaming up with HUBFORUM Paris for a day dedicated to digital innovation. Sociabble will host two sessions on social selling.");
            Assert.AreEqual(ogs.TwitterTitle, "Sociabble at Microsoft experiences and HUBFORUM Paris");
            Assert.AreEqual(ogs.TwitterSite, "@SociabbleApp");
            Assert.AreEqual(ogs.TwitterImage, "http://www.sociabble.com/wp-content/uploads/2016/09/MS-Experiences-2016_opt.png");
            Assert.AreEqual(ogs.TwitterCreator, "@SociabbleApp");
        }

        protected OpenGraph ParseUrl(string u)
        {
            return OpenGraph.ParseUrl(u);
        }
    }
}
