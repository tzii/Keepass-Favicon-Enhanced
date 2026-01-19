using System;
using System.Collections.Generic;

namespace YetAnotherFaviconDownloader
{
    /// <summary>
    /// Comprehensive mapping of Android package names to web domains
    /// Used for converting androidapp:// URLs to fetchable web domains
    /// </summary>
    public static class AndroidPackageMapping
    {
        private static readonly Dictionary<string, string> Mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Social Media
            { "com.facebook.katana", "facebook.com" },
            { "com.facebook.orca", "messenger.com" },
            { "com.facebook.lite", "facebook.com" },
            { "com.instagram.android", "instagram.com" },
            { "com.twitter.android", "twitter.com" },
            { "com.twitter.android.lite", "twitter.com" },
            { "com.snapchat.android", "snapchat.com" },
            { "com.pinterest", "pinterest.com" },
            { "com.tumblr", "tumblr.com" },
            { "com.linkedin.android", "linkedin.com" },
            { "com.linkedin.android.lite", "linkedin.com" },
            { "com.reddit.frontpage", "reddit.com" },
            { "com.tiktok.plus", "tiktok.com" },
            { "com.ss.android.ugc.trill", "tiktok.com" },
            { "com.zhiliaoapp.musically", "tiktok.com" },
            { "tv.twitch.android.app", "twitch.tv" },
            { "com.discord", "discord.com" },
            { "com.discord.pmp", "discord.com" },
            { "me.wechat", "wechat.com" },
            { "com.tencent.mm", "wechat.com" },
            { "jp.naver.line.android", "line.me" },
            { "org.telegram.messenger", "telegram.org" },
            { "org.telegram.messenger.web", "web.telegram.org" },
            { "com.viber.voip", "viber.com" },
            
            // Messaging
            { "com.whatsapp", "whatsapp.com" },
            { "com.whatsapp.w4b", "whatsapp.com" },
            { "com.skype.raider", "skype.com" },
            { "com.skype.m2", "skype.com" },
            { "com.Slack", "slack.com" },
            { "com.slack", "slack.com" },
            { "us.zoom.videomeetings", "zoom.us" },
            { "com.microsoft.teams", "teams.microsoft.com" },
            { "com.google.android.apps.meetings", "meet.google.com" },
            { "com.webex.meetings", "webex.com" },
            { "com.cisco.webex.meetings", "webex.com" },
            { "com.gotomeeting", "gotomeeting.com" },
            
            // Google Apps
            { "com.google.android.gm", "gmail.com" },
            { "com.google.android.gm.lite", "gmail.com" },
            { "com.google.android.youtube", "youtube.com" },
            { "com.google.android.apps.youtube.music", "music.youtube.com" },
            { "com.google.android.apps.youtube.kids", "youtubekids.com" },
            { "com.google.android.apps.maps", "maps.google.com" },
            { "com.google.android.apps.photos", "photos.google.com" },
            { "com.google.android.apps.docs", "docs.google.com" },
            { "com.google.android.apps.docs.editors.docs", "docs.google.com" },
            { "com.google.android.apps.docs.editors.sheets", "sheets.google.com" },
            { "com.google.android.apps.docs.editors.slides", "slides.google.com" },
            { "com.google.android.calendar", "calendar.google.com" },
            { "com.google.android.keep", "keep.google.com" },
            { "com.google.android.apps.translate", "translate.google.com" },
            { "com.google.android.googlequicksearchbox", "google.com" },
            { "com.google.android.apps.chromecast.app", "google.com/chromecast" },
            { "com.google.android.apps.tachyon", "duo.google.com" },
            { "com.google.android.apps.nbu.files", "files.google.com" },
            { "com.google.android.apps.fitness", "fit.google.com" },
            { "com.google.android.apps.walletnfcrel", "pay.google.com" },
            { "com.google.android.apps.authenticator2", "google.com" },
            { "com.google.android.deskclock", "google.com" },
            { "com.google.android.contacts", "contacts.google.com" },
            { "com.android.chrome", "google.com/chrome" },
            { "com.chrome.beta", "google.com/chrome" },
            { "com.chrome.dev", "google.com/chrome" },
            { "com.chrome.canary", "google.com/chrome" },
            
