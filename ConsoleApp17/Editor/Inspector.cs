using ImGuiNET;
using SimulationFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Editor;
internal class Inspector : DebugWindow
{
    public static readonly Inspector Instance = new();

    public IInspectable? Inspectable { get; private set; }

    public override Key? KeyBind => Key.F3;
    public override string Title => "Inspector (F3)";

    public Inspector()
    {
    }

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


    public void LayoutObject(object? obj, string? name = null)
    {
        if (obj is null)
        {
            ImGui.Text("null");
            return;
        }

        
        Type type = obj.GetType();

        if (ImGui.TreeNode(name is null ? obj.ToString() : $"{name}: {obj}"))
        {
            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public);

            foreach (var member in members)
            {
                switch (member)
                {
                    case FieldInfo field:
                        LayoutField(field, obj);
                        break;
                    case PropertyInfo property:
                        LayoutProperty(property, obj);
                        break;
                    case MethodInfo method when method.GetCustomAttribute<ButtonAttribute>() is not null:
                        LayoutButton(obj, method);
                        break;
                    default:
                        break;
                }
            }

            ImGui.TreePop();
        }
    }

    private void LayoutField(FieldInfo fieldInfo, object obj)
    {
        var name = fieldInfo.Name;
        var isReadonly = fieldInfo.IsInitOnly;
        var value = fieldInfo.GetValue(obj);

        switch (value)
        {
            case float floatValue:
                floatValue = LayoutFloat(name, isReadonly, floatValue);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, floatValue);
                return;
            case bool boolValue:
                boolValue = LayoutBool(name, isReadonly, boolValue);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, boolValue);
                return;
            case int intValue:
                intValue = LayoutInt(name, isReadonly, intValue); 
                if (!isReadonly)
                    fieldInfo.SetValue(obj, intValue);
                return;
            case Vector2 vec2Value:
                vec2Value = LayoutVector2(name, isReadonly, vec2Value);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, vec2Value);
                return;
            case IEnumerable enumerable:
                LayoutEnumerable(name, enumerable);
                return;
            default:
                LayoutObject(value, name);
                break;
        }
    }

    private void LayoutProperty(PropertyInfo fieldInfo, object obj)
    {
        if (!fieldInfo.CanRead)
            return;

        var name = fieldInfo.Name;
        var isReadonly = !fieldInfo.CanWrite;
        var value = fieldInfo.GetValue(obj);

        switch (value)
        {
            case float floatValue:
                floatValue = LayoutFloat(name, isReadonly, floatValue);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, floatValue);
                return;
            case bool boolValue:
                boolValue = LayoutBool(name, isReadonly, boolValue);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, boolValue);
                return;
            case int intValue:
                intValue = LayoutInt(name, isReadonly, intValue);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, intValue);
                return;
            case XNAVector2 xnaVec2:
                xnaVec2 = LayoutVector2(name, isReadonly, xnaVec2.AsNumericsVector()).AsXNA();
                if (!isReadonly)
                    fieldInfo.SetValue(obj, xnaVec2);
                break;
            case Vector2 vec2Value:
                vec2Value = LayoutVector2(name, isReadonly, vec2Value);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, vec2Value);
                return;
            case IEnumerable enumerable:
                LayoutEnumerable(name, enumerable);
                return;
            case Enum @enum:
                @enum = LayoutEnum(name, @enum);
                if (!isReadonly)
                    fieldInfo.SetValue(obj, @enum);
                return;
            default:
                LayoutObject(value, name);
                break;
        }
    }

    private Enum LayoutEnum(string name, Enum enumValue)
    {
        var values = Enum.GetValues(enumValue.GetType());
        var names = Enum.GetNames(enumValue.GetType());

        int index = Array.IndexOf(values, enumValue);
        ImGui.Combo(name, ref index, names, names.Length);
        return (Enum)values.GetValue(index);
    }

    private void LayoutButton(object obj, MethodInfo method)
    {
        if (ImGui.Button(method.GetCustomAttribute<ButtonAttribute>()?.Name ?? method.Name))
        {
            method.Invoke(obj, Type.EmptyTypes);
        }
    }

    private void LayoutEnumerable(string name, IEnumerable enumerable)
    {
        if (ImGui.TreeNode(name))
        {
            int i = 0;
            foreach (var obj in enumerable)
            {
                ImGui.PushID(i++);
                LayoutObject(obj);
                ImGui.PopID();
            }
            ImGui.TreePop();
        }
    }

    private float LayoutFloat(string name, bool isReadonly, float value)
    {
        if (isReadonly)
        {
            ImGui.Text($"{name}: {value}");
        }
        else
        {
            ImGui.DragFloat(name, ref value);
        }

        return value;
    }

    private int LayoutInt(string name, bool isReadonly, int value)
    {
        if (isReadonly)
        {
            ImGui.Text($"{name}: {value}");
        }
        else
        {
            ImGui.DragInt(name, ref value);
        }

        return value;
    }

    private bool LayoutBool(string name, bool isReadonly, bool value)
    {
        if (isReadonly)
        {
            ImGui.Text($"{name}: {value}");
        }
        else
        {
            ImGui.Checkbox(name, ref value);
        }

        return value;
    }

    private Vector2 LayoutVector2(string name, bool isReadonly, Vector2 value)
    {
        if (isReadonly)
        {
            ImGui.Text($"{name}: {value}");
        }
        else
        {
            ImGui.DragFloat2(name, ref value);
        }

        return value;
    }
}
