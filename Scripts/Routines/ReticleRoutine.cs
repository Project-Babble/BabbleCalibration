using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;
using GodotPlugins.Game;
using OverlaySDK.Packets;

namespace BabbleCalibration.Scripts.Routines;

public class ReticleRoutine : RoutineBase
{
    public override bool PlaySounds => true;
    private Transform3D _transform = Transform3D.Identity;
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        
        if (args is not null)
        {
            var time = 10f;
            var transform = Transform3D.Identity.TranslatedLocal((Vector3.Forward * 2) + (Vector3.Up * backend.HeadTransform().Origin.Y));
            
            if (args.TryGetValue("time", out var value) && value.VariantType is Variant.Type.Float) 
                time = value.AsSingle();
            
            var (element, interf) = this.Load<ProgressCircle>("res://Scenes/Routines/ProgressCircle.tscn");
            _transform = element.ElementTransform = OriginOffset * transform;
            element.ElementWidth = 0.075f;
            interf.Start(time);
            
            MainScene.Instance.TimerEndConnect(interf.Timer);
        }
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        var packet = new HmdPositionalDataPacket();

        var headTransform = Backend.HeadTransform();
        var leftEye = Backend.EyeTransform(true);
        var rightEye = Backend.EyeTransform(false);

        /*
        DebugDraw3D.DrawSphere(headTransform.TranslatedLocal(Vector3.Forward).Origin, 0.5f, Colors.Yellow);
        
        DebugDraw3D.DrawArrow(headTransform.Origin, headTransform.TranslatedLocal(new Vector3(1,0,0)).Origin, Colors.Red);
        DebugDraw3D.DrawArrow(headTransform.Origin, headTransform.TranslatedLocal(new Vector3(0,1,0)).Origin, Colors.GreenYellow);
        DebugDraw3D.DrawArrow(headTransform.Origin, headTransform.TranslatedLocal(new Vector3(0,0,1)).Origin, Colors.Blue);
        */

        (packet.RoutinePitch, packet.RoutineYaw, packet.RoutineDistance) = TransformToReticule(headTransform);
        (packet.RightEyePitch, packet.RightEyeYaw, _) = TransformToReticule(rightEye);
        (packet.LeftEyePitch, packet.LeftEyeYaw, _) = TransformToReticule(leftEye);
        
        //GD.Print($"{new Vector2(packet.LeftEyePitch, packet.LeftEyeYaw)}, {new Vector2(packet.RightEyePitch, packet.RightEyeYaw)}");
        
        MainScene.Instance.SendPacket(packet);

        return;
        
        (float Pitch, float Yaw, float Distance) TransformToReticule(Transform3D transform)
        {
            var angleTo = (transform.AffineInverse() * _transform).Origin;
            var lookAt = Basis.LookingAt(angleTo, Vector3.Up);

            var euler = lookAt.GetRotationQuaternion().GetEuler();

            var length = transform.Origin.DistanceTo(_transform.Origin);

            //var endPoint = ((transform * new Transform3D(Basis.FromEuler(new Vector3(euler.X, euler.Y, 0)), Vector3.Zero)).TranslatedLocal(Vector3.Forward * length)).Origin;
            
            //DebugDraw3D.DrawLine(headTransform.Origin, endPoint);

            return (Mathf.RadToDeg(euler.X), -Mathf.RadToDeg(euler.Y), length);
        }
    }
}