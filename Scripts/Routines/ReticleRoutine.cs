using BabbleCalibration.Scripts.Elements;
using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;
using OverlaySDK.Packets;

namespace BabbleCalibration.Scripts.Routines;

public class ReticleRoutine : RoutineBase
{
    public override bool PlaySounds => true;
    protected float Height;
    protected Transform3D Transform = Transform3D.Identity;
    protected ElementBase Element;
    protected ProgressCircle Interface;

    protected LabelRoutineInterface Text;
    protected LabelRoutineInterface Instruction;
    private LabelRoutineInterface _debugHud;
    protected Quaternion PreviousHeadRotation = Quaternion.Identity;
    protected float Speed;

    protected virtual bool SpeedCheck => true;
    private bool _tooFast;
    private bool _tooSlow;
    private float _slowTimer;
    
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);

        if (args is null) return;
        
        var time = 10f;
            
        Height = backend.HeadTransform().Origin.Y;
            
        Transform = Transform3D.Identity.TranslatedLocal((Vector3.Forward * 2) + (Vector3.Up * Height));
            
        if (args.TryGetValue("time", out var value) && value.VariantType is Variant.Type.Float) 
            time = value.AsSingle();

        (Element, Interface) = this.CreateProgressCircle(time, false, Transform);

        (var textElem, Text) = this.Load<LabelRoutineInterface>("res://Scenes/Routines/TextRoutine.tscn", true);
        textElem.ElementTransform = Transform3D.Identity.TranslatedLocal(Vector3.Forward);
        Text.Label.Text = "";

        // Optional persistent expression prompt (e.g. "Squint and follow the dot"), shown on its own
        // label below the speed-warning label so the SlowDown/SpeedUp warnings don't overwrite it.
        if (args.TryGetValue("text", out var instructionValue) &&
            instructionValue.VariantType is Variant.Type.String)
        {
            var instruction = instructionValue.AsString();
            if (!string.IsNullOrEmpty(instruction))
            {
                (var instrElem, Instruction) = this.Load<LabelRoutineInterface>("res://Scenes/Routines/TextRoutine.tscn", true);
                instrElem.ElementTransform = Transform3D.Identity.TranslatedLocal(Vector3.Forward + (Vector3.Down * 0.35f));
                Instruction.Label.Text = instruction;
            }
        }

        // Head-locked live packet readout, opt-in via --debug-hud.
        if (MainScene.ShowDebugHud)
        {
            (var hudElem, _debugHud) = this.Load<LabelRoutineInterface>("res://Scenes/Routines/TextRoutine.tscn", true);
            hudElem.ElementTransform = Transform3D.Identity.TranslatedLocal(Vector3.Forward + (Vector3.Down * 0.55f));
            hudElem.ElementWidth = 0.6f;
            _debugHud.Label.Text = "";
        }

        MainScene.Instance.TimerEndConnect(Interface.Timer);
    }
    private static float Damp(float a, float b, float lambda, float dt) => Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    private static readonly StringName SlowDownString = "SlowDown";
    private static readonly StringName SpeedUpString = "SpeedUp";
    public override void Update(float delta)
    {
        base.Update(delta);

        var packet = new HmdPositionalDataPacket();

        var headTransform = Backend.HeadTransform();

        if (SpeedCheck)
        {
            var currentRotation = headTransform.Basis.GetRotationQuaternion();
            var rotationDelta = PreviousHeadRotation.Inverse() * currentRotation;
            var diff = rotationDelta.AngleTo(Quaternion.Identity);
            var currentSpeed = diff / delta;
            PreviousHeadRotation = currentRotation;
            Speed = Damp(Speed, currentSpeed, 2, delta);
        
            if (Speed > 2)
            {
                if (_tooFast) return;
                Text.Label.Text = TranslationServer.Translate(SlowDownString);
                Interface.CenterColor = Colors.Red;
                _tooFast = true;
                _tooSlow = false;
                _slowTimer = 0f;
                return;
            }
            if (_tooFast)
            {
                Text.Label.Text = "";
                Interface.CenterColor = Colors.White;
                _tooFast = false;
            }
            
            if (Speed < 0.1f)
            {
                _slowTimer += delta;
                
                if (_slowTimer >= 2) // seconds
                {
                    if (!_tooSlow)
                    {
                        Text.Label.Text = TranslationServer.Translate(SpeedUpString);
                        Interface.CenterColor = Colors.Red;
                        _tooSlow = true;
                    }
                    return;
                }
            }
            else
            {
                _slowTimer = 0f;
                if (_tooSlow)
                {
                    Text.Label.Text = "";
                    Interface.CenterColor = Colors.White;
                    _tooSlow = false;
                }
            }
        }
        var leftEye = Backend.EyeTransform(true);
        var rightEye = Backend.EyeTransform(false);

        (packet.RoutinePitch, packet.RoutineYaw, packet.RoutineDistance) = TransformToReticule(headTransform);
        (packet.RightEyePitch, packet.RightEyeYaw, _) = TransformToReticule(rightEye);
        (packet.LeftEyePitch, packet.LeftEyeYaw, _) = TransformToReticule(leftEye);

        MainScene.Instance.SendPacket(packet);

        if (_debugHud != null)
        {
            var pos = headTransform.Origin;
            _debugHud.Label.Text =
                $"HEAD P={packet.RoutinePitch,6:F1} Y={packet.RoutineYaw,6:F1} D={packet.RoutineDistance,5:F2}\n" +
                $"L    P={packet.LeftEyePitch,6:F1} Y={packet.LeftEyeYaw,6:F1}\n" +
                $"R    P={packet.RightEyePitch,6:F1} Y={packet.RightEyeYaw,6:F1}\n" +
                $"conv(R-L)={packet.RightEyeYaw - packet.LeftEyeYaw,5:F2}  spd={Speed,4:F1}\n" +
                $"pos=({pos.X,5:F2},{pos.Y,5:F2},{pos.Z,5:F2})  ref: P~0 Y~0 D~2  L_Y~-1 R_Y~+1";
        }

        return;
        
        (float Pitch, float Yaw, float Distance) TransformToReticule(Transform3D transform)
        {
            var angleTo = (transform.AffineInverse() * Transform).Origin;
            var lookAt = Basis.LookingAt(angleTo, Vector3.Up);

            var euler = lookAt.GetRotationQuaternion().GetEuler();

            var length = transform.Origin.DistanceTo(Transform.Origin);
            return (Mathf.RadToDeg(euler.X), Mathf.RadToDeg(euler.Y), length);
        }
    }
}