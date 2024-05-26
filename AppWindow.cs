using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KeyOverlay;

public class AppWindow
{
	private readonly RenderWindow _window;
	private Vector2u _size;
	private List<Key> _keyList;
	private List<RectangleShape> _squareList;
	private float _barSpeed;
	private float _ratioX;
	private float _ratioY;
	private int _outlineThickness;
	private Color _backgroundColor;
	private Color _fontColor;
	private bool _fading;
	private bool _counter;
	private List<Drawable> _staticDrawables;
	private uint _maxFPS;
	private readonly Clock _clock = new();
	private readonly Config _config;
	private Key? _hideKey;
	private readonly object _lock = new();
	private readonly KpsHandler _kpsHandler = new();
	private int _kpsPositionValue;
	private Color _kpsColor;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public AppWindow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	{
		this._window = new RenderWindow(new VideoMode(600, 800), "Key Overlay");
		this._config = new Config("config.ini", this.Initialize);
		this.Initialize();
	}

	public void Initialize()
	{
		lock (this._lock)
		{
			Dictionary<string, string> general = this._config["General"];

			this._kpsPositionValue = int.Parse(general["kpsPosition"]);
			this._barSpeed = float.Parse(general["barSpeed"], CultureInfo.InvariantCulture);
			this._backgroundColor = ItemFactory.CreateColor(general["backgroundColor"]);
			this._maxFPS = uint.Parse(general["fps"]);
			this._hideKey = general.TryGetValue("hideKey", out string? k) ? new Key(k) : null;
			this._kpsColor = ItemFactory.CreateColor(this._config["Colors"]["kps"]);

			//create keys which will be used to create the squares and text
			this._keyList = new List<Key>();
			foreach (KeyValuePair<string, string> item in this._config["Keys"])
			{
				Key key = new(item.Value);

				if (this._config["Display"].ContainsKey(item.Key))
					key.KeyLetter = this._config["Display"][item.Key];

				if (this._config["Colors"].ContainsKey(item.Key))
					key.Color = ItemFactory.CreateColor(this._config["Colors"][item.Key]);

				if (this._config["Size"].ContainsKey(item.Key))
					key.Size = uint.Parse(this._config["Size"][item.Key]);

				this._keyList.Add(key);
			}

			//create squares and add them to _staticDrawables list
			this._outlineThickness = int.Parse(general["outlineThickness"]);
			int keySize = int.Parse(general["keySize"]);
			int margin = int.Parse(general["margin"]);

			int windowWidth = margin;
			foreach (Key key in this._keyList)
			{
				windowWidth += (keySize * (int)key.Size) + (this._outlineThickness * 2) + margin;
			}

			int windowHeight = int.Parse(general["height"]);
			this._size = new Vector2u((uint)windowWidth, (uint)windowHeight);

			//calculate screen ratio relative to original program size for easy resizing
			this._ratioX = windowWidth / 480f;
			this._ratioY = windowHeight / 960f;

			this._staticDrawables = new List<Drawable>();
			this._squareList = ItemFactory.CreateKeys(this._keyList, keySize, this._ratioX, this._ratioY, margin, this._outlineThickness);
			foreach (RectangleShape square in this._squareList) this._staticDrawables.Add(square);

			// //create text and add it to _staticDrawables list
			this._fontColor = new Color(255, 255, 255, 255);
			for (int i = 0; i < this._keyList.Count; i++)
			{
				Text text = ItemFactory.CreateText(this._keyList[i].KeyLetter, this._squareList[i], this._fontColor, false);
				this._staticDrawables.Add(text);
			}

			if (general["fading"] == "yes")
				this._fading = true;
			if (general["counter"] == "yes")
				this._counter = true;
		}
	}

	private void OnClose(object? sender, EventArgs e)
	{
		this._window.Close();
	}

