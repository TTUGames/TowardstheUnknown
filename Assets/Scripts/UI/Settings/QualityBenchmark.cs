using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Picks the graphics quality that suits the machine: tries the presets from High down, measuring the frame rate each
/// gives for a moment, and keeps the first one that holds the target (60 frames a second, or the screen's rate or the cap if
/// lower, with a margin); if even Low doesn't, it lowers the render scale as well. Run from the options, so the game is
/// drawn behind the menu as it is measured
/// </summary>
public class QualityBenchmark : MonoBehaviour
{
    /// <summary>
    /// The preset and the render scale (in percent) kept, with the frame rate they reached
    /// </summary>
    public readonly struct Result
    {
        public readonly int preset;
        public readonly float renderScale;
        public readonly float fps;

        public Result(int preset, float renderScale, float fps)
        {
            this.preset = preset;
            this.renderScale = renderScale;
            this.fps = fps;
        }
    }

    // Real seconds the new settings settle (shaders compile, the frames steady), then the frames are measured
    private const float Settle = 0.7f;
    private const float Measure = 1.5f;
    // The share of the target the measured rate must reach
    private const float Margin = 0.9f;
    private const float TargetFps = 60;
    private static readonly float[] renderScales = { 100, 85, 75, 67, 50 };

    public static bool Running { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Running = false;

    /// <param name="progress">Called with the preset about to be measured, or -1 with the render scale for the scales</param>
    /// <param name="done">Called with the settings kept, which are applied and saved</param>
    public static void Run(Action<int, float> progress, Action<Result> done)
    {
        if (Running) return;
        Running = true;
        new GameObject(nameof(QualityBenchmark)).AddComponent<QualityBenchmark>().Begin(progress, done);
    }

    private void Begin(Action<int, float> progress, Action<Result> done) => StartCoroutine(Search(progress, done));

    private IEnumerator Search(Action<int, float> progress, Action<Result> done)
    {
        float target = Target() * Margin;
        float fps = 0;
        int chosen = GraphicsQuality.Low;
        GameSettings.Set(GameSetting.RenderScale, 100);
        foreach (int preset in new[] { GraphicsQuality.High, GraphicsQuality.Medium, GraphicsQuality.Low })
        {
            GameSettings.Set(GameSetting.Quality, preset);
            progress?.Invoke(preset, 100);
            yield return Measured(rate => fps = rate);
            chosen = preset;
            if (fps >= target) break;
        }
        float scale = 100;
        if (fps < target)
            for (int i = 1; i < renderScales.Length; i++)
            {
                scale = renderScales[i];
                GameSettings.Set(GameSetting.RenderScale, scale);
                progress?.Invoke(-1, scale);
                yield return Measured(rate => fps = rate);
                if (fps >= target) break;
            }
        Running = false;
        done?.Invoke(new Result(chosen, scale, fps));
        Destroy(gameObject);
    }

    // The frame rate over a moment, once the settings settled: the slowest twentieth of the frames left out, the hitches
    // of a shader compiling or a room loading not counting
    private static IEnumerator Measured(Action<float> rate)
    {
        yield return new WaitForSecondsRealtime(Settle);
        var frames = new List<float>();
        for (float time = 0; time < Measure; time += Time.unscaledDeltaTime)
        {
            yield return null;
            frames.Add(Time.unscaledDeltaTime);
        }
        frames.Sort();
        int count = Mathf.Max(1, frames.Count - frames.Count / 20);
        float sum = 0;
        for (int i = 0; i < count; i++) sum += frames[i];
        rate(count / Mathf.Max(sum, 0.0001f));
    }

    // 60, or less where the sync or the cap holds the game
    private static float Target()
    {
        int refresh = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        float limit = QualitySettings.vSyncCount > 0 && refresh > 0 ? refresh / (float)QualitySettings.vSyncCount
            : Application.targetFrameRate > 0 ? Application.targetFrameRate
            : refresh > 0 ? refresh : TargetFps;
        return Mathf.Min(TargetFps, limit);
    }
}
