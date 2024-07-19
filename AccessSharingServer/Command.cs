namespace AccessSharingServer
{
    public class Command
    {
        public string Name { get; set; }
        public Action<string[]> Action { get; set; }
    }
}
