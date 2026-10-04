using UnityEngine;

/// <summary>
/// Drareg's garden (the antechamber and the boss room) has no snow: there, the snowfall stops, glowing pollen floats instead
/// and the snow on the ground melts away (<c>_SnowSuppressed</c>, read by the snow of Snow Lit and Nature Lit). Elsewhere the
/// snow falls again. Anniversary only: the Classic's snow falls everywhere, as the original's
/// </summary>
public class GardenWeather : MonoBehaviour
{
    private static readonly int SuppressedId = Shader.PropertyToID("_SnowSuppressed");

    [SerializeField, Tooltip("The snowfalls stopped in the garden")] private ParticleSystem[] snowfalls = System.Array.Empty<ParticleSystem>();
    [SerializeField, Tooltip("Floating in the garden instead, made under this object")] private GameObject pollenPrefab;

    private GameObject pollen;

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
        Apply();
        //Turned back on with the edition, whose EditionOnly plays the snowfall again after: stop it once more
        StartCoroutine(ApplyNextFrame());
    }

    private System.Collections.IEnumerator ApplyNextFrame()
    {
        yield return null;
        Apply();
    }

    private void Apply()
    {
        Map map = GameScene.Map;
        if (map != null && map.CurrentRoom != null) OnRoomEntered(map.CurrentRoom, false);
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        Shader.SetGlobalFloat(SuppressedId, 0);
        if (pollen != null) pollen.SetActive(false);
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        bool garden = room.type is RoomType.ANTECHAMBER or RoomType.BOSS;
        Shader.SetGlobalFloat(SuppressedId, garden ? 1 : 0);
        foreach (ParticleSystem snowfall in snowfalls)
        {
            if (snowfall == null) continue;
            if (garden) snowfall.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else if (!snowfall.isPlaying) snowfall.Play(true);
        }
        if (garden && pollen == null && pollenPrefab != null) pollen = Instantiate(pollenPrefab, transform, false);
        if (pollen == null) return;
        pollen.SetActive(garden);
        //Its system may have been stopped while hidden (the Classic stops the weather's)
        if (garden && pollen.TryGetComponent(out ParticleSystem pollenSystem) && !pollenSystem.isPlaying) pollenSystem.Play(true);
    }
}
