using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts.Routines;

public class DilationRoutine : RoutineBase
{
    public override bool PlaySounds => true;
    private GradientTexture1D _texture = new();
    private Gradient _gradient = new();
    private Stopwatch _stopwatch = new();
    private Curve _lerpCurve = new();

    private bool _ended = false;
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        _gradient.SetColors([Colors.Black]);
        _texture.Gradient = _gradient;

        CreatePlane(Vector3.Forward);
        CreatePlane(Vector3.Back);
        CreatePlane(Vector3.Up);
        CreatePlane(Vector3.Down);
        CreatePlane(Vector3.Left);
        CreatePlane(Vector3.Right);
        
        _lerpCurve.MaxDomain = 16;
        
        _lerpCurve.ClearPoints();
        _lerpCurve.AddPoint(Vector2.Zero);
        _lerpCurve.AddPoint(new Vector2(3, 0));
        _lerpCurve.AddPoint(new Vector2(13, 1));
        _lerpCurve.AddPoint(new Vector2(16, 1));
        
        
        _stopwatch.Start();

        return;

        void CreatePlane(Vector3 direction)
        {
            var i = new PanelContainer();
            i.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            i.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            var rect = new TextureRect();
            i.AddChild(rect);
            rect.Texture = _texture;
            rect.StretchMode = TextureRect.StretchModeEnum.Scale;
            rect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;

            var elem = backend.CreateElementWithObject(i, true);

            const float elementSize = 0.5f;
            
            var vec = direction.Normalized();
            var position = vec * elementSize * 0.5f;
            var rotation = new Quaternion(Vector3.Forward, direction);

            var transform = new Transform3D(new Basis(rotation), position);
            
            elem.ElementTransform = transform;
            elem.ElementWidth = elementSize;
            elem.ElementResolution = Vector2I.One * 8;
        }
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        if (!_ended && _stopwatch.Elapsed.TotalSeconds > 16)
        {
            _ended = true;
            MainScene.Instance.SendRoutineEnded(MainScene.Instance.CurrentRoutineName);
        }

        var elapsed = (float)_stopwatch.Elapsed.TotalSeconds;
        var sample = _lerpCurve.Sample(elapsed);
        
        _gradient.SetColors([Colors.Black.Lerp(Colors.White, sample)]);
    }
}
