using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfApplication1.IrcClient
{
    public static class DataHandler
    {
        private const string MessageMatch =
            @"^@color=(?<color>#\w{6})?;display-name=(?<name>[^;]+)?;emotes=(?<emote>[^;]*);(?:sent-ts=\d+;)?subscriber=(?<issub>\d);(?:tmi-sent-ts=\d+;)?turbo=(?<isturbo>\d);user-id=(?<userid>\d+);?user-type=(?<usertype>\S*) :(?<secondname>\S+)!\S+@\S+\.tmi\.twitch\.tv PRIVMSG #\w+ :(?<message>.+)$";

        private const string ModeMatch = @":jtv MODE #\S+ (?<change>[+-])o (?<user>\S+)";
        private const string PingMatch = @"PING :(?<ip>\S+)";
        private const string TimeoutMatch = @":tmi\.twitch\.tv CLEARCHAT #\S+ :(?<user>\S+)";

        private const string NotifyMatch =
            @":twitchnotify!twitchnotify@twitchnotify.tmi.twitch.tv PRIVMSG #\S+ :(?<message>.+)";

        private const string CapAckMatch = @":tmi\.twitch\.tv CAP \* ACK :(?<cap>.+)";
        private const string CodeMatch = @":tmi\.twitch\.tv (?<code>\d{3}) \S+ :(?<message>.+)";
        private const string Code2Match = @":\S+\.tmi\.twitch\.tv (?<code>\d{3}) \S+ = #\S+ :(?<message>.+)";
        private const string Code3Match = @":\S+\.tmi\.twitch\.tv (?<code>\d{3}) \S+ #\S+ :(?<message>.+)";
        private const string JoinMatch = @":\S+!\S+@(?<name>\S+)\.tmi\.twitch\.tv JOIN #\S+";
        private const string PartMatch = @":\S+!\S+@(?<name>\S+)\.tmi\.twitch\.tv PART #\S+";

        private const string GlobalUserStateMatch =
            "@color=(?<color>#\\d{6})?;display-name=(?<name>\\S+);emote-sets=(?<emotesets>\\S+);turbo=(?<isturbo>\\d);user-id=(?<userid>\\d+);user-type=(?<usertype>\\S*) :tmi\\.twitch\\.tv GLOBALUSERSTATE";

        private const string UserStateMatch =
            "@color=(?<color>#\\d{6})?;display-name=(?<name>\\S+);emote-sets=(?<emotesets>\\S+);subscriber=(?<issub>\\d);turbo=(?<isturbo>\\d);user-type=(?<usertype>\\S*) :tmi\\.twitch\\.tv USERSTATE #\\S+";

        private const string RoomStateMatch =
            "@broadcaster-lang=(?<lang>\\S+)?;r9k=(?<isr9k>\\d);slow=(?<isslow>\\d+);subs-only=(?<issub>\\d) :tmi\\.twitch\\.tv ROOMSTATE #\\S+";

        private const string NoticeMatch = "@msg-id=(?<msgid>\\S+) :tmi\\.twitch\\.tv NOTICE #\\S+ :(?<message>.+)";

        public static void HandleData(string data)
        {
            Match match;
            if ((match = Regex.Match(data, MessageMatch)).Success)
            {
                MessageHandler.HandleMessage(data);
            }
            else if ((match = Regex.Match(data, ModeMatch)).Success)
            {
                Globals.OnUi(() =>
                {
                    var paragraph = new Paragraph();
                    var inlines = paragraph.Inlines;
                    var run = new Run("[Modded]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(
                        new Run(
                            $": {match.Groups["user"].Value} was {(match.Groups["change"].Value == "+" ? "modded" : "unmodded")}"));
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, PingMatch)).Success)
            {
                Globals.IrcClient.WriteOther($"PONG {match.Groups["ip"].Value}");
            }
            else if ((match = Regex.Match(data, TimeoutMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines = paragraph.Inlines;
                    var run = new Run("[Timeout]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(new Run($": {match.Groups["user"].Value} was timed out"));
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, NotifyMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines = paragraph.Inlines;
                    var run = new Run("[Notification]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(new Run($": {match.Groups["message"].Value}"));
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, CapAckMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines = paragraph.Inlines;
                    var run = new Run("[CapAck]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(new Run($": {match.Groups["cap"].Value}"));
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, CodeMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines1 = paragraph.Inlines;
                    var run1 = new Run("[Code]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines1.Add(run1);
                    paragraph.Inlines.Add(new Run(": "));
                    var inlines2 = paragraph.Inlines;
                    var run2 = new Run($"{match.Groups["code"].Value}")
                    {
                        Foreground = new SolidColorBrush(Colors.Red)
                    };
                    inlines2.Add(run2);
                    paragraph.Inlines.Add(new Run(": "));
                    paragraph.Inlines.Add($"{match.Groups["message"].Value}");
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, Code2Match)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines1 = paragraph.Inlines;
                    var run1 = new Run("[Code]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines1.Add(run1);
                    paragraph.Inlines.Add(new Run(": "));
                    var inlines2 = paragraph.Inlines;
                    var run2 = new Run($"{match.Groups["code"].Value}")
                    {
                        Foreground = new SolidColorBrush(Colors.Red)
                    };
                    inlines2.Add(run2);
                    paragraph.Inlines.Add(new Run(": "));
                    paragraph.Inlines.Add($"{match.Groups["message"].Value}");
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, Code3Match)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines1 = paragraph.Inlines;
                    var run1 = new Run("[Code]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines1.Add(run1);
                    paragraph.Inlines.Add(new Run(": "));
                    var inlines2 = paragraph.Inlines;
                    var run2 = new Run($"{match.Groups["code"].Value}")
                    {
                        Foreground = new SolidColorBrush(Colors.Red)
                    };
                    inlines2.Add(run2);
                    paragraph.Inlines.Add(new Run(": "));
                    paragraph.Inlines.Add($"{match.Groups["message"].Value}");
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, JoinMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines = paragraph.Inlines;
                    var run = new Run("[Join]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(new Run($": {match.Groups["name"].Value} joined the chat"));
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, PartMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var inlines = paragraph.Inlines;
                    var run = new Run("[Part]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(
                        new Run(
                            $": {match.Groups["name"].Value} left the chat"));
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Regex.Match(data, GlobalUserStateMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var str = match.Groups["usertype"].Value;
                    if (str == "mod")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Moderator.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (str == "global_mod")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.GlobalModerator.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (str == "admin")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Admin.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (str == "staff")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Staff.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (match.Groups["isturbo"].Value == "1")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Turbo.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    var rgb = match.Groups["color"].Success ? match.Groups["color"].Value : "#000000";
                    var text = match.Groups["name"].Value.Replace("\\s", " ");
                    var inlines = paragraph.Inlines;
                    var run = new Run(text)
                    {
                        Foreground = Globals.RgbToBrush(rgb),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(new Run(": "));
                    Globals.MessageStart = paragraph;
                });
            }
            else if ((match = Regex.Match(data, UserStateMatch)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    var str = match.Groups["usertype"].Value;
                    if (str == "mod")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Moderator.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (str == "global_mod")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.GlobalModerator.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (str == "admin")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Admin.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (str == "staff")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Staff.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (match.Groups["isturbo"].Value == "1")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Turbo.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (match.Groups["issub"].Value == "1")
                    {
                        var image = Globals.FromResource("WpfApplication1.Images.Subscriber.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    var rgb = match.Groups["color"].Success ? match.Groups["color"].Value : "#000000";
                    var text = match.Groups["name"].Value.Replace("\\s", " ");
                    var inlines = paragraph.Inlines;
                    var run = new Run(text)
                    {
                        Foreground = Globals.RgbToBrush(rgb),
                        FontWeight = FontWeights.Bold
                    };
                    inlines.Add(run);
                    paragraph.Inlines.Add(new Run(": "));
                    Globals.MessageStart = paragraph;
                });
            }
            else if ((match = Regex.Match(data, RoomStateMatch)).Success)
            {
                Globals.OnUi(() =>
                {
                    var list = new List<string>();
                    if (match.Groups["issub"].Value == "1")
                    {
                        list.Add("Sub");
                    }
                    if (match.Groups["isr9k"].Value == "1")
                    {
                        list.Add("r9k");
                    }
                    if (match.Groups["isslow"].Value != "0")
                    {
                        list.Add($"Slow: {match.Groups["isslow"].Value}");
                    }
                    Globals.ChatStatusBox.Text = string.Join(",", list);
                });
            }
            else if ((match = Regex.Match(data, NoticeMatch)).Success)
            {
                Globals.OnUi(() =>
                {
                    var paragraph = new Paragraph();
                    var inlines = paragraph.Inlines;
                    var run = new Run($": {match.Groups["message"].Value}")
                    {
                        Foreground = new SolidColorBrush(Colors.Gray)
                    };
                    inlines.Add(run);
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else
            {
                Globals.OnUi(() =>
                {
                    var concurrentQueue = Globals.ChatTextBoxQueue;
                    var paragraph = new Paragraph(new Run(data)
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    });
                    concurrentQueue.Enqueue(paragraph);
                });
            }
        }
    }
}