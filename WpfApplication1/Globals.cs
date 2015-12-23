using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SharpConfig;

namespace WpfApplication1
{
    internal static class Globals
    {
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
            BitmapImage bitmapImage = new BitmapImage(new Uri(url));
            Image image = new Image
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
                new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
            else
                OnUi(
                    delegate
                    {
                        new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).ApplyPropertyValue(
                    Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
                    });
        }

        public static Brush RgbToBrush(string rgb)
        {
            var obj = ColorConverter.ConvertFromString(rgb.Length > 0 ? rgb : "#000000");
            return obj == null ? new SolidColorBrush(Colors.Black) : new SolidColorBrush((Color)obj);
        }

        public static Image FromResource(string path)
        {
            Stream manifestResourceStream = Assembly.GetEntryAssembly().GetManifestResourceStream(path);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = manifestResourceStream;
            bitmapImage.EndInit();
            Image image = new Image
            {
                Source = bitmapImage,
                MaxHeight = bitmapImage.PixelHeight,
                MaxWidth = bitmapImage.PixelHeight
            };
            return image;
        }
    }
}
