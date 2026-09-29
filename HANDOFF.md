# Handoff — TowardstheUnknown — 2026-09-30

**Branche** : `anniversary-attack-feel` (pas d'upstream, rien de poussé ; 24 commits depuis `anniversary-edition` @ `7ec33db9`) · **Dernier commit** : `45f63b6e` docs(attacks): where the attack rework stops (24 of 31)
**Arbre** : propre (avant ce fichier)

## Ce qui a été fait cette session (autopilot)
- Refonte du feel de **23 attaques sur 31** pour l'Anniversary, une par commit : BasicDamage, SlashAttack, Strike, Push, PrecisionShot, Estoc, DefensiveFluid, OffensiveFluid, Barrier, BasicShield, FightingSpirit, Haunting, ProtectiveEnvelope, Puddle, Impale, LightningExecution, WaterBlade, RockFall, GunShot, DuelMastery, WithoutFear, ShockWave, Bastion (23). Le détail (impact et durée avant/après, VFX, ancres, films) est dans `docs/attack-rework.md`.
- Code : `AttackTiming.recovery` (temps gardé après l'impact, lu seulement quand l'horloge joue les phases : la Classic garde `duration`), `AttackAnimationAction.Recovery`, `AttackRecoveryAction` ; `VFXPlayRate` (vitesse des graphes et des systèmes de particules d'un VFX, un `VFXPlayRate` imbriqué garde son propre temps).
- VFX : chaque attaque a son prefab Anniversary `Prefabs/VFX/Attacks/<Nom>` (copie retimée : vitesse, délai de départ, taille, capacité réduite aux salves, étincelles d'impact pour les coups), apparié dans `ClassicSkin` avec une copie `_Classic` du prefab d'avant (`Art/Classic/Prefabs/VFX/Attacks`). Les ennemis gardent les prefabs de `Prefabs/VFX`.
- Outillage `attack-inspect` : `AttackVFXBuild.cs`, `attack_pair.py`, `times.sh`, `VFXDump.cs`, `films_both.sh`, `rework.sh`, `timing.py` gère `recovery`, `Reload` décharge vraiment les valeurs mises en mémoire. Documenté dans `docs/tech/editor-tooling.md` et le SKILL.
- Chaque attaque filmée avant/après dans les deux éditions (films hors dépôt : `S:/Unity/attack-films`), console sans erreur, `coverage.py` à 0 référence cassée.

## Reprendre ici
- **Prochaine action** : EchoBomb, puis ExplosiveSacrifice, CelestialSword, OrbitalShot, CriticalShot, Vampirism (voir la section « Reprendre ici » de `docs/attack-rework.md` pour les réglages de chacune). Rush et HitBuff n'ont pas de clip : il faut d'abord un réglage Anniversary hors du clip.
- Une attaque = `.claude/skills/attack-inspect/scripts/rework.sh <Attaque> "<timing.x=v ...>" <étincelles> <r,g,b> [échelle] [durée]`, lire les sheets, reconstruire avec `AttackVFXBuild` si besoin, remplir la ligne de la fiche, commit (`DOCS_REVIEWED=1` quand seules la fiche et les données changent).
- Commande : `git switch anniversary-attack-feel`, éditeur Unity ouvert (port 7800).

## Décisions prises
- Ne rien toucher de partagé avec la Classic : `impactDelay`, `duration` et les délais des VFX restent ; l'Anniversary passe par `timing` (dont `recovery`) et par ses propres prefabs appariés.
- Un nouveau prefab par attaque plutôt qu'éditer le prefab existant : plusieurs sont partagés avec des ennemis (Drareg, Nanuko, Golem, GreatKameiko), hors périmètre.
- Pas de VFX Graph neuf : les graphes existants sont gardés et retimés (`VFXPlayRate`) ; écrire un graphe hors de l'éditeur visuel n'était pas fiable dans le temps imparti.
- Pas de push, pas de merge, pas de PR (consigne).

## Points ouverts / risques
- Push : un halo cyan de ~0,3 s au lancement (dans les deux éditions), probablement l'éclair de `PlayerGlow` en bleu pur.
- PrecisionShot : la balle reste une petite boule sombre ; Puddle : la brume noircit les personnages ; Bastion : rubans tardifs très présents.
- Attaques de zone filmées sur un seul mannequin (pas de variante de map à plusieurs cibles).
- `organize.py` veut déplacer `SkirtTop_Classic.mat` (préexistant) : ne pas le lancer sans trancher.
- Tout est listé dans `docs/todo.md` (section Attaques).
