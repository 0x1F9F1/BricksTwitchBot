using System;
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

            Globals.LogWriter = File.AppendText("Logging.txt");
            Globals.LogWriter.AutoFlush = true;

            Globals.Log("Started Bot");

            if (!File.Exists("TwitchBot.Ini"))
            {
                using (var manifestResourceStream =
                        Globals.GetResourceStream("TwitchBot.ini"))
                {
                    using (var fileStream = new FileStream("TwitchBot.ini", FileMode.Create, FileAccess.Write))
                    {
                        manifestResourceStream?.CopyTo(fileStream);
                    }
                }
                Globals.Log("Created new TwitchBot.ini settings file");
            }

            Globals.OptionsConfig = Configuration.LoadFromFile("TwitchBot.ini");
            Globals.Log("Successfully loaded settings file");
            
            Globals.WindowDispatcher = Dispatcher;
            Globals.ChatStatusBox = ChatStatusBox;

            Globals.FontFamily = new FontFamily("Helvetica Neue");

            UsernameInputBox.Text = Globals.OptionsConfig["Options"]["Username"].StringValue;
            OauthInputBox.Password = Globals.OptionsConfig["Options"]["Oauth"].StringValue;
            ChannelToJoinInput.Text = Globals.OptionsConfig["Options"]["ChannelToJoin"].StringValue;
            AutoConnectCheckBox.IsChecked = Globals.OptionsConfig["Options"]["Auto-Connect"].BoolValue;
            AnonymousCheckBox.IsChecked = Globals.OptionsConfig["Options"]["Anonymous"].BoolValue;
            MaxLinesSlider.Value = Globals.OptionsConfig["Options"]["MaxLines"].DoubleValue;

            new Thread(ChatBoxUpdateThread)
            {
                IsBackground = true
            }.Start();

            new Thread(LogBoxUpdateThread)
            {
                IsBackground = true
            }.Start();

            Globals.Log("Started textbox updater threads");

            if (AutoConnectCheckBox.IsChecked ?? false)
            {
                Connect();
                Globals.Log("Auto Connecting");
            }
        }

        private void StartListenerClient()
        {
            Globals.ListenIrc = new IrcClient(null, null, Globals.OptionsConfig["Options"]["ChannelToJoin"].StringValue);

            Globals.Log("Connected to IRC (Listener)");

            while (Globals.ListenIrc?.Connected ?? false)
            {
                var data = Globals.ListenIrc.ReadData();
                if (data != null)
                {
                    DataHandler.HandleMessage(data);

                    if (!Globals.ChatIrc?.Connected ?? true) // If we are in anonymous mode, handle the other messages
                    {
                        DataHandler.HandleData(data);
                    }
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

            Globals.Log("Connected to IRC (Chat)");

            while (Globals.ChatIrc?.Connected ?? false)
            {
                var data = Globals.ChatIrc.ReadData();
                if (data != null)
                {
                    DataHandler.HandleData(data);
                }
                else
                {
                    Thread.Sleep(100);
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
                    Globals.Log(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);
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
            Globals.OnUi(delegate
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

                textbox.Document.Blocks.Add(paragraph);

                if (scroll)
                {
                    if ((!textbox.IsSelectionActive || ChatTextBox.Selection.Text.Length == 0) &&
                        !(textbox.VerticalOffset + textbox.ViewportHeight < ChatTextBox.ExtentHeight - 50.0))
                    {
                        textbox.ScrollToEnd();
                    }

                    if (textbox.Document.Blocks.Count > MaxLinesSlider.Value)
                    {
                        textbox.Document.Blocks.Remove(textbox.Document.Blocks.FirstBlock);
                    }
                }
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

        private void LogInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                LogButton_Click(sender, null);
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
                    Globals.ChatIrc.WriteMessage(text);
                    ChatInput.Clear();
                }
            }
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            var text = LogInput.Text;
            if (text.Length > 0)
            {
                Globals.LogTextBoxQueue.Enqueue(new Paragraph(new Run(text)));
                LogInput.Clear();
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Connect();
            Globals.ListenIrc?.JoinChannel(ChannelToJoinInput.Text);
            Globals.ChatIrc?.JoinChannel(ChannelToJoinInput.Text);
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Connect()
        {
            if (!Globals.ChatIrc?.Connected ?? true)
            {
                Globals.ChatIrc?.Disconnect();

                if (!AnonymousCheckBox.IsChecked ?? false)
                {
                    new Thread(StartChatClient)
                    {
                        IsBackground = true
                    }.Start();
                }
            }

            if (!Globals.ListenIrc?.Connected ?? true)
            {
                Globals.ListenIrc?.Disconnect();

                new Thread(StartListenerClient)
                {
                    IsBackground = true
                }.Start();
            }
        }

        private void Disconnect()
        {
            Globals.ListenIrc?.Disconnect();
            Globals.ChatIrc?.Disconnect();


            Globals.Log("Disconnected from IRC Connections");
        }

        private void UsernameInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Username"].SetValue(UsernameInputBox.Text);
            Globals.SaveConfig();
        }

        private void OauthInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Oauth"].SetValue(OauthInputBox.Password);
            Globals.SaveConfig();
        }

        private void ChannelToJoinInput_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["ChannelToJoin"].SetValue(ChannelToJoinInput.Text);
            Globals.SaveConfig();
        }

        private void AutoConnectCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Auto-Connect"].SetValue(AutoConnectCheckBox.IsChecked);
            Globals.SaveConfig();
        }

        private void AnonymousCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Anonymous"].SetValue(AnonymousCheckBox.IsChecked);
            Globals.SaveConfig();
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            Globals.Running = false;
            Disconnect();

            Globals.SaveConfig();

            Globals.LogWriter.Flush();
            Globals.LogWriter.Close();
        }

        private void MaxLinesSlider_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["MaxLines"].DoubleValue = MaxLinesSlider.Value;
            Globals.SaveConfig();
        }
    }
}