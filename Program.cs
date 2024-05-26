using System;
using System.IO;

namespace KeyOverlay;

internal class Program
{
	private static void Main()
	{
		AppWindow window;
		try
		{
			window = new AppWindow();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			using StreamWriter sw = new("Latest.log");
			sw.WriteLine(e.Message);
			throw;
		}
		window.Run();
	}
}