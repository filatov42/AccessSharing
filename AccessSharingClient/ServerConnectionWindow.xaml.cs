namespace AccessSharingClient
{
    public partial class ServerConnectionWindow : Window
    {
        bool remember = false;
        public ServerConnectionWindow()
        {
            InitializeComponent();
            LoadConnectInfo();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConnectInfo();
            this.DialogResult = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveConnectInfo()
        {
            string[] info = new string[4];
            if (File.Exists("connectinfo.xml")) File.Delete("connectinfo.xml");
            if (!remember)
            {
                return;
            }
            info[0] = Login();
            info[1] = Ip();
            info[2] = Port();
            info[3] = "true";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(string[]));
            using (FileStream fs = new FileStream("connectinfo.xml", FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, info);
            }
        }

        private void LoadConnectInfo()
        {
            if (!File.Exists("connectinfo.xml")) return;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(string[]));
            using (FileStream fs = new FileStream("connectinfo.xml", FileMode.OpenOrCreate))
            {
                string[]? info = xmlSerializer.Deserialize(fs) as string[];
                if (info == null || info[3] == "false")
                {
                    return;
                }
                loginTextBox.Text = info[0];
                ipTextBox.Text = info[1];
                portTextBox.Text = info[2];
                rememberCheckBox.IsChecked = true;
                remember = true;
            }
        }


        public string Login()
        {
            return loginTextBox.Text;
        }

        public string UserPassword()
        {
            return userPasswordBox.Password;
        }
        public string Ip()
        {
            return ipTextBox.Text;
        }
        public string Port()
        {
            return portTextBox.Text;
        }
        public string ServerPassword()
        {
            return serverPasswordBox.Password;
        }

        private void rememberCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            remember = true;
        }

        private void rememberCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            remember = false;
        }
    }
}
