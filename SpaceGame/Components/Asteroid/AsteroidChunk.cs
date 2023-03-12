using ConsoleApp17;
using SpaceGame;
using SpaceGame.Components.Asteroid.Algorithms;
using System.Diagnostics;

namespace SpaceGame.Components.Asteroid;

class AsteroidChunk : Component
{
    public const int CHUNK_SIZE = 16;

    public int x, y;
    public bool dirty = true;
    public float timeSinceLastEdit;

    public AsteroidChunk()
    {
    }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
        timeSinceLastEdit += Time.DeltaTime;
        if (dirty)
        {
            GetSibling<AsteroidPolygon>().Rebuild();
            dirty = false;
            timeSinceLastEdit = 0;
        }
    }

    public override void Render(ICanvas canvas)
    {
        if (DebugDrawWindow.Instance.DrawChunkBorders)
        {
            canvas.StrokeWidth(0);
            canvas.Stroke(Color.Yellow);

            canvas.DrawRect(0, 0, CHUNK_SIZE, CHUNK_SIZE);
        }

        canvas.Fill(Color.Red);
        canvas.FontStyle(.5f, FontStyle.Normal);
        canvas.Scale(1, -1);
        canvas.DrawText($"{x}, {y}", 0, 0);

        base.Render(canvas);
    }

    //public record struct Provider(int X, int Y) : IComponentProvider<AsteroidChunk>
    //{
    //    public AsteroidChunk CreateComponent(Entity parent)
    //    {
    //        return new(X, Y) { ParentEntity = parent } ;
    //    }
    //}

    public bool IsEmpty()
    {
        var manager = ParentEntity.GetSibling<AsteroidChunkManager>();
        var volume = GetSibling<MarchingSquaresVolume>();

        MarchingSquaresVolume? rightNeighbor = manager.GetChunk(x + 1, y)?.GetSibling<MarchingSquaresVolume>();
        MarchingSquaresVolume? topRightNeighbor = manager.GetChunk(x + 1, y + 1)?.GetSibling<MarchingSquaresVolume>();
        MarchingSquaresVolume? topNeighbor = manager.GetChunk(x, y + 1)?.GetSibling<MarchingSquaresVolume>();

        for (int y = 0; y < volume.Height; y++)
        {
            for (int x = 0; x < volume.Width; x++)
            {
                if (volume[x, y] >= MarchingSquares.THRESHOLD)
                    return false;
            }
        }

        if (rightNeighbor is not null)
        {
            for (int y = 0; y < volume.Height; y++)
            {
                if (rightNeighbor[0, y] >= MarchingSquares.THRESHOLD)
                    return false;
            }
        }

        if (topNeighbor is not null)
        {
            for (int x = 0; x < volume.Width; x++)
            {
                if (topNeighbor[x, 0] >= MarchingSquares.THRESHOLD)
                    return false;
            }
        }

        if (topRightNeighbor is not null && topRightNeighbor[0, 0] >= MarchingSquares.THRESHOLD)
            return false;

        return true;
    }
}