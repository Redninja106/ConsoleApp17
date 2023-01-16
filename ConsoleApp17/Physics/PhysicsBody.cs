using ExCSS;
using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Physics;
internal class PhysicsBody : Component
{
    public Vector2 Velocity
    {
        get => InternalBody.LinearVelocity.AsNumericsVector();
        set => InternalBody.ApplyLinearImpulse(InternalBody.Mass * (value.AsXNA() - InternalBody.LinearVelocity));
    }

    public float AngularVeloctiy
    {
        get => InternalBody.AngularVelocity;
        set => InternalBody.AngularVelocity = value;
    }

    public bool Fixed = false;
    public bool Kinematic = false;

    public Body InternalBody { get; private set; }

    public event Action<Collider, Collider, Contact>? OnCollision;

    public override void Initialize(Entity parent)
    {
        BodyDef bodyDef = new()
        {
            Position = parent.Transform.Position.AsXNA(),
            Angle = parent.Transform.Rotation,
            Type = Fixed ? BodyType.Static : (Kinematic ? BodyType.Kinematic : BodyType.Dynamic),
        };

        InternalBody = Scene.Active.Physics.World.CreateBody(bodyDef);

        InternalBody.OnCollision = this.HandleCollision;

        Scene.Active.Physics.AfterStep += Physics_AfterStep;
        Scene.Active.Physics.BeforeStep += Physics_BeforeStep;
    }

    private void HandleCollision(Fixture fixtureA, Fixture fixtureB, Genbox.VelcroPhysics.Collision.ContactSystem.Contact contact)
    {
        var colliderA = (Collider)fixtureA.UserData;
        var colliderB = (Collider)fixtureB.UserData;

        Contact c = new(contact.Manifold.LocalPoint.AsNumericsVector(), contact.Manifold.LocalNormal.AsNumericsVector());
        OnCollision?.Invoke(colliderA, colliderB, c);
    }

    private void Physics_AfterStep()
    {
        SyncToTransform();
    }

    private void Physics_BeforeStep()
    {
        SyncFromTransform();
    }

    public void SyncToTransform()
    {
        if (this.InternalBody.Position.AsNumericsVector() != this.ParentEntity.Transform.Position || this.InternalBody.Rotation != this.ParentEntity.Transform.Rotation)
        {
            this.ParentEntity.Transform.Position = this.InternalBody.Position.AsNumericsVector();
            this.ParentEntity.Transform.Rotation = this.InternalBody.Rotation;
        }
    }

    public void SyncFromTransform()
    {
        if (this.InternalBody.Position.AsNumericsVector() != this.ParentEntity.Transform.Position || this.InternalBody.Rotation != this.ParentEntity.Transform.Rotation)
        {
            this.InternalBody.SetTransform(ParentEntity.Transform.Position.AsXNA(), ParentEntity.Transform.Rotation);
            this.InternalBody.Awake = true;
        }
    }

    public override void Update()
    {
        
    }

    public void AddForce(Vector2 force)
    {
        InternalBody.ApplyForce(force.AsXNA());
    }

    public void AddImpulse(Vector2 impulse)
    {
        InternalBody.ApplyLinearImpulse(impulse.AsXNA());
    }
}