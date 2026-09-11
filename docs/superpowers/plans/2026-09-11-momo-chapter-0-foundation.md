# MOMO Chapter 0 Production Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transformer la tranche verticale validée en fondation de production et livrer un premier début réel de MOMO : profil, Momo assistante, personnes importantes, première zone, quête data-driven, économie légère, logement décoré et restauration visible.

**Architecture:** Migration incrémentale : conserver les comportements validés tout en remplaçant le runtime statique et le contenu codé en dur par des services persistants, des modèles versionnés et des définitions de contenu. Chaque tâche doit laisser le projet compilable et testable ; le Chapitre 0 se compose de scènes réutilisables au lieu d'un `Main.tscn` monolithique.

**Tech Stack:** Godot 4.7.2 .NET, C#, .NET 8, `System.Text.Json`, xUnit pour logique pure, scènes/resources Godot pour intégration.

**Spec:** `docs/superpowers/specs/2026-09-11-momo-chapter-0-design.md`

## Global Constraints

- Moteur : Godot 4.7.2 .NET.
- Langage : C#.
- Approche : fondation de production + premier contenu réel.
- Le cœur reste jouable localement et hors ligne.
- La sauvegarde du monde et la mémoire de Momo restent réellement séparées.
- Les scènes publient des événements et ne connaissent pas les détails internes de Momo.
- Dialogues, quêtes, objets et recettes nouveaux sont pilotés par des données avec identifiants stables.
- Les formats persistants sont versionnés et validés avant application.
- Jusqu'à cinq personnes importantes sont facultatives ; leurs prénoms ne créent jamais une liaison réseau.
- Twitch, webcam, microphone, serveur réel d'amis, faux bureau avancé et loupe complète sont hors périmètre.
- Ne pas déclarer un test automatisé exécuté si l'environnement ne possède pas Godot/.NET.
- TDD : test rouge observé, implémentation minimale, test vert, puis commit.
- YAGNI : aucune infrastructure externe non nécessaire au Chapitre 0.

---

## File Structure

Structure cible incrémentale (adapter uniquement les chemins déjà présents sans dupliquer les classes existantes) :

```text
MOMO/
├─ project.godot
├─ MOMO.csproj
├─ scenes/
│  ├─ Main.tscn
│  ├─ bootstrap/GameRoot.tscn
│  ├─ chapter0/Chapter0Intro.tscn
│  ├─ chapter0/VillageEntry.tscn
│  ├─ housing/PlayerHome.tscn
│  ├─ actors/Player.tscn
│  ├─ actors/Npc.tscn
│  └─ ui/
│     ├─ ProfileSetup.tscn
│     ├─ MomoAssistantPanel.tscn
│     ├─ DialogueBox.tscn
│     └─ HousingPlacement.tscn
├─ src/
│  ├─ Bootstrap/GameServices.cs
│  ├─ Profile/PlayerProfile.cs
│  ├─ Profile/ProfileService.cs
│  ├─ Content/ContentCatalog.cs
│  ├─ Content/ContentLoader.cs
│  ├─ Dialogue/DialogueDefinition.cs
│  ├─ Dialogue/DialogueService.cs
│  ├─ Quests/QuestDefinition.cs
│  ├─ Quests/QuestService.cs
│  ├─ Inventory/InventoryService.cs
│  ├─ Economy/EconomyService.cs
│  ├─ Crafting/RecipeDefinition.cs
│  ├─ Crafting/CraftingService.cs
│  ├─ Housing/HousingItem.cs
│  ├─ Housing/HousingPlacement.cs
│  ├─ Housing/HousingService.cs
│  ├─ Save/WorldSaveData.cs
│  ├─ Save/MomoMemoryData.cs
│  ├─ Save/SaveService.cs
│  ├─ World/SceneFlowService.cs
│  └─ Dev/DevStateService.cs
├─ content/chapter0/
│  ├─ dialogues.json
│  ├─ quests.json
│  ├─ items.json
│  └─ recipes.json
└─ tests/
   ├─ ProfileServiceTests.cs
   ├─ ContentLoaderTests.cs
   ├─ DialogueServiceTests.cs
   ├─ QuestServiceTests.cs
   ├─ EconomyServiceTests.cs
   ├─ CraftingServiceTests.cs
   ├─ HousingServiceTests.cs
   ├─ SaveServiceTests.cs
   └─ SceneFlowServiceTests.cs
```

