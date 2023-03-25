using ConsoleApp17;
using SpaceInvaders.Components.Projectile;
using SpaceInvaders.Components.Ship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Player;

internal class PlayerShipController : Component, IShipController
{
    public Vector2 GetMovementDirection()
    {
        Vector2 delta = Vector2.Zero;

        if (Keyboard.IsKeyDown(Key.A))
        {
            delta -= Vector2.UnitX;
        }

        if (Keyboard.IsKeyDown(Key.D))
        {
            delta += Vector2.UnitX;
        }

        return delta;
    }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        if (Keyboard.IsKeyPressed(Key.Space))
        {
            var projectileEntity = Entity.Create("./Components/Projectile/Projectile.arch", Scene.Active);

            projectileEntity.Transform.Position = this.ParentTransform.Position;
            projectileEntity.GetComponent<ProjectileController>().Speed = 20;
        }

        if (Keyboard.IsKeyPressed(Key.E))
        {
            var enemy = Entity.Create("./Components/Enemy/EnemyShip.arch", Scene.Active);
            enemy.Transform.Position = Camera.Active.ScreenToWorld(Mouse.Position);
        }
    }
}
