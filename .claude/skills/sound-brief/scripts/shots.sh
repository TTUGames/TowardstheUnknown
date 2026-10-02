#!/bin/bash
# The shots of the sound brief, as filmed on 2026-10-02: run from the repo root with the editor and Wwise open.
#   .claude/skills/sound-brief/scripts/shots.sh [shot ...]   (all of them by default; a shot is a function name below)
# Each REC ... STOP pair writes <work>/clips/<name>.mp4, .wav (the Wwise capture) and .json (the voices that started, from
# the profiler). Check each STOP's list: the placeholder must be in it. Then compose.py and make_site.py.
cd "$(git rev-parse --show-toplevel)" || exit 2
source .claude/skills/sound-brief/scripts/lib.sh

test_room() {
    SCENE Assets/Scenes/Tests/RoomTestScene.unity 15
    BEGIN
    # Deploy phase: hover the deploy tiles, pick one, deploy (Combat and PlayerTurn banners)
    REC 01_deploiement; sleep 1
    M "tile 4 4 1.2; wait 0.8; tile 4 -4 1.2; wait 0.8; tile -3 3 1.2; wait 0.8; tile 4 -4 1; wait 0.4; click; wait 1.5; tile 4 4 1; wait 0.4; click; wait 1.5; hud Action 0 0.5 1.2; wait 0.6; click" 19
    STOP
    # A click out of reach, then the end of the turn (confirmed): the enemies' turn and the player's
    REC 02_tours_et_refus; sleep 0.5
    M "tile -3 -5 1.5; wait 0.4; click; wait 1.2; tile -4 -4 0.8; wait 0.3; click; wait 1.5; hud Action 0 0.5 1.2; wait 0.5; click; wait 1.2; click" 9
    sleep 13; STOP
    # The finisher: the enemies at 1 health, the first killed off camera, the last one filmed
    S EnemyHealth 1; APPROACH '1'; CAST BasicDamage; sleep 5
    APPROACH '1'; M "screen 0.75 0.3 0.5" 1
    REC 03_finisher_victoire; sleep 1.5; CAST BasicDamage; sleep 9; STOP
    # The four relics, walked into (the player moves between (3,3) and (1,3) of CombatRoom02)
    S ClearOrbs
    orb() { S SpawnOrb "\"$2\"" $3 $4; sleep 1; M "screen 0.5 0.5 0.3" 0.5; REC $1; sleep 1; M "tile $3 $4 1.2; wait 0.4; click" 2; sleep 5.5; PT Bag.Toggle >/dev/null; sleep 2; STOP; }
    orb 04_orbe_commune COMMON 3 3; orb 05_orbe_rare RARE 1 3; orb 06_orbe_epique EPIC 3 3; orb 07_orbe_legendaire LEGENDARY 1 3
    # A piece dropped where it doesn't fit, twice
    M "screen 0.6 0.5 0.3" 0.5
    REC 08_inventaire_refus; sleep 0.8
    M "hud Bag 0 0.5 1.2; wait 0.3; click; wait 1.2; piece 4 1; wait 0.3; down; slot 1 1 1.2; wait 0.3; up; wait 1.5; piece 4 0.8; wait 0.3; down; slot 4 2 1.2; wait 0.3; up; wait 1.2; hud Bag 0 0.5 1; wait 0.3; click" 15
    sleep 1.5; STOP
    REC 09_pause; sleep 2; PT Menus.Pause '[true]'; sleep 3; PT Menus.Pause '[false]'; sleep 2.5; STOP
}

sandbox() {
    SCENE Assets/Scenes/Tests/CombatSandbox.unity 15
    BEGIN
    PT Combat.Deploy; sleep 3; S Cursor false
    art() { APPROACH "$2"; sleep 1; REC $1; sleep 1.5; CAST $3; sleep ${4:-5}; STOP; }
    art 10_artefact_HitBuff 1 HitBuff 4.5; art 11_artefact_OrbitalShot 2 OrbitalShot 5; art 12_artefact_CriticalShot 2 CriticalShot 4.5
}

gallery() {
    # The gallery's row: SpawnRoom01, Antechamber, TreasureRoom, CombatRoom01, CombatRoom10 (pools, drops), CombatRoom11 (cliff)...
    SCENE Assets/Scenes/Tests/RoomGallery.unity 20
    BEGIN
    M "screen 0.62 0.75 0.3" 0.5
    REC 13_ambiance_grotte; sleep 14; STOP
    REC 14_sortie_et_transition; sleep 1; M "exit EAST 1.5; wait 1.5; click" 3; sleep 9; STOP
    M "screen 0.62 0.78 0.5" 1
    REC 15_ambiance_onirique; sleep 14; STOP
    for i in 1 2 3; do PT World.Move '["EAST"]' >/dev/null; sleep 6; done
    M "screen 0.62 0.78 0.5" 1
    REC 16_eau_et_gouttes; sleep 16; STOP
    REC 17_vers_la_falaise; sleep 1; M "exit EAST 1.5; wait 0.8; click" 3; sleep 16; STOP
}

game() {
    # A real run: the spawn's NORTH exit leads to a combat room in the 2026-10-02 shoot (the map is random: read Probe.Status)
    SCENE Assets/Scenes/Game/2-Game.unity 20
    BEGIN
    REC 18_entree_en_combat; sleep 1; M "exit NORTH 1.5; wait 0.6; click" 3; sleep 9; M "hud Action 0 0.5 1.2; wait 0.4; click" 2.5; sleep 5; STOP
    M "screen 0.8 0.25 0.6" 1
    REC 19_victoire_et_portails; sleep 1.5; PT Combat.HitAll '[999]' >/dev/null; sleep 9; M "exit ANY 1.5" 3; sleep 1; STOP
}

for shot in ${@:-test_room sandbox gallery game}; do $shot; done
END
