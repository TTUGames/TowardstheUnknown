# Plan : eau, ce qui reste à faire

Plan de travail temporaire (septembre 2026). La nouvelle eau est en place et documentée dans [map.md](../features/map.md#water). On supprime ce fichier quand la liste est vide.

## Reste à faire

1. **Bords des bassins** : dans CR10, le grand bassin (« Cube (2) ») a été allongé de 10 m vers +X, parce que le chenal continuait. Il faut vérifier les autres salles pour la même coupure : l'utilisateur les teste dans la `RoomGallery`.
2. **Sons** : `WaterDrip` a un champ `AK.Wwise.Event` vide. Il faut un event de goutte, et éventuellement un ambiant d'eau clapotante par bassin visible, à créer dans Wwise (skill `wwise-events`).
3. **Brume au ras de l'eau** : laissée au chantier brume (session dédiée, voir son plan) ; sa brume peut couvrir les bassins.
