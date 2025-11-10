using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts;

public static class RoutineHelpers
{
    public static Dictionary LabelRoutineArgs(string text, bool head = false, Transform3D? transform = null, bool sounds = false)
    {
        return new Dictionary
        {
            {"text", text },
            {"head", head },
            {"transform", transform ?? Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up) },
            {"sounds", sounds }
        };
    }
    
    public static Dictionary FilePathRoutineArgs(string filePath, string text, bool head = false, Transform3D? transform = null, bool sounds = false)
    {
        return new Dictionary
        {
            {"path", filePath },
            {"text", text },
            {"head", head },
            {"transform", transform ?? Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up) },
            {"sounds", sounds },
        };
    }
    
    public static Dictionary FilePathTimeRoutineArgs(string filePath, string text, float time, bool head = false, Transform3D? transform = null, bool sounds = false)
    {
        return new Dictionary
        {
            {"path", filePath },
            {"text", text },
            {"head", head },
            {"time", time },
            {"transform", transform ?? Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up) },
            {"sounds", sounds },
        };
    }
    
    public static Dictionary LabelTimerRoutineArgs(string text, float time, bool head = false, Transform3D? transform = null, bool sounds = false)
    {
        return new Dictionary
        {
            {"text", text },
            {"head", head },
            {"transform", transform ?? Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up) },
            {"time", time },
            {"sounds", sounds },
        };
    }
    
    public static Dictionary TimeArgs(float time, bool sounds = false)
    {
        return new Dictionary
        {
            {"time", time },
            {"sounds", sounds },
        };
    }
}