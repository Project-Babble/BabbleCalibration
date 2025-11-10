using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts.Routines;

public class ImageTimerRoutine : RoutineBase
{
    public string Text = "{0}";
    public TextureTimerRoutineInterface Interface;
    private bool _playSounds;
    public override bool PlaySounds => _playSounds;
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        
        if (args is not null)
        {
            var text = "";
            var head = false;
            var transform = Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up);
            var imagePath = "res://Assets/PlaceholderTexture.tres";
            var time = 10f;
            
            if (args.TryGetValue("text", out var value) && value.VariantType is Variant.Type.String) 
                text = value.AsString();
            if (args.TryGetValue("path", out value) && value.VariantType is Variant.Type.String) 
                imagePath = value.AsString();
            if (args.TryGetValue("head", out value) && value.VariantType is Variant.Type.Bool) 
                head = value.AsBool();
            if (args.TryGetValue("transform", out value) && value.VariantType is Variant.Type.Transform3D) 
                transform = value.AsTransform3D();
            if (args.TryGetValue("time", out value) && value.VariantType is Variant.Type.Float)
                time = value.AsSingle();
            if (args.TryGetValue("sounds", out value) && value.VariantType is Variant.Type.Bool)
                _playSounds = value.AsBool();
            
            (var element, Interface) = this.Load<TextureTimerRoutineInterface>("res://Scenes/Routines/ImageTimerRoutine.tscn", head);
            if (ResourceLoader.Exists(imagePath)) Interface.TextureRect.Texture = ResourceLoader.Load<Texture2D>(imagePath);
            Interface.Label.Text = text;
            element.ElementTransform = (head ? Transform3D.Identity : OriginOffset) * transform;
            
            Interface.Timer.WaitTime = time;
            Interface.Timer.Start();

            MainScene.Instance.TimerEndConnect(Interface.Timer);
            
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
        var timerTime = Mathf.FloorToInt(Interface.Timer.TimeLeft).ToString();
        var text = string.Format(Text, timerTime);
        Interface.Label.Text = text;
    }
}