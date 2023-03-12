using ConsoleApp17;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvaders.Components.Ship;
internal class ShipMovement : Component
{
    public float MoveSpeed = 5.0f;

    private IShipController? controller;

    public override void Initialize(Entity parent)
    {
        controller = parent.GetComponent<IShipController>();
    }

    public override void Update()
    {
        if (controller is null)
            return;

        this.ParentTransform.Position += controller.GetMovementDirection() * MoveSpeed * Time.DeltaTime;
    }
}
