using ConsoleApp17.Components.Asteroid.Algorithms;
using System.Runtime.InteropServices;

namespace ConsoleApp17.Components.Asteroid;

class AsteroidRenderer : Component
{
    private AsteroidChunk chunk;
    private MarchingSquaresVolume Volume;
    private AsteroidPolygon polygon;

    public AsteroidRenderer()
    {
    }

    public override void Initialize(Entity parent)
    {
        Volume = GetSibling<MarchingSquaresVolume>();
        chunk = GetSibling<AsteroidChunk>();
        polygon = GetSibling<AsteroidPolygon>();
    }

    public override void Update()
    {
    }

    static Color[] colors = typeof(Color).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Select(f => (Color)f.GetValue(null)).ToArray();

    public override void Render(ICanvas canvas)
    {
        canvas.DrawTriangles(polygon.TrianglesBuffer);

        canvas.Stroke(Color.Red);

        if (DebugDrawWindow.Instance.DrawAsteroidValues)
        {
            for (int y = 0; y < Volume.Height; y++)
            {
                for (int x = 0; x < Volume.Width; x++)
                {
                    canvas.DrawCircle(x, y, Volume[x, y] / 10f);
                }
            }
        }

        if (DebugDrawWindow.Instance.DrawAsteroidEdges)
        {
            var colors = AsteroidRenderer.colors.GetEnumerator();

            foreach (var verts in polygon.Edges)
            {
                colors.MoveNext();
                canvas.Stroke((Color)colors.Current);
                canvas.DrawVertices(verts);
            }
        }
    }

    void DrawArrow(ICanvas canvas, Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float r = MathF.Atan2(dir.Y, dir.X);
        canvas.DrawLine(from, to);
        canvas.DrawLine(to, to + (Vector2.One*.25f).Rotate(r));
        canvas.DrawLine(to, to + (Vector2.One*.25f).Rotate(r+MathF.PI));
    }

    public void BuildPolygons()
    {
        //polygons.Clear();
        //List<Vector2> points = new();

        //var manager = ParentEntity.ParentEntity.GetComponent<AsteroidChunkManager>();

        //AsteroidChunk? rightNeighbor = manager.GetChunk(chunk.x + 1, chunk.y);
        //AsteroidChunk? topRightNeighbor = manager.GetChunk(chunk.x + 1, chunk.y + 1);
        //AsteroidChunk? topNeighbor = manager.GetChunk(chunk.x, chunk.y + 1);

        //for (int y = 0; y < Volume.Height; y++)
        //{
        //    for (int x = 0; x < Volume.Width; x++)
        //    {
        //        float bl, br, tl, tr;

        //        bl = Volume[x, y];

        //        if (x + 1 == Volume.Width && y + 1 == Volume.Height)
        //        {
        //            tr = topRightNeighbor?.Volume[0, 0] ?? 0;
        //            br = rightNeighbor?.Volume[0, y] ?? 0;
        //            tl = topNeighbor?.Volume[x, 0] ?? 0;
        //        }
        //        else if (y + 1 == Volume.Height)
        //        {
        //            tr = topNeighbor?[x + 1, 0] ?? 0;
        //            tl = topNeighbor?.Volume[x, 0] ?? 0;
        //            br = Volume[x + 1, y];
        //        }
        //        else if (x + 1 == Volume.Width)
        //        {
        //            tr = rightNeighbor?.Volume[0, y + 1] ?? 0;
        //            br = rightNeighbor?.Volume[0, y] ?? 0;
        //            tl = Volume[x, y + 1];
        //        }
        //        else
        //        {
        //            tr = Volume[x + 1, y + 1];
        //            br = Volume[x + 1, y];
        //            tl = Volume[x, y + 1];
        //        }

        //        ProcessCell(bl, br, tl, tr, new(x, y), points);
        //        polygons.Add(points.ToArray());
        //        points.Clear();
        //    }
        //}
    }

    //private void ProcessCell(float bl, float br, float tl, float tr, Vector2 offset, List<Vector2> resultPoints)
    //{
    //    const float threshold = 0.5f;

