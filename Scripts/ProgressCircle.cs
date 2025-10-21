using Godot;

namespace BabbleCalibration.Scripts;

public partial class ProgressCircle : PanelContainer
{
    [Export] public float Time = 5;
    [Export] public bool AutoStart;
    
    [ExportGroup("Internal")]
    [Export] private ShaderMaterial _shaderMaterial;
    [Export] public Timer Timer { get; private set; }
    
    public Color CenterColor
    {
        get => _shaderMaterial.GetShaderParameter(CenterColorString).AsColor();
        set => _shaderMaterial.SetShaderParameter(CenterColorString, value);
    }
    private static readonly StringName CenterColorString = "CenterColor";

    private static readonly StringName ProgressParam = "Progress";

    public override void _Ready()
    {
        base._Ready();
        Timer.WaitTime = Time;
        if (AutoStart) Timer.Start();
    }

    public void Start(float time)
    {
        Time = time;
        Timer.WaitTime = Time;
        Timer.Start();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        _shaderMaterial.SetShaderParameter(ProgressParam, 1 - (float)(Timer.TimeLeft / Time));
    }
}