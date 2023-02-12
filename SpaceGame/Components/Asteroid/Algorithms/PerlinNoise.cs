using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Asteroid.Algorithms;

// https://en.wikipedia.org/wiki/Perlin_noise#Implementation
internal class PerlinNoise
{
    int seed;

    public PerlinNoise(int seed)
    {
        this.seed = seed;
    }

    public float Sample(Vector2 position)
    {
        int x0 = (int)MathF.Floor(position.X);
        int x1 = x0 + 1;
        int y0 = (int)MathF.Floor(position.Y);
        int y1 = y0 + 1;

        float sx = position.X - x0;
        float sy = position.Y - y0;

        float n0, n1, ix0, ix1;

        n0 = GetGradient(x0, y0, position.X, position.Y);
        n1 = GetGradient(x1, y0, position.X, position.Y);
        ix0 = MathHelper.Lerp(n0, n1, sx);

        n0 = GetGradient(x0, y1, position.X, position.Y);
        n1 = GetGradient(x1, y1, position.X, position.Y);
        ix1 = MathHelper.Lerp(n0, n1, sx);

        return MathHelper.Lerp(ix0, ix1, sy);
    }

    private float GetGradient(int ix, int iy, float x, float y)
    {
        var random = new Random(unchecked(seed ^ ~ix ^ ~iy + ix + (iy<<17)));
        float angle = random.NextSingle() * MathF.Tau;
        Vector2 gradient = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

        // Compute the distance vector
        float dx = x - ix;
        float dy = y - iy;

        // Compute the dot-product
        return (dx * gradient.X + dy * gradient.Y);
    }
}
