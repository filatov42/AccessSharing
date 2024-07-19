namespace AccessSharingServer
{
    public class Client
    {
        static int dataBufferSize = 4096;

        public string AccountLogin = string.Empty;
        public int Id;
        public TcpClient TcpClient;

        NetworkStream stream;
        byte[] receiveBuffer;
        Packet receivedData;

        public Client(int Id)
        {
            this.Id = Id;
        }

        public void Connect(TcpClient client)
        {
            TcpClient = client;
            TcpClient.ReceiveBufferSize = dataBufferSize;
            TcpClient.SendBufferSize = dataBufferSize;

            stream = TcpClient.GetStream();
            receiveBuffer = new byte[dataBufferSize];
            receivedData = new Packet();

            ServerSend.Handshake(Id);

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void Disconnect()
        {
            if (TcpClient != null)
            {
                TcpClient.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                TcpClient = null;
                AccountLogin = string.Empty;
            }
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
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if(byteLength <= 0)
                {
                    Server.clients[Id].Disconnect();
                }
                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);
                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch(Exception e)
            {
                Server.clients[Id].Disconnect();
                Console.WriteLine(e.Message);
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
                        Server.packetHandlers[packetId](Id, packet);
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
    }
}
