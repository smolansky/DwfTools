using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using DwfTools.W2d.Opcodes;
using DwfTools.Renderer;

namespace DwfTools.Controls
{
    public class GdiRendering
    {
        private Graphics _graphics;
        private Graphics _canvas;
        private Rectangle _rectangle;

        public GdiRendering(Graphics graphics, Rectangle rectangle)
        {
            _graphics = graphics;
            _rectangle = rectangle;
        }

        readonly bool currentFillMode = false;

        bool currentIsVisible = true;

        bool absoluteCoordinates = false;

        Point currentPoint = Point.Zero;

        int currentLineWeight = 0;

        Color currentColor = Color.Black;

        List<Color> indexedColors = DefaultColorMap.Default.ToList();

        int polyCount = 0;

        public void Render(IEnumerable<IOpcode> opcodes)
        {
            using (Bitmap backBuffer = new Bitmap(_rectangle.Width, _rectangle.Height))
            using (_canvas = Graphics.FromImage(backBuffer))
            {
                foreach (var opcode in opcodes)
                {
                    Render(opcode);
                }

                _graphics.DrawImage(backBuffer, 0, 0);
            }
        }

        void Render(IOpcode opcode)
        {
            if (opcode is SetVisibillityOn)
            {
                currentIsVisible = true;
            }
            else if (opcode is SetCurrentPoint)
            {
                var op = (SetCurrentPoint)opcode;
                currentPoint = new Point(op.X, op.Y);
            }
            else if (opcode is SetLineWeight)
            {
                //var op = (SetLineWeight)opcode;
                //currentLineWeight = op.Weight / 20; //TODO: Just some constant to try and get a sane line weight
            }
            else if (opcode is DrawPolylineShort)
            {
                polyCount++;
                absoluteCoordinates = false;
                var op = (DrawPolylineShort)opcode;
                DrawPolyline(op.Points);
            }
            else if (opcode is DrawPolylineLong)
            {
                absoluteCoordinates = false;
                var op = (DrawPolylineLong)opcode;
                DrawPolyline(op.Points);
            }
            else if (opcode is DrawPolymarkerShort)
            {
                absoluteCoordinates = false;
                var op = (DrawPolymarkerShort)opcode;
                DrawPolymarker(op.Points);
            }
            else if (opcode is DrawPolymarkerLong)
            {
                absoluteCoordinates = false;
                var op = (DrawPolymarkerLong)opcode;
                DrawPolymarker(op.Points);
            }
            else if (opcode is DrawLineShort)
            {
                absoluteCoordinates = false;
                var op = (DrawLineShort)opcode;
                DrawLine(op.StartingPoint, op.EndPoint);
            }
            else if (opcode is DrawLineLong)
            {
                absoluteCoordinates = false;
                var op = (DrawLineLong)opcode;
                DrawLine(op.StartingPoint, op.EndPoint);
            }
            else if (opcode is SetVisibillityOff)
            {
                currentIsVisible = false;
            }
            else if (opcode is SetColorIndex)
            {
                var op = (SetColorIndex)opcode;
                SetColorIndex(op.Index);
            }
            else if (opcode is SetColorRgba)
            {
                var op = (SetColorRgba)opcode;
                SetColorRgba(Color.FromArgb(op.A, op.R, op.G, op.B));
            }
            else if (opcode is DrawTextBasic)
            {
                var op = (DrawTextBasic)opcode;
                DrawTextBasic(op.Position, op.Text);
            }
            else
            {
                var op = (UnrecognizedOpcode)opcode;

                if (op.OpcodeId == "") return;
                if (op.OpcodeId == "URL") return;

                Console.WriteLine("unrecognized opcode:" + op.OpcodeId);

                return;
            }
        }

        private void DrawTextBasic(Point point, string text)
        {
            var absolutePoint = GetTranslatedPoint(point);
            var textElement = "<text x=\"{0}\" y=\"{1}\" font-family=\"Verdana\" font-size=\"80\" fill=\"blue\">{2}</text>";
            //AppendTextToOutput(string.Format(textElement, absolutePoint.X, absolutePoint.Y, text));
        }

        private void DrawPolymarker(Point[] points)
        {
            //TODO: actually draw the polymarkers
            foreach (var point in points)
            {
                var absolutePoint = GetTranslatedPoint(point);
                var pointText = "<circle cx=\"{0}\" cy=\"{1}\" r=\"10\" {2} />";
                // AppendTextToOutput(string.Format(pointText, absolutePoint.X, absolutePoint.Y, GetCurrentStyle()));
            }
        }

        public void DrawLine(Point point1, Point point2)
        {
            const string line = "<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" {4} />";

            point1 = GetTranslatedPoint(point1);
            point2 = GetTranslatedPoint(point2);
            //  AppendTextToOutput(string.Format(line, point1.X, point1.Y, point2.X, point2.Y, GetCurrentStyle()));
        }

        public void DrawPolyline(Point[] points)
        {
            _canvas.DrawLines(Pens.Black, points.Select(p => GetTranslatedPoint(p))
                                                .Select(p => new PointF((p.X / 200.0f) * zoom, (p.Y / 200.0f) * zoom))
                                                .ToArray());


            //var pointsString = new StringBuilder();

            //var firstPoint = GetTranslatedPoint(points[0]);
            //pointsString.AppendLine("ctx.beginPath();");
            //pointsString.AppendLine(string.Format("ctx.moveTo({0}, {1});", firstPoint.X / 1000, -firstPoint.Y / 1000));

            //for (int i = 1; i < points.Length; i++)
            //{
            //    var currentPoint = GetTranslatedPoint(points[i]);
            //    pointsString.AppendLine(string.Format("ctx.lineTo({0}, {1});", currentPoint.X / 1000, -currentPoint.Y / 1000));
            //}

            //pointsString.AppendLine("ctx.stroke();");

            //AppendTextToOutput(pointsString.ToString());
        }

        private void SetColorIndex(byte colorIndex)
        {
            if (indexedColors.Count <= colorIndex) return;

            currentColor = indexedColors[colorIndex];
        }

        private void SetColorRgba(Color color)
        {
            currentColor = color;
        }

        Point GetTranslatedPoint(Point point)
        {
            if (!absoluteCoordinates)
            {
                //translate point to absolute coordinates
                point = new Point(point.X + currentPoint.X,
                                  point.Y + currentPoint.Y);

            }

            currentPoint = point;

            //Invert Y coordinate
            return new Point(point.X, point.Y);
        }

        private void AppendTextToOutput(string text)
        {
            //only output visible things
            if (!currentIsVisible) return;

            //canvasString.AppendLine(text);
        }

        public int zoom { get; set; }
    }
}
