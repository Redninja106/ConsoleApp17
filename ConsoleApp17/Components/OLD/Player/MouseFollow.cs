using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.OLD.Player;
internal class MouseFollow : Component, IInspectable
{
    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        ParentEntity.Transform.Position = Camera.Active.ScreenToWorld(Mouse.Position);
    }

    public void Layout()
    {
        ImGui.Text(Mouse.Position.ToString());
    }
}
