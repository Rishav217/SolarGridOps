using Microsoft.Maui.Graphics;

namespace SolarGridOps.Maui.Controls;

public class LineSparklineDrawable : IDrawable
{
    public float[] Values { get; set; } = [];
    public Color LineColor { get; set; } = Colors.DodgerBlue;
    public Color FillColor { get; set; } = Color.FromArgb("#332F6FED");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values.Length < 2)
        {
            return;
        }

        var min = Values.Min();
        var max = Values.Max();
        var range = max - min;
        if (range <= 0)
        {
            range = 1;
        }

        const float padding = 6f;
        var width = dirtyRect.Width - (padding * 2);
        var height = dirtyRect.Height - (padding * 2);
        var stepX = width / (Values.Length - 1);

        var points = new PointF[Values.Length];
        for (var i = 0; i < Values.Length; i++)
        {
            var x = padding + (i * stepX);
            var normalized = (Values[i] - min) / range;
            var y = padding + height - (normalized * height);
            points[i] = new PointF(x, y);
        }

        var fillPath = new PathF();
        fillPath.MoveTo(points[0].X, dirtyRect.Height - padding);
        foreach (var point in points)
        {
            fillPath.LineTo(point.X, point.Y);
        }

        fillPath.LineTo(points[^1].X, dirtyRect.Height - padding);
        fillPath.Close();

        canvas.FillColor = FillColor;
        canvas.FillPath(fillPath);

        var linePath = new PathF();
        linePath.MoveTo(points[0].X, points[0].Y);
        for (var i = 1; i < points.Length; i++)
        {
            linePath.LineTo(points[i].X, points[i].Y);
        }

        canvas.StrokeColor = LineColor;
        canvas.StrokeSize = 3;
        canvas.DrawPath(linePath);

        canvas.FillColor = LineColor;
        foreach (var point in points)
        {
            canvas.FillCircle(point.X, point.Y, 3.5f);
        }
    }
}
