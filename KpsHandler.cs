using System.Linq;
using System.Threading;

namespace KeyOverlay;
public class KpsHandler
{
	private byte index;
	private byte[] kps;

	public float Kps
	{
		get
		{
			return this.kps.Sum(x => x) / (float)10;
		}
	}

	public KpsHandler()
	{
		this.kps = new byte[10];
		Timer timer = new(this.Tick, null, 0, 100);
	}

	private void Tick(object? sender)
	{
		this.kps[this.index] = 0;
		if (++this.index >= 10)
		{
			this.index = 0;
		}
	}

	public void Update(byte keyCount)
	{
		for (byte i = 0; i < 10; i++)
		{
			this.kps[i] += keyCount;
		}
	}
}
