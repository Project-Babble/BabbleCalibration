using System.Linq;
using BabbleCalibration.Scripts.Elements;
using Godot;

namespace BabbleCalibration.Scripts.Backends;

public partial class OpenXRBackend : Node, IBackend
{
    [Export] public XRCamera3D Camera;
    [Export] public Node ElementRoot;
    private XRInterface _interface;
    public Node Self => this;
    public bool IsOverlay => false;
    public static IBackend Create() => ResourceLoader.Load<PackedScene>("res://Scenes/Backends/OpenXRBackend.tscn").Instantiate<OpenXRBackend>();

    public void Initialize()
    {
        var xrInterface = XRServer.FindInterface("OpenXR");
        if(xrInterface != null && xrInterface.IsInitialized())
        {
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            GetViewport().UseXR = true;
            //GetWindow().Visible = false;
            _interface = xrInterface;
        }
    }

    public void Shutdown()
    {
        if (_interface == null) return;
        if (IsInsideTree()) GetViewport().UseXR = false;
        _interface.Uninitialize();
        _interface = null;
    }

    public ElementBase CreateHeadElement(bool persistent = false)
    {
        var elem = OpenXRElement.CreateElement();
        elem.Head = Camera;
        elem.IsInternal = persistent;
        ElementRoot.AddChild(elem);
        return elem;
    }

    public ElementBase CreateWorldElement(bool persistent = false)
    {
        var elem = OpenXRElement.CreateElement();
        elem.IsInternal = persistent;
        ElementRoot.AddChild(elem);
        return elem;
    }
    public void ClearElements()
    {
        foreach (var c in ElementRoot.GetChildren().OfType<OpenXRElement>().Where(i => !i.IsInternal))
        {
            ElementRoot.RemoveChild(c);
            c.QueueFree();
        }
    }

    public Transform3D HeadTransform() => Camera.GlobalTransform;

    public Transform3D EyeTransform(bool left)
    {
        var head = Camera.GlobalTransform;
        var eyeBall = _interface.GetTransformForView(left ? 1u : 0u, Transform3D.Identity);

        return new Transform3D(head.Basis, eyeBall.Origin);
    }
}