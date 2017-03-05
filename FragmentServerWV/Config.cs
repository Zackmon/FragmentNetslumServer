using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV
{
    public static class Config
    {
        public static Dictionary<string, string> configs;
        public static void Load()
        {
            configs = new Dictionary<string, string>();
            if (File.Exists("settings.txt"))
            {
                string[] lines = File.ReadAllLines("settings.txt");
                foreach(string line in lines)
                    if (line.Trim() != "" && !line.Trim().StartsWith("#"))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                            configs.Add(parts[0].Trim().ToLower(), parts[1].Trim());
                    }
            }
        }
    }
}
