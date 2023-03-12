using ConsoleApp17;
using ConsoleApp17.Components.Asteroid.Algorithms;
using SpaceGame.Components.Asteroid.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Components.Asteroid;
internal class AsteroidGenerator : Component
{
    public float Scale;
    public bool visible;
    public Vector2 offset;

    [Button]
    public void Generate()
    {
        var perlin = new PerlinNoise(Random.Shared.Next());

        var asteroid = Entity.Create(Archetypes.Asteroid, Scene.Active);

        var chunkManager = asteroid.GetComponent<AsteroidChunkManager>();

        for (int y = -2; y <= 2; y++)
        {
            for (int x = -2; x <= 2; x++)
            {
                var c = chunkManager.GetOrCreateChunk(x, y);

                var volume = c.GetSibling<MarchingSquaresVolume>();

                for (int cy = 0; cy < volume.Height; cy++)
                {
                    for (int cx = 0; cx < volume.Width; cx++)
                    {
                        Vector2 pos = new(x * volume.Width + cx, y * volume.Height + cy);
                        float value = perlin.Sample(pos * Scale) * .5f + .5f;
                        volume[cx, cy] = value;
                    }
                }

                c.dirty = true;
            }
        }
    }

    [Button]
    public void Show()
    {
        visible = !visible;
    }

    public override void Initialize(Entity parent)
    {
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        if (visible)
        {
            var perlin = new PerlinNoise(0);

            for (int y = -2; y <= 2; y++)
            {
                for (int x = -2; x <= 2; x++)
                {
                    for (int cy = 0; cy < AsteroidChunk.CHUNK_SIZE; cy++)
                    {
                        for (int cx = 0; cx < AsteroidChunk.CHUNK_SIZE; cx++)
                        {
                            Vector2 pos = new Vector2(x * AsteroidChunk.CHUNK_SIZE + cx, y * AsteroidChunk.CHUNK_SIZE + cy);
                            canvas.DrawCircle(pos, .5f * perlin.Sample(pos * Scale + offset));
                        }
                    }
                }
            }
        }
        base.Render(canvas);
    }
}
