namespace ConsoleApp17.Components.Asteroid.MarchingSquares;

class MarchingSquaresRenderer : Component
{
    public MarchingSquaresVolume Volume { get; set; }

    private List<Vector2[]> polygons = new();

    public MarchingSquaresRenderer()
    {
    }

    public override void Initialize(Entity parent)
    {
        Volume = parent.GetComponent<MarchingSquaresVolume>();
       
        for (int y = 0; y < Volume.Height; y++)
        {
            for (int x = 0; x < Volume.Width; x++)
            {
                //Volume[x, y] = Random.Shared.NextSingle();
            }
        }

        BuildPolygons();
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        BuildPolygons();

        foreach (var polygon in polygons)
        {
            if (polygon.Any())
                canvas.DrawPolygon(polygon);
        }

        canvas.Stroke(Color.Red);

        for (int y = 0; y < Volume.Height - 1; y++)
        {
            for (int x = 0; x < Volume.Width - 1; x++)
            {
                canvas.DrawCircle(x, y, Volume[x, y] / 10f);
            }
        }

    }

    public void BuildPolygons()
    {
        polygons.Clear();
        List<Vector2> points = new();

        for (int y = 0; y < Volume.Height - 1; y++)
        {
            for (int x = 0; x < Volume.Width - 1; x++)
            {
                ProcessCell(Volume[x, y], Volume[x + 1, y], Volume[x, y + 1], Volume[x + 1, y + 1], new(x, y), points);
                polygons.Add(points.ToArray());
                points.Clear();
            }
        }
    }

