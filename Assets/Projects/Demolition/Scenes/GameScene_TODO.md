# GameScene_Demolition — Tâches manuelles dans Unity

## État actuel

La GameScene contient : Camera, Light, EventSystem, Canvas (vide), Ground, StructuresParent, DemolitonManager.

**L'idée :** Un seul `EnvSpawner` spawn un environnement complet (Terrain + Décor + ObstacleAnchors intégrés). Les ObstacleAnchors ne sont pas dans la scène — ils sont DANS les prefabs Terrain.

---

## ✅ À faire dans Unity

### 1. Canvas — UI Texts

| GameObject | Type | Rôle |
|---|---|---|
| `ScoreText` | TMP Text | "Score: 0" |
| `TimerText` | TMP Text | Timer scène (60, 30...) |
| `SceneNumberText` | TMP Text | "Niveau en cours" |
| `GlobalTimerText` | TMP Text | "5:00" (timer global) |
| `FadeCanvas` | Canvas (Image noire + CanvasGroup) | Fondu noir entrée/sortie |

- [ ] Ajouter `ScoreText` dans Canvas (TMP, font 48, ancré en haut à gauche)
- [ ] Ajouter `TimerText` dans Canvas (TMP, font 72, ancré en haut)
- [ ] Ajouter `SceneNumberText` dans Canvas (TMP, font 24, ancré en haut à droite)
- [ ] Ajouter `GlobalTimerText` dans Canvas (TMP, font 36, ancré en bas à droite)
- [ ] Ajouter `FadeCanvas` : Image noire plein écran + CanvasGroup (alpha=1, blocksRaycasts=false)

### 2. GeneralVariable.prefab

- [ ] Glisser `Prefabs/GeneralVariable.prefab` dans la scène (racine)
- [ ] Vérifier que `gameName = "Demolition"` dans le prefab (pas dans l'override scène)
- [ ] Vérifier que `ScoreBoardDisplayer`, `Font` et `WinnerColor` sont assignés dans le prefab

### 3. DemolitionManager (déjà présent)

- [ ] **sceneNames[]** — Ajouter toutes les scènes dans l'inspecteur, ex :
  - `GameScene_Demolition_1`, `GameScene_Demolition_2`
  - `GameScene_Demolition_Night_1`, `GameScene_Demolition_Night_2`
- [ ] **Audio clips** — `sceneClearSound`, `gameOverSound`

### 4. EnvSpawner (unique empty dans la scène)

- [ ] Créer **un seul** empty `EnvSpawner` avec `Demolition_EnvironmentSpawner.cs`
- [ ] Remplir les tableaux :

| Champ | Prefabs |
|---|---|
| `dayTerrainPrefabs[]` | Terrain_Jour_1, Terrain_Jour_2... (contiennent ObstacleAnchors + décor + sol) |
| `nightTerrainPrefabs[]` | Terrain_Nuit_1, Terrain_Nuit_2... |
| `dayEnvPrefabs[]` | Env_Jour_1, Env_Jour_2... (décor uniquement) |
| `nightEnvPrefabs[]` | Env_Nuit_1, Env_Nuit_2... |

**ObstacleAnchors et ObstacleSpawner sont DANS les prefabs Terrain**, pas dans la scène.

### 5. StructuresParent (déjà présent)

- [ ] Vérifier qu'il est à (0,0,0) — parent des obstacles spawnés

---

## ⚡ Préfabs à créer dans Prefabs/

### Terrains (avec ObstacleAnchors intégrés)

| Prefab | Contient |
|---|---|
| `Terrain_Jour_1.prefab` | Sol + mesh décor + BoxCollider + **ObstacleSpawner (`Demolition_ObstacleSpawner`)** + **ObstacleAnchors enfants avec positions + radius** |
| `Terrain_Jour_2.prefab` | Variante avec obstacles placés différemment |
| `Terrain_Nuit_1.prefab` | Variante nuit |
| `Terrain_Nuit_2.prefab` | Variante nuit |

Chaque Terrain prefab contient dans sa hiérarchie :
```
Terrain_Jour_1.prefab
├── ObstacleSpawner (Demolition_ObstacleSpawner)
├── ObstacleAnchors (empty)
│   ├── Anchor_Fantome (Demolition_ObstacleAnchor, isFantomeAnchor=true, fantomePrefab assigné)
│   └── Anchor_Obstacles_1 (Demolition_ObstacleAnchor, obstaclePrefabs[], radius=2, min=1, max=3)
├── Sol (mesh + BoxCollider)
└── Décor (mesh visuel)
```

### Environnements (décor uniquement, pas d'obstacles)

| Prefab | Contenu |
|---|---|
| `Env_Jour_1.prefab` | Décors arrière-plan jour |
| `Env_Jour_2.prefab` | Variante |
| `Env_Nuit_1.prefab` | Décors arrière-plan nuit |
| `Env_Nuit_2.prefab` | Variante |

### Obstacles (instanciés par ObstacleSpawner)

| Prefab | Composants |
|---|---|
| `Caisse.prefab` | Mesh + BoxCollider + Rigidbody + `Demolition_Destructible` |
| `Baril.prefab` | Mesh + BoxCollider + Rigidbody + `Demolition_Destructible` |
| `Fantome.prefab` | Mesh + BoxCollider + Rigidbody + `Demolition_Fantome` |

### Impact

| Prefab | Composants |
|---|---|
| `ImpactExplosion.prefab` | Particle System + audio (existe déjà) |

---

## 🔧 Vérifications globales

- [ ] `Build Settings` → toutes les scènes Demolition sont ajoutées (Accueil, Menu, GameScene_Demolition_1, GameScene_Demolition_Night_1..., Score)
- [ ] `GameScoreBoard.Demolition` existe dans `ScoreBoardManager.cs` (enum)
- [ ] Les **Mesh** 3D des scènes achetées sont dans `Mesh/` (cage.fbx, etc.) — à assigner sur les prefabs Terrain/Env/Obstacles
- [ ] Les **Sprites** 2D dans `Sprites/` sont redimensionnés aux bonnes tailles (64×32 blocs, etc.)
- [ ] `Canvas.m_RenderMode = 1` (Screen Space Camera) — sinon les sprites 2D ne s'affichent pas
- [ ] L'éclairage (Directional Light) est réglé jour ou nuit selon le nom de la scène

---

## 📝 Rappel des règles

- **Ne pas éditer le YAML .unity** → tout se fait dans l'éditeur Unity
- **Ne pas assigner manuellement** ce que les scripts trouvent via `GameObject.Find()` (ScoreText, TimerText, etc.)
- **Police du Menu** → ne pas toucher
- Pour les scripts, fichiers `.cs` uniquement — pas de modification YAML/prefab à la main