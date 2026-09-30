# Handoff — TowardstheUnknown — 2026-09-30 (session 2)

**Branche** : `anniversary-attack-feel` (pas d'upstream, rien de poussé, pas de PR, consigne) · **Arbre** : propre après le commit de ce fichier

## Ce qui a été fait (autopilot, ~85 min)
- **Tâche 1, rework terminé (31/31)** : EchoBomb, ExplosiveSacrifice, CelestialSword, OrbitalShot, CriticalShot, Vampirism, Rush, HitBuff, un commit chacun, avec leur ligne dans `docs/attack-rework.md` (timing et VFX avant/après, films dans `S:/Unity/attack-films`).
  - Code : `AbilityData.Clock` fait courir l'horloge d'une attaque sans clip sur sa `duration` (Rush et HitBuff s'accélèrent dans l'Anniversary).
  - Leçon : quand la pose tenue entre le geste et l'impact *est* l'effet (charge de CriticalShot, bras tendu de Vampirism), mettre `strike` à l'impact (0), sinon le délai reste après le geste.
- **Tâche 2, ancrage au sol** : la liste justifiée est dans la section « Ancrage au sol » de `docs/attack-rework.md`. Les 9 retenues ont un enfant `Ground` dans leur prefab Anniversary : WithoutFear, RockFall, ShockWave, CelestialSword, OrbitalShot, EchoBomb, ExplosiveSacrifice, Bastion et ProtectiveEnvelope.
  - Chaque `Ground` contient une tache, une marque (fissures, glyphe, sceau ou sang) et sa lueur qui refroidit, un anneau de contact, de la poussière et un flash de lumière, avec un fondu de 4 à 5 s.
  - Code : `VFXLifetime` garde l'instance jusqu'à la fin du fondu. `AttackAnimationAction` la libère plus tard, sans script qui tourne.
  - Matériaux `Art/VFX/Ground` (URP Particles/Unlit, textures existantes).
  - Outils : `AttackGroundBuild.cs` et `ground_try.sh`, plus `EVERY` pour `films_both.sh` et `SKIP_BEFORE` pour `rework.sh`.
- La Classic est inchangée : copies `_Classic` sans marque, films Classic vérifiés, `coverage.py` à 0 référence cassée à chaque commit, console sans erreur.

## Reprendre ici
- Rien d'obligatoire. Les suites sont dans `docs/todo.md` (section Attaques) :
  - fondu de profondeur des dômes ;
  - revue des clips des packs `D:/Unity/Assets/Animations`, surtout pour Rush et HitBuff ;
  - rayons de Vampirism ;
  - fin du graphe d'EchoBomb (flash blanc et vague sombre) ;
  - VFX de HitBuff trop tardif.
- À tester en jeu (RoomGallery) : la lisibilité des marques sous l'overlay de début de tour, et Bastion lancé sur soi.

## Décisions prises
- Pas de VFX Graph neuf : une marque tient en un quad par couche, les Particle Systems suffisent.
- Pas de clip de pack importé : le temps a servi au rework et aux marques, et un clip Anniversary pour une attaque sans clip demanderait une paire `ClassicSkin` au côté Classic vide (noté).
- La marque d'EchoBomb est posée à l'explosion finale du graphe (2,55 s), car la zone du graphe la couvrirait avant.
