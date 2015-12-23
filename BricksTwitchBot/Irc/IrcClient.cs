using System.IO;
using System.Net.Sockets;

namespace BricksTwitchBot.IrcClient
{
    public class IrcClient
    {
        private readonly string Channel;
        private readonly string Username;
        public StreamReader StreamReader;
        public StreamWriter StreamWriter;
        public TcpClient TcpClient;

        public bool Connected
        {
            get
            {
                return TcpClient.Connected;
            }
        }

        public IrcClient(string username, string oauth, string channel)
        {
            TcpClient = new TcpClient("irc.twitch.tv", 6667);
            StreamReader = new StreamReader((Stream)TcpClient.GetStream());
            StreamWriter = new StreamWriter((Stream)TcpClient.GetStream())
            {
                AutoFlush = true
            };
            Channel = channel.ToLower();
            Username = username.ToLower();
            WriteOther("CAP REQ :twitch.tv/membership");
            WriteOther("CAP REQ :twitch.tv/commands");
            WriteOther("CAP REQ :twitch.tv/tags");
            WriteOther("USER " + Username + " irc twitch");
            WriteOther("PASS " + oauth);
            WriteOther("NICK " + Username);
            WriteOther("JOIN #" + Channel);
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

        public void WriteOther(string o)
        {
            StreamWriter.WriteLine(o);
        }

        public void WriteMessage(string m)
        {
            StreamWriter.WriteLine("PRIVMSG #{0} :{1}", (object)Channel, (object)m);
        }

        public void Disconnect()
        {
            TcpClient.Close();
        }
    }
}
