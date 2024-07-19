namespace AccessSharingClient
{
    public class ClientHandle
    {
        //public static void Example(Packet packet)
        //{
        //    string msg = packet.ReadString();
        //    Console.WriteLine(msg);
        //}

        public static void HandshakeReceived(Packet packet)
        {
            Client.Instance.Id = packet.ReadInt();
            ClientSend.Handshake();
        }

        public static void SuccessfulLoginReceived(Packet packet)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    MainWindow w = window as MainWindow;
                    w.connectButton.IsEnabled = false;
                    w.disconnectButton.IsEnabled = true;
                    w.mainTabControl.IsEnabled = true;
                    w.refreshButton.IsEnabled = true;
                }
            }
            ClientSend.RoleListRequest();
            ClientSend.AccountListRequest();
        }

        public static void Error(Packet packet)
        {
            string msg = packet.ReadString();
            bool disconnect = packet.ReadBool();

            Thread thread = new Thread(new ThreadStart(() =>
            {
                string caption = "Ошибка";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage image = MessageBoxImage.Error;
                MessageBoxResult result;
                result = MessageBox.Show(msg, caption, button, image);
            }
            ));
            thread.Start();

            if(disconnect )
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Disconnect();
                    }
                }
            }
        }

        public static void Message(Packet packet)
        {
            string msg = packet.ReadString();

            Thread thread = new Thread(new ThreadStart(() =>
            {
                string caption = "Сообщение";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage image = MessageBoxImage.Information;
                MessageBoxResult result;
                result = MessageBox.Show(msg, caption, button, image);
            }
            ));
            thread.Start();
        }

        public static void AccountReceived(Packet packet)
        {
            string username = packet.ReadString();
            string login = packet.ReadString();
            AccountManager.CreateAccount(login, string.Empty);

            Account account = AccountManager.GetAccount(login);
            account.Name = username;
            int rolesCount = packet.ReadInt();
            for(int i = 0; i< rolesCount; i++)
            {
                account.RoleManager.AddRole(packet.ReadString());
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).UpdateUserList();
                }
            }
        }
        public static void AccountDeleteReceived(Packet packet)
        {
            AccountManager.DeleteAccount(packet.ReadString());
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).UpdateUserList();
                }
            }
        }
        public static void RoleReceived(Packet packet)
        {
            Role role = new Role();
            role.Name = packet.ReadString();
            RoleLibrary.RemoveRole(role.Name);
            role.Color = packet.ReadInt();
            role.SetAccessLevel(packet.ReadInt());
            int count;
            count = packet.ReadInt();
            for (int i = 0; i < count; i++)
            {
                role.AddDataTag(packet.ReadString());
            }
            count = packet.ReadInt();
            for (int i = 0; i < count; i++)
            {
                role.AddPermission((Permission)packet.ReadInt());
            }
            RoleLibrary.CreateRole(role);
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).UpdateRolesList();
                }
            }
        }

        public static void RoleDeleteReceived(Packet packet)
        {
            RoleLibrary.RemoveRole(packet.ReadString());
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).UpdateRolesList();
                }
            }
        }

        public static void DocumentReceived(Packet packet)
        {
            Document document = new Document();
            document.Name = packet.ReadString();
            document.DataTag = packet.ReadString();
            document.AccessLevel = packet.ReadInt();
            document.UploadTime = DateTime.FromBinary(packet.ReadLong());
            DocumentManager.LoadDocument(document);
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).UpdateDocumentsList();
                }
            }
        }

        public static void DocumentDeleteReceived(Packet packet)
        {
            string name = packet.ReadString();
            DocumentManager.DeleteDocument(name);
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).UpdateDocumentsList();
                }
            }
        }

        public static void FilePartsRequestReceived(Packet packet)
        {
            string path = packet.ReadString();
            int part = packet.ReadInt();
            if(part == 0)
            {
                FileManager.AddData(new FileInfo(path).Name, File.ReadAllBytes(path));
            }
            ClientSend.FilePart(new FileInfo(path).Name, part);
        }


        public static void DownloadFileResponseReceived(Packet packet)
        {
            string filename = packet.ReadString();
            ClientSend.FilePartRequest(filename, 0);
        }

        public static void FilePartReceived(Packet packet)
        {
            string fileName = packet.ReadString();
            bool end = packet.ReadBool();
            if (end)
            {
                Directory.CreateDirectory("documents");
                FileManager.BuildFile(fileName, $"documents/{fileName}");
                return;
            }
            int part = packet.ReadInt();
            byte[] buff = new byte[packet.ReadInt()];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = packet.ReadByte();
            }
            FileManager.AddData(fileName, buff);
            ClientSend.FilePartRequest(fileName, part + 1);
        }
    }
}
