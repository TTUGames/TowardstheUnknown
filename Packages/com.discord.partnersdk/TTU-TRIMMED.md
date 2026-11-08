# Discord Social SDK 1.10.19337, trimmed

Downloaded on 18/10/2026 from the Discord Developer Portal (application Towards the Unknown, Social SDK > Downloads,
`DiscordSocialSdk-UnityPlugin-1.10.19337.zip`, 573 MB). The game only uses the rich presence on desktop, so only these
files are kept (48 MB instead of 1.2 GB; the Debug macOS library alone is 173 MB, over GitHub's 100 MB limit):

- `Runtime/Discord.cs` and its asmdef, `Editor/DiscordPluginSelector.cs` and its asmdef, `package.json`, `License-Notices.txt`
- the Release libraries: `Runtime/Plugins/x86_64/Release/discord_partner_sdk.dll` (Windows), `libdiscord_partner_sdk.so` (Linux),
  `Runtime/Plugins/Release/libdiscord_partner_sdk.dylib` (macOS)

Left out: the Debug libraries, Krisp (voice noise suppression: its libraries and models), iOS, Android and its
`DiscordPartnerSdkDependencies.xml`, the `.dbg` symbols.

Two patches to `DiscordPluginSelector.cs`, marked `TTU:`: it uses the Release libraries when the Debug ones are missing
(the editor and development builds), and skips copying the Krisp models when there are none.

One patch to `Runtime/Discord.cs`, marked `TTU:`, for Unity 7 (CoreCLR): `NativeMethods.Keep` keeps alive the delegates of the
106 callbacks the wrapper hands the native SDK, and `ManagedUserData` keeps its free function in a static field. Without it,
CoreCLR frees a collected delegate's native thunk and `Discord_RunCallbacks` crashes the editor (see `docs/tech/unity7.md`).

To update: download the new UnityPlugin zip, copy the same files over (with their `.meta`), keep the three patches (or
check that the new `Discord.cs` keeps its callback delegates alive).
