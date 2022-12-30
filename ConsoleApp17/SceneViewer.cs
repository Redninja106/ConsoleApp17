using ImGuiNET;
using Silk.NET.SDL;
using SimulationFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class SceneViewer : DebugWindow
{
    public static readonly SceneViewer Instance = new();

    public override Key? KeyBind => Key.F2;
    public override string Title => "Scene Viewer (F2)";
    public override ImGuiWindowFlags WindowFlags => ImGuiWindowFlags.None;

    protected override void OnLayout()
    {
        LayoutEntity(Scene.Active);
    }

    private void LayoutEntity(Entity entity)
    {
        if (TreeEntry(entity.ToString(), entity)) 
        {
            foreach (var component in entity.Components)
            {
                LayoutComponent(component);
            }

            ImGui.TreePop();
        }
    }

    private void LayoutComponent(Component component)
    {
        if (component is Entity entity)
        {
            LayoutEntity(entity);
        }
        else
        {
            TextEntry(component.ToString(), component);
        }
    }

    private bool TreeEntry(string name, Entity entity)
    {
        bool isOpen = ImGui.TreeNode(name);
        SelectButton(entity);
        return isOpen;
    }

    private void TextEntry(string name, Component component)
    {
        ImGui.Text(name);
        SelectButton(component);
    }

    private void SelectButton(Component component)
    {
        ImGui.SameLine();
        ImGui.PushID((int)component.ID);
        if (ImGui.SmallButton("select"))
        {
            Inspector.Instance.Inspect(component);
        }
        ImGui.PopID();
    }
}
