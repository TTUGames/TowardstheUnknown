# Plan : édition Originale / Anniversary

Un réglage des options fait basculer le jeu entre l'**Anniversary** (la branche `dev`) et l'**Originale**, c'est-à-dire le rendu, le level art et l'expérience de la version sortie (`main`, 1.0.1, Unity 2020.3). Ce qui reste commun aux deux éditions : le code refactoré, le gameplay, les données, l'IA, la génération des cartes, la progression et les corrections de bugs. Tout le reste suit l'édition :
- les shaders et les matériaux ;
- le pipeline de rendu ;
- le level art des salles ;
- les ajouts d'ambiance ;
- les feedbacks ;
- les améliorations d'UX ;
- le mix audio ;
- l'habillage de l'UI.

Dans le code et les assets, l'édition d'origine s'appelle `Classic`, pour éviter la confusion avec les mots « original » et « source ». Dans le jeu, l'option s'appelle « Originale ».

## Ce que dit l'inventaire (27/09/2026)

La comparaison de `main` et `dev` porte sur les GUID, les contenus et les références des prefabs.

- **Déjà identique à l'original, rien à faire** :
  - les 7 modèles de personnages et les 3 armes ;
  - toutes les animations ;
  - les 106 sons et la musique ;
  - les icônes et les polices ;
  - les meshes des tuiles et du décor ;
  - les 38 prefabs de VFX des capacités (mêmes textures, meshes et Shader Graphs), sauf 12 shaders Amplify réécrits en HLSL URP ;
  - l'angle de caméra.
- **Changé en gardant le GUID de l'original.** C'est le piège : même asset, autre rendu. Les matériaux du décor, de la végétation, des tuiles, de l'eau et des cristaux sont passés sur `SnowLit` et `NatureLit`. S'y ajoutent la tenue lumineuse du joueur, l'arme de Drareg, les 12 shaders Amplify, les overlays de cases, les prefabs de drop et le profil de post-process du jeu.
- **Remplacé puis supprimé de `dev`** :
  - les matériaux des ennemis et leurs shaders (`VFX_GlowGun`, `Crystal`, Glow, `VFX`) ;
  - l'eau Bitgem ;
  - QuickOutline ;
  - les chiffres de dégâts TMP ;
  - l'aura des drops (`vfxGraph_Drop`) ;
  - toute l'UI UGUI.
- **Pipeline de rendu.** L'original tournait sur la qualité Medium : `UniversalRP-MediumQuality`, sans HDR, sans texture de profondeur ni d'opaque, sans ombres douces, sans ombres des lumières additionnelles, ombres jusqu'à 50 m, 4 lumières par objet. `dev` a tout ça activé, plus SSAO et Outline dans le renderer. Le bloom, les particules douces et la distorsion en dépendent : le rendu d'origine demande **son propre asset URP**.
- **Level art des salles.** Le gros des différences vient du refactor des tuiles (les FBX sont devenus des instances de `Tile.prefab`) et des plantes (les FBX du cave pack sont devenus des prefabs `Nature`, même mesh). Ces différences ne se voient pas. Les vraies différences visibles :
  - les lampes du cave pack (`ZLPC_Lamp_01_2`, `ZLPC_Lamp_02`, `ZLPC_Lamp_02_2`, `ZLPC_Lamp_03`, `Lamp_01`, 9 au total) remplacées par les lanternes (`Lantern`, `LanternCurved`, `LanternArm`, 9) ;
  - les gouttes (`WaterDrip`, 5) et l'eau refaite (`Water`, 5) ;
  - l'herbe (`GrassPatch`, objets dépaquetés dans les salles) ;
  - les plantes qui bougent (vent de `NatureLit`, `WindAnchor`) ;
  - les éventuels déplacements de props, à mesurer par salle (étape 4).
- **Ambiance ajoutée** : neige proche, impacts de neige, poussière, brume, volume de la faille, neige au sol, lumière de la faille, vent, reflets et ondes de l'eau. Tout est dans `Prefabs/Environment/Snow.prefab` (dans `GameRig`) et `WaterReflection` (dans `Gameplay.prefab`). L'original n'avait que le système de particules `Snow`.

