using ConsoleApp17.Components.Asteroid.Algorithms;
using ConsoleApp17.Physics;
using Genbox.VelcroPhysics;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.PolygonManipulation;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using System.Diagnostics;
using System.Runtime;

namespace ConsoleApp17.Components.Asteroid;

class AsteroidCollider : Collider
{
    private MarchingSquaresVolume volume;

    // private List<Vertices> polygons = new();

    private AsteroidPolygon polygon;

    private readonly List<PolygonShape> shapes = new();
    private readonly List<Fixture> fixtures = new();

    private PhysicsBody body;
    private List<(Vector2 first, Vector2 second)> edges;
    private bool DrawAsteroidBorderPolygon;

    public override void Initialize(Entity parent)
    {
        Settings.MaxPolygonVertices = 100;
        volume = parent.GetComponent<MarchingSquaresVolume>() ?? throw new Exception();
        body = parent.ParentEntity.GetComponent<PhysicsBody>() ?? throw new Exception();
        polygon = GetSibling<AsteroidPolygon>();
        polygon.Invalidated += Polygon_Invalidated;
        // polygons = GetVertices();
        base.Initialize(parent);
    }

    private void Polygon_Invalidated()
    {
        Invalidate();
    }

    public override void Update()
    {
    }

    public override void Layout()
    {
        if (ImGui.Button("Generate"))
        {
            Invalidate();
        }

        ImGui.Checkbox("DrawAsteroidBorderPolygon", ref DrawAsteroidBorderPolygon);
        base.Layout();
    }

    public override IEnumerable<Shape> CreateShapes(float density)
    {
        var chunk = GetSibling<AsteroidChunk>();

        return polygon.Triangles.Where(p => !IsDegenerateTriangle(p)).Select(p => new PolygonShape(new(p.Select(p => p + new XNAVector2(chunk.x * AsteroidChunk.CHUNK_SIZE, chunk.y * AsteroidChunk.CHUNK_SIZE))), 1.0f));
    }

