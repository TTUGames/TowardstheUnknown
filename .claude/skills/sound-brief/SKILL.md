---
name: sound-brief
description: "Builds a brief for someone outside the code (a sound designer, an artist): films each moment of the game it is about (Unity Recorder, with the game's own Wwise output), labels the instant each sound plays from the Wwise profiler, and writes a folder with an HTML page, the labelled videos, the files to replace and a zip. Made for the placeholder sounds; reused for any other brief of the same kind. Use when the placeholder list changes, a sound is replaced, or the user says « brief sons », « vidéos pour le sound designer », « refais le dossier des placeholders », « fais un dossier comme celui des sons », « film the sounds »."
---

# sound-brief

The concept: a brief that shows rather than tells. Each item (a sound to make) gets its moment in the game filmed for real, with the game's own sound, a label on screen at the exact instant it plays, and a page listing the items (what, when, intention, length, the file to replace, a button opening the video at that instant), all in one folder and its zip, ready to hand over.

The data is a Python module: `scripts/brief.py` for the placeholder sounds (`SOUNDS`: key = the Wwise event, French name, files under `Originals/SFX`, length, when, intention, bus, `loop`, `impact`, `voices`; `CLIPS`: name, title, what it shows; `BY_KEY`, `key_of(voice)` mapping a Wwise voice to an item). The brief may leave out placeholders not meant for the person (on 2026-10-02 the user left out the three artifacts HitBuff, OrbitalShot and CriticalShot: no attack sounds for the sound designer for now); the full list stays in [audio](../../../docs/tech/audio.md#placeholders).

## Steps

1. Open the project in Unity and in Wwise (WAAPI on, port 8090). The CLI drives the editor unfocused: `Rec.Focus` lets the input reach the game anyway and `Rec.Start` wakes Wwise (it suspends while the editor is unfocused); `END` restores both.
2. Film: `.claude/skills/sound-brief/scripts/shots.sh [test_room|sandbox|gallery|game]` (all by default; `SOUND_BRIEF_WORK` sets the work folder, the temp folder by default). Each `STOP` prints the voices that started during the clip, read from the profiler's history: check that the item is there. A shot is a `REC <name> ... STOP` in `shots.sh` and a line of `CLIPS`.
3. Edit: `python .claude/skills/sound-brief/scripts/compose.py <work> [clip ...]` writes `<work>/out`: the game's sound, the title, a pink label at each item's instant (an `IMPACT` mark after `impact` seconds), the ambience loop playing (clips 13 and after), a timeline with the marks. Look at a few frames (`ffmpeg -ss <t> -i <clip>.mp4 -frames:v 1`).
4. Build: `python .claude/skills/sound-brief/scripts/make_site.py <work> D:/Downloads/TTU_Sons_Placeholders` replaces the folder's content in place (it may be open in an explorer) and rewrites the zip beside it. Only the clips of `CLIPS` go in, so removing an item or a clip from the data and rebuilding is enough: no need to film again. Preview through `python -m http.server` (browsers block `file:` from tools).

## Another brief of the same kind

Copy `brief.py` to a new module with the same names, point `SOUND_BRIEF_DATA` to it for `compose.py` and `make_site.py`, write its shots with the `lib.sh` helpers (`SCENE`, `BEGIN`, `REC`, `M` mouse steps, `PT` playtest probes, `CAST`, `APPROACH`, `STOP`, `END`), and adapt the page's texts in `make_site.py` (delivery, mix) when the brief is not about sounds. An item is found in the clips through the Wwise voices it plays (`key_of`); for something without a sound, add its instants to the clip's JSON by hand (`events`: `t`, `sound` = the item's key).

## How it works

- `Rec.cs`: the Recorder (MP4 1080p, 30 fps, capped so that the game runs in real time) and the Wwise output capture (`AkUnitySoundEngine.StartOutputCapture`, written to the game's persistent data folder; `lib.sh` copies it, Wwise keeps the file open). A click (`Button_Hover`) on the `StudioSync` game object marks the start and the end: `scan.py` finds both in the profiler's history and gives each voice that started its time from the start. The video starts about 0.15 s after the sound; `compose.py` takes the difference of their lengths as the offset and cuts the clicks.
- `scan.py` reads the profiler's voices every 25 ms over the clip through HTTP WAAPI (`ak.wwise.core.profiler.getVoices` at a time), so that even a 60 ms tick is seen, and the voices playing at the start (`active`). The WAMP subscription to the capture log hung Wwise's WebSocket server: avoid it.
- `Studio.cs`: a cursor drawn over every panel, and a mouse that glides and clicks (`Mouse_` steps: `tile x z s`, `hud <name|.class> n x s`, `exit DIR s`, `piece n s`, `slot x y s`, `screen fx fy s`, `click`, `down`, `up`, `wait s`). It writes the mouse's whole state with `InputState.Change(..., InputUpdateType.Dynamic)` from the game's frames: events queued from the CLI land in the editor's input buffer while the Game view is unfocused. `SpawnOrb` places a relic of a rarity, `EnemyHealth` sets the enemies' health, `Orbs`, `ClearOrbs`, `Tiles`, `Pieces` read the state.
- Wwise writes `ProfilingSession*.prof` in its project while capturing: ignored by git, delete it once Wwise lets it go.
