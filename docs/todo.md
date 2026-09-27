# À faire

Ce qu'on garde pour plus tard. On ajoute une ligne quand on repère quelque chose qu'on ne fait pas tout de suite, et on la supprime dès que c'est fait, dans le même commit.

## Eau

- **Son des gouttes** : `WaterDrip` a un champ `AK.Wwise.Event` vide, et le projet n'a aucun event de goutte ni d'eau. Il faut le créer dans Wwise, puis le brancher sur `Prefabs/Environment/Water/WaterDrip.prefab` (skill `wwise-events`). On peut ajouter un ambiant d'eau clapotante par bassin visible.
- **Bords des bassins** : dans CombatRoom10, le chenal de droite s'arrêtait net en bord d'écran (il a été allongé). Vérifier les autres salles qui ont de l'eau pour la même coupure, dans la `RoomGallery`.

## Ambiance

- **Brume** : régler en jeu les bancs de `Mat_RiftVolume` (`_MistBanks`, `_MistDrift`) et les volutes de `Particle_MistWisp` (opacité, hauteur, couleur). Une nappe au ras des bassins reste possible avec le même shader.

## Ennemis

- **Réglage des ennemis** : affiner en jeu dans `Tests/EnemyShowcase` (le blanc de l'ours reste gris sous l'éclairage des salles ; les fragments des Great). `EnemyGlow` n'agit pas sur le Golem, dont le shader `MagicCrystal` n'a pas de `_GlowMultiplier`.
- **Drareg** : lui passer le même traitement (Enemy Energy, aura et volutes), ses deux phases et la transition. Ses Shader Graphs `DraregGlow` et `DraregGunGlow` (`Art/Models/Characters/Drareg`) sont repris de VFX.

- **Compte à rebours de l'ultime de Drareg** : `ultimateCountdownIndicators` de `Prefabs/Entities/Enemies/Drareg.prefab` pointe vers `OldCataIndicatorState1-3`, supprimés dans `a48d753f` (références mortes). Les restaurer depuis `main` sous leurs GUID ou vider le champ et revoir l'indicateur.

## Audio

- **Events muets** : `Wolf_Claw`, `Wolf_Howl`, `Drareg_Haunting`, `Drareg_RockFall`, `Player_OrbitalShot`, `Player_HitBuff`, `Player_ClearRoomArtifact` et `PlayerTurn` n'ont aucune action dans Wwise (déjà le cas sur `main`). Le son `PlayerTurn` existe mais n'est ciblé par aucun event, donc `PlayerTurn.turnStartSound` ne joue rien.
- **Références d'events manquantes** : `Drareg_RockFall`, `Player_ClearRoomArtifact`, `Player_NanukoPaw`, `Wolf_Claw` et `Wolf_Howl` n'ont pas d'asset dans `Assets/Wwise/ScriptableObjects/Event`, et l'asset orphelin `test` ne pointe vers aucun event.

## Juice

- **Musique étouffée au coup reçu** : `PlayerHurtAudio` règle déjà `PlayerHurt`, mais sa courbe de low-pass sur le bus `Music` est à plat (0) en attendant de revoir les délais des attaques. La remonter ensuite (65 à 40, 85 à 100 sonnait trop long avec un maintien de 0,35 s).
- **Son de refus** : `UISounds.refused` est vide, le projet Wwise n'a aucun event de refus (un clic hors de portée, un artefact trop cher). Le créer dans Wwise puis le brancher (skill `wwise-events`).

## Menus

- **Onglets des options à la manette et au clavier** : les onglets Jeu / Vidéo / Audio (`OptionsView.ShowPage`) ne se changent qu'à la souris. Il manque un raccourci (LB/RB, Q/E) et une navigation au focus vérifiée dans les pages.
- **Avertissement CS0252 dans `OptionsView.HighlightLanguage`** : `button.userData == LocalizationSettings.SelectedLocale` compare des références par `object`. Ça marche, les locales sont uniques, mais `Equals` ou un cast en `Locale` le ferait taire.

## Édition Originale (branche `edition-classique`)

Plan et avancement : [plans/edition-classique.md](plans/edition-classique.md). Tout est compilé mais pas encore testé en jeu.

