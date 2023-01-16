using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Editor;
internal class DebugDrawWindow : DebugWindow
{
    public static DebugDrawWindow Instance = new();

    public override Key? KeyBind => Key.F8;

    public bool DrawChunkBorders;
    public bool DrawAsteroidBorderPolygon;
    public bool DrawAsteroidEdges;
    public bool DrawAsteroidValues;
    public bool DrawReflexVertices;

    protected override void OnLayout()
    {
        if (ImGui.CollapsingHeader("Asteroids"))
        {
            ImGui.Checkbox(nameof(DrawChunkBorders), ref DrawChunkBorders);
            ImGui.Checkbox(nameof(DrawAsteroidBorderPolygon), ref DrawAsteroidBorderPolygon);
            ImGui.Checkbox(nameof(DrawAsteroidEdges), ref DrawAsteroidEdges);
            ImGui.Checkbox(nameof(DrawAsteroidValues), ref DrawAsteroidValues);
            ImGui.Checkbox(nameof(DrawReflexVertices), ref DrawReflexVertices);
        }
    }
}