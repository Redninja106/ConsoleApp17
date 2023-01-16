using Genbox.VelcroPhysics.Shared;
using SimulationFramework.Drawing;
using SimulationFramework.SkiaSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
internal static class CanvasExtensions
{
    public static void DrawSprite(this ICanvas canvas, Sprite sprite)
    {
        sprite.Render(canvas);
    }

    public static unsafe void DrawVertices(this ICanvas canvas, Vertices vertices)
    {
        var span = CollectionsMarshal.AsSpan(vertices);

        if (!span.IsEmpty)
        {
            canvas.DrawPolygon(MemoryMarshal.Cast<XNAVector2, Vector2>(span));

            canvas.DrawLine(vertices.First().AsNumericsVector(), vertices.Last().AsNumericsVector());

            if (vertices.Holes is not null)
            {
                foreach (var hole in vertices.Holes)
                {
                    DrawVertices(canvas, hole);
                }
            }
        }
    }

    private static SKPaint trisPaint = new()
    {
        FilterQuality = SKFilterQuality.High,
        Style = SKPaintStyle.Fill,
        Shader = SKShader.CreateColor(SKColors.Gray),
        Color = SKColors.White,
        IsAntialias = true,
    };

    public static unsafe void DrawTriangles(this ICanvas canvas, Span<Vector2> triangles)
    {
        var skcanvas = SkiaInterop.GetCanvas(canvas);

        fixed (Vector2* trisPtr = triangles) 
        {
            var verts = SkiaNative.sk_vertices_make_copy(SKVertexMode.Triangles, triangles.Length, (SKPoint*)trisPtr, (SKPoint*)trisPtr, null, 0, null);

            SkiaNative.sk_canvas_draw_vertices(skcanvas.Handle, verts, SKBlendMode.SrcOver, trisPaint.Handle);

            SkiaNative.sk_vertices_unref(verts);
        }
    }

    unsafe static class SkiaNative
    {
        [DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sk_vertices_make_copy(SKVertexMode vmode, int vertexCount, SKPoint* positions, SKPoint* texs, uint* colors, int indexCount, ushort* indices);

        [DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sk_canvas_draw_vertices(IntPtr canvas, IntPtr vertices, SKBlendMode mode, IntPtr paint);

        [DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sk_vertices_unref(IntPtr cvertices);
    }
}
