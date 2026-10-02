# À faire

Ce qu'on garde pour plus tard. On ajoute une ligne quand on repère quelque chose qu'on ne fait pas tout de suite, et on la supprime dès que c'est fait, dans le même commit.

## Eau

- **Bords des bassins** : dans CombatRoom10, le chenal de droite s'arrêtait net en bord d'écran (il a été allongé). Vérifier les autres salles qui ont de l'eau pour la même coupure, dans la `RoomGallery`.

## Ambiance

- **Brume** : régler en jeu (fini quand la brume se lit sans masquer les cases dans CombatRoom03, 06 et 10) les bancs de `Mat_RiftVolume` (`_MistBanks`, `_MistDrift`) et les volutes de `Particle_MistWisp` (opacité, hauteur, couleur). Une nappe au ras des bassins reste possible avec le même shader.

## Sorties

- **Sons des portails** : `Portal_Open`, `Portal_Close`, `Portal_Hover` et `Portal_Click` sont des placeholders synthétisés (`Originals/SFX/Map/Portal_*.wav`) : les faire refaire et remplacer les samples dans Wwise (les events et les champs de `RoomExit.prefab` restent), puis régénérer `Main`. L'ouverture joue aussi en entrant dans une salle sans combat (spawn, trésor, salle déjà faite), derrière le volet : à garder ou non à l'écoute.
- **À voir à l'œil** : l'apparition des portails n'a été vue qu'image par image (`_Reveal` fixé à la main), pas en temps réel à la fin d'un combat ; et le dernier réglage du survol d'une salle visitée sur la minimap (contour blanc, fond à 0,06) n'a pas été capturé.
- **Survol à la manette** : le portail et la minimap réagissent à `Room.TileHovered` ; vérifier que le curseur de la manette le déclenche aussi sur une sortie.

## Ennemis

- **Réglage des ennemis** : dans `Tests/EnemyShowcase`, régler les fragments des Great. `EnemyGlow` n'agit pas sur le Golem, dont le shader `MagicCrystal` n'a pas de `_GlowMultiplier`.
- **Drareg** : lui passer le même traitement que les ennemis (Enemy Energy, aura et volutes), ses deux phases et la transition. Ses Shader Graphs `DraregGlow` et `DraregGunGlow` (`Art/Models/Characters/Drareg`) sont repris de VFX.


## Performance

Relevé de l'audit du 29/09 (mesures dans l'éditeur : l'Anniversary coûte bien plus que l'Originale, surtout en GPU ; scripts et GC au repos sont faibles). Ce qui reste :

- **À vérifier en jeu — fondu de la vignette de vie basse** : `.low-health` passe en `visibility: hidden` une fois éteinte ; la première version coupait le fondu de sortie à mi-course (opacité 0,11), corrigée par un `transition-delay` sur la visibilité (`Hud.uss`) mais pas revérifiée (l'éditeur ne répondait plus). Suivre l'opacité image par image en soignant le joueur depuis 15 PV : elle doit descendre jusqu'à 0 avant de passer cachée ; sinon retirer la visibilité de la règle.
- **Textures UI de l'Originale non compressées** : 44 des 61 textures de `Art/Classic/UI` sont en RGBA32 (tailles non multiples de 4, jusqu'à 2851x4516), chargées seulement en Originale depuis `ClassicStyles`. Compresser (padding à un multiple de 4, BC7) change légèrement les pixels : à valider par A/B.
- **SSAO dans la réflexion de l'eau** : la caméra miroir de `WaterReflection` refait le prépass profondeur/normales et le SSAO de la salle. Un renderer sans SSAO pour elle change les reflets (mesuré : ~0,8 % des pixels, jusqu'à 89/255) : à trancher.
- **Flou de mouvement de l'Originale caméra immobile** : le couper tant que la matrice de vue ne bouge pas serait identique, à condition qu'URP garde la matrice précédente à jour pendant ce temps (sinon une frame floue à la reprise) : à vérifier avant.
- **`CutShape`** recrée un `VectorImage` à chaque frame d'une transition de taille (barres de vie et d'armure après un coup, ~70 par coup) : dessiner les parts sans texte par `generateVisualContent`, ou garder les images des états qui alternent.
- **Inventaire reconstruit à chaque prise et pose** (`TetrisInventory`, `grid.Clear()` puis toutes les pièces) : ne mettre à jour que la pièce déplacée (les autres garderaient leur phase d'animation au lieu d'en tirer une nouvelle).
- **Petits coûts** : la neige fait des collisions monde en qualité High sur ~900 particules (~0,8 ms CPU, la baisser change où les flocons se posent).

