using SimulationFramework;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.OLD.Player;
internal class UFOShipUnit : Component
{
    Sprite ship;

    public override void Initialize(Entity parent)
    {
        ship = new Sprite("Assets/SpaceShooterRedux/ufoBlue.png", new(1, 1));
        parent.AddComponent<Collider>();
    }

    public override void Layout()
    {
    }

    public override void Render(ICanvas canvas)
    {
        ParentEntity.Transform.Rotation += Time.DeltaTime * MathF.PI * .25f;

        canvas.DrawSprite(ship);
    }

    public override void Update()
    {

    }
}
