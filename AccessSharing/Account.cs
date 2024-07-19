namespace AccessSharing
{
    public class Account
    {
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public RoleManager RoleManager { get; set; } = new RoleManager();

        public override string ToString()
        {
            return Name;
        }
    }
}
