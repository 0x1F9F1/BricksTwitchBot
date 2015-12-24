﻿using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BricksTwitchBot.Irc;
using SharpConfig;

namespace BricksTwitchBot
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Globals.Running = true;
            if (!File.Exists("TwitchBot.Ini"))
            {
                using (var manifestResourceStream =
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("TwitchBot.ini"))
                {
                    using (var fileStream = new FileStream("TwitchBot.ini", FileMode.Create, FileAccess.Write))
                    {
                        manifestResourceStream?.CopyTo(fileStream);
                    }
                }
            }

            Globals.OptionsConfig = Configuration.LoadFromFile("TwitchBot.ini");
            Globals.WindowDispatcher = Dispatcher;
            Globals.ChatStatusBox = ChatStatusBox;

            Globals.FontFamily = new FontFamily("Helvetica Neue");

            Globals.LogWriter = File.AppendText("Logging.txt");
            Globals.LogWriter.AutoFlush = true;

            UsernameInputBox.Text = Globals.OptionsConfig["Options"]["Username"].StringValue;
            OauthInputBox.Password = Globals.OptionsConfig["Options"]["Oauth"].StringValue;
            ChannelToJoinInput.Text = Globals.OptionsConfig["Options"]["ChannelToJoin"].StringValue;
            AutoConnectCheckBox.IsChecked = Globals.OptionsConfig["Options"]["Auto-Connect"].BoolValue;

            new Thread(ChatBoxUpdateThread)
            {
                IsBackground = true
            }.Start();

            new Thread(LogBoxUpdateThread)
            {
                IsBackground = true
            }.Start();

            if (AutoConnectCheckBox.IsChecked ?? false)
            {
                Connect();
            }
        }

        private void StartListenerClient()
        {
            Globals.ListenIrc = new IrcClient(null, null, Globals.OptionsConfig["Options"]["ChannelToJoin"].StringValue);

            while (Globals.ListenIrc?.Connected ?? false)
            {
                var data = Globals.ListenIrc.ReadData();
                if (data != null)
                {
                    DataHandler.HandleData(data);
                }
                else
                {
                    Thread.Sleep(250);
                }
            }

            Disconnect();
        }
        private void StartChatClient()
        {
            Globals.ChatIrc = new IrcClient(
               Globals.OptionsConfig["Options"]["Username"].StringValue,
               Globals.OptionsConfig["Options"]["Oauth"].StringValue,
               Globals.OptionsConfig["Options"]["ChannelToJoin"].StringValue);

            while (Globals.ChatIrc?.Connected ?? false)
            {
                var data = Globals.ChatIrc.ReadData();
                if (data == null)
                {
                    Thread.Sleep(250);
                }
            }

            Disconnect();
        }

        private void ChatBoxUpdateThread()
        {
            while (Globals.Running)
            {
                Paragraph paragraph;
                if (Globals.ChatTextBoxQueue.TryDequeue(out paragraph))
                {
                    AddToTextBox(ChatTextBox, paragraph, true);
                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void LogBoxUpdateThread()
        {
            while (Globals.Running)
            {
                Paragraph paragraph;
                if (Globals.LogTextBoxQueue.TryDequeue(out paragraph))
                {
                    AddToTextBox(LogTextBox, paragraph, true);
                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void AddToTextBox(RichTextBox textbox, Paragraph paragraph, bool scroll)
        {
            paragraph.Dispatcher.Invoke(delegate
            {
                paragraph.Inlines.InsertBefore(paragraph.Inlines.FirstInline,
                new Run($"{DateTime.Now:t} ")
                {
                    FontSize = 9.0,
                    Foreground = new SolidColorBrush(Colors.Gray)
                });
                paragraph.FontFamily = Globals.FontFamily;

                var textRange = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
                textRange.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Center);

                Globals.LogWriter.WriteLine($"{ textbox.Name } | {textRange.Text}");

                textbox.Dispatcher.Invoke(delegate
                {
                    textbox.Document.Blocks.Add(paragraph);

                    if (scroll)
                    {
                        if ((!textbox.IsSelectionActive || ChatTextBox.Selection.Text.Length == 0) &&
                            !(textbox.VerticalOffset + textbox.ViewportHeight < ChatTextBox.ExtentHeight - 50.0))
                        {
                            textbox.ScrollToEnd();
                        }
                    }
                });
            });
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ChatButton_Click(sender, null);
                e.Handled = true;
            }
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Globals.ChatIrc?.Connected ?? false) && (Globals.ListenIrc?.Connected ?? false))
            {
                var text = ChatInput.Text;
                if (text.Length > 0)
                {
                    //var paragraph = new Paragraph();
                    //if (Globals.MessageStart != null)
                    //{
                    //    foreach (var inline in Globals.MessageStart)
                    //    {
                    //        paragraph.Inlines.Add(inline);
                    //    }
                    //    paragraph.Inlines.Add(new Run(text));
                    //    Globals.ChatTextBoxQueue.Enqueue(paragraph);
                    //}

                    Globals.ChatIrc.WriteMessage(text);
                    ChatInput.Clear();
                }
            }
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Connect()
        {
            Disconnect();

            new Thread(StartListenerClient)
            {
                IsBackground = true
            }.Start();
            new Thread(StartChatClient)
            {
                IsBackground = true
            }.Start();
        }

        private void Disconnect()
        {
            Globals.ChatIrc?.Disconnect();
            Globals.ListenIrc?.Disconnect();
        }

        private void UsernameInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Username"].SetValue(UsernameInputBox.Text);
            SaveConfig();
        }

        private void OauthInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Oauth"].SetValue(OauthInputBox.Password);
            SaveConfig();
        }

        private void ChannelToJoinInput_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["ChannelToJoin"].SetValue(ChannelToJoinInput.Text);
            SaveConfig();
        }

        private void AutoConnectCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Auto-Connect"].SetValue(AutoConnectCheckBox.IsChecked);
            SaveConfig();
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            Globals.Running = false;
            Disconnect();

            SaveConfig();

            Globals.LogWriter.Flush();
            Globals.LogWriter.Close();

        }

        private void SaveConfig()
        {
            Globals.OptionsConfig.SaveToFile("TwitchBot.ini");
        }
    }
}