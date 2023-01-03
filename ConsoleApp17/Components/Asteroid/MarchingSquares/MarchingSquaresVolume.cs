using Silk.NET.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Asteroid.MarchingSquares;
internal class MarchingSquaresVolume : Component
{
    private float[] data;
    public int Width { get; set; }
    public int Height { get; set; }

    public ref float this[int x, int y] => ref data[y * Width + x];

    public MarchingSquaresVolume()
    {
    }

    public override void Initialize(Entity parent)
    {
        data = new float[Width * Height];
    }

    public override void Update()
    {
    }
}