using KeePass.Plugins;
using KeePass.Util;
using KeePassLib;
using KeePassLib.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace YetAnotherFaviconDownloader
{
    public sealed class YetAnotherFaviconDownloaderExt : Plugin
    {
        // Public RSA Key (4096 bits)
        private static readonly string UpdateKey =
            "<RSAKeyValue><Modulus>yF54V4nhAFE+N7nwcHmKMU3nd+4P7CEq0zpp8w2Wq+sKofN4mw" +
            "5xzC4y7MKj8KjJjlRZboBrwPs3Zgh1SrvJyPMqyHwCORciJj0ws254Ma8IYu4Fw8qMWurdIM" +
            "EEYQB3d5C9+l+9u31VVS1JNfdRsaOAN4kfYbOsAgkIMyun585hyIKdbqsQQDALwRbi8KIQ8i" +
            "AWTuiR1Iz5kf72u4C+Q6l6yNWTclEmvKkZcXH/doN/H1C4FzV6Kc4J3Se1xTYSDV5uhvk+g0" +
            "Hqm9gt9TIJVl31sMoMiQcjAArwnipU1KwB/SpoIUW1IQ53sQVJJdTLlOpu9FAdgjInziIug2" +
            "NcG2rwVQvr3/dbP80Aj1cGjhZgF3LO3hkr2gz/hEPUY0zHt817dWcga1nXvy6GdsotbDEQ+7" +
            "T7MGLgIWHfXZW+WcGfXgtbSPr+xHJXOMPoJ0ZSdHKyZU2m2WwX0NFJ7wc3xRyigLaFe9OZxe" +
            "TT1HzOfymtc9YJs0qw7wkDWdZZwSWPLhytEAG2SQAkVy/vp4jP8SqSDojeCCI/QGOxXPujBw" +
            "ZNlWGBunuSxuaCR/Vlx4vrlYr7lw7mFfQSjkSim7yUxoesJrYWWwjf/n6RBalOVy/REh4CTM" +
            "6wZMd7Ux9lXI89ml1tebjhAZ+GCk3QLS0wNxB9btbffDgWhAfHs7WKUk0=</Modulus><Exp" +
            "onent>AQAB</Exponent></RSAKeyValue>";

        public override string UpdateUrl
        {
            get { return "https://github.com/navossoc/KeePass-Yet-Another-Favicon-Downloader/raw/master/VERSION"; }
        }

        // Custom settings
        public static Configuration Config;

        // Plugin host interface
        private IPluginHost pluginHost;

        // Icon used by YAFD menus
        private Image menuImage;

        // Entry Context Menu
        private ToolStripSeparator entrySeparator;
        private ToolStripMenuItem entryDownloadFaviconsItem;
        private ToolStripMenuItem entryDownloadFaviconsCustomItem;

        // Group Context Menu
        private ToolStripSeparator groupSeparator;
        private ToolStripMenuItem groupDownloadFaviconsItem;
        private ToolStripMenuItem groupDownloadFaviconsCustomItem;

        // Tools Menu
        private ToolStripSeparator toolsMenuSeparator;
        private ToolStripMenuItem toolsMenuYAFD;

        // YAFD SubItems - Original
        private ToolStripMenuItem toolsSubItemsPrefixURLsItem;
        private ToolStripMenuItem toolsSubItemsTitleFieldItem;
        private ToolStripMenuItem toolsSubItemsUpdateModifiedItem;
        private ToolStripMenuItem toolsSubItemsMaximumIconSizeItems;
        private ToolStripMenuItem toolsSubItemsDownloadProviderItem;

        // YAFD SubItems - New
        private ToolStripMenuItem toolsSubItemsSkipExistingItem;
        private ToolStripMenuItem toolsSubItemsIconPrefixItem;
        private ToolStripMenuItem toolsSubItemsAllowInvalidCertsItem;
        private ToolStripMenuItem toolsSubItemsTimeoutItem;
        private ToolStripMenuItem toolsSubItemsAutoSaveItem;
        private ToolStripMenuItem toolsSubItemsFallbackProvidersItem;

        // YAFD Icon SubItems
        const int iconSizeMin = 16;
        const int iconSizeMax = 128;
        const int iconSizeIncr = 16;
        private List<ToolStripMenuItem> toolsMaxIconSizeSubItems;

        // YAFD Timeout SubItems
        private List<ToolStripMenuItem> toolsTimeoutSubItems;

        public override bool Initialize(IPluginHost host)
        {
            Util.Log("Plugin Initialize");

            Debug.Assert(host != null);
            if (host == null)
            {
                return false;
            }
            pluginHost = host;

            // Custom settings
            Config = new Configuration(pluginHost.CustomConfig);

            // Require a signed version file
            UpdateCheckEx.SetFileSigKey(UpdateUrl, UpdateKey);

            // Load menus icon resource
            menuImage = Properties.Resources.Download_32;

            // Add Entry Context menu items
            entrySeparator = new ToolStripSeparator();
            entryDownloadFaviconsItem = new ToolStripMenuItem("Download &Favicons", menuImage, DownloadFaviconsEntry_Click);
            entryDownloadFaviconsCustomItem = new ToolStripMenuItem("Download &Favicons (Custom)", menuImage, DownloadFaviconsCustomEntry_Click);
            pluginHost.MainWindow.EntryContextMenu.Items.Add(entrySeparator);
            pluginHost.MainWindow.EntryContextMenu.Items.Add(entryDownloadFaviconsItem);
            pluginHost.MainWindow.EntryContextMenu.Items.Add(entryDownloadFaviconsCustomItem);
            pluginHost.MainWindow.EntryContextMenu.Opening += EntryContextMenu_Opening;

            // Add Group Context menu items
            groupSeparator = new ToolStripSeparator();
            groupDownloadFaviconsItem = new ToolStripMenuItem("Download Fa&vicons (recursively)", menuImage, DownloadFaviconsGroup_Click);
            groupDownloadFaviconsCustomItem = new ToolStripMenuItem("Download Fa&vicons (Custom) (recursively)", menuImage, DownloadFaviconsCustomGroup_Click);
            pluginHost.MainWindow.GroupContextMenu.Items.Add(groupSeparator);
            pluginHost.MainWindow.GroupContextMenu.Items.Add(groupDownloadFaviconsItem);
            pluginHost.MainWindow.GroupContextMenu.Items.Add(groupDownloadFaviconsCustomItem);
            pluginHost.MainWindow.GroupContextMenu.Opening += GroupContextMenu_Opening;

            //////////////////////////////////////////////////////////////////////////

            // Tools -> YAFD -> SubItems

            // Automatic prefix URLs with http(s)://
            toolsSubItemsPrefixURLsItem = new ToolStripMenuItem("Automatic prefix URLs with http(s)://", null, PrefixURLsMenu_Click);
            toolsSubItemsPrefixURLsItem.Checked = Config.GetAutomaticPrefixURLs();

            // Use title field if URL field is empty
            toolsSubItemsTitleFieldItem = new ToolStripMenuItem("Use title field if URL field is empty", null, TitleFieldMenu_Click);
            toolsSubItemsTitleFieldItem.Checked = Config.GetUseTitleField();

            // Update last modified date when adding/updating icons
            toolsSubItemsUpdateModifiedItem = new ToolStripMenuItem("Update entry last modification time", null, LastModifiedMenu_Click);
            toolsSubItemsUpdateModifiedItem.Checked = Config.GetUpdateLastModified();

            // Skip entries that already have icons
            toolsSubItemsSkipExistingItem = new ToolStripMenuItem("Skip entries with existing icons", null, SkipExistingMenu_Click);
            toolsSubItemsSkipExistingItem.Checked = Config.GetSkipExistingIcons();

            // Use fallback providers when direct download fails
            toolsSubItemsFallbackProvidersItem = new ToolStripMenuItem("Use fallback providers (Google, DuckDuckGo, etc.)", null, FallbackProvidersMenu_Click);
            toolsSubItemsFallbackProvidersItem.Checked = Config.GetUseFallbackProviders();

            // Allow invalid/self-signed certificates
            toolsSubItemsAllowInvalidCertsItem = new ToolStripMenuItem("Allow self-signed SSL certificates", null, AllowInvalidCertsMenu_Click);
            toolsSubItemsAllowInvalidCertsItem.Checked = Config.GetAllowInvalidCertificates();

            // Auto-save database after download
            toolsSubItemsAutoSaveItem = new ToolStripMenuItem("Auto-save database after download", null, AutoSaveMenu_Click);
            toolsSubItemsAutoSaveItem.Checked = Config.GetAutoSaveDatabase();

            // Tools -> YAFD -> Maximum icon size -> SubItems
            toolsMaxIconSizeSubItems = new List<ToolStripMenuItem>();

            // 16x16 ~ 128x128
            for (int i = iconSizeMin; i <= iconSizeMax; i += iconSizeIncr)
            {
                var size = string.Format("{0}x{0} px", i);
                var item = new ToolStripMenuItem(size, null, MaximumIconSize_Click);
                toolsMaxIconSizeSubItems.Add(item);
            }

            int index = (Config.GetMaximumIconSize() / iconSizeIncr) - 1;
            if (index >= 0 && index < toolsMaxIconSizeSubItems.Count)
            {
                toolsMaxIconSizeSubItems[index].Checked = true;
            }

            toolsSubItemsMaximumIconSizeItems = new ToolStripMenuItem("Maximum icon size", (Image)pluginHost.Resources.GetObject("B16x16_Edit"), toolsMaxIconSizeSubItems.ToArray());

            // Tools -> YAFD -> Connection timeout -> SubItems
            toolsTimeoutSubItems = new List<ToolStripMenuItem>();
            int[] timeoutValues = new int[] { 5, 10, 15, 20, 30, 60 };
            int currentTimeout = Config.GetConnectionTimeout();
            
            foreach (int timeout in timeoutValues)
            {
                var timeoutText = string.Format("{0} seconds", timeout);
                var item = new ToolStripMenuItem(timeoutText, null, ConnectionTimeout_Click);
                item.Tag = timeout;
                item.Checked = (timeout == currentTimeout);
                toolsTimeoutSubItems.Add(item);
            }

            toolsSubItemsTimeoutItem = new ToolStripMenuItem("Connection timeout", (Image)pluginHost.Resources.GetObject("B16x16_History"), toolsTimeoutSubItems.ToArray());

            // Icon name prefix
            toolsSubItemsIconPrefixItem = new ToolStripMenuItem("Configure icon name prefix...", (Image)pluginHost.Resources.GetObject("B16x16_EditPaste"), IconPrefixMenu_Click);

            // Custom download provider
            toolsSubItemsDownloadProviderItem = new ToolStripMenuItem("Custom download provider...", (Image)pluginHost.Resources.GetObject("B16x16_WWW"), ToolsSubItemsDownloadProviderItem_Click);

            //

            // Add Tools menu items
            toolsMenuSeparator = new ToolStripSeparator();

            // Build menu with proper separators
            toolsMenuYAFD = new ToolStripMenuItem("Yet Another Favicon Downloader", menuImage);
            
            // Basic settings
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsPrefixURLsItem);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsTitleFieldItem);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsUpdateModifiedItem);
            toolsMenuYAFD.DropDownItems.Add(new ToolStripSeparator());
            
            // Enhanced settings
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsSkipExistingItem);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsFallbackProvidersItem);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsAllowInvalidCertsItem);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsAutoSaveItem);
            toolsMenuYAFD.DropDownItems.Add(new ToolStripSeparator());
            
            // Advanced settings
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsMaximumIconSizeItems);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsTimeoutItem);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsIconPrefixItem);
            toolsMenuYAFD.DropDownItems.Add(toolsSubItemsDownloadProviderItem);
            
