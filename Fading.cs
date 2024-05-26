using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace KeyOverlay;
public static class Fading
{
	public static List<Sprite> GetBackgroundColorFadingTexture(
		Color backgroundColor,
		uint windowWidth,
		float ratioY)
	{
		List<Sprite> sprites = new();
		int alpha = 255;
		Color color = backgroundColor;
		for (int i = 0; i < 255; i++)
		{
			Image img = ratioY >= .5f ? new Image(windowWidth, (uint)(2 * ratioY), color) : new Image(windowWidth, 1, color);
			Sprite sprite = new(new Texture(img))
			{
				Position = new Vector2f(0, (img.Size.Y * i) - 1)
			};
			sprites.Add(sprite);
			alpha -= 1;
			color.A = (byte)alpha;
		}

		return sprites;
	}
}
