using BabbleCalibration.Scripts.Elements;
using Godot;

namespace BabbleCalibration.Scripts.Backends;

public partial class OpenXROverlayBackend : Node, IBackend
{
    public Node Self => this;
    public bool IsOverlay => true;
    public static IBackend Create()
    {
        throw new System.NotImplementedException();
    }
    public void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public ElementBase CreateHeadElement(bool persistent = false)
    {
        throw new System.NotImplementedException();
    }

    public ElementBase CreateWorldElement(bool persistent = false)
    {
        throw new System.NotImplementedException();
    }

    public void ClearElements()
    {
        throw new System.NotImplementedException();
    }

    public Transform3D HeadTransform()
    {
        throw new System.NotImplementedException();
    }

    public Transform3D EyeTransform(bool left)
    {
        throw new System.NotImplementedException();
    }
}