"""The data of the sound brief: every placeholder sound to replace, and the clips that show them"""

# Each placeholder: Wwise event, French name, the file(s) to overwrite (Originals/SFX/...), target length, intention, bus,
# voices = the Wwise sound names that play it (to find it in the clips)
SOUNDS = [
    # ---- Orbes
    dict(key='PickOrb_Common', group='Orbe de coffre', name='Orbe commune', files=['Loot/PickOrb_Common.wav'], length='~0,5 s', bus='SFX',
         when="Le joueur marche sur une orbe de rareté commune : elle éclate dans sa couleur, l'inventaire s'ouvre 0,1 s plus tard (OpenInventory, son définitif).",
         intent="Éclat court et sec, discret. Le son de base dont les trois autres sont des versions enrichies. Cristallin, un peu surnaturel : l'orbe est une sphère de verre sombre qui « glitche ». Ne pas masquer l'ouverture de l'inventaire juste après."),
    dict(key='PickOrb_Rare', group='Orbe de coffre', name='Orbe rare', files=['Loot/PickOrb_Rare.wav'], length='~0,8 s', bus='SFX',
         when="Même moment, orbe rare (bleue).", intent="Même éclat, avec une première couche brillante et un peu de queue."),
    dict(key='PickOrb_Epic', group='Orbe de coffre', name='Orbe épique', files=['Loot/PickOrb_Epic.wav'], length='~1,2 s', bus='SFX',
         when="Même moment, orbe épique (violette).", intent="Plus large et plus grave, une résonance qui s'installe."),
    dict(key='PickOrb_Legendary', group='Orbe de coffre', name='Orbe légendaire', files=['Loot/PickOrb_Legendary.wav'], length='~1,5 à 2 s', bus='SFX',
         when="Même moment, orbe légendaire (dorée).", intent="Le moment fort : impact, scintillement, longue queue. On doit reconnaître une légendaire à l'oreille et avoir envie d'en retrouver une."),
    dict(key='Relic_Hover', group='Orbe de coffre', name="Survol d'une orbe", files=['Loot/Relic_Hover.wav'], length='~0,3 à 0,6 s', bus='SFX',
         when="La souris arrive sur une orbe posée au sol (ou sur sa case) : elle cesse de glitcher et s'illumine, ses failles s'allument, une onde fine part autour d'elle et elle lâche quelques fragments. Le même son pour toutes les raretés. Pas filmé.",
         intent="Une attirance : l'orbe « remarque » le joueur. Cristallin et surnaturel, de la même matière que les PickOrb mais plus léger, sans éclat : une promesse, pas encore la récompense. Supporte d'être rejoué souvent (va-et-vient de la souris)."),
    # ---- Ambiances
    dict(key='Ambience_Cave', group='Ambiances', name='Ambiance grotte', files=['Ambience/Ambience_Cave.wav'], length='boucle 60 s et plus', bus='SFX', loop=True,
         when="En continu dans les salles de type grotte (le spawn, les salles au trésor, la plupart des salles de combat). Fondu enchaîné de 2 s en changeant de lieu, derrière le volet de transition.",
         intent="Fond de faille enneigée : air froid, réverbération de roche, vent lointain et étouffé. Les salles sont au fond d'une faille, sous la neige qui tombe. Boucle sans couture, sans événement reconnaissable qui reviendrait à chaque tour. La musique joue par-dessus en permanence : laisser de la place. (Le placeholder actuel est l'ancienne ambiance unique du jeu.)"),
    dict(key='Ambience_Cliff', group='Ambiances', name='Ambiance falaise', files=['Ambience/Ambience_Cliff.wav'], length='boucle 60 s et plus', bus='SFX', loop=True,
         when="En continu dans les salles en falaise : Combat Room 02, 08, 09, 11, 16 et 18.",
         intent="Lieu ouvert et exposé : vent plus présent, rafales, sensation de vide en contrebas. Mêmes contraintes de boucle et de place pour la musique."),
    dict(key='Ambience_Dream', group='Ambiances', name='Ambiance onirique (jardin de Drareg)', files=['Ambience/Ambience_Dream.wav'], length='boucle 60 s et plus', bus='SFX', loop=True,
         when="En continu dans l'antichambre et la salle du boss.",
         intent="Le jardin de Drareg : pas de neige ni de vent froid ici. Calme, irréel, suspendu, une eau verte qui luit."),
    dict(key='Ambience_Water', group='Ambiances', name="Couche d'eau", files=['Ambience/Ambience_Water.wav'], length='boucle 30 s et plus', bus='SFX', loop=True,
         when="Par-dessus l'ambiance du lieu, dans toute salle qui a un bassin (grotte, falaise ou onirique). Jouée à -6 dB.",
         intent="Eau calme de bassin souterrain : clapotis léger, pas de cascade ni de rivière. Doit se poser sur les trois ambiances sans les brouiller. Les gouttes des stalactites ont leur propre son (Water_Drip) : ne pas les mettre dans la boucle."),
    dict(key='Water_Drip', group='Ambiances', name='Goutte de stalactite', files=['Map/Water_Drip_1.wav', 'Map/Water_Drip_2.wav', 'Map/Water_Drip_3.wav'], length='~0,3 à 0,5 s', bus='SFX',
         when="Ponctuel : une goutte tombe d'une stalactite et touche l'eau d'un bassin, toutes les quelques secondes, de plusieurs stalactites (Combat Room 10, salle du boss).",
         intent="Une goutte qui tombe dans une eau calme et profonde, avec un peu d'écho de grotte. Trois variations jouées au hasard : on les entend souvent, elles ne doivent pas lasser."),
    # ---- Déploiement
    dict(key='Deploy_TileHover', group='Phase de déploiement', name="Survol d'une case de départ", files=['UI/Deploy/Deploy_TileHover_1.wav', 'UI/Deploy/Deploy_TileHover_2.wav', 'UI/Deploy/Deploy_TileHover_3.wav'], length='< 0,1 s', bus='SFX',
         when="Avant chaque combat, quand la souris passe sur une des cases de départ proposées (en orange).",
         intent="Tic très léger. Joué souvent et rapidement quand la souris balaie les cases : ne doit jamais fatiguer. Trois variations jouées au hasard."),
    dict(key='Deploy_TileSelect', group='Phase de déploiement', name="Sélection d'une case", files=['UI/Deploy/Deploy_TileSelect.wav'], length='~0,2 s', bus='SFX',
         when="Clic sur une case de départ : le personnage s'y place.", intent="Confirmation nette, plus posée que le survol."),
    dict(key='Combat_Start', group='Phase de déploiement', name='Début du combat (« Déployer » et bannière « Combat »)', files=['UI/Banner/Combat_Start.wav'], length='~0,5 à 0,9 s', bus='SFX',
         when="Clic sur « Déployer » : le combat commence, la bannière « Combat » glisse au centre (reste 0,9 s, s'efface en 0,25 s) et la musique passe en combat juste après. Un seul son pour le clic et la bannière, qui arrivent toujours ensemble. Le clic de bouton générique (Button_Click) joue en même temps.",
         intent="Un son d'engagement, avec du poids : l'annonce du combat, plus marquée que les bannières de tour qui suivent."),
    # ---- Bannières
    dict(key='Banner_PlayerTurn', group='Bannières de combat', name='Bannière « Votre tour »', files=['UI/Banner/Banner_PlayerTurn.wav'], length='~0,5 à 0,9 s', bus='SFX',
         when="Au début de chaque tour du joueur. Même animation (0,9 s puis 0,25 s).",
         intent="Un « à toi de jouer » clair, entendu à chaque tour : court et pas envahissant."),
    dict(key='Banner_EnemyTurn', group='Bannières de combat', name='Bannière « Tour ennemi »', files=['UI/Banner/Banner_EnemyTurn.wav'], length='~0,5 à 0,9 s', bus='SFX',
         when="Quand les ennemis commencent à jouer (une fois, pas à chaque ennemi).",
         intent="Le pendant de « Votre tour », plus sombre ou menaçant, aussi court."),
    dict(key='Banner_Victory', group='Bannières de combat', name='Bannière « Victoire »', files=['UI/Banner/Banner_Victory.wav'], length='~1 à 1,5 s', bus='SFX',
         when="Fin du combat, juste après le finisher. La bannière reste 1,5 s.",
         intent="La résolution du combat, gratifiante, sans couvrir la queue du finisher."),
    dict(key='Finisher', group='Combat', name='Finisher (dernier coup du combat)', files=['Combat/Finisher.wav'], length='~1,5 à 2 s', bus='Impacts (fait baisser la musique)',
         when="Le coup qui tue le dernier ennemi : l'image se fige 0,12 s, puis ralenti à 25 % pendant environ 1 s avec un zoom sur la victime, et la caméra revient en 0,8 s. Joué par-dessus le son de l'attaque.",
         intent="Un accent lourd sur l'impact et une queue qui s'étire dans le ralenti."),
    # ---- Transitions et portails
    dict(key='Transition_In', group='Transitions et sorties', name='Volet de transition : couverture', files=['UI/Transition/Transition_In.wav'], length='~0,4 s', bus='SFX',
         when="Changement de salle (et chargement de scène, menu vers jeu) : des bandes obliques balayent l'écran pour le couvrir en 0,4 s.",
         intent="Un « whoosh » qui monte ou se referme. Deux sons séparés plutôt qu'un seul : le chargement entre les deux varie."),
    dict(key='Transition_Out', group='Transitions et sorties', name='Volet de transition : révélation', files=['UI/Transition/Transition_Out.wav'], length='~0,4 s', bus='SFX',
         when="La salle suivante est chargée, les bandes se retirent en 0,4 s.", intent="Le même geste, en sens inverse."),
    dict(key='Portal_Hover', group='Transitions et sorties', name="Survol d'une sortie", files=['Map/Portal_Hover.wav'], length='~0,2 à 0,4 s', bus='SFX',
         when="La souris arrive sur une sortie ouverte : son portail s'illumine, des ondes vont vers son centre et la salle visée bat sur la minimap.",
         intent="Une invitation légère, magique, de la même matière que les portails."),
    dict(key='Portal_Click', group='Transitions et sorties', name="Clic sur une sortie", files=['Map/Portal_Click.wav'], length='~0,3 à 0,5 s', bus='SFX',
         when="Clic sur une sortie : le joueur part vers elle, le volet de transition suit à son arrivée.",
         intent="Une validation, le portail qui « accepte » le joueur."),
    dict(key='Portal_Open', group='Transitions et sorties', name='Ouverture des sorties', files=['Map/Portal_Open.wav'], length='~1 à 1,5 s', bus='SFX',
         when="Une fois pour toutes les sorties d'une salle, quand elles s'ouvrent : à la victoire d'un combat, et en entrant dans une salle sans combat. Le bord du portail jaillit avec un éclair et une onde de choc, une bande de lumière monte, en vague depuis le joueur (1,1 s).",
         intent="L'ouverture d'un passage, énergique mais pas triomphale (la bannière de victoire joue en même temps)."),
    dict(key='Portal_Close', group='Transitions et sorties', name='Fermeture des sorties', files=['Map/Portal_Close.wav'], length='~0,3 à 0,4 s', bus='SFX',
         when="Une fois pour toutes les sorties qui se referment (0,35 s, l'ouverture jouée à l'envers). Rare en jeu : seulement si un combat commence alors que des sorties étaient ouvertes. Pas filmé.",
         intent="L'ouverture à l'envers, plus courte."),
    # ---- Inventaire et pause
    dict(key='RefuseArtifactInventory', group='Inventaire, pause et refus', name='Placement refusé (inventaire)', files=['UI/Inventory/RefuseArtifact.wav'], length='~0,2 à 0,3 s', bus='SFX', voices=['RefuseArtifact'],
         when="Dans l'inventaire façon Tetris, une pièce lâchée là où elle ne rentre pas retourne à sa place et tremble (remplace le son de dépôt).",
         intent="Un « non » bref et sourd, clairement une erreur mais pas agressif. Dans la famille de DropArtifactInventory."),
    dict(key='RefuseCombat', group='Inventaire, pause et refus', name='Action refusée (combat)', files=['UI/HUD/RefuseCombat.wav'], length='~0,2 s', bus='SFX',
         when="En combat : un clic hors de portée, ou un artefact trop cher (la case clignote en rouge, l'icône tremble).",
         intent="Même famille que le refus d'inventaire (une variante suffit, ou le même son)."),
    dict(key='OpenPause', group='Inventaire, pause et refus', name='Ouverture de la pause', files=['UI/Pause/OpenPause.wav'], length='~0,3 s', bus='SFX',
         when="Le jeu se fige, le panneau de pause arrive.", intent="Dans la famille de OpenInventory, en plus feutré."),
    dict(key='ClosePause', group='Inventaire, pause et refus', name='Fermeture de la pause', files=['UI/Pause/ClosePause.wav'], length='~0,3 s', bus='SFX',
         when="Retour au jeu.", intent="Le miroir de l'ouverture."),
]

