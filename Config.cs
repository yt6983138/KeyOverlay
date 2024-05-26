using System;
using System.Collections.Generic;
using System.IO;

namespace KeyOverlay;

public class Config
{
	private readonly string _name;
	private Dictionary<string, Dictionary<string, string>> _config;
	private readonly FileSystemWatcher _watcher;
	private readonly Action _callback;

	public Config(string name, Action callback)
	{
		this._config = null!; // suppress warning

		this._name = name;
		this._callback = callback;
		this.Load();

		this._watcher = new FileSystemWatcher
		{
			Path = ".",
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess,
			Filter = name
		};
		this._watcher.Changed += new FileSystemEventHandler(this.OnChanged);
		this._watcher.EnableRaisingEvents = true;
	}

	public Dictionary<string, string> this[string key] => this._config[key];

	private void Read()
	{
		Dictionary<string, Dictionary<string, string>> objectDict = new();
		string[] lines = File.ReadAllLines(this._name);
		string current = "";
		foreach (string line in lines)
		{
			if (line == "" || line.StartsWith("//")) continue;
			if (line.StartsWith("["))
			{
				current = line[1..^1];
				objectDict.Add(current, new());
			}
			else
			{
				string key = line.Split('=')[0];
				string value = line.Split('=')[1];
				objectDict[current].Add(key, value);
			}

		}
		this._config = objectDict;
	}

	private void Check()
	{
		string[] required = { "General", "Keys" };
		string[] optional = { "Display", "Size", "Colors" };
		foreach (string name in required)
		{
			if (!this._config.ContainsKey(name)) throw new InvalidDataException("Missing required data from config file");
		}
		foreach (string name in optional)
		{
			if (!this._config.ContainsKey(name)) this._config.Add(name, new());
		}
	}
	public void Load()
	{
		this._config = new();
		this.Read();
		this.Check();
	}

	private void OnChanged(object source, FileSystemEventArgs e)
	{
		// funky event *before* the file is actually saved
		System.Threading.Thread.Sleep(100);
		this.Load();
		this._callback();
	}
}
