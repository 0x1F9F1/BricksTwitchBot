using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BricksTwitchBot.Irc;
using SharpConfig;

namespace BricksTwitchBot
{
    internal static class Globals
    {
        public static readonly Regex MessageMatch =         new Regex(@"^@color=(?<color>#\w{6})?;display-name=(?<name>[^;]+)?;emotes=(?<emote>[^;]*);(?:sent-ts=\d+;)?subscriber=(?<issub>[01]);(?:tmi-sent-ts=\d+;)?turbo=(?<isturbo>[01]);user-id=(?<userid>\d+);?user-type=(?<usertype>\S*) :(?<secondname>\S+)!\S+@\S+\.tmi\.twitch\.tv PRIVMSG #\w+ :(?<message>.+)$", RegexOptions.Compiled);
        public static readonly Regex ModeMatch =            new Regex(@":jtv MODE #\S+ (?<change>[+-])o (?<user>\S+)", RegexOptions.Compiled);
        public static readonly Regex PingMatch =            new Regex(@"PING :(?<ip>\S+)", RegexOptions.Compiled);
        public static readonly Regex TimeoutMatch =         new Regex(@":tmi\.twitch\.tv CLEARCHAT #\S+ :(?<user>\S+)", RegexOptions.Compiled);
        public static readonly Regex NotifyMatch =          new Regex(@":twitchnotify!twitchnotify@twitchnotify.tmi.twitch.tv PRIVMSG #\S+ :(?<message>.+)", RegexOptions.Compiled);
        public static readonly Regex CapAckMatch =          new Regex(@":tmi\.twitch\.tv CAP \* ACK :(?<cap>.+)", RegexOptions.Compiled);
        public static readonly Regex CodeMatch =            new Regex(@":tmi\.twitch\.tv (?<code>\d{3}) \S+ :(?<message>.+)", RegexOptions.Compiled);
        public static readonly Regex Code2Match =           new Regex(@":\S+\.tmi\.twitch\.tv (?<code>\d{3}) \S+ = #\S+ :(?<message>.+)", RegexOptions.Compiled);
        public static readonly Regex Code3Match =           new Regex(@":\S+\.tmi\.twitch\.tv (?<code>\d{3}) \S+ #\S+ :(?<message>.+)", RegexOptions.Compiled);
        public static readonly Regex JoinMatch =            new Regex(@":\S+!\S+@(?<name>\S+)\.tmi\.twitch\.tv JOIN #\S+", RegexOptions.Compiled);
        public static readonly Regex PartMatch =            new Regex(@":\S+!\S+@(?<name>\S+)\.tmi\.twitch\.tv PART #\S+", RegexOptions.Compiled);
        public static readonly Regex GlobalUserStateMatch = new Regex(@"@color=(?<color>#\d{6})?;display-name=(?<name>\S+);emote-sets=(?<emotesets>\S+);turbo=(?<isturbo>\d);user-id=(?<userid>\d+);user-type=(?<usertype>\S*) :tmi\.twitch\.tv GLOBALUSERSTATE", RegexOptions.Compiled);
        public static readonly Regex UserStateMatch =       new Regex(@"@color=(?<color>#\d{6})?;display-name=(?<name>\S+);emote-sets=(?<emotesets>\S+);subscriber=(?<issub>\d);turbo=(?<isturbo>\d);user-type=(?<usertype>\S*) :tmi\.twitch\.tv USERSTATE #\S+", RegexOptions.Compiled);
        public static readonly Regex RoomStateMatch =       new Regex(@"@(?:broadcaster-lang=(?<lang>[^;]*);?)?(?:r9k=(?<isr9k>[01]);?)?(?:slow=(?<isSlow>\d+);?)?(?:subs-only=(?<isSub>[01]);?)? :tmi\.twitch\.tv ROOMSTATE #\S+$", RegexOptions.Compiled);
        public static readonly Regex NoticeMatch =          new Regex(@"@msg-id=(?<msgid>\S+) :tmi\.twitch\.tv NOTICE #\S+ :(?<message>.+)", RegexOptions.Compiled);

        public static readonly Regex UrlRegex = new Regex(@"\S+[.]\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex EmailRegex = new Regex(@"([a-zA-Z_0-9.-]+\@[a-zA-Z_0-9.-]+\.\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static ConcurrentQueue<Paragraph> ChatTextBoxQueue = new ConcurrentQueue<Paragraph>();
        public static ConcurrentQueue<Paragraph> LogTextBoxQueue =  new ConcurrentQueue<Paragraph>();

        public static Dispatcher WindowDispatcher;
        public static IrcClient ChatIrc;
        public static IrcClient ListenIrc;
        public static Configuration OptionsConfig;
        public static Inline[] MessageStart;
        public static Label ChatStatusBox;
        public static bool Running;

        public static FontFamily FontFamily;
        public static StreamWriter LogWriter;

        public static Image ImageFromUrl(string url)
        {
            var bitmapImage = new BitmapImage(new Uri(url));
            var image = new Image
            {
                Source = bitmapImage,
                MaxHeight = 25.0,
                MaxWidth = 20.0
            };
            return image;
        }

        public static void OnUi(Action action)
        {
            WindowDispatcher.BeginInvoke(action);
        }

        public static Brush RgbToBrush(string rgb)
        {
            if (string.IsNullOrEmpty(rgb))
            {
                var rand = new Random(DateTime.Now.Millisecond);
                return
                    new SolidColorBrush(Color.FromRgb((byte) rand.Next(0, 255), (byte) rand.Next(0, 255),
                        (byte) rand.Next(0, 255)));
            }
            var obj = ColorConverter.ConvertFromString(rgb);
            return obj != null ? new SolidColorBrush((Color) obj) : new SolidColorBrush(Colors.Black);
        }

        public static Image FromResource(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{path}");
            bitmapImage.EndInit();
            return new Image
            {
                Source = bitmapImage,
                MaxHeight = bitmapImage.PixelHeight,
                MaxWidth = bitmapImage.PixelHeight
            };
        }
    }
}