using ConsoleApp17.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Pong;
internal class Goal : Component
{
    public int PlayerIndex;

    PhysicsBody body;

    public event Action? OnScore;

    public override void Initialize(Entity parent)
    {
        body = parent.GetComponent<PhysicsBody>() ?? throw new Exception();
        body.OnCollision += Body_OnCollision;
    }

    private void Body_OnCollision(Collider arg1, Collider arg2, Contact arg3)
    {
        OnScore();
    }

    public override void Update()
    {
    }
}
