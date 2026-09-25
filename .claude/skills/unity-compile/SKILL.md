---
name: unity-compile
description: "Compiles the Unity project in the open editor through the `unity` CLI and reports the C# errors: imports the new and changed files, recompiles, waits for the result and exits 1 on failure. Use after any change to a .cs file, before a playtest or a commit, or when the user says « compile », « ça compile ? », « vérifie les erreurs », « recompile Unity », « check the build errors »."
---

# unity-compile

Run from the repo root:

```bash
.claude/skills/unity-compile/scripts/compile.sh
```

- Output: `completed` or `up_to_date`, followed by `FAILED` and one line per error (`path(line,col): error CSxxxx: message`). Exit code 0 compiled, 1 failed, 2 editor not reachable.
- `up_to_date` right after a change is normal: the asset refresh already compiled it.
- Exit 2: the editor is closed or busy (domain reload, modal dialog). Ask the user to open the project in Unity; NEVER fall back to `dotnet build` as proof of a working build, its `.csproj` is stale after files are added or moved.

On failure, fix the errors and run the script again until it passes. Compiling does not validate the scene and prefab wiring: follow with the `unity-playtest` skill for gameplay changes.
