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
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);

        if (args is null) return;
        
        var time = 10f;
            
        Height = backend.HeadTransform().Origin.Y;
            
        Transform = Transform3D.Identity.TranslatedLocal((Vector3.Forward * 2) + (Vector3.Up * Height));
            
        if (args.TryGetValue("time", out var value) && value.VariantType is Variant.Type.Float) 
            time = value.AsSingle();

        (Element, var interf) = this.CreateProgressCircle(time, false, Transform);
            
        MainScene.Instance.TimerEndConnect(interf.Timer);
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        var packet = new HmdPositionalDataPacket();

        var headTransform = Backend.HeadTransform();
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

            return (Mathf.RadToDeg(euler.X), -Mathf.RadToDeg(euler.Y), length);
        }
    }
}