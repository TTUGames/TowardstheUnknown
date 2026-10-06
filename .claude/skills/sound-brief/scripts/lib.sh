# Shooting helpers: source it from the repo root. The raw clips go to $SOUND_BRIEF_WORK/clips (default: the temp folder)
R=$(cygpath -m "$(git rev-parse --show-toplevel)/.claude/skills/sound-brief/scripts")
WORK=$(cygpath -m "${SOUND_BRIEF_WORK:-${TMP:-/tmp}/sound-brief}")
CLIPS="$WORK/clips"
# Where Wwise writes its output capture: the game's persistent data folder
CAPTURES="$(cygpath -u "$USERPROFILE")/AppData/LocalLow/TTU Games/Towards the Unknown"
P=.claude/skills/unity-playtest/scripts/playtest.sh
A=.claude/skills/attack-inspect/scripts
mkdir -p "$CLIPS"

S() { python "$R/u.py" "$R/Studio.cs" "Studio.$1" "${@:2}"; }
# Mouse steps, then waits for them to play (seconds)
M() { S Mouse_ "\"$1\""; sleep "${2:-0}"; }
PT() { "$P" "$@"; }
CAST() { python "$R/u.py" "$(cygpath -m "$PWD")/$A/AttackFilm.cs" AttackFilm.Cast "\"$1\"" 0; }
APPROACH() { python "$R/u.py" "$(cygpath -m "$PWD")/$A/AttackFilm.cs" AttackFilm.Approach "$1"; }

# Starts a scene, ready to film: input to the game, Wwise awake, the cursor drawn
SCENE() {
    "$P" stop >/dev/null 2>&1
    "$P" start "$1" "${2:-15}" >/dev/null
    python "$R/u.py" "$R/Rec.cs" Rec.Focus >/dev/null
    S Cursor true >/dev/null
}

REC() {
    CLIP="$1"
    CLIP_START=$(date +%s)
    python "$R/u.py" "$R/Rec.cs" Rec.Focus >/dev/null
    python "$R/u.py" "$R/Rec.cs" Rec.Start "\"$CLIPS/$CLIP\"" "\"$CLIP.wav\""
}

STOP() {
    python "$R/u.py" "$R/Rec.cs" Rec.Stop
    sleep 1.5
    local window=$(( $(date +%s) - CLIP_START + 8 ))
    sleep 1; cp -f "$CAPTURES/$CLIP.wav" "$CLIPS/$CLIP.wav"
    python "$R/scan.py" "$window" "$CLIPS/$CLIP.json"
}

# A frame of the clip at t seconds, to look at
FRAME() { ffmpeg -v error -y -ss "$2" -i "$CLIPS/$1.mp4" -frames:v 1 -vf scale=960:-1 "$WORK/frame_$1_$2.png"; echo "$WORK/frame_$1_$2.png"; }

WAAPI=.claude/skills/wwise-events/scripts/waapi.py
# Connects the open Wwise to the game in the editor (Play mode) and captures its profiler: scan.py reads its history
BEGIN() {
    python - <<'PY'
import sys
sys.path.insert(0, '.claude/skills/wwise-events/scripts')
from waapi import call
if not call('ak.wwise.core.remote.getConnectionStatus').get('isConnected'):
    game = next(c for c in call('ak.wwise.core.remote.getAvailableConsoles')['consoles'] if 'Editor' in c['appName'])
    call('ak.wwise.core.remote.connect', {'host': game['host'], 'commandPort': game['commandPort']})
call('ak.wwise.core.profiler.startCapture')
print('profiling the editor')
PY
}

# Gives the editor back as it was: the cursor, the input settings, Play mode, the profiler
END() {
    S Cursor false >/dev/null 2>&1
    python "$R/u.py" "$R/Rec.cs" Rec.Restore
    "$P" stop
    python "$WAAPI" ak.wwise.core.profiler.stopCapture >/dev/null
    python "$WAAPI" ak.wwise.core.remote.disconnect >/dev/null
    echo "Wwise may leave TowardstheUnknown_WwiseProject/ProfilingSession*.prof: delete it once Wwise lets it go, never commit it"
}
