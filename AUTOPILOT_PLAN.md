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
- [x] FightingSpirit
- [x] Haunting
- [x] ProtectiveEnvelope
- [x] Puddle
- [x] Impale
- [x] LightningExecution
- [x] WaterBlade
- [x] RockFall
- [x] GunShot
- [x] DuelMastery
- [x] WithoutFear
- [x] ShockWave
- [x] Bastion
- [ ] EchoBomb, ExplosiveSacrifice, CelestialSword, OrbitalShot, CriticalShot, Vampirism
- [ ] Rush, HitBuff (sans clip : un réglage Anniversary hors du clip d'abord)

## REPRENDRE ICI

Arrêt propre à 98 min (budget de temps du backstop), 24 attaques sur 31 faites, arbre propre. Suite : section « Reprendre ici » de [docs/attack-rework.md](docs/attack-rework.md) ; la boucle tient en un appel de `.claude/skills/attack-inspect/scripts/rework.sh`.
