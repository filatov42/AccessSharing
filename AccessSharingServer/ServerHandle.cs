using System.IO;

namespace AccessSharingServer
{
    public class ServerHandle
    {
        //public static void Example(int fromClient, Packet packet)
        //{
        //    string msg = packet.ReadString();

        //    Console.WriteLine($"{Server.clients[fromClient].Username}: {msg}");
        //}

        public static void HandshakeReceived(int fromClient, Packet packet)
        {
            string login = packet.ReadString();
            string userPassword = packet.ReadString();
            string serverPassword = packet.ReadString();

            if (serverPassword != Server.Settings.Password)
            {
                ServerSend.Error(fromClient, "Неверный пароль сервера!", true);
                Server.clients[fromClient].Disconnect();
                return;
            }

            if(userPassword == string.Empty)
            {
                ServerSend.Error(fromClient, "Пароль должен содержать символы!", true);
                return;
            }

            if (!AccountManager.HaveAccount(login))
            {
                AccountManager.CreateAccount(login, userPassword);
                AccountManager.GetAccount(login).RoleManager.AddRole("1");
            }

            if (!AccountManager.CheckPassword(login, userPassword))
            {
                ServerSend.Error(fromClient, "Неверный пароль пользователя!", true);
                Server.clients[fromClient].Disconnect();
                return;
            }

            Server.clients[fromClient].AccountLogin = login;
            ServerSend.Message(fromClient, "Успешное подключение!");
            ServerSend.SuccessfulLogin(fromClient);
        }

        public static void AccountListRequestReceived(int fromClient, Packet packet)
        {
            ServerSend.AccountList(fromClient);
        }

        public static void RoleListRequestReceived(int fromClient, Packet packet)
        {
            ServerSend.RoleList(fromClient);
        }

        public static void ChangeUsernameRequestReceived(int fromClient, Packet packet)
        {
            Account sender = AccountManager.GetAccount(Server.clients[fromClient].AccountLogin);
            Account change = AccountManager.GetAccount(packet.ReadString());

            if (!sender.RoleManager.HavePermission(Permission.ChangeUsernames))
            {
                ServerSend.Error(fromClient, "У вас нет прав изменять имена пользователей!", false);
                return;
            }
            change.Name = packet.ReadString();

            ServerSend.Account(change);
        }

        public static void DocumentsListRequestReceived(int fromClient, Packet packet)
        {
            ServerSend.DocumentList(fromClient);
        }

        public static void FileReceived(int fromClient, Packet packet)
        {
            string filename = packet.ReadString();
            string fullName = packet.ReadString();
            int secLevel = packet.ReadInt();
            string tag = packet.ReadString();

            Account sender = AccountManager.GetAccount(Server.clients[fromClient].AccountLogin);
            if (!sender.RoleManager.HavePermission(Permission.LoadFiles))
            {
                ServerSend.Error(fromClient, "У вас нет прав загружать файлы.", false);
                return;
            }
            if (!sender.RoleManager.HaveAccessLevel(secLevel))
            {
                ServerSend.Error(fromClient, $"Ваш уровень доступа ({sender.RoleManager.AccessLevel()}) ниже, чем у загружаемого файла ({secLevel}).", false);
                return;
            }
            if (!sender.RoleManager.HaveDataTag(tag) && !sender.RoleManager.HavePermission(Permission.WorkWithAnyDocumentTag))
            {
                ServerSend.Error(fromClient, $"У вас нет прав работать с файлами типа {tag}.", false);
                return;
            }
            if (File.Exists($"documents/{filename}"))
            {
                ServerSend.Error(fromClient, $"Файл с таким именем уже существует.", false);
                return;
            }
            if (!Directory.Exists("documents")) Directory.CreateDirectory("documents");
            string path = $"documents/{filename}";
            using (File.Create(path)) { }
            DocumentManager.LoadDocument(new FileInfo(path), tag, secLevel);
            ServerSend.FilePartRequest(fullName, 0, fromClient);
        }

        public static void FilePartReceived(int fromClient, Packet packet)
        {
            string fileName = packet.ReadString();
            bool end = packet.ReadBool();
            if (end)
            {
                FileManager.BuildFile(fileName, $"documents/{fileName}");
                return;
            }
            int part = packet.ReadInt();
            byte[] buff = new byte[packet.ReadInt()];
            for(int i = 0; i < buff.Length; i++)
            {
                buff[i] = packet.ReadByte();
            }
            FileManager.AddData(fileName, buff);
            ServerSend.FilePartRequest(fileName, part + 1, fromClient);
        }

