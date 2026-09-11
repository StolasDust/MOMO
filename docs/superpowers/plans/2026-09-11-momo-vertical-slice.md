# MOMO Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construire une première tranche verticale locale et jouable de MOMO qui prouve déplacement 2D, interaction, dialogue, quête, inventaire, énigme, combat, sauvegarde, Directeur narratif, mémoire minimale de Momo et première anomalie contrôlée.

**Architecture:** Godot 4.7.2 .NET avec C#. Le gameplay publie des événements dans un `EventBus`; `NarrativeDirector` les évalue sans injecter la logique de Momo dans chaque scène. `GameStateService`, `MomoStateService` et `SaveService` restent séparés. Les services réseau/Twitch/webcam/micro/loupe avancée sont hors de cette tranche et ne doivent pas être implémentés maintenant.

**Tech Stack:** Godot 4.7.2 .NET, C#, .NET SDK compatible avec Godot 4.7.2, JSON local via `System.Text.Json`, tests C# séparés des scènes lorsque la logique est pure.

**Spec:** `docs/superpowers/specs/2026-09-11-momo-design.md`

## Global Constraints

- Moteur : Godot 4.7.2 .NET.
- Langage : C#.
- Architecture : hybride modulaire.
- Le cœur du jeu doit rester jouable localement et hors ligne.
- Les scènes ne doivent pas connaître les détails internes de Momo.
- La mémoire de Momo doit rester distincte de la sauvegarde du monde.
- Ne pas implémenter Twitch, serveur d'amis, webcam, microphone ou loupe complète dans cette tranche.
- Les effets de corruption/plantage utilisés narrativement sont simulés et ne doivent pas endommager les vraies sauvegardes.
- YAGNI : aucune infrastructure réseau ou méta-horreur réelle tant que le socle local n'est pas validé.

---

## File Structure

```text
MOMO/
├─ project.godot
├─ MOMO.csproj
├─ scenes/
│  ├─ Main.tscn
│  ├─ world/VerticalSlice.tscn
│  ├─ actors/Player.tscn
│  ├─ actors/Momo.tscn
│  ├─ actors/Npc.tscn
│  ├─ combat/TrainingEnemy.tscn
│  └─ ui/DialogueBox.tscn
├─ scripts/
│  ├─ actors/PlayerController.cs
│  ├─ actors/Interactable.cs
│  ├─ actors/NpcController.cs
│  ├─ combat/Combatant.cs
│  ├─ combat/TrainingEnemy.cs
│  ├─ core/GameEvent.cs
│  ├─ core/EventBus.cs
│  ├─ core/GameState.cs
│  ├─ core/GameStateService.cs
│  ├─ narrative/NarrativeDirector.cs
│  ├─ narrative/MomoState.cs
│  ├─ narrative/MomoStateService.cs
│  ├─ narrative/DialogueLine.cs
│  ├─ narrative/DialogueService.cs
│  ├─ quests/QuestState.cs
│  ├─ quests/QuestService.cs
│  ├─ inventory/InventoryService.cs
│  ├─ puzzles/DoorPuzzle.cs
│  └─ save/SaveService.cs
├─ tests/
│  ├─ MOMO.Tests.csproj
│  ├─ EventBusTests.cs
│  ├─ GameStateTests.cs
│  ├─ MomoStateTests.cs
│  ├─ QuestServiceTests.cs
│  ├─ InventoryServiceTests.cs
│  ├─ NarrativeDirectorTests.cs
│  └─ SaveServiceTests.cs
└─ docs/superpowers/
   ├─ specs/2026-09-11-momo-design.md
   └─ plans/2026-09-11-momo-vertical-slice.md
```

Chaque fichier a une responsabilité unique. Les scènes Godot composent les objets; la logique testable reste autant que possible dans les classes C#.

---

### Task 1: Bootstrap Godot C# et test de fumée

**Files:**
- Create: `project.godot`
- Create: `MOMO.csproj`
- Create: `scenes/Main.tscn`
- Create: `tests/MOMO.Tests.csproj`
- Create: `tests/GameStateTests.cs`

**Interfaces:**
- Consumes: aucune.
- Produces: projet Godot C# chargeable; projet de tests compilable; convention `MOMO.*`.

- [ ] **Step 1: Vérifier l'environnement avant d'écrire du gameplay**