## Principes

1. **Un seul code de jeu.** Aucun `if (classic)` dans le gameplay ni dans les vues. L'édition passe par les cinq mécanismes décrits plus bas : la table d'assets, les objets propres à une édition, le rendu, le profil d'édition et le thème UI. Tout autre besoin est une exception à justifier ici.
2. **L'Originale est fidèle, bugs de rendu compris.** Ses shaders sont ceux de l'original, pas des ports. Un effet qui ne marchait pas en 2022 ne marche toujours pas : le GrabPass de `SphericalDistortion` ne fait rien sous URP, et les particules douces sans texture de profondeur non plus. On ne réimplémente un système d'origine que s'il n'existe plus du tout et se voit (le contour au survol, les chiffres de dégâts). On le fait alors par le système moderne, avec le look d'origine.
3. **Toutes les améliorations d'UX restent dans l'Anniversary.** L'Originale sert aussi à montrer ce que l'Anniversary apporte (voir [UX](#ux)). Seule exception : ce qu'exige une mécanique commune aux deux éditions et qu'on ne peut pas retirer sans casser le jeu. Elle est listée et justifiée.
4. **Le switch se fait en direct**, derrière le volet de transition des salles : le volet couvre l'écran, l'édition s'applique, le volet se retire. Tout consommateur sait donc s'appliquer dans les deux sens, à tout moment.
5. **Les assets de `main` n'écrasent jamais ceux de `dev`.** Les versions d'origine vivent à part, sous `Assets/Art/Classic/`, avec un GUID neuf quand celui d'origine est pris.
6. **Tout nouveau visuel ou toute nouvelle aide de l'Anniversary déclare son comportement en Originale** : une paire dans la table, ou le marquage Anniversary seulement. C'est ce qui garde les deux éditions maintenables (voir [Maintenance](#maintenance)).

## Branche

Tout le chantier se fait sur la branche `edition-classique`, créée depuis `dev`.
- On y fusionne `dev` régulièrement, jamais l'inverse.
- Elle revient dans `dev` une fois tout testé : chaque salle dans les deux éditions, un run complet dans chacune, le switch dans les deux sens.
- Un visuel ajouté sur `dev` pendant le chantier se traite au merge suivant : on lui ajoute une paire ou on le marque Anniversary seulement.

## Architecture

### `Edition` (Core)

- `enum GameEdition { Anniversary, Classic }` et `static class Edition` : `Current`, `IsClassic`, l'event `Changed`, `Set(GameEdition)`, plus un `ResetStatics`.
- Le choix est enregistré par `GameSettings` (`GameSetting.Edition`, un switch, clé `Edition`, Anniversary par défaut). `GameSettings.Load` l'applique **avant** le premier chargement de salle : une partie lancée en Originale ne montre jamais une image Anniversary.
- **Switch** (fait) : les options appellent `SceneTransition.Play(() => Edition.Set(edition), …)`. Le volet des chargements de scène, le même que celui des salles, couvre tout l'écran au-dessus du menu et de la pause, en temps non mis à l'échelle. `Edition.Set` s'exécute pendant qu'il est fermé, puis le volet se retire une frame plus tard. Pas d'attente de la file d'actions : échanger des matériaux ou des objets en plein cast ne gêne pas le jeu, et la pause fige la file.

### Mécanisme 1 : la table d'assets `EditionSkin`

Un ScriptableObject `Assets/Data/Editions/ClassicSkin.asset`, référencé par `GameAssets`. C'est le seul endroit qui connaît les deux éditions.

- `materials` : des paires `{ anniversary, classic }` pour tout ce qui est partagé, c'est-à-dire le décor, les tuiles, la végétation, l'eau, les cristaux et **les matériaux des VFX** qui utilisent un des 12 shaders Amplify (la copie d'origine du matériau pointe vers le shader d'origine).
- `prefabs` : des paires pour les prefabs que du code instancie et qu'on remplace en bloc (les auras de drop de `Collectable`), via `EditionSkin.Resolve(prefab)`.
- La recherche marche dans les deux sens : c'est ce qui permet le switch en direct.

`EditionMaterials.Apply(GameObject root)` remplace, sur chaque `Renderer` sous `root` (particules comprises), les `sharedMaterials` qui ont une paire. Il est appelé à trois endroits :
- dans `RoomInfo`, après l'`Instantiate` de la salle ;
- dans `EnemySpawnPoint`, après celui de l'entité ;
- dans `VFXPool`, après celui d'un VFX, une seule fois par instance puisque le pool les réutilise.

Sur `Edition.Changed`, il repasse sur la salle courante, les entités vivantes et les instances du pool.

**`EntityLook`** : sur les prefabs des 8 entités, les tableaux complets de matériaux d'origine, renderer par renderer et slot par slot. Il est nécessaire parce que le nombre de slots diffère entre les éditions (Nanuko en surcharge 5 contre 3, Kameiko 2 contre 1 : les slots en trop reprennent les matériaux embarqués du FBX) et parce que des matériaux Anniversary sont partagés (`EnemyEyes`). Il garde aussi les noms des propriétés que `PlayerGlow` écrit (`_LaserColor` en Originale, comme l'ancien `ChangeColor.cs`).

### Mécanisme 2 : `EditionOnly`

`EditionOnly { GameEdition edition; GameObject[] objects; Behaviour[] behaviours; }` active ou désactive des objets ou des composants selon l'édition, sur `OnEnable` et sur `Edition.Changed`. Les systèmes concernés ne connaissent pas l'édition.

- **Pour** : un ajout cosmétique, dont l'absence ne change pas le déroulement du jeu.
- **Pas pour** : un système qui porte une étape du jeu, comme la disparition du cadavre. Celui-là lit le profil d'édition.

**Level art des salles : `RoomEditionArt`**, une variante d'`EditionOnly` posée sur la racine de chaque salle. Elle a deux listes :
- `anniversaryOnly` : l'herbe, les lanternes, les gouttes, les plantes absentes de l'original ;
- `classicOnly` : les lampes du cave pack et les props de l'original, restaurés à leur place d'origine, désactivés en Anniversary.

Un outil d'éditeur la remplit (voir [Outillage](#outillage)).

### Mécanisme 3 : le rendu

- **Asset URP d'origine.** `Rendering/URPSettings/UniversalRP-Classic.asset` reprend les réglages du Medium de `main` (pas de HDR, pas de texture de profondeur ni d'opaque, ombres dures, pas d'ombres des lumières additionnelles, 50 m, 4 lumières par objet). Son renderer `ClassicRenderer` n'a aucune feature. `Edition` applique `QualitySettings.renderPipeline = classic` ou le remet à `null` (le pipeline de la qualité). On ne modifie jamais l'asset Anniversary au runtime : l'éditeur garderait la modification après le Play.
- **Post-process.** `ClassicVolume` est ajouté dans le rig, avec une priorité au-dessus de `GameVolume`. Il ne contient que les différences avec l'original : vignettage 0.25, motion blur actif. Un `EditionOnly` l'active. `GameVolume` ne change pas, donc la luminosité et le contraste des options continuent de s'appliquer. Le menu garde son profil actuel : la profondeur de champ d'origine floutait tout l'écran, ce qui est refusé.
- **Lumière.** `RiftLighting` garde les valeurs d'origine de la lumière et les restaure dans son `OnDisable`. Il est désactivé en Originale.
- **HDR.** Les couleurs HDR des matériaux Anniversary n'ont pas d'importance, puisque les matériaux d'origine reviennent avec l'asset d'origine. Le bloom du profil de jeu, lui, change d'aspect sans HDR : c'est voulu, c'était le rendu de 2022.

### Mécanisme 4 : `EditionProfile`

Deux ScriptableObjects, `AnniversaryProfile` et `ClassicProfile`, exposés par `GameAssets.Edition` selon `Edition.Current`. Ils ne contiennent que des réglages, pour les systèmes qui ne peuvent pas être coupés en bloc :
- les durées du hit stop, la force du tremblement, la durée de disparition des cadavres ;
- des booléens d'UX : `showThreatTiles`, `showPath`, `queueCasts`, `confirmEndTurn`, `refuseFeedback`…

Un système qui lit le profil ne teste jamais l'édition lui-même. Un besoin qui dépasse un réglage passe par `EditionOnly` ou par une paire.

### Mécanisme 5 : le thème UI

- **La classe `classic`** est posée sur la racine de chaque document UI (HUD, inventaire, résultats, pause, menu principal, splash, `SceneTransition`), par `MenuScreen.Setup`, et suit `Edition.Changed`.
- **`Styles/Classic.uss`** est importé en dernier par `Theme.tss`, avec des règles sous `.classic` uniquement.
  - **Tokens.** `.classic { --color-accent: #D22F45; --font-display: …; }` : les propriétés custom héritent, donc toute l'UI suit.
  - **Cadres.** Les ~35 sprites d'origine (`Art/Classic/UI/`) passent en `background-image` et `-unity-slice-*` sur les classes existantes.
  - **Aides d'UX.** Leurs éléments sont masqués (`display: none`), voir [UX](#ux).
  - **Placement.** Si l'écart gêne, on déplace par USS vers les positions d'origine : barre de sorts, timeline, minimap, fiche du joueur.
- **Composants.** `CutShape` lit `--cut-shape` (`none` en Originale) et saute son dessin (biseau, trait, flou) pour laisser le fond USS. Il ne connaît toujours pas l'édition.
- **Ce qui reste Anniversary.**
  - La structure des écrans : l'UI d'origine en UGUI ne revient pas, et deux UI seraient deux codes.
  - La forme des pièces d'inventaire : `ArtifactPiece` les dessine, les sprites tetris d'origine sont écartés.
  - Le volet de transition.

## Rendu du monde : shaders, particules, neige, volumétrie, eau

| Élément | Anniversary | Originale |
|---|---|---|
| Neige proche, impacts, poussière, brume (`Mist.shader`), volume de la faille (`RiftVolumetrics.shader`) | enfants `SnowNear`, `SnowImpact`, `Dust`, `Mist`, `RiftVolume` de `Snow.prefab` | `EditionOnly` : désactivés |
| Neige de fond | enfant `Snow` | Variante d'origine : les réglages de `main:Assets/Resources/Prefabs/Snow.prefab` (densité, taille, vitesse, matériau) dans un enfant `SnowClassic`, l'un ou l'autre actif |
| Neige au sol | `SnowCover`, `SnowHeight.shader`, `SnowHeat` | Désactivés ; les matériaux d'origine ne la lisent pas |
| Vent, plantes qui bougent | `Wind`, rafales, `WindAnchor`, balancement de `NatureLit` | `Wind` désactivé ; les plantes reprennent leurs matériaux d'origine, statiques |
| Herbe | `GrassPatch` | `RoomEditionArt.anniversaryOnly` |
| Lanternes | `Lantern*`, `LightFlicker` | Anniversary seulement ; les lampes du cave pack reviennent en `classicOnly` |
| Lumière de la faille | `RiftLighting` | Désactivée, lumière restaurée |
| Eau | `WaterSurface` + `Water.shader`, `WaterReflection`, `WaterRipples`, `WaterSplash`, `WaterDrip` | Reflet, ondes, éclaboussures, gouttes désactivés. Surface avec **les matériaux d'origine** (`example-water-02`, `water_drareg`) et le graph Bitgem `WaterVolume-URP.shadergraph`, restaurés. Les scripts Bitgem ne reviennent que si le graph a besoin du mesh de `WaterVolumeBox` (spike 0.5) |
| Décor, roches, grotte | `SnowLit` (`Mat_SnowCave`, `Mat_SnowRock`, `Mat_SnowRock_Alt`, `Mat_SnowProps`, `Mat_BlackRock`) | `ZLPC_Cave`, `ZLPC_CaveSnow0/1` (`Snow.shadergraph`), `ZLPC_Prop`, `PathCreator Black` |
| Végétation | `NatureLit` (`Mat_Nature_*`, `Mat_SnowPlants_Cave`) | `ZLPC_Plant`, `ZLPC_Mushroom`, `MAT_Lifetree`, `MAT_Deadtree`, `Glowplant*`, `MAT_Plante`, `MAT_Leaf`, `MAT_Tree`, `MAT_Crystal` (`Crystal.shadergraph`), `MAT_SnowTree` |
| Cristaux | `Mat_MagicCrystal` | Matériau d'origine des cristaux du cave pack |
| Tuiles | `Mat_LightBlue_Rock`, `Mat_DarkBlueRock` sur `SnowLit` | Versions `main` (URP Lit) |
| Overlays de cases | `glowtile_*` en URP Particles/Unlit, `TileOverlayGlow.png` teinté | Versions `main` (shader intégré, `Case Glow.png` blanc) |
| Grille, chemin, cases menacées, anneaux | `CombatGrid`, `PathLine`, `glowtile_threat`, `EntityRing` | Retirés (UX) |
| VFX des capacités | 12 shaders Amplify portés en HLSL | **Shaders Amplify d'origine** restaurés sous de nouveaux GUID (`Art/Classic/Shaders/`), avec leurs matériaux copiés et appariés dans `materials`. Concernés : `Add_CenterGlow`, `Blend_CenterGlow`, `DeformPaw`, `Deform`, `DissolveNoise`, `NDissolveNoise`, `Aura`, `SmashWave`, `Wind`, `Cutout`, `GlowCutout`, `SphericalDistortion`. Les 7 VFX Graphs restent les mêmes : leur mise à jour est automatique, et Unity 6 ne relit plus la version d'origine |
| `LightningExecution` | `LightningIntegrated.shader`, inchangé | Identique : c'est le shader d'origine |
| SSAO, contour | renderer Anniversary | Renderer d'origine sans features |

**Restaurer un shader d'origine.** Les `.shader` générés par Amplify sont du texte : le plugin n'est pas nécessaire pour les compiler. Les Shader Graphs de `main` (URP 10) sont mis à jour à l'import. Chaque shader restauré est ouvert pour vérifier qu'il compile, puis comparé visuellement.

## Entités

| Entité | Originale (restaurée depuis `main`) |
|---|---|
| Joueur | Tenue : copies de `GlowMasque`, `GlowVeste`, `GlowBotte` sur `VFX_GlowGun.shadergraph` (GUID `4a5375…` libre, restauré tel quel). Corps et armes identiques |
| Drareg, phases 1 et 2 | Arme : copie de `GlowBlue 1` sur `VFX.shadergraph`. Le reste est identique |
| Golem | `MAT_OrigineGolem`, `Crystal.shadergraph`, `GolemUV.png`, `Bigtesty3/6.png`, `UI/Fade.png`, `Ramp2`, `Blackramp`, `Noise63` |
| Kameiko | `GlowBlue.mat` (Glow shadergraph) et le matériau embarqué du FBX |
| GreatKameiko | `WhiteGlow.mat`, `GreatNanuko.mat` (+ `OBS_Albedo.tif`), ancienne aura (`Smoke21bcg.mat` + `VFXSphereICO_01.fbx`) en `EditionOnly` Originale |
| Nanuko | `WhiteBearMat.mat`, `GlowBear.mat` ×2 (+ `Color_Bear.jpg`), les 2 matériaux embarqués du FBX |
| GreatNanuko | `GreatNanuko.mat`, `WhiteGlow` ×2, ancienne aura |
| Tous | Désactivés : `EnemyGlow`, `EnemyWisps`, `EntityRing`, `FootstepDust`. `FootIK` est une correction, pas un effet : gardé |

Modèles, rigs, avatars et clips identiques : aucun travail d'animation.

## Feedbacks

| Feedback | Originale |
|---|---|
| Flash au coup (`HitFlash`), bris d'armure (`ArmorBreakFeedback`), soins et armure (`RecoveryFeedback`), mort en particules (`DeathFeedback`), focus de caméra (`TurnCameraFocus`) | Retirés (`EditionOnly`) |
| Recul (`EntityFeedback`), tremblement, zoom et hit stop (`ImpactFeedback`) | Nuls, par le profil |
| Disparition du cadavre | Instantanée ou presque, par le profil (l'original détruisait l'entité tout de suite) |
| Particule d'impact (`EntityHit`), dissolution de Drareg phase 2 | Identiques à l'original : gardées |
| Contour | L'original n'en avait qu'au survol de la timeline : `EntityOutline` s'affiche seulement dans ce cas (profil), avec la couleur et l'épaisseur de QuickOutline. Le renderer d'origine a besoin d'une passe de contour : soit une feature Outline seule sur `ClassicRenderer`, soit le repli sur QuickOutline restauré (spike 0.3) |
| Chiffres de dégâts | `CombatPopups` avec le style d'origine (police, couleur, pas de taille variable ni de cumul), par USS et le profil |
| Aura des drops | Les 4 prefabs `Common/Rare/Epic/LegendaryDrop` d'origine avec `vfxGraph_Drop.vfx`, restaurés sous de nouveaux GUID, appariés dans `prefabs` |
| Compte à rebours de l'ultime de Drareg | Bug commun : restaurer `OldCataIndicatorState1-3` sous leurs GUID d'origine, ce qui répare les deux éditions |

## UX

Première liste, faite en comparant le HUD de `main` (`UI.prefab`, scripts `UI/`) avec celui de `dev`. Elle est à confirmer à l'étape 7, écran par écran.

| Aide | L'original l'avait | Originale |
|---|---|---|
| Timeline, minimap, barre de sorts, fiche du joueur (stats), buffs, infos de l'entité survolée, tooltip d'artefact, inventaire, pause, résultats et score | Oui | Gardé, habillé |
| Cases menacées par l'ennemi survolé, ligne de chemin, grille de combat, anneaux sous les entités | Non | Retiré (profil, `EditionOnly`) |
| Aperçu des dégâts, coût d'énergie prévu | Non | Masqué (USS) |
| Tooltips des stats, des statuts, de la timeline ; la timeline qui pointe le plateau | Non | Retirés : `HudTooltip` n'enregistre que ceux du profil |
| Bannières (combat, tours, victoire), barre du boss | Non | Masquées (USS) |
| Vignette de basse vie, battement de cœur | Non | Masqués ; son coupé (audio) |
| Refus : tremblement, couleur, son | Non | Coupés (profil `refuseFeedback`, USS) |
| Battement et confirmation du bouton de fin de tour | Non | Coupés (profil `confirmEndTurn`, USS) |
| Confirmation au second clic des boutons Menu principal et Quitter | Non | Coupée (profil) |
| **File de casts** (lancer plusieurs sorts à la suite, avec leurs marqueurs) | Non | **Coupée** (profil `queueCasts` : un clic pendant un cast est ignoré, comme dans l'original). C'est une mécanique, pas un visuel : à valider en jeu |
| Onglets des options, vitesse de jeu, force du tremblement | Non | Gardés : ce sont des options, pas le jeu |

## Audio

Les sons sont identiques ; seul le mix change.
- Un Game Parameter `Edition` (0 ou 1), posé par `Edition.Changed`, pilote dans Wwise trois choses :
  - le **Bypass** de l'`ImpactMeter` du bus `Impacts` : plus de baisse de la musique sur les coups ;
  - les courbes de `LowHealth` et `PlayerHurt` sur le low-pass du bus `Music`, remises à plat ;
  - le volume de `Heartbeat`, coupé.
- La musique, ses pistes et ses transitions sont identiques.

## Outillage

- **`tools/restore_from_main.py <chemin main> <dossier dev>`**, documenté dans [editor tooling](../tech/editor-tooling.md). Il lit l'asset et son `.meta` dans `main`.
  - GUID libre dans `dev` : l'asset est restauré avec son `.meta` d'origine, et les références de `main` marchent telles quelles. C'est le cas des shaders supprimés, des textures supprimées et de `OldCataIndicatorState*`.
  - GUID pris : l'asset est écrit sans `.meta`, pour que Unity lui donne un GUID neuf. Les références internes des copies restaurées ensemble sont réécrites.
  - Dans les deux cas, il suit les dépendances manquantes et les propose. Les assets vont dans `Assets/Art/Classic/<catégorie>/`, suffixés `_Classic` en cas d'homonyme.
- **`tools/room_edition_diff.py`**. Pour chaque salle, il compare les objets de `main` et de `dev` : source (prefab ou mesh), position, rotation, échelle.
  - Il ignore les équivalences connues : FBX de tuile ↔ `Tile.prefab`, FBX du cave pack ↔ prefab `Nature` de même mesh.
  - Il sort, par salle : les objets ajoutés, les objets retirés et les objets déplacés.
  - Un `run_script` d'éditeur remplit ensuite `RoomEditionArt` : les ajouts en `anniversaryOnly` ; les retraits recréés à leur place d'origine en `classicOnly` (les lampes du cave pack) ; les déplacements traités par une instance d'origine en `classicOnly` et l'actuelle en `anniversaryOnly`.
- **`EditionCoverage`** (menu Tools, appelable par `run_script`). Il liste :
  - les matériaux utilisés par les salles, entités et VFX, ni appariés ni identiques à l'original ;
  - les prefabs d'effet ou d'ambiance sans `EditionOnly` ;
  - les salles dont `RoomEditionArt` est en retard sur `room_edition_diff`.
- **Sonde `Playtest.cs`** : `edition --set classic|anniversary`.

## Étapes

Chaque étape compile et se commite sur `edition-classique` avec sa doc. Le test en jeu se fait dans `RoomGallery` et `Tests/EnemyShowcase`.

### Étape 0 : spikes

Chacun se fait sans rien brancher.
1. **Un ennemi d'origine** : `GlowBear.mat`, `VFX_GlowGun.shadergraph` et `Color_Bear.jpg` posés à la main sur Nanuko, puis `Crystal.shadergraph` sur le Golem. Le rendu sous URP 17 est-il celui de 2022 ? C'est le risque principal.
2. **Un shader Amplify d'origine** (`Add_CenterGlow`) sur une copie de matériau de VFX : compile-t-il sous Unity 6 ?
3. **Asset URP d'origine** posé par `QualitySettings.renderPipeline` en direct : pas de saut ni d'erreur ? Le contour au survol est-il possible sans feature, ou avec une feature Outline seule ?
4. **UI** : la classe `classic` sur la racine du HUD. Les tokens hérités sont-ils relus en direct par `CutShape` ? `--cut-shape: none` et un fond 9-slice sur un `SlantedButton`.
5. **Eau** : `example-water-02` et `WaterVolume-URP.shadergraph` sur un `WaterSurface`. Le graph a-t-il besoin du mesh Bitgem ?

Si un Shader Graph de 2020 est irrécupérable, le repli est de le recréer en HLSL dans `Assets/Rendering/Classic/`, avec le même rendu. C'est à estimer avant de continuer.

### Étape 1 : fondations

**Fait**, dans le premier commit de la branche :
- `Edition`, enregistrée dans ses propres PlayerPrefs plutôt que par `GameSettings`.
- L'option dans la page Jeu (clés `OptionsEdition`, `EditionAnniversary`, `EditionClassic`) et le switch derrière le volet.
- `EditionOnly`, `EditionProfile` (deux assets), `EditionSkin` (vide).
- `EditionMaterials.Apply` branché dans `RoomInfo`, `EnemySpawnPoint`, `VFXPool`, et appliqué à chaque scène chargée (ce qui couvre le joueur).
- La classe `classic` posée par `MenuScreen.Setup`.
- La sonde `Editions`.
- La doc `docs/features/editions.md`.

### Étape 2 : rendu et ambiance

L'asset URP d'origine, `ClassicVolume`, `RiftLighting` qui restaure sa lumière, les `EditionOnly` de `Snow.prefab` et `SnowClassic`, `SnowCover`, `Wind`, l'eau (ses effets), `WaterReflection`.

### Étape 3 : audio

Le Game Parameter `Edition`, les courbes dans Wwise, puis la régénération des banques.

### Étape 4 : salles

`restore_from_main.py` et `room_edition_diff.py`, puis :
- les matériaux du décor, des tuiles et de l'eau dans la table ;
- le remplissage de `RoomEditionArt` salle par salle : herbe, lanternes et gouttes en Anniversary, lampes d'origine en Originale, déplacements.

### Étape 5 : entités et VFX

- `EntityLook` sur les 8 entités, avec les shaders et matériaux restaurés.
- Les 12 shaders Amplify d'origine et les copies de leurs matériaux dans la table.
- Les auras des Great.

### Étape 6 : feedbacks

Les `EditionOnly` et le profil, le contour au survol de la timeline, les chiffres de dégâts, les auras de drop, et la réparation du compte à rebours de Drareg.

### Étape 7 : UX et UI

- La liste [UX](#ux) confirmée écran par écran.
- Les réglages du profil branchés : `showThreatTiles`, `showPath`, `queueCasts`, `confirmEndTurn`, `refuseFeedback`, les tooltips.
- `Classic.uss`, `--cut-shape` et les sprites d'origine.

### Étape 8 : doc, contrôle et retour sur `dev`

- `docs/features/editions.md` (mécanismes, table, règle de maintenance), listé dans `docs/README.md` et dans le tableau de `CLAUDE.md`.
- La règle dans `CLAUDE.md` et dans [conventions](../tech/conventions.md).
- `doc_check.py` qui connaît `Art/Classic` et `Data/Editions`.
- `EditionCoverage` sans alerte.
- Un run complet dans chaque édition, le switch depuis le menu et depuis la pause, puis le merge dans `dev`. Ce plan est alors retiré : la doc de la feature le remplace.

## Maintenance

- **Nouveau visuel, effet, ambiance ou aide d'UX Anniversary** : marqué Anniversary seulement (le cas par défaut), ou apparié. `EditionCoverage` le signale sinon.
- **Nouvelle capacité, nouvel ennemi, nouvel écran** : pas d'équivalent d'origine. Il s'affiche en Originale avec son look, sans ses effets `EditionOnly`, et l'UI suit les tokens.
- **Bug de gameplay** : corrigé une fois, pour les deux éditions.
- **À ne jamais écrire** : un test d'édition dans une vue, une action ou un système de jeu. C'est un réglage du profil, un `EditionOnly` ou une paire.

## Décisions

| Question | Décision |
|---|---|
| Nom de l'option | « Édition : Anniversary / Originale » (proposé) |
| Choix au premier lancement | Anniversary par défaut, l'option dans la page Jeu (proposé) |
| Switch pendant un combat | Oui, derrière le volet des salles, une fois la file d'actions vide |
| Shaders | **Décidé** : ceux de l'original, y compris les 12 Amplify des VFX |
| Level art | **Décidé** : celui de l'original ; herbe, lanternes, gouttes et plantes qui bougent en Anniversary seulement |
| UX | **Décidé** : toutes les améliorations en Anniversary seulement |
| Eau | **Décidé** : les matériaux et le shader d'origine |
| Profondeur de champ du menu | **Décidé** : non |
| Structure de l'UI | L'UI Toolkit habillée ; l'UGUI ne revient pas |
| Branche | **Décidé** : `edition-classique` depuis `dev` |
