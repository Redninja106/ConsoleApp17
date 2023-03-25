using ConsoleApp17;
using ConsoleApp17.Physics;
using SpaceInvaders.Components.Collision;
using SpaceInvaders.Components.Ship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Enemy;
internal class EnemyShipController : Component, IShipController
{
    public Vector2 GetMovementDirection()
    {
        return -Vector2.UnitX;
    }

    RectCollider collider;

    public override void Initialize(Entity parent)
    {
        collider = GetSibling<RectCollider>();
    }

    public override void Update()
    {
        if (collider.collisions.Any())
        {
            ParentEntity.Destroy();
            collider.collisions.First().Destroy();
        }
    }
}
