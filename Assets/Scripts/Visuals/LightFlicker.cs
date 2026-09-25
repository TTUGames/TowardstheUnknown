using UnityEngine;

/// <summary>
/// Makes a flame's light flicker: its intensity wanders around its value with noise, each light at its own pace
/// </summary>
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField, Range(0, 1), Tooltip("Largest change of the intensity, as a fraction of it")] private float amount = 0.3f;
    [SerializeField, Tooltip("Noise speed")] private float speed = 6f;

    private new Light light;
    private float baseIntensity;
    private float seed;

    private void Awake()
    {
        light = GetComponent<Light>();
        baseIntensity = light.intensity;
        seed = Random.value * 100;
    }

    private void Update()
    {
        // Two noises: a slow sway and a quick crackle
        float time = Time.time * speed;
        float noise = Mathf.PerlinNoise(seed, time * 0.35f) * 0.7f + Mathf.PerlinNoise(seed + 50, time) * 0.3f;
        light.intensity = baseIntensity * (1 + (noise * 2 - 1) * amount);
    }

    private void OnDisable()
    {
        if (light != null) light.intensity = baseIntensity;
    }
}
