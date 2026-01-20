using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using YetAnotherFaviconDownloader;

namespace Tests
{
    [TestClass]
    public class FaviconDownloaderTest
    {
        #region Android URL Conversion Tests

        [TestMethod]
        public void TestAndroidUrl_KnownMapping_ReturnsCorrectDomain()
        {
            // Test known mappings from AndroidPackageMapping
            Assert.AreEqual("facebook.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.facebook.katana"));
            Assert.AreEqual("instagram.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.instagram.android"));
            Assert.AreEqual("twitter.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.twitter.android"));
            Assert.AreEqual("reddit.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.reddit.frontpage"));
            Assert.AreEqual("spotify.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.spotify.music"));
            Assert.AreEqual("netflix.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.netflix.mediaclient"));
            Assert.AreEqual("amazon.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.amazon.mShop.android.shopping"));
        }

        [TestMethod]
        public void TestAndroidUrl_UnknownPackage_DerivesDomain()
        {
            // Test heuristic derivation for unknown packages
            Assert.AreEqual("example.com", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://com.example.app"));
            Assert.AreEqual("testcompany.org", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://org.testcompany.app"));
            Assert.AreEqual("myapp.net", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://net.myapp.client"));
            Assert.AreEqual("coolapp.io", FaviconDownloader.ConvertAndroidUrlToDomain("androidapp://io.coolapp.mobile"));
        }

        [TestMethod]
        public void TestAndroidUrl_InvalidUrl_ReturnsNull()
        {
            // Test invalid inputs
            Assert.IsNull(FaviconDownloader.ConvertAndroidUrlToDomain(null));
            Assert.IsNull(FaviconDownloader.ConvertAndroidUrlToDomain(""));
            Assert.IsNull(FaviconDownloader.ConvertAndroidUrlToDomain("https://example.com"));
            Assert.IsNull(FaviconDownloader.ConvertAndroidUrlToDomain("not-an-android-url"));
        }

        [TestMethod]
        public void TestIsAndroidUrl_ValidUrls_ReturnsTrue()
        {
            Assert.IsTrue(FaviconDownloader.IsAndroidUrl("androidapp://com.example.app"));
            Assert.IsTrue(FaviconDownloader.IsAndroidUrl("androidapp://com.facebook.katana"));
            Assert.IsTrue(FaviconDownloader.IsAndroidUrl("androidapp://org.telegram.messenger"));
        }

        [TestMethod]
        public void TestIsAndroidUrl_InvalidUrls_ReturnsFalse()
        {
            Assert.IsFalse(FaviconDownloader.IsAndroidUrl(null));
            Assert.IsFalse(FaviconDownloader.IsAndroidUrl(""));
            Assert.IsFalse(FaviconDownloader.IsAndroidUrl("https://example.com"));
            Assert.IsFalse(FaviconDownloader.IsAndroidUrl("http://facebook.com"));
            Assert.IsFalse(FaviconDownloader.IsAndroidUrl("ftp://files.example.com"));
        }

        #endregion

        #region AndroidPackageMapping Tests

        [TestMethod]
        public void TestAndroidMapping_GetDomain_KnownPackages()
        {
            // Social Media
            Assert.AreEqual("facebook.com", AndroidPackageMapping.GetDomain("com.facebook.katana"));
            Assert.AreEqual("messenger.com", AndroidPackageMapping.GetDomain("com.facebook.orca"));
            Assert.AreEqual("whatsapp.com", AndroidPackageMapping.GetDomain("com.whatsapp"));
            Assert.AreEqual("telegram.org", AndroidPackageMapping.GetDomain("org.telegram.messenger"));
            
            // Google Apps
            Assert.AreEqual("gmail.com", AndroidPackageMapping.GetDomain("com.google.android.gm"));
            Assert.AreEqual("youtube.com", AndroidPackageMapping.GetDomain("com.google.android.youtube"));
            Assert.AreEqual("maps.google.com", AndroidPackageMapping.GetDomain("com.google.android.apps.maps"));
            
            // Microsoft Apps
            Assert.AreEqual("outlook.com", AndroidPackageMapping.GetDomain("com.microsoft.office.outlook"));
            Assert.AreEqual("teams.microsoft.com", AndroidPackageMapping.GetDomain("com.microsoft.teams"));
            
            // Finance
            Assert.AreEqual("paypal.com", AndroidPackageMapping.GetDomain("com.paypal.android.p2pmobile"));
            Assert.AreEqual("coinbase.com", AndroidPackageMapping.GetDomain("com.coinbase.android"));
        }

        [TestMethod]
        public void TestAndroidMapping_GetDomain_UnknownPackage_ReturnsNull()
        {
            Assert.IsNull(AndroidPackageMapping.GetDomain("com.unknown.package.name"));
            Assert.IsNull(AndroidPackageMapping.GetDomain("totally.fake.app"));
            Assert.IsNull(AndroidPackageMapping.GetDomain(null));
            Assert.IsNull(AndroidPackageMapping.GetDomain(""));
        }

        [TestMethod]
        public void TestAndroidMapping_DeriveDomain_CommonPatterns()
        {
            // com.company.app -> company.com
            Assert.AreEqual("testapp.com", AndroidPackageMapping.DeriveDomain("com.testapp.mobile"));
            
            // org.company.app -> company.org
            Assert.AreEqual("mozilla.org", AndroidPackageMapping.DeriveDomain("org.mozilla.someapp"));
            
            // net.company.app -> company.net
            Assert.AreEqual("someservice.net", AndroidPackageMapping.DeriveDomain("net.someservice.client"));
            
            // io.company.app -> company.io
            Assert.AreEqual("startup.io", AndroidPackageMapping.DeriveDomain("io.startup.app"));
        }

        [TestMethod]
        public void TestAndroidMapping_DeriveDomain_CountryCodePatterns()
        {
            // uk.co.company.app -> company.co.uk
            Assert.AreEqual("bbc.co.uk", AndroidPackageMapping.DeriveDomain("uk.co.bbc.iplayer"));
            
            // de.company.app -> company.de
            Assert.AreEqual("german.de", AndroidPackageMapping.DeriveDomain("de.german.app"));
        }

        [TestMethod]
        public void TestAndroidMapping_DeriveDomain_InvalidInput_ReturnsNull()
        {
            Assert.IsNull(AndroidPackageMapping.DeriveDomain(null));
            Assert.IsNull(AndroidPackageMapping.DeriveDomain(""));
            Assert.IsNull(AndroidPackageMapping.DeriveDomain("single"));
        }

        [TestMethod]
        public void TestAndroidMapping_HasMapping_ReturnsCorrectly()
        {
            Assert.IsTrue(AndroidPackageMapping.HasMapping("com.facebook.katana"));
            Assert.IsTrue(AndroidPackageMapping.HasMapping("com.google.android.gm"));
            Assert.IsFalse(AndroidPackageMapping.HasMapping("com.unknown.app"));
            Assert.IsFalse(AndroidPackageMapping.HasMapping(null));
        }

        [TestMethod]
        public void TestAndroidMapping_Count_HasManyMappings()
        {
            // Verify we have a substantial number of mappings (should be 300+)
            Assert.IsTrue(AndroidPackageMapping.Count >= 300, 
                $"Expected at least 300 mappings, but found {AndroidPackageMapping.Count}");
        }

        [TestMethod]
        public void TestAndroidMapping_CaseInsensitive()
        {
            // Mappings should be case-insensitive
            Assert.AreEqual("facebook.com", AndroidPackageMapping.GetDomain("COM.FACEBOOK.KATANA"));
            Assert.AreEqual("facebook.com", AndroidPackageMapping.GetDomain("Com.Facebook.Katana"));
        }

        #endregion

        #region GetValidHost Tests

        [TestMethod]
        public void TestGetValidHost_StandardUrls()
        {
            using (FaviconDownloader fd = new FaviconDownloader())
            {
                Assert.AreEqual("example.com", fd.GetValidHost("https://example.com"));
                Assert.AreEqual("example.com", fd.GetValidHost("http://example.com"));
                Assert.AreEqual("example.com", fd.GetValidHost("https://example.com/path/to/page"));
                Assert.AreEqual("subdomain.example.com", fd.GetValidHost("https://subdomain.example.com"));
            }
        }

        [TestMethod]
        public void TestGetValidHost_WithoutScheme()
        {
            using (FaviconDownloader fd = new FaviconDownloader())
            {
                Assert.AreEqual("example.com", fd.GetValidHost("example.com"));
                Assert.AreEqual("example.com", fd.GetValidHost("example.com/path"));
            }
        }

        [TestMethod]
        public void TestGetValidHost_AndroidUrls()
        {
            using (FaviconDownloader fd = new FaviconDownloader())
            {
                Assert.AreEqual("facebook.com", fd.GetValidHost("androidapp://com.facebook.katana"));
                Assert.AreEqual("spotify.com", fd.GetValidHost("androidapp://com.spotify.music"));
            }
        }

        [TestMethod]
        public void TestGetValidHost_InvalidUrls()
        {
            using (FaviconDownloader fd = new FaviconDownloader())
            {
                Assert.AreEqual("", fd.GetValidHost(null));
                Assert.AreEqual("", fd.GetValidHost(""));
            }
        }

        #endregion
    }
}
