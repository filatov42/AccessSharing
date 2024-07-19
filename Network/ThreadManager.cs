namespace Network
{
    public class ThreadManager
    {
        static List<Action> execute = new List<Action>();
        static List<Action> executeCopied = new List<Action>();
        static bool actionToExecute = false;
        public static void Execute(Action action)
        {
            lock (execute)
            {
                execute.Add(action);
                actionToExecute = true;
            }
        }

        public static void Update()
        {
            if (actionToExecute)
            {
                executeCopied.Clear();
                lock (execute)
                {
                    executeCopied.AddRange(execute);
                    execute.Clear();
                    actionToExecute = false;
                }

                for (int i = 0; i < executeCopied.Count; i++)
                {
                    executeCopied[i]();
                }
            }
        }
    }
}
