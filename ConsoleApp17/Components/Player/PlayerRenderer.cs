using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Player;
internal class PlayerRenderer : Component
{
    Sprite player;

    public override void Initialize(Entity parent)
    {
        player = new Sprite("Assets/SpaceShooterExtensions/Astronauts/spaceAstronauts_001.png", new(1,1));
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        canvas.DrawSprite(player);
        base.Render(canvas);
    }
}
