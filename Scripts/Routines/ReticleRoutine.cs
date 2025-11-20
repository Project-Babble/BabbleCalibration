using System.Collections.Generic;
using BabbleCalibration.Scripts.Elements;
using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;
using GodotPlugins.Game;
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
    protected Quaternion PreviousHeadRotation = Quaternion.Identity;
    protected float Speed;

    protected virtual bool SpeedCheck => true;
    private bool _tooFast;
    
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
            
        MainScene.Instance.TimerEndConnect(Interface.Timer);
    }
    private static float Damp(float a, float b, float lambda, float dt) => Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    private static readonly StringName SlowDownString = "SlowDown";
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
                return;
            }
            if (_tooFast)
            {
                Text.Label.Text = "";
                Interface.CenterColor = Colors.White;
                _tooFast = false;
            }
        }
        var leftEye = Backend.EyeTransform(true);
        var rightEye = Backend.EyeTransform(false);

        (packet.RoutinePitch, packet.RoutineYaw, packet.RoutineDistance) = TransformToReticule(headTransform);
        (packet.RightEyePitch, packet.RightEyeYaw, _) = TransformToReticule(rightEye);
        (packet.LeftEyePitch, packet.LeftEyeYaw, _) = TransformToReticule(leftEye);

        MainScene.Instance.SendPacket(packet);

        return;
        
        (float Pitch, float Yaw, float Distance) TransformToReticule(Transform3D transform)
        {
            var angleTo = (transform.AffineInverse() * Transform).Origin;
            var lookAt = Basis.LookingAt(angleTo, Vector3.Up);

            var euler = lookAt.GetRotationQuaternion().GetEuler();

            var length = transform.Origin.DistanceTo(Transform.Origin);

            return (-Mathf.RadToDeg(euler.X), Mathf.RadToDeg(euler.Y), length);
        }
    }
}