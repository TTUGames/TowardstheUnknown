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
- **Texture de `VFXWarmup`** : la texture de sa caméra n'est libérée qu'au changement d'édition.
- **Mesh Read/Write** sur ~200 meshes des salles (`ThirdParty/LowPolyCavePack/Models`, `Art/Models/Nature`), ~15 Mo de RAM : le couper si aucun système de particules ou VFX ne les échantillonne.
- **Petits coûts** : l'Originale fait deux picks UI et deux raycasts par frame (`Tile.FindHoveredTile` puis `FindHoveredModel`) ; `CombatPopups` crée un `Label` par popup ; la neige fait des collisions monde en qualité High sur ~900 particules (~0,8 ms CPU, la baisser change où les flocons se posent).

## Audio

- **Sons d'attaque à régler à l'oreille** (voir [attack sounds](tech/audio.md#attack-sounds)) : les `soundDelay` de 20 abilities et les trims Anniversary des cinq sons coupés (Estoc, Impale, LightningExecution, CelestialSword, ExplosiveSacrifice) sont des valeurs de départ mesurées (impact de l'Anniversary moins le pic du sample), jamais écoutées en jeu. CelestialSword et ExplosiveSacrifice perdent 0,2 à 0,35 s de montée audible : à juger. Les trois sources de CelestialSword lèvent un avertissement à la génération des banques (« loop start position is out of range »), sans effet sur un son qui ne boucle pas.
- **Events muets** : `Wolf_Claw`, `Wolf_Howl`, `Drareg_Haunting`, `Drareg_RockFall`, `Player_OrbitalShot`, `Player_HitBuff`, `Player_ClearRoomArtifact` et `PlayerTurn` n'ont aucune action dans Wwise (déjà le cas sur `main`). Le son `PlayerTurn` existe mais n'est ciblé par aucun event, donc `PlayerTurn.turnStartSound` ne joue rien.
- **Références d'events manquantes** : `Drareg_RockFall`, `Player_ClearRoomArtifact`, `Player_NanukoPaw`, `Wolf_Claw` et `Wolf_Howl` n'ont pas d'asset dans `Assets/Wwise/ScriptableObjects/Event`, et l'asset orphelin `test` ne pointe vers aucun event.

## Attaques

- **Rush et HitBuff sans animation** : sans clip, le joueur reste immobile jusqu'au coup (l'horloge les accélère seulement). Leur donner un clip Anniversary (ruée, coup sur soi) demande qu'une paire `ClassicSkin` de clip accepte un côté Classic vide (`EditionSkin.Current` rendrait `null`). Le VFX de HitBuff, ancré sur l'épée, n'apparaît que vers 0,8 s : l'ancrer sur la case du joueur.
- **Rayons de Vampirism** : les rayons de drain (`Prefabs/VFX/Attacks/Vampirism`, ancre RIGHTHAND) partent vers le haut, au-delà de la cible ; les réorienter de la cible vers la main dans la version Anniversary.
- **Fin du graphe d'EchoBomb** : l'explosion finale du graphe (flash blanc plein écran vers 2,6 s) puis sa vague sombre qui assombrit tout l'écran jusqu'à 4,0 s (la Classic la coupe à 3,5 s, `vfxDuration` ; l'Anniversary la laisse finir, `VFXLifetime` gardant l'instance pour son sceau au sol). Doser le flash et la vague dans `Prefabs/VFX/Attacks/EchoBomb`.
- **Marques au sol : suites** (`AttackGroundBuild`, voir la section « Ancrage au sol » de `docs/attack-rework.md`) : l'intersection dôme/sol passe par l'anneau de contact ; un vrai fondu de profondeur des dômes reste à faire sur des copies Anniversary de leurs matériaux (`Mat_Withoutfear` a `_InvFade`, `Mat_Shield` et `Mat_ShieldBastion` un masque de profondeur `_DEPTHMASKENABLED` déjà actif). La poussière est d'un gris neutre partout : la teinter par biome demanderait que le VFX connaisse la salle. Les marques restent discrètes sous l'overlay vert de début de tour : à revoir en jeu (`RoomGallery`), avec Bastion lancé sur soi (les films le lancent sur la case du mannequin, sa marque n'a pas été vue sous le joueur).
- **Clips des packs d'animation** (dossier `D:/Unity/Assets/Animations` : Human Melee, Throwing, Magic Animation Blend…) : pas encore passés en revue pour remplacer un clip d'attaque dans l'Anniversary (extraction ciblée du `.unitypackage`, paire `ClassicSkin` de clip) ; Rush et HitBuff, sans clip, en sont les premiers candidats.
- **VFX Graph** : aucun graphe neuf écrit ; les marques au sol sont des Particle Systems (un quad par couche, rien qui gagne à passer en graphe).
- **Attaques de zone à plusieurs cibles** : ShockWave, Puddle et les autres attaques de zone n'ont été filmées que sur le mannequin seul de CombatSandbox ; ajouter une variante de map avec plusieurs mannequins pour vérifier leurs ancres.
- **Éclair de lancement de Push** : un halo cyan couvre le joueur et la cible pendant ~0,3 s au lancement de Push (couleur d'artefact bleu pur), dans les deux éditions ; vérifier que c'est l'éclair de `PlayerGlow` et le doser pour les couleurs saturées.
- **Balle de PrecisionShot** : `PrecisionShotBullet` (partagée avec Drareg) se lit comme une petite boule sombre qui reste près de la main à courte portée ; agrandie, `Mat_Waterball` la rend noire. Lui faire un vrai tracé lumineux Anniversary (paire `ClassicSkin`).
- **Brume de Puddle** : la brume violette de `Prefabs/VFX/Attacks/Puddle` couvre les personnages (la cible devient noire) ; baisser son opacité ou sa taille dans la version Anniversary.
- **`organize.py`** veut déplacer `SkirtTop_Classic.mat` de `Art/Classic/Models/Characters/Protagonist/Materials` vers `Art/Classic/Materials` : trancher (le déplacer ou corriger `pairs.json`).

## Juice

- **Musique étouffée au coup reçu** : `PlayerHurtAudio` règle déjà `PlayerHurt`, mais sa courbe de low-pass sur le bus `Music` est à plat (0) en attendant de revoir les délais des attaques. La remonter ensuite (65 à 40, 85 à 100 sonnait trop long avec un maintien de 0,35 s).
- **Son d'ouverture de coffre** : les pièces qui arrivent dans le coffre (`TetrisInventory.Reveal`) jouent `UISounds.artifactDrop` faute de mieux. Créer dans Wwise un son de dévoilement qui monte avec la rareté (un event par rareté, ou un game parameter), et un son d'éclat pour l'orbe (`Collectable.TryPickUp`, aucun son aujourd'hui).
- **Couleur du sang** : rouge sombre pour l'instant (`startColor` de `Prefabs/VFX/BloodSpurt` et `BloodMarks`). À trancher : noir aux reflets rouges, ou par entité (noir pour les créatures, rouge pour le joueur et Drareg, un champ de `BloodFeedback.variants`).
- **Éclats du Golem** : `Prefabs/VFX/CrystalShards` (ses cristaux projetés au lieu du sang) n'a pas été vu en jeu. Le filmer dans `Tests/EnemyShowcase` et régler la taille et le nombre des éclats (les mêmes `lightCount` / `heavyCount` que les jets de sang, peut-être trop nombreux).
- **Écrasement du joueur au coup** : `Player.prefab` garde `squashPerMeter` à 0,6 alors que le défaut est passé à 0,8 ; l'aligner ou le garder plus discret sur le joueur.
- **Son de refus** : `UISounds.refused` est vide, le projet Wwise n'a aucun event de refus (un clic hors de portée, un artefact trop cher). Le créer dans Wwise puis le brancher (skill `wwise-events`).

## Attaques : timing, courbes et impact

Plan du 29/09 : on garde les clips et on joue sur le temps. Fait : les durées suivent le budget (commun ≤ 1,2 s, rare et épique ≤ 2 s, légendaire ≤ 3 s, ennemis ≤ 1,5 s), les VFX leur survivent (`vfxDuration`), la courbe de temps (`AttackTiming`, voir combat.md) est réglée sur les attaques du joueur, de Drareg et des loups, l'épée se matérialise avant le coup et laisse un trail (`WeaponTrail`), la caméra donne un à-coup dans l'axe du coup, les sorts lancés sur place gardent les jambes en posture (`AbilityData.legs`, un layer masqué au haut du corps), et le skill `attack-inspect` mesure et filme les attaques.

- **Attaques sans courbe** : l'ultime et le Golem (Barrier, BasicShield, EchoBomb, OrbitalShot, CriticalShot et Vampirism l'ont reçue ; Vampirism joue encore 7,8 s de mocap ×2 dont le geste fort est à la fin : recouper la plage d'import) ; les ours (rig générique : les filmer demande un ours dans la sandbox, dont le mannequin est un loup) et le hurlement du GreatKameiko. Les lames de Drareg pourraient aussi laisser un trail (`WeaponTrail` sur son prefab, `SwingsBlade` sur ses patterns).
- **Impact côté attaquant** : lueur de l'arme qui monte pendant l'élan, étincelles orientées dans le sens du coup ; puis une passe sur les VFX mous (vitesse initiale, easing, fondu).
- **Fenêtre « Attack Lab »** (option) : régler la courbe à l'œil dans l'éditeur, la courbe et le marqueur de contact sur une timeline ; `attack-inspect` couvre le besoin en ligne de commande.

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
- **Robustesse** : `TurnSystem.EndPlayerTurn` ignore sans rien dire une fin de tour pendant une marche ; la coroutine de délai de `VFXInfo.Play` tourne sur le lanceur (elle meurt avec lui, et un `delay` plus long que `duration` laisse le VFX jamais rendu au pool) ; la génération continue avec une carte incomplète après 100 essais ratés.

## Game design (décisions à prendre)

- **Menace des Grands ennemis sur toute la carte** : leurs patterns « Fluid » (buff sur eux-mêmes) ciblent le joueur avec un cercle de 0 à 100, faute de ciblage de soi pour les ennemis ; `EnemyAttack.GetThreatenedTiles` montre donc toute la carte menacée. Donner aux ennemis des patterns sur soi (`castEffects`, cible `Caster`) et les exclure de la menace.
- **Durée des statuts** : elle décompte au début du tour du porteur, donc un statut d'une durée de 1 posé par un ennemi sur le joueur expire avant qu'il joue. Le `DraregUltimateSuccessPattern` donne ainsi AttackUp pour 1 tour à sa cible, le joueur, sans effet : il visait sans doute Drareg. Choisir une règle (décompte à la fin du tour du porteur, ou durée comptée en tours du porteur).
- **Cooldown décalé** : `cooldown = N` bloque N−1 tours (`cooldown = 1` ne bloque rien), et l'UI affiche `cooldown - 1` à deux endroits (`Artifact.CooldownDescription`, `InventoryScreen.ShowDescription`). Recaler les données et le code sur « N tours bloqués ».
- **Réordonner l'inventaire en combat change la barre de sorts** : `InventoryDrag.Grab` retire la pièce et la remet en fin de `Artifacts`, ce qui décale les touches 1-9. Garder l'ordre de la barre indépendant de la grille.
- **Effets sur le lanceur** : certains artefacts les mettent dans `castEffects` (Bastion, ExplosiveSacrifice), d'autres dans les `effects` par cible (Vampirism, DuelMastery, HitBuff, WithoutFear, LightningExecution), qui ne marchent que tant qu'ils sont à cible unique. L'autodégât de HitBuff passe par l'armure, celui d'ExplosiveSacrifice non. Tout ramener dans `castEffects`.
- **Épée après un sort** : en combat, l'épée réapparaît à la fin de chaque sort du joueur (`PlayerAttack.OnCastEnd` → `Dissolving.Start`, comme sur `main`) et reste jusqu'à la prochaine attaque ennemie. À garder ou à cacher jusqu'à la fin du combat.
- **Données** : `CombatRoomArtifactPool` liste `Strike` deux fois et son groupe vide pèse 20 contre 22 (~48 % des récompenses de combat vides) ; une vingtaine d'artefacts et la plupart des patterns ennemis gardent l'`impactDelay` de 0,5 s par défaut ; le tir de précision de Drareg part à 0,5 s, 1 u au-dessus de sa case (`DraregPrecisionShotPattern.projectile`) : à caler sur son animation en jeu.
