using System;
using System.Collections.Generic;
using System.IO;

namespace KeyOverlay
{
    public class Config
    {
        private string name;
        private Dictionary<string, Dictionary<string, string>> config;
        private FileSystemWatcher watcher;
        private Action callback;

        public Config(string name, Action callback)
        {
            this.name = name;
            this.callback = callback;
            Load();

            watcher = new FileSystemWatcher();
            watcher.Path = ".";
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess;
            watcher.Filter = name;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        public Dictionary<string, string> this[string key] => config[key];

        private void Read()
        {
            var objectDict = new Dictionary<string, Dictionary<string, string>>();
            var lines = File.ReadAllLines(name);
            var current = "";
            foreach (var line in lines)
            {
                if (line == "") continue;
                if (line.StartsWith("["))
                {
                    current = line.Substring(1, line.Length - 2);
                    objectDict.Add(current, new());
                }
                else
                {
                    var key = line.Split('=')[0];
                    var value = line.Split('=')[1];
                    objectDict[current].Add(key, value);
                }

            }
            this.config = objectDict;
        }

        private void Check()
        {
            string[] required = { "General", "Keys" };
            string[] optional = { "Display", "Size", "Colors" };
            foreach (var name in required)
            {
                if (!config.ContainsKey(name)) throw new InvalidDataException("Missing required data from config file");
            }
            foreach (var name in optional)
            {
                if (!config.ContainsKey(name)) config.Add(name, new());
            }
        }
        public void Load()
        {
            config = new();
            Read();
            Check();
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // funky event *before* the file is actually saved
            System.Threading.Thread.Sleep(100);
            Load();
            callback();
        }
    }
}
