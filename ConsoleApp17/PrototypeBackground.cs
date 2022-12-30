using SimulationFramework;
using SimulationFramework.Drawing;
using SimulationFramework.SkiaSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class PrototypeBackground : Component
{
    private ITexture backgroundTexture;

    public float Scale { get; set; } = 2;

    public override void Initialize(Entity parent)
    {
        backgroundTexture = Assets.GetSpriteTexture("Assets/PrototypeTextures/Dark/texture_07.png");
    }

    public override void Update()
    {

    }

    public override void Render(ICanvas canvas)
    {
        canvas.DrawRect(0, 0, Scale * 2, Scale * 2, Alignment.Center);

        var skcanvas = SkiaInterop.GetCanvas(canvas);
        var bitmap = SkiaInterop.GetBitmap(backgroundTexture);

        SKPaint paint = new()
        {
            IsAntialias = false,
            FilterQuality = SKFilterQuality.High,
        };

        for (float y = -50; y < 50; y++)
        {
            for (float x = -50; x < 50; x++)
            {
                skcanvas.DrawBitmap(bitmap, new SKRect(x * Scale - float.Epsilon, y * Scale - float.Epsilon, (x + 1) * Scale + float.Epsilon, (y + 1) * Scale + float.Epsilon), paint);      
                // canvas.DrawTexture(backgroundTexture, new Rectangle(x, y, 1, 1, Alignment.Center));
            }
        }

        base.Render(canvas);
    }
}
