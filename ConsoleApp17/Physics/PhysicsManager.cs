using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genbox.VelcroPhysics;
using Genbox.VelcroPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace ConsoleApp17.Physics;
internal class PhysicsManager : Component
{
    public World World { get; private set; }

    public event Action? BeforeStep;
    public event Action? AfterStep;

    public override void Initialize(Entity parent)
    {
        World = new(Vector2.Zero.AsXNA());
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
}
