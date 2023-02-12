using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Physics;
public abstract class Collider : Component
{
    public float Density { get; set; } = 1.0f;
    public float Friction { get; set; } = 0.2f;
    public float Restitution { get; set; } = 0.0f;

    public PhysicsBody Body { get; private set; }
    private readonly List<Fixture> fixtures = new();

    public abstract IEnumerable<Shape> CreateShapes(float density);

    public override void Initialize(Entity parent)
    {
        Body = GetComponentInParents<PhysicsBody>() ?? throw new Exception();
        
        Invalidate();
    }

    public override void Update()
    {
    }

    protected void Invalidate()
    {
        RemoveFixtures();

        foreach (var shape in CreateShapes(Density))
        {
            FixtureDef def = new()
            {
                Shape = shape,
                Friction = Friction,
                Restitution = Restitution,
            };


            var fixture = Body.InternalBody.CreateFixture(def);

            fixture.UserData = this;

            fixtures.Add(fixture);
        }
    }

    public override void Destroy()
    {
        RemoveFixtures();

        base.Destroy();
    }

    void RemoveFixtures()
    {
        foreach (var f in fixtures)
        {
            Body.InternalBody.DestroyFixture(f);
        }

        fixtures.Clear();
    }
}
