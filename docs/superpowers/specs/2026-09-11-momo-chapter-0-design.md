# MOMO — Phase 2 / Chapitre 0 « Bienvenue » — Spécification de conception

**Date :** 2026-09-11  
**Statut :** conception approuvée  
**Moteur :** Godot 4.7.2 .NET  
**Langage :** C#  
**Approche :** fondation de production + premier contenu réel

## 1. Objectif

La Phase 2 transforme la tranche verticale validée en fondation du vrai jeu. Elle ne consiste ni en un refactoring invisible complet, ni en l'empilement de contenu sur le prototype. Elle construit simultanément une architecture de production et le début réellement jouable de MOMO.

Le résultat attendu est un Chapitre 0 chaleureux qui remplace progressivement la salle de test : lancement, création du joueur, configuration innocente des personnes importantes, arrivée dans la première région, premières activités, logement personnalisable et accompagnement initial de Momo comme assistante numérique.

L'horreur ne doit pas être nécessaire pour rendre ce début intéressant. Les premières heures doivent fonctionner comme un bon jeu 2D à part entière.

## 2. Expérience d'ouverture

MOMO démarre comme un jeu 2D adorable et rassurant. Aucun glitch, jumpscare, écran inquiétant ou musique menaçante ne signale artificiellement le genre horrifique.

Momo apparaît d'abord dans l'interface comme l'assistante officielle du jeu. Elle aide à la configuration et à la création du personnage avec un ton naturel et chaleureux.

Le joueur croit disposer d'un créateur de personnage complet : prénom, apparence et plusieurs choix visuels. Certains paramètres possèdent cependant déjà une valeur interne avant leur sélection. Ce mécanisme reste imperceptible lors d'une première partie normale et ne doit pas produire d'anomalie évidente pendant l'ouverture.

Après le profil du joueur, Momo propose une fonction de personnalisation permettant de renseigner jusqu'à cinq « personnes importantes ». Ces prénoms sont facultatifs. Ils peuvent plus tard servir à personnaliser la narration, mais ne constituent jamais à eux seuls une liaison réseau.

Toute liaison réelle entre deux joueurs nécessite une confirmation explicite des comptes ou codes concernés. Deux inconnus partageant le même prénom ne sont jamais reliés automatiquement.

## 3. Première région

La première région est suffisamment grande pour soutenir plusieurs heures de jeu. Elle comprend au minimum :

- un village principal ;
- une petite zone commerçante ;
- une forêt ;
- un lac ;
- le logement du joueur ;
- des lieux secondaires destinés aux quêtes, mini-jeux et secrets.

La vue du dessus constitue la perspective principale d'exploration. Certains lieux utilisent naturellement une vue de côté : bord du lac, intérieur particulier, grotte, passage de forêt, mini-jeu ou séquence dédiée. Le changement de perspective doit servir le lieu ou le gameplay, pas seulement démontrer une fonctionnalité technique.

La direction visuelle vise une 2D détaillée, chaleureuse et expressive. Les environnements doivent être suffisamment riches pour contenir des détails anodins au premier passage et significatifs lors des revisites avec la loupe.

## 4. Structure des premières heures

Le personnage arrive sans grande exposition expliquant sa présence. Son installation semble déjà prévue : son logement l'attend et les habitants trouvent son arrivée normale. Au premier passage, cela doit ressembler à une convention naturelle de jeu vidéo.

Pendant les premières heures, aucune quête épique ne domine l'expérience. Le joueur découvre la région, rencontre les habitants, accomplit de petites quêtes, explore, joue à des mini-jeux, collecte des ressources, gagne de la monnaie et aménage son logement.

Plusieurs activités apparemment secondaires peuvent déjà produire des événements utiles au Directeur narratif et à la mémoire de Momo. Elles ne doivent cependant pas afficher leur importance future.

La première couche de gameplay doit rester satisfaisante indépendamment du mystère : exploration, dialogues, quêtes, collecte, économie, fabrication légère, décoration, mini-jeux et mécaniques de jeu déjà éprouvées dans la tranche verticale.

## 5. Première présence physique de Momo

Pendant environ une à deux heures, Momo accompagne principalement le joueur sous sa forme d'assistante numérique.

Sa première apparition physique survient ensuite au cours d'une quête normale avec plusieurs PNJ. Les habitants la connaissent déjà et lui parlent naturellement. Certains peuvent faire référence à des expériences passées avec elle.

La mise en scène ne signale pas cette situation comme une anomalie : pas de jumpscare, pas de glitch, pas de rupture musicale obligatoire et pas de commentaire du jeu demandant au joueur de trouver cela étrange.

Cette scène doit être conçue pour pouvoir être revisitée beaucoup plus tard. Les états avancés de la loupe pourront alors révéler des contradictions, détails falsifiés ou incohérences concernant les souvenirs des PNJ et la présence antérieure de Momo.

## 6. Logement du joueur

