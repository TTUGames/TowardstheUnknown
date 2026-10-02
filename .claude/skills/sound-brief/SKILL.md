---
name: sound-brief
description: "Builds the sound designer's brief of the placeholder sounds: films each moment of the game where a placeholder plays (Unity Recorder, with the game's own Wwise output), labels the instant each sound plays from the Wwise profiler, and writes a folder with an HTML brief, the videos, the current placeholders and a zip. Use when the placeholder list changes, a sound is replaced, or the user says « brief sons », « vidéos pour le sound designer », « refais le dossier des placeholders », « film the sounds »."
---

# sound-brief

The placeholders are listed in `scripts/brief.py` (`SOUNDS`: event, files under `Originals/SFX`, target length, when, intention, bus; `CLIPS`: the videos). Keep it in step with [audio](../../../docs/tech/audio.md#placeholders): a placeholder added, replaced or removed changes both.

## Steps

1. Open the project in Unity and in Wwise (WAAPI on, port 8090). The CLI drives the editor unfocused: `Rec.Focus` lets the input reach the game anyway, and `Rec.Start` wakes Wwise (it suspends while the editor is not focused). `END` restores both.
2. Film: `.claude/skills/sound-brief/scripts/shots.sh [test_room|sandbox|gallery|game]` (all by default; `SOUND_BRIEF_WORK` sets the work folder, the temp folder by default). Each `STOP` prints the voices that started during the clip, read from the profiler's history: check that the placeholder is there. A new shot is a `REC <name> ... STOP` in `shots.sh` and a line of `CLIPS`.
3. Edit: `python .claude/skills/sound-brief/scripts/compose.py <work> [clip ...]` writes `<work>/out`: the game's sound, the title, a pink label at each placeholder's instant (an `impact` mark for the artifacts), the ambience loop playing (clips 13 and after), a timeline with the marks. Look at a few frames (`ffmpeg -ss <t> -i <clip>.mp4 -frames:v 1`).
4. Build: `python .claude/skills/sound-brief/scripts/make_site.py <work> D:/Downloads/TTU_Sons_Placeholders` writes the folder (`index.html`, `videos/`, `placeholders_actuels/`, `LISEZMOI.txt`) and its zip beside it. Preview it through `python -m http.server` (browsers block `file:` from tools).

## How it works

- `Rec.cs`: the Recorder (MP4 1080p, 30 fps, capped so that the game runs in real time) and the Wwise output capture (`AkUnitySoundEngine.StartOutputCapture`, written to the game's persistent data folder; `lib.sh` copies it, Wwise keeps the file open). A click (`Button_Hover`) on the `StudioSync` game object marks the start and the end: `scan.py` finds both in the profiler's history and gives each voice that started its time from the start. The video starts about 0.15 s after the sound; `compose.py` takes the difference of their lengths as the offset and cuts the clicks.
- `scan.py` reads the profiler's voices every 25 ms over the clip through HTTP WAAPI (`ak.wwise.core.profiler.getVoices` at a time): a voice lasts long enough to be seen, even a 60 ms tick. The WAMP subscription to the capture log hung Wwise's WebSocket server: avoid it.
- `Studio.cs`: a cursor drawn over every panel, and a mouse that glides and clicks (`Mouse_` steps: `tile x z s`, `hud <name|.class> n x s`, `exit DIR s`, `piece n s`, `slot x y s`, `screen fx fy s`, `click`, `down`, `up`, `wait s`). It writes the mouse's whole state with `InputState.Change(..., InputUpdateType.Dynamic)` from the game's frames: events queued from the CLI land in the editor's input buffer while the Game view is unfocused, so `Pointer.*` of `unity-playtest` don't reach the game then. `SpawnOrb` places a relic of a rarity, `EnemyHealth` sets the enemies' health, `Orbs`, `ClearOrbs`, `Tiles`, `Pieces` read the state.
- Wwise writes `ProfilingSession*.prof` in its project while capturing: ignored by git, delete it once Wwise lets it go.