    bool IsDegenerateTriangle(Vertices v)
    {
        if (v.Count != 3)
            return true;

        for (int i = 0; i < v.Count; i++)
        {
            for (int j = 0; j < v.Count; j++)
            {
                if (i == j)
                    continue;

                if (Vector2.DistanceSquared(v[i].AsNumericsVector(), v[j].AsNumericsVector()) < (.5f * Settings.LinearSlop) * (.5f * Settings.LinearSlop))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void DrawX(ICanvas canvas, Vector2 point, float size)
    {
        canvas.DrawLine(point.X, point.Y + size, point.X, point.Y - size);
        canvas.DrawLine(point.X + size, point.Y, point.X - size, point.Y);
    }

    public override void Render(ICanvas canvas)
    {
        if (DrawAsteroidBorderPolygon)
        {
            canvas.StrokeWidth(0f);
            canvas.Stroke(Color.Blue);
            if (polygon.Triangles is not null)
            {
                foreach (var poly in polygon.Triangles)
                {
                    if (poly.Count >= 3)
                        canvas.DrawPolygon(poly.Select(x => x.AsNumericsVector()).ToArray());
                }
            }
        }

        if (polygon.Triangles is not null && DebugDrawWindow.Instance.DrawReflexVertices)
        {
            foreach (var poly in polygon.Triangles)
            {
                for (int i = 0; i < poly.Count; i++)
                {
                    bool reflex = IsReflexVertex(poly, i);
                    var point = poly[i];
                    canvas.StrokeWidth(0);
                    canvas.Stroke(Color.Purple);
                    if (reflex)
                        DrawX(canvas, point.AsNumericsVector(), 1f);
                }
            }
        }

        foreach (var shape in shapes)
        {
            canvas.DrawPolygon(shape.Vertices.Select(x => x.AsNumericsVector()).ToArray());
        }

        base.Render(canvas);
    }

    public static bool IsReflexVertex(Vertices polygon, int vertexIndex)
    {
        int prevVertexIndex = WrapIndex(vertexIndex - 1, polygon.Count);
        int nextVertexIndex = WrapIndex(vertexIndex + 1, polygon.Count);

        Microsoft.Xna.Framework.Vector2 from = polygon[prevVertexIndex] - polygon[vertexIndex];
        Microsoft.Xna.Framework.Vector2 to = polygon[nextVertexIndex] - polygon[vertexIndex];
        float t = (to.X * from.Y) - (to.Y * from.X);
        return t > 0.01f;
    }

    private static int WrapIndex(int index, int length)
    {
        return (index < 0 ? (index + length) : index) % length;
    }

    //public List<Vertices> GetVertices()
    //{
    //    List<(Vector2 first, Vector2 second)> segments = new();

    //    var chunk = ParentEntity.GetComponent<AsteroidChunk>();
    //    var manager = ParentEntity.ParentEntity.GetComponent<AsteroidChunkManager>();
    //    var volume = ParentEntity.GetComponent<MarchingSquaresVolume>();

    //    AsteroidChunk? rightNeighbor = manager.GetChunk(chunk.x + 1, chunk.y);
    //    AsteroidChunk? topRightNeighbor = manager.GetChunk(chunk.x + 1, chunk.y + 1);
    //    AsteroidChunk? topNeighbor = manager.GetChunk(chunk.x, chunk.y + 1);

    //    for (int y = 0; y < this.volume.Height; y++)
    //    { 
    //        for (int x = 0; x < this.volume.Width; x++)
    //        {
    //            float bl, br, tl, tr;

    //            bl = volume[x, y];

    //            if (x + 1 == volume.Width && y + 1 == volume.Height)
    //            {
    //                tr = topRightNeighbor?.Volume[0, 0] ?? 0;
    //                br = rightNeighbor?.Volume[0, y] ?? 0;
    //                tl = topNeighbor?.Volume[x, 0] ?? 0;
    //            }
    //            else if (y + 1 == volume.Height)
    //            {
    //                tr = topNeighbor?.Volume[x + 1, 0] ?? 0;
    //                tl = topNeighbor?.Volume[x, 0] ?? 0;
    //                br = volume[x + 1, y];
    //            }
    //            else if (x + 1 == volume.Width)
    //            {
    //                tr = rightNeighbor?.Volume[0, y + 1] ?? 0;
    //                br = rightNeighbor?.Volume[0, y] ?? 0;
    //                tl = volume[x, y + 1];
    //            }
    //            else
    //            {
    //                tr = volume[x + 1, y + 1];
    //                br = volume[x + 1, y];
    //                tl = volume[x, y + 1];
    //            }

    //            int val = ProcessCell(bl, br, tl, tr, x, y, segments);
    //        }
    //    }

    //    edges = new(segments);

    //    if (!segments.Any())
    //        return new();

    //    List<List<Vector2>> polygon = new();

    //    polygon.Add(new()
    //    {
    //        segments[0].first,
    //        segments[0].second
    //    });

    //    segments.RemoveAt(0);

    //    int polyIndex = 0;
    //    int index = 1;

    //    while (segments.Any())
    //    {
    //        Vector2 pt = polygon[polyIndex][index];

    //        if (segments.Any(s => s.first == pt || s.second == pt))
    //        {
    //            var segment = segments.First(s => s.first == pt || s.second == pt);
    //            segments.Remove(segment);

    //            if (segment.first == pt)
    //            {
    //                polygon[polyIndex].Add(segment.second);
    //            }
    //            else
    //            {
    //                polygon[polyIndex].Add(segment.first);
    //            }

    //            index++;
    //        }
    //        else
    //        {
    //            polyIndex++;
    //            polygon.Add(new()
    //            {
    //                segments[0].first,
    //                segments[0].second
    //            });
    //            segments.RemoveAt(0);
    //            index = 1;
    //        }
    //    }

    //    return polygon.SelectMany(l => Triangulate.ConvexPartition(new(l.Select(x => x.AsXNA())), TriangulationAlgorithm.Delauny)).ToList();
    //}

    const float threshold = 0.5f;

    float GetRatio(float a, float b)
    {
        float v1 = MathF.Max(a, b);
        float v2 = MathF.Min(a, b);

        return (threshold - v1) / (v2 - v1);
    }

    private int ProcessCell(float bl, float br, float tl, float tr, int x, int y, List<(Vector2, Vector2)> segments)
    {
        var offset = new Vector2(x, y);

        void AddEdge(float x1, float y1, float x2, float y2)
        {
            segments.Add((offset + new Vector2(x1, y1), offset + new Vector2(x2, y2)));
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


        bool isBottomEdge = y == 0;
        bool isTopEdge = y == volume.Height - 1;

        bool isLeftEdge = x == 0;
        bool isRightEdge = x == volume.Width - 1;

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
                segments.Add(
                    (offset + new Vector2(0, leftRatio),
                    offset + new Vector2(bottomRatio, 0))
                    );


                if (isBottomEdge)
                    AddEdge(bottomRatio, 0, 0, 0);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, leftRatio);

                break;
            // o-----o
            // |     |
            // |    /|
            // o-----*
            case 2:
                segments.Add(
                    (offset + new Vector2(1f - bottomRatio, 0),
                    offset + new Vector2(1, rightRatio))
                    );

                if (isBottomEdge)
                    AddEdge(1, 0, 1f - bottomRatio, 0);
                if (isRightEdge)
                    AddEdge(1, rightRatio, 1, 0);

                break;
            // o-----o
            // |_____|
            // |     |
            // *-----*
            case 3:
                segments.Add(
                    (offset + new Vector2(0, leftRatio),
                    offset + new Vector2(1, rightRatio))
                    );

                if (isBottomEdge)
                    AddEdge(1, 0, 0, 0);
                if (isRightEdge)
                    AddEdge(1, rightRatio, 1, 0);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, leftRatio);

                break;
            // o-----*
            // |    \|
            // |     |
            // o-----o
            case 4:
                segments.Add(
                    (offset + new Vector2(1, 1f - rightRatio),
                    offset + new Vector2(1f - topRatio, 1))
                    );

                if (isRightEdge)
                    AddEdge(1, 1, 1, 1f - rightRatio);
                if (isTopEdge)
                    AddEdge(1f - topRatio, 1, 1, 1);

                break;
            case 5:
                if (avg > threshold)
                {
                    // o-----*
                    // |/    |
                    // |    /|
                    // *-----o
                    segments.Add(
                        (offset + new Vector2(0, leftRatio),
                        offset + new Vector2(1f - topRatio, 1))
                        );

                    segments.Add(
                        (offset + new Vector2(1, 1f - rightRatio),
                        offset + new Vector2(bottomRatio, 0))
                        );
                }
                else
                {
                    // o-----*
                    // |    \|
                    // |\    |
                    // *-----o

                    segments.Add(
                        (offset + new Vector2(bottomRatio, 0),
                        offset + new Vector2(0, leftRatio))
                        );

                    segments.Add(
                        (offset + new Vector2(1, 1f - rightRatio),
                        offset + new Vector2(1f - topRatio, 1))
                        );
                }

                if (isBottomEdge)
                    AddEdge(bottomRatio, 0, 0, 0);
                if (isRightEdge)
                    AddEdge(1, 1, 1, 1f - rightRatio);
                if (isTopEdge)
                    AddEdge(1f - topRatio, 1, 1, 1);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, leftRatio);
                break;
            // o-----*
            // |  |  |
            // |  |  |
            // o-----*
            case 6:
                segments.Add(
                    (offset + new Vector2(1f - bottomRatio, 0),
                    offset + new Vector2(1f - topRatio, 1))
                    );

                if (isBottomEdge)
                    AddEdge(1, 0, 1f - bottomRatio, 0);
                if (isRightEdge)
                    AddEdge(1, 1, 1, 0);
                if (isTopEdge)
                    AddEdge(1f - topRatio, 1, 1, 1);

                break;
            // o-----*
            // |/    |
            // |     |
            // *-----*
            case 7:
                segments.Add(
                    (offset + new Vector2(0, leftRatio),
                    offset + new Vector2(1f - topRatio, 1))
                    );

                if (isBottomEdge)
                    AddEdge(1, 0, 0, 0);
                if (isRightEdge)
                    AddEdge(1, 1, 1, 0);
                if (isTopEdge)
                    AddEdge(1f - topRatio, 1, 1, 1);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, leftRatio);

                break;
            // *-----o
            // |/    |
            // |     |
            // o-----o
            case 8:
                segments.Add(
                    (offset + new Vector2(topRatio, 1),
                    offset + new Vector2(0, 1f - leftRatio))
                    );

                if (isTopEdge)
                    AddEdge(0, 1, topRatio, 1);
                if (isLeftEdge)
                    AddEdge(0, 1f - leftRatio, 0, 1);

                break;
            // *-----o
            // |  |  |
            // |  |  |
            // *-----o
            case 9:
                segments.Add(
                    (offset + new Vector2(topRatio, 1),
                    offset + new Vector2(bottomRatio, 0))
                    );

                if (isBottomEdge)
                    AddEdge(bottomRatio, 0, 0, 0);
                if (isTopEdge)
                    AddEdge(0, 1, topRatio, 1);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, 1);

