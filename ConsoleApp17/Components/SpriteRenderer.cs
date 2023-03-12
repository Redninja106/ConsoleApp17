using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components;
public class SpriteRenderer : Component
{
    public Sprite? Sprite { get; set; }
    public Vector2 RenderScale { get; set; }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        if (Sprite is not null)
        {
            canvas.DrawSprite(Sprite);
        }

        base.Render(canvas);
    }
}
