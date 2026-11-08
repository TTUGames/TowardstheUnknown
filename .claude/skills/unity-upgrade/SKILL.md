---
name: unity-upgrade
description: "Moves the project to a new Unity editor version, typically the next Unity 7 alpha: installs it, opens the project once in batch mode on a branch chore/unity-<version>, fails on C# or shader errors, lists what the upgrade re-serialized, then checks the game (replay, log, build) before the commit. Use when the user says « mets à jour l'alpha », « nouvelle alpha de Unity », « passe à la 7000.0.0aN », « upgrade Unity », « update the Unity version »."
---

# unity-upgrade

The project follows the Unity 7 alphas (weekly builds). Each one goes through this skill, on the Windows machine that builds the game (the Corsair), editor closed:

```powershell
pwsh .claude/skills/unity-upgrade/scripts/upgrade.ps1 -Version 7000.0.0a17
```

1. The script refuses a working tree with changes, installs the version with Windows IL2CPP if it is missing (`unity install`), closes the open editor, creates `chore/unity-<version>`, deletes `Library` (`-KeepLibrary` keeps it) and opens the project once in batch mode. It exits 1 on a C# error or a shader error (the log is `Logs/upgrade-<version>.log`), else prints the files the upgrade re-serialized by folder and the packages that moved.
2. Open the editor on the new version and check the game for real:
   - replay a saved combat (`unity-playtest`: `Replay.Start` on a journal, then `Replay.Status`): every state identical, as on the previous version;
   - read the console (`playtest.sh errors`): no exception from the patched vendored code below, no shader error;
   - build (`unity-build`) and launch the player: Steam initializes (`Player.log`: no `DllNotFoundException` for `steam_api`).
3. Commit `chore(deps): upgrade the project to Unity <version>` with what broke and how it was fixed, update the version in `CLAUDE.md`, `README.md`, `docs/README.md` and [Unity 7](../../../docs/tech/unity7.md), then open the PR to `anniversary-edition`.

Going back: `git switch anniversary-edition` and delete the branch; a version already merged is undone by reverting its commit. The last Unity 6.6 commit is tagged `last-unity-6.6` (its assets don't open in Unity 7's format the other way round).

## Patches to keep

Vendored code carries Unity 7 patches marked `TTU:`. An update of the vendor (Asset Store, Discord portal, Steamworks.NET release) drops them: re-apply them, or check that the new release handles Unity 7, before committing it. The list is in [Unity 7](../../../docs/tech/unity7.md).