            // Microsoft Apps
            { "com.microsoft.office.outlook", "outlook.com" },
            { "com.microsoft.office.officehubrow", "office.com" },
            { "com.microsoft.office.word", "office.com" },
            { "com.microsoft.office.excel", "office.com" },
            { "com.microsoft.office.powerpoint", "office.com" },
            { "com.microsoft.office.onenote", "onenote.com" },
            { "com.microsoft.onedrive", "onedrive.com" },
            { "com.microsoft.skydrive", "onedrive.com" },
            { "com.microsoft.todos", "todo.microsoft.com" },
            { "com.microsoft.bing", "bing.com" },
            { "com.microsoft.emmx", "edge.microsoft.com" },
            { "com.microsoft.launcher", "microsoft.com" },
            { "com.microsoft.authenticator", "microsoft.com" },
            { "com.microsoft.xboxone.smartglass", "xbox.com" },
            { "com.microsoft.xboxone.smartglass.beta", "xbox.com" },
            { "com.microsoft.azure.authenticator", "azure.microsoft.com" },
            { "com.microsoft.cortana", "cortana.ai" },
            
            // Entertainment & Streaming
            { "com.netflix.mediaclient", "netflix.com" },
            { "com.amazon.avod.thirdpartyclient", "primevideo.com" },
            { "com.amazon.avod", "primevideo.com" },
            { "com.disney.disneyplus", "disneyplus.com" },
            { "com.hbo.hbonow", "hbomax.com" },
            { "com.hbo.stream", "hbomax.com" },
            { "com.hulu.plus", "hulu.com" },
            { "com.cbs.app", "paramountplus.com" },
            { "com.peacocktv.peacockandroid", "peacocktv.com" },
            { "com.crunchyroll.crunchyroid", "crunchyroll.com" },
            { "com.funimation.funimation", "funimation.com" },
            { "com.plexapp.android", "plex.tv" },
            { "com.spotify.music", "spotify.com" },
            { "com.spotify.lite", "spotify.com" },
            { "com.apple.android.music", "music.apple.com" },
            { "com.pandora.android", "pandora.com" },
            { "com.soundcloud.android", "soundcloud.com" },
            { "deezer.android.app", "deezer.com" },
            { "com.clearchannel.iheartradio.controller", "iheart.com" },
            { "com.audible.application", "audible.com" },
            { "tunein.player", "tunein.com" },
            { "com.shazam.android", "shazam.com" },
            
            // Shopping
            { "com.amazon.mShop.android.shopping", "amazon.com" },
            { "com.amazon.mShop.android", "amazon.com" },
            { "com.amazon.windowshop", "amazon.com" },
            { "com.ebay.mobile", "ebay.com" },
            { "com.alibaba.aliexpresshd", "aliexpress.com" },
            { "com.shopify.arrive", "shopify.com" },
            { "com.etsy.android", "etsy.com" },
            { "com.walmart.android", "walmart.com" },
            { "com.target.ui", "target.com" },
            { "com.bestbuy.android", "bestbuy.com" },
            { "com.homedepot", "homedepot.com" },
            { "com.lowes.android", "lowes.com" },
            { "com.costco.app.android", "costco.com" },
            { "com.macys.android", "macys.com" },
            { "com.nordstrom.app", "nordstrom.com" },
            { "com.kohls.mcommerce.opal", "kohls.com" },
            { "com.jcpenney.android", "jcpenney.com" },
            { "com.sephora", "sephora.com" },
            { "com.ulta.ulta", "ulta.com" },
            { "com.nike.omega", "nike.com" },
            { "com.adidas.app", "adidas.com" },
            { "com.zappos.android", "zappos.com" },
            { "com.wish.android", "wish.com" },
            { "com.shein.android", "shein.com" },
            { "com.zara.zara", "zara.com" },
            { "com.hm.goe", "hm.com" },
            { "com.ikea.kompis", "ikea.com" },
            