    //    float GetRatio(float a, float b)
    //    {
    //        float v1 = MathF.Max(a, b);
    //        float v2 = MathF.Min(a, b);

    //        return (threshold - v1) / (v2 - v1);
    //    }

    //    int val = 0;
    //    val |= (tl > threshold ? 1 : 0) << 3;
    //    val |= (tr > threshold ? 1 : 0) << 2;
    //    val |= (br > threshold ? 1 : 0) << 1;
    //    val |= (bl > threshold ? 1 : 0) << 0;

    //    float avg = (bl + br + tl + tr) / 4f;

    //    float topRatio = GetRatio(tl, tr),
    //        bottomRatio = GetRatio(bl, br),
    //        leftRatio = GetRatio(bl, tl),
    //        rightRatio = GetRatio(br, tr);

    //    // 8-----4
    //    // |     |
    //    // |     |
    //    // 1-----2

    //    switch (val)
    //    {
    //        // o-----o
    //        // |     |
    //        // |\    |
    //        // *-----o
    //        case 1:
    //            resultPoints.Add(offset + new Vector2(0, leftRatio));
    //            resultPoints.Add(offset + new Vector2(bottomRatio, 0));
    //            resultPoints.Add(offset + new Vector2(0, 0));
    //            break;
    //        // o-----o
    //        // |     |
    //        // |    /|
    //        // o-----*
    //        case 2:
    //            resultPoints.Add(offset + new Vector2(1, rightRatio));
    //            resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
    //            resultPoints.Add(offset + new Vector2(1, 0));
    //            break;
    //        // o-----o
    //        // |_____|
    //        // |     |
    //        // *-----*
    //        case 3:
    //            resultPoints.Add(offset + new Vector2(0, leftRatio));
    //            resultPoints.Add(offset + new Vector2(1, rightRatio));
    //            resultPoints.Add(offset + new Vector2(1, 0f));
    //            resultPoints.Add(offset + new Vector2(0, 0f));
    //            break;
    //        // o-----*
    //        // |    \|
    //        // |     |
    //        // o-----o
    //        case 4:
    //            resultPoints.Add(offset + new Vector2(1, 1));
    //            resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
    //            resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
    //            break;
    //        case 5:
    //            if (avg > threshold)
    //            {
    //                resultPoints.Add(offset + new Vector2(0, leftRatio));
    //                resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
    //                resultPoints.Add(offset + new Vector2(1, 1));
    //                resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
    //                resultPoints.Add(offset + new Vector2(bottomRatio, 0));
    //                resultPoints.Add(offset + new Vector2(0, 0));
    //            }
    //            else
    //            {
    //                // o-----*
    //                // |    \|
    //                // |\    |
    //                // *-----o
    //                resultPoints.Add(offset + new Vector2(0, leftRatio));
    //                resultPoints.Add(offset + new Vector2(bottomRatio, 0));
    //                resultPoints.Add(offset + new Vector2(0, 0));

    //                polygons.Add(resultPoints.ToArray());
    //                resultPoints.Clear();

    //                resultPoints.Add(offset + new Vector2(1, 1));
    //                resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
    //                resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
    //            }
    //            break;
    //        // o-----*
    //        // |  |  |
    //        // |  |  |
    //        // o-----*
    //        case 6:
    //            resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
    //            resultPoints.Add(offset + new Vector2(1f, 0));
    //            resultPoints.Add(offset + new Vector2(1, 1));
    //            resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
    //            break;
    //        // o-----*
    //        // |/    |
    //        // |     |
    //        // *-----*
    //        case 7:
    //            resultPoints.Add(offset + new Vector2(0, 0));
    //            resultPoints.Add(offset + new Vector2(1, 0));
    //            resultPoints.Add(offset + new Vector2(1, 1));
    //            resultPoints.Add(offset + new Vector2(1f - topRatio, 1));
    //            resultPoints.Add(offset + new Vector2(0, leftRatio));
    //            break;
    //        // *-----o
    //        // |/    |
    //        // |     |
    //        // o-----o
    //        case 8:
    //            resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
    //            resultPoints.Add(offset + new Vector2(0, 1));
    //            resultPoints.Add(offset + new Vector2(topRatio, 1));
    //            break;
    //        // *---->o
    //        // ^  |  ^
    //        // |  |  |
    //        // *---->o
    //        case 9:
    //            resultPoints.Add(offset + new Vector2(0, 0));
    //            resultPoints.Add(offset + new Vector2(0, 1));
    //            resultPoints.Add(offset + new Vector2(topRatio, 1));
    //            resultPoints.Add(offset + new Vector2(bottomRatio, 0));
    //            break;
    //        case 10:
    //            if (avg > threshold)
    //            {
    //                // *-----o
    //                // |\  \ |
    //                // | \  \|
    //                // o-----*