Run:
```powershell
dotnet --info
```
Expected: un SDK .NET compatible est listé. Si `dotnet` n'est pas reconnu, installer le SDK requis par Godot 4.7.2 .NET avant de continuer.

- [ ] **Step 2: Créer le projet C# minimal**

`MOMO.csproj`:
```xml
<Project Sdk="Godot.NET.Sdk/4.7.2">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <RootNamespace>MOMO</RootNamespace>
  </PropertyGroup>
</Project>
```

`project.godot` doit déclarer `scenes/Main.tscn` comme scène principale et définir les actions `move_left`, `move_right`, `move_up`, `move_down`, `interact` et `attack`.

- [ ] **Step 3: Créer le test de fumée**

`tests/MOMO.Tests.csproj` référence `../MOMO.csproj` et utilise xUnit. `tests/GameStateTests.cs` contient d'abord un test volontairement rouge référençant `MOMO.Core.GameState`.

```csharp
using MOMO.Core;
using Xunit;

namespace MOMO.Tests;

public sealed class GameStateTests
{
    [Fact]
    public void NewGameStartsAtVerticalSlice()
    {
        var state = new GameState();
        Assert.Equal("vertical_slice", state.CurrentRegionId);
    }
}
```

- [ ] **Step 4: Vérifier l'échec attendu**

Run:
```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: FAIL car `GameState` n'existe pas encore.

- [ ] **Step 5: Commit**

```powershell
git add project.godot MOMO.csproj scenes/Main.tscn tests
git commit -m "build: bootstrap Godot C# project"
```

---

### Task 2: État du monde et EventBus

**Files:**
- Create: `scripts/core/GameState.cs`
- Create: `scripts/core/GameStateService.cs`
- Create: `scripts/core/GameEvent.cs`
- Create: `scripts/core/EventBus.cs`
- Modify: `tests/GameStateTests.cs`
- Create: `tests/EventBusTests.cs`

**Interfaces:**
- Produces: `GameState.CurrentRegionId`, `GameState.Flags`, `GameStateService.State`, `GameEvent(string Type, IReadOnlyDictionary<string,string> Data)`, `EventBus.Publish(GameEvent)`, `EventBus.Subscribe(string, Action<GameEvent>)`.

- [ ] **Step 1: Écrire les tests rouges**

```csharp
[Fact]
public void FlagCanBePersistedInWorldState()
{
    var state = new GameState();
    state.Flags["met_momo"] = true;
    Assert.True(state.Flags["met_momo"]);
}
```

```csharp
[Fact]
public void EventBusDeliversMatchingEventOnce()
{
    var bus = new EventBus();
    var count = 0;
    bus.Subscribe("door_opened", _ => count++);
    bus.Publish(new GameEvent("door_opened", new Dictionary<string,string>()));
    Assert.Equal(1, count);
}
```

- [ ] **Step 2: Exécuter et confirmer l'échec**

```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: FAIL sur les types manquants.

- [ ] **Step 3: Implémenter le minimum**

`GameState` initialise `CurrentRegionId = "vertical_slice"` et `Flags = new Dictionary<string,bool>()`. `GameStateService` expose une propriété `State`. `EventBus` conserve les abonnements par type et appelle uniquement les abonnés du type publié.

- [ ] **Step 4: Vérifier les tests**

```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add scripts/core tests
git commit -m "feat: add world state and event bus"
```

---

### Task 3: Joueur, déplacement et interaction

**Files:**
- Create: `scenes/actors/Player.tscn`
- Create: `scripts/actors/PlayerController.cs`
- Create: `scripts/actors/Interactable.cs`
- Create: `scenes/world/VerticalSlice.tscn`
- Modify: `scenes/Main.tscn`

**Interfaces:**
- Consumes: `EventBus.Publish(GameEvent)`.
- Produces: `PlayerController`; interaction avec le plus proche `Interactable`; événement `player_interacted`.

- [ ] **Step 1: Construire Player.tscn**

Racine `CharacterBody2D`, enfant `CollisionShape2D`, visuel temporaire simple et `Area2D` d'interaction. Attacher `PlayerController.cs`.

- [ ] **Step 2: Implémenter le déplacement minimal**

