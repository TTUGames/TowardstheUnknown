# Autopilot : refonte des attaques du joueur (Anniversary)

- Branche : `anniversary-attack-feel` (depuis `anniversary-edition` @ `7ec33db9`). Pas de push, pas de merge, pas de PR (consigne).
- Session 2 (2026-09-30) partie de `d0e845c3`.
- But : timings plus courts et plus secs, VFX refaits, pour toutes les attaques du joueur, Anniversary seulement ; la Classic identique. Puis ancrer les VFX dans le monde (marque au sol, intersection dôme/sol, contact sol), Anniversary seulement.
- Suivi détaillé attaque par attaque : [docs/attack-rework.md](docs/attack-rework.md).

## Tâches

- [x] Outillage : `AttackTiming.recovery`, `VFXPlayRate`, `AttackVFXBuild`, `timing.py recovery`
- [x] BasicDamage, SlashAttack, Strike, Push, PrecisionShot, Estoc, DefensiveFluid, OffensiveFluid, Barrier, BasicShield, FightingSpirit, Haunting, ProtectiveEnvelope, Puddle, Impale, LightningExecution, WaterBlade, RockFall, GunShot, DuelMastery, WithoutFear, ShockWave, Bastion (23)

### Tâche 1 : finir le rework
- [x] EchoBomb
- [x] ExplosiveSacrifice
- [x] CelestialSword
- [x] OrbitalShot
- [x] CriticalShot
- [x] Vampirism
- [x] Rush, HitBuff (horloge sans clip sur la `duration`)

### Tâche 2 : ancrer les VFX dans le monde
- [x] Tour de toutes les attaques, liste justifiée dans `docs/attack-rework.md` (section « Ancrage au sol »)
- [x] Outillage : `VFXLifetime`, `AttackGroundBuild`, matériaux `Art/VFX/Ground`, `ground_try.sh`
- [x] WithoutFear
- [x] RockFall
- [x] ShockWave
- [x] CelestialSword
- [x] OrbitalShot
- [x] EchoBomb
- [x] ExplosiveSacrifice
- [x] Bastion
- [x] ProtectiveEnvelope

## REPRENDRE ICI

Tâche 2 : outillage fait. Prochaine action : marque au sol de (aucune) (`ground_try.sh` pour régler, `EVERY=6 films_both.sh <Nom> ground 5.5`, commit). `start` = impact réel − départ réel du VFX (`times.sh <Nom> <délai VFX>`).