            // Finance & Banking
            { "com.paypal.android.p2pmobile", "paypal.com" },
            { "com.venmo", "venmo.com" },
            { "com.squareup.cash", "cash.app" },
            { "com.zellepay.zelle", "zellepay.com" },
            { "com.coinbase.android", "coinbase.com" },
            { "piuk.blockchain.android", "blockchain.com" },
            { "com.binance.dev", "binance.com" },
            { "co.mona.android", "crypto.com" },
            { "exodusmovement.exodus", "exodus.com" },
            { "com.krakenfutures.trade", "kraken.com" },
            { "com.robinhood.android", "robinhood.com" },
            { "com.etrade.mobilepro.activity", "etrade.com" },
            { "com.tdameritrade.mobile", "tdameritrade.com" },
            { "com.schwab.mobile", "schwab.com" },
            { "com.fidelity.android", "fidelity.com" },
            { "com.vanguard.retail", "vanguard.com" },
            { "com.inuit.turbotax.mobile", "turbotax.intuit.com" },
            { "com.intuit.quickbooks", "quickbooks.intuit.com" },
            { "com.mint", "mint.com" },
            { "com.ynab.ynab", "ynab.com" },
            { "com.wf.wellsfargomobile", "wellsfargo.com" },
            { "com.citi.citimobile", "citi.com" },
            { "com.chase.sig.android", "chase.com" },
            { "com.konylabs.capitalone", "capitalone.com" },
            { "com.usaa.mobile.android.usaa", "usaa.com" },
            { "com.usbank.mobilebanking", "usbank.com" },
            { "com.pnc.ecommerce.mobile", "pnc.com" },
            { "com.ally.MobileBanking", "ally.com" },
            { "com.discoverfinancial.mobile", "discover.com" },
            { "com.americanexpress.android.acctsvcs.us", "americanexpress.com" },
            { "com.barclays.android.barclaysmobilebanking", "barclays.com" },
            { "com.hsbc.hsbcnet", "hsbc.com" },
            { "com.wealthfront", "wealthfront.com" },
            { "com.betterment", "betterment.com" },
            { "com.acorns.android", "acorns.com" },
            { "com.stash.stashinvest", "stash.com" },
            { "com.revolut.revolut", "revolut.com" },
            { "com.transferwise.android", "wise.com" },
            
            // Travel & Transportation
            { "com.ubercab", "uber.com" },
            { "com.ubercab.eats", "ubereats.com" },
            { "me.lyft.android", "lyft.com" },
            { "com.lyft.android", "lyft.com" },
            { "com.airbnb.android", "airbnb.com" },
            { "com.booking", "booking.com" },
            { "com.tripadvisor.tripadvisor", "tripadvisor.com" },
            { "com.expedia.bookings", "expedia.com" },
            { "com.hopper.mountainview.play", "hopper.com" },
            { "com.kayak.android", "kayak.com" },
            { "com.southwest.android", "southwest.com" },
            { "com.delta.mobile.android", "delta.com" },
            { "com.united.mobile.android", "united.com" },
            { "com.aa.android", "aa.com" },
            { "com.jetblue.JetBlueAndroid", "jetblue.com" },
            { "com.marriott.mrt", "marriott.com" },
            { "com.hilton.android.hhonors", "hilton.com" },
            { "com.ihg.apps.android", "ihg.com" },
            { "com.hertz.android", "hertz.com" },
            { "com.enterprise.android.ehi", "enterprise.com" },
            { "com.nationalcar.reservations", "nationalcar.com" },
            { "com.turo.android", "turo.com" },
            { "com.getaround.android", "getaround.com" },
            { "com.mapquest.android.ace", "mapquest.com" },
            { "com.waze", "waze.com" },
            { "com.sygic.aura", "sygic.com" },
            { "com.tomtom.gplay.navapp", "tomtom.com" },
            { "com.gasbaddy", "gasbuddy.com" },
            
