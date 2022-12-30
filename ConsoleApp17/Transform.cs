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
internal struct Transform : IInspectable
{
    private readonly Entity parentEntity;
    public Vector2 Position;
    public float Rotation;

    public Vector2 Forward => Vector2.UnitX.Rotate(Rotation);
    public Vector2 Backward => Vector2.UnitX.Rotate(Rotation + MathF.PI);
    public Vector2 Left => Vector2.UnitX.Rotate(Rotation + MathF.PI * .5f);
    public Vector2 Right => Vector2.UnitX.Rotate(Rotation + MathF.PI * 1.5f);

    public Transform(Entity entity)
    {
        this.parentEntity = entity;
    }

    public void Translate(Vector2 translation, TransformSpace space)
    {
        Position += space switch
        {
            TransformSpace.Local => translation.Rotate(this.Rotation),
            TransformSpace.World => translation,
            _ => throw new ArgumentException(null, nameof(space)),
        };
    }

    public Vector2 TransformPoint(Vector2 point, TransformSpace from, TransformSpace to)
    {
        return Vector2.Transform(point, CreateMatrix(from, to));
    }

    public Matrix3x2 CreateMatrix(TransformSpace from, TransformSpace to)
    {
        if (from == to)
            return Matrix3x2.Identity;

        return (from, to) switch
        {
            (TransformSpace.Local, TransformSpace.Parent) => CreateLocalToParentMatrix(),
            (TransformSpace.Parent, TransformSpace.World) => parentEntity.Transform.CreateLocalToWorldMatrix(),
            (TransformSpace.Local, TransformSpace.World) => CreateLocalToWorldMatrix(),
            (TransformSpace.Parent, TransformSpace.Local) => CreateParentToLocalMatrix(),
            (TransformSpace.World, TransformSpace.Parent) => parentEntity.Transform.CreateWorldToLocalMatrix(),
            (TransformSpace.World, TransformSpace.Local) => CreateWorldToLocalMatrix()
        };
    }

    private Matrix3x2 CreateParentToLocalMatrix()
    {
        return Matrix3x2.CreateTranslation(-Position) * Matrix3x2.CreateRotation(-Rotation);
    }

    private Matrix3x2 CreateLocalToParentMatrix()
    {
        return Matrix3x2.CreateRotation(Rotation) * Matrix3x2.CreateTranslation(Position);
    }

    private Matrix3x2 CreateLocalToWorldMatrix()
    {
        if (parentEntity is null)
        {
            return Matrix3x2.Identity;
        }
        else
        {
            return CreateLocalToParentMatrix() * parentEntity.Transform.CreateLocalToWorldMatrix();
        }
    }

    private Matrix3x2 CreateWorldToLocalMatrix()
    {
        if (parentEntity is null)
        {
            return Matrix3x2.Identity;
        }
        else
        {
            return parentEntity.Transform.CreateWorldToLocalMatrix() * CreateParentToLocalMatrix();
        }
    }

    public Vector2 LocalToWorld(Vector2 point)
    {
        return Vector2.Transform(point, CreateLocalToWorldMatrix());
    }

    public Vector2 WorldToLocal(Vector2 point)
    {
        return Vector2.Transform(point, CreateWorldToLocalMatrix());
    }

    public void ApplyTo(ICanvas canvas)
    {
        canvas.Transform(this.CreateLocalToWorldMatrix());
    }

    public void Layout()
    {
        ImGui.DragFloat2("Position", ref this.Position);
        ImGui.DragFloat("Rotation", ref this.Rotation);
    }
}