## Audio

- **Remplacer les placeholders** (liste dans [audio](tech/audio.md#placeholders) ; dossier du sound designer avec les vidéos, TTU_Sons_Placeholders dans les Téléchargements et son zip, refait par le skill `sound-brief`) : orbes, ambiances, déploiement, bannières, volet, pause, refus, finisher, gouttes, portails, HitBuff, OrbitalShot, CriticalShot. Écraser le fichier dans `Originals/SFX` sous le même nom puis régénérer `Main`. Rien de tout ça n'a été écouté en jeu : volumes et `soundDelay` des trois artifacts à régler (0 pour l'instant). Le fondu enchaîné de 2 s des ambiances, lancé à l'entrée de la salle derrière le volet, est à juger à l'oreille.
- **`Portal_Close` presque jamais entendu** : les sorties d'une salle de combat sont déjà fermées quand on y entre, donc la fermeture ne joue que si un combat démarre avec des sorties ouvertes (vu dans aucune partie filmée le 02/10). Décider s'il faut le garder, ou le jouer à un autre moment (le joueur qui quitte la salle ?).
- **Son du dévoilement des pièces** : les pièces qui arrivent dans le coffre (`TetrisInventory.Reveal`) jouent encore `UISounds.artifactDrop` ; un son qui monte avec la rareté reste à créer si le drop ne suffit pas.

- **Sons d'attaque à régler à l'oreille** (voir [attack sounds](tech/audio.md#attack-sounds)) : les `soundDelay` de 20 abilities et les trims Anniversary des cinq sons coupés (Estoc, Impale, LightningExecution, CelestialSword, ExplosiveSacrifice) sont des valeurs de départ mesurées (impact de l'Anniversary moins le pic du sample), jamais écoutées en jeu. CelestialSword et ExplosiveSacrifice perdent 0,2 à 0,35 s de montée audible : à juger. Les trois sources de CelestialSword lèvent un avertissement à la génération des banques (« loop start position is out of range »), sans effet sur un son qui ne boucle pas.

## Attaques

- **Rush et HitBuff sans animation** : sans clip, le joueur reste immobile jusqu'au coup (l'horloge les accélère seulement). Leur donner un clip Anniversary (ruée, coup sur soi) et l'apparier à `null` dans `clips` de `pairs.json`, pour que la Classic reste sans animation (côté Classic vide accepté depuis le 02/10, testé seulement par une paire injectée en mémoire).
- **Rayons de Vampirism** : les rayons de drain (`Prefabs/VFX/Attacks/Vampirism`, ancre RIGHTHAND) partent vers le haut, au-delà de la cible ; les réorienter de la cible vers la main dans la version Anniversary.
- **Fin du graphe d'EchoBomb** : l'explosion finale du graphe (flash blanc plein écran vers 2,6 s) puis sa vague sombre qui assombrit tout l'écran jusqu'à 4,0 s (la Classic la coupe à 3,5 s, `vfxDuration` ; l'Anniversary la laisse finir, `VFXLifetime` gardant l'instance pour son sceau au sol). Doser le flash et la vague dans `Prefabs/VFX/Attacks/EchoBomb`.
- **Marques au sol : suites** (`AttackGroundBuild`, voir la section « Ancrage au sol » de `docs/attack-rework.md`) : l'intersection dôme/sol passe par l'anneau de contact ; un vrai fondu de profondeur des dômes reste à faire sur des copies Anniversary de leurs matériaux (`Mat_Withoutfear` a `_InvFade`, `Mat_Shield` et `Mat_ShieldBastion` un masque de profondeur `_DEPTHMASKENABLED` déjà actif). La poussière est d'un gris neutre partout : la teinter par biome demanderait que le VFX connaisse la salle. Les marques restent discrètes sous l'overlay vert de début de tour : à revoir en jeu (`RoomGallery`), avec Bastion lancé sur soi (les films le lancent sur la case du mannequin, sa marque n'a pas été vue sous le joueur).
- **Clips des packs d'animation** : les 12 packs d'animation de `D:/Unity/Assets/Animations` sont extraits dans `Assets/ThirdParty/Animations` (2 237 clips humanoïdes masculins triés par usage, noms uniformes, `SOURCES.csv`, état dans `A_LIRE_ETAT.md`), exclu en local par `.git/info/exclude` : à écrémer, puis à commiter. Choisir ensuite les clips d'attaque de l'Anniversary (paire `ClassicSkin` de clip) ; Rush et HitBuff, sans clip, en sont les premiers candidats. Licence du pack Souls Like à vérifier (il contient un fichier d'un site de redistribution).
- **Geste de Vampirism** : l'attaque voulue est une main tendue lentement vers l'ennemi, le poing qui se ferme, puis un retrait très rapide qui porte l'impact ; lui trouver un clip (packs ci-dessus ; les essais d'IA UniMate n'ont pas convaincu), régler sa courbe, puis refaire son VFX autour du retrait (voir les rayons ci-dessus). L'artifact est sans arme (`weapon: none`).
- **Foot IK en jeu** : le Foot IK d'Unity est actif sur tous les états de `Entity.controller` (les pieds ne glissent plus avec un clip fait sur un autre rig), vérifié en preview seulement. Regarder en Play mode que les pieds ne tremblent pas avec `FootIK` (même passe IK), et décider si la Classic, qui partage le controller, doit garder le glissement d'origine.
- **VFX Graph** : aucun graphe neuf écrit ; les marques au sol sont des Particle Systems (un quad par couche, rien qui gagne à passer en graphe).
- **Attaques de zone à plusieurs cibles** : ShockWave, Puddle et les autres attaques de zone n'ont été filmées que sur le mannequin seul de CombatSandbox ; ajouter une variante de map avec plusieurs mannequins pour vérifier leurs ancres.
- **Éclair de lancement de Push** : un halo cyan couvre le joueur et la cible pendant ~0,3 s au lancement de Push (couleur d'artefact bleu pur), dans les deux éditions ; vérifier que c'est l'éclair de `PlayerGlow` et le doser pour les couleurs saturées.
- **Balle de PrecisionShot** : `PrecisionShotBullet` (partagée avec Drareg) se lit comme une petite boule sombre qui reste près de la main à courte portée ; agrandie, `Mat_Waterball` la rend noire. Lui faire un vrai tracé lumineux Anniversary (paire `ClassicSkin`).
- **À regarder en jeu — brume de Puddle** : dans `Prefabs/VFX/Attacks/Puddle` (Anniversary), les fumées `Dust` (violette, alpha 0,5) et `Dust (1)` (presque noire, alpha 0,45) sont surchargées, au lieu d'opaques ; le film de la sandbox laisse la cible hors cadre : vérifier que la cible reste lisible, sinon réduire aussi leur taille.

## Juice

- **Musique étouffée au coup reçu** : `PlayerHurtAudio` règle déjà `PlayerHurt`, mais sa courbe de low-pass sur le bus `Music` est à plat (0) en attendant de revoir les délais des attaques. La remonter ensuite (65 à 40, 85 à 100 sonnait trop long avec un maintien de 0,35 s).
- **Couleur du sang** (gardé de côté le 02/10, à voir plus tard) : rouge sombre pour l'instant (`startColor` de `Prefabs/VFX/BloodSpurt` et `BloodMarks`). À trancher : noir aux reflets rouges, ou par entité (noir pour les créatures, rouge pour le joueur et Drareg, un champ de `BloodFeedback.variants`).
- **Éclats du Golem** : `Prefabs/VFX/CrystalShards` (ses cristaux projetés au lieu du sang) n'a pas été vu en jeu. Le filmer dans `Tests/EnemyShowcase` et régler la taille et le nombre des éclats (les mêmes `lightCount` / `heavyCount` que les jets de sang, peut-être trop nombreux).

## Attaques : timing, courbes et impact

Plan du 29/09 : on garde les clips et on joue sur le temps. Fait : les durées suivent le budget (commun ≤ 1,2 s, rare et épique ≤ 2 s, légendaire ≤ 3 s, ennemis ≤ 1,5 s), les VFX leur survivent (`vfxDuration`), la courbe de temps (`AttackTiming`, voir combat.md) est réglée sur les attaques du joueur, de Drareg et des loups, l'épée se matérialise avant le coup et laisse un trail (`WeaponTrail`), la caméra donne un à-coup dans l'axe du coup, les sorts lancés sur place gardent les jambes en posture (`AbilityData.legs`, un layer masqué au haut du corps), et le skill `attack-inspect` mesure et filme les attaques.

- **Attaques sans courbe** : l'ultime et le Golem (Barrier, BasicShield, EchoBomb, OrbitalShot, CriticalShot et Vampirism l'ont reçue ; Vampirism joue encore 7,8 s de mocap ×2 dont le geste fort est à la fin : recouper la plage d'import) ; les ours (rig générique : les filmer demande un ours dans la sandbox, dont le mannequin est un loup) et le hurlement du GreatKameiko. Les lames de Drareg pourraient aussi laisser un trail (`WeaponTrail` sur son prefab, `SwingsBlade` sur ses patterns).
- **Impact côté attaquant** : lueur de l'arme qui monte pendant l'élan, étincelles orientées dans le sens du coup ; puis une passe sur les VFX mous (vitesse initiale, easing, fondu).
- **Fenêtre « Attack Lab »** (option) : régler la courbe à l'œil dans l'éditeur, la courbe et le marqueur de contact sur une timeline ; `attack-inspect` couvre le besoin en ligne de commande.

## Menus

- **Tout le jeu à la manette** (décision du 02/10) : on ne porte pas une partie isolée (les onglets Jeu / Vidéo / Audio d'`OptionsView.ShowPage`, qui ne se changent qu'à la souris, n'auraient de sens qu'avec le reste) ; si on s'y met, c'est tout le jeu d'un coup : menus, options (LB/RB, Q/E), HUD, choix des cases et des cibles en combat, inventaire et coffres, résultats, avec un focus visible partout. À tester, ce serait un plus.

## Édition Originale

Vérifiée en jeu et fusionnée dans `dev` le 28/09 (voir [editions](features/editions.md)) ; restent des écarts mineurs avec l'original.

- **Tests à poursuivre** : chaque salle de la `RoomGallery` dans les deux éditions, `Tests/EnemyShowcase`, un run complet dans chacune, le switch depuis le menu et depuis la pause. Vérifier en priorité le rendu des Shader Graphs de 2020 restaurés (glow des ennemis, cristal du Golem, tenue du joueur, eau Bitgem sur le cube des bassins) et des 12 shaders Amplify d'origine dans les VFX.
- **Eau** : les volumes Bitgem d'origine (générés par le `WaterVolumeBox` de `main`) ne reviennent pas : l'Originale met le matériau d'origine sur les cubes des bassins de l'Anniversary, dont la forme et la place diffèrent un peu (CombatRoom03, 6, 10).
- **UI** : les extrémités en biais des barres de vie et d'armure (`-unity-slice-scale`) sont à vérifier en jeu.
- **Matériaux** : `Mat_SnowPlants_Cave` remplaçait le `MAT_SnowTree` de `main` (CombatRoom18) et `MAT_SnowTree 1` (CombatRoom10), apparié au premier. Les tuiles d'origine avaient un second slot de matériau (`Workshop_Set.fbx` ou un GUID manquant), que l'Anniversary a retiré.
- **Flammes des torches** : `Prefabs/VFX/TorchFlame` est préchauffée (`prewarm`, pas sur `main`) et la flamme de `ZLPC_Torch_06` a bougé d'environ 0,2 m (sa lumière a retrouvé ses valeurs d'origine par `EditionLight`).
- **À surveiller — coût caché en Originale** : `WindAnchor` laisse un property block sur les plantes tant qu'il est actif (posé une fois, les sort du SRP Batcher ; il ne l'efface qu'une fois désactivé), même avec les matériaux Classic qui ne lisent pas `_WindAnchor`, et les panneaux HUD masqués par `ClassicHud.uss` (vignette, bannières, barre de boss, aperçu des dégâts, marqueurs) continuent d'écouter leurs events. Faible coût, à couper si le profilage le montre.
- **Décision — flou derrière les menus d'origine** : la pause, l'inventaire et les résultats flouaient tout l'écran (profondeur de champ, `ChangeUI.ChangeBlur` sur `main`). Non repris : le flou plein écran est écarté dans le jeu. À trancher si l'Originale doit l'avoir. Contexte (utilisateur, 02/10) : ce flou simulait du glassmorphism, mis sur tout l'écran faute de savoir flouter juste derrière les panneaux ; en suspens, peu important.
- **Textes d'origine** : les boutons des résultats étaient en capitales (« RECOMMENCER »), le titre des crédits en casse normale (« Crédits ») ; la page d'avertissement d'origine avait le titre « Attention », un séparateur, le texte FR puis EN justifié, les liens Discord, le bouton « Lancer le jeu » et le logo du studio, à chaque lancement (pas de splash). Demande une casse par édition (la classe `caps` est appliquée par `MenuScreen` au texte localisé).
- **Inventaire (détails)** : l'original tournait une pièce au relâchement du clic droit, jouait le son de clic à chaque clic gauche partout à l'écran, et rejouait l'ouverture du panneau quand le coffre remplaçait la fiche.

### Écarts connus, sans action prévue

- **Ombres des lumières** : le biais, le plan proche et la qualité des ombres douces de certaines lumières diffèrent de l'original (réglages par lumière, pas repris).
- **Titre de l'infobulle de la timeline** : Kallisto Bold d'origine ; le texte riche de UI Toolkit ne sait faire qu'un gras synthétique de Kallisto Medium.
- **Orbe de transition de Drareg** : l'original extrapolait sa couleur au-delà du rouge (`Mathf.Min`, non borné) ; la nôtre s'arrête au rouge.
- **Traits de 1 pt** (menu principal) : disparaissent sous 1080p, comme dans l'original.

## Refactor (audits du 26/09 et du 29/09)

- **À vérifier en jeu — poussées et ruées** : `MoveTowardsAction` (le `MoveEffect` de Push, Rush, ShockWave, WithoutFear, OrbitalShot et des patterns de ruée et d'onde de choc des ennemis) passe par `EntityStats.Move` / `Tile` depuis le refactor `TacticsMove.Stats`, compilé mais jamais joué : lancer Push et Rush dans `Tests/CombatSandbox`, et laisser le Great Nanuko charger dans `Tests/EnemyShowcase`.
- **`Artifact` recopie ses données** : 13 propriétés reprennent `ArtifactData` et le champ de données est gardé deux fois (`Artifact`, `Ability`). Un `Ability<T>` générique les exposerait une fois (appelants dans `UI/Components` et `Inventory`).
- **Pointeur du plateau** : les events statiques et l'`Update` du survol sont dans `Room` (un composant par salle) et le raycast dans `Tile.FindHovered`. Un composant unique de pointeur du plateau (à créer) serait plus clair.
- **Cases de déploiement en double** : `SpawnPlayerDeploy.spawnTile` double les cases de déploiement du `RoomLayout` (la recherche de l'entrée est partagée par `PlayerDeploy.Entrance` depuis le 01/10, compilée mais pas jouée : entrer dans une salle de combat vide et dans la salle de départ).
- **Trésors hors `RoomLayout`** : la salle de départ et l'antichambre n'ont un « layout » que par leur `TreasureSpawnLayout` (`Room.LayoutCount` compte les `SpawnLayout`). Des cases trésor dans `RoomLayout` supprimeraient `SpawnLayout` et `TreasureSpawnLayout`.
- **Gameplay qui appelle l'UI** : `Collectable.TryPickUp` (`UI.Inventory.OpenChest`), `CombatPlayerDeploy` (`UI.Fade`, `Hud.EnterDeployState`), `Map` (minicarte, fondu), `Tile` (`Hud.IsPointerOver`) ; à l'inverse `EntityInfoPanel` écrit `Tile.IsThreat`. Passer par `GameEvents`.
- **À vérifier en jeu — tokens de couleur de l'UI** : toutes les feuilles sont passées en tokens (`--color-clear`, `--color-minimap-*`, l'inventaire, les flous `--panel-blur-button` et `-icon`, `--classic-*`) sans éditeur ouvert, valeurs inchangées mais rendu jamais regardé : `playtest.sh styles`, puis le HUD, la minicarte, les options, le disclaimer, la pause, l'inventaire et les résultats dans les deux éditions.
- **À regarder en jeu — durées et tailles de l'UI en USS** : toutes les durées de l'UI sont lues par `UssTime` (valeurs relues en Play mode le 02/10, échelonnement du menu pause et infobulle de la barre de vie vus), mais le rendu n'a pas été regardé : la pulsation et le battement du bouton d'action, le quart de tour et l'éclat d'une pièce, les détails plus petits des infobulles et l'infobulle de la timeline en Originale (28 / 18 / 30).
- **Sondes `Pointer.*` hors focus** : les events de souris mis en file par `Playtest.cs` arrivent dans le tampon de l'éditeur tant que la Game view n'a pas le focus (vu le 02/10 : survol et clics sans effet). `Studio.cs` du skill `sound-brief` écrit l'état de la souris depuis les frames du jeu (`InputState.Change`, update `Dynamic`) : porter cette méthode dans `Pointer`.

## Game design (décisions à prendre)

- **À vérifier en jeu — épée en main par défaut** (02/10) : vus en Play mode un tir au pistolet, un sort à main nue et un tour ennemi ; pas joués : une chaîne de sorts qui passe du pistolet à l'épée (la visée doit retomber, `hold.StopAim` dans `PlayerAttack.Cast`), la fin d'un combat sur le dernier coup, et la Classic, où l'épée reste aussi en main en combat (l'original la dissolvait au début du combat) et où le pistolet s'affiche à côté d'elle pendant un tir (pas de `weaponSwapSpeed`, comme l'original).
- **Données** : `CombatRoomArtifactPool` liste `Strike` deux fois (son groupe vide, ~48 % des récompenses de combat, est voulu) ; GunShot, HitBuff et Rush gardent l'`impactDelay` de 0,5 s par défaut sans strike mesuré (les deux derniers n'ont pas de clip) ; le tir de précision de Drareg part au strike (0,47), 1 u au-dessus de sa case (`DraregPrecisionShotPattern.projectile`) : à caler sur son animation en jeu.
- **À regarder en jeu — impacts recalés le 02/10** : les effets de ProtectiveEnvelope, Bastion, Puddle, WithoutFear, DuelMastery, FightingSpirit, BasicShield, Barrier, EchoBomb et des patterns Blast et PrecisionShot de Drareg tombent sur le strike mesuré (`impactDelay` = `timing.strike`, plus tôt sauf EchoBomb), et les `soundDelay` d'Envelope, Puddle (0) et WithoutFear (0,3) ont suivi. En Originale, sans courbe, l'impact suit les mêmes valeurs.
