using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Player;
internal class PlayerCameraController : Component
{
    float zoomFactor;
    Camera camera;

    public override void Initialize(Entity parent)
    {
        camera = parent.GetComponent<Camera>();
    }

    public override void Update()
    {
        float zoomDelta = 0;
        if (Keyboard.IsKeyDown(Key.Plus))
            zoomDelta++;
        if (Keyboard.IsKeyDown(Key.Minus))
            zoomDelta--;

        zoomFactor += zoomDelta * Time.DeltaTime;
        zoomFactor += Mouse.ScrollWheelDelta;

        camera.VerticalSize = MathF.Pow(1.1f, -zoomFactor);
    }
}
