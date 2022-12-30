using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class CollisionHandler
{
    private readonly List<Collider> colliders = new();

    internal CollisionHandler()
    {

    }

    public void AddCollider(Collider collider)
    {
        colliders.Add(collider);
    }

    public bool GetCollision(Collider collider, out Collider? other)
    {
        foreach (var c in colliders)
        {
            if (collider == c)
                continue;

            if (CirclesColliding(collider.GetWorldSpaceBounds(), c.GetWorldSpaceBounds()))
            {
                other = c;
                return true;
            }
        }

        other = null;
        return false;
    }

    public bool GetCollisionAtPoint(Vector2 point, out Collider? other)
    {
        foreach (var c in colliders)
        {
            if (PointInCircle(point, c.GetWorldSpaceBounds()))
            {
                other = c;
                return true;
            }
        }

        other = null;
        return false;
    }

    private bool CirclesColliding(Circle circleA, Circle circleB)
    {
        var distanceSquared = Vector2.DistanceSquared(circleA.Position, circleB.Position);
        var radiusSquared = (circleA.Radius + circleB.Radius) * (circleA.Radius + circleB.Radius);

        return distanceSquared < radiusSquared;
    }

    private bool PointInCircle(Vector2 point, Circle circle)
    {
        return CirclesColliding(circle, new(point, 0));
    }
}
