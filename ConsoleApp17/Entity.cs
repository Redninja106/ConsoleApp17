using ImGuiNET;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class Entity : Component
{
    private readonly List<Component> components = new();
    private Transform transform;
    private bool isInitialized = false;

    public IEnumerable<Component> Components => components;
    public ref Transform Transform => ref transform;

    public string Name { get; init; }

    protected Entity(string? name = null)
    {
        this.Name = name ?? base.ToString();
    }

    public override void Initialize(Entity parent)
    {
        Vector2 pos = transform.Position;
        float rotation = transform.Rotation;

        transform = new(parent);

        transform.Position = pos;
        transform.Rotation = rotation;

        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i];
            component.Initialize(this);
        }

        isInitialized = true;
    }

    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T() { ParentEntity = this };

        AddComponentCore(component);

        return component;
    }

    public T AddComponent<T>(IComponentProvider<T> provider) where T : Component
    {
        var component = provider.CreateComponent(this);

        AddComponentCore(component);

        return component;
    }

    public Component AddComponent(Type type)
    {
        var component = (Component)(Activator.CreateInstance(type) ?? throw new ArgumentException(null, nameof(type)));
        var entityProperty = type.GetProperty(nameof(Component.ParentEntity)) ?? throw new ArgumentException(null, nameof(type));
        entityProperty.SetValue(component, this);

        AddComponentCore(component);

        return component;
    }

    private protected virtual void AddComponentCore<T>(T component) where T : Component
    {
        Debug.Assert(component.ParentEntity == this);
        Debug.Assert(!components.Contains(component));

        components.Add(component);

        if (isInitialized)
        {
            component.Initialize(this);
        }
    }

    public void RemoveComponent<T>(T component) where T : Component
    {
        Debug.Assert(components.Contains(component));

        components.Remove(component);
    }

    public T? GetComponent<T>() where T : Component
    {
        return GetComponent<T>(_ => true);
    }

    public T? GetComponent<T>(Predicate<T> predicate) where T : Component
    {
        return components.OfType<T>().FirstOrDefault(c => predicate(c));
    }

    public override void Layout()
    {
        ImGui.Text(this.ToString());

        ImGui.Separator();

        if (ImGui.CollapsingHeader("Transform"))
        {
            transform.Layout();
            ImGui.TreePop();
        }
    }

    public override void Render(ICanvas canvas)
    {
        canvas.PushState();
        transform.ApplyTo(canvas);  

        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i];
            component.Render(canvas);
        }

        canvas.PopState();
    }

    public override void Update()
    {
        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i];
            component.Update();
        }
    }

    public static Entity Create(Entity parent, string? name = null)
    {
        return parent.AddComponent(GetProvider(name));
    }

    public static IComponentProvider<Entity> GetProvider(string? name)
    {
        return new EntityProvider(name);
    }

    public override string ToString()
    {
        return this.Name;
    }

    private record struct EntityProvider(string? Name) : IComponentProvider<Entity>
    {
        public Entity CreateComponent(Entity parent)
        {
            return new(Name) { ParentEntity = parent };
        }
    }
}
