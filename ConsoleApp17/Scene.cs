using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp17.Physics;

namespace ConsoleApp17;
internal class Scene : Entity
{
    public static Scene Active { get; private set; }

    public PhysicsManager Physics => GetComponent<PhysicsManager>() ?? throw new Exception();

    public Scene() : base()
    {

    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
    }

    private void RenderComponent(Component component)
    {
        if (component is Entity entity)
        {

        }
    }

    public void Initialize()
    {
        this.Initialize(null!);
    }

    public void SetActive()
    {
        Active = this;
    }

    public void UpdatePhysics()
    {
        Physics.UpdateWorld();
    }
}