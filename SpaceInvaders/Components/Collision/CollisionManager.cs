using ConsoleApp17;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Collision;
internal class CollisionManager : Component
{
    internal static readonly List<RectCollider> colliders = new();

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        foreach (var collider in colliders)
        {
            collider.collisions.Clear();
        }

        for (int i = 0; i < colliders.Count; i++)
        {
            for (int j = 0; j < colliders.Count; j++)
            {
                if (i == j)
                    continue;

                var collider = colliders[i];
                var other = colliders[j];

                if (collider.GetTransformedBounds().Intersects(other.GetTransformedBounds()))
                {
                    collider.collisions.Add(other);
                    other.collisions.Add(collider);
                }
            }
        }
    }
}
