using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Extensions.DebugView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Physics;
internal class PhysicsDebugView : DebugViewBase
{
    public ICanvas canvas;

    public PhysicsDebugView(World world) : base(world)
    {
    }

    public override void DrawCircle(XNAVector2 center, float radius, Microsoft.Xna.Framework.Color color)
    {
    }

    public override void DrawPolygon(XNAVector2[] vertices, int count, Microsoft.Xna.Framework.Color color, bool closed = true)
    {
    }

    public override void DrawSegment(XNAVector2 start, XNAVector2 end, Microsoft.Xna.Framework.Color color)
    {
    }

    public override void DrawSolidCircle(XNAVector2 center, float radius, XNAVector2 axis, Microsoft.Xna.Framework.Color color)
    {
    }

    public override void DrawSolidPolygon(XNAVector2[] vertices, int count, Microsoft.Xna.Framework.Color color, bool outline = true)
    {
    }

    public override void DrawTransform(ref Genbox.VelcroPhysics.Shared.Transform transform)
    {
    }
}
