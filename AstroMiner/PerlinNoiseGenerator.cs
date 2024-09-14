using System;

namespace AstroMiner;

public class PerlinNoiseGenerator
{
    private const int GradientSizeTable = 256;
    private readonly float[] _gradientsX = new float[GradientSizeTable];
    private readonly float[] _gradientsY = new float[GradientSizeTable];
    private readonly int[] _p; // Doubled permutation array for overflow
    private readonly int[] _permutation;
    private readonly Random _random;

    public PerlinNoiseGenerator(int seed)
    {
        _random = new Random(seed);
        _permutation = new int[GradientSizeTable];
        _p = new int[GradientSizeTable * 2];
        InitGradients();
        InitPermutationTable();
    }

    private void InitGradients()
    {
        for (var i = 0; i < GradientSizeTable; i++)
        {
            // Random angle for gradient
            var angle = _random.NextDouble() * Math.PI * 2;
            _gradientsX[i] = (float)Math.Cos(angle);
            _gradientsY[i] = (float)Math.Sin(angle);
        }
    }

    private void InitPermutationTable()
    {
        for (var i = 0; i < GradientSizeTable; i++) _permutation[i] = i;

        // Shuffle the permutation table
        for (var i = 0; i < GradientSizeTable; i++)
        {
            var swapIndex = _random.Next(GradientSizeTable);
            (_permutation[i], _permutation[swapIndex]) = (_permutation[swapIndex], _permutation[i]);
        }

        // Duplicate the permutation table
        for (var i = 0; i < GradientSizeTable * 2; i++) _p[i] = _permutation[i % GradientSizeTable];
    }

    // Fade function to smooth the interpolation
    private float Fade(float t)
    {
        // 6t^5 - 15t^4 + 10t^3
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    // Linear interpolation
    private float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    // Dot product of gradient vector and distance vector
    private float DotGridGradient(int ix, int iy, float x, float y)
    {
        // Compute the distance vector
        var dx = x - ix;
        var dy = y - iy;

        // Compute the gradient index
        var index = _p[(_p[ix & 255] + iy) & 255];

        // Get the gradient vector
        var gx = _gradientsX[index];
        var gy = _gradientsY[index];

        // Compute the dot product
        return dx * gx + dy * gy;
    }

    // Perlin noise function
    public float Noise(float x, float y)
    {
        // Grid cell coordinates
        var x0 = (int)Math.Floor(x);
        var x1 = x0 + 1;
        var y0 = (int)Math.Floor(y);
        var y1 = y0 + 1;

        // Interpolation weights
        var sx = Fade(x - x0);
        var sy = Fade(y - y0);

        // Dot products at each corner
        float n0, n1, ix0, ix1, value;

        n0 = DotGridGradient(x0, y0, x, y);
        n1 = DotGridGradient(x1, y0, x, y);
        ix0 = Lerp(n0, n1, sx);

        n0 = DotGridGradient(x0, y1, x, y);
        n1 = DotGridGradient(x1, y1, x, y);
        ix1 = Lerp(n0, n1, sx);

        value = Lerp(ix0, ix1, sy);

        // Scale to 0 - 1
        return (value + 1) / 2f;
    }
}