	public void Run()
	{
		this._window.Closed += this.OnClose;
		this._window.SetFramerateLimit(this._maxFPS);

		//Creating a sprite for the fading effect
		List<Sprite> fadingList = Fading.GetBackgroundColorFadingTexture(
			this._backgroundColor,
			this._size.X,
			this._ratioY);

		RenderTexture fadingTexture = new(this._size.X, (uint)(255 * 2 * this._ratioY));
		fadingTexture.Clear(Color.Transparent);
		if (this._fading)
		{
			foreach (Sprite sprite in fadingList)
				fadingTexture.Draw(sprite);
		}

		fadingTexture.Display();
		Sprite fadingSprite = new(fadingTexture.Texture);

		bool activeState = true;
		while (this._window.IsOpen)
		{
			this._window.DispatchEvents();

			this._window.Size = this._size;
			this._window.SetView(new View(new FloatRect(0, 0, this._size.X, this._size.Y)));

			this._window.Clear(this._backgroundColor);

			if (this._hideKey is not null)
			{
				bool pressed = Keyboard.IsKeyPressed(this._hideKey.KeyboardKey);
				if (pressed)
				{
					this._hideKey.Hold++;
				}
				else
				{
					this._hideKey.Hold = 0;
				}

				if (activeState && this._hideKey.Hold == 1)
				{
					this._window.SetVisible(false);
					activeState = false;
					continue;
				}
				else if (!activeState && this._hideKey.Hold == 1)
				{
					this._window.SetVisible(true);
					activeState = true;
				}
			}

			lock (this._lock)
			{
				this._window.Clear(this._backgroundColor);
				// //if no keys are being held fill the square with bg color

				for (int i = 0; i < this._keyList.Count; i++)
				{
					Key key = this._keyList[i];

					if ((key.IsKey && Keyboard.IsKeyPressed(key.KeyboardKey)) ||
							(!key.IsKey && Mouse.IsButtonPressed(key.MouseButton)))
					{
						key.Hold++;
						this._squareList[i].FillColor = key.ColorPressed;
					}
					else
					{
						key.Hold = 0;
						this._squareList[i].FillColor = this._backgroundColor;
					}
				}

				this.MoveBars(this._keyList, this._squareList);

				foreach (Drawable staticDrawable in this._staticDrawables) this._window.Draw(staticDrawable);
				byte keys = 0;

				for (int i = 0; i < this._keyList.Count; i++)
				{
					Key key = this._keyList[i];

					if (key.Hold == 1 && this._kpsPositionValue > -1)
					{
						keys++;
					}

					if (this._counter)
					{
						Text text = ItemFactory.CreateText(
								Convert.ToString(key.Counter),
								this._squareList[i],
									Color.White,
									true
								);
						this._window.Draw(text);
					}
					foreach (RectangleShape bar in key.BarList) this._window.Draw(bar);
				}
				if (this._kpsPositionValue > -1)
				{
					this._kpsHandler.Update(keys);
					Text text2 = new(this._kpsHandler.Kps.ToString(), ItemFactory.Font)
					{
						FillColor = this._kpsColor
					};
					text2.Position = new((this._window.Size.X / 2) - (text2.GetLocalBounds().Width / 2), this._kpsPositionValue);
					this._window.Draw(text2);
				}

				this._window.Draw(fadingSprite);
			}

			this._window.Display();
		}
	}

	/// <summary>
	/// if a key is a new input create a new bar, if it is being held stretch it and move all bars up
	/// </summary>
	private void MoveBars(List<Key> keyList, List<RectangleShape> squareList)
	{
		float moveDist = this._clock.Restart().AsSeconds() * this._barSpeed;

		foreach (Key key in keyList)
		{
			if (key.Hold == 1)
			{
				RectangleShape rect = ItemFactory.CreateBar(squareList.ElementAt(keyList.IndexOf(key)), this._outlineThickness,
						moveDist);
				key.BarList.Add(rect);
				key.Counter++;
			}
			else if (key.Hold > 1)
			{
				RectangleShape rect = key.BarList.Last();
				rect.Size = new Vector2f(rect.Size.X, rect.Size.Y + moveDist);
			}

			foreach (RectangleShape rect in key.BarList)
				rect.Position = new Vector2f(rect.Position.X, rect.Position.Y - moveDist);
			if (key.BarList.Count > 0 && key.BarList.First().Position.Y + key.BarList.First().Size.Y < 0)
				key.BarList.RemoveAt(0);
		}
	}
}