# The clips, in order: file name, title, what it shows
CLIPS = [
    ('01_deploiement', 'Phase de déploiement', "Survol des cases de départ, choix d'une case, puis « Déployer » : le combat commence (bannière « Combat »), puis « Votre tour »."),
    ('02_tours_et_refus', 'Refus en combat et tours', "Un clic hors de portée est refusé, puis fin du tour : bannière « Tour ennemi », les ennemis jouent, puis « Votre tour »."),
    ('03_finisher_victoire', 'Finisher et victoire', "Le dernier ennemi meurt : arrêt sur image, ralenti et zoom, puis la bannière « Victoire »."),
    ('04_orbe_commune', 'Orbe commune', "Le joueur marche sur une orbe commune : éclat, puis ouverture du coffre."),
    ('05_orbe_rare', 'Orbe rare', "Même moment avec une orbe rare."),
    ('06_orbe_epique', 'Orbe épique', "Même moment avec une orbe épique."),
    ('07_orbe_legendaire', 'Orbe légendaire', "Même moment avec une orbe légendaire."),
    ('08_inventaire_refus', 'Inventaire : placement refusé', "Ouverture de l'inventaire, une pièce lâchée là où elle ne rentre pas revient à sa place en tremblant (deux fois)."),
    ('09_pause', 'Pause', "Ouverture puis fermeture du menu pause."),
    ('13_ambiance_grotte', 'Ambiance grotte', "La salle de départ, une grotte au fond de la faille."),
    ('14_sortie_et_transition', 'Sortie et transition vers le jardin de Drareg', "Survol et clic d'une sortie, le joueur y marche, volet de transition, arrivée dans l'antichambre : fondu vers l'ambiance onirique et sa couche d'eau, ouverture des sorties."),
    ('15_ambiance_onirique', 'Ambiance onirique', "L'antichambre de Drareg, avec ses bassins d'eau verte."),
    ('16_eau_et_gouttes', "Couche d'eau et gouttes", "Une grotte avec des bassins (Combat Room 10) : la couche d'eau et les gouttes des stalactites."),
    ('17_vers_la_falaise', 'Transition vers une falaise', "De la grotte à bassins vers une salle en falaise (Combat Room 11) : fondu de l'ambiance grotte et de l'eau vers l'ambiance falaise."),
    ('18_entree_en_combat', 'Entrée dans une salle de combat', "Dans une partie normale : sortie, transition, phase de déploiement, « Déployer »."),
    ('19_victoire_et_portails', 'Victoire et ouverture des sorties', "Dans une partie normale : le dernier ennemi tombe, finisher, victoire et ouverture des sorties, puis survol d'une sortie."),
]

VOICE_TO_KEY = {}
for s in SOUNDS:
    for v in s.get('voices', []) or []:
        VOICE_TO_KEY[v] = s['key']


def key_of(voice):
    if voice in VOICE_TO_KEY:
        return VOICE_TO_KEY[voice]
    for s in SOUNDS:
        if voice == s['key'] or (len(s['files']) > 1 and voice.startswith(s['key'] + '_')):
            return s['key']
    return None


BY_KEY = {s['key']: s for s in SOUNDS}
