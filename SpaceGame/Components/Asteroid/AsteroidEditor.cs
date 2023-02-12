using Genbox.VelcroPhysics.Dynamics;

namespace ConsoleApp17.Components.Asteroid;

class AsteroidEditor : Component
{
    public AsteroidChunkManager? selectedManager;
    public RadialBrush brush;
    public bool Active;
    public bool justSelectedExisting;

    public override void Initialize(Entity parent)
    {
        brush = new(10, .75f);
    }

    public override void Update()
    {
        void EditTerrain(Vector2 position, float scalar)
        {
            brush.EditSpeed = Math.Abs(brush.EditSpeed) * scalar;
            selectedManager!.Modify(brush, position);
        }

        if (Keyboard.IsKeyPressed(Key.E))
        {
            if (selectedManager is not null)
            {
                Deselect();
            }
            else
            {
                Active = !Active;
            }
        }

        if (!Active)
            return;

        if (justSelectedExisting && Mouse.IsButtonReleased(MouseButton.Right))
            justSelectedExisting = false;

        if (selectedManager is null)
        {
            if (Mouse.IsButtonDown(MouseButton.Left)) 
            {
                Select(Entity.Create("./Components/Asteroid/asteroid.arch", Scene.Active).GetComponent<AsteroidChunkManager>());
            }
            else if (Mouse.IsButtonPressed(MouseButton.Right))
            {
                var mousePosition = ParentTransform.WorldToLocal(Camera.Active.ScreenToWorld(Mouse.Position));
                var collider = Scene.Active.Physics.TestPoint(mousePosition);
                if (collider is AsteroidCollider)
                {
                    Select(collider.ParentEntity.GetSibling<AsteroidChunkManager>());
                    justSelectedExisting = true;
                }
            }
        }

        if (selectedManager is not null) 
        {
            var mousePosition = selectedManager.ParentTransform.WorldToLocal(Camera.Active.ScreenToWorld(Mouse.Position));

            if (Mouse.IsButtonDown(MouseButton.Left))
            {
                EditTerrain(mousePosition, 1);
            }
            else if (!justSelectedExisting && Mouse.IsButtonDown(MouseButton.Right))
            {
                EditTerrain(mousePosition, -1);
            }
        }
    }

    public override void Render(ICanvas canvas)
    {
        if (Active)
        {
            canvas.ResetState();
            float margin = canvas.State.FontSize + 2f;
            if (selectedManager is not null)
            {
                canvas.DrawText($"Volume Edit Mode Active (compid {selectedManager.ID})", new(0, 0));
                canvas.DrawText("E - Deselect", new(0, margin));
                canvas.DrawText("Left Click - Add To Volume", new(0, margin * 2));
                canvas.DrawText("Right Click - Subtract From Volume", new(0, margin * 3));
            }
            else
            {
                canvas.DrawText("Volume Edit Mode Active (No Volume Selected)", new(0, 0));
                canvas.DrawText("E - Deactivate Volume Edit Mode", new(0, margin));
                canvas.DrawText("Left Click - Create New Volume", new(0, margin * 2));
                canvas.DrawText("Right Click - Select Existing Volume", new(0, margin * 3));
            }
        }

        base.Render(canvas);
    }

    private void OnSelectedManagerDestroyed(Component component)
    {
        Deselect();
    }

    public void Select(AsteroidChunkManager manager)
    {
        selectedManager = manager ?? throw new Exception();
        selectedManager.Destroyed += OnSelectedManagerDestroyed;
    }

    public void Deselect()
    {
        selectedManager!.Destroyed -= OnSelectedManagerDestroyed;
        selectedManager = null;
    }

    public override void Layout()
    {
        ImGui.DragFloat("Edit Speed", ref brush.EditSpeed);

        base.Layout();
    }
    public class RadialBrush : AsteroidBrush
    {
        public float brushSize = 10;
        public float EditSpeed = 50f;

        public RadialBrush(float brushSize, float editSpeed)
        {
            this.brushSize = brushSize;
            EditSpeed = editSpeed;
        }

        public override void ApplyTo(ref float value, Vector2 valuePosition, Vector2 brushPosition)
        {
            var dist = Vector2.Distance(valuePosition, brushPosition);

            var radius = brushSize / 2f;

            if (dist < radius)
            {
                if (EditSpeed > 0)
                {
                    if (Scene.Active.Physics.TestPoint(valuePosition) is null)
                        value = MathF.Max(value, MathHelper.Normalize(EditSpeed / MathF.Sqrt(dist)));
                }
                else
                {
                    value = MathF.Min(value, 1f-MathHelper.Normalize(EditSpeed / MathF.Sqrt(dist))); 
                }
                value = MathHelper.Normalize(value);
            }
        }

        public override Rectangle GetBounds(Vector2 position)
        {
            int x = (int)MathF.Round(position.X),
                y = (int)MathF.Round(position.Y);

            return new Rectangle(x, y, brushSize, brushSize, Alignment.Center);
        }
    }
}