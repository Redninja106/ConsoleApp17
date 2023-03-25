using ConsoleApp17;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Projectile;
internal class ProjectileController : Component
{
    public float Speed { get; set; }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        this.ParentTransform.Position += Vector2.UnitY * Speed * Time.DeltaTime;

        if (this.ParentTransform.Position.Y > 10)
            this.ParentEntity.Destroy();
    }

    public override void Render(ICanvas canvas)
    {
        canvas.DrawCircle(0, 0, .1f);
        base.Render(canvas);
    }
}