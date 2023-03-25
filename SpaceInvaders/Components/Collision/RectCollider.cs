using ConsoleApp17;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Collision;
internal class RectCollider : Component
{
    public bool Visualize { get; set; } = true;

    internal readonly HashSet<RectCollider> collisions = new();

    public Rectangle Bounds
    { 
        get 
        { 
            return new(Offset, Size, Alignment.Center); 
        } 
        set 
        { 
            Size = value.Size;
            Offset = value.Center;
        } 
    }

    public Vector2 Size { get; set; }
    public Vector2 Offset { get; set; }

    public override void Initialize(Entity parent)
    {
        CollisionManager.colliders.Add(this);
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        if (Visualize)
        {
            canvas.Stroke(Color.Red);
            canvas.DrawRect(Bounds);
        }

        base.Render(canvas);
    }

    public override void Destroy()
    {
        CollisionManager.colliders.Remove(this);
        base.Destroy();
    }

    public Rectangle GetTransformedBounds()
    {
        Rectangle result = Bounds;
        result.Position += this.ParentTransform.WorldPosition;
        result.Size *= 2;
        return result;
    }
}