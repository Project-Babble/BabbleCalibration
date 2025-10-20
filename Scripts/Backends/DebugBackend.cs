using BabbleCalibration.Scripts.Elements;
using Godot;

namespace BabbleCalibration.Scripts.Backends;

public partial class DebugBackend : Node, IBackend
{
    [Export] public Camera3D Camera;
    [Export] public Node ElementRoot;
    private Vector2 _currentRotation;
    private Vector2 _previousMouseMovement;
    private Window _window;
    private Vector2 _mouseMovement;

    private Vector2 CameraMovement => _mouseMovement * Mathf.Pi * 5;
    
    public Node Self => this;
    public bool IsOverlay => false;
    public static IBackend Create() => ResourceLoader.Load<PackedScene>("res://Scenes/Backends/DebugBackend.tscn").Instantiate<DebugBackend>();

    public void Initialize()
    {
        _window = GetViewport().GetWindow();
    }
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseMotion motion) _mouseMovement += -(motion.ScreenRelative / _window.Size.Y);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _mouseMovement -= _previousMouseMovement;
        _previousMouseMovement = _mouseMovement;
        _currentRotation += CameraMovement;

        _currentRotation = _currentRotation with { Y = Mathf.Clamp(_currentRotation.Y, -Mathf.Pi / 2, Mathf.Pi / 2) };

        Camera.Transform = new Transform3D(Basis.FromEuler(new Vector3(_currentRotation.Y, _currentRotation.X, 0)),
            (Vector3.Up * 1.8f));
    }

    public ElementBase CreateHeadElement()
    {
        var elem = OpenXRElement.CreateElement();
        elem.Head = Camera;
        ElementRoot.AddChild(elem);
        return elem;
    }

    public ElementBase CreateWorldElement()
    {
        var elem = OpenXRElement.CreateElement();
        ElementRoot.AddChild(elem);
        return elem;
    }

    public void ClearElements() => BackendHelpers.ClearAllChildren(ElementRoot);
    public Transform3D HeadTransform() => Camera.GlobalTransform;
    public Transform3D EyeTransform(bool left) => Camera.GlobalTransform.TranslatedLocal((left ? Vector3.Left : Vector3.Right) * 0.035f);
}