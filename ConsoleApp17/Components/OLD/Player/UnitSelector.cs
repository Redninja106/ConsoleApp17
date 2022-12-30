using ConsoleApp17.Components.OLD.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.OLD.Player;
internal class UnitSelector : Component, IInspectable
{
    private ShipController? selectedUnit;

    public override void Initialize(Entity parent)
    {

    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
    }

    public override void Update()
    {
        Vector2 mousePos = Camera.Active.ScreenToWorld(Mouse.Position);

        if (Mouse.IsButtonPressed(MouseButton.Left))
        {
            if (Collider.Handler.GetCollisionAtPoint(mousePos, out var collider) && collider is not null)
            {
                var ship = collider.ParentEntity.GetComponent<ShipController>();

                if (ship == selectedUnit)
                {
                    if (selectedUnit is not null)
                    {
                        selectedUnit.isSelected = false;
                    }

                    selectedUnit = null;
                }
                else
                {
                    if (selectedUnit is not null)
                    {
                        selectedUnit.isSelected = false;
                    }

                    selectedUnit = ship;

                    if (selectedUnit is not null)
                    {
                        selectedUnit.isSelected = true;
                    }
                }
            }
            else if (selectedUnit is not null)
            {
                selectedUnit.SetTarget(mousePos, Keyboard.IsKeyDown(Key.LeftControl));
            }
        }

        if (Mouse.IsButtonPressed(MouseButton.Right))
        {
            if (selectedUnit is not null)
            {
                selectedUnit.isSelected = false;
            }

            selectedUnit = null;
        }
    }
}