using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BricksTwitchBot.IrcClient;
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
                using (
                    var manifestResourceStream =
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("WpfApplication1.TwitchBot.ini"))
                {
                    using (var fileStream = new FileStream("TwitchBot.ini", FileMode.Create, FileAccess.Write))
                    {
                        manifestResourceStream?.CopyTo(fileStream);
                    }
                }
                Thread.Sleep(100);
            }
            Globals.OptionsConfig = Configuration.LoadFromFile("TwitchBot.ini");
            Globals.WindowDispatcher = Dispatcher;
            Globals.ChatStatusBox = ChatStatusBox;
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
            if (Globals.OptionsConfig["Options"]["Auto-Connect"].BoolValue)
            {
                new Thread(StartIrcClient)
                {
                    IsBackground = true
                }.Start();
            }
        }

        private void StartIrcClient()
        {
            Globals.IrcClient = new IrcClient.IrcClient(Globals.OptionsConfig["Options"]["Username"].StringValue,
                Globals.OptionsConfig["Options"]["Oauth"].StringValue,
                Globals.OptionsConfig["Options"]["ChannelToJoin"].StringValue);
            while (Globals.IrcClient.Connected)
            {
                var data = Globals.IrcClient.ReadData();
                if (data != null)
                {
                    DataHandler.HandleData(data);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void ChatBoxUpdateThread()
        {
            var fontFamily = new FontFamily("Helvetica Neue");
            var logger = File.AppendText("Logging.txt");
            logger.AutoFlush = true;
            while (Globals.Running)
            {
                Paragraph paragraph;
                if (Globals.ChatTextBoxQueue.TryDequeue(out paragraph))
                {
                    Globals.OnUi(delegate
                    {
                        var inlines = paragraph.Inlines;
                        var run = new Run($"{DateTime.Now:t} ")
                        {
                            FontSize = 9.0,
                            Foreground = new SolidColorBrush(Colors.Gray)
                        };
                        inlines.InsertBefore(paragraph.Inlines.FirstInline, run);
                        paragraph.FontFamily = fontFamily;
                        var textRange = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
                        textRange.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
                        logger.WriteLine(textRange.Text);
                        ChatTextBox.Document.Blocks.Add(paragraph);
                        if ((!ChatTextBox.IsSelectionActive || ChatTextBox.Selection.Text.Length == 0) &&
                            !(ChatTextBox.VerticalOffset + ChatTextBox.ViewportHeight < ChatTextBox.ExtentHeight - 50.0))
                        {
                            ChatTextBox.ScrollToEnd();
                        }
                    });
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void LogBoxUpdateThread()
        {
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
            if (Globals.IrcClient != null &&  Globals.IrcClient.Connected)
            {
                var paragraph = Globals.MessageStart;
                paragraph.Inlines.Add(new Run(ChatInput.Text));
                Globals.ChatTextBoxQueue.Enqueue(paragraph);
                Globals.IrcClient.WriteMessage(ChatInput.Text);
                ChatInput.Clear();
            }
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.IrcClient != null && Globals.IrcClient.Connected)
            {
                DisconnectButton_Click(sender, null);
            }
            new Thread(StartIrcClient)
            {
                IsBackground = true
            }.Start();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Globals.IrcClient?.Disconnect();
        }

        private void UsernameInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Username"].SetValue(UsernameInputBox.Text);
            Globals.OptionsConfig.SaveToFile("TwitchBot.ini");
        }

        private void OauthInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Oauth"].SetValue(OauthInputBox.Password);
            Globals.OptionsConfig.SaveToFile("TwitchBot.ini");
        }

        private void ChannelToJoinInput_LostFocus(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["ChannelToJoin"].SetValue(ChannelToJoinInput.Text);
            Globals.OptionsConfig.SaveToFile("TwitchBot.ini");
        }

        private void AutoConnectCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Globals.OptionsConfig["Options"]["Auto-Connect"].SetValue(AutoConnectCheckBox.IsChecked);
            Globals.OptionsConfig.SaveToFile("TwitchBot.ini");
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Globals.Running = false;
            Globals.OptionsConfig.SaveToFile("TwitchBot.ini");
        }
    }
}