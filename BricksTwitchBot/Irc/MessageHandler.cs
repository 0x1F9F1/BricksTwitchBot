using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace BricksTwitchBot.Irc
{
    public static class MessageHandler
    {
        public static void HandleMessage(string data)
        {
            Match match;
            if ((match = Globals.MessageMatch.Match(data)).Success)
            {
                Globals.OnUi(delegate
                {
                    var paragraph = new Paragraph();
                    switch (match.Groups["usertype"].Value)
                    {
                        case "mod":
                            {
                                paragraph.Inlines.Add(Globals.FromResource("Images.Moderator.png"));
                                paragraph.Inlines.Add(new Run(" "));
                            }
                            break;
                        case "admin":
                            {
                                paragraph.Inlines.Add(Globals.FromResource("Images.Admin.png"));
                                paragraph.Inlines.Add(new Run(" "));
                            }
                            break;
                        case "global_mod":
                            {
                                paragraph.Inlines.Add(Globals.FromResource("Images.GlobalModerator.png"));
                                paragraph.Inlines.Add(new Run(" "));
                            }
                            break;
                        case "staff":
                            {
                                paragraph.Inlines.Add(Globals.FromResource("Images.Staff.png"));
                                paragraph.Inlines.Add(new Run(" "));
                            }
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
                                .Replace(@"\s", " ")) // Replace \s (space) with a space (only used for usernames)
                        {
                            Foreground = Globals.RgbToBrush(match.Groups["color"].Value), // Remember to use the color of their name
                            FontWeight = FontWeights.Bold
                        });

                    paragraph.Inlines.Add(new Run(": "));

                    var message = new Paragraph(new Run(match.Groups["message"].Value));
                    //var message = new Run(match.Groups["message"].Value);
                    var emoteString = match.Groups["emote"].Value;

                    var emoteList = new List<Emote>();

                    if (!string.IsNullOrWhiteSpace(emoteString))
                    {
                        var emoteStrings = emoteString.Split('/');
                        foreach (var splitEmoteString in emoteStrings)
                        {
                            var emoteSplitIdAndIndexs = splitEmoteString.Split(':');

                            foreach (var emoteIndexBeginAndEnd in emoteSplitIdAndIndexs[1].Split(','))
                            {
                                var emoteIndexBeginAndEndSplit = emoteIndexBeginAndEnd.Split('-');

                                emoteList.Add(new Emote
                                {
                                    ID = emoteSplitIdAndIndexs[0],
                                    Start = int.Parse(emoteIndexBeginAndEndSplit[0]),
                                    End = int.Parse(emoteIndexBeginAndEndSplit[1]) + 1
                                });
                            }
                        }

                        var range = new TextRange(message.ContentStart, message.ContentEnd);

                        foreach (Emote emote in emoteList.OrderBy(s => s.Start).Reverse())
                        {
                            var image = Globals.ImageFromUrl($"https://static-cdn.jtvnw.net/emoticons/v1/{emote.ID}/1.0");

                            var imageStart = range.Start.GetPositionAtOffset(emote.Start);
                            var imageEnd = range.Start.GetPositionAtOffset(emote.End);
                            var imageRange = new TextRange(imageStart, imageEnd);
                            imageRange.Text = "";
                            InlineUIContainer imageContainer = new InlineUIContainer(image, imageStart);
                        }
                    }

                    paragraph.Inlines.AddRange(message.Inlines.ToArray());

                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
        }

        private struct Emote
        {
            public string ID;
            public int Start;
            public int End;

        }
    }
}