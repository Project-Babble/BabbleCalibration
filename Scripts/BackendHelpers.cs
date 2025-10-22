using BabbleCalibration.Scripts.Elements;
using Godot;

namespace BabbleCalibration.Scripts;

public static class BackendHelpers
{
    public static ElementBase CreateElementWithObject(this IBackend backend, Control obj, bool head = false, bool persistent = false)
    {
        var elem = head ? backend.CreateHeadElement(persistent) : backend.CreateWorldElement(persistent);
        elem.Root.AddChild(obj);
        return elem;
    }
    
    public static (ElementBase Element, T Interface) Load<T>(this RoutineBase rout, string path, bool head = false) where T : Control
    {
        var interf = RoutineBase.LoadScene<T>(path);
        var elem = rout.Backend.CreateElementWithObject(interf, head);
        return (elem, interf);
    }
    
    public static (ElementBase Element, ProgressCircle Interface) CreateProgressCircle(this RoutineBase rout, float time, bool head = false, Transform3D? transform = null)
    {
        var interf = RoutineBase.LoadScene<ProgressCircle>("res://Scenes/Routines/ProgressCircle.tscn");
        /*
        interf.Time = time;
        interf.AutoStart = true;
        */
        var elem = rout.Backend.CreateElementWithObject(interf, head);
        elem.ElementTransform = transform ?? (head ? Transform3D.Identity.TranslatedLocal(Vector3.Forward) : Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up));
        elem.ElementWidth = 0.075f;
        interf.Start(time);
        return (elem, interf);
    }

    public static void ClearAllChildren(Node node)
    {
        if (node is null) return;
        foreach (var c in node.GetChildren())
        {
            node.RemoveChild(c);
            c.QueueFree();
        }
    }
}