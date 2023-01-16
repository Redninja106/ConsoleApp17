using ConsoleApp17.Editor;
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
    
    public bool IsDestroyed { get; private set; }

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

    public ref Transform ParentTransform => ref ParentEntity.Transform;

    public ulong ID { get; }

    public event Action<Component>? Destroyed;

    public Component()
    {
        this.ID = nextID++;
    }

    public abstract void Initialize(Entity parent);

    public abstract void Update();

    public virtual void Layout() 
    {
        Inspector.Instance.LayoutObject(this);
    }

    public virtual void Destroy()
    {
        Destroyed?.Invoke(this);
        ParentEntity.OnChildDestroyed(this);
        IsDestroyed = true;
    }

    public virtual void Render(ICanvas canvas) 
    {
    }

    public override string ToString()
    {
        return $"{GetType().Name} (id: {ID})";
    }

    public T? GetSibling<T>() where T : Component => ParentEntity.GetComponent<T>();
    public T? GetSibling<T>(Predicate<T> predicate) where T : Component => ParentEntity.GetComponent<T>(predicate);

    public T? GetComponentInParents<T>() where T : Component
    {
        Entity? current = parentEntity;
        T? result = null;

        while (current is not null)
        {
            result = current.GetComponent<T>();

            if (result is not null)
                break;
            
            current = current.parentEntity;
        }

        return result;
    }
}