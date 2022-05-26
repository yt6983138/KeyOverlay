using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace KeyOverlay
{
    public class AppWindow
    {
        private RenderWindow _window;
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
        private Clock _clock = new();
        private Config _config;
        private object _lock = new object();


        public AppWindow()
        {
            _window = new RenderWindow(new VideoMode(600, 800), "Key Overlay");
            _config = new Config("config.ini", Initialize);
            Initialize();
        }

        public void Initialize()
        {
            lock (_lock)
            {
                var general = _config["General"];

                _barSpeed = float.Parse(general["barSpeed"], CultureInfo.InvariantCulture);
                _backgroundColor = CreateItems.CreateColor(general["backgroundColor"]);
                _maxFPS = uint.Parse(general["fps"]);

                //create keys which will be used to create the squares and text
                _keyList = new List<Key>();
                foreach (var item in _config["Keys"])
                {
                    var key = new Key(item.Value);

                    if (_config["Display"].ContainsKey(item.Key))
                        key.setKeyLetter(_config["Display"][item.Key]);

                    if (_config["Colors"].ContainsKey(item.Key))
                        key.setColor(CreateItems.CreateColor(_config["Colors"][item.Key]));

                    if (_config["Size"].ContainsKey(item.Key))
                        key.setSize(uint.Parse(_config["Size"][item.Key]));

                    _keyList.Add(key);
                }

                //create squares and add them to _staticDrawables list
                _outlineThickness = int.Parse(general["outlineThickness"]);
                var keySize = int.Parse(general["keySize"]);
                var margin = int.Parse(general["margin"]);

                var windowWidth = margin;
                foreach (var key in _keyList)
                {
                    windowWidth += keySize * (int) key._size + _outlineThickness * 2 + margin;
                }

                var windowHeight = int.Parse(general["height"]);
                _size = new Vector2u((uint) windowWidth, (uint) windowHeight);

                //calculate screen ratio relative to original program size for easy resizing
                _ratioX = windowWidth / 480f;
                _ratioY = windowHeight / 960f;

                _staticDrawables = new List<Drawable>();
                _squareList = CreateItems.CreateKeys(_keyList, keySize, _ratioX, _ratioY, margin, _outlineThickness);
                foreach (var square in _squareList) _staticDrawables.Add(square);

                // //create text and add it to _staticDrawables list
                _fontColor = new Color(255, 255, 255, 255);
                for (var i = 0; i < _keyList.Count; i++)
                {
                    var text = CreateItems.CreateText(_keyList[i].KeyLetter, _squareList[i], _fontColor, false);
                    _staticDrawables.Add(text);
                }

                if (general["fading"] == "yes")
                    _fading = true;
                if (general["counter"] == "yes")
                    _counter = true;
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            _window.Close();
        }

        public void Run()
        {
            _window.Closed += OnClose;
            _window.SetFramerateLimit(_maxFPS);

            //Creating a sprite for the fading effect
            var fadingList = Fading.GetBackgroundColorFadingTexture(_backgroundColor, _window.Size.X, _ratioY);
            var fadingTexture = new RenderTexture(_window.Size.X, (uint)(255 * 2 * _ratioY));
            fadingTexture.Clear(Color.Transparent);
            if (_fading)
                foreach (var sprite in fadingList)
                    fadingTexture.Draw(sprite);
            fadingTexture.Display();
            var fadingSprite = new Sprite(fadingTexture.Texture);


            while (_window.IsOpen)
            {
                _window.DispatchEvents();

                _window.Size = _size;
                _window.SetView(new View(new FloatRect(0, 0, _size.X, _size.Y)));
                
                _window.Clear(_backgroundColor);
                
                lock (_lock) {
                    // //if no keys are being held fill the square with bg color
                    for (var i = 0; i < _keyList.Count; i++)
                    {
                        var key = _keyList[i];

                        if (key.isKey && Keyboard.IsKeyPressed(key.KeyboardKey) ||
                                !key.isKey && Mouse.IsButtonPressed(key.MouseButton))
                        {
                            key.Hold++;
                            _squareList[i].FillColor = key._colorPressed;
                        }
                        else
                        {
                            key.Hold = 0;
                            _squareList[i].FillColor = _backgroundColor;
                        }
                    }

                    MoveBars(_keyList, _squareList);

                    foreach (var staticDrawable in _staticDrawables) _window.Draw(staticDrawable);

                    for (var i = 0; i < _keyList.Count; i++)
                    {
                        var key = _keyList[i];

                        if (_counter)
                        {
                            var text = CreateItems.CreateText(
                                Convert.ToString(key.Counter),
                                    _squareList[i],
                                    Color.White,
                                    true
                                );
                            _window.Draw(text);
                        }
                        foreach (var bar in key.BarList) _window.Draw(bar);
                    }
                    _window.Draw(fadingSprite);
                }
                _window.Display();
            }
        }

        /// <summary>
        /// if a key is a new input create a new bar, if it is being held stretch it and move all bars up
        /// </summary>
        private void MoveBars(List<Key> keyList, List<RectangleShape> squareList)
        {
            var moveDist = _clock.Restart().AsSeconds() * _barSpeed;

            foreach (var key in keyList)
            {
                if (key.Hold == 1)
                {
                    var rect = CreateItems.CreateBar(squareList.ElementAt(keyList.IndexOf(key)), _outlineThickness,
                        moveDist);
                    key.BarList.Add(rect);
                    key.Counter++;
                }
                else if (key.Hold > 1)
                {
                    var rect = key.BarList.Last();
                    rect.Size = new Vector2f(rect.Size.X, rect.Size.Y + moveDist);
                }

                foreach (var rect in key.BarList)
                    rect.Position = new Vector2f(rect.Position.X, rect.Position.Y - moveDist);
                if (key.BarList.Count > 0 && key.BarList.First().Position.Y + key.BarList.First().Size.Y < 0)
                    key.BarList.RemoveAt(0);
            }
        }
    }
}
