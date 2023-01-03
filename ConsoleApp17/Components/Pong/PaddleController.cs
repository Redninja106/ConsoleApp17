using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Pong;
internal class PaddleController : Component
{
    private static (Key up, Key down)[] playerKeys = new[]
    {
        (Key.UpArrow, Key.DownArrow),
        (Key.W, Key.S),
    };

    public float Height = 1.5f;
    public float Speed = 10.0f;

    public int PlayerIndex { get; set; }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        float delta = 0;

        var keys = playerKeys[PlayerIndex];
        
        if (Keyboard.IsKeyDown(keys.up))
            delta++;
        if (Keyboard.IsKeyDown(keys.down))
            delta--;

        ParentTransform.Position.Y += delta * Speed * Time.DeltaTime;

        ParentTransform.Position.Y = Math.Clamp(ParentTransform.Position.Y, (Height - Camera.Active.VerticalSize) * .5f, (Camera.Active.VerticalSize - Height) * .5f);
    }
}
