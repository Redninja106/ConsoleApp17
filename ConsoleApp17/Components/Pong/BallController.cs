using ConsoleApp17.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Pong;
internal class BallController : Component
{
    PhysicsBody body;

    public Vector2 initialBallVelocity;

    public override void Initialize(Entity parent)
    {
        body = parent.GetComponent<PhysicsBody>() ?? throw new Exception();
        body.AddForce(initialBallVelocity);
        body.OnCollision += Body_OnCollision;
    }

    private void Body_OnCollision(Collider arg1, Collider arg2, Contact contact)
    {
        body.Velocity = Vector2.Reflect(body.Velocity, contact.Normal);
    }

    public override void Update()
    {
        if (Keyboard.IsKeyDown(Key.R))
        {
            Reset(null);
        }
    }

    public override void Render(ICanvas canvas)
    {
        canvas.DrawCircle(0, 0, .1f);
        base.Render(canvas);
    }

    public void Reset(Vector2? newVelocity)
    {
        if (newVelocity is not null)
            this.body.Velocity = newVelocity.Value;

        this.ParentTransform.Position = Vector2.Zero;
        body.SyncFromTransform();
    }
}
