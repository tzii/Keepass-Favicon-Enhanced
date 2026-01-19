using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace YetAnotherFaviconDownloader
{
    public sealed class FaviconDialog
    {
        private readonly IPluginHost pluginHost;
        private readonly BackgroundWorker bgWorker;
        private readonly IStatusLogger logger;

        private PwEntry[] entries;
        private string status;
        private int actualChanges = 0;

        private class ProgressInfo
        {
            public int Success;
            public int NotFound;
            public int Error;
            public int Skipped;
            public int Current;
            public int Remaining;

            private int _total;
            public int Total { get { return _total; } private set { _total = value; } }
            public float Percent
            {
                get { return ((Total - Remaining) * 100f) / Total; }
            }

            public ProgressInfo(int total)
            {
                Total = Remaining = total;
            }
        }

        public FaviconDialog(IPluginHost host)
        {
            // KeePass plugin host
            pluginHost = host;

            // Set up BackgroundWorker
            bgWorker = new BackgroundWorker();

            // BackgroundWorker Events
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;

            // Status Progress Form
            Form fStatusDialog;
            logger = StatusUtil.CreateStatusDialog(pluginHost.MainWindow, out fStatusDialog, "Yet Another Favicon Downloader", "Downloading favicons...", true, false);

            // Block UI
            pluginHost.MainWindow.UIBlockInteraction(true);
        }

        public void Run(PwEntry[] entries, bool customProvider)
        {
            this.entries = entries;
            bgWorker.RunWorkerAsync(customProvider);
        }

        /// <summary>
        /// Resolves KeePass placeholders in a URL string
        /// </summary>
        private string ResolvePlaceholders(PwEntry entry, string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // Check if URL contains placeholders
            if (!url.Contains("{"))
                return url;

            try
            {
                // Use KeePass's SprEngine to resolve placeholders
                SprContext ctx = new SprContext(entry, pluginHost.Database, SprCompileFlags.All);
                string resolved = SprEngine.Compile(url, ctx);
                
                if (resolved != url)
                {
                    Util.Log("Placeholder resolved: {0} => {1}", url, resolved);
                }
                
                return resolved;
            }
            catch (Exception ex)
            {
                Util.Log("Placeholder resolution failed: {0}", ex.Message);
                return url;
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Custom provider download (argument)
            bool customProvider = (bool)e.Argument;

            // Progress information
            ProgressInfo progress = new ProgressInfo(entries.Length);

            // Custom icons that will be added to the database
            PwCustomIcon[] icons = new PwCustomIcon[entries.Length];

            // Track which entries actually changed
            bool[] entryChanged = new bool[entries.Length];

            // Set up proxy information for all WebClients
            FaviconDownloader.Proxy = Util.GetKeePassProxy();

            // Get config options
            bool skipExisting = YetAnotherFaviconDownloaderExt.Config.GetSkipExistingIcons();
            bool useFallback = YetAnotherFaviconDownloaderExt.Config.GetUseFallbackProviders();
            string iconPrefix = YetAnotherFaviconDownloaderExt.Config.GetIconNamePrefix();

            using (ManualResetEvent waiter = new ManualResetEvent(false))
            {
                for (int j = 0; j < entries.Length; j++)
                {
                    PwEntry entry = entries[j];
                    ThreadPool.QueueUserWorkItem(delegate (object notUsed)
                    {
                        // Checks whether the user pressed the cancel button or the close button
                        if (!logger.ContinueWork())
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            int i = Interlocked.Increment(ref progress.Current) - 1;

                            // Check if we should skip entries with existing icons
                            if (skipExisting && !entry.CustomIconUuid.Equals(PwUuid.Zero))
                            {
                                Util.Log("Skipping entry with existing icon: {0}", entry.Strings.ReadSafe(PwDefs.TitleField));
                                Interlocked.Increment(ref progress.Skipped);
                                entries[i] = null;
                            }
                            else
                            {
                                // Fields
                                string url = entry.Strings.ReadSafe(PwDefs.UrlField);

                                // Resolve placeholders
                                url = ResolvePlaceholders(entry, url);

                                if (url == string.Empty)
                                {
                                    // If the user wants to use the title field, let's give it a try
                                    if (YetAnotherFaviconDownloaderExt.Config.GetUseTitleField())
                                    {
                                        url = entry.Strings.ReadSafe(PwDefs.TitleField);
                                        // Resolve placeholders in title too
                                        url = ResolvePlaceholders(entry, url);
                                        
                                        // When using title as URL, it often lacks the scheme (e.g. "google.com")
                                        // We should manually add it to ensure it passes validation
                                        if (!string.IsNullOrEmpty(url) && !url.Contains("://"))
                                        {
                                            url = "http://" + url;
                                        }
                                    }
                                }

                                // Empty URL field
                                if (url == string.Empty)
                                {
                                    // Can't find an icon
                                    Interlocked.Increment(ref progress.NotFound);
                                    entries[i] = null;
                                }
                                else
                                {
                                    Util.Log("Downloading: {0}", url);

                                    using (FaviconDownloader fd = new FaviconDownloader())
                                    {
                                        try
                                        {
                                            byte[] data = null;
                                            
                                            // Download favicon
                                            if (customProvider)
                                            {
                                                data = fd.GetIconCustomProvider(url);
                                            }
                                            else if (useFallback)
                                            {
                                                data = fd.GetIconWithFallback(url);
                                            }
                                            else
                                            {
                                                data = fd.GetIcon(url);
                                            }
                                            
                                            Util.Log("Icon downloaded with success");

                                            // Hash icon data (avoid duplicates)
                                            byte[] hash = Util.HashData(data);

                                            // Creates an icon only if your UUID does not exist
                                            PwUuid uuid = new PwUuid(hash);
                                            if (!pluginHost.Database.CustomIcons.Exists(delegate (PwCustomIcon x) { return x.Uuid.Equals(uuid); }))
                                            {
                                                // Add icon
                                                icons[i] = new PwCustomIcon(uuid, data);

                                                #region For KeePass 2.48+ only
                                                if (PwDefs.FileVersion64 >= 0x0002003000000000UL)
                                                {
                                                    var pwCustomIconType = icons[i].GetType();

                                                    // Name the icon with configurable prefix
                                                    var nameProperty = pwCustomIconType.GetProperty("Name");
                                                    if (nameProperty != null)
                                                    {
                                                        var host = fd.GetValidHost(url);
                                                        string iconName = string.IsNullOrEmpty(iconPrefix) ? host : iconPrefix + host;
                                                        nameProperty.SetValue(icons[i], iconName);
                                                    }

                                                    // Update last modification time
                                                    var lastModificationTimeProperty = pwCustomIconType.GetProperty("LastModificationTime");
                                                    if (lastModificationTimeProperty != null)
                                                    {
                                                        lastModificationTimeProperty.SetValue(icons[i], DateTime.UtcNow);
                                                    }
                                                }
                                                #endregion
                                            }

                                            // Check if icon is the same - this is the FIX for ghost modifications
                                            if (entry.CustomIconUuid.Equals(uuid))
                                            {
                                                // Icon is exactly the same, don't mark as changed
                                                Util.Log("Icon unchanged for entry");
                                                entries[i] = null;
                                                entryChanged[i] = false;
                                            }
                                            else
                                            {
                                                // Associate with this entry
                                                entry.CustomIconUuid = uuid;
                                                entryChanged[i] = true;
                                                Interlocked.Increment(ref actualChanges);
                                            }

                                            // Icon downloaded with success
                                            Interlocked.Increment(ref progress.Success);
                                        }
                                        catch (FaviconDownloaderException ex)
                                        {
                                            Util.Log("Failed to download favicon");

                                            if (ex.Status == FaviconDownloaderExceptionStatus.NotFound)
                                            {
                                                // Can't find an icon
                                                Interlocked.Increment(ref progress.NotFound);
                                            }
                                            else
                                            {
                                                // Some other error (network, etc)
                                                Interlocked.Increment(ref progress.Error);
                                            }

                                            // Avoid updating the entry
                                            entries[i] = null;
                                            entryChanged[i] = false;
                                        }
                                    }
                                }
                            }
                        }

                        // Notifies that all downloads are finished
                        if (Interlocked.Decrement(ref progress.Remaining) == 0)
                        {
                            waiter.Set();
                        }
                    });

                }

                // Wait until the downloads are finished
                int lastValue = 0;
                do
                {
                    // Update progress only when needed
                    if (lastValue != progress.Remaining)
                    {
                        ReportProgress(progress);
                        lastValue = progress.Remaining;
                    }
                } while (!waiter.WaitOne(100));
            }

            // Progress 100%
            ReportProgress(progress);
            
            // Build status message
            List<string> statusParts = new List<string>();
            statusParts.Add(string.Format("Success: {0}", progress.Success));
            if (progress.Skipped > 0)
                statusParts.Add(string.Format("Skipped: {0}", progress.Skipped));
            statusParts.Add(string.Format("Not Found: {0}", progress.NotFound));
            if (progress.Error > 0)
                statusParts.Add(string.Format("Error: {0}", progress.Error));
            
            status = "YAFD: " + string.Join(" / ", statusParts.ToArray()) + ".";

            // Prevents inserting duplicate icons
            MergeCustomIcons(icons);

            // Only mark database as needing icon update if there were actual changes
            if (actualChanges > 0)
            {
                pluginHost.Database.UINeedsIconUpdate = true;
            }

            // Waits long enough until we can see the output
            Thread.Sleep(3000);
        }

        private void ReportProgress(ProgressInfo progress)
        {
            logger.SetProgress((uint)progress.Percent);
            
            string statusText = string.Format("YAFD: Success: {0} / Skipped: {1} / Not Found: {2} / Error: {3} / Remaining: {4}", 
                progress.Success, progress.Skipped, progress.NotFound, progress.Error, progress.Remaining);
            
            logger.SetText(statusText, LogStatusType.Info);
        }

        private void MergeCustomIcons(PwCustomIcon[] icons)
        {
            List<PwCustomIcon> customIcons = pluginHost.Database.CustomIcons;

            // Removes duplicate downloaded icons
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] == null)
                {
                    continue;
                }

                for (int j = i + 1; j < icons.Length; j++)
                {
                    if (icons[j] == null)
                    {
                        continue;
                    }

                    if (icons[i].Uuid.Equals(icons[j].Uuid))
                    {
                        icons[j] = null;
                    }
                }
            }

            // Add all icons to the database
            customIcons.AddRange(icons);

            // Remove invalid entries
            customIcons.RemoveAll(delegate (PwCustomIcon x) { return x == null; });
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Util.Log("Cancelled");
            }
            else if (e.Error != null)
            {
                Util.Log("Error: {0}", e.Error.Message);
            }
            else
            {
                Util.Log("Done - {0} entries actually changed", actualChanges);
            }

            // Update entries (avoid cross-thread operation with other plugins)
            // Only touch entries that actually changed
            bool updateLastModified = YetAnotherFaviconDownloaderExt.Config.GetUpdateLastModified();
            foreach (PwEntry entry in entries)
            {
                // You can't touch this (oh-oh oh oh oh-oh-oh)
                if (entry == null) continue;

                // Save it - only if configured to update last modified
                entry.Touch(updateLastModified, false);
            }

            // Unblock UI
            pluginHost.MainWindow.UIBlockInteraction(false);

            // Refresh icons
            pluginHost.MainWindow.RefreshEntriesList();
            pluginHost.MainWindow.UpdateUI(false, null, false, null, false, null, true);

            logger.EndLogging();

            // Report how many icons have been downloaded
            pluginHost.MainWindow.SetStatusEx(status);

            // Auto-save if enabled and there were actual changes
            if (actualChanges > 0 && YetAnotherFaviconDownloaderExt.Config.GetAutoSaveDatabase())
            {
                try
                {
                    Util.Log("Auto-saving database...");
                    pluginHost.MainWindow.SaveDatabase(pluginHost.Database, null);
                }
                catch (Exception ex)
                {
                    Util.Log("Auto-save failed: {0}", ex.Message);
                }
            }
        }
    }
}
