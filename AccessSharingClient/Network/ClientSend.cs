using Network;

namespace AccessSharingClient
{
    public class ClientSend
    {
        static void SendData(Packet packet)
        {
            packet.WriteLenght();
            Client.Instance.SendData(packet);
        }

        //public static void Example(string msg)
        //{
        //    using (Packet packet = new Packet((int)ClientPackets.Example))
        //    {
        //        packet.Write(msg);

        //        SendData(packet);
        //    }
        //}

        public static void Handshake()
        {
            using (Packet packet = new Packet((int)ClientPackets.Handshake))
            {
                packet.Write(Client.Instance.AccountLogin);
                packet.Write(Client.Instance.Password);
                packet.Write(Client.ServerPassword);

                SendData(packet);
            }
        }

        public static void AccountListRequest()
        {
            using (Packet packet = new Packet((int)ClientPackets.AccountListRequest))
            {
                SendData(packet);
            }
        }

        public static void RoleListRequest()
        {
            using (Packet packet = new Packet((int)ClientPackets.RoleListRequest))
            {
                SendData(packet);
            }
        }

        public static void ChangeUsername(string login, string name)
        {
            using (Packet packet = new Packet((int)ClientPackets.ChangeUsernameRequest))
            {
                packet.Write(login);
                packet.Write(name);

                SendData(packet);
            }
        }

        public static void DocumentsListRequest()
        {
            using (Packet packet = new Packet((int)ClientPackets.DocumentsListRequest))
            {
                SendData(packet);
            }
        }

        public static void File(FileInfo file, int secLevel, string tag)
        {
            using (Packet packet = new Packet((int)ClientPackets.File))
            {
                packet.Write(file.Name);
                packet.Write(file.FullName);
                packet.Write(secLevel);
                packet.Write(tag);

                SendData(packet);
            }
        }

        public static void FilePart(string fileName, int part)
        {
            using (Packet packet = new Packet((int)ClientPackets.FilePart))
            {
                packet.Write(fileName);
                byte[]? buffer = FileManager.GetFilePart(fileName, part);
                if (buffer == null)
                {
                    packet.Write(true);
                    SendData(packet);
                    FileManager.RemoveData(fileName);
                    return;
                }
                packet.Write(false);
                packet.Write(part);
                packet.Write(buffer.Length);
                packet.Write(buffer);
                SendData(packet);
            }
        }

        public static void DeleteFileRequest(string filename)
        {
            using (Packet packet = new Packet((int)ClientPackets.FileDeleteRequest))
            {
                packet.Write(filename);
                SendData(packet);
            }
        }

        public static void DownloadFileRequest(string filename)
        {
            using (Packet packet = new Packet((int)ClientPackets.DownloadFileRequest))
            {
                packet.Write(filename);
                SendData(packet);
            }
        }

        public static void FilePartRequest(string filename, int part)
        {
            using (Packet packet = new Packet((int)ClientPackets.FilePartRequest))
            {
                packet.Write(filename);
                packet.Write(part);

                SendData(packet);
            }
        }

        public static void AddUserRole(string userLogin, string roleName)
        {
            using (Packet packet = new Packet((int)ClientPackets.AddUserRole))
            {
                packet.Write(userLogin);
                packet.Write(roleName);

                SendData(packet);
            }
        }

        public static void RemoveUserRole(string userLogin, string roleName)
        {
            using (Packet packet = new Packet((int)ClientPackets.RemoveUserRole))
            {
                packet.Write(userLogin);
                packet.Write(roleName);

                SendData(packet);
            }
        }
    }
}