        public static void FileDeleteRequestReceived(int fromClient, Packet packet)
        {
            string filename = packet.ReadString();
            Document? document = DocumentManager.GetDocument(filename);
            Account? sender = AccountManager.GetAccount(Server.clients[fromClient].AccountLogin);
            if(document == null)
            {
                ServerSend.Error(fromClient, "Файл не найден.", false);
                return;
            }
            if (!sender.RoleManager.HavePermission(Permission.DeleteFiles))
            {
                ServerSend.Error(fromClient, "У вас нет прав удалять файлы.", false);
                return;
            }
            if (!sender.RoleManager.HaveAccessLevel(document.AccessLevel))
            {
                ServerSend.Error(fromClient, $"Ваш уровень доступа ({sender.RoleManager.AccessLevel()}) ниже, чем у удаляемого файла ({document.AccessLevel}).", false);
                return;
            }
            if (!sender.RoleManager.HaveDataTag(document.DataTag) && !sender.RoleManager.HavePermission(Permission.WorkWithAnyDocumentTag))
            {
                ServerSend.Error(fromClient, $"У вас нет прав работать с файлами типа {document.DataTag}.", false);
                return;
            }
            DocumentManager.DeleteDocument(filename);
            ServerSend.DocumentDelete(filename);
        }

        public static void FileDownloadRequestReceived(int fromClient, Packet packet)
        {
            string filename = packet.ReadString();
            Document? document = DocumentManager.GetDocument(filename);
            Account? sender = AccountManager.GetAccount(Server.clients[fromClient].AccountLogin);
            if (document == null)
            {
                ServerSend.Error(fromClient, "Файл не найден.", false);
                return;
            }
            if (!sender.RoleManager.HavePermission(Permission.DeleteFiles))
            {
                ServerSend.Error(fromClient, "У вас нет прав загружать файлы.", false);
                return;
            }
            if (!sender.RoleManager.HaveAccessLevel(document.AccessLevel))
            {
                ServerSend.Error(fromClient, $"Ваш уровень доступа ({sender.RoleManager.AccessLevel()}) ниже, чем у загружаемого файла ({document.AccessLevel}).", false);
                return;
            }
            if (!sender.RoleManager.HaveDataTag(document.DataTag) && !sender.RoleManager.HavePermission(Permission.WorkWithAnyDocumentTag))
            {
                ServerSend.Error(fromClient, $"У вас нет прав работать с файлами типа {document.DataTag}.", false);
                return;
            }
            FileManager.AddData(filename, File.ReadAllBytes(DocumentManager.GetDocument(filename).Path));
            ServerSend.DownloadFileResponse(filename, fromClient);
        }

        public static void FilePartsRequestReceived(int fromClient, Packet packet)
        {
            string filename = packet.ReadString();
            int part = packet.ReadInt();
            ServerSend.FilePart(fromClient, filename, part);
        }

        public static void AddUserRoleRequestReceived(int fromClient, Packet packet)
        {
            string login = packet.ReadString();
            string roleName = packet.ReadString();
            Account? sender = AccountManager.GetAccount(Server.clients[fromClient].AccountLogin);
            Account? account = AccountManager.GetAccount(login);
            if (sender == null || account == null) return;
            if (!sender.RoleManager.HavePermission(Permission.ChangeUserRoles))
            {
                ServerSend.Error(fromClient, "У вас нет прав изменять роли пользователей.", false);
                return;
            }
            if (sender.RoleManager.AccessLevel() <= account.RoleManager.AccessLevel())
            {
                ServerSend.Error(fromClient, $"Ваш уровень доступа ({sender.RoleManager.AccessLevel()}) не позволяет изменить роли пользователя с уровнем доступа {account.RoleManager.AccessLevel()}.", false);
                return;
            }
            if(sender.RoleManager.AccessLevel() <= RoleLibrary.GetRole(roleName).AccessLevel)
            {
                ServerSend.Error(fromClient, $"Ваш уровень доступа ({sender.RoleManager.AccessLevel()}) не позволяет добавить эту роль.", false);
                return;
            }
            account.RoleManager.AddRole(roleName);
            ServerSend.Account(account);
        }

        public static void RemoveUserRoleRequestReceived(int fromClient, Packet packet)
        {
            string login = packet.ReadString();
            string roleName = packet.ReadString();
            Account? sender = AccountManager.GetAccount(Server.clients[fromClient].AccountLogin);
            Account? account = AccountManager.GetAccount(login);
            if (sender == null || account == null) return;
            if (!sender.RoleManager.HavePermission(Permission.ChangeUserRoles))
            {
                ServerSend.Error(fromClient, "У вас нет прав изменять роли пользователей.", false);
                return;
            }
            if (sender.RoleManager.AccessLevel() <= account.RoleManager.AccessLevel())
            {
                ServerSend.Error(fromClient, $"Ваш уровень доступа ({sender.RoleManager.AccessLevel()}) не позволяет изменить роли пользователя с уровнем доступа {account.RoleManager.AccessLevel()}.", false);
                return;
            }
            account.RoleManager.RemoveRole(roleName);
            ServerSend.Account(account);
        }
    }
}