            // Food & Delivery
            { "com.grubhub.android", "grubhub.com" },
            { "com.dd.doordash", "doordash.com" },
            { "com.postmates.android", "postmates.com" },
            { "com.instacart.client", "instacart.com" },
            { "com.contextlogic.geek", "geek.com" },
            { "com.mcd.mcdonalds", "mcdonalds.com" },
            { "com.starbucks.mobilecard", "starbucks.com" },
            { "com.dunkinbrands.otgo", "dunkindonuts.com" },
            { "com.subway.subway", "subway.com" },
            { "com.wendys.nutritiontool", "wendys.com" },
            { "com.tacobell.ordering", "tacobell.com" },
            { "com.chipotle.ordering", "chipotle.com" },
            { "com.pizzahut.android", "pizzahut.com" },
            { "com.dominos.cust.profile", "dominos.com" },
            { "com.littlecaesars", "littlecaesars.com" },
            { "com.papajohns.android", "papajohns.com" },
            { "com.bfrw.bf", "buffalowildwings.com" },
            { "com.olo.chilis", "chilis.com" },
            { "com.applebees.AppleBees", "applebees.com" },
            { "com.olivegarden.mobile", "olivegarden.com" },
            { "com.redrobin.mobile", "redrobin.com" },
            { "com.dine.loyalty.mod.ihop", "ihop.com" },
            { "com.crackerbarrel", "crackerbarrel.com" },
            { "com.panera.bread", "panerabread.com" },
            { "com.chickfila.cfaone", "chick-fil-a.com" },
            { "com.popeyes.loyalty", "popeyes.com" },
            
            // News & Reading
            { "com.nytimes.android", "nytimes.com" },
            { "com.washingtonpost.android", "washingtonpost.com" },
            { "com.guardian", "theguardian.com" },
            { "com.cnn.mobile.android.phone", "cnn.com" },
            { "com.foxnews.android", "foxnews.com" },
            { "com.nbcuni.nbc.newdigitalplus", "nbcnews.com" },
            { "au.com.newscorpaustralia.skynews", "skynews.com" },
            { "com.bbc.news", "bbc.com" },
            { "com.npr.one.android", "npr.org" },
            { "com.huffingtonpost.android", "huffpost.com" },
            { "com.buzzfeed.android", "buzzfeed.com" },
            { "com.flipboard.app", "flipboard.com" },
            { "com.google.android.apps.magazines", "news.google.com" },
            { "com.medium.reader", "medium.com" },
            { "com.app.xt3.pocket", "getpocket.com" },
            { "com.ideashower.readitlater.pro", "getpocket.com" },
            { "com.instapaper.android", "instapaper.com" },
            { "org.feedly.mobile", "feedly.com" },
            { "com.amazon.kindle", "kindle.amazon.com" },
            { "com.bn.nook.app", "barnesandnoble.com" },
            { "com.kobo.arc", "kobo.com" },
            { "com.goodreads", "goodreads.com" },
            { "com.wattpad", "wattpad.com" },
            { "com.scribd.app.reader0", "scribd.com" },
            
            // Productivity & Tools
            { "com.dropbox.android", "dropbox.com" },
            { "com.dropbox.paper", "paper.dropbox.com" },
            { "com.box.android", "box.com" },
            { "com.evernote", "evernote.com" },
            { "md.obsidian", "obsidian.md" },
            { "com.roamresearch.android", "roamresearch.com" },
            { "com.notion.id", "notion.so" },
            { "com.atlassian.android.jira.core", "atlassian.com" },
            { "com.atlassian.android.trello", "trello.com" },
            { "com.asana.app", "asana.com" },
            { "com.monday.monday", "monday.com" },
            { "com.basecamp.bc3", "basecamp.com" },
            { "com.agilebits.onepassword", "1password.com" },
            { "com.lastpass.lpandroid", "lastpass.com" },
            { "com.dashlane", "dashlane.com" },
            { "keepass2android.keepass2android", "keepass.info" },
            { "com.bitwarden.android", "bitwarden.com" },
            { "com.nordpass.android.app.password.manager", "nordpass.com" },
            { "com.adobe.reader", "adobe.com" },
            { "com.adobe.scan.android", "adobe.com" },
            { "com.adobe.lrmobile", "lightroom.adobe.com" },
            { "com.adobe.psmobile", "photoshop.adobe.com" },
            { "com.canva.editor", "canva.com" },
            { "com.figma.mirror", "figma.com" },
            { "com.figma.figma", "figma.com" },
            { "com.grammarly.android.keyboard", "grammarly.com" },
            { "ai.x.app.translate", "translate.google.com" },
            { "com.duolingo", "duolingo.com" },
            { "com.babbel.mobile.android.en", "babbel.com" },
            { "com.rosettastone.mobile", "rosettastone.com" },
            { "com.memrise.android.memrisecompanion", "memrise.com" },
            
