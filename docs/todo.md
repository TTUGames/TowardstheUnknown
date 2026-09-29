# À faire

Ce qu'on garde pour plus tard. On ajoute une ligne quand on repère quelque chose qu'on ne fait pas tout de suite, et on la supprime dès que c'est fait, dans le même commit.

## Eau

- **Son des gouttes** : `WaterDrip` a un champ `AK.Wwise.Event` vide, et le projet n'a aucun event de goutte ni d'eau. Il faut le créer dans Wwise, puis le brancher sur `Prefabs/Environment/Water/WaterDrip.prefab` (skill `wwise-events`). On peut ajouter un ambiant d'eau clapotante par bassin visible.
- **Bords des bassins** : dans CombatRoom10, le chenal de droite s'arrêtait net en bord d'écran (il a été allongé). Vérifier les autres salles qui ont de l'eau pour la même coupure, dans la `RoomGallery`.

## Ambiance

- **Brume** : régler en jeu (fini quand la brume se lit sans masquer les cases dans CombatRoom03, 06 et 10) les bancs de `Mat_RiftVolume` (`_MistBanks`, `_MistDrift`) et les volutes de `Particle_MistWisp` (opacité, hauteur, couleur). Une nappe au ras des bassins reste possible avec le même shader.

## Ennemis

- **Réglage des ennemis** : dans `Tests/EnemyShowcase`, régler les fragments des Great. `EnemyGlow` n'agit pas sur le Golem, dont le shader `MagicCrystal` n'a pas de `_GlowMultiplier`.
- **Drareg** : lui passer le même traitement que les ennemis (Enemy Energy, aura et volutes), ses deux phases et la transition. Ses Shader Graphs `DraregGlow` et `DraregGunGlow` (`Art/Models/Characters/Drareg`) sont repris de VFX.


## Performance

Relevé de l'audit du 29/09 (mesures dans l'éditeur : l'Anniversary coûte bien plus que l'Originale, surtout en GPU ; scripts et GC au repos sont faibles). Ce qui reste :

- **À vérifier en jeu — fondu de la vignette de vie basse** : `.low-health` passe en `visibility: hidden` une fois éteinte ; la première version coupait le fondu de sortie à mi-course (opacité 0,11), corrigée par un `transition-delay` sur la visibilité (`Hud.uss`) mais pas revérifiée (l'éditeur ne répondait plus). Suivre l'opacité image par image en soignant le joueur depuis 15 PV : elle doit descendre jusqu'à 0 avant de passer cachée ; sinon retirer la visibilité de la règle.
- **Textures UI de l'Originale chargées dans les deux éditions (~340 Mo)** : `Theme.tss` importe les feuilles `Classic*.uss`, dont les `url()` chargent les 61 textures de `Art/Classic/UI` (44 en RGBA32 non compressé, tailles non multiples de 4, jusqu'à 2527x2906). Charger les feuilles Classic à la demande (un thème Classic échangé sur `Edition.Changed`, sans référence sérialisée qui les charge d'avance) ; `ArtifactData.classicInventorySprite` les charge aussi. Compresser (padding à un multiple de 4, BC7) change légèrement les pixels : à valider par A/B.
- **SSAO dans la réflexion de l'eau** : la caméra miroir de `WaterReflection` refait le prépass profondeur/normales et le SSAO de la salle. Un renderer sans SSAO pour elle change les reflets (mesuré : ~0,8 % des pixels, jusqu'à 89/255) : à trancher.
- **Flou de mouvement de l'Originale caméra immobile** : le couper tant que la matrice de vue ne bouge pas serait identique, à condition qu'URP garde la matrice précédente à jour pendant ce temps (sinon une frame floue à la reprise) : à vérifier avant.
- **`CutShape`** recrée un `VectorImage` à chaque frame d'une transition de taille (barres de vie et d'armure après un coup, ~70 par coup) : dessiner les parts sans texte par `generateVisualContent`, ou garder les images des états qui alternent.
- **Inventaire reconstruit à chaque prise et pose** (`TetrisInventory`, `grid.Clear()` puis toutes les pièces) : ne mettre à jour que la pièce déplacée (les autres garderaient leur phase d'animation au lieu d'en tirer une nouvelle).
- **Instances de matériaux jamais détruites** : `.material` dans `PlayerGlow` (armes), `Dissolving`, `DraregArena`, `DraregPhaseTransitionAction` ; la texture de la caméra de `VFXWarmup` n'est libérée qu'au changement d'édition. À vérifier : après un changement d'édition, `PlayerGlow.weaponMaterials` pointerait encore les anciennes instances (teinte des armes figée).
- **Mesh Read/Write** sur ~200 meshes des salles (`ThirdParty/LowPolyCavePack/Models`, `Art/Models/Nature`), ~15 Mo de RAM : le couper si aucun système de particules ou VFX ne les échantillonne.
- **Petits coûts** : l'Originale fait deux picks UI et deux raycasts par frame (`Tile.FindHoveredTile` puis `FindHoveredModel`) ; `DamagePreview` bat toutes les 400 ms même sans aperçu ; `CombatPopups` crée un `Label` par popup ; la neige fait des collisions monde en qualité High sur ~900 particules (~0,8 ms CPU, la baisser change où les flocons se posent).

