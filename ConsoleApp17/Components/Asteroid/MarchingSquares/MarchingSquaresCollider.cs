using ConsoleApp17.Physics;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;

namespace ConsoleApp17.Components.Asteroid.MarchingSquares;

class MarchingSquaresCollider : Collider
{
    private MarchingSquaresVolume volume;

    private readonly List<Vertices> polygons = new();
    private readonly List<PolygonShape> shapes = new();
    private readonly List<Fixture> fixtures = new();
    Vector2[][] vertices;

    private PhysicsBody body;

    public override void Initialize(Entity parent)
    {
        volume = parent.GetComponent<MarchingSquaresVolume>() ?? throw new Exception();
        body = parent.GetComponent<PhysicsBody>() ?? throw new Exception();
        vertices = GetVertices();
    }

    public override void Update()
    {

        //polygons.Clear();
        //foreach (Vector2[] p in vertices)
        //{
        //    polygons.Add(new Vertices(p.Select(v => v.AsXNA())));
        //}

        //shapes.Clear();
        //shapes.AddRange(polygons.Select(p => new PolygonShape(p, 1.0f)));

        //foreach (var f in fixtures)
        //{
        //    body.InternalBody.DestroyFixture(f);
        //}

        //fixtures.Clear();
        //foreach (var shape in shapes)
        //{
        //    fixtures.Add(body.InternalBody.CreateFixture(shape));
        //}
    }

    public override Shape CreateShape(float density)
    {
        return null!;
    }

    public override void Render(ICanvas canvas)
    {
        vertices = GetVertices();
        canvas.StrokeWidth(0.1f);
        canvas.Stroke(Color.Blue);
        if (vertices is not null)
        {
            foreach (var poly in vertices)
            {
                if (poly.Length >= 3)
                    canvas.DrawPolygon(poly);
            }
        }

        base.Render(canvas);
    }

    public Vector2[][] GetVertices()
    {
        List<(Vector2 first, Vector2 second)> segments = new();

        for (int y = 0; y < volume.Height-1; y++)
        {
            for (int x = 0; x < volume.Width-1; x++)
            {
                ProcessCell(volume[x, y], volume[x + 1, y], volume[x, y + 1], volume[x + 1, y + 1], new(x, y), segments);
            }
        }

        if (!segments.Any())
            return Array.Empty<Vector2[]>();

        List<List<Vector2>> polygon = new();

        polygon.Add(new()
        {
            segments[0].first,
            segments[0].second
        });

        segments.RemoveAt(0);

        int polyIndex = 0;
        int index = 1;

        while (segments.Any())
        {
            Vector2 pt = polygon[polyIndex][index];

            if (segments.Any(s => s.first == pt || s.second == pt))
            {
                var segment = segments.Single(s => s.first == pt || s.second == pt);
                segments.Remove(segment);

                if (segment.first == pt)
                {
                    polygon[polyIndex].Add(segment.second);
                }
                else
                {
                    polygon[polyIndex].Add(segment.first);
                }

                index++;
            }
            else
            {
                polyIndex++;
                polygon.Add(new()
                {
                    segments[0].first,
                    segments[0].second
                });
                segments.RemoveAt(0);
                index = 1;
            }
        }

        return polygon.Select(l => l.ToArray()).ToArray();
    }


    private void ProcessCell(float bl, float br, float tl, float tr, Vector2 offset, List<(Vector2, Vector2)> segments)
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
                segments.Add(
                    (offset + new Vector2(0, leftRatio),
                    offset + new Vector2(bottomRatio, 0))
                    );
                break;
            // o-----o
            // |     |
            // |    /|
            // o-----*
            case 2:
                segments.Add(
                    (offset + new Vector2(1, rightRatio),
                    offset + new Vector2(1f - bottomRatio, 0))
                    );
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
                break;
            // o-----*
            // |    \|
            // |     |
            // o-----o
            case 4:
                segments.Add(
                    (offset + new Vector2(1f - topRatio, 1), 
                    offset + new Vector2(1, 1f - rightRatio))
                    );
                break;
            case 5:
                if (avg > threshold)
                {
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
                        (offset + new Vector2(0, leftRatio),
                        offset + new Vector2(bottomRatio, 0))
                        );

                    segments.Add(
                        (offset + new Vector2(1f - topRatio, 1),
                        offset + new Vector2(1, 1f - rightRatio))
                        );
                }
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
                break;
            // o-----*
            // |/    |
            // |     |
            // *-----*
            case 7:
                segments.Add(
                    (offset + new Vector2(1f - topRatio, 1),
                    offset + new Vector2(0, leftRatio))
                    );
                break;
            // *-----o
            // |/    |
            // |     |
            // o-----o
            case 8:
                segments.Add(
                    (offset + new Vector2(0, 1f - leftRatio),
                    offset + new Vector2(topRatio, 1))
                    );
                break;
            // *---->o
            // ^  |  ^
            // |  |  |
            // *---->o
            case 9:
                segments.Add(
                    (offset + new Vector2(topRatio, 1),
                    offset + new Vector2(bottomRatio, 0))
                    );
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
                        (offset + new Vector2(0, 1f - leftRatio),
                        offset + new Vector2(topRatio, 1))
                        );


                    segments.Add(
                        (offset + new Vector2(1, rightRatio),
                        offset + new Vector2(1f - bottomRatio, 0))
                        );
                }
                break;
            // *-----o
            // |    \|
            // |     |
            // *-----*
            case 11:
                segments.Add(
                    (offset + new Vector2(1, rightRatio),
                    offset + new Vector2(topRatio, 1))
                    );
                break;
            // *-----*
            // |_____|
            // |     |
            // o-----o
            case 12:
                segments.Add(
                    (offset + new Vector2(0, 1f - leftRatio),
                    offset + new Vector2(1, 1f - rightRatio))
                    );
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
                break;
            // *-----*
            // |     |
            // |     |
            // *-----*
            case 15:
                break;
            default:
                break;
        }
    }
}
