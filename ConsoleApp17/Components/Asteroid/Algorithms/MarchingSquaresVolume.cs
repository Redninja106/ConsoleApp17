using Silk.NET.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Asteroid.Algorithms;
internal class MarchingSquaresVolume : Component
{
    private float[] data;
    public int Width => AsteroidChunk.CHUNK_SIZE;
    public int Height => AsteroidChunk.CHUNK_SIZE;

    public ref float this[int x, int y] => ref data[y * Width + x];

    public MarchingSquaresVolume()
    {
        data = new float[Width * Height];
    }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
    }

    public void CopyTo(MarchingSquaresVolume? newVolume)
    {
        ArgumentNullException.ThrowIfNull(newVolume);

        data.CopyTo(newVolume.data.AsSpan());
    }
}