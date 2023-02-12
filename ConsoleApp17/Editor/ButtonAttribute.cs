using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Editor;

[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : Attribute
{
    public string? Name { get; }

    public ButtonAttribute(string? name = null)
    {
        this.Name = name;
    }
}
