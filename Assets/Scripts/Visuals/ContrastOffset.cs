using UnityEngine;

/// <summary>
/// Adds a contrast to the player's contrast setting while this object is active: a place's volume (the antechamber's) can't
/// override the contrast, the settings' volume is above it, so it adds its own to the setting's
/// </summary>
public class ContrastOffset : MonoBehaviour
{
    [SerializeField, Range(-50, 50), Tooltip("The contrast added to the setting's while the object is active")]
    private float contrast = 12;

    private void OnEnable()
    {
        GameSettings.AddContrastOffset(contrast);
    }

    private void OnDisable()
    {
        GameSettings.AddContrastOffset(-contrast);
    }
}
