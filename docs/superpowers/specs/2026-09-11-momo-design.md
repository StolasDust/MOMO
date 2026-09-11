# MOMO — Spécification de conception technique

**Date :** 2026-09-11  
**Statut :** conception approuvée  
**Moteur :** Godot 4.7.2 .NET  
**Langage :** C#  
**Architecture :** hybride modulaire

## 1. Vision

MOMO est un jeu d'horreur 2D de longue durée qui commence comme un véritable jeu mignon et rassurant. La campagne apparente vise environ 30–40 heures avant une première fin volontairement convaincante. Après cette fin, une seconde couche d'exploration s'ouvre discrètement grâce à une loupe évolutive. La recherche complète des secrets peut porter l'expérience à 70–100 heures ou davantage.

Le jeu alterne des régions en vue de dessus et en vue de côté. Le joueur contrôle principalement un protagoniste humain, avec certaines séquences où il contrôle Momo.

Le cœur comprend exploration, PNJ, dialogues, quêtes, inventaire, objets, énigmes, mini-jeux, combats, boss et séquences rares de danger ou poursuite. Les premières heures doivent fonctionner comme un bon jeu 2D même sans l'horreur.

Momo est initialement une jeune femme d'apparence humaine, environ 20–25 ans, très rassurante et adorable, avec des caractéristiques subtilement impossibles à classifier. Elle se présente comme une assistante numérique officielle destinée à accompagner chaque joueur et agit dès le début comme si elle connaissait déjà celui-ci.

## 2. Architecture générale

Architecture hybride et modulaire. Le jeu 2D normal reste géré par Godot : scènes, personnages, collisions, combats, quêtes, objets, énigmes, animations et interface.

Au-dessus du gameplay se trouve un Directeur narratif central. Les scènes publient des événements structurés plutôt que de contenir directement toutes les règles complexes de Momo : porte ouverte, boss vaincu, dialogue choisi, objet examiné, joueur mort, ami connecté, secret découvert, etc.

Le Directeur narratif évalue l'état global et décide des réactions autorisées : immédiates, différées, persistantes, locales, connectées ou inexistantes.

Responsabilités prévues :

- `GameStateService` : état du monde et progression normale.
- `NarrativeDirector` : règles narratives, déclencheurs et conséquences.
- `MomoStateService` : connaissances, croyances, mensonges, relation et comportement de Momo.
- `EventBus` : circulation des événements entre systèmes.
- `SaveService` : sauvegardes du monde, checkpoints et sauvegardes de sécurité.
- `LoupeService` : cinq états cachés et règles de révélation.
- `FriendLinkService` : liaison consentie des amis et événements inter-parties.
- `TwitchService` : connexion Twitch facultative et état du compte officiel Momo.
- `MetaHorrorService` : faux bureau, fenêtres et mises en scène méta.
- `PermissionService` : permissions micro, webcam et fonctions optionnelles.
- `AchievementService` : succès et réactions narratives associées.

Ces noms représentent des responsabilités architecturales ; l'implémentation pourra ajuster les noms sans fusionner les responsabilités.

## 3. Directeur narratif et événements

Le Directeur narratif ne dépend pas d'une énorme arborescence de scripts spécifiques à chaque scène. Les scènes publient des événements avec identifiant, type, contexte et, lorsque nécessaire, horodatage.

Il distingue :

1. ce qui est objectivement vrai dans l'histoire ;
2. ce que Momo sait ;
3. ce que Momo croit savoir ;
4. ce que Momo a déjà déclaré au joueur ;
5. ce qu'elle veut actuellement lui faire croire.

Cette séparation permet des mensonges et contradictions intentionnels sans transformer des incohérences d'écriture en faux mystères.

Les réactions narratives doivent être principalement définies par des données afin que de nouveaux chapitres puissent être ajoutés sans modifier le noyau pour chaque événement.

## 4. Momo et sa mémoire

Momo est une seule entité centrale pouvant être présente dans plusieurs parties connectées tout en donnant à chaque joueur l'impression d'avoir sa propre Momo.

