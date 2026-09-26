# À faire

Ce qu'on garde pour plus tard. On ajoute une ligne quand on repère quelque chose qu'on ne fait pas tout de suite, et on la supprime dès que c'est fait, dans le même commit.

## Eau

- **Son des gouttes** : `WaterDrip` a un champ `AK.Wwise.Event` vide, et le projet n'a aucun event de goutte ni d'eau. Il faut le créer dans Wwise, puis le brancher sur `Prefabs/Environment/Water/WaterDrip.prefab` (skill `wwise-events`). On peut ajouter un ambiant d'eau clapotante par bassin visible.
- **Bords des bassins** : dans CombatRoom10, le chenal de droite s'arrêtait net en bord d'écran (il a été allongé). Vérifier les autres salles qui ont de l'eau pour la même coupure, dans la `RoomGallery`.

## Ambiance

- **Brume** : un plan de brume sous les cases (bancs qui dérivent avec le vent dans `RiftVolumetrics`, particules `Mist.shader`) attend d'être validé. Elle peut aussi couvrir les bassins, au ras de l'eau.

## Nettoyage

- **`VFX_WaterBlade.prefab`** (`Art/VFX/WaterBlade`) : aucun asset ne le référence. À supprimer s'il ne sert plus.
