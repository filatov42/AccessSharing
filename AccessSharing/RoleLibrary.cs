namespace AccessSharing
{
    public class RoleLibrary
    {
        static Dictionary<string, Role> Roles = new Dictionary<string, Role>();

        public static void CreateRole(Role role)
        {
            if (Roles.ContainsKey(role.Name)) return;
            Roles.Add(role.Name, role);
        }

        public static void RemoveRole(string roleName)
        {
            if (!Roles.ContainsKey(roleName)) return;
            Role? role = GetRole(roleName);
            if (role == null) return;
            foreach(string log in role.Accounts)
            {
                Account? account = AccountManager.GetAccount(log);
                if (account == null) continue;
                account.RoleManager.RemoveRole(roleName, true);
            }
            Roles.Remove(roleName);
        }

        public static Role? GetRole(string roleName)
        {
            if (!Roles.ContainsKey(roleName)) return null;
            return Roles[roleName];
        }

        public static bool HaveRole(string roleName)
        {
            if (Roles.ContainsKey(roleName)) return true;
            return false;
        }

        public static int RolesCount()
        {
            return Roles.Values.Count;
        }

        public static List<Role> GetRolesList()
        {
            List<Role> roles = new List<Role>();
            roles.AddRange(Roles.Values);
            return roles;
        }

        public static List<Role> GetRolesListSorted()
        {
            List<Role> roles = new List<Role>();
            roles.AddRange(Roles.Values);
            int maxAccess = 0;
            foreach (Role role in roles)
            {
                if(role.AccessLevel > maxAccess)
                {
                    maxAccess = role.AccessLevel;
                }
            }
            List<Role> rolesSorted = new List<Role>();
            for(int i = 0; i <= maxAccess; i++)
            {
                foreach (Role role in roles)
                {
                    if (role.AccessLevel != i) continue;
                    rolesSorted.Add(role);
                }
            }
            rolesSorted.Reverse();
            return rolesSorted;
        }

        public static void Clear()
        {
            Roles.Clear();
        }

        public static void Save()
        {
            if (File.Exists("roles.xml")) File.Delete("roles.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Role>));
            using (FileStream fs = new FileStream("roles.xml", FileMode.OpenOrCreate))
            {
                List<Role> roles = new List<Role>();
                roles.AddRange(Roles.Values);
                xmlSerializer.Serialize(fs, roles);
            }
        }

        public static void Load()
        {
            if (!File.Exists("roles.xml")) return;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Role>));
            using (FileStream fs = new FileStream("roles.xml", FileMode.OpenOrCreate))
            {
                List<Role>? roles = xmlSerializer.Deserialize(fs) as List<Role>;
                if (roles == null)
                {
                    Console.WriteLine("Failed to load roles.xml");
                    return;
                }
                foreach (Role role in roles)
                {
                    Roles.Add(role.Name, role);
                }
            }
        }
    }
}
