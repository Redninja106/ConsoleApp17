using ImGuiNET;
using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Editor;
internal class Inspector : DebugWindow
{
    public static readonly Inspector Instance = new();

    public IInspectable? Inspectable { get; private set; }

    public override Key? KeyBind => Key.F3;
    public override string Title => "Inspector (F3)";

    protected override void OnLayout()
    {
        if (Inspectable is not null)
        {
            Inspectable.Layout();
        }
        else
        {
            ImGui.Text("Nothing is selected.");
        }
    }

    public void Inspect(IInspectable element)
    {
        IsOpen = true;
        Inspectable = element;
    }
}
