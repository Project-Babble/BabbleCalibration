using BabbleCalibration.Scripts.Elements;
using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;
using GodotPlugins.Game;
using OverlaySDK.Packets;

namespace BabbleCalibration.Scripts.Routines;

public class ConvergenceRoutine : ReticleRoutine
{
    private float _currentTime;
    protected override bool SpeedCheck => false;
    private static float InOut(float t, float b, float c, float d) => -c / 2 * (Mathf.Cos(Mathf.Pi * t / d) - 1) + b;
    
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        
        if (args is not null) UpdateTransform();
    }

    private void UpdateTransform()
    {
        const float interval = 2;
        var lerp = InOut(Mathf.PingPong(_currentTime, interval) / interval, 0, 1, 1);
        Transform = OriginOffset * Transform3D.Identity.TranslatedLocal((Vector3.Forward * 0.5f).Lerp(Vector3.Forward * 2, lerp) + (Vector3.Up * Height));
        Element.ElementTransform = Transform;
    }

    public override void Update(float delta)
    {
        _currentTime += delta;
        UpdateTransform();
        base.Update(delta);
    }
}