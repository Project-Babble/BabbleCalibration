using Godot;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BabbleCalibration.Scripts;
using BabbleCalibration.Scripts.Backends;
using BabbleCalibration.Scripts.Elements;
using BabbleCalibration.Scripts.Routines;
using Godot.Collections;
using OverlaySDK;
using OverlaySDK.Packets;
using Timer = Godot.Timer;

public partial class MainScene : Node
{
    [Export] private AudioStreamPlayer _audioPlayer;
    public static MainScene Instance { get; private set; }
    public IBackend Backend { get; private set; }
    public RoutineBase CurrentRoutine { get; private set; }
    public string CurrentRoutineName { get; private set; }

    public GodotPacketHandler PacketHandler { get; private set; }

    private bool _sendPackets = true;

    private Socket TryConnect(int retries = 5)
    {
        var reconnectCounter = 0;
        while (true)
        {
            try
            {
                return new SocketFactory().CreateClient("127.0.0.1", 2425);
            }
            catch
            {
                reconnectCounter++;
                Thread.Sleep(500);
                if (reconnectCounter > retries) return null;
            }
        }
    }
    
    public Transform3D OriginOffset { get; private set; } = Transform3D.Identity;
    
    public override void _Ready()
    {
        base._Ready();

        Instance = this;
        
        var args = OS.GetCmdlineArgs();
        var argsLower = args.Select(i => i.ToLowerInvariant().Trim()).ToArray();
        
        var enableXr = false;
        var enableXrOverlay = false;
        var enableOpenVr = false;
        var enableDebug = false;
        var enableTestRoutines = false;
        
        var xrInterface = XRServer.FindInterface("OpenXR");
        if (xrInterface != null && xrInterface.IsInitialized()) enableXr = true;

        var os = OS.GetName();
        var device = OS.GetModelName().ToLower();
        if (os == "Android")
        {
            enableXr = true;
            if (device.Contains("pico")) enableXrOverlay = true; //only monado and pico support overlays
        }
        else
        {
            foreach (var item in argsLower)
            {
                if (item == "--use-openvr") enableOpenVr = true;
                else if (item == "--use-debug") enableDebug = true;
                else if (item == "--use-openxr-overlay") enableXrOverlay = true;
                else if (item == "--test-routines") enableTestRoutines = true;
            }
        }
        
        if (!enableOpenVr && !enableXr && !enableDebug) throw new Exception("Invalid configuration, no backend provided");
        if (enableOpenVr && (enableXr || enableXrOverlay)) throw new Exception("Invalid configuration, OpenXR cannot be enabled at the same time as OpenVR");
        if (enableXrOverlay && !enableXr) throw new Exception("Invalid configuration, OpenXR must be enabled to use OpenXR Overlay");

        if (enableTestRoutines)
        {
            var test = new TestClient();
            
            PacketHandler = new GodotPacketHandler
            (
                new OverlayMessageDispatcher
                (
                    new GodotLogger(), 
                    test
                )
            );

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                //test.SendPacket(new RunFixedLenghtRoutinePacket("dilation"));
                //test.SendPacket(new RunFixedLenghtRoutinePacket("gazetutorial"));
                test.SendPacket(new RunVariableLenghtRoutinePacket("debug1", TimeSpan.FromSeconds(30)));
                //await Task.Delay(5000);
                //test.SendPacket(new RunVariableLenghtRoutinePacket("convergence", TimeSpan.FromSeconds(5)));
                //test.SendPacket(new RunFixedLenghtRoutinePacket("debug"));
                //test.SendPacket(new RunVariableLenghtRoutinePacket("convergencetutorial", TimeSpan.FromSeconds(5)));
                //await Task.Delay(5000);
                //test.SendPacket(new RunVariableLenghtRoutinePacket("convergence", TimeSpan.FromSeconds(20)));
                //await Task.Delay(20000);
            });
        }
        else
        {
            var sock = TryConnect();
            if (sock == null)
            {
                GD.Print("Could not connect to Baballonia");
                GetTree().Quit(-1);
                return;
            }
            try
            {
                PacketHandler = new GodotPacketHandler
                (
                    new OverlayMessageDispatcher
                    (
                        new GodotLogger(), 
                        new EventDrivenJsonClient
                        (
                            new EventDrivenTcpClient(sock)
                        )
                    )
                );
            }
            catch
            {
                _sendPackets = false;
            }
        }

        if (enableOpenVr) Backend = OpenVRBackend.Create();
        else if (enableDebug) Backend = DebugBackend.Create();
        else Backend = enableXrOverlay ? OpenXROverlayBackend.Create() : OpenXRBackend.Create();
        
        AddChild(Backend.Self);
        
        Backend.Initialize();
        
        var elem = Backend.CreateElementWithObject(ResourceLoader.Load<PackedScene>("res://Scenes/Routines/FloorIndicator.tscn").Instantiate<PanelContainer>(), persistent: true);
        elem.ElementTransform = OriginOffset * new Transform3D(new Basis(new Quaternion(Vector3.Forward, Vector3.Down)), Vector3.Up * 0.001f);
        elem.ElementWidth = 1.25f;
        
        if (elem is OpenXRElement openXrElement) GD.Print(openXrElement.IsInternal);
        
        StartRoutine<TextRoutine>(RoutineHelpers.LabelRoutineArgs(Tr(ConnectingString), true, Transform3D.Identity.TranslatedLocal(Vector3.Forward)));

        /*Task.Run(async () =>
        {
            await Task.Delay(50);
            var headTransform = Backend.HeadTransform();
            var position = headTransform.Origin with { Y = 0 };
            var projected = ((headTransform.Basis.GetRotationQuaternion() * Vector3.Forward) with { Y = 0 }).Normalized();
            var newQuaternion = new Quaternion(Vector3.Forward, projected);

            OriginOffset = new Transform3D(new Basis(newQuaternion), position);
        });*/
    }
    private static readonly StringName ConnectingString = "Connecting";
    public void SendPacket<T>(T packet) where T : IPacket
    {
        if (!_sendPackets) return;
        PacketHandler.Dispatcher.Dispatch(packet);
    }

    private static readonly AudioStream StartSound = ResourceLoader.Load<AudioStream>("res://Assets/drop_002.ogg");
    private static readonly AudioStream EndSound = ResourceLoader.Load<AudioStream>("res://Assets/confirmation_001.ogg");
    
    private static readonly StringName GazeTutorialString = "GazeTutorial";
    private static readonly StringName GazeTutorialShortString = "GazeTutorialShort";
    
    private static readonly StringName BlinkTutorialString = "BlinkTutorial";
    private static readonly StringName BlinkRoutineString = "BlinkRoutine";
    
    private static readonly StringName DilationTutorialString = "DilationTutorial";
    
    private static readonly StringName WidenTutorialString = "WidenTutorial";
    private static readonly StringName WidenRoutineString = "WidenRoutine";
    
    private static readonly StringName ConvergenceTutorialString = "ConvergenceTutorial";
    
    private static readonly StringName SquintTutorialString = "SquintTutorial";
    private static readonly StringName SquintRoutineString = "SquintRoutine";
    
    private static readonly StringName BrowTutorialString = "BrowTutorial";
    private static readonly StringName BrowRoutineString = "BrowRoutine";

    private static readonly System.Collections.Generic.Dictionary<string, (StringName text, bool sounds)> TextTimerRoutines = new()
    {
        { "gazetutorialshort", (GazeTutorialShortString, false) },
        { "blinktutorial", (BlinkTutorialString, false) },
        { "blink", (BlinkRoutineString, true) },
        { "dilationtutorial", (DilationTutorialString, false) },
        { "widentutorial", (WidenTutorialString, false) },
        { "widen", (WidenRoutineString, true) },
        { "squinttutorial", (SquintTutorialString, false) },
        { "squint", (SquintRoutineString, true) },
        { "browtutorial", (BrowTutorialString, false) },
        { "brow", (BrowRoutineString, true) },
        { "convergencetutorial", (ConvergenceTutorialString, false) },
    };
    public void StartRoutine(string name, float time = 0)
    {
        if (TextTimerRoutines.TryGetValue(name, out var info))
        {
            StartRoutine<TextTimerRoutine>(RoutineHelpers.LabelTimerRoutineArgs(Tr(info.text),
                time, true, Transform3D.Identity.TranslatedLocal(Vector3.Forward), info.sounds));
            CurrentRoutineName = name;
            return;
        }
        switch (name)
        {
            case "gazetutorial":
                StartRoutine<VideoRoutine>(RoutineHelpers.FilePathRoutineArgs("res://Assets/BabbleCalibration.ogv",
                    Tr(GazeTutorialString), true,
                    Transform3D.Identity.TranslatedLocal(Vector3.Forward)));
                CurrentRoutine.CreateProgressCircle(9999999999, false,
                    Transform3D.Identity.TranslatedLocal(Vector3.Forward * 2 +
                                                         (Vector3.Up * Backend.HeadTransform().Origin.Y)));
                break;
            case "gaze":
                StartRoutine<ReticleRoutine>(RoutineHelpers.TimeArgs(time));
                break;
            case "dilation":
                StartRoutine<DilationRoutine>();
                break;
            case "convergence":
                StartRoutine<ConvergenceRoutine>(RoutineHelpers.TimeArgs(time));
                break;
            case "trainer":
                StartRoutine<GraphRoutine>();
                break;
            case "close":
                GetTree().Quit();
                break;
            case "debug":
                StartRoutine<DebugRoutine>();
                break;
            case "debug1":
                StartRoutine<ImageTimerRoutine>(RoutineHelpers.FilePathTimeRoutineArgs("blah", "blah {0}", time, sounds: true));
                break;
        }
        
        CurrentRoutineName = name;
    }
    public void TimerEndConnect(Timer timer) => timer.Connect(Timer.SignalName.Timeout, Callable.From(SendRoutineEnded));
    public void SendRoutineEnded() => SendPacket(new RoutineFinishedPacket(CurrentRoutineName));
    public void PlayStartSound() => PlaySound(StartSound);
    public void PlayEndSound() => PlaySound(EndSound);
    private void PlaySound(AudioStream stream)
    {
        _audioPlayer.Stop();
        _audioPlayer.Stream = stream;
        _audioPlayer.Play();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var deltaf = (float)delta;
        CurrentRoutine?.Update(deltaf);
    }

    public void StartRoutine<T>(Dictionary args = null) where T : RoutineBase, new()
    {
        CurrentRoutine?.End();
        Backend.ClearElements();
        CurrentRoutine = new T();
        CurrentRoutine.Initialize(Backend, args);
    }
}