            // Health & Fitness
            { "com.fitbit.FitbitMobile", "fitbit.com" },
            { "com.strava", "strava.com" },
            { "com.myfitnesspal.android", "myfitnesspal.com" },
            { "com.nike.ntc", "nike.com" },
            { "com.underarmour.record", "underarmour.com" },
            { "com.fitnesskeeper.runkeeper.pro", "runkeeper.com" },
            { "cc.pacer.androidapp", "pacer.cc" },
            { "com.calm.android", "calm.com" },
            { "com.headspace.android", "headspace.com" },
            { "com.innerbalance", "meditopia.com" },
            { "com.flo.health", "flo.health" },
            { "com.clue.android", "helloclue.com" },
            { "com.ovuline.parenting", "ovia.health" },
            { "com.webmd.android", "webmd.com" },
            { "com.healthtap.userhtexpress", "healthtap.com" },
            { "com.zocdoc.android", "zocdoc.com" },
            { "com.teladoc.members", "teladoc.com" },
            { "com.cvs.launchers.cvs", "cvs.com" },
            { "com.walgreens.android", "walgreens.com" },
            { "com.riteaid.app", "riteaid.com" },
            { "com.goodrx", "goodrx.com" },
            { "com.epocrates", "epocrates.com" },
            
            // Gaming
            { "com.epicgames.fortnite", "fortnite.com" },
            { "com.ea.games.apexlegendsmobile", "ea.com" },
            { "com.tencent.ig", "pubgmobile.com" },
            { "com.activision.callofduty.shooter", "callofduty.com" },
            { "com.mojang.minecraftpe", "minecraft.net" },
            { "com.roblox.client", "roblox.com" },
            { "com.supercell.clashofclans", "clashofclans.com" },
            { "com.supercell.clashroyale", "clashroyale.com" },
            { "com.supercell.brawlstars", "brawlstars.com" },
            { "com.king.candycrushsaga", "king.com" },
            { "com.playrix.gardenscapes", "playrix.com" },
            { "com.playrix.homescapes", "playrix.com" },
            { "com.zynga.words3", "zynga.com" },
            { "com.zynga.poker", "zynga.com" },
            { "com.nianticlabs.pokemongo", "pokemongo.com" },
            { "com.halfbrick.fruitninja", "halfbrick.com" },
            { "com.halfbrick.jetpackjoyride", "halfbrick.com" },
            { "com.rovio.angrybirds", "angrybirds.com" },
            { "com.gameloft.android.GloftA8HM", "gameloft.com" },
            { "com.kabam.marvelbattle", "kabam.com" },
            { "com.blizzard.diablo.immortal", "blizzard.com" },
            { "com.blizzard.wtcg.hearthstone", "hearthstone.blizzard.com" },
            { "com.miHoYo.GenshinImpact", "genshin.mihoyo.com" },
            { "com.nintendo.zaka", "nintendo.com" },
            { "com.steam.mobileapp", "steampowered.com" },
            { "com.valvesoftware.android.steam.community", "steamcommunity.com" },
            { "com.gog.galaxy", "gog.com" },
            { "com.epicgames.portal", "epicgames.com" },
            { "com.playstation.mobile2", "playstation.com" },
            { "com.xbox.gamepasshub", "xbox.com" },
            { "com.nvidia.geforcenow", "geforcenow.com" },
            
            // Developer Tools
            { "com.github.android", "github.com" },
            { "io.github.nicholasclark", "github.com" },
            { "com.gitlab.app", "gitlab.com" },
            { "com.bitbucket.android", "bitbucket.org" },
            { "com.atlassian.bitbucket", "bitbucket.org" },
            { "com.termux", "termux.com" },
            { "com.docker.android", "docker.com" },
            { "io.kodular.kreatordev.kodular", "kodular.io" },
            { "com.digitalocean.digitalocean", "digitalocean.com" },
            { "com.aws.consoleapp", "aws.amazon.com" },
            { "com.microsoft.azure.mobile", "azure.microsoft.com" },
            { "com.google.cloud.console", "console.cloud.google.com" },
            
