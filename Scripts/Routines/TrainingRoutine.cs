using System;
using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts.Routines;

public class TrainingRoutine : RoutineBase
{
    private LabelRoutineInterface _interface;

    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        (var tutorial, _interface) = this.Load<LabelRoutineInterface>("res://Scenes/Routines/TrainingRoutine.tscn");
        tutorial.ElementTransform = Transform3D.Identity.TranslatedLocal((Vector3.Forward * 2) + Vector3.Up);
    }

    public override void Update(float delta)
    {
        base.Update(delta);
        _interface.Label.Text = Random.Shared.Next().ToString();
    }
}