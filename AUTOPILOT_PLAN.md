# Autopilot : refonte des attaques du joueur (Anniversary)

- Branche : `anniversary-attack-feel` (depuis `anniversary-edition` @ `7ec33db9`). Pas de push, pas de merge, pas de PR (consigne).
- Session 2 (2026-09-30) partie de `d0e845c3`.
- But : timings plus courts et plus secs, VFX refaits, pour toutes les attaques du joueur, Anniversary seulement ; la Classic identique. Puis ancrer les VFX dans le monde (marque au sol, intersection dôme/sol, contact sol), Anniversary seulement.
- Suivi détaillé attaque par attaque : [docs/attack-rework.md](docs/attack-rework.md).

## Tâches

- [x] Outillage : `AttackTiming.recovery`, `VFXPlayRate`, `AttackVFXBuild`, `timing.py recovery`
- [x] BasicDamage, SlashAttack, Strike, Push, PrecisionShot, Estoc, DefensiveFluid, OffensiveFluid, Barrier, BasicShield, FightingSpirit, Haunting, ProtectiveEnvelope, Puddle, Impale, LightningExecution, WaterBlade, RockFall, GunShot, DuelMastery, WithoutFear, ShockWave, Bastion (23)

### Tâche 1 : finir le rework
- [ ] EchoBomb
- [ ] ExplosiveSacrifice
- [ ] CelestialSword
- [ ] OrbitalShot
- [ ] CriticalShot
- [ ] Vampirism
- [ ] Rush, HitBuff (sans clip : un réglage Anniversary hors du clip d'abord)

### Tâche 2 : ancrer les VFX dans le monde
- [ ] Tour de toutes les attaques, liste justifiée dans `docs/attack-rework.md`
- [ ] Marque au sol + contact sol + intersection dôme/sol, une attaque par commit (liste à venir)

## REPRENDRE ICI

Démarrage session 2 : prochaine action EchoBomb (voir « Reprendre ici » de [docs/attack-rework.md](docs/attack-rework.md)). La boucle tient en un appel de `.claude/skills/attack-inspect/scripts/rework.sh`.
