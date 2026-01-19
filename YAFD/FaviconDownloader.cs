using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace YetAnotherFaviconDownloader
{
    [System.ComponentModel.DesignerCategory("")]
    public sealed class FaviconDownloader : WebClient
    {
        // Proxy
        private static IWebProxy _proxy;
        public static new IWebProxy Proxy { get { return _proxy; } set { _proxy = value; } }

        // User Agent - Modern Chrome
        private static readonly string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

        // Regular expressions
        private static readonly Regex dataSchema, httpSchema, androidAppSchema;
        private static readonly Regex headTag, baseTag, commentTag, scriptStyleTag;
        private static readonly Regex linkTags, relAttribute, relAppleTouchIcon, relMaskIcon;
        private static readonly Regex hrefAttribute, sizesAttribute, typeAttribute;

        // Android package to domain mapping patterns - use the comprehensive mapping class
        // (Inline mappings kept as fallback for common apps)

        // URI after redirection
        private Uri responseUri;
        private CookieContainer cookieContainer;

        // Configurable timeout (in milliseconds)
        private int timeoutMs = 15000;
        private int readWriteTimeoutMs = 45000;

        static FaviconDownloader()
        {
            // Data URI schema
            dataSchema = new Regex(@"data:(?<mediatype>.*?)(;(?<base64>.+?))?,(?<data>.+)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

            // HTTP URI schema
            httpSchema = new Regex(@"^http(s)?://.+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            // Android app URI schema: androidapp://com.package.name
            androidAppSchema = new Regex(@"^androidapp://(?<package>[a-zA-Z0-9_.]+)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

            // <head> tag
            headTag = new Regex(@"<head\b.*?>.*?</head>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            // <!-- --> comment tags
            commentTag = new Regex(@"<!--.*?-->", RegexOptions.Compiled | RegexOptions.Singleline);

            // <script> or <style> tags
            scriptStyleTag = new Regex(@"<(script|style)\b.*?>.*?</\1>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            // <base> tags
            baseTag = new Regex(@"<base\b.*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            // <link> tags
            linkTags = new Regex(@"<link\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            // rel="icon" or rel="shortcut icon"
            relAttribute = new Regex(@"rel\s*=\s*(icon\b|(?<q>'|"")\s*(shortcut\s*\b)?icon\b\s*\k<q>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

            // rel="apple-touch-icon" or rel="apple-touch-icon-precomposed"
            relAppleTouchIcon = new Regex(@"rel\s*=\s*(?<q>'|"")?apple-touch-icon(-precomposed)?\b\s*\k<q>?", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

            // rel="mask-icon" (Safari pinned tab)
            relMaskIcon = new Regex(@"rel\s*=\s*(?<q>'|"")?mask-icon\b\s*\k<q>?", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

            // href attribute
            hrefAttribute = new Regex(@"href\s*=\s*((?<q>'|"")(?<url>.*?)(\k<q>|>)|(?<url>.*?)(\s+|>))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

            // sizes attribute (e.g., sizes="32x32" or sizes="180x180")
            sizesAttribute = new Regex(@"sizes\s*=\s*(?<q>'|"")?(?<size>\d+)x\d+\s*\k<q>?", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

            // type attribute (to detect SVG)
            typeAttribute = new Regex(@"type\s*=\s*(?<q>'|"")?(?<type>[^'""\s>]+)\s*\k<q>?", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

            // Enable TLS for newer .NET versions
            try
            {
                SecurityProtocolType spt = (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls);

                Type tSpt = typeof(SecurityProtocolType);
                string[] vSpt = Enum.GetNames(tSpt);
                foreach (string strSpt in vSpt)
                {
                    if (strSpt.Equals("Tls11", StrUtil.CaseIgnoreCmp) ||
                        strSpt.Equals("Tls12", StrUtil.CaseIgnoreCmp) ||
                        strSpt.Equals("Tls13", StrUtil.CaseIgnoreCmp))
                        spt |= (SecurityProtocolType)Enum.Parse(tSpt, strSpt, true);
                }

                ServicePointManager.SecurityProtocol = spt;
            }
            catch (Exception) { }
        }

        public FaviconDownloader()
        {
            cookieContainer = new CookieContainer();

            // Load timeout from config
            try
            {
                timeoutMs = YetAnotherFaviconDownloaderExt.Config.GetConnectionTimeout() * 1000;
                readWriteTimeoutMs = timeoutMs * 3;
            }
            catch { }

            // Set up certificate validation if needed
            try
            {
                if (YetAnotherFaviconDownloaderExt.Config.GetAllowInvalidCertificates())
                {
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertificates;
                }
            }
            catch { }
        }

        private static bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Converts an Android app URL to a web domain
        /// </summary>
        public static string ConvertAndroidUrlToDomain(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            Match match = androidAppSchema.Match(url);
            if (!match.Success)
                return null;

            string packageName = match.Groups["package"].Value;

            // First, check our known mappings using the comprehensive mapping class
            string domain = AndroidPackageMapping.GetDomain(packageName);
            if (domain != null)
            {
                return domain;
            }

            // Try to derive domain from package name using heuristics
            domain = AndroidPackageMapping.DeriveDomain(packageName);
            if (domain != null)
            {
                return domain;
            }

            return null;
        }

        /// <summary>
        /// Check if URL is an Android app URL
        /// </summary>
        public static bool IsAndroidUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && androidAppSchema.IsMatch(url);
        }

        public byte[] GetIcon(string url)
        {
            bool wasAndroidUrl = false;
            
            // Handle Android URLs
            if (IsAndroidUrl(url))
            {
                try
                {
                    string domain = ConvertAndroidUrlToDomain(url);
                    if (!string.IsNullOrEmpty(domain))
                    {
                        Util.Log("Android URL converted: {0} => {1}", url, domain);
                        url = domain;
                        wasAndroidUrl = true;
                    }
                    else
                    {
                        Util.Log("Android URL could not be converted: {0}", url);
                        throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
                    }
                }
                catch (FaviconDownloaderException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Util.Log("Error converting Android URL {0}: {1}", url, ex.Message);
                    throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
                }
            }

            string origURL = url;

            // Check if the URL could be a site address
            // Force prefix validation if it was an Android URL, as we know it's a domain now
            if (!IsValidURL(ref url, "https://", wasAndroidUrl))
            {
                throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
            }

            int attempts = 0;

        retry_http:

            if (++attempts > 4)
            {
                throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.Error);
            }
            Util.Log("Attempt {0}: {1}", attempts, url);

            try
            {
                Uri address = new Uri(url);

                // Download and parse page
                string page = DownloadPage(address);
                string head = StripPage(page);
                List<IconCandidate> candidates = GetIconCandidates(responseUri, head);

                // Sort by priority (highest quality first)
                candidates.Sort((a, b) => b.Priority.CompareTo(a.Priority));

                // Try to find a valid image
                foreach (IconCandidate candidate in candidates)
                {
                    try
                    {
                        byte[] data = DownloadAsset(candidate.Url);

                        if (ResizeImage(ref data))
                        {
                            return data;
                        }
                    }
                    catch (WebException)
                    {
                        // ignore and try next
                    }
                    catch (Exception)
                    {
                        // ignore and try next
                    }
                }

                // No valid image found - try stripping path
                if (address.PathAndQuery != "/")
                {
                    url = address.GetLeftPart(UriPartial.Authority);
                    goto retry_http;
                }
            }
            catch (WebException ex)
            {
                // Retry with HTTP prefix
                if (!httpSchema.IsMatch(origURL) && url.StartsWith("https://"))
                {
                    url = origURL;

                    if (IsValidURL(ref url, "http://"))
                    {
                        goto retry_http;
                    }
                }

                HttpWebResponse response = ex.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
                }
                else
                {
                    throw new FaviconDownloaderException(ex);
                }
            }

            throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
        }

        /// <summary>
        /// Try multiple fallback providers to get an icon
        /// </summary>
        public byte[] GetIconWithFallback(string url)
        {
            // First try direct download
            try
            {
                return GetIcon(url);
            }
            catch (FaviconDownloaderException)
            {
                // Continue to fallback providers
            }

            // Check if fallback is enabled
            if (!YetAnotherFaviconDownloaderExt.Config.GetUseFallbackProviders())
            {
                throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
            }

            var hostname = GetValidHost(url);
            if (string.IsNullOrEmpty(hostname))
            {
                throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
            }

            // Get scheme for providers that need it
            string scheme = "https";
            try
            {
                if (httpSchema.IsMatch(url))
                {
                    Uri uri = new Uri(url);
                    scheme = uri.Scheme;
                }
            }
            catch { }

            // Fallback provider URLs
            string[] fallbackProviders = new string[]
            {
                string.Format("https://icons.duckduckgo.com/ip3/{0}.ico", hostname),
                string.Format("https://www.google.com/s2/favicons?domain={0}://{1}&sz=128", scheme, hostname),
                string.Format("https://icon.horse/icon/{0}", hostname),
                string.Format("https://favicon.yandex.net/favicon/{0}", hostname),
            };

            foreach (string providerUrl in fallbackProviders)
            {
                try
                {
                    Util.Log("Trying fallback: {0}", providerUrl);
                    byte[] data = DownloadData(new Uri(providerUrl));

                    if (ResizeImage(ref data))
                    {
                        return data;
                    }
                }
                catch
                {
                    // Try next provider
                }
            }

            throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
        }

        public byte[] GetIconCustomProvider(string url)
        {
            // Handle Android URLs
            if (IsAndroidUrl(url))
            {
                try
                {
                    string domain = ConvertAndroidUrlToDomain(url);
                    if (!string.IsNullOrEmpty(domain))
                    {
                        url = "https://" + domain;
                    }
                    else
                    {
                        Util.Log("Android URL could not be converted for custom provider: {0}", url);
                        throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
                    }
                }
                catch (FaviconDownloaderException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Util.Log("Error converting Android URL for custom provider {0}: {1}", url, ex.Message);
                    throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
                }
            }

            var hostname = GetValidHost(url);
            
            // Validate hostname before proceeding
            if (string.IsNullOrEmpty(hostname))
            {
                Util.Log("Could not extract hostname from URL: {0}", url);
                throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
            }
            
            var scheme = "https";

            try
            {
                if (httpSchema.IsMatch(url))
                {
                    Uri uri = new Uri(url);
                    scheme = uri.Scheme;
                }
            }
            catch { }

            var providerURL = YetAnotherFaviconDownloaderExt.Config.GetCustomDownloadProvider();
            var iconSize = YetAnotherFaviconDownloaderExt.Config.GetMaximumIconSize().ToString();

            // Support more placeholders
            providerURL = Regex.Replace(providerURL, "{URL:HOST}", hostname, RegexOptions.IgnoreCase);
            providerURL = Regex.Replace(providerURL, "{URL:SCM}", scheme, RegexOptions.IgnoreCase);
            providerURL = Regex.Replace(providerURL, "{YAFD:ICON_SIZE}", iconSize, RegexOptions.IgnoreCase);

            Uri address = new Uri(providerURL);

            Util.Log("CustomProvider: {0} => {1}", url, providerURL);

            try
            {
                byte[] data = DownloadData(address);

                if (ResizeImage(ref data))
                {
                    return data;
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
                }
                else
                {
                    throw new FaviconDownloaderException(ex);
                }
            }

            throw new FaviconDownloaderException(FaviconDownloaderExceptionStatus.NotFound);
        }

        public string GetValidHost(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";
                
            try
            {
                // Handle Android URLs
                if (IsAndroidUrl(url))
                {
                    string domain = ConvertAndroidUrlToDomain(url);
                    if (!string.IsNullOrEmpty(domain))
                    {
                        return domain;
                    }
                    return "";
                }

                if (!httpSchema.IsMatch(url))
                {
                    url = "http://" + url;
                }

                Uri result;
                if (Uri.TryCreate(url, UriKind.Absolute, out result))
                {
                    return result.Host ?? "";
                }
            }
            catch (Exception ex)
            {
                Util.Log("Error getting valid host from URL {0}: {1}", url, ex.Message);
            }

            return "";
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;

            // Set up proxy information
            request.Proxy = Proxy;

            // Configurable timeouts
            request.Timeout = timeoutMs;
            request.ReadWriteTimeout = readWriteTimeoutMs;
            request.ContinueTimeout = 1000;

            // Follow redirection responses
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 10;

            // Sets the cookies associated with the request
            request.CookieContainer = cookieContainer;

            // Sets a modern user agent
            request.UserAgent = userAgent;

            // Modern browser headers
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
            request.AutomaticDecompression |= DecompressionMethods.GZip | DecompressionMethods.Deflate;

            // Additional headers to look more like a real browser
            request.Headers.Add("Sec-Fetch-Dest", "document");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "none");
            request.Headers.Add("Sec-Fetch-User", "?1");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");

            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = base.GetWebResponse(request);
                responseUri = response.ResponseUri;
            }
            catch (WebException)
            {
                // not handling here
            }

            return response;
        }

        private byte[] DownloadAsset(Uri address)
        {
            // Data URI scheme
            if (address.Scheme == "data")
            {
                string uri = address.ToString();

                Match match = dataSchema.Match(uri);
                if (match.Success)
                {
                    string data = match.Groups["data"].Value;

                    try
                    {
                        return Convert.FromBase64String(data);
                    }
                    catch (FormatException)
                    {
                        return null;
                    }
                }

                return null;
            }

            // HTTP/HTTPS scheme
            if (address.Scheme == "http" || address.Scheme == "https")
            {
                return DownloadData(address);
            }

            return null;
        }

        private bool IsValidURL(ref string url, string prefix, bool force = false)
        {
            // If it already has a scheme, check if valid
            if (httpSchema.IsMatch(url))
            {
                Uri result;
                return Uri.TryCreate(url, UriKind.Absolute, out result);
            }

            // Check if automatic prefix is enabled, OR if we are forcing it (Android/Title cases)
            if (!force && !YetAnotherFaviconDownloaderExt.Config.GetAutomaticPrefixURLs())
            {
                return false;
            }

            // Prefix the URL with a valid schema
            string old = url;
            url = prefix + url;
            Util.Log("AutoPrefix: {0} => {1}", old, url);

            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult);
        }

        private string DownloadPage(Uri address)
        {
            string html = DownloadString(address);
            return html;
        }

        private string StripPage(string html)
        {
            // Extract <head> tag
            Match match = headTag.Match(html);
            if (match.Success)
            {
                html = match.Value;
                html = commentTag.Replace(html, string.Empty);
                html = scriptStyleTag.Replace(html, string.Empty);
            }

            return html;
        }

        private bool NormalizeHref(Uri baseUri, string relativeUri, out Uri result)
        {
            StringBuilder sb = new StringBuilder(relativeUri.Trim());
            sb.Replace("\t", "");
            sb.Replace("\n", "");
            sb.Replace("\r", "");

            relativeUri = sb.ToString();

            if (Uri.TryCreate(baseUri, relativeUri, out result))
            {
                switch (result.Scheme)
                {
                    case "data":
                    case "http":
                    case "https":
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Represents an icon candidate with priority
        /// </summary>
        private class IconCandidate
        {
            public Uri Url { get; set; }
            public int Priority { get; set; }
            public int Size { get; set; }
            public string Type { get; set; }

            public IconCandidate(Uri url, int priority, int size = 0, string type = null)
            {
                Url = url;
                Priority = priority;
                Size = size;
                Type = type;
            }
        }

        /// <summary>
        /// Gets all icon candidates from HTML, sorted by priority
        /// </summary>
        private List<IconCandidate> GetIconCandidates(Uri entryUrl, string html)
        {
            // Extract <base> tag
            Match match = baseTag.Match(html);
            if (match.Success)
            {
                string baseHtml = match.Value;
                Match hrefHtml = hrefAttribute.Match(baseHtml);
                if (hrefHtml.Success)
                {
                    string href = hrefHtml.Groups["url"].Value;
                    Uri baseUrl;
                    if (NormalizeHref(entryUrl, href, out baseUrl))
                    {
                        entryUrl = baseUrl;
                    }
                }
            }

            List<IconCandidate> candidates = new List<IconCandidate>();
            int targetSize = YetAnotherFaviconDownloaderExt.Config.GetMaximumIconSize();

            // Loops through each <link> tag
            foreach (Match linkTag in linkTags.Matches(html))
            {
                string linkHtml = linkTag.Value;
                Uri iconUrl;

                // Extract href first
                Match hrefMatch = hrefAttribute.Match(linkHtml);
                if (!hrefMatch.Success)
                    continue;

                string href = hrefMatch.Groups["url"].Value;
                if (!NormalizeHref(entryUrl, href, out iconUrl))
                    continue;

                // Get size if specified
                int size = 0;
                Match sizeMatch = sizesAttribute.Match(linkHtml);
                if (sizeMatch.Success)
                {
                    int.TryParse(sizeMatch.Groups["size"].Value, out size);
                }

                // Get type if specified
                string type = null;
                Match typeMatch = typeAttribute.Match(linkHtml);
                if (typeMatch.Success)
                {
                    type = typeMatch.Groups["type"].Value.ToLowerInvariant();
                }

                // Skip SVG for now (KeePass doesn't support them well)
                if (type != null && type.Contains("svg"))
                    continue;

                // Calculate priority based on type and size
                int priority = 0;

                // Apple touch icons are high quality
                if (relAppleTouchIcon.IsMatch(linkHtml))
                {
                    priority = 1000;
                    if (size >= 180) priority += 200;
                    else if (size >= 152) priority += 150;
                    else if (size >= 120) priority += 100;
                    else if (size > 0) priority += size;
                }
                // Standard icon
                else if (relAttribute.IsMatch(linkHtml))
                {
                    priority = 500;

                    // Prefer sizes close to our target
                    if (size > 0)
                    {
                        if (size >= targetSize) priority += 100 + (200 - Math.Abs(size - targetSize));
                        else priority += size;
                    }

                    // PNG is preferred over ICO
                    if (href.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        priority += 50;
                    else if (href.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                        priority += 25;

                    // Detect common high-quality patterns
                    if (href.Contains("favicon-32x32") || href.Contains("favicon-96x96") || href.Contains("favicon-192x192"))
                        priority += 75;
                }
                // Mask icon (Safari pinned tab) - lower priority
                else if (relMaskIcon.IsMatch(linkHtml))
                {
                    priority = 100;
                }
                else
                {
                    // Unknown link type with href - might still be useful
                    continue;
                }

                candidates.Add(new IconCandidate(iconUrl, priority, size, type));
            }

            // Add fallback: /favicon.ico
            Uri faviconIco;
            if (Uri.TryCreate(entryUrl, "/favicon.ico", out faviconIco))
            {
                candidates.Add(new IconCandidate(faviconIco, 50, 16, "image/x-icon"));
            }

            // Add fallback: /apple-touch-icon.png
            Uri appleTouchIcon;
            if (Uri.TryCreate(entryUrl, "/apple-touch-icon.png", out appleTouchIcon))
            {
                candidates.Add(new IconCandidate(appleTouchIcon, 200, 180, "image/png"));
            }

            // Add fallback: /apple-touch-icon-precomposed.png
            Uri appleTouchIconPrecomposed;
            if (Uri.TryCreate(entryUrl, "/apple-touch-icon-precomposed.png", out appleTouchIconPrecomposed))
            {
                candidates.Add(new IconCandidate(appleTouchIconPrecomposed, 195, 180, "image/png"));
            }

            // Add fallback: /favicon-32x32.png
            Uri favicon32;
            if (Uri.TryCreate(entryUrl, "/favicon-32x32.png", out favicon32))
            {
                candidates.Add(new IconCandidate(favicon32, 150, 32, "image/png"));
            }

            // Remove duplicates by URL
            var seen = new HashSet<string>();
            var uniqueCandidates = new List<IconCandidate>();
            foreach (var c in candidates)
            {
                if (seen.Add(c.Url.ToString()))
                {
                    uniqueCandidates.Add(c);
                }
            }

            return uniqueCandidates;
        }

        private bool IsValidImage(byte[] data)
        {
            if (data == null)
            {
                return false;
            }

            try
            {
                Image image = GfxUtil.LoadImage(data);
                return true;
            }
            catch (Exception)
            {
                // Invalid image format
            }

            return false;
        }

        private bool ResizeImage(ref byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return false;
            }

            // Check for common "not found" placeholder images
            if (data.Length < 100)
            {
                // Suspiciously small, probably a 1x1 placeholder
                return false;
            }

            int MaxWidth, MaxHeight;
            MaxWidth = MaxHeight = YetAnotherFaviconDownloaderExt.Config.GetMaximumIconSize();

            Image image;
            try
            {
                image = GfxUtil.LoadImage(data);
            }
            catch (Exception)
            {
                return false;
            }

            // Check for tiny placeholder images (1x1, 2x2)
            if (image.Width <= 2 || image.Height <= 2)
            {
                return false;
            }

            // Checks if we need to resize
            if (image.Width <= MaxWidth && image.Height <= MaxHeight)
            {
                return true;
            }

            // Try to resize the image to png
            try
            {
                double ratioWidth = MaxWidth / (double)image.Width;
                double ratioHeight = MaxHeight / (double)image.Height;

                double ratioImage = Math.Min(ratioHeight, ratioWidth);
                int h = (int)Math.Round(image.Height * ratioImage);
                int w = (int)Math.Round(image.Width * ratioImage);

                image = GfxUtil.ScaleImage(image, w, h);

                using (var ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    data = ms.ToArray();
                    return true;
                }
            }
            catch (Exception)
            {
                // Can't resize, return original
                return true;
            }
        }
    }
}
