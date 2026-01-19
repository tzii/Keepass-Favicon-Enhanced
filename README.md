# Yet Another Favicon Downloader - Enhanced

[![GitHub release](https://img.shields.io/github/release/tzii/Keepass-Favicon-Enhanced.svg)](https://github.com/tzii/Keepass-Favicon-Enhanced/releases/latest)
[![license](https://img.shields.io/github/license/tzii/Keepass-Favicon-Enhanced.svg)](/LICENSE)

## Table of contents

- [Overview](#overview)
  - [Features](#features)
  - [What's New](#whats-new)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Android URL Support](#android-url-support)
- [Changelog](#changelog)
- [Contributing](#contributing)
- [Copyright and license](#copyright-and-license)

## Overview

_Yet Another Favicon Downloader Enhanced_ is a significantly improved fork of the original YAFD plugin for _KeePass_ 2.x. It allows you to quickly download favicons for your password entries with enhanced reliability, more features, and better icon detection.

### Features

- **Concurrent and responsive** - Allows faster downloads without freezing the user interface
- **Lightweight and reliable** - Designed to download thousands of icons in batch
- **Smart and efficient**
  - Avoids duplicate icons (reuses custom icons already present in the database)
  - Download entries without a URL scheme (automatically prefix URLs with `https://` or `http://`)
  - Download entries without a URL field (automatically uses Title field)
  - Automatically resize icons (scales down to configurable max size, default 128x128 px)
  - **Skip entries with existing icons** (optional)
  - **Configurable icon name prefix** (default: `yafd-`, can be removed)
- **Enhanced Icon Detection**
  - Prioritizes high-quality `apple-touch-icon` images
  - Detects `favicon-32x32.png`, `favicon-96x96.png`, and other modern patterns
  - Supports `sizes` attribute for intelligent icon selection
  - Multiple fallback locations (`/favicon.ico`, `/apple-touch-icon.png`, etc.)
- **Robust Fallback System**
  - Direct website download with smart HTML parsing
  - Automatic fallback to multiple providers (Google, DuckDuckGo, Icon Horse, Yandex)
  - Handles redirects, cookies, and modern browser headers
- **Android URL Support** - Converts `androidapp://` URLs to web domains
- **Placeholder Resolution** - Resolves KeePass placeholders like `{REF:U@I:UUID}`
- **Self-signed Certificate Support** - Optional bypass for internal servers
- **Configurable Timeout** - 5-60 seconds, prevents hanging on expired domains
- **Auto-save Option** - Automatically save database after downloading
- **Ghost Modification Fix** - Database only marked as modified when icons actually change
- Linux support (_experimental_)
- Proxy support (respects _KeePass_ settings)
- Modern TLS support (supports TLS up to 1.3 on .NET 4.8)

### What's New (Enhanced Edition)

| Feature | Description |
|---------|-------------|
| 🔧 **Fixed FaviconKit** | Removed dead provider, added Icon Horse & Clearbit |
| 🔧 **Ghost Modifications Fixed** | Database only marked modified when icons actually change |
| ✨ **Skip Existing Icons** | Option to skip entries that already have custom icons |
| ✨ **Configurable Prefix** | `yafd-` prefix can be customized or removed |
| ✨ **Android URL Support** | 300+ app mappings for `androidapp://` URLs |
| ✨ **Fallback Providers** | Auto-retry with Google, DuckDuckGo, Icon Horse, Yandex |
| ✨ **Self-signed Certs** | Support for internal servers with self-signed SSL |
| ✨ **Configurable Timeout** | 5-60 seconds timeout setting |
| ✨ **Auto-save** | Option to save database after downloading |
| ✨ **Placeholder Resolution** | Supports `{REF:...}` placeholders in URLs |
| ✨ **Enhanced Detection** | apple-touch-icon, sizes attribute, modern favicon patterns |

## Requirements

- _KeePass_ 2.34 or higher.

### Linux

- _Mono_ 4.8.0 or newer.

### Windows

- _.NET Framework_ 4.5 or newer.

## Installation

- Download the [latest release](https://github.com/tzii/Keepass-Favicon-Enhanced/releases/latest).
- Copy _YetAnotherFaviconDownloader.plgx_ into _KeePass_ plugins folder.
- Restart _KeePass_ in order to load the plugin.

## Usage

This plugin adds a new menu item called **"Download Favicons"** into the entry and group context menus of _KeePass_.

Select one or more entries and click _Download Favicons_ to download the _favicon_ associated with that _URL_.

![Entry Context Menu](docs/images/entry-context-menu.gif)

---

You can also select one group and click _Download Favicons_ to download _favicons_ for all entries in this group and its subgroups.

![Group Context Menu](docs/images/group-context-menu.gif)

## Configuration

Access settings via **Tools → Yet Another Favicon Downloader**:

### Basic Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Automatic prefix URLs** | Add `https://` or `http://` to URLs without scheme | Off |
| **Use title field if URL empty** | Fallback to entry title for favicon lookup | Off |
| **Update entry last modification time** | Touch entry when icon changes | On |

### Enhanced Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Skip entries with existing icons** | Don't re-download icons for entries that already have one | Off |
| **Use fallback providers** | Try Google/DuckDuckGo/etc. when direct download fails | On |
| **Allow self-signed SSL certificates** | Allow connections to servers with invalid SSL | Off |
| **Auto-save database after download** | Automatically save after batch download | Off |

### Advanced Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Maximum icon size** | 16x16 to 128x128 pixels | 128x128 |
| **Connection timeout** | 5 to 60 seconds | 15s |
| **Configure icon name prefix** | Prefix for icon names (can be empty) | `yafd-` |
| **Custom download provider** | Use a specific favicon API | None |

## Android URL Support

The Enhanced edition supports Android app URLs in the format:

```
androidapp://com.reddit.frontpage
androidapp://com.instagram.android
androidapp://com.spotify.music
```

These are automatically converted to web domains for favicon fetching. The plugin includes **300+ pre-mapped Android packages** covering:

- Social Media (Facebook, Instagram, Twitter, TikTok, etc.)
- Messaging (WhatsApp, Telegram, Slack, Discord, etc.)
- Google Apps (Gmail, YouTube, Maps, Drive, etc.)
- Microsoft Apps (Outlook, Teams, OneDrive, etc.)
- Entertainment (Netflix, Spotify, Disney+, etc.)
- Shopping (Amazon, eBay, Walmart, etc.)
- Finance (PayPal, Venmo, Banks, Crypto, etc.)
- Travel (Uber, Airbnb, Booking, etc.)
- And many more!

Unknown packages are automatically derived using smart heuristics:
- `com.example.app` → `example.com`
- `org.example.app` → `example.org`
- `uk.co.example.app` → `example.co.uk`

## Custom Provider URLs

When setting up a custom provider, use these placeholders:

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{URL:HOST}` | The domain name | `example.com` |
| `{URL:SCM}` | The URL scheme | `https` |
| `{YAFD:ICON_SIZE}` | Maximum icon size setting | `128` |

### Example Provider URLs

```
# Google (with scheme fix)
https://www.google.com/s2/favicons?domain={URL:SCM}://{URL:HOST}&sz={YAFD:ICON_SIZE}

# DuckDuckGo
https://icons.duckduckgo.com/ip3/{URL:HOST}.ico

# Icon Horse
https://icon.horse/icon/{URL:HOST}

# Clearbit Logo (high quality)
https://logo.clearbit.com/{URL:HOST}
```

## Changelog

See the [Releases section](https://github.com/tzii/Keepass-Favicon-Enhanced/releases) for more details of each release.

### Enhanced Edition Changes

- **v2.0.0** - Major enhancement release
  - Removed dead FaviconKit provider
  - Added Icon Horse and Clearbit Logo providers
  - Fixed ghost database modifications issue
  - Added "Skip existing icons" option
  - Added configurable icon name prefix
  - Added Android URL support with 300+ package mappings
  - Added fallback provider cascade
  - Added self-signed certificate support
  - Added configurable timeout
  - Added auto-save option
  - Added placeholder URL resolution
  - Enhanced HTML parsing (apple-touch-icon, sizes, modern patterns)
  - Added `{URL:SCM}` placeholder for scheme
  - Improved icon priority scoring

## Contributing

Have a bug or a feature request? Please first search for [open and closed issues](https://github.com/tzii/Keepass-Favicon-Enhanced/issues?q=is%3Aissue). If your problem or idea is not addressed yet, [please open a new issue](https://github.com/tzii/Keepass-Favicon-Enhanced/issues/new).

## Copyright and license

_Yet Another Favicon Downloader Enhanced_ source code is licensed under the [MIT License](LICENSE).

Original plugin by [navossoc](https://github.com/navossoc/KeePass-Yet-Another-Favicon-Downloader).

[Documentation](docs/README.md) is licensed under a [Creative Commons Attribution-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-sa/4.0/).

When you contribute to this repository you are doing so under the above licenses.
