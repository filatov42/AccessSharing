namespace AccessSharing
{
    public class Document
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int AccessLevel { get; set; }
        public string DataTag { get; set; } = string.Empty;
        public DateTime UploadTime { get; set; }
    }
}
