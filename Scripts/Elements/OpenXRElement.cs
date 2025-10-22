using Godot;

namespace BabbleCalibration.Scripts.Elements;

public partial class OpenXRElement : ElementBase
{
    [Export] public SubViewport Viewport;
    [Export] public MeshInstance3D Mesh;
    [Export] public PlaneMesh PlaneMesh;
    [Export] public Node3D Head;
    [Export] public bool IsInternal = false;

    public override Transform3D ElementTransform
    {
        get => _transform;
        set => _transform = value;
    }

    private Transform3D _transform;

    public override Vector2I ElementResolution
    {
        get => Viewport.Size;
        set
        {
            Viewport.Size = value;
            UpdatePlaneSize(PlaneMesh.Size.X);
        }
    }

    public override float ElementWidth
    {
        get => PlaneMesh.Size.X;
        set => UpdatePlaneSize(value);
    }

    private void UpdatePlaneSize(float width)
    {
        var res = ElementResolution;
        var resolutionRatio = (float)res.Y / res.X;

        PlaneMesh.Size = new Vector2(width, resolutionRatio * width);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Mesh.GlobalTransform = (Head?.GlobalTransform ?? Transform3D.Identity) * _transform;
    }
    
    public static OpenXRElement CreateElement() => ResourceLoader.Load<PackedScene>("res://Scenes/Elements/OpenXRElement.tscn").Instantiate<OpenXRElement>();
}