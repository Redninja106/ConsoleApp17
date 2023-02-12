using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genbox.VelcroPhysics;
using Genbox.VelcroPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace ConsoleApp17.Physics;
public class PhysicsManager : Component
{
    public World World { get; private set; }
    private PhysicsDebugView debugView;

    public event Action? BeforeStep;
    public event Action? AfterStep;

    public override void Initialize(Entity parent)
    {
        Settings.UseConvexHullPolygons = false;
        World = new(Vector2.Zero.AsXNA());
        debugView = new(World);
    }

    public void UpdateWorld()
    {
        BeforeStep?.Invoke();

        const int steps = 1;

        for (int i = 0; i < steps; i++)
        {
            World.Step(Time.DeltaTime / steps);
        }

        AfterStep?.Invoke();
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
    }

    public Collider? TestPoint(Vector2 point)
    {
        Fixture? fixture = World.TestPoint(point.AsXNA());

        return fixture?.UserData as Collider;
    }
}