Les noms déjà présents dans le projet ont priorité : déplacer/renommer une classe existante uniquement si cela réduit réellement la duplication et si tous ses consommateurs sont migrés dans la même tâche.

---

### Task 1: Composition persistante des services

**Files:**
- Create: `src/Bootstrap/GameServices.cs`
- Create: `scenes/bootstrap/GameRoot.tscn`
- Modify: `scenes/Main.tscn`
- Modify: `project.godot`
- Test: `tests/GameServicesTests.cs`

**Interfaces:**
- Consumes: services existants `EventBus`, `GameStateService`, `MomoStateService`, `SaveService`, `NarrativeDirector` (adapter leurs constructeurs réels après inspection).
- Produces: `GameServices` comme point de composition persistant ; accès stable aux services sans `NarrativeRuntime` statique pour le nouveau Chapitre 0.

- [ ] **Step 1: Inspecter les signatures réelles avant migration**

Lire les classes existantes et noter leurs constructeurs et dépendances. Ne pas créer de doublon sous un autre namespace. Identifier tous les consommateurs de `NarrativeRuntime`.

- [ ] **Step 2: Écrire un test rouge de composition**

Créer `GameServicesTests` qui construit le conteneur de logique pure (ou sa factory) et vérifie que `EventBus`, `GameStateService` et `MomoStateService` sont des instances stables :

```csharp
[Fact]
public void CoreServicesAreStableWithinOneComposition()
{
    var services = GameServices.CreateForTests();
    Assert.Same(services.EventBus, services.EventBus);
    Assert.Same(services.GameState, services.GameState);
    Assert.Same(services.MomoState, services.MomoState);
}
```

- [ ] **Step 3: Exécuter le test et confirmer l'échec**

Run:
```powershell
dotnet test tests/MOMO.Tests.csproj --filter GameServicesTests
```
Expected: FAIL car `GameServices` n'existe pas encore.

- [ ] **Step 4: Implémenter la composition minimale**

Créer `GameServices` avec propriétés en lecture seule et une factory de test. Dans Godot, `GameRoot.tscn` héberge un Node C# persistant qui crée une seule composition et charge le flux du jeu. Ne migrer dans cette tâche que les consommateurs nécessaires au démarrage.

- [ ] **Step 5: Vérifier tests + démarrage Godot**

Run:
```powershell
dotnet test tests/MOMO.Tests.csproj --filter GameServicesTests
```
Expected: PASS.

Dans Godot : lancer `Main.tscn`; Expected: aucun doublon de services, aucune erreur C#, ancienne tranche verticale encore accessible temporairement.

- [ ] **Step 6: Commit**

```powershell
git add project.godot scenes src/Bootstrap tests/GameServicesTests.cs
git commit -m "refactor: add persistent game service composition"
```

---

### Task 2: Profil local et cinq personnes importantes

**Files:**
- Create: `src/Profile/PlayerProfile.cs`
- Create: `src/Profile/ProfileService.cs`
- Create: `tests/ProfileServiceTests.cs`

**Interfaces:**
- Produces: `PlayerProfile.Name`, `PlayerProfile.Appearance`, `PlayerProfile.ImportantPeople`; `ProfileService.CreateProfile(...)`; maximum cinq noms non vides ; aucun identifiant réseau.

- [ ] **Step 1: Écrire les tests rouges**

