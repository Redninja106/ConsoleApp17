using ConsoleApp17;
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

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
    }
}
