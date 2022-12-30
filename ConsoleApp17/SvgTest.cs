using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Svg;

namespace ConsoleApp17;
internal class SvgTest : Component
{
    private SvgDocument document;

    public override void Initialize(Entity parent)
    {
        document = Svg.SvgDocument.Open("./Assets/SVG/sheet.svg");
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        document.RenderElement(new SvgRenderer(canvas));
        base.Render(canvas);
    }

    private class SvgRenderer : ISvgRenderer
    {
        public float DpiY { get; }
        public SmoothingMode SmoothingMode { get; set; }
        public Matrix Transform 
        { 
            get => new(
                canvas.State.Transform.M11,
                canvas.State.Transform.M12,
                canvas.State.Transform.M21,
                canvas.State.Transform.M22,
                canvas.State.Transform.M31,
                canvas.State.Transform.M32
                );
            set => canvas.SetTransform(new(
                value.Elements[0],
                value.Elements[1],
                value.Elements[2],
                value.Elements[3],
                value.Elements[4],
                value.Elements[5]
                )); 
        }

        private readonly ICanvas canvas;

        public SvgRenderer(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        public void Dispose()
        {
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
        {
            throw new NotImplementedException();
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit, float opacity)
        {
            throw new NotImplementedException();
        }

        public void DrawImageUnscaled(Image image, Point location)
        {
            throw new NotImplementedException();
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            throw new NotImplementedException();
        }

        public void FillPath(Brush brush, GraphicsPath path)
        {
            Span<PointF> points = path.PathPoints;

            Span<Vector2> points2 = stackalloc Vector2[points.Length + 1];

            points2[points.Length] = new(points[0].X, points[0].Y);
            MemoryMarshal.Cast<PointF, Vector2>(points).CopyTo(points2);
            
            if (brush is SolidBrush solid)
                canvas.Fill(new SimulationFramework.Color(solid.Color.R, solid.Color.G, solid.Color.B));

            canvas.DrawPolygon(points2); 
            
            canvas.Stroke(SimulationFramework.Color.Yellow);
            canvas.DrawPolygon(points2);
        }

        public ISvgBoundable GetBoundable()
        {
            return new Boundable();
        }

        class Boundable : ISvgBoundable
        {
            public PointF Location { get; }
            public SizeF Size { get; }
            public RectangleF Bounds { get; }
        }

        public Region GetClip()
        {
            return new Region(rect: new(0, 0, canvas.Width, canvas.Height));
        }

        public ISvgBoundable PopBoundable()
        {
            return new Boundable();
        }

        public void RotateTransform(float fAngle, MatrixOrder order = MatrixOrder.Append)
        {
            switch (order)
            {
                case MatrixOrder.Prepend:
                    canvas.SetTransform(Matrix3x2.CreateRotation(fAngle) * canvas.State.Transform);
                    break;
                case MatrixOrder.Append:
                    canvas.SetTransform(canvas.State.Transform * Matrix3x2.CreateRotation(fAngle));
                    break;
                default:
                    throw new Exception();
            }
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Append)
        {
            switch (order)
            {
                case MatrixOrder.Prepend:
                    canvas.SetTransform(Matrix3x2.CreateScale(sx, sy) * canvas.State.Transform);
                    break;
                case MatrixOrder.Append:
                    canvas.SetTransform(canvas.State.Transform * Matrix3x2.CreateScale(sx, sy));
                    break;
                default:
                    throw new Exception();
            }
        }

        public void SetBoundable(ISvgBoundable boundable)
        {
        }

        public void SetClip(Region region, CombineMode combineMode = CombineMode.Replace)
        {
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Append)
        {
            switch (order)
            {
                case MatrixOrder.Prepend:
                    canvas.SetTransform(Matrix3x2.CreateTranslation(dx, dy) * canvas.State.Transform);
                    break;
                case MatrixOrder.Append:
                    canvas.SetTransform(canvas.State.Transform * Matrix3x2.CreateTranslation(dx, dy));
                    break;
                default:
                    throw new Exception();
            }
        }
    }
}
