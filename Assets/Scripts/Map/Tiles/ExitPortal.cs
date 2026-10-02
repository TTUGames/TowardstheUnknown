using System.Collections;
using UnityEngine;

/// <summary>
/// The VFX of an open exit (RoomExit.prefab): opens with a reveal (the rim springs out, a flash, a band of light rising)
/// and closes by playing it backwards, through the <c>_Reveal</c> of its renderers. The exits of a room open in a wave
/// leaving the player. Its look tells where the exit leads (<see cref="ExitDestination"/>): a color, and how much light
/// rises from it. It brightens and draws ripples in while the pointer is on its exit (<c>_Hover</c>)
/// </summary>
public class ExitPortal : MonoBehaviour
{
    [System.Serializable]
    private struct Look
    {
        [ColorUsage(false, true)] public Color color;
        [Tooltip("The streaks' brightness (_Brightness)")] public float streaks;
        [Tooltip("The motes floating up (_Motes)")] public float motes;
    }

    [SerializeField] private Renderer floor;
    [SerializeField] private Renderer streaks;
    [Header("Looks, by destination")]
    // The hues of the minimap's rooms (--color-room-* of Common.uss), in HDR
    [SerializeField] private Look visited = new Look { color = new Color(0.85f, 0.88f, 0.95f), streaks = 0.5f, motes = 0 };
    [SerializeField] private Look combat = new Look { color = new Color(0.35f, 0.6f, 2f), streaks = 1.2f, motes = 0.5f };
    [SerializeField] private Look treasure = new Look { color = new Color(1.1f, 0.72f, 0.14f), streaks = 1.2f, motes = 1.5f };
    [SerializeField, Min(0.01f), Tooltip("Seconds the hover highlight takes to come and go")] private float hoverDuration = 0.15f;
    [Header("Sounds")]
    [SerializeField, Tooltip("Once for all the exits opening together")] private AK.Wwise.Event openSound = new AK.Wwise.Event();
    [SerializeField, Tooltip("Once for all the exits closing together")] private AK.Wwise.Event closeSound = new AK.Wwise.Event();
    [SerializeField, Tooltip("When the pointer comes on the exit")] private AK.Wwise.Event hoverSound = new AK.Wwise.Event();
    [Header("Reveal")]
    [SerializeField] private float revealDuration = 1.1f;
    [SerializeField] private float closeDuration = 0.35f;
    [SerializeField, Tooltip("The delay of the reveal per meter from the player")] private float delayPerMeter = 0.05f;
    [SerializeField] private float maxDelay = 0.4f;

    private static readonly int RevealID = Shader.PropertyToID("_Reveal");
    // The frames of the last opening and closing sounds: the exits of a room open and close together, heard once
    private static int openSoundFrame = -1;
    private static int closeSoundFrame = -1;
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int BrightnessID = Shader.PropertyToID("_Brightness");
    private static readonly int MotesID = Shader.PropertyToID("_Motes");
    private static readonly int HoverID = Shader.PropertyToID("_Hover");

    private Renderer[] renderers;
    private MaterialPropertyBlock block;
    private Coroutine playing;
    private bool open;
    private float hover;
    private bool hovered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        openSoundFrame = -1;
        closeSoundFrame = -1;
    }

    private void Awake() {
        renderers = GetComponentsInChildren<Renderer>(true);
        block = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Takes the look of what the exit leads to
    /// </summary>
    public void SetDestination(ExitDestination destination) {
        Look look = destination switch {
            ExitDestination.VISITED => visited,
            ExitDestination.TREASURE => treasure,
            _ => combat,
        };
        floor.GetPropertyBlock(block);
        block.SetColor(ColorID, look.color);
        floor.SetPropertyBlock(block);
        streaks.GetPropertyBlock(block);
        block.SetColor(ColorID, look.color);
        block.SetFloat(BrightnessID, look.streaks);
        block.SetFloat(MotesID, look.motes);
        streaks.SetPropertyBlock(block);
    }

    private void OnDisable() {
        // Left with its room: hidden until it opens again
        playing = null;
        SetReveal(0);
        hovered = false;
        hover = 0;
        SetFloat(HoverID, 0);
    }

    /// <summary>
    /// Brightens the portal and draws ripples into it while the pointer is on its exit
    /// </summary>
    public void SetHovered(bool hovered) {
        if (hovered && !this.hovered && open) hoverSound.Post(SoundHolder);
        this.hovered = hovered;
    }

    // The exit's tile, which stays active while the portal closes and deactivates: the sound plays out
    private GameObject SoundHolder => transform.parent != null ? transform.parent.gameObject : gameObject;

    private void PostOnce(AK.Wwise.Event sound, ref int frame) {
        if (frame == Time.frameCount) return;
        frame = Time.frameCount;
        sound.Post(SoundHolder);
    }

    private void Update() {
        float target = hovered && open ? 1 : 0;
        if (hover == target) return;
        hover = Mathf.MoveTowards(hover, target, Time.deltaTime / hoverDuration);
        SetFloat(HoverID, hover);
    }

    /// <summary>
    /// Plays the reveal (the object activated) or the closing (deactivated at its end)
    /// </summary>
    public void SetOpen(bool open) {
        bool shown = gameObject.activeInHierarchy && (playing != null || CurrentReveal() >= 1);
        if (open && this.open && shown) return;
        this.open = open;
        if (open) gameObject.SetActive(true);
        else if (!gameObject.activeInHierarchy) {
            gameObject.SetActive(false);
            return;
        }
        if (!gameObject.activeInHierarchy) return;
        if (open) PostOnce(openSound, ref openSoundFrame);
        else PostOnce(closeSound, ref closeSoundFrame);
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(open ? Reveal() : Close());
    }

    private IEnumerator Reveal() {
        SetReveal(0);
        Transform player = GameScene.Player != null ? GameScene.Player.transform : null;
        float delay = player != null ? Mathf.Min(Vector3.Distance(player.position, transform.position) * delayPerMeter, maxDelay) : 0;
        if (delay > 0) yield return new WaitForSeconds(delay);
        for (float time = 0; time < revealDuration; time += Time.deltaTime) {
            SetReveal(time / revealDuration);
            yield return null;
        }
        SetReveal(1);
        playing = null;
    }

    private IEnumerator Close() {
        float from = CurrentReveal();
        for (float time = 0; time < closeDuration; time += Time.deltaTime) {
            SetReveal(from * (1 - time / closeDuration));
            yield return null;
        }
        SetReveal(0);
        playing = null;
        gameObject.SetActive(false);
    }

    private float CurrentReveal() {
        if (renderers.Length == 0) return 0;
        renderers[0].GetPropertyBlock(block);
        return block.isEmpty ? 1 : block.GetFloat(RevealID);
    }

    private void SetReveal(float reveal) => SetFloat(RevealID, reveal);

    private void SetFloat(int id, float value) {
        foreach (Renderer renderer in renderers) {
            renderer.GetPropertyBlock(block);
            block.SetFloat(id, value);
            renderer.SetPropertyBlock(block);
        }
    }
}
