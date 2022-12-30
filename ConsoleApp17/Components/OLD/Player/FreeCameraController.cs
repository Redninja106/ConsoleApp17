using ImGuiNET;
using SimulationFramework;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.OLD.Player;
internal class FreeCameraController : Component
{
    float zoomFactor = 0;

    public override void Initialize(Entity parent)
    {
    }

    public override void Layout()
    {
        ImGui.DragFloat(zoomFactor.ToString(), ref zoomFactor);
        base.Layout();
    }

    public override void Update()
    {
        var camera = Camera.Main;

        Vector2 delta = Vector2.Zero;

        if (Keyboard.IsKeyDown(Key.W))
            delta += Vector2.UnitY;
        if (Keyboard.IsKeyDown(Key.A))
            delta -= Vector2.UnitX;
        if (Keyboard.IsKeyDown(Key.S))
            delta -= Vector2.UnitY;
        if (Keyboard.IsKeyDown(Key.D))
            delta += Vector2.UnitX;

        if (Keyboard.IsKeyDown(Key.LeftShift))
            delta *= 5;
        if (Keyboard.IsKeyDown(Key.LeftAlt))
            delta /= 5;

        ParentEntity.Transform.Position += delta * Time.DeltaTime * 25;

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