```csharp
[Fact]
public void ProfileAcceptsZeroImportantPeople()
{
    var profile = ProfileService.CreateProfile("Mickael", new(), Array.Empty<string>());
    Assert.Empty(profile.ImportantPeople);
}

[Fact]
public void ProfileRejectsMoreThanFiveImportantPeople()
{
    Assert.Throws<ArgumentException>(() => ProfileService.CreateProfile(
        "Mickael", new(), new[] { "A", "B", "C", "D", "E", "F" }));
}

[Fact]
public void ImportantPeopleAreNarrativeNamesNotNetworkLinks()
{
    var profile = ProfileService.CreateProfile("Mickael", new(), new[] { "Nicolas" });
    Assert.Equal("Nicolas", profile.ImportantPeople[0]);
    Assert.False(profile.HasConfirmedFriendLinks);
}
```

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter ProfileServiceTests`  
Expected: FAIL sur les types manquants.

- [ ] **Step 3: Implémenter le modèle minimal**

`Appearance` est un dictionnaire de choix stables pour cette tranche (`hair`, `eyes`, `outfit`, etc.). Normaliser les espaces des noms, ignorer les entrées vides, conserver l'ordre, interdire plus de cinq valeurs finales. `HasConfirmedFriendLinks` reste faux : aucune logique réseau n'est créée.

- [ ] **Step 4: Vérifier le vert**

Run: `dotnet test tests/MOMO.Tests.csproj --filter ProfileServiceTests`  
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/Profile tests/ProfileServiceTests.cs
git commit -m "feat: add local player profile"
```

---

### Task 3: Chargement de contenu data-driven

**Files:**
- Create: `src/Content/ContentCatalog.cs`
- Create: `src/Content/ContentLoader.cs`
- Create: `src/Dialogue/DialogueDefinition.cs`
- Create: `src/Quests/QuestDefinition.cs`
- Create: `src/Crafting/RecipeDefinition.cs`
- Create: `content/chapter0/dialogues.json`
- Create: `content/chapter0/quests.json`
- Create: `content/chapter0/items.json`
- Create: `content/chapter0/recipes.json`
- Create: `tests/ContentLoaderTests.cs`

**Interfaces:**
- Produces: `ContentLoader.LoadChapter0(string rootPath) -> ContentCatalog`; catalogues indexés par identifiant stable.

- [ ] **Step 1: Écrire les tests rouges**

Tester un catalogue minimal en mémoire : un dialogue `momo.welcome`, une quête `village.first_help`, un objet `home.small_plant`, une recette `craft.small_plant`. Vérifier qu'un identifiant dupliqué ou une référence de récompense inconnue provoque `ContentValidationException`.

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter ContentLoaderTests`  
Expected: FAIL sur les types manquants.

- [ ] **Step 3: Implémenter modèles, désérialisation et validation**

Utiliser `System.Text.Json`. Chaque définition a un `Id` non vide. `ContentCatalog` indexe les définitions par `StringComparer.Ordinal`. La validation rejette doublons et références manquantes nécessaires à la tranche.

- [ ] **Step 4: Ajouter le contenu minimal du Chapitre 0**

`dialogues.json` contient au minimum `momo.welcome` et `npc.first_greeting`. `quests.json` contient `village.first_help`. `items.json` contient une ressource collectable et `home.small_plant`. `recipes.json` contient une recette simple transformant la ressource en décoration.

- [ ] **Step 5: Vérifier tests**

Run: `dotnet test tests/MOMO.Tests.csproj --filter ContentLoaderTests`  
Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/Content src/Dialogue/DialogueDefinition.cs src/Quests/QuestDefinition.cs src/Crafting/RecipeDefinition.cs content tests/ContentLoaderTests.cs
git commit -m "feat: add validated chapter content catalog"
```

---

### Task 4: Dialogue et quête pilotés par les données

**Files:**
- Modify/Create: `src/Dialogue/DialogueService.cs`
- Modify/Create: `src/Quests/QuestService.cs`
- Modify: contrôleurs de dialogue/PNJ actuellement utilisés
- Test: `tests/DialogueServiceTests.cs`
- Test: `tests/QuestServiceTests.cs`

**Interfaces:**
- Consumes: `ContentCatalog`, `EventBus`, `GameStateService`.
- Produces: `DialogueService.Start(string id)`, `DialogueService.Advance()`, `QuestService.Start(string id)`, `QuestService.Handle(GameEvent e)`.

- [ ] **Step 1: Écrire les tests rouges**

