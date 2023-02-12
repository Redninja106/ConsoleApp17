using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp17.Components.Asteroid.Algorithms;

namespace ConsoleApp17.Components.Asteroid;
internal abstract class AsteroidBrush
{
    public abstract Rectangle GetBounds(Vector2 position);

    public abstract void ApplyTo(ref float value, Vector2 valuePosition, Vector2 brushPosition);
}
