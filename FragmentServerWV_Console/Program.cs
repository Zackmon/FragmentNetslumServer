using System;
using FragmentServerWV;

namespace FragmentServerWV_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            LogEventDelegate logEventDelegate = new LogEventDelegate();
            logEventDelegate.Logging += LogToConsole;
            Config.Load();
            Log.InitLogs(logEventDelegate);
            Server.Start();
        }

        public static void LogToConsole(String text, int logSize)
        {
            Console.WriteLine(text);
        }
    }
}
