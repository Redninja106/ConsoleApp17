using ConsoleApp17.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Dev;
internal class BoxSpawner : Component
{
    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        if (Mouse.IsButtonPressed(MouseButton.Middle))
        {
            var box = Entity.Create(Scene.Active);

            box.AddComponent<BoxRenderer>();
            box.AddComponent<PhysicsBody>();
            box.AddComponent<BoxCollider>();

            box.Transform.Position = Camera.Active.ScreenToWorld(Mouse.Position);
        }
    }
}
