using System.Linq;
using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts;

[Tool]
public partial class GraphControl : Control
{
    //x = left, y = right, z = top, w = bottom
    [Export]
    public Vector4 Margins= Vector4I.One * 8;

    [Export] public Vector2[] Points = [];
    [Export(PropertyHint.Range, "1,10,0.1")] public Vector2 GridLineSpacing = Vector2.One;
    [Export] public Color GridColor = Colors.DimGray;
    [Export] public Color LineColor = Colors.DarkGray;
    [Export] public Color SideColor = Colors.White;

    [Export] public bool ShowHorizontalText = true;
    [Export] public bool ShowVerticalText = true;
    [Export(PropertyHint.Range, "0, 10")] public Vector2I TextInterval = Vector2I.Zero;

    [Export]
    public bool Normalize
    {
        get;
        set
        {
            field = value;
            NotifyPropertyListChanged();
            QueueRedraw();
        }
    }

    [Export]
    public Vector2 RangeMinMax
    {
        get;
        set
        {
            field = value;
            QueueRedraw();
        }
    } = new(0, 1);
    
    [ExportToolButton("Redraw")] 
    private Callable RedrawButton => Callable.From(QueueRedraw);

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);
        if (property["name"].AsStringName() == PropertyName.RangeMinMax && Normalize)
            property["usage"] = (int)(PropertyUsageFlags.Default & ~PropertyUsageFlags.Editor);
    }

    public override void _Draw()
    {
        base._Draw();

        var size = Size;
        
        var left = Margins.X;
        var right = size.X - Margins.Y;
        var top = Margins.Z;
        var bottom = size.Y - Margins.W;

        if (Points is not null && Points.Length >= 2)
        {
            var pointMinPosition = Points.Select(i => i.X).Min();
            var pointMaxPosition = Points.Select(i => i.X).Max();
        
            var pointMinValue = Normalize ? Points.Select(i => i.Y).Min() : RangeMinMax.X;
            var pointMaxValue = Normalize ? Points.Select(i => i.Y).Max() : RangeMinMax.Y;

            var interval = 0;

            var xmax = pointMaxPosition + GridLineSpacing.X * 0.001f;
            for (var i = Mathf.Snapped(pointMinPosition, GridLineSpacing.X); i <= xmax; i += GridLineSpacing.X)
            {
                var x = Mathf.Remap(i, pointMinPosition, pointMaxPosition, left, right);
                DrawLine(new Vector2(x, bottom), new Vector2(x, top), GridColor);
                if (interval == 0)
                {
                    DrawString(ThemeDB.FallbackFont, new Vector2(x - 16, ((bottom + size.Y) * 0.5f) + 4), i.ToString(), HorizontalAlignment.Center, 32);
                    interval = TextInterval.X;
                }
                else interval--;
            }

            interval = 0;
        
            var ymax = pointMaxValue + GridLineSpacing.Y * 0.001f;
            for (var i = Mathf.Snapped(pointMinValue, GridLineSpacing.Y); i <= ymax; i += GridLineSpacing.Y)
            {
                var y = Mathf.Remap(i, pointMinValue, pointMaxValue, bottom, top);
                DrawLine(new Vector2(left, y), new Vector2(right, y), GridColor);
                if (interval == 0)
                {
                    DrawString(ThemeDB.FallbackFont, new Vector2(0, y), i.ToString(), HorizontalAlignment.Right, left - 4);
                    interval = TextInterval.Y;
                }
                else interval--;
            }
        
            DrawPolyline(Points.Select(i => new Vector2(
                Mathf.Remap(i.X, pointMinPosition, pointMaxPosition, left, right), 
                Mathf.Remap(i.Y, pointMinValue, pointMaxValue, bottom, top))
            ).ToArray(), LineColor, 3);
        }
        
        DrawLine(new Vector2(left, top), new Vector2(left, bottom), SideColor, 4);
        DrawLine(new Vector2(left, bottom), new Vector2(right, bottom), SideColor, 4);
    }
}