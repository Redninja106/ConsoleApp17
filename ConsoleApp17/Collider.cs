using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class Collider : Component
{
    public static readonly CollisionHandler Handler = new();

    public Circle Bounds = new(0, 0, .5f);

    public override void Initialize(Entity parent)
    {
        Handler.AddCollider(this);
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        // canvas.Stroke(Color.Red);
        // canvas.StrokeWidth(0f);
        // canvas.DrawCircle(Bounds);
    }

    public Circle GetWorldSpaceBounds()
    {
        return new(ParentEntity.Transform.LocalToWorld(Bounds.Position), Bounds.Radius);
    }
}
