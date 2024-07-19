using System.Globalization;

namespace AccessSharing
{
    public class AccountManager
    {
        static Dictionary<string, Account> Accounts { get; set; } = new Dictionary<string, Account>();

        public static void CreateAccount(string login, string password)
        {
            if (Accounts.ContainsKey(login)) return;
            Account account = new Account()
            {
                Name = login,
                Login = login,
                Password = password
            };
            account.RoleManager.Login = login;
            Accounts.Add(login, account);

            Accounts.Order();
        }

        public static void DeleteAccount(string login)
        {
            if (!Accounts.ContainsKey(login)) return;
            Accounts.Remove(login);
        }

        public static Account? GetAccount(string login)
        {
            if (!Accounts.ContainsKey(login)) return null;
            return Accounts[login];
        }

        public static bool CheckPassword(string login, string password)
        {
            if (!Accounts.ContainsKey(login)) return false;
            if (string.IsNullOrEmpty(password)) return false;
            if (Accounts[login].Password != password) return false;
            return true;
        }

        public static bool HaveAccount(string login)
        {
            if (Accounts.ContainsKey(login)) return true;
            return false;
        }

        public static string GetLogin(string username)
        {
            foreach(Account a in Accounts.Values)
            {
                if(a.Name == username) return a.Login;
            }
            return string.Empty;
        }

        public static int Count()
        {
            return Accounts.Count;
        }

        public static List<Account> GetAccounts()
        {
            List<Account> accounts = new List<Account>();
            accounts.AddRange(Accounts.Values);
            return accounts;
        }

        public static void Save()
        {
            if (File.Exists("accounts.xml")) File.Delete("accounts.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Account>));
            using(FileStream fs = new FileStream("accounts.xml", FileMode.OpenOrCreate))
            {
                List<Account> accounts = new List<Account>();
                accounts.AddRange(Accounts.Values);
                xmlSerializer.Serialize(fs, accounts);
            }
        }

        public static void Load()
        {
            if(!File.Exists("accounts.xml")) return;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Account>));
            using (FileStream fs = new FileStream("accounts.xml", FileMode.OpenOrCreate))
            {
                List<Account>? accounts = xmlSerializer.Deserialize(fs) as List<Account>;
                if (accounts == null) 
                {
                    Console.WriteLine("Failed to load accounts.xml");
                    return;
                }
                foreach (Account account in accounts)
                {
                    Accounts.Add(account.Login, account);
                }
            }
        }

        public static void Clear()
        {
            Accounts.Clear();
        }
    }
}