Dialogue : démarrer `momo.welcome`, vérifier la première ligne et la fin après progression. Quête : démarrer `village.first_help`, publier l'événement attendu et vérifier passage à `Completed` une seule fois.

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter "DialogueServiceTests|QuestServiceTests"`  
Expected: FAIL sur les nouvelles interfaces/comportements.

- [ ] **Step 3: Implémenter le minimum et adapter l'UI existante**

Les contrôleurs Godot demandent un dialogue par ID au service. Aucun texte du nouveau Chapitre 0 ne doit être encodé directement dans `NpcDialogue`/contrôleur équivalent. `QuestService` traduit les événements en progression sans faire connaître les quêtes aux scènes.

- [ ] **Step 4: Vérifier tests et interaction Godot**

Run: même commande. Expected: PASS.  
Godot Expected: un PNJ peut lancer `npc.first_greeting`; la quête peut démarrer et se terminer sans erreur.

- [ ] **Step 5: Commit**

```powershell
git add src/Dialogue src/Quests src content tests/DialogueServiceTests.cs tests/QuestServiceTests.cs
git commit -m "refactor: drive dialogue and quests from content data"
```

---

### Task 5: Économie, ressources et fabrication légère

**Files:**
- Modify/Create: `src/Inventory/InventoryService.cs`
- Create: `src/Economy/EconomyService.cs`
- Create: `src/Crafting/CraftingService.cs`
- Test: `tests/EconomyServiceTests.cs`
- Test: `tests/CraftingServiceTests.cs`

**Interfaces:**
- Produces: `EconomyService.Balance`, `Add(int)`, `TrySpend(int)`; `CraftingService.CanCraft(string recipeId)`, `TryCraft(string recipeId)`.

- [ ] **Step 1: Écrire tests rouges économie**

Vérifier ajout de monnaie, refus d'un achat sans fonds et absence de solde négatif.

- [ ] **Step 2: Écrire tests rouges fabrication**

Avec deux unités de ressource, vérifier qu'une recette valide consomme exactement ses ingrédients et ajoute `home.small_plant`; sans ressource suffisante, aucun état ne change.

- [ ] **Step 3: Exécuter et confirmer les échecs**

Run: `dotnet test tests/MOMO.Tests.csproj --filter "EconomyServiceTests|CraftingServiceTests"`  
Expected: FAIL.

- [ ] **Step 4: Implémenter le minimum**

Montants entiers non négatifs. La fabrication valide d'abord toutes les entrées puis applique la transaction atomiquement en mémoire. Publier `currency_changed` et `item_crafted` après succès.

- [ ] **Step 5: Vérifier tests**

Run: même commande. Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/Inventory src/Economy src/Crafting tests/EconomyServiceTests.cs tests/CraftingServiceTests.cs
git commit -m "feat: add chapter economy and light crafting"
```

---

### Task 6: Logement et placement persistant

**Files:**
- Create: `src/Housing/HousingItem.cs`
- Create: `src/Housing/HousingPlacement.cs`
- Create: `src/Housing/HousingService.cs`
- Create: `scenes/housing/PlayerHome.tscn`
- Create: `scenes/ui/HousingPlacement.tscn`
- Create: `tests/HousingServiceTests.cs`

**Interfaces:**
- Produces: `HousingService.Place(itemId, slotId, transformData)`, `Remove(slotId)`, `Placements`; un slot ne contient qu'un placement.

- [ ] **Step 1: Écrire tests rouges**