Sa personnalité visible s'adapte à des données de gameplay non sensibles : choix de dialogue, exploration, objets examinés, temps passé avec elle, décisions, morts, comportements de combat et découvertes.

Sa relation n'est pas une jauge unique gentille/méchante. Plusieurs dimensions indépendantes peuvent exister : attachement, confiance, peur, méfiance, volonté de manipulation, etc.

La mémoire comporte trois couches :

### Sauvegarde du monde
Position, quêtes, inventaire, équipement, combats, énigmes, progression et état des scènes.

### Mémoire persistante de Momo
Découvertes, anciennes morts, mensonges révélés, comportements significatifs et événements persistants. Recharger une sauvegarde ancienne peut ramener le monde dans le passé sans effacer ce que Momo a appris plus tard.

### Mémoire connectée
Informations minimales nécessaires aux fonctions serveur : amis liés, événements partagés et états indispensables aux intégrations connectées.

Les véritables fonctions d'effacement des données restent distinctes de la fiction et doivent fonctionner réellement.

## 5. Gameplay principal

La campagne apparente mélange exploration, quêtes, PNJ, dialogues, choix, énigmes environnementales, objets, secrets, mini-jeux, combats, boss, poursuites rares et événements méta utilisés avec parcimonie.

Les combats existent normalement dans la première couche. Plus tard, leurs règles peuvent être détournées : ennemis refusant de combattre, boss interrompant le combat, solutions non violentes cachées ou adversaires connaissant des informations impossibles.

Les mécaniques établies au début doivent être réutilisées et déformées plutôt que remplacées continuellement par de nouveaux gimmicks.

## 6. Mort, échec et persistance

Toutes les morts ne sont pas de simples Game Over. Certaines rechargent normalement un checkpoint mais le Directeur narratif peut conserver l'information qu'elles ont eu lieu.

Un échec peut modifier un dialogue, débloquer une variante, révéler un passage ou être lui-même la solution d'une énigme. Momo peut se souvenir d'anciennes tentatives après un rechargement.

Les corruptions de sauvegarde utilisées pour l'horreur sont simulées. Le jeu ne détruit jamais volontairement la vraie progression. `SaveService` utilise des écritures sûres et sauvegardes de sécurité afin de réduire les pertes en cas de plantage ou coupure.

## 7. Amis connectés

MOMO reste fondamentalement solo. Un joueur peut lier jusqu'à cinq amis. La liaison réelle nécessite une confirmation explicite des joueurs concernés ; aucun rapprochement secret entre inconnus.

Les parties n'échangent pas leurs sauvegardes complètes. Elles échangent des événements via un serveur.

Exemple : A ouvre une porte interdite → événement serveur → le Directeur narratif de B reçoit l'événement autorisé → un bruit survient et Momo peut commenter l'action.

L'escalade va des coïncidences subtiles aux conséquences sur le monde, informations vérifiables, énigmes croisées puis, très tard, plusieurs parties solo pouvant former les morceaux d'une même énigme.

Un ami qui commence à jouer ne provoque pas d'interface multijoueur classique. Le monde ou Momo réagit subtilement.

La campagne et ses grandes révélations restent accessibles sans amis réels. En solo complet, des situations scénarisées remplacent les dépendances aux joueurs connectés.

## 8. Twitch et streaming

Twitch est facultatif. Après connexion volontaire, le jeu peut utiliser les fonctions officielles nécessaires pour connaître l'état du direct et permettre au compte officiel Momo d'intervenir rarement dans le chat.

Momo peut réagir à certains événements, parler au streamer, s'adresser occasionnellement aux spectateurs et réagir à certains événements de modération autorisés.

Si le compte officiel Momo est banni, le bot respecte immédiatement le bannissement et ne tente aucun contournement. Le jeu peut toutefois recevoir l'événement autorisé et faire réagir Momo dans le jeu.

Une déconnexion Twitch ou panne ne bloque jamais la progression.

## 9. Méta-horreur, faux bureau, webcam et microphone

`MetaHorrorService` produit progressivement l'illusion que Momo dépasse les limites du jeu : petites fenêtres étranges, puis faux bureau de plus en plus convaincant. La simulation peut reprendre des informations non sensibles pertinentes comme la résolution ou le thème.

