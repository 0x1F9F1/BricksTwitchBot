using System.IO;
using System.Net.Sockets;

namespace BricksTwitchBot.Irc
{
    public class IrcClient
    {
        private readonly string _channel;
        private readonly string _username;

        public StreamReader StreamReader;
        public StreamWriter StreamWriter;
        public TcpClient TcpClient;

        public bool Connected => TcpClient.Connected;

        public IrcClient(string username, string oauth, string channel)
        {
            TcpClient = new TcpClient("irc.twitch.tv", 6667);
            StreamReader = new StreamReader(TcpClient.GetStream());
            StreamWriter = new StreamWriter(TcpClient.GetStream())
            {
                AutoFlush = true
            };
            _channel = channel.ToLower();
            _username = username.ToLower();
            WriteOther("CAP REQ :twitch.tv/membership");
            WriteOther("CAP REQ :twitch.tv/commands");
            WriteOther("CAP REQ :twitch.tv/tags");
            WriteOther($"PASS {oauth}");
            WriteOther($"NICK {_username}");
            WriteOther($"JOIN #{_channel}");
        }

        public string ReadData()
        {
            try
            {
                return StreamReader.ReadLine();
            }
            catch (IOException)
            {
                return null;
            }
        }

        public void WriteOther(string other)
        {
            StreamWriter.WriteLine(other);
        }

        public void WriteMessage(string message)
        {
            StreamWriter.WriteLine("PRIVMSG #{0} :{1}", _channel, message);
        }

        public void Disconnect()
        {
            TcpClient.Close();
        }
    }
}
