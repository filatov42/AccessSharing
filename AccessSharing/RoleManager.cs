using System.Reflection.Emit;

namespace AccessSharing
{
    public class RoleManager
    {
        public List<string> Roles { get; set; } = new List<string>();
        public string Login = string.Empty;

        public void AddRole(string roleName)
        {
            Role? role = RoleLibrary.GetRole(roleName);
            if(role == null) return;
            role.Accounts.Add(Login);
            Roles.Add(roleName);
        }

        public void RemoveRole(string roleName, bool fromRoleLibrary = false)
        {
            if(!Roles.Contains(roleName)) return;
            Roles.Remove(roleName);
            Role? role = RoleLibrary.GetRole(roleName);
            if (role == null) return;
            if (fromRoleLibrary) return;
            role.Accounts.Remove(Login);
        }

        public bool HaveRole(string roleName)
        {
            if(Roles.Contains(roleName)) return true;
            return false;
        }

        public bool HavePermission(Permission permission)
        {
            foreach(string role in Roles)
            {
                Role r = RoleLibrary.GetRole(role);
                if (!r.HavePermission(permission)) continue;
                return true;
            }
            return false;
        }

        public bool HaveDataTag(string tag)
        {
            foreach (string role in Roles)
            {
                Role r = RoleLibrary.GetRole(role);
                if (!r.HaveDataTag(tag)) continue;
                return true;
            }
            return false;
        }

        public int AccessLevel()
        {
            int secLevel = 0;
            foreach (string role in Roles)
            {
                Role r = RoleLibrary.GetRole(role);
                if(r.AccessLevel > secLevel)
                {
                    secLevel = r.AccessLevel;
                }
            }
            return secLevel;
        }

        public bool HaveAccessLevel(int level)
        {
            foreach (string role in Roles)
            {
                Role r = RoleLibrary.GetRole(role);
                if (!r.HaveSecurityLevel(level)) continue;
                return true;
            }
            return false;
        }

        public List<string> GetRoles()
        {
            List<string> roles = new List<string>();
            foreach(string r in Roles)
            {
                roles.Add(r);
            }
            return roles;
        }
    }
}
