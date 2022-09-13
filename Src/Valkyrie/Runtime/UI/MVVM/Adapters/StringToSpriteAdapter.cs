using System;
using System.Linq;
using UnityEngine;
using Utils;
using Valkyrie.Tools;
using Vector2 = UnityEngine.Vector2;

namespace Valkyrie.MVVM.Adapters
{
    public class StringToSpriteAdapter : IBindingAdapter
    {
        private static Sprite _transparent;

        private static Sprite Transparent
        {
            get
            {
                if (_transparent == null)
                {
                    // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
                    var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
 
                    // set the pixel values
                    texture.SetPixel(0, 0, Color.clear);
                    texture.SetPixel(1, 0, Color.clear);
                    texture.SetPixel(0, 1, Color.clear);
                    texture.SetPixel(1, 1, Color.clear);
 
                    // Apply all SetPixel calls
                    texture.Apply();

                    _transparent = Sprite.Create(texture,
                        new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * 0.5f);
                }

                return _transparent;
            }
        }

        private string _lastValue;

        private string ImagePath
        {
            set
            {
                if(_lastValue == value)
                    return;
                _lastValue = value;

                string imagePath = null;
                int imageIndex = -1;
                string spriteName = null;

                var datas = value?.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
                switch (datas.Length)
                {
                    case 1:
                        imagePath = datas[0];
                        imageIndex = -1;
                        spriteName = null;
                        break;
                    case 2:
                        imagePath = datas[0];
                        if (!int.TryParse(datas[1], out imageIndex))
                        {
                            spriteName = datas[1];
                            imageIndex = -1;
                        }
                        break;
                }

                if (imagePath.IsNullOrEmpty())
                    _sprite = Transparent;
                else
                {
                    if (imageIndex >= 0 || spriteName.NotNullOrEmpty())
                    {
                        var allSprites = Resources.LoadAll<Sprite>(imagePath);
                        _sprite = imageIndex >= 0 ? allSprites[imageIndex] : allSprites.FirstOrDefault(u => u.name == spriteName);
                    }
                    else
                        _sprite = Resources.Load<Sprite>(imagePath);
                }

                if (_sprite == null)
                    _sprite = Transparent;
            }
        }
        
        public bool IsAvailableSourceType(Type type)
        {
            return type == typeof(string);
        }

        public Type GetResultType()
        {
            return typeof(Sprite);
        }

        public object Convert(object source)
        {
            ImagePath = (string) source;
            return _sprite;
        }

        Sprite _sprite;
    }
}