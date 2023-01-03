using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal static class VectorExtensions
{
    /// <summary>
    /// Rotates a vector around the origin.
    /// </summary>
    public static Vector2 Rotate(this Vector2 vector, float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        float x = cos * vector.X - sin * vector.Y;
        float y = sin * vector.X + cos * vector.Y;

        return new(x, y);
    }

    public static Microsoft.Xna.Framework.Vector2 AsXNA(this Vector2 vector)
    {
        return Unsafe.As<Vector2, Microsoft.Xna.Framework.Vector2>(ref vector);
    }
    public static Vector2 AsNumericsVector(this Microsoft.Xna.Framework.Vector2 vector)
    {
        return Unsafe.As<Microsoft.Xna.Framework.Vector2, Vector2>(ref vector);
    }

    public static Vector2 WithLength(this Vector2 vector, float length)
    {
        return vector.Normalized() * length;
    }
}
