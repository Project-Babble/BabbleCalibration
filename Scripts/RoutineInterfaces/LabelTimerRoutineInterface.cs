using Godot;

namespace BabbleCalibration.Scripts.RoutineInterfaces;

public partial class LabelTimerRoutineInterface : PanelContainer
{
    [Export] public Label Label;
    [Export] public Timer Timer;
}
