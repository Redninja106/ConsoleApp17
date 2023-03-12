using ConsoleApp17;
using ConsoleApp17.Physics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Components.Player;
internal class PlayerController : Component
{
    public float MaxSpeed { get; set; }
    public float Acceleration { get; set; }


    private PhysicsBody body;
    private Entity rendererEntity;

    public override void Initialize(Entity parent)
    {
        body = parent.GetComponent<PhysicsBody>() ?? throw new Exception();
        body.InternalBody.FixedRotation = true;

        rendererEntity = parent.GetComponent<Entity>(c => c is Entity e && e.HasComponent<PlayerRenderer>());
    }

    public override void Update()
    {
        var diff = Camera.Active.ScreenToWorld(Mouse.Position) - ParentEntity.Transform.Position;

        rendererEntity.Transform.Rotation = MathF.Atan2(diff.Y, diff.X);

        Vector2 moveDirection = Vector2.Zero;

        if (Keyboard.IsKeyDown(Key.W))
            moveDirection += Vector2.UnitY;

        if (Keyboard.IsKeyDown(Key.A))
            moveDirection -= Vector2.UnitX;

        if (Keyboard.IsKeyDown(Key.S))
            moveDirection -= Vector2.UnitY;

        if (Keyboard.IsKeyDown(Key.D))
            moveDirection += Vector2.UnitX;

        if (moveDirection == Vector2.Zero)
        {
            moveDirection = -body.Velocity;
        }

        body.InternalBody.LinearDamping = .9f;

        if (moveDirection.LengthSquared() > 1)
            moveDirection = moveDirection.Normalized();

        body.AddForce(moveDirection * 5000 * Time.DeltaTime);
    }
}