Dans `_PhysicsProcess`, utiliser `Input.GetVector("move_left", "move_right", "move_up", "move_down")`, multiplier par une propriété exportée `Speed = 180f`, affecter `Velocity`, puis appeler `MoveAndSlide()`.

- [ ] **Step 3: Implémenter l'interaction**

`Interactable` expose `public virtual void Interact(PlayerController player)`. Lorsque `interact` est pressé, `PlayerController` sélectionne l'interactable chevauchant sa zone et appelle `Interact` une seule fois par pression.

- [ ] **Step 4: Test manuel Godot**

Run:
```powershell
godot --path . --editor
```
Expected: `Main.tscn` lance `VerticalSlice.tscn`; le personnage se déplace dans quatre directions, collisionne avec les limites et l'action d'interaction ne produit aucune erreur.

- [ ] **Step 5: Commit**

```powershell
git add scenes scripts/actors
git commit -m "feat: add player movement and interaction"
```

---

### Task 4: Dialogue et premier PNJ

**Files:**
- Create: `scripts/narrative/DialogueLine.cs`
- Create: `scripts/narrative/DialogueService.cs`
- Create: `scripts/actors/NpcController.cs`
- Create: `scenes/actors/Npc.tscn`
- Create: `scenes/ui/DialogueBox.tscn`
- Modify: `scenes/world/VerticalSlice.tscn`

**Interfaces:**
- Produces: `DialogueLine(string Speaker, string Text)`, `DialogueService.Start(IReadOnlyList<DialogueLine>)`, `DialogueService.Advance()`, événement `dialogue_finished`.

- [ ] **Step 1: Créer un dialogue fixe de tranche verticale**

Le PNJ donne exactement trois lignes de test : accueil, demande de retrouver une clé, indication que Momo connaît le chemin.

- [ ] **Step 2: Implémenter DialogueService**

Le service conserve la liste active et l'index. `Start` commence à 0; `Advance` passe à la ligne suivante; après la dernière ligne il ferme l'UI et publie `dialogue_finished` avec `dialogue_id=key_quest_intro`.

- [ ] **Step 3: Connecter DialogueBox**

Afficher nom du locuteur et texte; `interact` avance lorsque le dialogue est actif et ne déclenche pas simultanément une nouvelle interaction monde.

- [ ] **Step 4: Test manuel**

Expected: approcher le PNJ, appuyer sur interaction, lire les trois lignes, fermer le dialogue sans double déclenchement.

- [ ] **Step 5: Commit**

```powershell
git add scenes scripts/narrative scripts/actors
git commit -m "feat: add dialogue and first npc"
```

---

### Task 5: Quête et inventaire minimal

**Files:**
- Create: `scripts/quests/QuestState.cs`
- Create: `scripts/quests/QuestService.cs`
- Create: `scripts/inventory/InventoryService.cs`
- Create: `tests/QuestServiceTests.cs`
- Create: `tests/InventoryServiceTests.cs`

**Interfaces:**
- Produces: `QuestService.StartQuest(string)`, `QuestService.CompleteQuest(string)`, `QuestService.GetState(string)`, `InventoryService.Add(string)`, `InventoryService.Contains(string)`, `InventoryService.Remove(string)`.

- [ ] **Step 1: Écrire les tests rouges**

Tester que `key_quest` passe `NotStarted -> Active -> Completed` et que `InventoryService` ajoute, détecte et retire `old_key`.

- [ ] **Step 2: Vérifier l'échec**

