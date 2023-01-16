using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp17.Components.Asteroid.Algorithms;
using ConsoleApp17.Physics;
using Genbox.VelcroPhysics.Shared;
using Silk.NET.Input.Extensions;

namespace ConsoleApp17.Components.Asteroid;
internal class AsteroidChunkManager : Component
{
    private readonly List<AsteroidChunk> chunks = new();
    private List<List<IslandPart>> islands;

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        if (chunks.Any())
        {
            var c = chunks[Random.Shared.Next(chunks.Count)];
            if (c.timeSinceLastEdit > 5 && c.IsEmpty())
            {
                chunks.Remove(c);
                c.ParentEntity.Destroy();
            }
        }
        else
        {
            this.ParentEntity.Destroy();
        }

        var polys = chunks.Select(chunk => (chunk, chunk.GetSibling<AsteroidPolygon>()!.Edges)).SelectMany(t => t.Edges.Select(e => new IslandPart(e, t.chunk))).ToList();
        islands = GetIslands(polys);
        SplitIslands(islands);
    }

    static Color[] colors = typeof(Color).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Select(f => (Color)f.GetValue(null)).ToArray();

    [Button]
    public void PrintIslands()
    {
        var polys = chunks.Select(chunk => (chunk, chunk.GetSibling<AsteroidPolygon>()!.Edges)).SelectMany(t => t.Edges.Select(e => new IslandPart(e, t.chunk))).ToList();

        islands = GetIslands(polys);
        foreach (var island in islands)
        {
            Console.WriteLine("START ISLAND");
            foreach (var part in island)
            {
                Console.WriteLine(part);
            }
        }

        // SplitIslands(islands);
    }

    public override void Render(ICanvas canvas)
    {
        if (islands is not null)
        {
            for (int i = 0; i < islands.Count; i++)
            {
                canvas.Stroke(colors[i]);
                canvas.StrokeWidth(.1f);
                foreach (var verts in islands[i])
                {
                    canvas.Translate(verts.Position * AsteroidChunk.CHUNK_SIZE);
                    canvas.DrawVertices(verts.Polygon);
                    canvas.Translate(-verts.Position * AsteroidChunk.CHUNK_SIZE);
                }
            }
        }

        base.Render(canvas);
    }

    private AsteroidChunk CreateChunk(int x, int y)
    {
        var entity = Entity.Create("./Components/Asteroid/asteroidChunk.arch", this.ParentEntity);

        entity.Transform.Position = new(x * AsteroidChunk.CHUNK_SIZE, y * AsteroidChunk.CHUNK_SIZE);

        var chunk = entity.GetComponent<AsteroidChunk>();
        chunk.x = x;
        chunk.y = y;

        chunks.Add(chunk);

        return chunk;
    }

    public void Modify(AsteroidBrush brush, Vector2 position)
    {
        var bounds = brush.GetBounds(position);

        for (int y = 0; y < bounds.Height; y++)
        {
            for (int x = 0; x < bounds.Width; x++)
            {
                int ix = (int)MathF.Round(bounds.X + x);
                int iy = (int)MathF.Round(bounds.Y + y);

                int cx = (int)MathF.Floor(ix / (float)AsteroidChunk.CHUNK_SIZE),
                    cy = (int)MathF.Floor(iy / (float)AsteroidChunk.CHUNK_SIZE);

                AsteroidChunk chunk = GetOrCreateChunk(cx, cy);
                MarchingSquaresVolume volume = chunk.GetSibling<MarchingSquaresVolume>() ?? throw new Exception();

                int localX = ix - chunk.x * AsteroidChunk.CHUNK_SIZE;
                int localY = iy - chunk.y * AsteroidChunk.CHUNK_SIZE;

                brush.ApplyTo(ref volume[localX, localY], new(ix, iy), position);

                chunk.dirty = true;
            }
        }
    }

    public AsteroidChunk GetOrCreateChunk(int x, int y)
    {
        return GetChunk(x, y) ?? CreateChunk(x, y);
    }

    public AsteroidChunk? GetChunk(int x, int y)
    {
        return chunks.SingleOrDefault(c => c.x == x && c.y == y);
    }

    void SplitIslands(List<List<IslandPart>> islands)
    {
        if (islands.Count < 2)
            return;
        
        islands.Sort((a, b) => Comparer<int>.Default.Compare(a.Count, b.Count));

        foreach (var island in islands.SkipLast(1))
        {
            var newAsteroid = Entity.Create("./Components/Asteroid/asteroid.arch", Scene.Active);

            newAsteroid.Transform.Position = this.ParentTransform.Position;
            newAsteroid.Transform.Rotation = this.ParentTransform.Rotation;

            var asteroidManager = newAsteroid.GetComponent<AsteroidChunkManager>();

            foreach (var part in island)
            {
                var newChunk = asteroidManager.GetOrCreateChunk(part.Chunk.x, part.Chunk.y);
                var newVolume = newChunk.GetSibling<MarchingSquaresVolume>();
                var oldVolume = part.Chunk.GetSibling<MarchingSquaresVolume>();

                if (part.Chunk.GetSibling<AsteroidPolygon>().Edges.Count is 1)
                {
                    // copy whole chunk
                    oldVolume!.CopyTo(newVolume);

                    this.chunks.Remove(part.Chunk);
                    part.Chunk.ParentEntity.Destroy();
                }
                else
                {
                    // copy part of chunk
                    var volume = part.Chunk.GetSibling<MarchingSquaresVolume>();

                    part.Chunk.dirty = true;

                    for (int y = 0; y < volume.Height; y++)
                    {
                        for (int x = 0; x < volume.Width; x++)
                        {
                            XNAVector2 point = new(x, y);
                            if (part.Polygon.PointInPolygonAngle(ref point))
                            {
                                newVolume[x, y] = oldVolume[x, y];
                                oldVolume[x, y] = 0f;
                            }
                            else if (oldVolume[x, y] < MarchingSquares.THRESHOLD)
                            {
                                newVolume[x, y] = oldVolume[x, y];
                            }
                        }
                    }
                }
            }
        }
    }

    private List<List<IslandPart>> GetIslands(List<IslandPart> polygons)
    {
        if (Mouse.IsButtonDown(MouseButton.Left))
        {
            Console.WriteLine('e');
        }

        List<List<IslandPart>> results = new();

        while (polygons.Any(p => !HasIsland(p)))
        {
            var poly = polygons.First(p => !HasIsland(p));

            List<IslandPart> island = new() { poly };
            results.Add(island);

            FillIsland(poly, polygons, island);
        }

        return results;

        bool HasIsland(IslandPart part) => results.Any(i => i.Contains(part));

        void FillIsland(IslandPart firstPart, List<IslandPart> polygons, List<IslandPart> island)
        {
            for (int i = 0; i < polygons.Count; i++)
            {
                var otherPart = polygons[i];

                if (HasIsland(otherPart))
                    continue;

                if (Contacts(firstPart, otherPart))
                {
                    island.Add(otherPart);

                    FillIsland(otherPart, polygons, island);
                }
            }
        }
    }

    bool Contacts(IslandPart partA, IslandPart partB)
    {
        var diff = partB.Position - partA.Position;

        if (diff.LengthSquared() != 1)
            return false;
        if (diff == Vector2.UnitX)
            return ContactsOnEdge(ContactEdge.Right, partA.Polygon, partB.Polygon);
        if (diff == -Vector2.UnitX)
            return ContactsOnEdge(ContactEdge.Left, partA.Polygon, partB.Polygon);
        if (diff == Vector2.UnitY)
            return ContactsOnEdge(ContactEdge.Top, partA.Polygon, partB.Polygon);
        if (diff == -Vector2.UnitY)
            return ContactsOnEdge(ContactEdge.Bottom, partA.Polygon, partB.Polygon);
        throw new Exception();
    }

    bool ContactsOnEdge(ContactEdge edge, Vertices polyA, Vertices polyB)
    {
        var otherEdge = InvertEdge(edge);

        for (int i = 0; i < polyA.Count; i++)
        {
            Vector2 pointA = polyA[i].AsNumericsVector();

            if (!IsOnEdge(pointA, edge))
                continue;

            Vector2 otherA = polyA[i + 1 >= polyA.Count ? 0 : i + 1].AsNumericsVector();

            if (!IsOnEdge(otherA, edge))
                continue;

            // skip other point
            i++;

            for (int j = 0; j < polyB.Count; j++)
            {
                Vector2 pointB = polyB[j].AsNumericsVector();

                if (!IsOnEdge(pointB, otherEdge))
                    continue;

                Vector2 otherB = polyB[j + 1 >= polyB.Count ? 0 : j + 1].AsNumericsVector();

                if (!IsOnEdge(otherB, otherEdge))
                    continue;

                // skip other point
                j++;

                if (edge is ContactEdge.Bottom or ContactEdge.Top)
                {
                    if (SegmentIntersect(pointA.X, otherA.X, pointB.X, otherB.X))
                        return true;
                }
                else if (edge is ContactEdge.Left or ContactEdge.Right)
                {
                    if (SegmentIntersect(pointA.Y, otherA.Y, pointB.Y, otherB.Y))
                        return true;
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        return false;
    }

    private bool SegmentIntersect(float a1, float b1, float a2, float b2)
    {
        return MathF.Min(MathF.Max(a1, b1), MathF.Max(a2, b2)) >= MathF.Max(MathF.Min(a1, b1), MathF.Min(a2, b2));
    }

    private bool IsOnEdge(Vector2 point, ContactEdge edge)
    {
        return edge switch
        {
            ContactEdge.Left => point.X is 0,
            ContactEdge.Bottom => point.Y is 0,
            ContactEdge.Right => point.X is AsteroidChunk.CHUNK_SIZE,
            ContactEdge.Top => point.Y is AsteroidChunk.CHUNK_SIZE,
            _ => throw new Exception(),
        };
    }

    private ContactEdge InvertEdge(ContactEdge edge)
    {
        return edge switch
        {
            ContactEdge.Left => ContactEdge.Right,
            ContactEdge.Right => ContactEdge.Left,
            ContactEdge.Bottom => ContactEdge.Top,
            ContactEdge.Top => ContactEdge.Bottom,
            _ => throw new Exception(),
        };
    }

    enum ContactEdge
    {
        Left,
        Right,
        Top,
        Bottom
    }

    struct IslandPart
    {
        public Vertices Polygon;
        public AsteroidChunk Chunk;
        public Vector2 Position;

        public IslandPart(Vertices polygon, AsteroidChunk chunk)
        {
            Polygon = polygon;
            Chunk = chunk;
            Position = new(Chunk.x, Chunk.y);
        }
    }
}