- **Test** : chaque salle de la `RoomGallery` dans les deux éditions, `Tests/EnemyShowcase`, un run complet dans chacune, le switch depuis le menu et depuis la pause. Vérifier en priorité le rendu des Shader Graphs de 2020 restaurés (glow des ennemis, cristal du Golem, tenue du joueur, eau Bitgem sur le cube des bassins) et des 12 shaders Amplify d'origine dans les VFX.
- **Baisse de la musique sur les coups** : elle joue encore en Originale. Dans Wwise, ajouter un Game Parameter `Edition` qui bypass l'`ImpactMeter` du bus `Impacts` (ou aplatit la courbe du bus `Music`), régénérer les banques, puis le poser depuis `Edition`.
- **Aura des Great** : l'aura d'origine de GreatKameiko et GreatNanuko (`Smoke21bcg.mat` + `VFXSphereICO_01.fbx`) n'est pas restaurée : en Originale, ils n'ont ni volutes ni aura.
- **Eau** : les volumes Bitgem d'origine (générés par `WaterVolumeBox`) ne reviennent pas : l'Originale met le matériau d'origine sur les cubes des bassins de l'Anniversary, dont la forme et la place diffèrent un peu (CombatRoom3, 6, 10).
- **Aura des drops** : les prefabs `Common/Rare/Epic/LegendaryDrop` d'origine (`vfxGraph_Drop.vfx`) ne sont pas restaurés ni appariés (`prefabs` de `pairs.json`, résolus par `Collectable`).
- **UI** : seul le HUD est habillé des sprites d'origine. L'inventaire, la fiche du personnage, la pause, les options, les résultats et le menu principal n'ont que les tokens et les rectangles. Le survol de la timeline d'origine montrait aussi ATT et DEF en pourcentage.
- **Matériaux** : `Mat_SnowPlants_Cave` remplaçait `MAT_SnowTree` (CombatRoom18) et `MAT_SnowTree 1` (CombatRoom10), apparié au premier. Les tuiles d'origine avaient un second slot de matériau (`Workshop_Set.fbx` ou un GUID manquant), que l'Anniversary a retiré.
- **`EditionCoverage`** : le contrôle d'éditeur du plan (matériaux et effets Anniversary sans version Originale) n'est pas écrit.

## Refactor (suite de l'audit du 26/09)

- **Fin des visuels d'attaque** : `AttackAnimationAction.End` (Core) appelle `GameScene.Player.playerAttack.EndAttackVisuals()` après chaque attaque, ennemis compris. `PlayerAttack` devrait finir ses visuels lui-même dans `OnCastEnd`, sans changer le rendu des casts enchaînés.
- **Triple recherche de cases des ennemis** : `EnemyAI` (`SetPlayingState`), `EnemyMove.MoveTowardsTarget` et `TacticsMove.OnMovementEnd` refont la même recherche à chaque déplacement. `isPlaying` / `isMapTransitioning` ne servent qu'au joueur et iraient dans `PlayerMove`.
- **`Tile.GetEntity()` rend un `TacticsMove`** alors que le combat travaille sur `EntityStats` : une dizaine d'allers-retours `GetComponent` (`Ability`, `EnemyAI`, `EnemyAttack`, `EnemyPattern`, `PlayerAttack`, `MoveTowardsAction`). Exposer `TacticsMove.Stats` et `EntityStats.Tile`.
- **`Artifact` recopie ses données** : 13 propriétés reprennent `ArtifactData` et le champ de données est gardé deux fois (`Artifact`, `Ability`). Un `Ability<T>` générique les exposerait une fois (appelants dans `UI/Components` et `Inventory`).
- **`TacticsMove.SetCurrentTileFromRaycast`** cherche l'enfant `TileWatcher` par nom et le layer `Terrain` par nom : sérialiser l'enfant et un `LayerMask`, ou passer la case connue par l'appelant.
- **Caméra** : `TurnCameraFocus` ne calcule qu'un offset pour `ImpactFeedback`, qui pilote la même caméra. Les fusionner gagnerait ~35 lignes (migration YAML de 5 champs dans `Gameplay.prefab`), au prix d'un composant qui mélange deux rôles : à trancher.
- **`SteamManager.cs`** : copie non modifiée de l'exemple de Valve (~190 lignes, hooks jamais surchargés). Un nettoyage gagnerait ~80 lignes.
- **`Localization.HighlightColor`** écrit `#e82a65` en dur, la valeur de `--color-accent` : le lire depuis l'USS. `GameSettings` écrit aussi les noms des RTPC Wwise en chaînes.
- **Labels posés sur le monde** : `CombatPopups`, `DamagePreview` et `QueuedCastMarkers` projettent chacun les positions du monde dans le panneau et gèrent leur propre pool. Un helper commun gagnerait ~20 lignes.
- **Pointeur du plateau** : les events statiques et l'`Update` du survol sont dans `Room` (un composant par salle) et le raycast dans `Tile.FindHoveredTile`. Un `BoardPointer` unique serait plus clair.
- **Bruit de `RiftVolumetrics` et `Mist.shader`** : leurs `Hash` / `ValueNoise` (et le `Fbm` de Mist) pourraient inclure `Rendering/Noise.hlsl` si les fonctions donnent le même résultat.
- **Entrée de salle en double** : `PlayerDeploy` (~48) et `CombatPlayerDeploy` (~22) cherchent chacun l'entrée du côté du joueur.
