using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyOverlay;

public static class ItemFactory
{
	public static Font Font { get; } = new(
		new DirectoryInfo("./Resources/")
			.GetFiles()
			.First(file => file.Name.ToLower().EndsWith(".ttf")) // search instead
			.FullName); // ok im too lazy to make config
	public static RectangleShape CreateBar(RectangleShape square, int outlineThickness, float barSpeed)
	{
		RectangleShape rect = new(new Vector2f(square.Size.X + (outlineThickness * 2), barSpeed))
		{
			Position = new Vector2f(square.Position.X - outlineThickness,
				square.Position.Y - square.Size.Y - square.OutlineThickness),
			FillColor = square.FillColor
		};
		return rect;
	}

	public static List<RectangleShape> CreateKeys(List<Key> keys, float size, float ratioX, float ratioY,
		int margin, int outlineThickness)
	{
		int keyAmount = keys.Count;
		int xPos = margin + outlineThickness;
		float yPos = 900 * ratioY;
		List<RectangleShape> keyList = new();

		foreach (Key key in keys)
		{
			RectangleShape square = new(new Vector2f(size * key.Size, size));
			int width = (int)(size * key.Size);

			square.FillColor = CreateColor("0,0,0,0");
			square.OutlineColor = key.Color;
			square.OutlineThickness = outlineThickness;
			square.Origin = new Vector2f(0, size);
			square.Position = new Vector2f(xPos, yPos);
			xPos += width + (outlineThickness * 2) + margin;
			keyList.Add(square);
		}
		return keyList;
	}

	public static Text CreateText(string key, RectangleShape square, Color color, bool counter)
	{
		Text text = new(key, Font)
		{
			CharacterSize = (uint)(50 * square.Size.Y / 140),
			Style = Text.Styles.Bold,
			FillColor = color
		};
		text.Origin = new Vector2f(text.GetLocalBounds().Width / 2f, 32 * square.Size.Y / 140f);
		if (counter)
		{
			text.Position = new Vector2f(square.GetGlobalBounds().Left + square.OutlineThickness + (square.Size.X / 2f),
				square.GetGlobalBounds().Top + square.OutlineThickness + square.Size.Y + text.CharacterSize);
		}
		else
		{
			text.Position = new Vector2f(square.GetGlobalBounds().Left + square.OutlineThickness + (square.Size.X / 2f),
				square.GetGlobalBounds().Top + square.OutlineThickness + (square.Size.Y / 2f));
		}

		return text;
	}

	public static Color CreateColor(string s)
	{
		byte[] bytes = s.Split(',').Select(int.Parse).Select(Convert.ToByte).ToArray();
		return new Color(bytes[0], bytes[1], bytes[2], bytes[3]);
	}
}