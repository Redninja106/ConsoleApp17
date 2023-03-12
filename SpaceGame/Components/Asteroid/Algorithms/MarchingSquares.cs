using ConsoleApp17;
using Genbox.VelcroPhysics;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.PolygonManipulation;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using Genbox.VelcroPhysics.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Components.Asteroid.Algorithms;
internal static class MarchingSquares
{
    public const float THRESHOLD = 0.5f;

    public static List<Vertices> GetVertices(MarchingSquaresVolume volume, MarchingSquaresVolume? rightNeighbor, MarchingSquaresVolume? topRightNeighbor, MarchingSquaresVolume? topNeighbor)
    {
        List<(Vector2 first, Vector2 second)> segments = new();

        for (int y = 0; y < volume.Height; y++)
        {
            for (int x = 0; x < volume.Width; x++)
            {
                float bl, br, tl, tr;

                bl = volume[x, y];

                if (x + 1 == volume.Width && y + 1 == volume.Height)
                {
                    tr = topRightNeighbor?[0, 0] ?? 0;
                    br = rightNeighbor?[0, y] ?? 0;
                    tl = topNeighbor?[x, 0] ?? 0;
                }
                else if (y + 1 == volume.Height)
                {
                    tr = topNeighbor?[x + 1, 0] ?? 0;
                    tl = topNeighbor?[x, 0] ?? 0;
                    br = volume[x + 1, y];
                }
                else if (x + 1 == volume.Width)
                {
                    tr = rightNeighbor?[0, y + 1] ?? 0;
                    br = rightNeighbor?[0, y] ?? 0;
                    tl = volume[x, y + 1];
                }
                else
                {
                    tr = volume[x + 1, y + 1];
                    br = volume[x + 1, y];
                    tl = volume[x, y + 1];
                }

                ProcessCell(x, y, volume.Width, volume.Height, bl, br, tl, tr, segments);
            }
        }

        if (!segments.Any())
            return new();

        List<List<Vector2>> polygons = new();

        polygons.Add(new()
        {
            segments[0].first,
            segments[0].second
        });

        segments.RemoveAt(0);

        int polyIndex = 0;
        int index = 1;

        while (segments.Any())
        {
            Vector2 pt = polygons[polyIndex][index];

            if (segments.Any(s => s.first == pt || s.second == pt))
            {
                var segment = segments.First(s => s.first == pt || s.second == pt);
                segments.Remove(segment);

                if (segment.first == pt)
                {
                    polygons[polyIndex].Add(segment.second);
                }
                else
                {
                    polygons[polyIndex].Add(segment.first);
                }

                index++;
            }
            else
            {
                polyIndex++;
                polygons.Add(new()
                {
                    segments[0].first,
                    segments[0].second
                });
                segments.RemoveAt(0);
                index = 1;
            }
        }

        // nearby point welding/colinear simplification
        var polys = polygons.Select(l => new Vertices(l.Select(v => v.AsXNA()))).ToList();
        polys = polys.Select(p => SimplifyTools.DouglasPeuckerSimplify(p, 0.0f)).ToList();
        polys = polys.Select(p => SimplifyTools.ReduceByDistance(p, 0.01f)).ToList();

        // detect holes 
        foreach (var hole in polys.Where(p => p.IsCounterClockWise()).ToArray())
        {
            foreach (var outer in polys.Where(p => !p.IsCounterClockWise()).ToArray())
            {
                var point = hole[0];
                if (outer.PointInPolygonAngle(ref point))
                {
                    polys.Remove(hole);
                    (outer.Holes ??= new()).Add(hole);
                    break;
                }
            }
        }

        // polys = polys.Select(p => SimplifyTools.ReduceByDistance(p, 0.01f));

        return polys;
    }


    static float GetRatio(float a, float b)
    {
        float v1 = MathF.Max(a, b);
        float v2 = MathF.Min(a, b);

        return (THRESHOLD - v1) / (v2 - v1);
    }

    private static int ProcessCell(int x, int y, int width, int height, float bl, float br, float tl, float tr, List<(Vector2, Vector2)> segments)
    {
        var offset = new Vector2(x, y);

        void AddEdge(float x1, float y1, float x2, float y2)
        {
            segments.Add((offset + new Vector2(x1, y1), offset + new Vector2(x2, y2)));
        }

        int val = 0;
        val |= (tl > THRESHOLD ? 1 : 0) << 3;
        val |= (tr > THRESHOLD ? 1 : 0) << 2;
        val |= (br > THRESHOLD ? 1 : 0) << 1;
        val |= (bl > THRESHOLD ? 1 : 0) << 0;

        float avg = (bl + br + tl + tr) / 4f;

        float topRatio = GetRatio(tl, tr),
            bottomRatio = GetRatio(bl, br),
            leftRatio = GetRatio(bl, tl),
            rightRatio = GetRatio(br, tr);


        bool isBottomEdge = y == 0;
        bool isTopEdge = y == height - 1;

        bool isLeftEdge = x == 0;
        bool isRightEdge = x == width - 1;

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
                if (avg > THRESHOLD)
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
                if (avg > THRESHOLD)
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

    private static Vector2[] Triangulate2(List<Vector2> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            var prev = i == 0 ? vertices.Last() : vertices[i];

            for (int j = 0; j < vertices.Count; j++)
            {
                if (i == j)
                    continue;
            }
        }

        return null;
    }
}