namespace OpenGraphNet.Tests
{
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// The open graph test fixture
    /// </summary>
    public class OpenGraphTests
    {
        /// <summary>
        /// The valid sample content
        /// </summary>
        private const string ValidSampleContent = @"<!DOCTYPE HTML>
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
        private const string InvalidSampleContent = @"<!DOCTYPE HTML>
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
        private const string InvalidMissingRequiredUrls = @"<!DOCTYPE HTML>
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
        private const string InvalidMissingAllMeta = @"<!DOCTYPE HTML>
<html>
<head>
    <title>some title</title>
</head>
<body>
</body>
</html>";

        /// <summary>
        /// Tests calling <c>MakeGraph</c> on <see cref="OpenGraphNet.OpenGraph"/>
        /// </summary>
        [Fact]
        public void MakeOpenGrapTest()
        {
            var title = "some title";
            var type = "website";
            var image = "http://www.go.com/someimg.jpg";
            var url = "http://www.go.com/";
            var description = "some description";
            var siteName = "my site";
            
            var graph = OpenGraph.MakeGraph(title, type, image, url, description, siteName);

            Assert.Equal(title, graph.Title);
            Assert.Equal(type, graph.Type);
            Assert.Equal(image, graph.Image.ToString());
            Assert.Equal(url, graph.Url.ToString());
            Assert.Equal(description, graph["description"]);
            Assert.Equal(siteName, graph["site_name"]);

            var expected = "<meta property=\"og:title\" content=\"some title\">" +
                "<meta property=\"og:type\" content=\"website\">" +
                "<meta property=\"og:image\" content=\"http://www.go.com/someimg.jpg\">" +
                "<meta property=\"og:url\" content=\"http://www.go.com/\">" +
                "<meta property=\"og:description\" content=\"some description\">" +
                "<meta property=\"og:site_name\" content=\"my site\">";
            Assert.Equal(expected, graph.ToString());
        }

        /// <summary>
        /// Tests parsing the HTML
        /// </summary>
        [Fact]
        public void ParseHtmlValidGraphParsingTest()
        {
            OpenGraph graph = OpenGraph.ParseHtml(ValidSampleContent, true);

            Assert.Equal("product", graph.Type);
            Assert.Equal("Product Title", graph.Title);
            Assert.Equal("http://www.test.com/test.png", graph.Image.ToString());
            Assert.Equal("http://www.test.com/", graph.Url.ToString());
            Assert.Equal("My Description", graph["description"]);
            Assert.Equal("Test Site", graph["site_name"]);
        }

        /// <summary>
        /// Tests parsing the HTML that is missing URLs
        /// </summary>
        [Fact]
        public void ParseHtmlHtmlMissingUrlsTest()
        {
            OpenGraph graph = OpenGraph.ParseHtml(InvalidMissingRequiredUrls, false);

            Assert.Equal("product", graph.Type);
            Assert.Equal("Product Title", graph.Title);
            Assert.Null(graph.Image);
            Assert.Null(graph.Url);
            Assert.Equal("My Description", graph["description"]);
            Assert.Equal("Test Site", graph["site_name"]);
        }

        /// <summary>
        /// Test that parsing the HTML with invalid graph specification throws an exception
        /// </summary>
        [Fact]
        public void ParseHtmlInvalidGraphParsingTest()
        {
            Assert.Throws<InvalidSpecificationException>(() => OpenGraph.ParseHtml(InvalidSampleContent, true));
        }

        /// <summary>
        /// Test that parsing the HTML with invalid graph specification throws an exception
        /// </summary>
        [Fact]
        public void ParseHtmlInvaidGraphParsingMissingAllMetaTest()
        {
            Assert.Throws<InvalidSpecificationException>(() => OpenGraph.ParseHtml(InvalidMissingAllMeta, true));
        }

        /// <summary>
        /// Test that parsing the HTML with invalid graph specification passes when validate specification boolean is off
        /// </summary>
        [Fact]
        public void ParseHtmlInvalidGraphParsingWithoutCheckTest()
        {
            OpenGraph graph = OpenGraph.ParseHtml(InvalidSampleContent);

            Assert.Equal(string.Empty, graph.Type);
            Assert.False(graph.ContainsKey("mistake"));
            Assert.Equal("Product Title", graph.Title);
            Assert.Equal("http://www.test.com/test.png", graph.Image.ToString());
            Assert.Equal("http://www.test.com/", graph.Url.ToString());
            Assert.Equal("My Description", graph["description"]);
            Assert.Equal("Test Site", graph["site_name"]);
        }

        /// <summary>
        /// Test parsing a URL
        /// </summary>
        [Fact]
        public async Task ParseUrlAmazonUrlTest()
        {
            OpenGraph graph = await OpenGraph.ParseUrlAsync("http://www.amazon.com/Spaced-Complete-Simon-Pegg/dp/B0019MFY3Q");

            Assert.Equal("http://www.amazon.com/dp/B0019MFY3Q/ref=tsm_1_fb_lk", graph.Url.ToString());
            Assert.True(graph.Title.StartsWith("Spaced: The Complete Series"));
            Assert.True(graph["description"].Contains("Spaced"));
            Assert.True(graph.Image.ToString().StartsWith("http://ecx.images-amazon.com/images/I"));
            Assert.Equal("movie", graph.Type);
            Assert.Equal("Amazon.com", graph["site_name"]);
        }
    }
}
