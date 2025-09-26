using System;
using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts.Routines;

public class PupilRoutine : RoutineBase
{
    private PupilRoutineInterface _interface;

    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        (var tutorial, _interface) = this.Load<PupilRoutineInterface>("res://Scenes/Routines/PupilRoutine.tscn");
        tutorial.ElementTransform = Transform3D.Identity.TranslatedLocal((Vector3.Forward * 2) + Vector3.Up);
    }

    public override void Update(float delta)
    {
        base.Update(delta);
        _interface.Label.Text = Random.Shared.Next().ToString();

        var secondsElapsed = Time.GetTicksMsec() / 1000f;
        var progress = secondsElapsed % 10f;
        _interface.Gradient.SetColor(0, Colors.White.Lerp(Colors.Black, progress / 10f));
    }
}
