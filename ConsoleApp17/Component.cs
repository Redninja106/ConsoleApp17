using ImGuiNET;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal abstract class Component : IInspectable
{
    private readonly Entity? parentEntity;
    private static ulong nextID = 1;

    public Entity ParentEntity 
    { 
        get
        {
            return parentEntity ?? throw new Exception("Component has no parent");
        }
        init
        {
            parentEntity = value;
        }
    }

    public ulong ID { get; }

    public Component()
    {
        this.ID = nextID++;
    }

    public abstract void Initialize(Entity parent);
    public abstract void Update();

    public virtual void Layout() 
    {
        ImGui.Text(this.ToString());
    }

    public virtual void Destroy()
    {

    }

    public virtual void Render(ICanvas canvas) 
    { 
    }

    public override string ToString()
    {
        return $"{GetType().Name} (id: {ID})";
    }

    
}