namespace AccessSharingServer
{
    public class Server
    {
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static ServerSettings Settings = new ServerSettings();
        static TcpListener listener;

        public static void Start() 
        {
            LoadSettings();
            InitializeServerData();
            listener = new TcpListener(IPAddress.Any, Settings.Port);
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        }

        public static void Stop()
        {
            listener.Stop();
        }

        public static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient tcpClient = listener.EndAcceptTcpClient(_result);
            listener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            for(int i = 1; i <= Settings.MaxClients; i++)
            {
                if (clients[i].TcpClient == null)
                {
                    clients[i].Connect(tcpClient);
                    return;
                }
            }
        }

        public static void Disconnect(int index)
        {
            clients[index].Disconnect();
        }
        public static void DisconnectAll()
        {
            for (int i = 1; i < Settings.MaxClients; i++)
            {
                if (clients[i].TcpClient != null)
                {
                    clients[i].Disconnect();
                }
            }
        }

        public static void SaveSettings()
        {
            if (File.Exists("settings.xml")) File.Delete("settings.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ServerSettings));
            using (FileStream fs = new FileStream("settings.xml", FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, Settings);
            }
        }

        public static void LoadSettings()
        {
            if (!File.Exists("settings.xml")) return;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ServerSettings));
            using (FileStream fs = new FileStream("settings.xml", FileMode.OpenOrCreate))
            {
                ServerSettings? settings = xmlSerializer.Deserialize(fs) as ServerSettings;
                if (settings == null)
                {
                    Console.WriteLine("Failed to load settings.xml");
                    return;
                }
                Settings = settings;
            }
        }

        static void InitializeServerData()
        {
            for(int i = 1; i <= Settings.MaxClients; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.Handshake, ServerHandle.HandshakeReceived},
                {(int)ClientPackets.AccountListRequest, ServerHandle.AccountListRequestReceived},
                {(int)ClientPackets.RoleListRequest, ServerHandle.RoleListRequestReceived},
                {(int)ClientPackets.ChangeUsernameRequest, ServerHandle.ChangeUsernameRequestReceived},
                {(int)ClientPackets.DocumentsListRequest, ServerHandle.DocumentsListRequestReceived},
                {(int)ClientPackets.File, ServerHandle.FileReceived},
                {(int)ClientPackets.FilePart, ServerHandle.FilePartReceived},
                {(int)ClientPackets.FileDeleteRequest, ServerHandle.FileDeleteRequestReceived},
                {(int)ClientPackets.DownloadFileRequest, ServerHandle.FileDownloadRequestReceived},
                {(int)ClientPackets.FilePartRequest, ServerHandle.FilePartsRequestReceived},
                {(int)ClientPackets.AddUserRole, ServerHandle.AddUserRoleRequestReceived},
                {(int)ClientPackets.RemoveUserRole, ServerHandle.RemoveUserRoleRequestReceived},
            };
        }
    }
}