Vérifier placement de `home.small_plant`, remplacement/refus contrôlé d'un slot occupé selon l'API choisie, suppression, et export/import d'une liste de placements.

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter HousingServiceTests`  
Expected: FAIL.

- [ ] **Step 3: Implémenter le modèle minimal**

Pour cette tranche, utiliser des emplacements/slots explicites dans la maison plutôt qu'un éditeur de construction libre. Conserver item ID, slot ID, position/rotation nécessaires. Le modèle doit pouvoir évoluer vers un placement plus libre sans changer les identifiants d'objets.

- [ ] **Step 4: Intégrer la maison Godot**

Créer une petite pièce chaleureuse avec plusieurs slots de décoration. L'UI permet de sélectionner `home.small_plant` et de le placer. Le visuel est un placeholder remplaçable par asset final ; aucune géométrie horrifique avancée maintenant.

- [ ] **Step 5: Vérifier tests + scène**

Run: test filtré. Expected: PASS.  
Godot Expected: entrer dans la maison, placer la décoration, sortir/revenir dans la scène sans perdre l'état en mémoire.

- [ ] **Step 6: Commit**

```powershell
git add src/Housing scenes/housing scenes/ui/HousingPlacement.tscn tests/HousingServiceTests.cs
git commit -m "feat: add persistent housing placement model"
```

---

### Task 7: Sauvegarde versionnée et restauration réelle

**Files:**
- Modify: `src/Save/WorldSaveData.cs`
- Modify: `src/Save/MomoMemoryData.cs`
- Modify: `src/Save/SaveService.cs`
- Modify: classes de sauvegarde existantes nécessaires
- Test: `tests/SaveServiceTests.cs`

**Interfaces:**
- Consumes: profil, état monde, inventaire, économie, quêtes, logement ; mémoire Momo séparée.
- Produces: `SaveWorld()`, `LoadWorld()`, `SaveMomoMemory()`, `LoadMomoMemory()` avec `SchemaVersion` explicite.

- [ ] **Step 1: Écrire tests rouges de round-trip**

Construire un monde avec profil, quête terminée, monnaie, ressource et plante placée. Sauvegarder vers un répertoire temporaire, recréer les services, charger et vérifier chaque valeur.

- [ ] **Step 2: Écrire test rouge de séparation temporelle**

Sauvegarder le monde, ajouter ensuite un événement persistant à Momo et sauvegarder sa mémoire. Recharger l'ancien monde : vérifier que la progression du monde recule mais que la mémoire Momo récente reste présente.

- [ ] **Step 3: Écrire test rouge de fichier invalide**

Créer un JSON monde invalide et une copie de sécurité valide. `LoadWorld()` doit refuser le fichier invalide et utiliser/indiquer la récupération sûre définie, sans écraser la copie saine.

- [ ] **Step 4: Vérifier les échecs**

Run: `dotnet test tests/MOMO.Tests.csproj --filter SaveServiceTests`  
Expected: FAIL sur le nouveau schéma/comportement.

- [ ] **Step 5: Implémenter versionnement et restauration**

Ajouter `SchemaVersion = 1`. Sérialiser uniquement des DTO, valider avant application, écrire dans un temporaire puis remplacer. Garder monde et mémoire Momo dans des fichiers distincts. Ajouter un adaptateur d'application d'état aux services vivants.

- [ ] **Step 6: Vérifier tests**

Run: même commande. Expected: PASS.

- [ ] **Step 7: Vérifier dans Godot**

Créer profil → terminer quête → fabriquer/placer plante → sauvegarder → fermer → relancer → charger. Expected: zone, profil, progression, inventaire, monnaie et plante sont visiblement restaurés ; mémoire Momo chargée séparément.

- [ ] **Step 8: Commit**

```powershell
git add src/Save tests/SaveServiceTests.cs
git commit -m "feat: restore versioned world and momo saves"
```

---

### Task 8: Flux de scènes et première zone du vrai jeu

**Files:**
- Create: `src/World/SceneFlowService.cs`
- Create: `scenes/chapter0/VillageEntry.tscn`
- Modify: `scenes/bootstrap/GameRoot.tscn`
- Create: `tests/SceneFlowServiceTests.cs`

**Interfaces:**
- Produces: `SceneFlowService.ChangeZone(string zoneId, string spawnId)`; zones connues `chapter0.village_entry`, `chapter0.player_home`.

- [ ] **Step 1: Écrire test rouge**

Vérifier qu'une demande vers une zone enregistrée produit zone/spawn cible, et qu'une zone inconnue est rejetée sans modifier l'état courant.

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter SceneFlowServiceTests`  
Expected: FAIL.

- [ ] **Step 3: Implémenter registre et transition**

Le service mappe des IDs stables vers PackedScenes. Avant changement, l'état de la zone sortante est capturé ; après chargement, le joueur est placé au spawn ID demandé. Ne pas stocker de chemins de scène dans les sauvegardes : stocker les IDs.

