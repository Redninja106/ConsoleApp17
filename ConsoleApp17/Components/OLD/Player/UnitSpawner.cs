using ConsoleApp17.Components.OLD.Units;
using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.OLD.Player;
internal class UnitSpawner : Component
{
    private HitTestCollider spawnCollider;

    public override void Initialize(Entity parent)
    {
        spawnCollider = parent.GetComponent<HitTestCollider>() ?? throw new Exception();
    }

    public override void Update()
    {
        if (Keyboard.IsKeyPressed(Key.R) && !Collider.Handler.GetCollision(spawnCollider, out _))
        {
            var newEntity = Entity.Create(Scene.Active);
            newEntity.Transform.Position = ParentEntity.Transform.Position;

            newEntity.AddComponent<ShipController>();
            newEntity.AddComponent<Collider>();
        }
    }
}