                break;
            case 10:
                if (avg > threshold)
                {
                    // *-----o
                    // |\  \ |
                    // | \  \|
                    // o-----*

                    segments.Add(
                        (offset + new Vector2(topRatio, 1),
                        offset + new Vector2(1, rightRatio))
                        );

                    segments.Add(
                        (offset + new Vector2(1f - bottomRatio, 0),
                        offset + new Vector2(0, 1f - leftRatio))
                        );
                }
                else
                {
                    // *-----o
                    // |/    |
                    // |    /|
                    // o-----*
                    segments.Add(
                        (offset + new Vector2(topRatio, 1),
                        offset + new Vector2(0, 1f - leftRatio))
                        );

                    segments.Add(
                        (offset + new Vector2(1f - bottomRatio, 0),
                        offset + new Vector2(1, rightRatio))
                        );
                }

                if (isBottomEdge)
                    AddEdge(1, 0, 1f - bottomRatio, 0);
                if (isRightEdge)
                    AddEdge(1, rightRatio, 1, 0);
                if (isTopEdge)
                    AddEdge(0, 1, topRatio, 1);
                if (isLeftEdge)
                    AddEdge(0, 1f - leftRatio, 0, 1);

                break;
            // *-----o
            // |    \|
            // |     |
            // *-----*
            case 11:
                segments.Add(
                    (offset + new Vector2(topRatio, 1),
                    offset + new Vector2(1, rightRatio))
                    );