#if DEBUG
            toolsMenuYAFD.DropDownItems.Add(new ToolStripSeparator());
            toolsMenuYAFD.DropDownItems.Add(new ToolStripMenuItem("Reset Icons", null, ResetIconsMenu_Click));
#endif

            pluginHost.MainWindow.ToolsMenu.DropDownItems.Add(toolsMenuSeparator);
            pluginHost.MainWindow.ToolsMenu.DropDownItems.Add(toolsMenuYAFD);

            return true;
        }

        private void ToolsSubItemsDownloadProviderItem_Click(object sender, EventArgs e)
        {
            var form = new UI.DownloadProvider(pluginHost);
            form.ShowDialog(pluginHost.MainWindow);
        }

        private void EntryContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var provider = Config.GetCustomDownloadProvider();
            bool visible = provider != string.Empty;
            entryDownloadFaviconsCustomItem.Visible = visible;
        }

        private void GroupContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var provider = Config.GetCustomDownloadProvider();
            bool visible = provider != string.Empty;
            groupDownloadFaviconsCustomItem.Visible = visible;
        }

        public override void Terminate()
        {
            Util.Log("Plugin Terminate");

            // This should never happen but better safe than sorry
            Debug.Assert(pluginHost != null);
            if (pluginHost == null)
            {
                return;
            }

            // Dispose resources
            if (menuImage != null)
            {
                menuImage.Dispose();
            }

            // Remove Entry Context menu items
            pluginHost.MainWindow.EntryContextMenu.Items.Remove(entrySeparator);
            pluginHost.MainWindow.EntryContextMenu.Items.Remove(entryDownloadFaviconsItem);
            pluginHost.MainWindow.EntryContextMenu.Items.Remove(entryDownloadFaviconsCustomItem);

            // Remove Group Context menu items
            pluginHost.MainWindow.GroupContextMenu.Items.Remove(groupSeparator);
            pluginHost.MainWindow.GroupContextMenu.Items.Remove(groupDownloadFaviconsItem);
            pluginHost.MainWindow.GroupContextMenu.Items.Remove(groupDownloadFaviconsCustomItem);

#if DEBUG
            // Remove Tools menu items
            pluginHost.MainWindow.ToolsMenu.DropDownItems.Remove(toolsMenuSeparator);
            pluginHost.MainWindow.ToolsMenu.DropDownItems.Remove(toolsMenuYAFD);
#endif
        }

        #region MenuItem EventHandler
        private void downloadFaviconsCustomEntry_Click(object sender, EventArgs e, bool customProvider)
        {
            Util.Log("Entry Context Menu -> Download Favicons clicked");

            PwEntry[] entries = pluginHost.MainWindow.GetSelectedEntries();
            DownloadFavicons(entries, customProvider);
        }

        private void downloadFaviconsGroup_Click(object sender, EventArgs e, bool customProvider)
        {
            Util.Log("Group Context Menu -> Download Favicons clicked");

            PwGroup group = pluginHost.MainWindow.GetSelectedGroup();
            if (group == null)
            {
                Util.Log("No group selected");
                return;
            }

            // Get all entries from the group
            PwObjectList<PwEntry> entriesInGroup = group.GetEntries(true);
            if (entriesInGroup == null || entriesInGroup.UCount == 0)
            {
                Util.Log("No entries in group");
                return;
            }

            // Copy PwObjectList<PwEntry> to PwEntry[]
            PwEntry[] entries = entriesInGroup.CloneShallowToList().ToArray();
            DownloadFavicons(entries, customProvider);
        }

        private void DownloadFaviconsEntry_Click(object sender, EventArgs e)
        {
            downloadFaviconsCustomEntry_Click(sender, e, false);
        }

        private void DownloadFaviconsGroup_Click(object sender, EventArgs e)
        {
            downloadFaviconsGroup_Click(sender, e, false);
        }

        private void DownloadFaviconsCustomEntry_Click(object sender, EventArgs e)
        {
            downloadFaviconsCustomEntry_Click(sender, e, true);
        }

        private void DownloadFaviconsCustomGroup_Click(object sender, EventArgs e)
        {
            downloadFaviconsGroup_Click(sender, e, true);
        }

        private void PrefixURLsMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            menu.Checked = !menu.Checked;

            Config.SetAutomaticPrefixURLs(menu.Checked);
        }

        private void TitleFieldMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            menu.Checked = !menu.Checked;

            Config.SetUseTitleField(menu.Checked);
        }

        private void LastModifiedMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            menu.Checked = !menu.Checked;

            Config.SetUpdateLastModified(menu.Checked);
        }

        private void SkipExistingMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            menu.Checked = !menu.Checked;

            Config.SetSkipExistingIcons(menu.Checked);
        }

        private void FallbackProvidersMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            menu.Checked = !menu.Checked;

            Config.SetUseFallbackProviders(menu.Checked);
        }

        private void AllowInvalidCertsMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            menu.Checked = !menu.Checked;

            Config.SetAllowInvalidCertificates(menu.Checked);

            // Show warning about security implications
            if (menu.Checked)
            {
                MessageBox.Show(
                    "Warning: Allowing self-signed certificates reduces security.\n\n" +
                    "Only enable this if you need to download favicons from internal servers with self-signed certificates.",
                    "Security Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void AutoSaveMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            menu.Checked = !menu.Checked;

            Config.SetAutoSaveDatabase(menu.Checked);
        }

        private void MaximumIconSize_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            for (int i = 0; i < toolsMaxIconSizeSubItems.Count; i++)
            {
                toolsMaxIconSizeSubItems[i].Checked = false;

                if (menu == toolsMaxIconSizeSubItems[i])
                {
                    menu.Checked = true;
                    Config.SetMaximumIconSize((i + 1) * iconSizeIncr);
                }
            }
        }

        private void ConnectionTimeout_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;

            foreach (var item in toolsTimeoutSubItems)
            {
                item.Checked = false;
            }

            menu.Checked = true;
            int timeout = (int)menu.Tag;
            Config.SetConnectionTimeout(timeout);
        }

        private void IconPrefixMenu_Click(object sender, EventArgs e)
        {
            string currentPrefix = Config.GetIconNamePrefix();
            
            using (var dialog = new Form())
            {
                dialog.Text = "Icon Name Prefix";
                dialog.Width = 350;
                dialog.Height = 150;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var label = new Label() { Left = 20, Top = 20, Text = "Enter prefix for icon names (leave empty for no prefix):", AutoSize = true };
                var textBox = new TextBox() { Left = 20, Top = 45, Width = 290, Text = currentPrefix };
                var btnOk = new Button() { Text = "OK", Left = 150, Top = 80, Width = 75, DialogResult = DialogResult.OK };
                var btnCancel = new Button() { Text = "Cancel", Left = 235, Top = 80, Width = 75, DialogResult = DialogResult.Cancel };

                dialog.Controls.Add(label);
                dialog.Controls.Add(textBox);
                dialog.Controls.Add(btnOk);
                dialog.Controls.Add(btnCancel);
                dialog.AcceptButton = btnOk;
                dialog.CancelButton = btnCancel;

                if (dialog.ShowDialog(pluginHost.MainWindow) == DialogResult.OK)
                {
                    Config.SetIconNamePrefix(textBox.Text);
                }
            }
        }

