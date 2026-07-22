using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts.Routines;

public class TextTimerRoutine : RoutineBase
{
    public string Text = "{0}";
    public LabelTimerRoutineInterface Interface;

    private bool _playSounds;
    private Timer _timer;
    private bool _showProgress;
    private bool _showTestText;
    public override bool PlaySounds => _playSounds;

    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        
        if (args is not null)
        {
            var head = false;
            var transform = Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up);
            var time = 10f;
            var showProgress = false;
            var showTestText = false;
            var winkSide = "";
            
            if (args.TryGetValue("text", out var value) && value.VariantType is Variant.Type.String) 
                Text = value.AsString();
            if (args.TryGetValue("head", out value) && value.VariantType is Variant.Type.Bool) 
                head = value.AsBool();
            if (args.TryGetValue("transform", out value) && value.VariantType is Variant.Type.Transform3D) 
                transform = value.AsTransform3D();
            if (args.TryGetValue("time", out value) && value.VariantType is Variant.Type.Float)
                time = value.AsSingle();
            if (args.TryGetValue("sounds", out value) && value.VariantType is Variant.Type.Bool)
                _playSounds = value.AsBool();
            if (args.TryGetValue("show_progress", out value) && value.VariantType is Variant.Type.Bool)
                showProgress = value.AsBool();
            if (args.TryGetValue("show_test_text", out value) && value.VariantType is Variant.Type.Bool)
                showTestText = value.AsBool();
            if (args.TryGetValue("wink_side", out value) && value.VariantType is Variant.Type.String)
                winkSide = value.AsString();
            _showProgress = showProgress;
            _showTestText = showTestText;
            
            
            (var element, Interface) = this.Load<LabelTimerRoutineInterface>("res://Scenes/Routines/TextTimerRoutine.tscn", head);
            element.ElementTransform = (head ? Transform3D.Identity : OriginOffset) * transform;

            var eyeIcons = Interface.GetNode<HBoxContainer>("Content/EyeIcons");
            var leftEyeIcon = Interface.GetNode<TextureRect>("Content/EyeIcons/LeftEye");
            var rightEyeIcon = Interface.GetNode<TextureRect>("Content/EyeIcons/RightEye");
            eyeIcons.Visible = !string.IsNullOrEmpty(winkSide);
            if (eyeIcons.Visible)
            {
                var openEye = ResourceLoader.Load<Texture2D>("res://Assets/Icons/eye-open.svg");
                var closedEye = ResourceLoader.Load<Texture2D>("res://Assets/Icons/eye-closed.svg");
                leftEyeIcon.Texture = winkSide == "left" ? closedEye : openEye;
                rightEyeIcon.Texture = winkSide == "right" ? closedEye : openEye;
            }

            if (showProgress)
            {
                // Reuse the persistent world-fixed target. Only its ring restarts for this test.
                var progressCircle = MainScene.Instance.StartMeasurementProgress(time);
                _timer = progressCircle.Timer;
                Interface.Label.Text = "";
            }
            else
            {
                // Tutorials and pauses intentionally only show their text countdown.
                Interface.Timer.WaitTime = time;
                Interface.Timer.Start();
                _timer = Interface.Timer;
            }

            if (!showProgress)
                MainScene.Instance.TimerEndConnect(_timer);
            
            UpdateText();
        }
    }

    public override void Update(float delta)
    {
        base.Update(delta);
        UpdateText();
    }

    private void UpdateText()
    {
        if (_timer == null) return;
        if (_showProgress)
        {
            Interface.Label.Text = _showTestText
                ? string.Format(Text, Mathf.CeilToInt(_timer.TimeLeft))
                : "";
            return;
        }
        var timerTime = Mathf.CeilToInt(_timer.TimeLeft).ToString();
        var text = string.Format(Text, timerTime);
        Interface.Label.Text = text;
    }
}
