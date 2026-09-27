using UnityEngine;

/// <summary>
/// Shows objects and enables components in one edition only: the additions of the Anniversary (ambience, feedbacks, level art),
/// or what only the Classic has. Put it on an object that stays active; the systems it turns off don't know the edition.
/// Only for what can go without changing how the game plays: a system carrying a step of the game reads <see cref="Edition.Profile"/>
/// </summary>
public class EditionOnly : MonoBehaviour
{
    [SerializeField, Tooltip("The edition they are shown in")] private GameEdition edition;
    [SerializeField, Tooltip("Active in this edition only")] private GameObject[] objects = System.Array.Empty<GameObject>();
    [SerializeField, Tooltip("Enabled in this edition only")] private Behaviour[] behaviours = System.Array.Empty<Behaviour>();
    [SerializeField, Tooltip("Drawn in this edition only: a particle system or a mesh on an object that must stay active")] private Renderer[] renderers = System.Array.Empty<Renderer>();

    private void OnEnable()
    {
        Apply(Edition.Current);
        Edition.Changed += Apply;
    }

    private void OnDisable() => Edition.Changed -= Apply;

    private void Apply(GameEdition current)
    {
        bool shown = current == edition;
        foreach (GameObject shownObject in objects)
            if (shownObject != null) shownObject.SetActive(shown);
        foreach (Behaviour behaviour in behaviours)
            if (behaviour != null) behaviour.enabled = shown;
        foreach (Renderer shownRenderer in renderers)
            if (shownRenderer != null) shownRenderer.enabled = shown;
    }
}
