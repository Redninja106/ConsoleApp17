using Genbox.VelcroPhysics.Collision.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Physics;
internal class CircleCollider : Collider
{
    public float Radius = 1.0f;

    public override Shape CreateShape(float density)
    {
        return new CircleShape(Radius, density);
    }
}
