using ConsoleApp17;
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

        Console.WriteLine(delta);
        return delta;
    }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
    }
}
