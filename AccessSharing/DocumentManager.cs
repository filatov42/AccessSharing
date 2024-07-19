namespace AccessSharing
{
    public class DocumentManager
    {
        static Dictionary<string, Document> Documents = new Dictionary<string, Document>();

        public static void LoadDocument(Document document)
        {
            if (Documents.ContainsKey(document.Name)) return;
            Documents.Add(document.Name, document);
            if (!Settings.IsServer) return;
            Save();
        }

        public static void LoadDocument(FileInfo file, string dataTag, int accesLevel)
        {
            if (Documents.ContainsKey(file.Name)) return;
            Document document = new Document();
            document.Name = file.Name;
            document.Path = file.FullName;
            document.DataTag = dataTag;
            document.AccessLevel = accesLevel;
            document.UploadTime = DateTime.Now;
            Documents.Add(document.Name, document);
            if (!Settings.IsServer) return;
            Save();
        }

        public static void DeleteDocument(string name)
        {
            if (!Documents.ContainsKey(name)) return;
            if (File.Exists(Documents[name].Path) && Settings.IsServer)
            {
                File.Delete(Documents[name].Path);
            }
            Documents.Remove(name);
            if (!Settings.IsServer) return;
            Save();
        }

        public static Document? GetDocument(string name)
        {
            if (!Documents.ContainsKey(name)) return null;
            return Documents[name];
        }
        public static List<Document> GetDocumentsList()
        {
            List<Document> documents = new List<Document>();
            documents.AddRange(Documents.Values);
            return documents;
        }

        public static void Clear()
        {
            Documents.Clear();
        }
        public static void Save()
        {
            if (File.Exists("documents.xml")) File.Delete("documents.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Document>));
            using (FileStream fs = new FileStream("documents.xml", FileMode.OpenOrCreate))
            {
                List<Document> documents = new List<Document>();
                documents.AddRange(Documents.Values);
                xmlSerializer.Serialize(fs, documents);
            }
        }

        public static void Load()
        {
            if (!File.Exists("documents.xml")) return;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Document>));
            using (FileStream fs = new FileStream("documents.xml", FileMode.OpenOrCreate))
            {
                List<Document>? documents = xmlSerializer.Deserialize(fs) as List<Document>;
                if (documents == null)
                {
                    Console.WriteLine("Failed to load documents.xml");
                    return;
                }
                foreach (Document document in documents)
                {
                    Documents.Add(document.Name, document);
                }
            }
        }
    }
}