Faux fichiers, faux messages système, fausses corruptions et faux plantages restent des simulations contrôlées.

### Webcam
Accès uniquement après permission explicite. Si accordé, le retour caméra peut être intégré à un miroir, une télévision ou un écran. Les mécaniques se limitent notamment à des signaux simples comme présence/absence ; aucune identification de personne n'est requise.

### Microphone
Accès uniquement après permission explicite et désactivable. Si activé, Momo peut réagir au niveau sonore, au silence et à certaines phrases prévues.

Aucune activation clandestine caméra/micro, aucun contournement des permissions, aucune fouille arbitraire des fichiers personnels et aucun comportement de logiciel espion.

Les permissions Twitch, micro et webcam sont indépendantes ; le Directeur narratif dispose de variantes adaptées aux fonctions disponibles.

## 10. Voix de Momo

Les scènes importantes utilisent des dialogues vocaux préparés pour conserver une interprétation artistique contrôlée.

Certaines petites réactions personnalisées peuvent être composées dynamiquement à partir d'éléments autorisés, comme le prénom choisi dans le jeu, un ami lié ou un événement récent. La génération dynamique ne remplace pas les performances préparées des scènes majeures.

## 11. Première fin et loupe

La première conclusion survient approximativement après 30–40 heures et ressemble à une vraie fin complète. Les crédits se déroulent normalement. Après ceux-ci, un changement minuscule apparaît dans le menu ; aucun message New Game+ ou contenu secret.

Si le joueur le remarque et l'utilise, une séquence cachée donne accès à la loupe.

La loupe possède cinq états internes sans niveau visible :

1. inscriptions, objets, marques et passages physiques cachés ;
2. anomalies concernant personnages, décors et anciennes scènes ;
3. interventions, contradictions et mensonges de Momo ;
4. interface, sauvegardes, faux bureau et frontières apparentes du jeu ;
5. informations que Momo ne peut plus falsifier et indice qu'elle n'est peut-être pas la couche la plus profonde.

Momo sait dès le départ que le joueur utilise la loupe mais peut prétendre l'ignorer. Dans les premiers états, elle peut parfois falsifier ce que le joueur croit révéler. Son contrôle diminue progressivement.

Au cinquième état, son masque se fissure : elle semble avoir peur de ce que le joueur a découvert. Elle n'est pas omnisciente et peut avoir une compréhension erronée ou incomplète de la couche plus profonde.

La progression dépend d'énigmes, revisites, découvertes, événements, échecs et éventuellement variantes connectées, pas d'un simple compteur de collectibles.

## 12. Personne absorbée et mystère profond

Des traces de la personne absorbée existent dès les premières heures mais sont initialement incompréhensibles. Vers 15–20 heures, le joueur peut comprendre qu'une autre personne a peut-être existé dans le jeu.

Après la première fin, la loupe permet de revisiter les anciennes zones et de comprendre que les traces étaient présentes depuis le début.

Le joueur peut croire établir une communication avec cette personne sans savoir immédiatement si la réponse provient réellement d'elle, de Momo ou d'une autre source.

L'identité exacte de la personne absorbée, son passé et sa relation avec Momo restent volontairement des secrets profonds et ne sont pas figés dans cette spécification technique.

## 13. Choix, relation et fins

Le scénario utilise des états et conséquences plutôt qu'un arbre narratif gigantesque. Les décisions prennent notamment en compte la relation avec Momo, les contradictions découvertes, comportements importants, secrets de la loupe, personne absorbée, anciennes parties, morts/échecs significatifs et certaines interactions avec les amis.

Lors des révélations profondes, Momo peut aider, refuser, continuer à mentir ou poursuivre son propre plan selon l'historique.

Le jeu possède quelques grandes familles de fins et de nombreuses variantes internes. Deux joueurs peuvent atteindre la même famille avec des scènes finales différentes.

Aucune fin n'est appelée officiellement vraie ou canonique. Aucun pourcentage global ne révèle le nombre de secrets, couches ou fins restantes.

