using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BricksTwitchBot.Irc
{
    public static class DataHandler
    {
        private static readonly List<string> RoomStateList = new List<string>();

        public static void HandleData(string data)
        {
            Match match;
            if ((match = Globals.MessageMatch.Match(data)).Success)
            {
                // Ignored due to 2 connections
            }
            else if ((match = Globals.ModeMatch.Match(data)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run("[Modded]")
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    });
                    paragraph.Inlines.Add(
                        new Run(
                            $": {match.Groups["user"].Value} was {(match.Groups["change"].Value == "+" ? "modded" : "unmodded")}"));
                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Globals.TimeoutMatch.Match(data)).Success)
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
            else if ((match = Globals.NotifyMatch.Match(data)).Success)
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
            else if ((match = Globals.CapAckMatch.Match(data)).Success)
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
                    Globals.LogTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Globals.CodeMatch.Match(data)).Success)
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
                    Globals.LogTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Globals.Code2Match.Match(data)).Success)
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
                    Globals.LogTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Globals.Code3Match.Match(data)).Success)
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
                    Globals.LogTextBoxQueue.Enqueue(paragraph);
                });
            }
            else if ((match = Globals.JoinMatch.Match(data)).Success)
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
            else if ((match = Globals.PartMatch.Match(data)).Success)
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
            else if ((match = Globals.GlobalUserStateMatch.Match(data)).Success)
            {
                // Ignored due to now using 2 connections
            }
            else if ((match = Globals.UserStateMatch.Match(data)).Success)
            {
                // Ignored due to now using 2 connections
            }
            else if ((match = Globals.RoomStateMatch.Match(data)).Success)
            {
                Globals.OnUi(delegate
                {
                    Group group;
                    if ((group = match.Groups["isSub"]).Success)
                    {
                        if (group.Value == "1")
                        {
                            RoomStateList.Add("Sub-Only");
                        }
                        else
                        {
                            RoomStateList.Remove("Sub-Only");
                        }
                    }
                    if ((group = match.Groups["isr9k"]).Success)
                    {
                        if (group.Value == "1")
                        {
                            RoomStateList.Add("R9K");
                        }
                        else
                        {
                            RoomStateList.Remove("R9K");
                        }
                    }
                    if ((group = match.Groups["isSlow"]).Success)
                    {
                        var parsed = int.Parse(group.Value);
                        if (parsed > 0)
                        {
                            RoomStateList.RemoveAll(s => s.StartsWith("Slow"));
                            RoomStateList.Add($"Slow ({parsed})");
                        }
                        else
                        {
                            RoomStateList.RemoveAll(s => s.StartsWith("Slow"));
                        }
                    }
                    Globals.ChatStatusBox.Content = string.Join(" | ", RoomStateList);
                });
            }
            else if ((match = Globals.NoticeMatch.Match(data)).Success)
            {
                Globals.OnUi(delegate
                {
                    Globals.ChatTextBoxQueue.Enqueue(new Paragraph(new Run($": {match.Groups["message"].Value}")
                    {
                        Foreground = new SolidColorBrush(Colors.Gray)
                    }));
                });
            }
            else
            {
                Globals.OnUi(delegate
                {
                    Globals.LogTextBoxQueue.Enqueue(new Paragraph(new Run(data)
                    {
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontWeight = FontWeights.Bold
                    }));
                });
            }
        }

        public static void HandleMessage(string data)
        {
            Match match = Globals.MessageMatch.Match(data);

            if (match.Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();

                    switch (match.Groups["usertype"].Value)
                    {
                        case "mod":
                            paragraph.Inlines.Add(Globals.FromResource("Images.Moderator.png"));
                            paragraph.Inlines.Add(new Run(" "));
                            break;
                        case "admin":
                            paragraph.Inlines.Add(Globals.FromResource("Images.Admin.png"));
                            paragraph.Inlines.Add(new Run(" "));
                            break;
                        case "global_mod":
                            paragraph.Inlines.Add(Globals.FromResource("Images.GlobalModerator.png"));
                            paragraph.Inlines.Add(new Run(" "));
                            break;
                        case "staff":
                            paragraph.Inlines.Add(Globals.FromResource("Images.Staff.png"));
                            paragraph.Inlines.Add(new Run(" "));
                            break;
                    }

                    if (match.Groups["isturbo"].Value.Equals("1"))
                    {
                        paragraph.Inlines.Add(Globals.FromResource("Images.Turbo.png"));
                        paragraph.Inlines.Add(new Run(" "));
                    }
                    if (match.Groups["issub"].Value.Equals("1"))
                    {
                        paragraph.Inlines.Add(Globals.FromResource("Images.Subscriber.png"));
                        paragraph.Inlines.Add(new Run(" "));
                    }

                    paragraph.Inlines.Add(new Run((
                        match.Groups["name"].Success // If getting the first name is a success (the field isn't empty)
                            ? match.Groups["name"].Value // Use the first name
                            : match.Groups["secondname"].Value) // Else use the 'fallback' name
                        .Replace(@"\s", " ")) // Replace \s (space) with a space (only needed for usernames)
                    {
                        Foreground = Globals.RgbToBrush(match.Groups["color"].Value),
                        // Remember to use the color of their name
                        FontWeight = FontWeights.Bold
                    });

                    paragraph.Inlines.Add(new Run(": "));

                    var emoteList = new List<Emote>();

                    var emoteRaw = match.Groups["emote"].Value;

                    if (!string.IsNullOrWhiteSpace(emoteRaw))
                    {
                        foreach (var splitEmoteString in emoteRaw.Split('/')) // Split emotes
                        {
                            var emoteSplitIdAndIndexs = splitEmoteString.Split(':'); // Split ID and Positions

                            foreach (var emoteIndexBeginAndEnd in emoteSplitIdAndIndexs[1].Split(',')) // Split position list
                            {
                                var emoteIndexBeginAndEndSplit = emoteIndexBeginAndEnd.Split('-'); // Split beginning and end

                                var emoteStart = int.Parse(emoteIndexBeginAndEndSplit[0]);
                                var emoteEnd = int.Parse(emoteIndexBeginAndEndSplit[1]) + 1;

                                emoteList.Add(new Emote
                                {
                                    Start = emoteStart,
                                    End = emoteEnd,
                                    Image = Globals.ImageFromUrl($"https://static-cdn.jtvnw.net/emoticons/v1/{ emoteSplitIdAndIndexs[0] }/1.0")
                                });
                            }
                        }

                        var message = new Paragraph(new Run(match.Groups["message"].Value));

                        var range = new TextRange(message.ContentStart, message.ContentEnd);

                        foreach (var emote in emoteList.OrderBy(s => s.Start).Reverse()) // Reverse so positions do not get messed up
                        {
                            var imageStart = range.Start.GetPositionAtOffset(emote.Start);
                            var imageEnd = range.Start.GetPositionAtOffset(emote.End);
                            var imageRange = new TextRange(imageStart, imageEnd) { Text = "" }; // Wipe the text where the image is going to go
                            var imageContainer = new InlineUIContainer(emote.Image, imageStart);
                        }

                        paragraph.Inlines.AddRange(message.Inlines.ToArray()); // Have to call .ToArray() to fix 'collection was edit' bug

                        Globals.ChatTextBoxQueue.Enqueue(paragraph);
                    }
                });
            }
        }

        private struct Emote
        {
            public int Start;
            public int End;
            public Image Image;
        }
    }
}