using SimulationFramework;
using SimulationFramework.Drawing;
using SimulationFramework.SkiaSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Dev;
internal class PrototypeBackground : Component
{
    private ITexture backgroundTexture;

    public float Scale { get; set; } = 8;
    public float Width { get; set; } = 800;
    public float Height { get; set; } = 800;
    
    public override void Initialize(Entity parent)
    {
        backgroundTexture = Assets.GetSpriteTexture("Assets/PrototypeTextures/Dark/texture_07.png");
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        var skcanvas = SkiaInterop.GetCanvas(canvas);
        var paint = SkiaInterop.GetPaint(canvas.State);

        var fillMatrix = Matrix3x2.CreateScale(Scale / MathF.Min(backgroundTexture.Width, backgroundTexture.Height));
        paint.Shader = SKShader.CreateBitmap(SkiaInterop.GetBitmap(backgroundTexture), SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, fillMatrix.AsSKMatrix());
        Rectangle rect = new(0, 0, Width, Height, Alignment.Center);
        skcanvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);
        paint.Shader.Dispose();

        //canvas.Antialias(true);
        //var fillMatrix = Matrix3x2.CreateScale(Scale / MathF.Min(backgroundTexture.Width, backgroundTexture.Height));
        //canvas.Fill(backgroundTexture, fillMatrix, TileMode.Repeat, TileMode.Repeat);
        //canvas.DrawRect(0, 0, Width, Height, Alignment.Center);

        base.Render(canvas);
    }
}