using ConsoleApp17;
using ExCSS;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using SpaceGame.Components.Asteroid.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Components.Asteroid;
internal class AsteroidPolygon : Component
{
    public List<Vertices> Edges { get; private set; } = new();
    public List<Vertices> Triangles { get; private set; } = new();
    public Vector2[] TrianglesBuffer { get; private set; } = Array.Empty<Vector2>();

    public event Action? Invalidated;

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
    }

    public void Rebuild()
    {
        var chunk = ParentEntity.GetComponent<AsteroidChunk>() ?? throw new Exception();
        var manager = ParentEntity.ParentEntity.GetComponent<AsteroidChunkManager>() ?? throw new Exception();
        var volume = ParentEntity.GetComponent<MarchingSquaresVolume>() ?? throw new Exception();

        MarchingSquaresVolume? rightNeighbor = manager.GetChunk(chunk.x + 1, chunk.y)?.GetSibling<MarchingSquaresVolume>();
        MarchingSquaresVolume? topRightNeighbor = manager.GetChunk(chunk.x + 1, chunk.y + 1)?.GetSibling<MarchingSquaresVolume>();
        MarchingSquaresVolume? topNeighbor = manager.GetChunk(chunk.x, chunk.y + 1)?.GetSibling<MarchingSquaresVolume>();

        Edges = MarchingSquares.GetVertices(volume, rightNeighbor, topRightNeighbor, topNeighbor);

        if (Keyboard.IsKeyPressed(Key.Q))
            Debugger.Break();

        try
        {
            Triangles = Edges.SelectMany(p => Triangulate.ConvexPartition(p, TriangulationAlgorithm.Delauny)).Where(p => p.Count is 3).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("triangulation error: " + ex);
            Triangles = Edges;
        }

        TrianglesBuffer = Triangles.Where(v => v.Count is 3).SelectMany(v => v.Select(v => v.AsNumericsVector())).ToArray();

        foreach (var t in Triangles.Where(v => v.Count is not 3))
        {
            Console.WriteLine(t);
        }

        Invalidated?.Invoke();
    }
}