```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: FAIL sur les services absents.

- [ ] **Step 3: Implémenter les services**

`QuestState` est un enum `NotStarted`, `Active`, `Completed`. Les transitions publient `quest_started`/`quest_completed`. L'inventaire utilise un `HashSet<string>` pour cette tranche et publie `item_added`/`item_removed`.

- [ ] **Step 4: Connecter la quête**

À la fin de `key_quest_intro`, démarrer `key_quest`. Ajouter dans la zone un objet `old_key` interactif qui disparaît après collecte.

- [ ] **Step 5: Tester et commit**

```powershell
dotnet test tests/MOMO.Tests.csproj
git add scripts scenes tests
git commit -m "feat: add key quest and inventory"
```
Expected: PASS et quête jouable.

---

### Task 6: Énigme de porte

**Files:**
- Create: `scripts/puzzles/DoorPuzzle.cs`
- Modify: `scenes/world/VerticalSlice.tscn`

**Interfaces:**
- Consumes: `InventoryService.Contains("old_key")`, `Remove("old_key")`.
- Produces: événements `door_opened` avec `door_id=sealed_door` et `puzzle_solved` avec `puzzle_id=sealed_door`.

- [ ] **Step 1: Ajouter une porte bloquée**

La porte est un `StaticBody2D` interactif avec collision active tant que `IsOpen == false`.

- [ ] **Step 2: Implémenter DoorPuzzle**

Sans `old_key`, interaction publie `door_locked`. Avec la clé, retirer `old_key`, désactiver la collision, rendre la porte ouverte et publier les deux événements de réussite.

- [ ] **Step 3: Test manuel sans clé**

Expected: porte reste fermée, aucune progression.

- [ ] **Step 4: Test manuel avec clé**

Expected: clé consommée, porte ouverte, collision retirée exactement une fois.

- [ ] **Step 5: Commit**

```powershell
git add scripts/puzzles scenes/world/VerticalSlice.tscn
git commit -m "feat: add sealed door puzzle"
```

---

### Task 7: Combat simple

**Files:**
- Create: `scripts/combat/Combatant.cs`
- Create: `scripts/combat/TrainingEnemy.cs`
- Create: `scenes/combat/TrainingEnemy.tscn`
- Modify: `scripts/actors/PlayerController.cs`
- Modify: `scenes/world/VerticalSlice.tscn`

**Interfaces:**
- Produces: `Combatant.TakeDamage(int)`, `Combatant.IsDead`, événements `player_attacked`, `enemy_damaged`, `enemy_defeated`.

- [ ] **Step 1: Implémenter Combatant**

`Combatant` possède `MaxHealth`, `Health`, `TakeDamage(int amount)` et protège contre les dégâts après la mort.

- [ ] **Step 2: Ajouter l'attaque du joueur**

L'action `attack` active brièvement une zone devant le joueur; chaque cible ne peut recevoir qu'un coup par activation.

- [ ] **Step 3: Créer TrainingEnemy**

Ennemi avec 3 PV, immobile pour la tranche. À 0 PV il publie `enemy_defeated` avec `enemy_id=training_shadow` puis se retire de la scène.

- [ ] **Step 4: Test manuel**

Expected: trois attaques valides éliminent l'ennemi; maintenir la touche ne provoque pas une rafale infinie; aucun crash après disparition.

- [ ] **Step 5: Commit**

```powershell
git add scripts/combat scripts/actors scenes
git commit -m "feat: add vertical slice combat"
```

---

### Task 8: MomoState et Directeur narratif

**Files:**
- Create: `scripts/narrative/MomoState.cs`
- Create: `scripts/narrative/MomoStateService.cs`
- Create: `scripts/narrative/NarrativeDirector.cs`
- Create: `tests/MomoStateTests.cs`
- Create: `tests/NarrativeDirectorTests.cs`
- Create: `scenes/actors/Momo.tscn`
- Modify: `scenes/world/VerticalSlice.tscn`

**Interfaces:**
- Produces: `MomoState.DeathsObserved`, `MomoState.SeenEventIds`, `MomoStateService.State`, `NarrativeDirector.Handle(GameEvent)`; événement `momo_reaction_requested`.

- [ ] **Step 1: Écrire le test de mémoire séparée**

Créer un `MomoState`, enregistrer `door_opened:sealed_door`, recréer un `GameState` et vérifier que le `MomoState` conserve l'événement indépendamment du monde.

- [ ] **Step 2: Écrire le test de première anomalie**

Quand `NarrativeDirector` reçoit pour la première fois `door_opened` avec `door_id=sealed_door`, il doit publier exactement une réaction `momo_reaction_requested` contenant `reaction_id=first_anomaly`.

- [ ] **Step 3: Vérifier les échecs puis implémenter**

```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected avant implémentation: FAIL. Implémenter ensuite le minimum pour faire passer les tests.

- [ ] **Step 4: Connecter Momo à la scène**

Momo commence comme accompagnatrice rassurante. Après ouverture de la porte, attendre un court délai puis afficher une phrase anormale unique liée à `first_anomaly`. La scène ne choisit pas elle-même cette réaction : elle répond uniquement à `momo_reaction_requested`.

- [ ] **Step 5: Vérifier et commit**