            // Utilities
            { "com.speedtest.android", "speedtest.net" },
            { "com.fast.speedtest.free", "fast.com" },
            { "org.torproject.torbrowser", "torproject.org" },
            { "com.duckduckgo.mobile.android", "duckduckgo.com" },
            { "com.brave.browser", "brave.com" },
            { "org.mozilla.firefox", "firefox.com" },
            { "org.mozilla.firefox_beta", "firefox.com" },
            { "com.opera.browser", "opera.com" },
            { "com.ksmobile.launcher", "apusapps.com" },
            { "com.sec.android.app.sbrowser", "samsung.com" },
            { "com.expressvpn.vpn", "expressvpn.com" },
            { "com.nordvpn.android", "nordvpn.com" },
            { "com.surfshark.vpnclient.android", "surfshark.com" },
            { "com.pia.android.vpn", "privateinternetaccess.com" },
            { "net.mullvad.mullvadvpn", "mullvad.net" },
            { "ch.protonvpn.android", "protonvpn.com" },
            { "ch.protonmail.android", "protonmail.com" },
            { "com.tutanota.tutanota", "tutanota.com" },
            { "com.themail.xyz.android", "mail.com" },
            { "com.cleanmaster.mguard", "cmcm.com" },
            { "com.avast.android.mobilesecurity", "avast.com" },
            { "com.avira.android", "avira.com" },
            { "com.avg.cleaner", "avg.com" },
            { "com.kaspersky.security.cloud", "kaspersky.com" },
            { "com.mcafee.vsm_android", "mcafee.com" },
            { "com.norton.mobilesecurity", "norton.com" },
            { "org.malwarebytes.antimalware", "malwarebytes.com" },
            
            // Cryptocurrency
            { "com.wallet.crypto.trustapp", "trustwallet.com" },
            { "io.metamask", "metamask.io" },
            { "com.tronlink.wallet", "tronlink.org" },
            { "com.blockchainvault", "blockchain.com" },
            { "network.electrum.electrum_ltc", "electrum-ltc.org" },
            { "de.schildbach.wallet", "bitcoin.org" },
            { "com.mycelium.wallet", "mycelium.com" },
            { "com.samourai.wallet", "samouraiwallet.com" },
            { "io.bluewallet.bluewallet", "bluewallet.io" },
            { "com.ledger.live", "ledger.com" },
            { "com.trezor.suite", "trezor.io" },
            
            // Dating
            { "com.tinder", "tinder.com" },
            { "com.bumble.app", "bumble.com" },
            { "com.match.android.matchmobile", "match.com" },
            { "com.spark.pof", "pof.com" },
            { "com.okcupid.okcupid", "okcupid.com" },
            { "com.hinge.app", "hinge.co" },
            { "com.coffee_meets_bagel.android", "coffeemeetsbagel.com" },
            { "co.hoop.app", "hoop.co" },
            { "com.grindrapp.android", "grindr.com" },
            { "com.scruff.app", "scruff.com" },
            
            // Miscellaneous
            { "com.xe.currency", "xe.com" },
            { "com.accuweather.android", "accuweather.com" },
            { "com.weather.Weather", "weather.com" },
            { "com.wunderground.android.weather", "wunderground.com" },
            { "com.darksky.darksky", "darksky.net" },
            { "com.yahoo.mobile.client.android.mail", "mail.yahoo.com" },
            { "com.yahoo.mobile.client.android.yahoo", "yahoo.com" },
            { "com.yahoo.mobile.client.android.finance", "finance.yahoo.com" },
            { "com.aol.mobile.aolapp", "aol.com" },
            { "com.verizon.myverizon", "verizon.com" },
            { "com.att.myWireless", "att.com" },
            { "com.sprint.care", "sprint.com" },
            { "com.tmobile.pr.mytmobile", "t-mobile.com" },
            { "com.yelp.android", "yelp.com" },
            { "com.foursquare.robin", "foursquare.com" },
            { "com.nextdoor", "nextdoor.com" },
            { "com.eventbrite.attendee", "eventbrite.com" },
            { "com.ticketmaster.mobile.android.na", "ticketmaster.com" },
            { "com.stubhub", "stubhub.com" },
            { "com.seatgeek.android", "seatgeek.com" },
            { "com.meetup", "meetup.com" },
        };

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
    }
}
