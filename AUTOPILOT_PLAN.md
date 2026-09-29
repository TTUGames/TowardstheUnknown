# Autopilot : refonte des attaques du joueur (Anniversary)

- Branche : `anniversary-attack-feel` (depuis `anniversary-edition` @ `7ec33db9`). Pas de push, pas de merge.
- But : timings plus courts et plus secs, VFX refaits, pour toutes les attaques du joueur, Anniversary seulement ; la Classic identique.
- Suivi détaillé attaque par attaque : [docs/attack-rework.md](docs/attack-rework.md).

## Tâches

- [x] Outillage : `AttackTiming.recovery`, `VFXPlayRate`, `AttackVFXBuild`, `timing.py recovery`
- [x] BasicDamage
- [x] SlashAttack
- [x] Strike
- [x] Push
- [x] PrecisionShot
- [x] Estoc
- [x] DefensiveFluid
- [x] OffensiveFluid
- [x] Barrier
- [x] BasicShield
- [ ] Les autres attaques, dans l'ordre de la fiche

## REPRENDRE ICI

Prochaine attaque : la première `⬜` de `docs/attack-rework.md`. Pour chacune : film avant (`film.sh`, `S:/Unity/attack-films/<Nom>-before`), `timing.py` (timing + `timing.recovery`), `AttackVFXBuild.Run`, `attack_pair.py <Artefact>`, film après, film Classic, `coverage.py`, commit.
Attention : `organize.py` veut déplacer `SkirtTop_Classic.mat` (hors sujet) : ne pas le lancer, ou annuler ce déplacement.
