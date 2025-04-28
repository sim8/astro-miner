using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;

namespace AstroMiner.Tests.UI;

// Mock implementation of BaseGame for testing
public class MockBaseGame([CanBeNull] Dictionary<string, Texture2D> textures) : BaseGame
{
    public Dictionary<string, Texture2D> Textures { get; } = textures;

    // required abstract method
    protected override void InitializeControls()
    {
    }
}