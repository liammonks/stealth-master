using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;

namespace sm_server
{
    public class SMServer : Plugin
    {

        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);

        public SMServer(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs args)
        {
            args.Client.MessageReceived += OnMessageReceived;
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs args)
        {

        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            using (Message message = args.GetMessage())
            {
                if (message.Tag == 4)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        string text = reader.ReadString();
                        Console.WriteLine(text);
                    }
                }
            }
        }
    }
}
