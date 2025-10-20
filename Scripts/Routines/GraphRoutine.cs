using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BabbleCalibration.Scripts.RoutineInterfaces;
using Godot;
using Godot.Collections;
using OverlaySDK.Packets;

namespace BabbleCalibration.Scripts.Routines;

public partial class GraphRoutine : RoutineBase
{
    private GraphRoutineInterface _interface;
    private readonly Stopwatch _stopwatch = new();
    private readonly List<Vector2> _points = [];

    private int _epochCount = -1;
    private int _batchCount = -1;

    private int _epochCurrent = -1;
    private int _batchCurrent = -1;
    
    public override void Initialize(IBackend backend, Dictionary args = null)
    {
        base.Initialize(backend, args);
        
        _stopwatch.Start();
        (var element, _interface) = this.Load<GraphRoutineInterface>("res://Scenes/Routines/GraphRoutine.tscn", true);
        element.ElementTransform = Transform3D.Identity.TranslatedLocal(Vector3.Forward + (Vector3.Down * 0.25f));
    }

    public void Handle(TrainerProgressReportPacket packet)
    {
        var loss = (float)packet.Loss;
        _interface.LossText.Text = $"Current Loss: {loss:F6}";

        var isEpoch = packet.ProgressName is "Epoch";
        var targetLabel = isEpoch ? _interface.EpochText : _interface.BatchText;
        var targetBar = isEpoch ? _interface.EpochBar : _interface.BatchBar;

        targetLabel.Text = $"{packet.ProgressName}: {packet.CurrentProgress}/{packet.TargetProgress}";
        targetBar.MaxValue = packet.TargetProgress;
        targetBar.Value = packet.CurrentProgress;

        if (isEpoch)
        {
            _epochCount = packet.TargetProgress;
            _epochCurrent = packet.CurrentProgress;
        }
        else
        {
            _batchCount = packet.TargetProgress;
            _batchCurrent = packet.CurrentProgress;
        }
        var currentTime = (float)_stopwatch.Elapsed.TotalSeconds;

        _points.Add(new Vector2(currentTime, loss));
        _interface.Graph.Points = _points.Count > 32 ? _points.TakeLast(32).ToArray() : _points.ToArray();
        _interface.Graph.QueueRedraw();

        if (_epochCount < 0 || _batchCount < 0 || _epochCurrent + _batchCurrent <= 0) return;
        
        var totalBatchCount = _epochCount * _batchCount;
        var completedBatches = (_epochCurrent * _batchCount) + _batchCurrent;

        var progress = (float)completedBatches / totalBatchCount;
        var remaining = 1f - progress;
        var multiplier = remaining / progress;

        var eta = currentTime * multiplier;
        
        if (eta <= 0f) return;
        
        _interface.TimeText.Text = $"ETA: {TimeSpan.FromSeconds(eta):m\\:ss}";
    }
}