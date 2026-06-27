using BabbleCalibration.Scripts.Routines;
using Godot;
using OverlaySDK;
using OverlaySDK.Packets;

namespace BabbleCalibration.Scripts;

public class GodotPacketHandler : PacketHandlerAdapter
{
    public OverlayMessageDispatcher Dispatcher { get; private set; }

    public GodotPacketHandler(OverlayMessageDispatcher dispatcher)
    {
        Dispatcher = dispatcher;
        Dispatcher.RegisterHandler(this);
    }
    
    public override void OnStartRoutine(RunVariableLenghtRoutinePacket routine)
    {
        var name = routine.RoutineName;
        var time = routine.Time;
        
        Callable.From(() =>
        {
            var routineToDo = name.ToLower().Trim();
            MainScene.Instance.StartRoutine(routineToDo, (float)time.TotalSeconds);
        }).CallDeferred();
    }

    public override void OnStartRoutine(RunFixedLenghtRoutinePacket routine)
    {
        base.OnStartRoutine(routine);

        var name = routine.RoutineName;
        
        Callable.From(() =>
        {
            var routineToDo = name.ToLower().Trim();
            MainScene.Instance.StartRoutine(routineToDo);
        }).CallDeferred();
    }

    public override void OnTrainerProgressReport(TrainerProgressReportPacket progressReport)
    {
        base.OnTrainerProgressReport(progressReport);
        
        Callable.From(() =>
        {
            var main = MainScene.Instance;
            if (main.CurrentRoutine is GraphRoutine graph) graph.Handle(progressReport);
        }).CallDeferred();
    }

    public override void OnTermination()
    {
        base.OnTermination();
        Callable.From(() => MainScene.Instance.Shutdown()).CallDeferred();
    }
}