Le logement est un lieu central et durable, pas un simple point de sauvegarde. Il est personnalisable avec des meubles, sols, murs, décorations, souvenirs et objets uniques. Le placement des éléments doit offrir une liberté raisonnable sans exiger un simulateur de construction complet.

Les objets d'aménagement proviennent de plusieurs sources : commerces, récompenses de quêtes et mini-jeux, fabrication à partir de ressources et objets uniques liés à certains PNJ ou secrets.

Le jeu mémorise l'aménagement réel du joueur afin que de futures anomalies puissent tenir compte de sa propre maison plutôt que d'utiliser uniquement une disposition prédéfinie.

Momo commence par visiter normalement le logement. Progressivement, bien après l'ouverture, elle peut y apparaître sans invitation. À plus long terme, la géométrie du logement peut elle-même devenir impossible : couloir trop long, pièce incompatible avec le plan extérieur, porte menant ailleurs ou espaces uniquement révélables par la loupe.

La Phase 2 doit préparer les données nécessaires à ces évolutions sans implémenter immédiatement leurs manifestations horrifiques avancées.

## 7. Économie et fabrication

Le jeu utilise une monnaie principale pour les commerces et services courants, des ressources de fabrication et, lorsque cela sert réellement une activité, quelques jetons spécifiques à un mini-jeu ou événement.

Il ne faut pas multiplier les monnaies sans nécessité. L'économie sert principalement l'exploration, les récompenses, la personnalisation du logement et les boucles de jeu de la première couche.

La fabrication reste légère dans le Chapitre 0. Elle doit démontrer une boucle claire ressource → recette → objet utile ou décoratif sans devenir un système industriel complexe.

## 8. Architecture de production

La tranche verticale a prouvé les mécaniques ; la Phase 2 remplace progressivement ses raccourcis par des frontières de production.

### 8.1 Services

Les responsabilités restent séparées :

- `GameStateService` : état courant du monde, progression, inventaire et données normales de partie ;
- `EventBus` : événements structurés du gameplay ;
- `NarrativeDirector` : interprétation des événements et règles narratives ;
- `MomoStateService` : mémoire persistante, connaissances, croyances, déclarations et relation de Momo ;
- `SaveService` : sauvegarde/chargement versionnés et sûrs ;
- `ProfileService` : profil local du joueur, apparence choisie et personnes importantes ;
- `DialogueService` : chargement et déroulement de dialogues pilotés par des données ;
- `QuestService` : états de quêtes pilotés par des données ;
- `InventoryService` : objets et ressources ;
- `EconomyService` : monnaie principale et jetons locaux autorisés ;
- `HousingService` : inventaire d'ameublement, placement et état du logement ;
- `SceneFlowService` : transition entre zones et perspectives.

Les noms exacts peuvent évoluer, mais les responsabilités ne doivent pas être fusionnées en un gestionnaire global unique.

### 8.2 État global Godot

Le prototype ne doit plus dépendre durablement d'un runtime statique global. Les services de longue durée sont hébergés via une racine persistante/autoload Godot ou une composition équivalente testable. Les scènes consomment des interfaces ou références explicites et publient des événements ; elles ne connaissent pas les détails internes de Momo.

### 8.3 Contenu piloté par les données

Les dialogues, quêtes, objets, recettes et principales définitions de contenu quittent progressivement le C# codé en dur. Les données possèdent des identifiants stables et sont validées au chargement.

Le C# implémente les règles et comportements ; les fichiers de contenu décrivent les instances du jeu. Cette séparation permet d'écrire plusieurs heures de contenu sans modifier le noyau pour chaque dialogue.

### 8.4 Scènes réutilisables

Le monolithe de la tranche verticale est découpé progressivement en scènes réutilisables : joueur, PNJ, interactable, objet ramassable, zone, porte, ennemi, interface de dialogue, commerce, station de fabrication et éléments de logement.

Le Chapitre 0 est composé à partir de ces briques plutôt que d'un unique `Main.tscn` contenant toute la logique.

## 9. Sauvegarde et restauration

La sauvegarde du monde et la mémoire de Momo restent réellement séparées.

La sauvegarde du monde contient au minimum : zone et position, progression, quêtes, inventaire, monnaie, ressources, états persistants des scènes et aménagement du logement.

La mémoire de Momo contient les événements persistants qui ne doivent pas être annulés par le simple chargement d'une ancienne sauvegarde.

Les formats sont versionnés. Le chargement valide les données avant application. Une sauvegarde invalide ne doit pas écraser immédiatement la dernière copie saine. Les écritures utilisent une stratégie temporaire/remplacement et des sauvegardes de sécurité adaptées aux capacités de la plateforme.

Contrairement au prototype, charger une partie doit restaurer réellement l'état visible des scènes concernées.

## 10. Profil, données et confidentialité

Le profil local conserve uniquement ce qui est nécessaire au jeu : prénom choisi, options de personnage et jusqu'à cinq personnes importantes renseignées volontairement.