- [ ] **Step 4: Construire `VillageEntry.tscn`**

Créer une première zone de dessus plus proche d'un vrai jeu que la salle de test : chemin, quelques bâtiments/volumes, entrée de maison, PNJ et ressource de la quête. Utiliser des placeholders propres et remplaçables ; ne pas prétendre qu'il s'agit de l'art final.

- [ ] **Step 5: Vérifier**

Run: test filtré. Expected: PASS.  
Godot Expected: village → maison → village fonctionne, spawn correct et état conservé.

- [ ] **Step 6: Commit**

```powershell
git add src/World scenes/chapter0 scenes/bootstrap tests/SceneFlowServiceTests.cs
git commit -m "feat: add chapter zero scene flow"
```

---

### Task 9: Interface d'ouverture, Momo assistante et création du profil

**Files:**
- Create: `scenes/chapter0/Chapter0Intro.tscn`
- Create: `scenes/ui/ProfileSetup.tscn`
- Create: `scenes/ui/MomoAssistantPanel.tscn`
- Create/Modify: scripts UI C# correspondants
- Modify: `scenes/bootstrap/GameRoot.tscn`
- Test: `tests/ProfileSetupFlowTests.cs`

**Interfaces:**
- Consumes: `ProfileService`, `DialogueService`, `SaveService`, `SceneFlowService`.
- Produces: flux `Momo welcome -> name/appearance -> 0..5 important people -> confirm -> save -> village`.

- [ ] **Step 1: Écrire test rouge du flux de validation**

