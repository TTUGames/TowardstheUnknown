// Built on the SteamManager of Steamworks.NET (public domain)

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

/// <summary>
/// Starts the Steam API once, before the first scene loads, runs its callbacks every frame and shuts it down when the game
/// quits. Without the Steam client the game runs without Steam: <see cref="Initialized"/> stays false and the Steam stats
/// and achievements are skipped
/// </summary>
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    private static SteamManager instance;
    // The Steam API can only be initialized once per process
    private static bool everInitialized;

    private bool initialized;

    /// <summary>
    /// The Steam API is running. Reading it never creates the manager: a call while the game quits or leaves Play mode
    /// would leave a new one in the scene
    /// </summary>
    public static bool Initialized => instance != null && instance.initialized;

    // Play mode starts without a domain reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        everInitialized = false;
    }

    // Once, before the first scene loads, then kept across scenes
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (instance == null) new GameObject(nameof(SteamManager)).AddComponent<SteamManager>();
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        if (everInitialized) throw new System.Exception("Tried to initialize the Steam API twice in one session");

        if (!Packsize.Test()) Debug.LogError("[Steamworks.NET] Packsize test failed: the wrong version of Steamworks.NET runs on this platform", this);
        if (!DllCheck.Test()) Debug.LogError("[Steamworks.NET] DllCheck test failed: one or more of the Steamworks binaries is the wrong version", this);
        try
        {
            if (!SteamAPI.IsSteamRunning())
            {
                Debug.LogWarning("[Steamworks.NET] The Steam client is not running: Steam stats and achievements are disabled", this);
                return;
            }
            initialized = SteamAPI.Init();
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load steam_api: it is likely not in the right place.\n" + e, this);
            return;
        }
        if (!initialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init failed: see Valve's documentation (https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown)", this);
            return;
        }
        everInitialized = true;
    }

    // A script reload in Play mode keeps this instance's fields but resets the Steam API's static state
    private void OnEnable()
    {
        if (instance == null) instance = this;
        if (initialized && !everInitialized) initialized = false;
    }

    private void Update()
    {
        if (initialized) SteamAPI.RunCallbacks();
    }

    // The manager lives until the game quits: the Steam API is shut down here, OnApplicationQuit being too early
    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
        if (initialized) SteamAPI.Shutdown();
    }
#else
    public static bool Initialized => false;
#endif
}
