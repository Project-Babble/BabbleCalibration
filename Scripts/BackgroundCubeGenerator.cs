using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BabbleCalibration.Scripts;

[Tool, GlobalClass]
public partial class BackgroundCubeGenerator : MultiMesh
{
    [ExportToolButton("Generate")]
    public Callable GenerateButton => Callable.From(Generate);
    public void Generate()
    {
        var mesh = new BoxMesh();
        mesh.Size = Vector3.One * 2;
        Mesh = mesh;

        var list = new List<Transform3D>();

        for (var i = 0; i < 7; i++) GenerateCube(25, 35, 5.5f, 7);
        for (var i = 0; i < 30; i++) GenerateCube(8, 15, 0.6f, 0.8f);
        for (var i = 0; i < 18; i++) GenerateCube(15, 24, 1.6f, 2.6f);

        InstanceCount = 0;
        TransformFormat = TransformFormatEnum.Transform3D;
        InstanceCount = list.Count;
        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            SetInstanceTransform(i, item);
        }
        
        return;
        
        void GenerateCube(float minRadius, float maxRadius, float minScale, float maxScale)
        {
            const float Highest = 16;
            
            var radius = Random(minRadius, maxRadius);
            var scale = Random(minScale, maxScale);
            
            var minHeight = scale * 0.75f;
            var maxHeight = Highest - minHeight;
            
            var randomQuaternion = new Quaternion(GD.Randf(), GD.Randf(), GD.Randf(), GD.Randf()).Normalized();

            for (var i = 0; i < 64; i++)
            {
                var angle = Random(0, Mathf.Tau);
                var height = Random(minHeight, maxHeight);
                var position = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);

                var ourSize = scale * 0.875f;
                if (list.Any(transform3D =>
                        transform3D.Origin.DistanceTo(position) < (2 * transform3D.Basis.Scale.X * 0.875f) + ourSize)) 
                    continue;
                
                var transform = new Transform3D(new Basis(randomQuaternion).ScaledLocal(Vector3.One * scale), position);
                list.Add(transform);
                return;
            }
        }
        float Random(float min, float max) => Mathf.Remap(GD.Randf(), 0, 1, min, max);
    }
}