Ces noms peuvent personnaliser localement des dialogues. Leur présence ne signifie pas qu'ils correspondent à de vrais comptes.

Les futures fonctions connectées restent séparées : une liaison d'ami possède son propre identifiant confirmé et son propre état de consentement. Les données narratives ne doivent pas contourner ce modèle.

## 11. Préparation des systèmes futurs

La Phase 2 ne construit pas encore les intégrations Twitch, webcam, microphone, serveur d'amis ni les cinq états complets de la loupe.

L'architecture évite cependant de les bloquer : les événements possèdent des identifiants stables, le Directeur narratif accepte des sources d'événements distinctes et les services futurs pourront s'abonner sans injecter leur logique dans les scènes de gameplay.

Aucun faux service réseau complexe n'est nécessaire maintenant. YAGNI s'applique : seules les interfaces minimales utiles à une frontière déjà nécessaire sont créées.

## 12. Outils de développement

Le développement d'un jeu de 30–100+ heures exige de pouvoir atteindre rapidement un état précis. La Phase 2 prépare un mode développeur capable de sélectionner une zone, positionner la progression du Chapitre 0, attribuer monnaie/objets et simuler des états narratifs locaux utiles.

Ces outils ne font pas partie de l'expérience joueur distribuée normalement. Ils servent aux tests et à la création de contenu.

## 13. Tests et validation

La logique pure continue d'être testée séparément des scènes Godot. Les changements suivent un cycle test rouge → implémentation minimale → test vert lorsque l'environnement permet l'exécution automatisée.

Tests prioritaires de la Phase 2 :

- création/validation du profil ;
- personnes importantes facultatives et limite de cinq ;
- sérialisation/versionnement ;
- séparation monde/mémoire Momo ;
- restauration réelle de l'état du monde ;
- progression des quêtes pilotées par les données ;
- inventaire, économie et recette simple ;
- placement/restauration d'éléments du logement ;
- événements du Directeur narratif ;
- transitions entre zones et perspectives.

Des tests d'intégration Godot vérifient lancement, création du profil, déplacement, interaction, transition de zone, sauvegarde/chargement et boucle du Chapitre 0.

Lorsque l'environnement de l'agent ne dispose pas de Godot/.NET, il ne doit pas déclarer les tests exécutés. La validation réelle est alors effectuée sur l'environnement Godot du projet et les résultats sont rapportés explicitement.

## 14. Tranche de production visée

La première tranche de la Phase 2 ne cherche pas à produire immédiatement deux heures complètes de contenu final. Elle doit produire un début représentatif et extensible :

1. écran de lancement et démarrage du Chapitre 0 ;
2. Momo comme assistante d'interface ;
3. création et sauvegarde du profil ;
4. saisie facultative de zéro à cinq personnes importantes ;
5. arrivée dans une première zone réellement illustrable/remplaçable par des assets finaux ;
6. exploration et interaction avec au moins un PNJ ;
7. une petite quête pilotée par les données ;
8. une boucle monnaie/ressource/récompense ;
9. accès au logement ;
10. placement d'au moins un objet décoratif ;
11. sauvegarde, fermeture/reprise et restauration visible de cet état.

La première apparition physique de Momo après une à deux heures appartient au Chapitre 0, mais elle n'a pas besoin d'être incluse dans la toute première tranche technique si le contenu précédent n'atteint pas encore cette durée.

## 15. Critères d'acceptation

La Phase 2 est correctement engagée lorsque :

- le jeu démarre sur une véritable ouverture plutôt que sur la salle de test ;
- le joueur peut créer un profil et renseigner jusqu'à cinq personnes importantes facultatives ;
- le premier espace ressemble à une base de vrai jeu et utilise des scènes réutilisables ;
- dialogues et quête de démonstration ne sont plus entièrement codés en dur dans les contrôleurs ;
- l'état du monde et la mémoire de Momo sont indépendants ;
- une sauvegarde restaurée remet réellement le monde et le logement dans leur état précédent ;
- le logement accepte au moins un placement décoratif persistant ;
- les systèmes futurs peuvent s'intégrer via événements/services sans être implémentés prématurément ;
- aucune fonction sensible ou connectée n'est nécessaire pour terminer cette tranche ;
- la base reste extensible vers plusieurs heures de contenu sans reconstruire le noyau.

## 16. Hors périmètre immédiat

Restent volontairement hors de cette première tranche Phase 2 :

- serveur réel de liaison entre amis ;
- Twitch et bot officiel Momo ;
- webcam et microphone ;
- faux bureau avancé ;
- système complet des cinq états de loupe ;
- géométrie impossible finale du logement ;
- grandes révélations sur la personne absorbée ;
- première fausse fin ;
- familles de fins avancées ;
- production des 30–40 heures complètes.

Ces éléments restent couverts par la spécification générale de MOMO et seront traités dans des phases dédiées.