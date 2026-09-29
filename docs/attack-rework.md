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
| Strike | ✅ | 0,54 / 1,1 → 0,37 / 0,82 | VFX Graph StrikeVFX (slash magenta, 1 mesh) → même graphe à ×1,8 + 20 étincelles magenta au contact | SWORD : slash sur le swing (0,37 s), étincelles sur la cible | attack-films/Strike-after/sheet.png, dense.png | Pose tenue 0,12 → 0,07 s, anticipation ×1,6, swing ×2, pose sur le coup 0,12 s (coup lourd) |
| Push | ✅ | 0,43 / 1,0 → 0,37 / reprise 0,3 s après la poussée | Shuriken PushVFX (6 systèmes, 1 particule chacun, capacité 1000 chacun) → mêmes systèmes décalés de 0,24 s sur le geste (strike 0,28 s), contenu ×0,8, capacité 1 chacun | LEFTHAND : la bourrasque part de la main au geste | attack-films/Push-after/sheet.png, early.png | Le halo cyan des 0,07–0,33 s n'est pas le VFX (présent aussi en Classic) : c'est l'éclair de lancement du joueur (`PlayerGlow`) en bleu HDR, à vérifier (voir todo). La durée compte après le `MoveTowardsAction` |
| PrecisionShot | ✅ | 0,65 / 1,2 → 0,49 / 0,84 (+ vol du projectile) | Shuriken PrecisionShotMark (croix de visée, 2 particules taille 5, capacité 1000) → taille ×0,55, capacité 2 ; balle inchangée | TARGETTILE : la croix au sol sous la cible, sans barrer l'écran ; GUN pour la balle | attack-films/PrecisionShot-after/sheet.png | Anticipation ×1,8, pose 0,08 s, geste ×1,8. Balle agrandie essayée puis retirée : `Mat_Waterball` la rend noire et plus visible, à refaire (todo). Drareg garde la croix d'origine |
| Estoc | ✅ | 0,79 / 1,25 → 0,63 / 1,08 | Shuriken EstocVFX (8 systèmes : lame, énergie, rayons, 20 étincelles ; capacité 1000 chacun) → mêmes systèmes à ×1,35, capacité aux salves + 16 étincelles blanches au contact | SWORD : la lame d'estoc sort sur le coup (0,67 s), étincelles sur la cible | attack-films/Estoc-after/sheet.png | Accroupi raccourci (anticipation ×2,6, pose 0,08 s), estoc ×1,8, pose sur le coup 0,12 s |
| DefensiveFluid | ✅ | 0,59 / 1,1 → 0,46 / 0,81 | Shuriken DefensiveFluidVFX (traînées seules, salve 10–20, capacité 10000) → même effet à ×1,27, capacité à sa salve | TARGETTILE (case du joueur) : les volutes l'enveloppent au geste | attack-films/DefensiveFluid-after/sheet.png | Anticipation ×1,6, pose 0,06 s, geste ×2 ; le bonus apparaît à 0,53 s au lieu de 0,67 s. GreatNanuko garde l'ancien VFX |
| OffensiveFluid | ✅ | 0,59 / 1,1 → 0,46 / 0,81 | Shuriken OffensiveFluidVFX (3 systèmes de rubans, salves de 4, capacité 10000 chacun) → même effet à ×1,27, capacité 4 chacun | TARGETTILE (case du joueur) : les rubans rouges l'enveloppent au geste | attack-films/OffensiveFluid-after/sheet.png | Même clip et même réglage que DefensiveFluid ; le bonus apparaît à 0,53 s. Le film « avant » a été refait avec l'ancien timing en mémoire (`probe.sh Set`). GreatKameiko garde l'ancien VFX |
| Barrier | ✅ | 0,5 / 1,2 → 0,3 / 0,65 | Shuriken BarrierVFX (bouclier maillé Mat_Shield3, 1 particule, capacité 1000) → à ×1,6, contenu ×0,75, capacité 1 | TARGETTILE (case du joueur) : le bouclier se forme sur la garde | attack-films/Barrier-after/sheet.png | Timing activé pour l'Anniversary (il était coupé) : garde levée d'un trait (×1,8 jusqu'à 0,45, pose 0,1 s). L'armure tombe à 0,40 s au lieu de 0,60 s |
| BasicShield | ✅ | 0,5 / 1,2 → 0,3 / 0,65 | Shuriken BasicShieldVFX (bouclier Mat_Shield + anneau Mat_Bastion1, 1 particule chacun, capacité 1000) → à ×1,6, contenu ×0,85, capacité 1 | TARGETTILE (case du joueur) : les arcs tournent autour de lui dès la garde | attack-films/BasicShield-after/sheet.png | Même clip et même réglage que Barrier (timing activé pour l'Anniversary) |
| FightingSpirit | ✅ | 0,60 / 1,1 → 0,47 / 0,82 | Shuriken FightingSpiritVFX (aura cyan : anneaux, rubans montants) → à ×1,27, contenu ×0,7, capacité aux salves | TARGETTILE (case du joueur) : l'aura serre le joueur au lieu de couvrir la case voisine | attack-films/FightingSpirit-after/sheet.png, small.png | Anticipation ×1,5, pose 0,06 s, geste ×2 ; le bonus d'attaque apparaît à 0,53 s |
| Haunting | ✅ | 0,46 / 1,0 → 0,41 / 0,76 | Shuriken HauntingVFX (flammes spectrales rouges et violettes sur la cible) → à ×1,11, capacité aux salves | TARGETTILE : les flammes montent sur la cible avant le coup | attack-films/Haunting-after/sheet.png, small.png | Geste ×1,6 puis ×2, pose 0,04 s ; l'impact garde le vol du sort (0,16 s après le geste). Drareg et Nanuko gardent l'ancien VFX |
| HitBuff | ⬜ | | | | | |
| Impale | ⬜ | | | | | |
| LightningExecution | ⬜ | | | | | |
| OrbitalShot | ⬜ | | | | | |
| ProtectiveEnvelope | ✅ | 0,56 / 1,1 → 0,45 / 0,80 | Shuriken ProtectiveEnvelopeVFX (dôme cyan et étincelles) → à ×1,23, capacité aux salves | TARGETTILE (case du joueur) : le dôme se ferme sur le geste | attack-films/ProtectiveEnvelope-after/sheet.png, small.png | Geste ×1,6 puis ×2 jusqu'à 0,36 (strike repoussé : sur soi, pas de vol de sort), pose 0,08 s |
| Puddle | ✅ | 0,56 / 1,1 → 0,45 / 0,80 | Shuriken PuddleVFX (flaque violette et brume de zone) → à ×1,23, capacité aux salves | TARGETTILE : la flaque se forme sous la cible au geste | attack-films/Puddle-after/sheet.png, small.png | Même réglage que ProtectiveEnvelope (strike repoussé à 0,36). La brume noie encore les personnages : à doser (todo) |
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
