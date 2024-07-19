namespace AccessSharingClient
{
    public class Client
    {
        public static Client Instance;
        public static int DataBufferSize = 4096;
        public static string Ip = "127.0.0.1";
        public static int Port = 42420;
        public static string ServerPassword = string.Empty;
        public int Id = 0;
        public string AccountLogin = string.Empty;
        public string Password = string.Empty;
        public TcpClient TcpClient;

        NetworkStream stream;
        byte[] receiveBuffer;
        Packet receivedData;


        delegate void PacketHandler(Packet packet);
        static Dictionary<int, PacketHandler> packetHandlers;
        public Client()
        {
        }

        public void Connect()
        {
            InitializeClientData();
            TcpClient = new TcpClient() { ReceiveBufferSize = DataBufferSize, SendBufferSize = DataBufferSize };
            receiveBuffer = new byte[DataBufferSize];
            Instance = this;
            TcpClient.BeginConnect(Ip, Port, ConnectCallback, TcpClient);
        }

        public void Disconnect()
        {
            TcpClient.Close();
            ThreadManager.Execute(new Action(() =>
            {
                RoleLibrary.Clear();
                AccountManager.Clear();
                DocumentManager.Clear();
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        MainWindow w = window as MainWindow;
                        w.connectButton.IsEnabled = true;
                        w.disconnectButton.IsEnabled = false;
                        w.mainTabControl.IsEnabled = false;
                        w.refreshButton.IsEnabled = false;
                    }
                }
            }));
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                TcpClient.EndConnect(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (!TcpClient.Connected)
            {
                return;
            }
            stream = TcpClient.GetStream();
            receivedData = new Packet();
            stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
        }



        public void SendData(Packet packet)
        {
            try
            {
                if (stream != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Instance.Disconnect();
                    return;
                }
                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);
                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLenght() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLenght())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.Execute(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        packetHandlers[packetId](packet);
                    }
                });
                packetLength = 0;

                if (receivedData.UnreadLenght() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }
            return false;
        }

        void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerPackets.Handshake, ClientHandle.HandshakeReceived },
                {(int)ServerPackets.Error, ClientHandle.Error },
                {(int)ServerPackets.Message, ClientHandle.Message },
                {(int)ServerPackets.SuccessfulLogin, ClientHandle.SuccessfulLoginReceived },
                {(int)ServerPackets.Role, ClientHandle.RoleReceived },
                {(int)ServerPackets.RoleDelete, ClientHandle.RoleDeleteReceived },
                {(int)ServerPackets.Account, ClientHandle.AccountReceived },
                {(int)ServerPackets.AccountDelete, ClientHandle.AccountDeleteReceived },
                {(int)ServerPackets.Document, ClientHandle.DocumentReceived },
                {(int)ServerPackets.DocumentDelete, ClientHandle.DocumentDeleteReceived},
                {(int)ServerPackets.FilePartRequest, ClientHandle.FilePartsRequestReceived},
                {(int)ServerPackets.DownloadFileResponse, ClientHandle.DownloadFileResponseReceived},
                {(int)ServerPackets.FilePart, ClientHandle.FilePartReceived},
            };
        }
    }
}
