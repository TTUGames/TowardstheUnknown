# Refonte des attaques (Anniversary)

Refonte du feel des attaques du joueur pour l'Anniversary : des timings plus courts et plus secs, des VFX refaits. La Classic ne bouge pas : elle joue les clips à vitesse constante (`attackTiming` de son profil) et attend toute la `duration`, et `ClassicSkin` lui rend le prefab VFX d'avant (copie `_Classic`).

## Méthode

- **Timing** : `AbilityData.timing` (`AttackTiming`) règle l'anticipation, la pose, le swing et la pose sur le strike ; `timing.recovery` est le temps (réel) gardé après l'impact, lu seulement quand les phases jouent. `duration`, `impactDelay` et les délais des VFX restent ceux de la Classic.
- **VFX** : `AttackVFXBuild` (skill `attack-inspect`) copie le prefab actuel en `Art/Classic/Prefabs/VFX/Attacks/<Nom>_Classic`, construit le prefab Anniversary `Prefabs/VFX/Attacks/<Nom>` (graphe accéléré par `VFXPlayRate` pour tomber sur le nouvel impact, étincelles d'impact), puis la paire va dans `pairs.json` et `build_skin.py`. Les ennemis gardent les prefabs de `Prefabs/VFX`.
- **Contrôle** : film avant/après (`film.sh`, CombatSandbox, joueur au contact), film Classic avant (changements mis de côté) et après, console sans erreur, `coverage.py`.
- Les films sont hors du dépôt, dans le dossier attack-films à côté du projet (S:/Unity/attack-films), un sous-dossier par film : <Attaque>-before, -after, -classic-before, -classic, chacun avec son sheet.png.

## Suivi

Timing : impact réel et fin de l'attaque (reprise de la main), en secondes, Anniversary.

| Attaque | Statut | Timing avant → après (impact, durée) | VFX avant → après (type, particules) | Ancres vérifiées | Contact sheet | Notes |
|---|---|---|---|---|---|---|
| BasicDamage | ✅ | 0,46 / 1,0 → 0,36 / 0,76 | VFX Graph SwordSlash (2 systèmes, 1 mesh chacun) → même graphe à ×1,3 + 18 étincelles Shuriken au contact (0,8 m vers la cible, projetées au-delà) | SWORD : slash sur la lame, étincelles sur le corps de la cible (film dense 0,43–0,5 s) | attack-films/BasicDamage-after/sheet.png | Prefab partagé avec `DraregBasicDamagePattern` : Drareg garde l'ancien. Anticipation raccourcie (swingStart 0,26 → 0,2, ×1,5), swing ×2,1 |
| SlashAttack | ✅ | 0,56 / 1,0 → 0,39 / 0,79 | VFX Graph SlashAttackVFX (slash rouge, 1 mesh) → même graphe à ×1,6 + 18 étincelles rouges au contact | SWORD : le slash naît sur le swing (0,37 s), étincelles sur la cible | attack-films/SlashAttack-after/sheet.png, dense.png | Pose tenue 0,1 → 0,06 s, anticipation ×1,6, swing ×2,2 ; VFX lancé à 0,125 s (délai 0,2 porté par l'horloge) |
| Strike | ⬜ | | | | | |
| Push | ⬜ | | | | | |
| PrecisionShot | ⬜ | | | | | |
| Estoc | ⬜ | | | | | |
| DefensiveFluid | ⬜ | | | | | |
| OffensiveFluid | ⬜ | | | | | |
| Barrier | ⬜ | | | | | |
| BasicShield | ⬜ | | | | | |
| FightingSpirit | ⬜ | | | | | |
| Haunting | ⬜ | | | | | |
| HitBuff | ⬜ | | | | | |
| Impale | ⬜ | | | | | |
| LightningExecution | ⬜ | | | | | |
| OrbitalShot | ⬜ | | | | | |
| ProtectiveEnvelope | ⬜ | | | | | |
| Puddle | ⬜ | | | | | |
| RockFall | ⬜ | | | | | |
| WaterBlade | ⬜ | | | | | |
| DuelMastery | ⬜ | | | | | |
| EchoBomb | ⬜ | | | | | |
| ExplosiveSacrifice | ⬜ | | | | | |
| GunShot | ⬜ | | | | | |
| Rush | ⬜ | | | | | |
| ShockWave | ⬜ | | | | | |
| WithoutFear | ⬜ | | | | | |
| Bastion | ⬜ | | | | | |
| CelestialSword | ⬜ | | | | | |
| CriticalShot | ⬜ | | | | | |
| Vampirism | ⬜ | | | | | |
