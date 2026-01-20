using KeePass.App.Configuration;

namespace YetAnotherFaviconDownloader
{
    public sealed class Configuration
    {
        private AceCustomConfig config;

        /// <summary>
        /// Plugin name used on settings to avoid collisions
        /// </summary>
        private const string pluginName = "YetAnotherFaviconDownloader.";

        /// <summary>
        /// Automatic prefix URLs with http(s):// setting (https first, then http)
        /// </summary>
        private const string automaticPrefixURLs = pluginName + "PrefixURLs";
        private bool? m_automaticPrefixURLs = null;

        /// <summary>
        /// Use title field if URL field is empty setting
        /// </summary>
        private const string useTitleField = pluginName + "TitleField";
        private bool? m_useTitleField = null;

        /// <summary>
        /// Update last modified date when adding/updating icons
        /// </summary>
        private const string updateLastModified = pluginName + "UpdateLastModified";
        private bool? m_updateLastModified = null;

        /// <summary>
        /// Maximum icon size
        /// </summary>
        private const string maximumIconSize = pluginName + "MaximumIconSize";
        private int? m_maximumIconSize = null;

        /// <summary>
        /// Custom download provider
        /// </summary>
        private const string customDownloadProvider = pluginName + "CustomDownloadProvider";
        private string m_customDownloadProvider = null;

        /// <summary>
        /// Skip entries that already have a custom icon
        /// </summary>
        private const string skipExistingIcons = pluginName + "SkipExistingIcons";
        private bool? m_skipExistingIcons = null;

        /// <summary>
        /// Custom icon name prefix (default: "yafd-", can be empty)
        /// </summary>
        private const string iconNamePrefix = pluginName + "IconNamePrefix";
        private string m_iconNamePrefix = null;
        private bool m_iconNamePrefixLoaded = false;

        /// <summary>
        /// Allow self-signed/invalid SSL certificates
        /// </summary>
        private const string allowInvalidCertificates = pluginName + "AllowInvalidCertificates";
        private bool? m_allowInvalidCertificates = null;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        private const string connectionTimeout = pluginName + "ConnectionTimeout";
        private int? m_connectionTimeout = null;

        /// <summary>
        /// Auto-save database after downloading favicons
        /// </summary>
        private const string autoSaveDatabase = pluginName + "AutoSaveDatabase";
        private bool? m_autoSaveDatabase = null;

        /// <summary>
        /// Use fallback providers when direct download fails
        /// </summary>
        private const string useFallbackProviders = pluginName + "UseFallbackProviders";
        private bool? m_useFallbackProviders = null;

        public Configuration(AceCustomConfig aceCustomConfig)
        {
            config = aceCustomConfig;
        }

        #region Original Settings

        public bool GetAutomaticPrefixURLs()
        {
            if (!m_automaticPrefixURLs.HasValue)
            {
                m_automaticPrefixURLs = config.GetBool(automaticPrefixURLs, false);
            }

            return m_automaticPrefixURLs.Value;
        }

        public void SetAutomaticPrefixURLs(bool value)
        {
            m_automaticPrefixURLs = value;
            config.SetBool(automaticPrefixURLs, value);
        }

        public bool GetUseTitleField()
        {
            if (!m_useTitleField.HasValue)
            {
                m_useTitleField = config.GetBool(useTitleField, false);
            }

            return m_useTitleField.Value;
        }

        public void SetUseTitleField(bool value)
        {
            m_useTitleField = value;
            config.SetBool(useTitleField, value);
        }

        public bool GetUpdateLastModified()
        {
            if (!m_updateLastModified.HasValue)
            {
                m_updateLastModified = config.GetBool(updateLastModified, true);
            }

            return m_updateLastModified.Value;
        }

        public void SetUpdateLastModified(bool value)
        {
            m_updateLastModified = value;
            config.SetBool(updateLastModified, value);
        }

        public int GetMaximumIconSize()
        {
            if (!m_maximumIconSize.HasValue)
            {
                // int is enough
                m_maximumIconSize = (int)config.GetLong(maximumIconSize, 128);
            }

            return m_maximumIconSize.Value;
        }

        public void SetMaximumIconSize(int value)
        {
            m_maximumIconSize = value;
            config.SetLong(maximumIconSize, value);
        }

        public string GetCustomDownloadProvider()
        {
            if (string.IsNullOrEmpty(m_customDownloadProvider))
            {
                m_customDownloadProvider = config.GetString(customDownloadProvider, "");
            }

            return m_customDownloadProvider;
        }

        public void SetCustomDownloadProvider(string value)
        {
            m_customDownloadProvider = value;
            config.SetString(customDownloadProvider, value);
        }

        #endregion

        #region New Settings

        public bool GetSkipExistingIcons()
        {
            if (!m_skipExistingIcons.HasValue)
            {
                m_skipExistingIcons = config.GetBool(skipExistingIcons, false);
            }

            return m_skipExistingIcons.Value;
        }

        public void SetSkipExistingIcons(bool value)
        {
            m_skipExistingIcons = value;
            config.SetBool(skipExistingIcons, value);
        }

        public string GetIconNamePrefix()
        {
            if (!m_iconNamePrefixLoaded)
            {
                m_iconNamePrefix = config.GetString(iconNamePrefix, "yafd-");
                m_iconNamePrefixLoaded = true;
            }

            return m_iconNamePrefix;
        }

        public void SetIconNamePrefix(string value)
        {
            m_iconNamePrefix = value ?? "";
            m_iconNamePrefixLoaded = true;
            config.SetString(iconNamePrefix, m_iconNamePrefix);
        }

        public bool GetAllowInvalidCertificates()
        {
            if (!m_allowInvalidCertificates.HasValue)
            {
                m_allowInvalidCertificates = config.GetBool(allowInvalidCertificates, false);
            }

            return m_allowInvalidCertificates.Value;
        }

        public void SetAllowInvalidCertificates(bool value)
        {
            m_allowInvalidCertificates = value;
            config.SetBool(allowInvalidCertificates, value);
        }

        public int GetConnectionTimeout()
        {
            if (!m_connectionTimeout.HasValue)
            {
                // Default 15 seconds, was 20 before
                m_connectionTimeout = (int)config.GetLong(connectionTimeout, 15);
            }

            return m_connectionTimeout.Value;
        }

        public void SetConnectionTimeout(int value)
        {
            // Clamp between 5 and 120 seconds
            if (value < 5) value = 5;
            if (value > 120) value = 120;
            m_connectionTimeout = value;
            config.SetLong(connectionTimeout, value);
        }

        public bool GetAutoSaveDatabase()
        {
            if (!m_autoSaveDatabase.HasValue)
            {
                m_autoSaveDatabase = config.GetBool(autoSaveDatabase, false);
            }

            return m_autoSaveDatabase.Value;
        }

        public void SetAutoSaveDatabase(bool value)
        {
            m_autoSaveDatabase = value;
            config.SetBool(autoSaveDatabase, value);
        }

        public bool GetUseFallbackProviders()
        {
            if (!m_useFallbackProviders.HasValue)
            {
                m_useFallbackProviders = config.GetBool(useFallbackProviders, true);
            }

            return m_useFallbackProviders.Value;
        }

        public void SetUseFallbackProviders(bool value)
        {
            m_useFallbackProviders = value;
            config.SetBool(useFallbackProviders, value);
        }

        #endregion
    }
}
