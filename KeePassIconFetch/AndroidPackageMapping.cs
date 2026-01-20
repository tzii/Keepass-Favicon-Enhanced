using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace YetAnotherFaviconDownloader
{
    /// <summary>
    /// Comprehensive mapping of Android package names to web domains
    /// Used for converting androidapp:// URLs to fetchable web domains
    /// 
    /// Mappings are loaded from an embedded JSON resource file for easy updates
    /// without recompilation.
    /// </summary>
    public static class AndroidPackageMapping
    {
        private static readonly Dictionary<string, string> Mappings;
        private static readonly object LoadLock = new object();
        private static bool _isLoaded = false;

        static AndroidPackageMapping()
        {
            Mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            LoadMappings();
        }

        /// <summary>
        /// Load mappings from embedded JSON resource
        /// </summary>
        private static void LoadMappings()
        {
            lock (LoadLock)
            {
                if (_isLoaded) return;

                try
                {
                    // Try to load from embedded resource
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "YetAnotherFaviconDownloader.Resources.android-mappings.json";
                    
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string json = reader.ReadToEnd();
                                ParseJsonMappings(json);
                                Util.Log("Loaded {0} Android package mappings from embedded resource", Mappings.Count);
                            }
                        }
                        else
                        {
                            Util.Log("Android mappings resource not found, using fallback mappings");
                            LoadFallbackMappings();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.Log("Failed to load Android mappings from resource: {0}", ex.Message);
                    LoadFallbackMappings();
                }

                _isLoaded = true;
            }
        }

        /// <summary>
        /// Simple JSON parser for the mappings object (no external dependencies)
        /// </summary>
        private static void ParseJsonMappings(string json)
        {
            try
            {
                // Find the "mappings" object
                var mappingsMatch = Regex.Match(json, @"""mappings""\s*:\s*\{([^}]+(?:\{[^}]*\}[^}]*)*)\}", RegexOptions.Singleline);
                if (!mappingsMatch.Success)
                {
                    Util.Log("Could not find mappings object in JSON");
                    LoadFallbackMappings();
                    return;
                }

                string mappingsContent = mappingsMatch.Groups[1].Value;
                
                // Parse key-value pairs: "package.name": "domain.com"
                var pairPattern = new Regex(@"""([^""]+)""\s*:\s*""([^""]+)""", RegexOptions.Compiled);
                
                foreach (Match match in pairPattern.Matches(mappingsContent))
                {
                    string packageName = match.Groups[1].Value;
                    string domain = match.Groups[2].Value;
                    
                    if (!Mappings.ContainsKey(packageName))
                    {
                        Mappings[packageName] = domain;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Log("Failed to parse JSON mappings: {0}", ex.Message);
                LoadFallbackMappings();
            }
        }

        /// <summary>
        /// Fallback mappings for the most common apps (used if JSON fails to load)
        /// </summary>
        private static void LoadFallbackMappings()
        {
            // Only add fallback if mappings are empty
            if (Mappings.Count > 0) return;

            // Social Media (most common)
            Mappings["com.facebook.katana"] = "facebook.com";
            Mappings["com.facebook.orca"] = "messenger.com";
            Mappings["com.instagram.android"] = "instagram.com";
            Mappings["com.twitter.android"] = "twitter.com";
            Mappings["com.whatsapp"] = "whatsapp.com";
            Mappings["org.telegram.messenger"] = "telegram.org";
            Mappings["com.snapchat.android"] = "snapchat.com";
            Mappings["com.linkedin.android"] = "linkedin.com";
            Mappings["com.reddit.frontpage"] = "reddit.com";
            Mappings["com.discord"] = "discord.com";
            
            // Google Apps
            Mappings["com.google.android.gm"] = "gmail.com";
            Mappings["com.google.android.youtube"] = "youtube.com";
            Mappings["com.google.android.apps.maps"] = "maps.google.com";
            Mappings["com.google.android.apps.photos"] = "photos.google.com";
            Mappings["com.android.chrome"] = "google.com/chrome";
            
            // Microsoft Apps
            Mappings["com.microsoft.office.outlook"] = "outlook.com";
            Mappings["com.microsoft.teams"] = "teams.microsoft.com";
            Mappings["com.microsoft.onedrive"] = "onedrive.com";
            
            // Entertainment
            Mappings["com.netflix.mediaclient"] = "netflix.com";
            Mappings["com.spotify.music"] = "spotify.com";
            Mappings["com.amazon.avod.thirdpartyclient"] = "primevideo.com";
            Mappings["com.disney.disneyplus"] = "disneyplus.com";
            
            // Shopping
            Mappings["com.amazon.mShop.android.shopping"] = "amazon.com";
            Mappings["com.ebay.mobile"] = "ebay.com";
            
            // Finance
            Mappings["com.paypal.android.p2pmobile"] = "paypal.com";
            Mappings["com.venmo"] = "venmo.com";
            Mappings["com.coinbase.android"] = "coinbase.com";
            
            // Travel
            Mappings["com.ubercab"] = "uber.com";
            Mappings["com.airbnb.android"] = "airbnb.com";
            
            Util.Log("Loaded {0} fallback Android package mappings", Mappings.Count);
        }

        /// <summary>
        /// Get the domain for an Android package name
        /// </summary>
        /// <param name="packageName">The Android package name (e.g., "com.reddit.frontpage")</param>
        /// <returns>The domain if found, or null if not found</returns>
        public static string GetDomain(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return null;

            string domain;
            if (Mappings.TryGetValue(packageName, out domain))
            {
                return domain;
            }

            return null;
        }

        /// <summary>
        /// Try to derive a domain from an unknown package name using heuristics
        /// </summary>
        public static string DeriveDomain(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return null;

            try
            {
                // First, check if we have a known mapping
                string known = GetDomain(packageName);
                if (known != null)
                    return known;

                // Try to derive from package name
                // Pattern: com.company.app -> company.com
                // Pattern: org.company.app -> company.org
                // Pattern: net.company.app -> company.net
                // Pattern: io.company.app -> company.io

                string[] parts = packageName.Split('.');
                if (parts == null || parts.Length < 2)
                    return null;

                string tld = parts[0].ToLowerInvariant();
                string company = parts[1].ToLowerInvariant();
                
                // Validate company name is not empty or just whitespace
                if (string.IsNullOrEmpty(company) || company.Trim().Length == 0)
                    return null;

                // Common TLDs at start of package name
                switch (tld)
                {
                    case "com":
                    case "org":
                    case "net":
                    case "io":
                    case "co":
                    case "me":
                    case "tv":
                    case "app":
                        // Try company.tld first, then company.com as fallback
                        if (tld == "com" || tld == "org" || tld == "net" || tld == "io")
                        {
                            return company + "." + tld;
                        }
                        return company + ".com";

                    default:
                        // Might be a country code like uk.co.company or de.company
                        if (parts.Length >= 3)
                        {
                            // uk.co.company.app -> company.co.uk
                            if (parts[1] == "co" || parts[1] == "com")
                            {
                                return parts[2] + "." + parts[1] + "." + tld;
                            }
                            // de.company.app -> company.de
                            return parts[1] + "." + tld;
                        }
                        break;
                }
            }
            catch (Exception)
            {
                // If any string manipulation fails, just return null
                return null;
            }

            return null;
        }

        /// <summary>
        /// Check if a package name has a known mapping
        /// </summary>
        public static bool HasMapping(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return false;

            return Mappings.ContainsKey(packageName);
        }

        /// <summary>
        /// Get the total number of known mappings
        /// </summary>
        public static int Count
        {
            get { return Mappings.Count; }
        }

        /// <summary>
        /// Reload mappings (useful if external file was updated)
        /// </summary>
        public static void Reload()
        {
            lock (LoadLock)
            {
                Mappings.Clear();
                _isLoaded = false;
                LoadMappings();
            }
        }
    }
}
