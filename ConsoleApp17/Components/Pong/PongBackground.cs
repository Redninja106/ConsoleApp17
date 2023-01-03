using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Pong;
internal class PongBackground : Component
{
    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(Color.DarkGray);
        //canvas.DrawRect(0, 0, 160f / 9f, 10f, Alignment.Center);

        base.Render(canvas);
    }
}
