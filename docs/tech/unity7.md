# Unity 7

The project runs on the Unity 7.0 alpha (`7000.0.0a16`) since 08/11/2026: a CoreCLR editor (.NET 10, C# 14, no domain reload), the player still built with IL2CPP. The alphas ship every week; each one goes through the `unity-upgrade` skill on its own branch. The last Unity 6.6 commit is tagged `last-unity-6.6`.

## Patches marked `TTU:`

Vendored code that broke under Unity 7 carries a patch marked `TTU:`, neutral in Unity 6. An update of the vendor drops it: re-apply it, or check that the new release handles Unity 7.

| Code | Broke | Patch |
|---|---|---|
| `Packages/com.discord.partnersdk/Runtime/Discord.cs` | The editor crashed on entering Play mode, in `Discord_RunCallbacks`: the generated wrapper hands the native SDK 106 callbacks as local delegates, and CoreCLR frees a collected delegate's native thunk (Mono kept them forever) | `NativeMethods.Keep` caches each callback delegate, and `ManagedUserData` keeps its free function in a static field |
| `Packages/com.rlabrecque.steamworks.net/Runtime/autogen/NativeMethods.cs`, `ISteamMatchmakingResponses.cs` | Unity 7 no longer defines `UNITY_64`: the player looked for the 32-bit `steam_api` and Steam never initialized | `UNITY_7000_0_OR_NEWER` counts as 64-bit |
| `Packages/com.rlabrecque.steamworks.net/Runtime/types/SteamDatagramTickets/SteamDatagramRelayAuthTicket.cs` | The editor logged `Broken assembly … com.rlabrecque.steamworks.net.dll`: `ExtraField.OptionValue` overlapped a `byte[]` with a `long`, which CoreCLR refuses to load (Mono allowed it) | Backport of Steamworks.NET `af82828c` (#735): a 128-byte `FixedBytes128` struct instead of the array; drop it when updating Steamworks.NET past 2025 |
| `Assets/Plugins/vFolders`, `vHierarchy`, `vInspector`, `vTabs` | Their `Init` threw: `EditorApplication.globalEventHandler` is an `Action<Event>` in Unity 7; vHierarchy also read `Event.s_Current`, moved to `EventIMGUIBindings` in Unity 6.7 | An `Action<Event>` adapter under `UNITY_7000_0_OR_NEWER`; vHierarchy reads `EventIMGUIBindings` like the other v* plugins |
| `Assets/Rendering/Mist.shader`, `RiftVolumetrics.shader`, `Water.shader` (game code) | Magenta: URP 17.7's `Lighting.hlsl` no longer includes `Core.hlsl` (`VertexPositionInputs` unknown) | They include `Core.hlsl` first |

Hot Reload works in Unity 7 (its `UAL0010` warnings ask for static cleanup attributes, harmless). Wwise, Odin, the Addressables and the IL2CPP build needed nothing.

## Measured against Unity 6.6

Same commit, same machine (Ryzen 9 9900X, RX 6900 XT), 08/11/2026, alpha 16. The Windows build: −6 % main thread CPU per frame, −24 % native memory, 1 GC collection instead of 11 over three combat replays, start to menu 4.95 s instead of 4.81 s; the GPU, which bounds the frame in combat (about 14 ms), barely moves. In the editor: +17 % FPS over the room gallery, a gameplay recompile until the editor is ready 6.3 s instead of 5.4 s, a Hot Reload patch 0.6 s instead of 0.3 s, a cold import 326 s instead of 334 s.
