using SimulationFramework.SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.OLD.Units;
internal class ShipController : Component
{
    public Vector2 targetPosition;
    public float thrust = 50;
    public bool isSelected = false;
    private Sprite ship;
    private static Circle highlightCircle = new(0, 0, .5f);
    private readonly Queue<Vector2> path = new();

    private float angularVelocity;
    private Vector2 velocity;

    public override void Initialize(Entity parent)
    {
        targetPosition = ParentEntity.Transform.Position;
        ship = new Sprite("Assets/SpaceShooterRedux/playerShip1_red.png", new(1, 1));
    }

    public override void Update()
    {
        //Vector2 direction = (targetPosition - this.ParentEntity.Transform.Position).Normalized() * speed * Time.DeltaTime;

        //float distance = MathF.Min(direction.Length(), Vector2.Distance(targetPosition, this.ParentEntity.Transform.Position));

        //this.ParentEntity.Transform.Position += direction.Normalized() * distance;

        float distance = Vector2.Distance(targetPosition, ParentEntity.Transform.Position);
        Vector2 direction = (targetPosition - ParentEntity.Transform.Position).Normalized();

        float stoppingDistance = velocity.LengthSquared() / 2;

        velocity *= MathF.Pow(.5f, Time.DeltaTime);
        velocity = velocity.Normalized() * MathF.Min(velocity.Length(), 5f);

        if (distance > stoppingDistance || Vector2.Dot(direction, velocity.Normalized()) < .9f)
        {
            velocity += direction * Time.DeltaTime * thrust;
            Console.WriteLine("plus");
        }
        else
        {
            velocity -= direction * Time.DeltaTime * thrust;
            Console.WriteLine("minus");
        }

        ParentEntity.Transform.Position += velocity * Time.DeltaTime;
        ParentEntity.Transform.Rotation += angularVelocity * Time.DeltaTime;

        if ((ParentEntity.Transform.Position - targetPosition).LengthSquared() < 0.05f && path.Any())
        {
            targetPosition = path.Dequeue();
        }
    }

    public override void Render(ICanvas canvas)
    {
        if (isSelected)
        {
            canvas.Stroke(Color.CornflowerBlue);
            canvas.StrokeWidth(0.1f);
            canvas.Rotate(Time.TotalTime);
            canvas.DrawCircle(0, 0, .5f);
            canvas.Rotate(-Time.TotalTime);

            canvas.PopState();
            canvas.PushState();

            canvas.Stroke(Color.CornflowerBlue);
            canvas.StrokeWidth(0.1f);
            canvas.DrawLine(ParentEntity.Transform.Position, targetPosition);
            Vector2 prevPoint = targetPosition;
            foreach (var point in path)
            {
                canvas.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            canvas.PopState();
            canvas.PushState();
            ParentEntity.Transform.ApplyTo(canvas);
        }

        canvas.DrawSprite(ship);
    }

    public void SetTarget(Vector2 point, bool queuePoint)
    {
        if (queuePoint)
        {
            path.Enqueue(point);
        }
        else
        {
            path.Clear();
            targetPosition = point;
        }
    }
}