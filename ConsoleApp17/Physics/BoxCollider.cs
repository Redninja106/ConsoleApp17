using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Physics;
internal class BoxCollider : Collider
{
    public float Width = 1;
    public float Height = 1;

    public override Shape CreateShape(float density)
    {
        var shape = new PolygonShape(density);
        shape.SetAsBox(Width / 2, Height / 2);
        return shape;
    }
}
