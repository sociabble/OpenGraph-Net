namespace OpenGraphNet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Http Download component
    /// </summary>
    /// <remarks>
    ///  see <a href="http://stackoverflow.com/a/2700707">solution</a>
    /// </remarks>
    public class HttpDownloader
    {
        /// <summary>
        /// The referrer
        /// </summary>
        private readonly string referrer;
    
        /// <summary>
        /// The user agent
        /// </summary>
        private readonly string userAgent;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDownloader" /> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="userAgent">The user agent.</param>
        public HttpDownloader(Uri url, string referrer, string userAgent)
        {
            this.Encoding = Encoding.GetEncoding("ISO-8859-1");
            this.Url = url;
            this.userAgent = userAgent;
            this.referrer = referrer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDownloader"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="userAgent">The user agent.</param>
        public HttpDownloader(string url, string referrer, string userAgent) : this(new Uri(url), referrer, userAgent)
        {
        }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        public HttpResponseHeaders Headers { get; set; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>
        /// The HTML content of a page
        /// </returns>
        public async Task<string> GetPageAsync()
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(this.referrer))
            {
                client.DefaultRequestHeaders.Referrer = new Uri(this.referrer);
            }

            if (!string.IsNullOrEmpty(this.userAgent))
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(this.userAgent, "1.0"));
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("deflate"));

            using (var response = await client.GetAsync(this.Url))
            {
                this.Headers = response.Headers;
                return await this.ProcessContentAsync(response);
            }
        }

        /// <summary>
        /// Processes the content.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The processed content</returns>
        private async Task<string> ProcessContentAsync(HttpResponseMessage response)
        {
            this.SetEncodingFromHeader(response);

            using (var source = await response.Content.ReadAsStreamAsync())
            {
                if (source == null)
                {
                    throw new NullReferenceException("The response stream was null!");
                }

                string html;
                using (var uncompressed = this.GetUncompressedStream(source, response.Content.Headers.ContentEncoding))
                {
                    using (var memStream = new MemoryStream())
                    {
                        int bytesRead;
                        var buffer = new byte[0x1000];
                        for (bytesRead = uncompressed.Read(buffer, 0, buffer.Length);
                             bytesRead > 0;
                             bytesRead = uncompressed.Read(buffer, 0, buffer.Length))
                        {
                            memStream.Write(buffer, 0, bytesRead);
                        }

                        memStream.Position = 0;
                        using (var r = new StreamReader(memStream, this.Encoding))
                        {
                            html = r.ReadToEnd().Trim();
                            html = this.CheckMetaCharSetAndReEncode(memStream, html);
                        }
                    }
                }

                return html;
            }
        }

        /// <summary>
        /// Gets the uncompressed stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="contentEncoding">The content encoding.</param>
        /// <returns>The uncompressed <see cref="Stream"/>.</returns>
        private Stream GetUncompressedStream(Stream source, ICollection<string> contentEncoding)
        {
            if (contentEncoding.Any(ce => "gzip".Equals(ce, StringComparison.OrdinalIgnoreCase)))
            {
                return new GZipStream(source, CompressionMode.Decompress);
            }

            return contentEncoding.Any(ce => "deflate".Equals(ce, StringComparison.OrdinalIgnoreCase)) ? new DeflateStream(source, CompressionMode.Decompress) : source;
        }

        /// <summary>
        /// Sets the encoding from header.
        /// </summary>
        /// <param name="response">The response.</param>
        private void SetEncodingFromHeader(HttpResponseMessage response)
        {
            if (!string.IsNullOrEmpty(response.Content.Headers.ContentType.CharSet))
            {
                try
                {
                    this.Encoding = Encoding.GetEncoding(response.Content.Headers.ContentType.CharSet);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        /// <summary>
        /// Checks the meta character set and re encode.
        /// </summary>
        /// <param name="memStream">The memory stream.</param>
        /// <param name="html">The HTML.</param>
        /// <returns>The re-encoded HTML</returns>
        private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
        {
            var m = new Regex(@"<meta\s+.*?charset\s*=\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Match(html);

            if (!m.Success)
            {
                return html;
            }

            var charset = m.Groups["charset"].Value.ToLower();
            if ((charset == "unicode") || (charset == "utf-16"))
            {
                charset = "utf-8";
            }

            try
            {
                var metaEncoding = Encoding.GetEncoding(charset);
                if (!this.Encoding.Equals(metaEncoding))
                {
                    memStream.Position = 0L;
                    using (var recodeReader = new StreamReader(memStream, metaEncoding))
                    {
                        html = recodeReader.ReadToEnd().Trim();
                    }
                }
            }
            catch (ArgumentException)
            {
                // Ignore
            }

            return html;
        }
    }
}
