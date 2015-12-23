using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SharpConfig;

namespace BricksTwitchBot
{
    internal static class Globals
    {
        public static readonly Regex MessageMatch =
            new Regex(
                @"^@color=(?<color>#\w{6})?;display-name=(?<name>[^;]+)?;emotes=(?<emote>[^;]*);(?:sent-ts=\d+;)?subscriber=(?<issub>\d);(?:tmi-sent-ts=\d+;)?turbo=(?<isturbo>\d);user-id=(?<userid>\d+);?user-type=(?<usertype>\S*) :(?<secondname>\S+)!\S+@\S+\.tmi\.twitch\.tv PRIVMSG #\w+ :(?<message>.+)$",
                RegexOptions.Compiled);

        public static readonly Regex ModeMatch = new Regex(@":jtv MODE #\S+ (?<change>[+-])o (?<user>\S+)",
            RegexOptions.Compiled);

        public static readonly Regex PingMatch = new Regex(@"PING :(?<ip>\S+)", RegexOptions.Compiled);

        public static readonly Regex TimeoutMatch = new Regex(@":tmi\.twitch\.tv CLEARCHAT #\S+ :(?<user>\S+)",
            RegexOptions.Compiled);

        public static readonly Regex NotifyMatch =
            new Regex(@":twitchnotify!twitchnotify@twitchnotify.tmi.twitch.tv PRIVMSG #\S+ :(?<message>.+)",
                RegexOptions.Compiled);

        public static readonly Regex CapAckMatch = new Regex(@":tmi\.twitch\.tv CAP \* ACK :(?<cap>.+)",
            RegexOptions.Compiled);

        public static readonly Regex CodeMatch = new Regex(@":tmi\.twitch\.tv (?<code>\d{3}) \S+ :(?<message>.+)",
            RegexOptions.Compiled);

        public static readonly Regex Code2Match =
            new Regex(@":\S+\.tmi\.twitch\.tv (?<code>\d{3}) \S+ = #\S+ :(?<message>.+)", RegexOptions.Compiled);

        public static readonly Regex Code3Match =
            new Regex(@":\S+\.tmi\.twitch\.tv (?<code>\d{3}) \S+ #\S+ :(?<message>.+)", RegexOptions.Compiled);

        public static readonly Regex JoinMatch = new Regex(@":\S+!\S+@(?<name>\S+)\.tmi\.twitch\.tv JOIN #\S+",
            RegexOptions.Compiled);

        public static readonly Regex PartMatch = new Regex(@":\S+!\S+@(?<name>\S+)\.tmi\.twitch\.tv PART #\S+",
            RegexOptions.Compiled);

        public static readonly Regex GlobalUserStateMatch =
            new Regex(
                @"@color=(?<color>#\d{6})?;display-name=(?<name>\S+);emote-sets=(?<emotesets>\S+);turbo=(?<isturbo>\d);user-id=(?<userid>\d+);user-type=(?<usertype>\S*) :tmi\.twitch\.tv GLOBALUSERSTATE",
                RegexOptions.Compiled);

        public static readonly Regex UserStateMatch =
            new Regex(
                @"@color=(?<color>#\d{6})?;display-name=(?<name>\S+);emote-sets=(?<emotesets>\S+);subscriber=(?<issub>\d);turbo=(?<isturbo>\d);user-type=(?<usertype>\S*) :tmi\.twitch\.tv USERSTATE #\S+",
                RegexOptions.Compiled);

        public static readonly Regex RoomStateMatch =
            new Regex(
                @"@broadcaster-lang=(?<lang>\S+)?;r9k=(?<isr9k>\d);slow=(?<isslow>\d+);subs-only=(?<issub>\d) :tmi\.twitch\.tv ROOMSTATE #\S+",
                RegexOptions.Compiled);

        public static readonly Regex NoticeMatch =
            new Regex(@"@msg-id=(?<msgid>\S+) :tmi\.twitch\.tv NOTICE #\S+ :(?<message>.+)", RegexOptions.Compiled);


        public static ConcurrentQueue<Paragraph> ChatTextBoxQueue = new ConcurrentQueue<Paragraph>();
        public static ConcurrentQueue<Paragraph> LogTextBoxQueue = new ConcurrentQueue<Paragraph>();
        public static Dispatcher WindowDispatcher;
        public static IrcClient.IrcClient IrcClient;
        public static Configuration OptionsConfig;
        public static Paragraph MessageStart;
        public static TextBox ChatStatusBox;
        public static bool Running;

        public static Image EmoteFromUrl(string url)
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

        public static void AlignText(RichTextBox textBox)
        {
            if (textBox.Dispatcher.CheckAccess())
            {
                new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).ApplyPropertyValue(
                    Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
            }
            else
            {
                OnUi(
                    delegate
                    {
                        new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).ApplyPropertyValue(
                            Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
                    });
            }
        }

        public static Brush RgbToBrush(string rgb)
        {
            var obj = ColorConverter.ConvertFromString(rgb.Length > 0 ? rgb : "#000000");
            return obj == null ? new SolidColorBrush(Colors.Black) : new SolidColorBrush((Color) obj);
        }

        public static Image FromResource(string path)
        {
            var manifestResourceStream = Assembly.GetEntryAssembly().GetManifestResourceStream(path);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = manifestResourceStream;
            bitmapImage.EndInit();
            var image = new Image
            {
                Source = bitmapImage,
                MaxHeight = bitmapImage.PixelHeight,
                MaxWidth = bitmapImage.PixelHeight
            };
            return image;
        }
    }
}