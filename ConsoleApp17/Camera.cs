using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class Camera : Component
{
    public int DisplayWidth { get; private set; }
    public int DisplayHeight { get; private set; }

    private float verticalSize = 1.0f;

    public static Camera Main { get; private set; }
    public static Camera Active { get; set; }

    public float AspectRatio => DisplayWidth / (float)DisplayHeight;

    public float VerticalSize
    {
        get => verticalSize;
        set => verticalSize = value;
    }

    public float HorizontalSize
    {
        get => verticalSize * AspectRatio;
        set => verticalSize = value / AspectRatio;
    }

    public Camera()
    {
        Main ??= this;
    }

    public override void Initialize(Entity parent)
    {
    }

    public override void Layout()
    {
    }

    public override void Render(ICanvas canvas)
    {
    }

    public override void Update()
    {
    }

    public void SetDisplaySize(int width, int height)
    {
        DisplayWidth = width;
        DisplayHeight = height;
    }

    public void ApplyTo(ICanvas canvas)
    {
        // world to screen space
        canvas.Transform(CreateLocalToScreenMatrix());

        // world to local space
        canvas.Transform(this.ParentEntity.Transform.CreateMatrix(TransformSpace.World, TransformSpace.Local));
    }

    public Vector2 ScreenToWorld(Vector2 point)
    {
        return this.ParentEntity.Transform.LocalToWorld(ScreenToLocal(point));
    }
    
    public Vector2 WorldToScreen(Vector2 point)
    {
        return LocalToScreen(this.ParentEntity.Transform.WorldToLocal(point));
    }

    public Vector2 LocalToScreen(Vector2 point)
    {
        return Vector2.Transform(point, CreateLocalToScreenMatrix());
    }

    public Vector2 ScreenToLocal(Vector2 point)
    {
        return Vector2.Transform(point, CreateScreenToLocalMatrix());
    }

    private Matrix3x2 CreateLocalToScreenMatrix()
    {
        return 
            Matrix3x2.CreateScale(1f, -1f) *
            Matrix3x2.CreateScale(DisplayHeight / verticalSize) *
            Matrix3x2.CreateTranslation(DisplayWidth * .5f, DisplayHeight * .5f);
    }

    private Matrix3x2 CreateScreenToLocalMatrix()
    {
        return 
            Matrix3x2.CreateTranslation(-DisplayWidth * .5f, -DisplayHeight * .5f) *
            Matrix3x2.CreateScale(verticalSize / DisplayHeight) *
            Matrix3x2.CreateScale(1f, -1f);
    }
}
