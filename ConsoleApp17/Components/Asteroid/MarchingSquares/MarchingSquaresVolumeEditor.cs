namespace ConsoleApp17.Components.Asteroid.MarchingSquares;

class MarchingSquaresVolumeEditor : Component
{
    public MarchingSquaresVolume volume;
    public float EditSpeed = 1.0f;

    public override void Initialize(Entity parent)
    {
        volume = parent.GetComponent<MarchingSquaresVolume>();
    }

    public override void Update()
    {
        var mousePosition = ParentTransform.WorldToLocal(Camera.Active.ScreenToWorld(Mouse.Position));

        if (Mouse.IsButtonDown(MouseButton.Left))
        {
            EditTerrain(mousePosition, 1);
        }
        if (Mouse.IsButtonDown(MouseButton.Right))
        {
            EditTerrain(mousePosition, -1);
        }
    }

    public override void Layout()
    {
        ImGui.DragFloat("Edit Speed", ref EditSpeed);

        base.Layout();
    }

    private void EditTerrain(Vector2 position, float scalar)
    {
        const float brushSize = 5;

        int x = (int)MathF.Round(position.X),
            y = (int)MathF.Round(position.Y);


        for (int bx = -5; bx < 5; bx++)
        {
            for (int by = -5; by < 5; by++)
            {
                int cx = x + bx, cy = y + by;

                if (cx < 0 || cx >= 100 || cy < 0 || cy >= 100)
                    continue;

                ref float value = ref volume[cx, cy];
                var dist = Vector2.Distance(position, new(cx, cy));

                if (dist < brushSize)
                {
                    value += Time.DeltaTime * EditSpeed * scalar * (brushSize - dist) * (brushSize - dist);
                    value = MathHelper.Normalize(value);
                }
            }
        }
    }
}