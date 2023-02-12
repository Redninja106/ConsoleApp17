using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp17;

namespace Pong.Components;
internal class PaddleRenderer : Component
{
    PaddleController controller;

    public override void Initialize(Entity parent)
    {
        controller = parent.GetComponent<PaddleController>() ?? throw new Exception();
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        canvas.DrawRect(0, 0, .2f, controller.Height, Alignment.Center);

        base.Render(canvas);
    }
}
