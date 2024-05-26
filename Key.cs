using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace KeyOverlay;

public class Key
{
	private Color _color;

	public int Hold { get; set; }
	public List<RectangleShape> BarList { get; set; } = new();
	public string KeyLetter { get; set; } = "";
	public Keyboard.Key KeyboardKey { get; }
	public Mouse.Button MouseButton { get; }
	public int Counter { get; set; } = 0;
	public bool IsKey { get; } = true;
	public Color ColorPressed { get; private set; }
	public Color Color
	{
		get => this._color;
		set
		{
			this._color = value;
			this.ColorPressed = new Color(value.R, value.G, value.B, (byte)(value.A / 1.618));
		}
	}
	public uint Size { get; set; } = 1;

	public Key(string key)
	{
		this.Color = Color.White;
		this.KeyLetter = key;
		if (!Enum.TryParse(key, out Keyboard.Key keyboardKey))
		{
			if (this.KeyLetter[0] == 'm')
			{
				this.KeyLetter = this.KeyLetter.Remove(0, 1);
			}
			if (Enum.TryParse(key[1..], out Mouse.Button mouseButton))
			{
				this.IsKey = false;
				this.MouseButton = mouseButton;
			}
			else
			{
				string exceptName = "Invalid key " + key;
				throw new InvalidOperationException(exceptName);
			}
		}

		this.KeyboardKey = keyboardKey;
	}
}