    //                resultPoints.Add(offset + new Vector2(0, 1));
    //                resultPoints.Add(offset + new Vector2(topRatio, 1));
    //                resultPoints.Add(offset + new Vector2(1, rightRatio));
    //                resultPoints.Add(offset + new Vector2(1, 0));
    //                resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
    //                resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
    //            }
    //            else
    //            {
    //                // *-----o
    //                // |/    |
    //                // |    /|
    //                // o-----*
    //                resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
    //                resultPoints.Add(offset + new Vector2(0, 1));
    //                resultPoints.Add(offset + new Vector2(topRatio, 1));

    //                polygons.Add(resultPoints.ToArray());
    //                resultPoints.Clear();

    //                resultPoints.Add(offset + new Vector2(1, rightRatio));
    //                resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
    //                resultPoints.Add(offset + new Vector2(1, 0));
    //            }
    //            break;
    //        // *-----o
    //        // |    \|
    //        // |     |
    //        // *-----*
    //        case 11:
    //            resultPoints.Add(offset + new Vector2(0, 0));
    //            resultPoints.Add(offset + new Vector2(1, 0));
    //            resultPoints.Add(offset + new Vector2(1, rightRatio));
    //            resultPoints.Add(offset + new Vector2(topRatio, 1));
    //            resultPoints.Add(offset + new Vector2(0, 1));
    //            break;
    //        // *-----*
    //        // |_____|
    //        // |     |
    //        // o-----o
    //        case 12:
    //            resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
    //            resultPoints.Add(offset + new Vector2(0, 1));
    //            resultPoints.Add(offset + new Vector2(1, 1));
    //            resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
    //            break;
    //        // *-----*
    //        // |     |
    //        // |    /|
    //        // *-----o
    //        case 13:
    //            resultPoints.Add(offset + new Vector2(0, 0));
    //            resultPoints.Add(offset + new Vector2(0, 1));
    //            resultPoints.Add(offset + new Vector2(1, 1));
    //            resultPoints.Add(offset + new Vector2(1, 1f - rightRatio));
    //            resultPoints.Add(offset + new Vector2(bottomRatio, 0));
    //            break;
    //        // *-----*
    //        // |     |
    //        // |\    |
    //        // o-----*
    //        case 14:
    //            resultPoints.Add(offset + new Vector2(1f - bottomRatio, 0));
    //            resultPoints.Add(offset + new Vector2(1, 0));
    //            resultPoints.Add(offset + new Vector2(1, 1));
    //            resultPoints.Add(offset + new Vector2(0, 1));
    //            resultPoints.Add(offset + new Vector2(0, 1f - leftRatio));
    //            break;
    //        // *-----*
    //        // |     |
    //        // |     |
    //        // *-----*
    //        case 15:
    //            resultPoints.Add(offset + new Vector2(1, 0));
    //            resultPoints.Add(offset + new Vector2(1, 1));
    //            resultPoints.Add(offset + new Vector2(0, 1));
    //            resultPoints.Add(offset + new Vector2(0, 0));
    //            break;
    //        default:
    //            break;
    //    }

    //    // resultPoints.Add(offset);
    //    // resultPoints.Add(offset + Vector2.UnitX);
    //    // resultPoints.Add(offset + Vector2.One);
    //    // resultPoints.Add(offset + Vector2.UnitY);
    //}
}