Tester le contrôleur logique sans Godot UI : profil incomplet ne confirme pas ; six personnes sont refusées ; zéro à cinq sont acceptées ; confirmation crée le profil une seule fois.

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter ProfileSetupFlowTests`  
Expected: FAIL.

- [ ] **Step 3: Implémenter le contrôleur de flux**

Séparer logique de formulaire et Nodes Godot. Les choix d'apparence de cette tranche sont des presets simples mais stockés par IDs stables. Les valeurs internes préexistantes prévues par le mystère peuvent être représentées par un état interne non affiché, sans produire d'effet horrifique maintenant.

- [ ] **Step 4: Construire l'UI chaleureuse**

Momo se présente comme assistante officielle. Elle explique les champs sans menace. Les cinq personnes sont présentées comme personnalisation facultative. Aucun texte ne promet de liaison automatique avec de vrais comptes.

- [ ] **Step 5: Vérifier tests + parcours Godot**

Run: test filtré. Expected: PASS.  
Godot Expected: nouveau lancement → Momo → création → personnes facultatives → confirmation → arrivée village. Recommencer après sauvegarde ne redemande pas le profil sauf action réelle de nouvelle partie/effacement.

- [ ] **Step 6: Commit**

```powershell
git add scenes/chapter0/Chapter0Intro.tscn scenes/ui src/Profile tests/ProfileSetupFlowTests.cs
git commit -m "feat: add momo-assisted chapter zero onboarding"
```

---

### Task 10: Boucle complète du premier contenu réel

**Files:**
- Modify: `content/chapter0/*.json`
- Modify: `scenes/chapter0/VillageEntry.tscn`
- Modify: `scenes/housing/PlayerHome.tscn`
- Modify: intégrations PNJ/ressource/crafting nécessaires
- Create: `tests/Chapter0LoopTests.cs`

**Interfaces:**
- Consumes: tous les services des Tasks 1–9.
- Produces: boucle jouable `onboarding -> village -> PNJ -> quête -> ressource/récompense -> craft -> maison -> décoration -> save/reload`.

- [ ] **Step 1: Écrire un test rouge de progression pure**

Simuler via services : créer profil, démarrer `village.first_help`, appliquer événement de collecte, terminer quête, attribuer récompense, fabriquer `home.small_plant`, placer la plante, produire un `WorldSaveData`. Vérifier les états finaux exacts.

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter Chapter0LoopTests`  
Expected: FAIL jusqu'à ce que toutes les intégrations exposent les interfaces attendues.

- [ ] **Step 3: Relier les scènes aux services**

Le PNJ utilise l'ID de dialogue et de quête. La ressource publie un événement. La récompense passe par inventaire/économie. La station de craft appelle `CraftingService`. La maison appelle `HousingService`. Aucun de ces Nodes ne modifie directement la mémoire interne de Momo.

- [ ] **Step 4: Ajouter les premiers événements narratifs innocents**

Publier des événements structurés comme `profile_created`, `chapter0_entered`, `first_npc_spoken`, `first_item_crafted`, `first_home_item_placed`. `NarrativeDirector` peut les observer et Momo peut répondre de manière rassurante ; aucune grosse anomalie n'est déclenchée dans cette tranche.

- [ ] **Step 5: Vérifier le test pur**

Run: test filtré. Expected: PASS.

- [ ] **Step 6: Vérification intégrale Godot**

Parcours manuel : nouveau jeu → création profil → village → parler au PNJ → quête → collecte → récompense → fabrication → maison → placement → sauvegarde → fermeture → relance → chargement.

Expected: aucun message d'erreur ; la progression et la décoration reviennent ; Momo conserve sa mémoire séparée ; l'ouverture ressemble au début d'un vrai jeu et non à la tranche verticale géométrique.

- [ ] **Step 7: Exécuter toute la suite automatisée**

Run:
```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: PASS. Si l'environnement agent n'a pas `dotnet`, exécuter cette étape sur la machine de développement avant de déclarer la tâche terminée.

- [ ] **Step 8: Commit**

```powershell
git add content scenes src tests
git commit -m "feat: complete chapter zero production foundation"
```

---

### Task 11: Outil développeur minimal et contrôle de régression

**Files:**
- Create: `src/Dev/DevStateService.cs`
- Create: UI/debug Node uniquement chargé en build/debug de développement
- Create: `tests/DevStateServiceTests.cs`
- Modify: documentation développeur si un README de projet existe

**Interfaces:**
- Produces: commandes de développement pour aller au village/maison, attribuer monnaie/ressource/décoration et fixer l'étape de quête du Chapitre 0 ; aucune disponibilité dans une build release normale.

- [ ] **Step 1: Écrire test rouge**

Vérifier qu'un preset `chapter0_after_quest` produit l'état attendu sans toucher à la mémoire persistante de Momo sauf demande explicite du preset.

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/MOMO.Tests.csproj --filter DevStateServiceTests`  
Expected: FAIL.

- [ ] **Step 3: Implémenter presets explicites**

Chaque preset appelle les APIs publiques des services ; il ne modifie pas leurs dictionnaires internes par réflexion ou accès caché. Le Node UI est conditionné aux builds de développement/debug.

- [ ] **Step 4: Vérifier**

Run: test filtré. Expected: PASS.  
Godot Expected: sauter directement aux états principaux du Chapitre 0 pour tester sans rejouer toute l'introduction.

- [ ] **Step 5: Régression finale**

Run:
```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: PASS.

Lancer également le parcours manuel complet de Task 10 et vérifier l'absence d'erreurs/warnings nouveaux dans Godot.

- [ ] **Step 6: Commit**

```powershell
git add src/Dev tests/DevStateServiceTests.cs README.md
git commit -m "dev: add chapter zero state jump tools"
```

---

## Definition of Done

La première fondation de production du Chapitre 0 est terminée lorsque :

- l'ouverture réelle remplace la salle de test pour un nouveau joueur ;
- Momo accompagne la création du profil comme assistante ;
- zéro à cinq personnes importantes peuvent être enregistrées sans liaison réseau ;
- village et logement sont des scènes distinctes et réutilisables ;
- une quête, un dialogue, une ressource et une recette sont chargés depuis les données ;
- économie, craft et décoration forment une boucle jouable ;
- monde et mémoire Momo sont sauvegardés séparément ;
- recharger restaure visiblement progression et logement ;
- le mode développeur permet de rejoindre rapidement les états utiles ;
- toute la suite de tests disponible passe sur un environnement disposant de .NET ;
- le parcours Godot complet est validé sans erreur nouvelle.