## Audio

- **Events muets** : `Wolf_Claw`, `Wolf_Howl`, `Drareg_Haunting`, `Drareg_RockFall`, `Player_OrbitalShot`, `Player_HitBuff`, `Player_ClearRoomArtifact` et `PlayerTurn` n'ont aucune action dans Wwise (déjà le cas sur `main`). Le son `PlayerTurn` existe mais n'est ciblé par aucun event, donc `PlayerTurn.turnStartSound` ne joue rien.
- **Références d'events manquantes** : `Drareg_RockFall`, `Player_ClearRoomArtifact`, `Player_NanukoPaw`, `Wolf_Claw` et `Wolf_Howl` n'ont pas d'asset dans `Assets/Wwise/ScriptableObjects/Event`, et l'asset orphelin `test` ne pointe vers aucun event.

## Juice

- **Musique étouffée au coup reçu** : `PlayerHurtAudio` règle déjà `PlayerHurt`, mais sa courbe de low-pass sur le bus `Music` est à plat (0) en attendant de revoir les délais des attaques. La remonter ensuite (65 à 40, 85 à 100 sonnait trop long avec un maintien de 0,35 s).
- **Son d'ouverture de coffre** : les pièces qui arrivent dans le coffre (`TetrisInventory.Reveal`) jouent `UISounds.artifactDrop` faute de mieux. Créer dans Wwise un son de dévoilement qui monte avec la rareté (un event par rareté, ou un game parameter), et un son d'éclat pour l'orbe (`Collectable.TryPickUp`, aucun son aujourd'hui).
- **Son de refus** : `UISounds.refused` est vide, le projet Wwise n'a aucun event de refus (un clic hors de portée, un artefact trop cher). Le créer dans Wwise puis le brancher (skill `wwise-events`).

## Attaques : timing, courbes et impact

