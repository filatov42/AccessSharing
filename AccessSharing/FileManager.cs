namespace AccessSharing
{
    public class FileManager
    {
        public static int PartSize = 8388608;
        static Dictionary<string, List<byte>> Files = new Dictionary<string, List<byte>>();

        public static void AddData(string filename, byte[] data)
        {
            if(!Files.ContainsKey(filename))
            {
                Files.Add(filename, new List<byte>());
            }
            Files[filename].AddRange(data);
        }

        public static void RemoveData(string filename)
        {
            if (!Files.ContainsKey(filename)) return; 
            Files.Remove(filename);
        }

        public static byte[]? GetFilePart(string filename, int part)
        {
            if (!Files.ContainsKey(filename)) return null;
            if (part * PartSize > Files[filename].Count) return null;
            List<byte> bytes = new List<byte>();
            for(int i = 0; i < PartSize; i++)
            {
                if (i + PartSize * part >= Files[filename].Count)
                {
                    break;
                }
                bytes.Add(Files[filename][i + PartSize * part]);
            }
            return bytes.ToArray();
        }

        public static void BuildFile(string filename, string path)
        {
            if (!Files.ContainsKey(filename)) return;
            BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(path));
            binaryWriter.Write(Files[filename].ToArray());
            binaryWriter.Dispose();

            Files[filename].Clear();
            Files.Remove(filename);
        }
    }
}
