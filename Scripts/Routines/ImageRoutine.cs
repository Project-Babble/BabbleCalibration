using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;

namespace BabbleCalibration.Scripts.Routines;

public class ImageRoutine : RoutineBase
{
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        
        if (args is not null)
        {
            var text = "";
            var head = false;
            var transform = Transform3D.Identity.TranslatedLocal(Vector3.Forward + Vector3.Up);
            var imagePath = "res://Assets/PlaceholderTexture.tres";
            
            if (args.TryGetValue("text", out var value) && value.VariantType is Variant.Type.String) 
                text = value.AsString();
            if (args.TryGetValue("path", out value) && value.VariantType is Variant.Type.String) 
                imagePath = value.AsString();
            if (args.TryGetValue("head", out value) && value.VariantType is Variant.Type.Bool) 
                head = value.AsBool();
            if (args.TryGetValue("transform", out value) && value.VariantType is Variant.Type.Transform3D) 
                transform = value.AsTransform3D();
            
            var (element, interf) = this.Load<TextureRoutineInterface>("res://Scenes/Routines/ImageRoutine.tscn", head);
            if (ResourceLoader.Exists(imagePath)) interf.TextureRect.Texture = ResourceLoader.Load<Texture2D>(imagePath);
            interf.Label.Text = text;
            element.ElementTransform = (head ? Transform3D.Identity : OriginOffset) * transform;
        }
    }
}