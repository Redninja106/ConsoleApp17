using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal static class CanvasExtensions
{
    public static void DrawSprite(this ICanvas canvas, Sprite sprite)
    {
        sprite.Render(canvas);
    }
}