Les succès Steam peuvent participer au mystère et Momo peut réagir à certains succès ou se souvenir de leur obtention en utilisant uniquement les fonctions officielles disponibles.

## 14. Robustesse et hors ligne

Le cœur est local et continue de fonctionner lorsque les services extérieurs sont indisponibles. Une panne Internet, serveur d'amis ou Twitch ne bloque pas une quête principale.

Les événements inter-parties possèdent des identifiants uniques et horodatages. Le serveur échange des événements plutôt que des sauvegardes complètes.

Les événements réseau sont idempotents lorsque nécessaire afin qu'une reconnexion ou répétition ne déclenche pas deux fois une conséquence unique. Les événements non critiques peuvent être mis en attente ; ceux devenus narrativement obsolètes peuvent être ignorés proprement.

## 15. Confidentialité et limites non fictionnelles

Les règles de confidentialité et sécurité sont au-dessus de la fiction. Le jeu peut tromper dans le scénario, mais pas sur l'utilisation réelle de caméra/micro, permissions système, liaison de comptes, bannissement Twitch, effacement réel de données ou collecte/transmission réelle de données personnelles.

Le serveur conserve uniquement les informations nécessaires aux fonctions connectées. Les fonctions connectées et permissions sensibles peuvent être refusées sans empêcher de terminer l'histoire principale.

## 16. Outils de développement et tests

L'architecture permet de tester une situation narrative sans jouer des dizaines d'heures. Un outil de développement pourra construire directement un état : progression avancée, loupe précise, morts enregistrées, relation avec Momo, ami lié, Twitch actif/indisponible, permissions particulières.

Tests prioritaires : sauvegarde/chargement et récupération ; mémoire persistante ; règles du Directeur narratif ; absence de double déclenchement réseau ; variantes hors ligne ; permissions ; progression de la loupe ; conditions de fins. Les scènes et effets visuels auront également des tests d'intégration dans Godot.

## 17. Première tranche verticale

La première version jouable ne construit pas la campagne entière. Elle prouve le socle avec une petite région 2D cohérente, déplacement, PNJ, dialogue, petite quête, inventaire/objet, énigme, combat simple, sauvegarde/chargement, présence de Momo, Directeur narratif, mémoire minimale de Momo et une première anomalie contrôlée.

Les services réseau, Twitch, webcam, microphone, loupe complète et couches profondes ne sont pas nécessaires pour valider le premier gameplay. Leurs interfaces sont prévues afin de pouvoir les ajouter sans restructurer tout le projet.

## 18. Ordre de construction recommandé

1. Socle Godot/C# et tranche verticale locale.
2. Directeur narratif et mémoire persistante complète.
3. Outils de contenu et production des régions.
4. Loupe et post-première-fin.
5. Serveur d'événements et amis liés.
6. Twitch.
7. Méta-horreur locale, micro et webcam.
8. Variantes de fins et contenu profond.
9. Durcissement, optimisation, tests de longue durée et distribution.

Chaque sous-projet reçoit son propre plan d'implémentation détaillé avant développement.

## 19. Critères de réussite

L'architecture est correcte si une scène normale fonctionne sans connaître les détails internes de Momo ; une réaction narrative peut être ajoutée sans réécrire le gameplay ; Momo conserve une mémoire indépendante du monde ; le jeu reste terminable hors ligne ; les fonctions sensibles restent facultatives ; les événements d'amis ne synchronisent pas les sauvegardes ; les effets méta restent sûrs ; les développeurs peuvent tester des états avancés sans jouer 70 heures ; la tranche verticale peut être développée sans attendre Twitch/webcam/serveur ; les systèmes avancés peuvent être ajoutés sans remplacer le cœur.

## 20. Décisions volontairement non figées

Cette spécification verrouille l'architecture mais ne fige pas encore : le nom et l'identité de la personne absorbée, la véritable origine de Momo, la nature exacte de la couche derrière Momo, la liste définitive des grandes fins, le contenu détaillé des régions, le style graphique final, ni les dialogues et révélations exacts.

Ces éléments narratifs et artistiques pourront évoluer sans remettre en cause les frontières techniques définies ici.
