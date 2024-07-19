using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace AccessSharingClient
{
    public partial class MainWindow : Window
    {
        Client client = new Client();
        DispatcherTimer timer = new DispatcherTimer();
        List<int> rolesPositions = new List<int>();
        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Start();

            disconnectButton.IsEnabled = false;
            mainTabControl.IsEnabled = false;
            refreshButton.IsEnabled = false;
            addUserRoleButton.IsEnabled = false;
            removeUserRoleButton.IsEnabled = false;

            downloadButton.IsEnabled = false;
            deleteFileButton.IsEnabled = false;

            UpdateUserList();
        }

        public void Connect()
        {
            ServerConnectionWindow serverConnectionWindow = new ServerConnectionWindow();
            if (serverConnectionWindow.ShowDialog() == false)
            {
                return;
            }
            Client.Ip = serverConnectionWindow.Ip();
            Client.Port = Convert.ToInt32(serverConnectionWindow.Port());
            Client.ServerPassword = serverConnectionWindow.ServerPassword();
            client.Password = serverConnectionWindow.UserPassword();
            client.AccountLogin = serverConnectionWindow.Login();
            client.Connect();
        }

        public void UpdateUserList()
        {
            usersListBox.Items.Clear();
            rolesPositions.Clear();
            List<Role> roles = RoleLibrary.GetRolesListSorted();
            List<Account> users = AccountManager.GetAccounts();
            List<Account> remove = new List<Account>();
            for (int i = 0; i < roles.Count; i++)
            {
                rolesPositions.Add(usersListBox.Items.Count);
                System.Drawing.Color c = System.Drawing.Color.FromArgb(roles[i].Color);
                System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);
                TextBlock roleName = new TextBlock();
                roleName.Text = roles[i].Name;
                roleName.Foreground = new SolidColorBrush(color);
                roleName.FontWeight = FontWeights.Bold;
                usersListBox.Items.Add(roleName);
                foreach(Account account in users)
                {
                    if (!account.RoleManager.HaveRole(roles[i].Name)) continue;
                    remove.Add(account);
                    TextBlock acc = new TextBlock();
                    acc.Text = account.Name;
                    acc.Foreground = new SolidColorBrush(color);
                    usersListBox.Items.Add(acc);
                }
                foreach(Account account in remove)
                {
                    users.Remove(account);
                }
            }
            if (users.Count == 0) return;
            rolesPositions.Add(usersListBox.Items.Count);
            System.Windows.Media.Color col = System.Windows.Media.Color.FromRgb(0,0,0);
            TextBlock noRoleName = new TextBlock();
            noRoleName.Text = "Нет роли";
            noRoleName.Foreground = new SolidColorBrush(col);
            noRoleName.FontWeight = FontWeights.Bold;
            usersListBox.Items.Add(noRoleName);
            foreach (Account account in users)
            {
                TextBlock acc = new TextBlock();
                acc.Text = account.Name;
                acc.Foreground = new SolidColorBrush(col);
                usersListBox.Items.Add(acc);
            }
        }

        public void UpdateRolesList()
        {
            rolesListBox.Items.Clear();
            List<Role> roles = RoleLibrary.GetRolesListSorted();
            for (int i = 0; i < roles.Count; i++)
            {
                System.Drawing.Color c = System.Drawing.Color.FromArgb(roles[i].Color);
                System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);
                TextBlock role = new TextBlock();
                role.Text = $"{roles[i].Name} ({roles[i].AccessLevel})";
                role.Foreground = new SolidColorBrush(color);
                role.FontWeight = FontWeights.Bold;
                rolesListBox.Items.Add(role);
            }
        }

        public void UpdateDocumentsList()
        {
            documentsDataGrid.Items.Clear();
            List<Document> documents = DocumentManager.GetDocumentsList();
            foreach (Document document in documents)
            {
                documentsDataGrid.Items.Add(document);
            }
        }

        private void UpdateUserRoleList(Account account)
        {
            possibleRolesListBox.Items.Clear();
            currentRolesListBox.Items.Clear();
            List<Role> roles = RoleLibrary.GetRolesList();
            foreach(Role role in roles)
            {
                TextBlock item = new TextBlock();
                item.Text = role.Name;
                System.Drawing.Color c = System.Drawing.Color.FromArgb(role.Color);
                System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);
                item.Foreground = new SolidColorBrush(color);
                if (account.RoleManager.HaveRole(role.Name))
                {
                    currentRolesListBox.Items.Add(item);
                    continue;
                }
                possibleRolesListBox.Items.Add(item);
            }
        }
        private void usersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            possibleRolesListBox.Items.Clear();
            currentRolesListBox.Items.Clear();
            usernameTextBox.Text = string.Empty;
            if (rolesPositions.Contains(usersListBox.SelectedIndex))
            {
                usersListBox.SelectedIndex = -1;
                return;
            }
            TextBlock? textBlock = usersListBox.SelectedItem as TextBlock;
            if (textBlock == null) return;
            Account? account = AccountManager.GetAccount(AccountManager.GetLogin(textBlock.Text));
            if (account == null) return;
            usernameTextBox.Text = account.Name;
            UpdateUserRoleList(account);
        }

        public void Disconnect()
        {
            if (client.TcpClient != null && client.TcpClient.Connected)
            {
                client.Disconnect();
            }
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            string caption = "Отключение от сервера";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Question;
            MessageBoxResult result;
            result = MessageBox.Show("Вы уверены, что хотите отключиться от сервера?", caption, button, image);
            if (result == MessageBoxResult.Yes)
            {
                Disconnect();
            }
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ThreadManager.Update();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadManager.Execute(new Action(() => Connect()));
        }

        private void possibleRolesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(possibleRolesListBox.SelectedIndex == -1)
            {
                addUserRoleButton.IsEnabled = false;
                return;
            }
            currentRolesListBox.SelectedIndex = -1;
            removeUserRoleButton.IsEnabled = false;
            addUserRoleButton.IsEnabled = true;
        }

        private void currentRolesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentRolesListBox.SelectedIndex == -1)
            {
                removeUserRoleButton.IsEnabled = false;
                return;
            }
            possibleRolesListBox.SelectedIndex = -1;
            addUserRoleButton.IsEnabled = false;
            removeUserRoleButton.IsEnabled = true;
        }

        private void applyUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock? textBlock = usersListBox.SelectedItem as TextBlock;
            if (textBlock == null) return;
            Account? account = AccountManager.GetAccount(AccountManager.GetLogin(textBlock.Text));
            if (account == null) return;
            ClientSend.ChangeUsername(account.Name, usernameTextBox.Text);
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            refreshButton.IsEnabled = false;
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(1000);
                if (Client.Instance.TcpClient == null || !Client.Instance.TcpClient.Connected) return;
                Dispatcher.Invoke(() => refreshButton.IsEnabled = true);
            });
            thread.Start();
            
            DocumentManager.Clear();
            RoleLibrary.Clear();
            AccountManager.Clear();
            UpdateDocumentsList();
            UpdateUserList();
            UpdateRolesList();
            ClientSend.RoleListRequest();
            ClientSend.DocumentsListRequest();
            ClientSend.AccountListRequest();
        }

        private void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            FileUploadWindow fileUploadWindow = new FileUploadWindow();
            if (fileUploadWindow.ShowDialog() == false)
            {
                return;
            }
            ClientSend.File(new FileInfo(fileUploadWindow.Path()), fileUploadWindow.SecurityLevel(), fileUploadWindow.DataTag());
        }

        private void documentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(documentsDataGrid.SelectedIndex == -1)
            {
                downloadButton.IsEnabled = false;
                deleteFileButton.IsEnabled = false;
                return;
            }
            downloadButton.IsEnabled = true;
            deleteFileButton.IsEnabled = true;
        }

        private void deleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            Document? doc = documentsDataGrid.SelectedItem as Document;
            if (doc == null) return;
            string caption = "Удаление файла";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Question;
            MessageBoxResult result;
            result = MessageBox.Show($"Вы уверены, что хотите удалить {doc.Name}?", caption, button, image);
            if (result == MessageBoxResult.Yes)
            {
                ClientSend.DeleteFileRequest(doc.Name);
            }
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            Document? doc = documentsDataGrid.SelectedItem as Document;
            if (doc == null) return;
            if (File.Exists($"documents/{doc.Name}"))
            {
                string caption = "Перезаписать файл?";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage image = MessageBoxImage.Question;
                MessageBoxResult result;
                result = MessageBox.Show($"Файл с таким именем уже существует. Перезаписать {doc.Name}?", caption, button, image);
                if (result == MessageBoxResult.Yes)
                {
                    ClientSend.DownloadFileRequest(doc.Name);
                }
                return;
            }
            ClientSend.DownloadFileRequest(doc.Name);
        }

        private void addUserRoleButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock? textBlock = usersListBox.SelectedItem as TextBlock;
            if (textBlock == null) return;
            Account? account = AccountManager.GetAccount(AccountManager.GetLogin(textBlock.Text));
            if (account == null) return;
            TextBlock? role = possibleRolesListBox.SelectedItem as TextBlock;
            if (role == null) return;
            ClientSend.AddUserRole(account.Login, role.Text);
        }

        private void removeUserRoleButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock? textBlock = usersListBox.SelectedItem as TextBlock;
            if (textBlock == null) return;
            Account? account = AccountManager.GetAccount(AccountManager.GetLogin(textBlock.Text));
            if (account == null) return;
            TextBlock? role = currentRolesListBox.SelectedItem as TextBlock;
            if (role == null) return;
            ClientSend.RemoveUserRole(account.Login, role.Text);
        }
    }
}
