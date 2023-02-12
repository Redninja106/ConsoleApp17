using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;

public interface IComponentProvider<TComponent> where TComponent : Component
{
    TComponent CreateComponent(Entity parent);
}