using Genbox.VelcroPhysics.Shared;
using ImGuiNET;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
public class Entity : Component
{
    private readonly List<Component> components = new();
    private bool canModifyComponents = true;
    
    private Transform transform;
    private bool isInitialized = false;

    private readonly Queue<Component> addQueue = new();
    private readonly Queue<Component> removeQueue = new();

    public IEnumerable<Component> Components => components;
    public ref Transform Transform => ref transform;

    public string Name { get; init; }

    protected Entity(string? name = null)
    {
        this.Name = name ?? base.ToString();
        this.transform.Reset();
    }

    public override void Initialize(Entity parent)
    {
        // save existing pos/rot/scale as it may have been set by scene loader
        Transform oldTransform = this.transform;

        transform = new(parent)
        {
            Position = oldTransform.Position,
            Rotation = oldTransform.Rotation,
            Scale = oldTransform.Scale
        };

        ClearQueues();

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

    public bool HasComponent<T>() where T : Component
    {
        return components.OfType<T>().Any();
    }

    public Component AddComponent(Type type)
    {
        var component = (Component)(Activator.CreateInstance(type) ?? throw new ArgumentException(null, nameof(type)));
        var entityProperty = type.GetProperty(nameof(ParentEntity)) ?? throw new ArgumentException(null, nameof(type));
        entityProperty.SetValue(component, this);

        AddComponentCore(component);

        return component;
    }

    private protected virtual void AddComponentCore<T>(T component) where T : Component
    {
        Debug.Assert(component.ParentEntity == this);
        Debug.Assert(!components.Contains(component));

        if (canModifyComponents)
        {
            components.Add(component);

        }
        else
        {
            addQueue.Enqueue(component);
        }

        if (isInitialized)
        {
            component.Initialize(this);
        }
    }

    public void RemoveComponent<T>(T component) where T : Component
    {
        RemoveComponent(component as Component);
    }

    public void RemoveComponent(Component component)
    {
        Debug.Assert(components.Contains(component));

        if (canModifyComponents)
        {
            components.Remove(component);
        }
        else
        {
            removeQueue.Enqueue(component);
        }
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
        canModifyComponents = false;

        ImGui.Text(this.ToString());

        ImGui.Separator();

        if (ImGui.CollapsingHeader("Transform"))
        {
            transform.Layout();
        }

        foreach (var component in components)
        {
            if (component is not Entity && ImGui.CollapsingHeader(component.GetType().Name))
            {
                component.Layout();
            }
        }
        canModifyComponents = true;
        ClearQueues();
    }

    public override void Render(ICanvas canvas)
    {
        canModifyComponents = false;

        canvas.PushState();
        transform.ApplyTo(canvas);

        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i];

            canvas.PushState();
            component.Render(canvas);
            canvas.PopState();
        }

        canvas.PopState();

        canModifyComponents = true;
        ClearQueues();
    }

    public override void Update()
    {
        canModifyComponents = false;

        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i];
            component.Update();
        }

        canModifyComponents = true;
        ClearQueues();
    }

    public static Entity Create(string archetypePath, Entity parent, string? name = null)
    {
        return Create(archetypePath, Assembly.GetCallingAssembly(), parent, name);
    }

    public static Entity Create(string archetypePath, Assembly? mainAssembly, Entity parent, string? name = null)
    {
        mainAssembly ??= Assembly.GetCallingAssembly();

        var entity = Create(parent, name);

        SceneLoader.AttachArchetype(entity, archetypePath, mainAssembly);

        return entity;
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

    public T? FindComponent<T>() where T : Component
    {
        return FindComponent<T>(c => true);
    }

    public T? FindComponent<T>(Predicate<T> predicate) where T : Component
    {
        foreach (var component in components)
        {
            if (component is T foundComponent && predicate(foundComponent))
                return foundComponent;
        }

        foreach (var entity in components.OfType<Entity>())
        {
            var component = entity.FindComponent(predicate);

            if (component is not null)
                return component;
        }

        return null;
    }

    private record struct EntityProvider(string? Name) : IComponentProvider<Entity>
    {
        public Entity CreateComponent(Entity parent)
        {
            return new(Name) { ParentEntity = parent };
        }
    }

    internal void OnChildDestroyed(Component component)
    {
        RemoveComponent(component);
    }

    private void ClearQueues()
    {
        while (removeQueue.Any())
        {
            components.Remove(removeQueue.Dequeue());
        }

        while (addQueue.Any())
        {
            components.Add(addQueue.Dequeue());
        }
    }

    public override void Destroy()
    {
        canModifyComponents = false;

        foreach (var component in components)
        {
            component.Destroy();
        }
        
        canModifyComponents = true;
        ClearQueues();

        base.Destroy();
    }
}
