# GameScene_Demolition — Tâches manuelles dans Unity

## Concept

- **Terrain** = pas un prefab, ce sont des **éléments** (mesh, collider) que tu utilises en jeu pour construire un environnement visuel
- **ObstacleAnchors** → directement DANS les environnements (Env_Jour.prefab / Env_Nuit.prefab)
- **ObstacleSpawner** → 3 niveaux de difficulté (structures d'obstacles de plus en plus complexes)
- **Universal_Button** → point critique, tout le binding est automatique via `SetupInteractable()` dans ObstacleSpawner

---

## ✅ À faire dans Unity (GameScene)

### 1. Canvas — UI Texts

| GameObject | Type | Rôle |
|---|---|---|
| `ScoreText` | TMP Text | "Score: 0" |
| `TimerText` | TMP Text | Timer scène (60, 30...) |
| `SceneNumberText` | TMP Text | "Niveau en cours" |
| `GlobalTimerText` | TMP Text | "5:00" (timer global) |
| `FadeCanvas` | Image noire + CanvasGroup | Fondu noir entrée/sortie |

- [ ] Ajouter `ScoreText` dans Canvas (TMP, font 48, ancré haut-gauche)
- [ ] Ajouter `TimerText` dans Canvas (TMP, font 72, ancré haut)
- [ ] Ajouter `SceneNumberText` dans Canvas (TMP, font 24, ancré haut-droite)
- [ ] Ajouter `GlobalTimerText` dans Canvas (TMP, font 36, ancré bas-droite)
- [ ] Ajouter `FadeCanvas` : Image noire plein écran + CanvasGroup (alpha=1, blocksRaycasts=false)

### 2. GeneralVariable.prefab

- [ ] Glisser `Prefabs/GeneralVariable.prefab` dans la scène (racine)
- [ ] Vérifier `gameName = "Demolition"` dans le prefab (pas override scène)
- [ ] Vérifier `ScoreBoardDisplayer`, `Font`, `WinnerColor` assignés

### 3. DemolitionManager (déjà présent)

- [ ] **sceneNames[]** : `GameScene_Demolition_1`, `GameScene_Demolition_Night_1`...
- [ ] **Audio clips** : `sceneClearSound`, `gameOverSound`

### 4. EnvSpawner (unique empty dans la scène)

- [ ] Créer un empty `EnvSpawner` avec `Demolition_EnvironmentSpawner.cs`
- [ ] Remplir les tableaux :
  - `dayEnvPrefabs[]` → `Env_Jour_1`, `Env_Jour_2`... (contiennent ObstacleAnchors + décor)
  - `nightEnvPrefabs[]` → `Env_Nuit_1`, `Env_Nuit_2`...
  - `dayTerrainElements[]` → éléments de terrain jour (mesh sol, mesh décor)
  - `nightTerrainElements[]` → éléments de terrain nuit

**ObstacleAnchors sont DANS les prefabs Env_Jour / Env_Nuit**, pas dans la scène.

### 5. ObstacleSpawner (3 niveaux)

- [ ] Créer un empty `ObstacleSpawner` avec `Demolition_ObstacleSpawner.cs`
- [ ] Configurer 3 niveaux de difficulté :

| Niveau | Structures | Complexité |
|---|---|---|
| **Niveau 1** | 1-2 obstacles simples (caisses isolées) | Facile |
| **Niveau 2** | 3-5 obstacles + début de piles (caisses empilées) | Moyen |
| **Niveau 3** | 5+ obstacles + fantôme + structures complexes | Difficile |

- [ ] Le niveau augmente à chaque `EndScene()` → le GameManager passe le niveau courant à l'ObstacleSpawner
- [ ] Niveau 1 : spawn dans Anchor_1 uniquement
- [ ] Niveau 2 : spawn dans Anchor_1 + Anchor_2
- [ ] Niveau 3 : spawn dans Anchor_1 + Anchor_2 + Anchor_Fantome

### 6. StructuresParent (déjà présent)

- [ ] Vérifier position (0,0,0) — parent des obstacles spawnés

---

## ⚡ Comment Universal_Button est intégré (CRITIQUE)

**C'est la partie la plus importante.** Voici exactement comment ça marche :

### Les obstacles (caisses, barils, fantôme) reçoivent Universal_Button AUTOMATIQUEMENT

Dans `Demolition_ObstacleSpawner.SetupInteractable()` — appelé pour chaque obstacle spawné :

```csharp
private void SetupInteractable(GameObject obj)
{
    // 1. Ajoute un Collider 3D si manquant
    if (obj.GetComponent<Collider>() == null)
        obj.AddComponent<BoxCollider>();

    // 2. Ajoute un Rigidbody 3D si manquant
    if (obj.GetComponent<Rigidbody>() == null)
        obj.AddComponent<Rigidbody>();

    // 3. Ajoute Universal_Button (type 3D → Universal_Button standard)
    if (obj.GetComponent<Universal_Button>() == null)
    {
        var btn = obj.AddComponent<Universal_Button>();

        // 4. Ajoute Demolition_Pushable (script métier)
        var pushable = obj.GetComponent<Demolition_Pushable>();
        if (pushable == null)
            pushable = obj.AddComponent<Demolition_Pushable>();

        // 5. Binding automatique : Event.AddListener(pushable.OnPushed)
        btn.Event.AddListener(pushable.OnPushed);
    }
}
```

### Ce que ça donne dans Unity

**Pour chaque obstacle (caisse, baril, fantôme), les composants suivants sont automatiquement ajoutés au runtime :**

| Composant | Type | Rôle |
|---|---|---|
| `BoxCollider` | Collider 3D | Détection physique et OSC |
| `Rigidbody` | Rigidbody 3D | Physique (gravité, collisions) |
| `Universal_Button` | Universal_Button **3D** | Détection du toucher OSC |
| `Demolition_Pushable` | Script C# | Force d'impulsion au toucher |

### Quel type de Universal_Button utiliser

| Type de rendu | Universal_Button à utiliser |
|---|---|
| **3D** (MeshRenderer, Collider 3D) → caisses, barils, fantôme | `Universal_Button` (détection par Physics.Raycast) |
| **2D** (SpriteRenderer, Collider2D) | `Universal_Collider2DButton` |
| **Canvas UI** (RectTransform) | `Universal_Button` (détection Rect) |

→ **Pour Demolition 3D, c'est `Universal_Button` standard** (pas Collider2DButton, pas Mask).

### Donc dans Unity tu n'as RIEN à assigner pour Universal_Button

Les prefabs obstacles (`Caisse.prefab`, `Baril.prefab`, `Fantome.prefab`) n'ont PAS besoin d'avoir Universal_Button dans leur prefab — `SetupInteractable()` l'ajoute au moment du spawn.

**Ce que tu dois juste vérifier :** que tes prefabs obstacles ont bien :
- Un **Mesh** visible
- **Pas** de Collider déjà présent (le `SetupInteractable` en ajoute un — si déjà présent, pas de problème)
- **Pas** de Rigidbody déjà présent (pareil, ajouté que si absent)
- Le script `Demolition_Pushable.cs` ou `Demolition_Destructible.cs` ou `Demolition_Fantome.cs` selon le type

---

## ⚡ Préfabs à créer dans Prefabs/

### Environnements (avec ObstacleAnchors intégrés dedans)

| Prefab | Contient |
|---|---|
| `Env_Jour_1.prefab` | Sol + mesh décor + **ObstacleAnchors enfants** (positionnés pour ce décor) |
| `Env_Jour_2.prefab` | Variante avec obstacles placés différemment |
| `Env_Nuit_1.prefab` | Variante nuit |
| `Env_Nuit_2.prefab` | Variante nuit |

Chaque Env prefab contient dans sa hiérarchie :
```
Env_Jour_1.prefab
├── Sol (mesh décoratif + BoxCollider — ou pas, le sol peut être séparé)
├── Décor (mesh visuel)
├── ObstacleAnchors (empty)
│   ├── Anchor_N1_Obstacles (Demolition_ObstacleAnchor, prefabs[] N1, radius=2, min=1, max=2)
│   ├── Anchor_N2_Obstacles (Demolition_ObstacleAnchor, prefabs[] N2, radius=2, min=2, max=4)
│   └── Anchor_Fantome (Demolition_ObstacleAnchor, isFantomeAnchor=true, fantomePrefab)
```

**ObstacleSpawner** est un empty séparé dans la scène (pas dans les Env) — c'est lui qui parcourt tous les ObstacleAnchors trouvés avec `FindObjectsOfType`.

### Obstacles (instanciés par ObstacleSpawner)

| Prefab | Composants essentiels |
|---|---|
| `Caisse.prefab` | Mesh + **aucun composant requis** (SetupInteractable ajoute Collider+Rigidbody+Universal_Button+Pushable) |
| `Baril.prefab` | Mesh + **idem** |
| `Fantome.prefab` | Mesh + **Collider + Rigidbody** (nécessaires pour OnCollisionEnter) + `Demolition_Fantome.cs` |

Le Fantôme : il a déjà `Demolition_Fantome.cs` dans le prefab → `ObstacleSpawner.SpawnFantome()` ne lui ajoute pas de Universal_Button (c'est un obstacle à choc, pas à impact direct). Il utilise `OnCollisionEnter` avec `collision.impulse.magnitude` pour les dégâts.

### Éléments de terrain (pas des prefabs complets)

Ce sont des **éléments** que l'EnvSpawner combine au runtime :

| Élément | Type | Usage |
|---|---|---|
| `Sol_Jour` | Mesh + BoxCollider | Sol de base jour |
| `Sol_Nuit` | Mesh + BoxCollider | Sol de base nuit |
| `Decor_Arbre_1` | Mesh | Décor planté sur le sol |
| `Decor_Rocher_1` | Mesh | Décor |

L'EnvSpawner instancie `Sol_Jour` + pioche 1-2 décors aléatoires pour construire l'environnement.

---

## 🔧 Vérifications globales

- [ ] `Build Settings` → toutes les scènes Demolition ajoutées
- [ ] `GameScoreBoard.Demolition` existe dans `ScoreBoardManager.cs`
- [ ] Les **Mesh 3D** dans `Mesh/` sont assignés sur les éléments de terrain / obstacles
- [ ] `Canvas.m_RenderMode = 1` (Screen Space Camera)
- [ ] L'éclairage (Directional Light) réglé jour ou nuit selon le nom de la scène

---

## 📝 Rappel des règles

- **Ne pas éditer le YAML .unity**
- **Ne pas assigner manuellement** ce que les scripts trouvent via `GameObject.Find()`
- **Police du Menu** → ne pas toucher
- **Universal_Button 3D** est ajouté automatiquement par `ObstacleSpawner.SetupInteractable()` — pas besoin de le mettre dans les prefabs obstacles
- **Fantôme** n'a PAS de Universal_Button — il utilise `OnCollisionEnter`
- Les **ObstacleAnchors** sont dans les prefabs `Env_Jour` / `Env_Nuit`, pas dans la scène