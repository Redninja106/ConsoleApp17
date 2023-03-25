using ConsoleApp17;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Ship;
internal class ShipMovement : Component
{
    public float MoveSpeed = 7.5f;

    private IShipController? controller;

    public override void Initialize(Entity parent)
    {
        controller = parent.GetComponent<IShipController>();
    }

    public override void Update()
    {
        if (controller is null)
            return;

        ref var position = ref this.ParentTransform.Position;

        var direction = controller.GetMovementDirection().Normalized();

        position += direction * MoveSpeed * Time.DeltaTime;

        position.X = Math.Clamp(position.X, -9.5f, 9.5f);
        position.Y = Math.Clamp(position.Y, -9.5f, 9.5f);
    }
}
