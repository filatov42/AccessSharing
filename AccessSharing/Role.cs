namespace AccessSharing
{
    public class Role
    {
        public List<Permission> Permissions { get; set; } = new List<Permission>();
        public List<string> DataTags { get; set; } = new List<string>();
        public int AccessLevel { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Color = System.Drawing.Color.Black.ToArgb();

        public List<string> Accounts = new List<string>();

        public void AddPermission(Permission permission)
        {
            if (Permissions.Contains(permission)) return;
            Permissions.Add(permission);
        }

        public void RemovePermission(Permission permission)
        {
            if (!Permissions.Contains(permission)) return;
            Permissions.Remove(permission);
        }

        public bool HavePermission(Permission permission)
        {
            if(Permissions.Contains(permission)) return true;
            return false;
        }

        public void AddDataTag(string tag)
        {
            if(DataTags.Contains(tag)) return;
            DataTags.Add(tag);
        }

        public void RemoveDataTag(string tag)
        {
            if (!DataTags.Contains(tag)) return;
            DataTags.Remove(tag);
        }

        public bool HaveDataTag(string tag)
        {
            if (DataTags.Contains(tag)) return true;
            return false;
        }

        public void SetAccessLevel(int level)
        {
            AccessLevel = level;
        }

        public bool HaveSecurityLevel(int level)
        {
            if(level <= AccessLevel) return true;
            return false;
        }
    }
}
