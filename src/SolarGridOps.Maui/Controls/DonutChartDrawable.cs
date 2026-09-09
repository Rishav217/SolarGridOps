using Microsoft.Maui.Graphics;

namespace SolarGridOps.Maui.Controls;

public class DonutSegment
{
    public float Value { get; set; }
    public Color Color { get; set; } = Colors.Gray;
}

public class DonutChartDrawable : IDrawable
{
    public IList<DonutSegment> Segments { get; set; } = new List<DonutSegment>();
    public float StrokeWidth { get; set; } = 18f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var total = Segments.Sum(s => s.Value);
        if (total <= 0 || Segments.Count == 0)
        {
            return;
        }

        var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - StrokeWidth;
        var cx = dirtyRect.Width / 2f;
        var cy = dirtyRect.Height / 2f;
        var radius = size / 2f;

        var startAngle = 0f;
        canvas.StrokeSize = StrokeWidth;
        canvas.StrokeLineCap = LineCap.Butt;

        foreach (var segment in Segments)
        {
            var sweep = segment.Value / total * 360f;
            canvas.StrokeColor = segment.Color;
            canvas.DrawArc(cx - radius, cy - radius, cx + radius, cy + radius, startAngle, startAngle + sweep, true, false);
            startAngle += sweep;
        }
    }
}