```powershell
dotnet test tests/MOMO.Tests.csproj
git add scripts/narrative scenes tests
git commit -m "feat: add Momo memory and narrative director"
```
Expected: PASS.

---

### Task 9: Sauvegarde atomique du monde et mémoire Momo séparée

**Files:**
- Create: `scripts/save/SaveService.cs`
- Create: `tests/SaveServiceTests.cs`
- Modify: `scripts/core/GameState.cs`
- Modify: `scripts/narrative/MomoState.cs`

**Interfaces:**
- Produces: `SaveService.SaveWorld(string path, GameState state)`, `LoadWorld(string path)`, `SaveMomo(string path, MomoState state)`, `LoadMomo(string path)`.

- [ ] **Step 1: Écrire les tests rouges**

Tester un round-trip de `GameState`, un round-trip de `MomoState` et vérifier que charger un ancien monde n'écrase pas l'instance de mémoire Momo chargée séparément.

- [ ] **Step 2: Ajouter le test de sauvegarde de sécurité**

Sauvegarder deux fois le monde et vérifier que la seconde écriture conserve un fichier `.bak` lisible contenant la version précédente.

- [ ] **Step 3: Implémenter l'écriture sûre**

Sérialiser en JSON vers `path + ".tmp"`; fermer le flux; déplacer l'ancien fichier vers `.bak`; remplacer ensuite le fichier principal par le `.tmp`. Ne jamais simuler une corruption sur ces fichiers réels.

- [ ] **Step 4: Exécuter les tests**

```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: PASS pour round-trip, séparation Momo/monde et backup.

- [ ] **Step 5: Commit**

```powershell
git add scripts/save scripts/core scripts/narrative tests/SaveServiceTests.cs
git commit -m "feat: add safe layered saves"
```

---

### Task 10: Intégration de la boucle verticale complète

**Files:**
- Modify: `scenes/world/VerticalSlice.tscn`
- Modify: `scenes/Main.tscn`
- Modify: `project.godot`
- Create: `docs/vertical-slice-test-checklist.md`

**Interfaces:**
- Consumes: tous les services précédents.
- Produces: boucle jouable complète et checklist de validation reproductible.

- [ ] **Step 1: Assembler le parcours**

Ordre attendu : apparition → rencontre rassurante avec Momo → PNJ → quête de clé → collecte → porte/énigme → combat simple → première anomalie de Momo → point de sauvegarde.

- [ ] **Step 2: Écrire la checklist manuelle exacte**

`docs/vertical-slice-test-checklist.md` doit contenir : lancement propre; quatre directions; collisions; dialogue; quête active; collecte clé; porte refusée sans clé; porte ouverte avec clé; combat en trois coups; anomalie unique; sauvegarde; fermeture; relance; chargement; monde restauré; mémoire Momo restaurée séparément; aucune dépendance Internet.

- [ ] **Step 3: Exécuter tous les tests automatisés**

```powershell
dotnet test tests/MOMO.Tests.csproj
```
Expected: 100% PASS.

- [ ] **Step 4: Exécuter la checklist dans Godot**

```powershell
godot --path . --editor
```
Expected: chaque ligne de `docs/vertical-slice-test-checklist.md` passe sans erreur bloquante dans le débogueur Godot.

- [ ] **Step 5: Commit final de la tranche**

```powershell
git add project.godot scenes scripts tests docs/vertical-slice-test-checklist.md
git commit -m "feat: complete MOMO vertical slice foundation"
```

---

## Self-Review

- La tranche couvre tous les éléments exigés par la section 17 de la spec : région, déplacement, PNJ, dialogue, quête, inventaire, énigme, combat, sauvegarde, Momo, Directeur narratif, mémoire minimale et anomalie.
- Les sous-systèmes indépendants prévus plus tard par la spec sont volontairement exclus : amis, Twitch, webcam, micro, méta-horreur avancée et loupe complète.
- Aucun service extérieur n'est requis pour terminer la tranche.
- La sauvegarde du monde et la mémoire de Momo ont des fichiers et API distincts.
- Les scènes publient/consomment des événements et ne décident pas directement des règles internes de Momo.
- Les tâches ont chacune un livrable testable et un commit dédié.
- Aucun placeholder `TODO`/`TBD` n'est requis pour exécuter cette tranche.
