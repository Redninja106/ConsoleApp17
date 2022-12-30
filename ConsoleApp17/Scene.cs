using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal class Scene : Entity
{
    public static Scene Active { get; private set; }
    
    public void Initialize()
    {
        base.Initialize(null!);
    }

    public void SetActive()
    {
        Active = this;
    }
}