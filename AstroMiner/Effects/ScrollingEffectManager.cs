using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Effects;

public class ScrollingEffectLayer
{
    public string TextureName { get; set; }
    public int TextureSize { get; set; }
    public float Speed { get; set; } // pixels per second
    public float Density { get; set; } // items per 100,000 square pixels
    public float MinOpacity { get; set; } = 1.0f;
    public float MaxOpacity { get; set; } = 1.0f;
    public Color Tint { get; set; } = Color.White;

    public List<ScrollingItem> Items { get; set; } = new();
    public Random Random { get; set; } = new();
    private bool _initialized = false;

    public void Initialize(int screenWidth, int screenHeight)
    {
        if (_initialized) return;

        Items.Clear();

        // Calculate number of items based on density and screen area
        var screenArea = screenWidth * screenHeight;
        var itemCount = (int)(Density * screenArea / 100000f);

        // Place items randomly across extended area (including off-screen spawn zones)
        var extendedWidth = screenWidth + TextureSize * 2;
        var extendedHeight = screenHeight + TextureSize * 2;

        for (int i = 0; i < itemCount; i++)
        {
            var x = Random.Next(-TextureSize, extendedWidth - TextureSize);
            var y = Random.Next(-TextureSize, extendedHeight - TextureSize);
            var opacity = (float)(MinOpacity + Random.NextDouble() * (MaxOpacity - MinOpacity));

            Items.Add(new ScrollingItem { X = x, Y = y, Opacity = opacity });
        }

        _initialized = true;
    }

    public void Update(float deltaTime, int screenWidth, int screenHeight)
    {
        if (!_initialized) Initialize(screenWidth, screenHeight);

        // Move all items to the left
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            item.X -= Speed * deltaTime;

            // If item has moved completely off the left side, recycle it to the right
            if (item.X < -TextureSize)
            {
                item.X = screenWidth + Random.Next(0, TextureSize);
                item.Y = Random.Next(0, screenHeight);
                item.Opacity = (float)(MinOpacity + Random.NextDouble() * (MaxOpacity - MinOpacity));
            }

            Items[i] = item;
        }
    }

    public void Render(SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures)
    {
        var texture = textures[TextureName];

        foreach (var item in Items)
        {
            var destRect = new Rectangle((int)item.X, (int)item.Y, TextureSize, TextureSize);
            var color = Tint * item.Opacity;
            spriteBatch.Draw(texture, destRect, color);
        }
    }
}

public struct ScrollingItem
{
    public float X;
    public float Y;
    public float Opacity;
}

public class ScrollingEffectManager
{
    public List<ScrollingEffectLayer> Layers { get; set; } = new();

    public void Update(float deltaTime, int screenWidth, int screenHeight)
    {
        foreach (var layer in Layers)
        {
            layer.Update(deltaTime, screenWidth, screenHeight);
        }
    }

    public void Render(SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures)
    {
        foreach (var layer in Layers)
        {
            layer.Render(spriteBatch, textures);
        }
    }

    public void AddLayer(ScrollingEffectLayer layer)
    {
        Layers.Add(layer);
    }

    public void Clear()
    {
        Layers.Clear();
    }
}