    private void ProcessCell(float bl, float br, float tl, float tr, Vector2 offset, List<Vector2> resultPoints)
    {
        const float threshold = 0.5f;

        float GetRatio(float a, float b)
        {
            float v1 = MathF.Max(a, b);
            float v2 = MathF.Min(a, b);

            return (threshold - v1) / (v2 - v1);
        }

        int val = 0;
        val |= (tl > threshold ? 1 : 0) << 3;
        val |= (tr > threshold ? 1 : 0) << 2;
        val |= (br > threshold ? 1 : 0) << 1;
        val |= (bl > threshold ? 1 : 0) << 0;

        float avg = (bl + br + tl + tr) / 4f;

        float topRatio = GetRatio(tl, tr),
            bottomRatio = GetRatio(bl, br),
            leftRatio = GetRatio(bl, tl),
            rightRatio = GetRatio(br, tr);

        // 8-----4
        // |     |
        // |     |
        // 1-----2

        switch (val)
        {
            // o-----o
            // |     |
            // |\    |
            // *-----o
            case 1:
                resultPoints.Add(offset + new Vector2(0, leftRatio));
                resultPoints.Add(offset + new Vector2(bottomRatio, 0));
                resultPoints.Add(offset + new Vector2(0, 0));
                break;
            // o-----o
            // |     |
            // |    /|
            // o-----*
            case 2:
                resultPoints.Add(offset + new Vector2(1, rightRatio));
                resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
                resultPoints.Add(offset + new Vector2(1, 0));
                break;
            // o-----o
            // |_____|
            // |     |
            // *-----*
            case 3:
                resultPoints.Add(offset + new Vector2(0, leftRatio));
                resultPoints.Add(offset + new Vector2(1, rightRatio));
                resultPoints.Add(offset + new Vector2(1, 0f));
                resultPoints.Add(offset + new Vector2(0, 0f));
                break;
            // o-----*
            // |    \|
            // |     |
            // o-----o
            case 4:
                resultPoints.Add(offset + new Vector2(1, 1));
                resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
                resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
                break;
            case 5:
                if (avg > threshold)
                {
                    resultPoints.Add(offset + new Vector2(0, leftRatio));
                    resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
                    resultPoints.Add(offset + new Vector2(1, 1));
                    resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
                    resultPoints.Add(offset + new Vector2(bottomRatio, 0));
                    resultPoints.Add(offset + new Vector2(0, 0));
                }
                else
                {
                    // o-----*
                    // |    \|
                    // |\    |
                    // *-----o
                    resultPoints.Add(offset + new Vector2(0, leftRatio));
                    resultPoints.Add(offset + new Vector2(bottomRatio, 0));
                    resultPoints.Add(offset + new Vector2(0, 0));

                    polygons.Add(resultPoints.ToArray());
                    resultPoints.Clear();

                    resultPoints.Add(offset + new Vector2(1, 1));
                    resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
                    resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
                }
                break;
            // o-----*
            // |  |  |
            // |  |  |
            // o-----*
            case 6:
                resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
                resultPoints.Add(offset + new Vector2(1f, 0));
                resultPoints.Add(offset + new Vector2(1, 1));
                resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
                break;
            // o-----*
            // |/    |
            // |     |
            // *-----*
            case 7:
                resultPoints.Add(offset + new Vector2(0, 0));
                resultPoints.Add(offset + new Vector2(1, 0));
                resultPoints.Add(offset + new Vector2(1, 1));
                resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
                resultPoints.Add(offset + new Vector2(0, leftRatio));
                break;
            // *-----o
            // |/    |
            // |     |
            // o-----o
            case 8:
                resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
                resultPoints.Add(offset + new Vector2(0, 1));
                resultPoints.Add(offset + new Vector2(topRatio, 1));
                break;
            // *---->o
            // ^  |  ^
            // |  |  |
            // *---->o
            case 9:
                resultPoints.Add(offset + new Vector2(0, 0));
                resultPoints.Add(offset + new Vector2(0, 1));
                resultPoints.Add(offset + new Vector2(topRatio, 1));
                resultPoints.Add(offset + new Vector2(bottomRatio, 0));
                break;
            case 10:
                if (avg > threshold)
                {
                    // *-----o
                    // |\  \ |
                    // | \  \|
                    // o-----*

                    resultPoints.Add(offset + new Vector2(0, 1));
                    resultPoints.Add(offset + new Vector2(topRatio, 1));
                    resultPoints.Add(offset + new Vector2(1, rightRatio));
                    resultPoints.Add(offset + new Vector2(1, 0));
                    resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
                    resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
                }
                else
                {
                    // *-----o
                    // |/    |
                    // |    /|
                    // o-----*
                    resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
                    resultPoints.Add(offset + new Vector2(0, 1));
                    resultPoints.Add(offset + new Vector2(topRatio, 1));

                    polygons.Add(resultPoints.ToArray());
                    resultPoints.Clear();

                    resultPoints.Add(offset + new Vector2(1, rightRatio));
                    resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
                    resultPoints.Add(offset + new Vector2(1, 0));
                }
                break;
            // *-----o
            // |    \|
            // |     |
            // *-----*
            case 11:
                resultPoints.Add(offset + new Vector2(0, 0));
                resultPoints.Add(offset + new Vector2(1, 0));
                resultPoints.Add(offset + new Vector2(1, rightRatio));
                resultPoints.Add(offset + new Vector2(topRatio, 1));
                resultPoints.Add(offset + new Vector2(0, 1));
                break;
            // *-----*
            // |_____|
            // |     |
            // o-----o
            case 12:
                resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
                resultPoints.Add(offset + new Vector2(0, 1));
                resultPoints.Add(offset + new Vector2(1, 1));
                resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
                break;
            // *-----*
            // |     |
            // |    /|
            // *-----o
            case 13:
                resultPoints.Add(offset + new Vector2(0, 0));
                resultPoints.Add(offset + new Vector2(0, 1));
                resultPoints.Add(offset + new Vector2(1, 1));
                resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
                resultPoints.Add(offset + new Vector2(bottomRatio, 0));
                break;
            // *-----*
            // |     |
            // |\    |
            // o-----*
            case 14:
                resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
                resultPoints.Add(offset + new Vector2(1, 0));
                resultPoints.Add(offset + new Vector2(1, 1));
                resultPoints.Add(offset + new Vector2(0, 1));
                resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
                break;
            // *-----*
            // |     |
            // |     |
            // *-----*
            case 15:
                resultPoints.Add(offset + new Vector2(1, 0));
                resultPoints.Add(offset + new Vector2(1, 1));
                resultPoints.Add(offset + new Vector2(0, 1));
                resultPoints.Add(offset + new Vector2(0, 0));
                break;
            default:
                break;
        }

        // resultPoints.Add(offset);
        // resultPoints.Add(offset + Vector2.UnitX);
        // resultPoints.Add(offset + Vector2.One);
        // resultPoints.Add(offset + Vector2.UnitY);
    }
}
