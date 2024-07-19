namespace AccessSharingServer
{
    public class ServerSend
    {
        static void SendData(int toClient, Packet packet)
        {
            packet.WriteLenght();
            Server.clients[toClient].SendData(packet);
        }
        static void SendData(Packet packet)
        {
            packet.WriteLenght();
            for (int i = 1; i < Server.clients.Count; i++)
            {
                Server.clients[i].SendData(packet);
            }
        }

        //public static void Example(int toClient, string msg)
        //{
        //    using (Packet packet = new Packet((int)ServerPackets.Example))
        //    {
        //        packet.Write(msg);
        //        packet.Write(toClient);

        //        SendData(toClient, packet);
        //    }
        //}

        public static void Handshake(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.Handshake))
            {
                packet.Write(toClient);

                SendData(toClient, packet);
            }
        }

        public static void SuccessfulLogin(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.SuccessfulLogin))
            {
                SendData(toClient, packet);
            }
        }

        public static void Error(int toClient, string message, bool disconnect)
        {
            using (Packet packet = new Packet((int)ServerPackets.Error))
            {
                packet.Write(message);
                packet.Write(disconnect);

                SendData(toClient, packet);
            }
        }

        public static void Message(int toClient, string message)
        {
            using (Packet packet = new Packet((int)ServerPackets.Message))
            {
                packet.Write(message);

                SendData(toClient, packet);
            }
        }

        public static void Account(int toClient, Account account)
        {
            using (Packet packet = new Packet((int)ServerPackets.Account))
            {
                packet.Write(account.Name);
                packet.Write(account.Login);

                List<string> roles = account.RoleManager.GetRoles();
                packet.Write(roles.Count);
                foreach (string role in roles)
                {
                    packet.Write(role);
                }

                SendData(toClient, packet);
            }
        }

        public static void Account(Account account)
        {
            using (Packet packet = new Packet((int)ServerPackets.Account))
            {
                packet.Write(account.Name);
                packet.Write(account.Login);

                List<string> roles = account.RoleManager.GetRoles();
                packet.Write(roles.Count);
                foreach (string role in roles)
                {
                    packet.Write(role);
                }

                SendData(packet);
            }
        }

        public static void AccountList(int toClient)
        {
            foreach(Account account in AccountManager.GetAccounts())
            {
                Account(toClient, account);
            }
        }
        public static void RoleList(int toClient)
        {
            List<Role> roles = RoleLibrary.GetRolesList();
            foreach (Role r in roles)
            {
                Role(toClient, r);
            }
        }
        public static void Role(int toClient, Role r)
        {
            using (Packet packet = new Packet((int)ServerPackets.Role))
            {
                packet.Write(r.Name);
                packet.Write(r.Color);
                packet.Write(r.AccessLevel);

                packet.Write(r.DataTags.Count);
                foreach (string s in r.DataTags)
                {
                    packet.Write(s);
                }

                packet.Write(r.Permissions.Count);
                foreach (Permission p in r.Permissions)
                {
                    packet.Write((int)p);
                }

                SendData(toClient, packet);
            }
        }
        public static void Role(Role r)
        {
            using (Packet packet = new Packet((int)ServerPackets.Role))
            {
                packet.Write(r.Name);
                packet.Write(r.Color);
                packet.Write(r.AccessLevel);

                packet.Write(r.DataTags.Count);
                foreach (string s in r.DataTags)
                {
                    packet.Write(s);
                }

                packet.Write(r.Permissions.Count);
                foreach (Permission p in r.Permissions)
                {
                    packet.Write((int)p);
                }

                SendData(packet);
            }
        }
        public static void RoleDelete(int toClient, string roleName)
        {
            using (Packet packet = new Packet((int)ServerPackets.RoleDelete))
            {
                packet.Write(roleName);

                SendData(toClient, packet);
            }
        }

        public static void RoleDelete(string roleName)
        {
            using (Packet packet = new Packet((int)ServerPackets.RoleDelete))
            {
                packet.Write(roleName);

                SendData(packet);
            }
        }
        public static void AccountDelete(int toClient, string accountName)
        {
            using (Packet packet = new Packet((int)ServerPackets.AccountDelete))
            {
                packet.Write(accountName);

                SendData(toClient, packet);
            }
        }

        public static void AccountDelete(string accountName)
        {
            using (Packet packet = new Packet((int)ServerPackets.AccountDelete))
            {
                packet.Write(accountName);

                SendData(packet);
            }
        }

        public static void Document(int toClient, Document d)
        {
            using (Packet packet = new Packet((int)ServerPackets.Document))
            {
                packet.Write(d.Name);
                packet.Write(d.DataTag);
                packet.Write(d.AccessLevel);
                packet.Write(d.UploadTime.ToBinary());

                SendData(toClient, packet);
            }
        }

        public static void DocumentList(int toClient)
        {
            List<Document> documents = DocumentManager.GetDocumentsList();
            Account? account = AccountManager.GetAccount(Server.clients[toClient].AccountLogin);
            if (account == null) return;
            foreach (Document document in documents)
            {
                if ((!account.RoleManager.HaveDataTag(document.DataTag) && !account.RoleManager.HavePermission(Permission.WorkWithAnyDocumentTag)) || !account.RoleManager.HaveAccessLevel(document.AccessLevel)) continue;
                Document(toClient, document);
            }
        }

        public static void DocumentDelete(string documentName, int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.DocumentDelete))
            {
                packet.Write(documentName);

                SendData(toClient, packet);
            }
        }
        public static void DocumentDelete(string documentName)
        {
            using (Packet packet = new Packet((int)ServerPackets.DocumentDelete))
            {
                packet.Write(documentName);

                SendData(packet);
            }
        }

        public static void FilePartRequest(string path, int part, int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.FilePartRequest))
            {
                packet.Write(path);
                packet.Write(part);

                SendData(toClient, packet);
            }
        }

        public static void DownloadFileResponse(string fileName, int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.DownloadFileResponse))
            {
                packet.Write(fileName);

                SendData(toClient,packet);
            }
        }

        public static void FilePart(int toClient, string fileName, int part)
        {
            using (Packet packet = new Packet((int)ServerPackets.FilePart))
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
    }
}