Plan du 29/09. On garde les clips (mocaps figés, une clé par frame : on ne retouche pas les os) et on joue sur le temps. Mesures du 29/09 (clip échantillonné sur le rig du joueur, pic de vitesse des mains). Fait : les `duration` suivent le budget ci-dessous, les VFX leur survivent (`vfxDuration`), Impale et LightningExecution frappent à l'estoc (1,15 s), DragonStrike à 0,6 s. Reste : 33 attaques frappent toujours au `impactDelay` par défaut (0,5 s), à vérifier à l'œil (les sorts surtout, dont le moment fort est dans le VFX) ; Estoc, commun, dépasse le budget (1,8 s, l'estoc part à 1,15 s) en attendant la courbe de temps ; les `vfxDuration` des VFX portés par l'arme ou la main (fin du blocage + 0,5 s) sont à revoir en jeu.

1. **Mesure** : skill `attack-inspect` (`run_script` hors Play mode) qui échantillonne chaque clip, sort la frame de contact (pic de vitesse de la pointe d'épée ou des mains), la reprise d'immobilité et un graphe par attaque, et compare à `impactDelay` et `duration`. Le pic de vitesse ne vaut pas le contact pour les sorts : à confirmer à l'œil.
2. **Budget de durée** (`duration`) : commun ≤ 1,2 s, rare et épique ≤ 2 s, légendaire ≤ 3 s, ennemis ≤ 1,5 s, l'ultime de Drareg en exception (5 s). Appliqué le 29/09, à régler en jeu.
3. **Courbe de temps par attaque** : un `AttackTiming` sur `AbilityData` (courbe temps réel → temps du clip, ou phases anticipation / coup / pose / récupération, et la frame de contact), lu par `EntityAnimator` à travers un paramètre Motion Time des états `AttackA/B` d'`Entity.controller`. `impactDelay` s'en déduit, les `VFXInfo.delay` se calent sur le contact. Courbe linéaire dans le Classic (`EditionProfile`).
4. **Fenêtre « Attack Lab »** pour régler à l'œil dans la `RoomGallery` : courbe, marqueur de contact, VFX sur une timeline, lecture en boucle. Skill `attack-filmstrip` en option (planche d'images autour de l'impact, en Play mode).
5. **Trail d'épée** : ruban garde-pointe (pas un `TrailRenderer`), à la couleur de l'artefact, allumé pendant la phase de coup ; BasicDamage, SlashAttack, WaterBlade, Strike, Impale, Estoc, LightningExecution, CelestialSword, DuelMastery, puis les lames de Drareg. `EditionOnly` Anniversary.
6. **Impact côté attaquant** : pose figée 2 à 4 frames au contact, shake dans l'axe du coup, lueur de l'arme qui monte pendant l'élan, étincelles orientées ; puis une passe sur les VFX mous (vitesse initiale, easing, fondu).
7. **Clips trop longs à la source** : Vampirism (7,8 s de mocap joué ×2), CriticalShot (2,4 s joué ×0,5), Impale/Estoc (3,2 s) : recouper la plage d'import ou compresser l'anticipation par la courbe.

## Menus

- **Onglets des options à la manette et au clavier** : les onglets Jeu / Vidéo / Audio (`OptionsView.ShowPage`) ne se changent qu'à la souris. Il manque un raccourci (LB/RB, Q/E) et une navigation au focus vérifiée dans les pages.

## Édition Originale

Vérifiée en jeu et fusionnée dans `dev` le 28/09 (voir [editions](features/editions.md)) ; restent des écarts mineurs avec l'original.

- **Tests à poursuivre** : chaque salle de la `RoomGallery` dans les deux éditions, `Tests/EnemyShowcase`, un run complet dans chacune, le switch depuis le menu et depuis la pause. Vérifier en priorité le rendu des Shader Graphs de 2020 restaurés (glow des ennemis, cristal du Golem, tenue du joueur, eau Bitgem sur le cube des bassins) et des 12 shaders Amplify d'origine dans les VFX.
- **Eau** : les volumes Bitgem d'origine (générés par le `WaterVolumeBox` de `main`) ne reviennent pas : l'Originale met le matériau d'origine sur les cubes des bassins de l'Anniversary, dont la forme et la place diffèrent un peu (CombatRoom03, 6, 10).
- **UI** : les extrémités en biais des barres de vie et d'armure (`-unity-slice-scale`) sont à vérifier en jeu.
- **Matériaux** : `Mat_SnowPlants_Cave` remplaçait le `MAT_SnowTree` de `main` (CombatRoom18) et `MAT_SnowTree 1` (CombatRoom10), apparié au premier. Les tuiles d'origine avaient un second slot de matériau (`Workshop_Set.fbx` ou un GUID manquant), que l'Anniversary a retiré.
- **Flammes des torches** : `Prefabs/VFX/TorchFlame` est préchauffée (`prewarm`, pas sur `main`) et la flamme de `ZLPC_Torch_06` a bougé d'environ 0,2 m (sa lumière a retrouvé ses valeurs d'origine par `EditionLight`).
- **À surveiller — coût caché en Originale** : `WindAnchor` laisse un property block sur les plantes tant qu'il est actif (posé une fois, les sort du SRP Batcher ; il ne l'efface qu'une fois désactivé), même avec les matériaux Classic qui ne lisent pas `_WindAnchor`, et les panneaux HUD masqués par `ClassicHud.uss` (vignette, bannières, barre de boss, aperçu des dégâts, marqueurs) continuent d'écouter leurs events. Faible coût, à couper si le profilage le montre.
- **Décision — flou derrière les menus d'origine** : la pause, l'inventaire et les résultats flouaient tout l'écran (profondeur de champ, `ChangeUI.ChangeBlur` sur `main`). Non repris : le flou plein écran est écarté dans le jeu. À trancher si l'Originale doit l'avoir.
- **Textes d'origine** : les boutons des résultats étaient en capitales (« RECOMMENCER »), le titre des crédits en casse normale (« Crédits ») ; la page d'avertissement d'origine avait le titre « Attention », un séparateur, le texte FR puis EN justifié, les liens Discord, le bouton « Lancer le jeu » et le logo du studio, à chaque lancement (pas de splash). Demande une casse par édition (la classe `caps` est appliquée par `MenuScreen` au texte localisé).
- **Inventaire (détails)** : l'original tournait une pièce au relâchement du clic droit, jouait le son de clic à chaque clic gauche partout à l'écran, et rejouait l'ouverture du panneau quand le coffre remplaçait la fiche.

### Écarts connus, sans action prévue

- **Ombres des lumières** : le biais, le plan proche et la qualité des ombres douces de certaines lumières diffèrent de l'original (réglages par lumière, pas repris).
- **Titre de l'infobulle de la timeline** : Kallisto Bold d'origine ; le texte riche de UI Toolkit ne sait faire qu'un gras synthétique de Kallisto Medium.
- **Orbe de transition de Drareg** : l'original extrapolait sa couleur au-delà du rouge (`Mathf.Min`, non borné) ; la nôtre s'arrête au rouge.
- **Traits de 1 pt** (menu principal) : disparaissent sous 1080p, comme dans l'original.

## Refactor (audits du 26/09 et du 29/09)

- **Fin des visuels d'attaque** : `AttackAnimationAction.End` (Core) appelle `GameScene.Player.playerAttack.EndAttackVisuals()` après chaque attaque, ennemis compris. `PlayerAttack` devrait finir ses visuels lui-même dans `OnCastEnd`, sans changer le rendu des casts enchaînés.
- **Triple recherche de cases des ennemis** : `EnemyAI` (`SetPlayingState`), `EnemyMove.MoveTowardsTarget` et `TacticsMove.OnMovementEnd` refont la même recherche à chaque déplacement. `isPlaying` / `isMapTransitioning` ne servent qu'au joueur et iraient dans `PlayerMove`.
- **`Tile.GetEntity()` rend un `TacticsMove`** alors que le combat travaille sur `EntityStats` : une dizaine d'allers-retours `GetComponent` (`Ability`, `EnemyAI`, `EnemyAttack`, `EnemyPattern`, `PlayerAttack`, `MoveTowardsAction`). Exposer `TacticsMove.Stats` et `EntityStats.Tile`.
- **`Artifact` recopie ses données** : 13 propriétés reprennent `ArtifactData` et le champ de données est gardé deux fois (`Artifact`, `Ability`). Un `Ability<T>` générique les exposerait une fois (appelants dans `UI/Components` et `Inventory`).
- **Décision — caméra** : `TurnCameraFocus` ne calcule qu'un offset pour `ImpactFeedback`, qui pilote la même caméra. Les fusionner gagnerait ~35 lignes (migration YAML de 5 champs dans `Gameplay.prefab`), au prix d'un composant qui mélange deux rôles : à trancher.
- **Labels posés sur le monde** : `CombatPopups`, `DamagePreview` et `QueuedCastMarkers` projettent chacun les positions du monde dans le panneau et gèrent leur propre pool. Un helper commun gagnerait ~20 lignes.
- **Pointeur du plateau** : les events statiques et l'`Update` du survol sont dans `Room` (un composant par salle) et le raycast dans `Tile.FindHoveredTile`. Un composant unique de pointeur du plateau (à créer) serait plus clair.
- **Bruit de `RiftVolumetrics` et `Mist.shader`** : leurs `Hash` / `ValueNoise` (et le `Fbm` de Mist) pourraient inclure `Rendering/Noise.hlsl` si les fonctions donnent le même résultat.
- **Entrée de salle en double** : `PlayerDeploy.DefaultDeploy` et `CombatPlayerDeploy.DeployPlayer` cherchent chacun l'entrée du côté du joueur ; `SpawnPlayerDeploy.spawnTile` double aussi les cases de déploiement du `RoomLayout`.
- **Trésors hors `RoomLayout`** : la salle de départ et l'antichambre n'ont un « layout » que par leur `TreasureSpawnLayout` (`Room.LayoutCount` compte les `SpawnLayout`). Des cases trésor dans `RoomLayout` supprimeraient `SpawnLayout` et `TreasureSpawnLayout`.
- **Gameplay qui appelle l'UI** : `Collectable.TryPickUp` (`UI.Inventory.OpenChest`), `CombatPlayerDeploy` (`UI.Fade`, `Hud.EnterDeployState`), `Map` (minicarte, fondu), `Tile` (`Hud.IsPointerOver`) ; à l'inverse `EntityInfoPanel` écrit `Tile.IsThreat`. Passer par `GameEvents`.
- **Tag `Tile` dans `Room.ReloadTilesWithRandomPrefab`** : il saute les sorties par `CompareTag("Tile")`, mais 89 `TransitionTile` ont le tag `Tile` (donc re-maillées) et 2 cases non-sorties ont `MapChangerTile`. Décider quelles cases gardent leur modèle avant de passer à un test de type.
- **`SteamAchievements`** recompte les ennemis tués et les salles au lieu de lire `RunStats`, et pousse les stats Steam depuis les scènes de test (CombatSandbox, EnemyShowcase) : lire `RunStats` et ne rien pousser hors du jeu normal.
- **Couleurs en dur restantes** : `ClassicHud.uss` / `ClassicMenus.uss` gardent les valeurs de l'original (#E82A65, #20D15F, rgb(116, 89, 216), #F5F5F5 ×6, #FFFFFF ×12...), `Hud.uss` et `Inventory.uss` quelques rgb, les flous 10 et 12 à côté de `--panel-blur`. Les passer en tokens (`--classic-*` pour l'Originale, pour ne pas suivre l'Anniversaire). Côté code : tailles et durées en dur dans `TimelinePanel`, `HudTooltip.Format` (`<size=85%>`), `Hud.PulseDuration`, `ArtifactPiece.TurnDuration`, `MenuScreen`.
- **Robustesse** : `EntityStats.Heal` soigne un mort ; `TurnSystem.EndPlayerTurn` ignore sans rien dire une fin de tour pendant une marche ; la coroutine de délai de `VFXInfo.Play` tourne sur le lanceur (elle meurt avec lui, et un `delay` plus long que `duration` laisse le VFX jamais rendu au pool) ; la génération continue avec une carte incomplète après 100 essais ratés.

## Game design (décisions à prendre)

- **Menace des Grands ennemis sur toute la carte** : leurs patterns « Fluid » (buff sur eux-mêmes) ciblent le joueur avec un cercle de 0 à 100, faute de ciblage de soi pour les ennemis ; `EnemyAttack.GetThreatenedTiles` montre donc toute la carte menacée. Donner aux ennemis des patterns sur soi (`castEffects`, cible `Caster`) et les exclure de la menace.
- **Durée des statuts** : elle décompte au début du tour du porteur, donc un statut d'une durée de 1 posé par un ennemi sur le joueur expire avant qu'il joue. Le `DraregUltimateSuccessPattern` donne ainsi AttackUp pour 1 tour à sa cible, le joueur, sans effet : il visait sans doute Drareg. Choisir une règle (décompte à la fin du tour du porteur, ou durée comptée en tours du porteur).
- **Cooldown décalé** : `cooldown = N` bloque N−1 tours (`cooldown = 1` ne bloque rien), et l'UI affiche `cooldown - 1` à deux endroits (`Artifact.CooldownDescription`, `InventoryScreen.ShowDescription`). Recaler les données et le code sur « N tours bloqués ».
- **Réordonner l'inventaire en combat change la barre de sorts** : `InventoryDrag.Grab` retire la pièce et la remet en fin de `Artifacts`, ce qui décale les touches 1-9. Garder l'ordre de la barre indépendant de la grille.
- **Effets sur le lanceur** : certains artefacts les mettent dans `castEffects` (Bastion, ExplosiveSacrifice), d'autres dans les `effects` par cible (Vampirism, DuelMastery, HitBuff, WithoutFear, LightningExecution), qui ne marchent que tant qu'ils sont à cible unique. L'autodégât de HitBuff passe par l'armure, celui d'ExplosiveSacrifice non. Tout ramener dans `castEffects`.
- **Épée après un sort** : en combat, l'épée réapparaît à la fin de chaque sort du joueur (`PlayerAttack.OnCastEnd` → `Dissolving.Start`, comme sur `main`) et reste jusqu'à la prochaine attaque ennemie. À garder ou à cacher jusqu'à la fin du combat.
- **Popup de dégâts « mortel »** : `GameEvents.DamageTaken` part avant la mise à jour de la vie, et `CombatPopups` en déduit un coup mortel (`healthLost >= CurrentHealth`) qui ne l'est pas pour le mannequin immortel ni pour Drareg bloqué au seuil de phase.
- **Données** : `CombatRoomArtifactPool` liste `Strike` deux fois et son groupe vide pèse 20 contre 22 (~48 % des récompenses de combat vides) ; une vingtaine d'artefacts et la plupart des patterns ennemis gardent l'`impactDelay` de 0,5 s par défaut ; le tir de précision de Drareg part à 0,5 s, 1 u au-dessus de sa case (`DraregPrecisionShotPattern.projectile`) : à caler sur son animation en jeu.
