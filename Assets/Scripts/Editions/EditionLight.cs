using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// A light the Anniversary changed: the Classic gives it back the original's type, intensity, range, color, cookie, soft shadows and place
/// (its transform, or the one of the object holding it), and the Anniversary gets its own values back
/// </summary>
[RequireComponent(typeof(Light))]
public class EditionLight : MonoBehaviour
{
    [SerializeField, Tooltip("Moved to the original's place in the Classic: this object, or the prefab instance holding the light")] private Transform moved;
    [SerializeField] private LightType classicType = LightType.Point;
    [SerializeField, Min(0)] private float classicIntensity = 1;
    [SerializeField, Min(0)] private float classicRange = 10;
    [SerializeField, Tooltip("Whether the Anniversary changed the light's color")] private bool classicColored;
    [SerializeField, ShowIf(nameof(classicColored))] private Color classicColor = Color.white;
    [SerializeField, Tooltip("The original had none: the Classic clears the cookie")] private bool classicNoCookie = true;
    [SerializeField, Tooltip("The original's light cast soft shadows, which the Anniversary turned off")] private bool classicSoftShadows;
    [SerializeField, Tooltip("Whether the Classic moves the light")] private bool classicPlace;
    [SerializeField, ShowIf(nameof(classicPlace))] private Vector3 classicLocalPosition;
    [SerializeField, ShowIf(nameof(classicPlace))] private Quaternion classicLocalRotation = Quaternion.identity;

    private Light lightSource;
    private LightType type;
    private float intensity, range;
    private Color color;
    private Texture cookie;
    private LightShadows shadows;
    private Vector3 localPosition;
    private Quaternion localRotation;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        if (moved == null) moved = transform;
        type = lightSource.type;
        intensity = lightSource.intensity;
        range = lightSource.range;
        color = lightSource.color;
        cookie = lightSource.cookie;
        shadows = lightSource.shadows;
        localPosition = moved.localPosition;
        localRotation = moved.localRotation;
    }

    private void OnEnable()
    {
        OnEditionChanged(Edition.Current);
        Edition.Changed += OnEditionChanged;
    }

    private void OnDisable() => Edition.Changed -= OnEditionChanged;

    // Again a frame later: a flicker turned off at the same time gives its own intensity back
    private void OnEditionChanged(GameEdition edition)
    {
        Apply(edition);
        if (isActiveAndEnabled) StartCoroutine(ApplyNextFrame(edition));
    }

    private IEnumerator ApplyNextFrame(GameEdition edition)
    {
        yield return null;
        Apply(edition);
    }

    private void Apply(GameEdition edition)
    {
        bool classic = edition == GameEdition.Classic;
        lightSource.type = classic ? classicType : type;
        lightSource.intensity = classic ? classicIntensity : intensity;
        lightSource.range = classic ? classicRange : range;
        lightSource.color = classic && classicColored ? classicColor : color;
        lightSource.cookie = classic && classicNoCookie ? null : cookie;
        lightSource.shadows = classic && classicSoftShadows ? LightShadows.Soft : shadows;
        if (!classicPlace) return;
        moved.SetLocalPositionAndRotation(classic ? classicLocalPosition : localPosition, classic ? classicLocalRotation : localRotation);
    }
}
