using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Physics;
internal abstract class Collider : Component
{
    private static readonly List<Collider> allColliders = new();

    private Shape shape;
    private Fixture fixture;
    public PhysicsBody Body { get; private set; }

    public abstract Shape CreateShape(float density);

    public override void Initialize(Entity parent)
    {
        allColliders.Add(this);

        Body = parent.GetComponent<PhysicsBody>() ?? throw new Exception();
        
        this.shape = CreateShape(1.0f);

        var def = new FixtureDef()
        {
            Shape = shape,
        };

        fixture = Body.InternalBody.CreateFixture(def);
    }

    public override void Update()
    {
    }

    public static Collider GetFromFixture(Fixture fixture)
    {
        return allColliders.Single(c => c.fixture == fixture);
    }
}