#if DEBUG
        private void ResetIconsMenu_Click(object sender, EventArgs e)
        {
            Util.Log("Tools Menu -> Reset Icons clicked");

            // Checks if there is an open database
            if (!pluginHost.Database.IsOpen)
            {
                Util.Log("Database not open");
                return;
            }

            // Reset icons from all groups
            PwObjectList<PwGroup> groups = pluginHost.Database.RootGroup.GetGroups(true);
            foreach (PwGroup group in groups)
            {
                //  Recycle bin
                if (group.Uuid.Equals(pluginHost.Database.RecycleBinUuid))
                {
                    group.IconId = PwIcon.TrashBin;
                }
                else
                {
                    group.IconId = PwIcon.Folder;
                }
                group.CustomIconUuid = PwUuid.Zero;
                group.Touch(true, false);
            }

            // Reset icons from all entries
            PwObjectList<PwEntry> entries = pluginHost.Database.RootGroup.GetEntries(true);
            foreach (PwEntry entry in entries)
            {
                entry.IconId = PwIcon.Key;
                entry.CustomIconUuid = PwUuid.Zero;
                entry.Touch(true, false);
            }

            // Remove all custom icons from database
            pluginHost.Database.CustomIcons.Clear();

            // Refresh icons
            pluginHost.MainWindow.UpdateUI(false, null, true, null, true, null, true);
        }
#endif
        #endregion

        private void DownloadFavicons(PwEntry[] entries, bool customProvider)
        {
            if (entries == null || entries.Length == 0)
            {
                Util.Log("No entries selected");
                return;
            }

            // Run all the work in a new thread
            FaviconDialog downloader = new FaviconDialog(pluginHost);
            downloader.Run(entries, customProvider);
        }
    }
}