                if (isBottomEdge)
                    AddEdge(1, 0, 0, 0);
                if (isRightEdge)
                    AddEdge(1, rightRatio, 1, 0);
                if (isTopEdge)
                    AddEdge(0, 1, topRatio, 1);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, 1);

                break;
            // *-----*
            // |_____|
            // |     |
            // o-----o
            case 12:
                segments.Add(
                    (offset + new Vector2(1, 1f - rightRatio),
                    offset + new Vector2(0, 1f - leftRatio))
                    );

                if (isRightEdge)
                    AddEdge(1, 1, 1, 1f - rightRatio);
                if (isTopEdge)
                    AddEdge(0, 1, 1, 1);
                if (isLeftEdge)
                    AddEdge(0, 1f - leftRatio, 0, 1);

                break;
            // *-----*
            // |     |
            // |    /|
            // *-----o
            case 13:
                segments.Add(
                    (offset + new Vector2(1, 1f - rightRatio),
                    offset + new Vector2(bottomRatio, 0))
                    );

                if (isBottomEdge)
                    AddEdge(bottomRatio, 0, 0, 0);
                if (isRightEdge)
                    AddEdge(1, 1, 1, 1f - rightRatio);
                if (isTopEdge)
                    AddEdge(0, 1, 1, 1);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, 1);

                break;
            // *-----*
            // |     |
            // |\    |
            // o-----*
            case 14:
                segments.Add(
                    (offset + new Vector2(1f - bottomRatio, 0),
                    offset + new Vector2(0, 1f - leftRatio))
                    );

                if (isBottomEdge)
                    AddEdge(1, 0, 1f - bottomRatio, 0);
                if (isRightEdge)
                    AddEdge(1, 1, 1, 0);
                if (isTopEdge)
                    AddEdge(0, 1, 1, 1);
                if (isLeftEdge)
                    AddEdge(0, 1f - leftRatio, 0, 1);

                break;
            // *-----*
            // |     |
            // |     |
            // *-----*
            case 15:
                if (isBottomEdge)
                    AddEdge(1, 0, 0, 0);
                if (isRightEdge)
                    AddEdge(1, 1, 1, 0);
                if (isTopEdge)
                    AddEdge(0, 1, 1, 1);
                if (isLeftEdge)
                    AddEdge(0, 0, 0, 1);
                break;
            default:
                break;
        }

        return val;
    }
}
