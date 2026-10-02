using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder.Encoder;
using UnityEngine;

/// <summary>
/// Records the Game view (Unity Recorder, MP4 1080p 30 fps) and the Wwise output (output capture, WAV) together.
/// The controller lives in the AppDomain's data between the calls (each run_script compiles a new assembly)
/// </summary>
public static class Rec
{
    const string Key = "ttu.rec.controller";

    /// <summary>
    /// Focuses the Game view: the Input System gives the game the pointer only while it has the focus
    /// </summary>
    public static string Focus()
    {
        var gameView = UnityEditor.EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView,UnityEditor"));
        gameView.Focus();
        // The editor is driven from outside, unfocused: the input goes to the game anyway (restored by Restore)
        var settings = UnityEngine.InputSystem.InputSystem.settings;
        if (System.AppDomain.CurrentDomain.GetData("ttu.input") == null)
            System.AppDomain.CurrentDomain.SetData("ttu.input", new object[] { settings.backgroundBehavior, settings.editorInputBehaviorInPlayMode });
        settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        AkUnitySoundEngine.WakeupFromSuspend();
        return "focused " + UnityEditor.EditorWindow.focusedWindow?.titleContent.text;
    }

    public static string Restore()
    {
        if (!(System.AppDomain.CurrentDomain.GetData("ttu.input") is object[] saved)) return "nothing to restore";
        var settings = UnityEngine.InputSystem.InputSystem.settings;
        settings.backgroundBehavior = (UnityEngine.InputSystem.InputSettings.BackgroundBehavior)saved[0];
        settings.editorInputBehaviorInPlayMode = (UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode)saved[1];
        System.AppDomain.CurrentDomain.SetData("ttu.input", null);
        return $"restored {settings.backgroundBehavior} {settings.editorInputBehaviorInPlayMode}";
    }

    public static string Start(string videoPathNoExt, string wavPath)
    {
        if (System.AppDomain.CurrentDomain.GetData(Key) is RecorderController old && old.IsRecording()) old.StopRecording();
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movie.name = "ttu";
        movie.Enabled = true;
        movie.EncoderSettings = new CoreEncoderSettings
        {
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
            Codec = CoreEncoderSettings.OutputCodec.MP4,
        };
        movie.CaptureAudio = false;
        movie.ImageInputSettings = new GameViewInputSettings { OutputWidth = 1920, OutputHeight = 1080 };
        movie.OutputFile = videoPathNoExt;
        settings.AddRecorderSettings(movie);
        settings.SetRecordModeToManual();
        settings.FrameRate = 30;
        settings.FrameRatePlayback = FrameRatePlayback.Constant;
        settings.CapFrameRate = true;
        var controller = new RecorderController(settings);
        controller.PrepareRecording();
        bool ok = controller.StartRecording();
        System.AppDomain.CurrentDomain.SetData(Key, controller);
        // Wwise suspends its rendering while the editor is not focused (the CLI drives it from outside)
        AkUnitySoundEngine.WakeupFromSuspend();
        AkUnitySoundEngine.StartOutputCapture(wavPath);
        AkUnitySoundEngine.PostEvent("Button_Hover", Marker());
        System.AppDomain.CurrentDomain.SetData("ttu.rec.frame", Time.frameCount);
        return $"recording={ok} frame={Time.frameCount} wall={(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds:0.000}";
    }

    // The sync marks: a short click on their own game object, found in the profiler's history (scan.py) and in the capture
    static GameObject Marker()
    {
        GameObject marker = GameObject.Find("StudioSync");
        if (marker == null) Object.DontDestroyOnLoad(marker = new GameObject("StudioSync"));
        return marker;
    }

    public static string Stop()
    {
        AkUnitySoundEngine.PostEvent("Button_Hover", Marker());
        AkUnitySoundEngine.StopOutputCapture();
        if (!(System.AppDomain.CurrentDomain.GetData(Key) is RecorderController controller)) return "no recording";
        int start = (int)System.AppDomain.CurrentDomain.GetData("ttu.rec.frame");
        controller.StopRecording();
        System.AppDomain.CurrentDomain.SetData(Key, null);
        return $"stopped frames={Time.frameCount - start} wall={(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds:0.000}";
    }
}
