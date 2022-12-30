using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class CameraWindow : DebugWindow
{
    public static readonly CameraWindow Instance = new();

    public override Key? KeyBind => Key.F5;
    public override string Title => "Camera (F5)";

    protected override void OnLayout()
    {
    }
}
