# Plan : eau, ce qui reste à faire

Plan de travail temporaire (septembre 2026). La nouvelle eau est en place et documentée dans [map.md](../features/map.md#water). On supprime ce fichier quand la liste est vide.

## Reste à faire

1. **Gouttes des stalactites** : `WaterDrip` est écrit mais n'est posé nulle part. Il faut :
   - un prefab `Prefabs/Environment/Water/WaterDrip.prefab`, avec une particule de goutte qui tombe avec la gravité et le masque du layer `Water` ;
   - le poser sous les stalactites au-dessus de l'eau, à repérer en capture ;
   - un son Wwise facultatif, avec le skill `wwise-events`.
2. **Courants et ruisseaux** : ils sont conditionnels. Il faut analyser les bords de map des salles qui ont de l'eau, et ne les faire que si un filet d'eau peut filer hors champ sans traverser le plateau. Sinon, on abandonne l'idée.
3. **Bords des bassins** : dans CR10, le grand bassin (« Cube (2) ») a été allongé de 10 m vers +X, parce que le chenal continuait. Il faut vérifier les autres salles pour la même coupure, en zoomant sur les bords de l'écran.
4. **Polish**, à mesurer une par une. Le reflet qui se trouble avec les rides est déjà fait.
   - Des scintillements fins sur les rides là où une lumière frappe.
   - Un bord mouillé : une bande sombre et brillante sur la roche juste au-dessus de la ligne d'eau, dans Snow Lit, à partir d'une globale des hauteurs d'eau.
   - Une brume basse au ras de l'eau, qui dérive avec le vent.
   - Les corps immergés un peu flous avec la profondeur.
   - Quelques anneaux au passage d'une rafale.
   - Un ambiant d'eau clapotante par volume visible.
