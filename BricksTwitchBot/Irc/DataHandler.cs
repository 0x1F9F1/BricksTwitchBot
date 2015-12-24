using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
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
                MessageHandler.HandleMessage(data);
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
    }
}