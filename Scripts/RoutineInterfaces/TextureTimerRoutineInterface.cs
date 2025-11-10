using Godot;

namespace BabbleCalibration.Scripts.RoutineInterfaces;

public partial class TextureTimerRoutineInterface : PanelContainer
{
    [Export] public Label Label;
    [Export] public TextureRect TextureRect;
    [Export] public Timer Timer;
}