using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
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

                    var emotes = match.Groups["emote"].Value;
                    var list = new List<Emote>();
                    if (emotes != "")
                    {
                        string[] emoteList;
                        if (!emotes.Contains("/"))
                        {
                            emoteList = new[]
                            {
                                emotes
                            };
                        }
                        else
                        {
                            emoteList = emotes.Split('/');
                        }
                        foreach (var rawEmote in emoteList)
                        {
                            var emoteParts = rawEmote.Split(':');
                            var emoteIndexString = emoteParts[1];
                            string[] emoteIndexes;
                            if (!emoteIndexString.Contains(","))
                            {
                                emoteIndexes = new[]
                                {
                                    emoteIndexString
                                };
                            }
                            else
                            {
                                emoteIndexes = emoteIndexString.Split(',');
                            }
                            foreach (var index in emoteIndexes)
                            {
                                var indexSplit = index.Split('-');
                                var emote = new Emote
                                {
                                    EmoteName = emoteParts[0],
                                    Indexes = new List<int>()
                                };
                                for (var i = int.Parse(indexSplit[0]); i <= int.Parse(indexSplit[1]); ++i)
                                {
                                    emote.Indexes.Add(i);
                                }
                                list.Add(emote);
                            }
                        }
                    }
                    var message = match.Groups["message"].Value;
                    var finalMessage = "";
                    for (int i = 0; i < message.Length; ++i)
                    {
                        if (!list.Exists(s => s.Indexes.Contains(i)))
                        {
                            finalMessage += message[i].ToString();
                        }
                        else if (list.Exists(s => s.Indexes[0] == i))
                        {

                            paragraph.Inlines.Add(new Run(finalMessage));
                            finalMessage = "";
                            var image =
                                Globals.ImageFromUrl(
                                    $"https://static-cdn.jtvnw.net/emoticons/v1/{ list.First(s => s.Indexes[0] == i).EmoteName}/1.0");
                            paragraph.Inlines.Add(image);
                        }
                    }

                    paragraph.Inlines.Add(new Run(finalMessage));

                    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                });
            }
        }

        private struct Emote
        {
            public string EmoteName;
            public List<int> Indexes;
        }
    }
}