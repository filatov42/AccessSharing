using System.Drawing;

namespace AccessSharingServer
{
    class Program
    {
        public static Thread commandsThread = new Thread(new ThreadStart(Commands.Enable));
        public static Thread updateThread = new Thread(new ThreadStart(Update));
        public static Thread saveThread = new Thread(new ThreadStart(SaveLoop));

        public static void Shutdown()
        {
            Save();
            Server.Stop();
            Environment.Exit(0);
        }

        static void Main()
        {
            DocumentManager.Load();
            RoleLibrary.Load();
            AccountManager.Load();
            Settings.IsServer = true;
            Server.Start();
            commandsThread.Start();
            updateThread.Start();
            saveThread.Start();
        }

        static void Update()
        {
            DateTime lastUpdate = DateTime.UtcNow;
            while (true)
            {
                if ((DateTime.UtcNow - lastUpdate).TotalMilliseconds < 100) continue;
                lastUpdate = DateTime.UtcNow;
                ThreadManager.Update();
            }
        }

        static void SaveLoop()
        {
            DateTime lastUpdate = DateTime.UtcNow;
            while (true)
            {
                if ((DateTime.UtcNow - lastUpdate).TotalMinutes < 5) continue;
                lastUpdate = DateTime.UtcNow;
                Save();
            }
        }

        public static void Save()
        {
            Server.SaveSettings();
            AccountManager.Save();
            RoleLibrary.Save();
            DocumentManager.Save();
        }
    }
}