# Demolition 3D — Setup Guide

## Architecture actuelle

Impact OSC direct (pas de projectile), fantôme se casse par force de choc, obstacles (caisses/barils) avec ObstacleAnchor. Timer scène + global, score, scènes aléatoires, fondu noir.

---

## Scripts (ce qui est utilisé)

| Script | Rôle |
|---|---|
| `Demolition_GameManager.cs` | Chef d'orchestre. MonoBehaviour pur (pas ReceiveParent). Singleton. Timers, score, fondu, audio, slow-mo |
| `Demolition_GeneralVariables.cs` | Constantes PlayerPrefs + helpers statiques (`GetSceneDurationFromPrefs()`, `GetGlobalTimeFromPrefs()`) |
| `Demolition_Fantome.cs` | Fantôme 3D : HP, dégâts par force de choc (`OnCollisionEnter`), mort → notifie GameManager |
| `Demolition_ObstacleSpawner.cs` | Au start, lit les ObstacleAnchor, spawn obstacles/fantôme, ajoute Universal_Button + Pushable |
| `Demolition_ObstacleAnchor.cs` | Zone de spawn (radius, prefabs, min/max count, isFantomeAnchor) |
| `Demolition_Pushable.cs` | Force d'impulsion 3D, bindé par ObstacleSpawner via `button.Event.AddListener(pushable.OnPushed)` |
| `Demolition_Destructible.cs` | Caisses/barils destructibles : HP, flash, destruction |
| `Demolition_PopupText.cs` | Popup score flottant avec overshoot bounce |
| `Demolition_DebrisSpawner.cs` | Particules, débris, poussière, étoiles |
| `Demolition_EnvironmentSpawner.cs` | Instancie Terrain + Env au Start() selon jour/nuit |

### Scripts obsolètes (2D — à ne pas utiliser)

`Demolition_Block.cs`, `Demolition_Structure.cs`, `Demolition_StructureBuilder.cs`, `Demolition_ScrollingBackground.cs`, `Demolition_GroundScroll.cs`, `Demolition_Projectile.cs`, `Demolition_PigBehavior.cs`, `Demolition_DataLoader.cs`

---

## Préfabs à créer (dans Prefabs/)

Ces préfabs sont instanciés **par script** (`Demolition_EnvironmentSpawner`) au runtime, pas placés dans la scène.

### Terrains (sol + ground)

| Prefab | Usage | Contenu |
|---|---|---|
| `Terrain.prefab` | Jour | Sol, BoxCollider, mesh décor |
| `TerrainNight.prefab` | Nuit | Sol, BoxCollider, mesh décor version nuit |

### Environnements (décor background 3D)

| Prefab | Usage | Contenu |
|---|---|---|
| `Env1Sun.prefab` | Jour — var1 | Décors, arrière-plan |
| `Env1Night.prefab` | Nuit — var1 | Décors, arrière-plan version nuit |
| `Env2Sun.prefab` | Jour — var2 | Décors, arrière-plan |
| `Env2Night.prefab` | Nuit — var2 | Décors, arrière-plan version nuit |
| `Env3Sun.prefab` | Jour — var3 | Décors, arrière-plan |
| `Env3Night.prefab` | Nuit — var3 | Décors, arrière-plan version nuit |

### EnvSpawner — champs à remplir

| Champ | Type | Description |
|---|---|---|
| `dayTerrainPrefabs[]` | GameObject[] | Terrains jour (Terrain.prefab...) |
| `nightTerrainPrefabs[]` | GameObject[] | Terrains nuit (TerrainNight.prefab...) |
| `dayEnvPrefabs[]` | GameObject[] | Env jour (Env1Sun, Env2Sun, Env3Sun) |
| `nightEnvPrefabs[]` | GameObject[] | Env nuit (Env1Night, Env2Night, Env3Night) |
| `nightSceneKeywords[]` | string[] | Mots-clés détectant la nuit (par défaut: Night, Nuit) |

L'EnvSpawner pioche aléatoirement dans la bonne liste au `Start()`.

## Convention de nommage des scènes (jour/nuit)

Le nom de la scène détermine le jour ou la nuit :

| Nom de scène | Éclairage | Terrain + Env |
|---|---|---|
| `GameScene_Demolition_1` | **Jour** (Directional Light jaune, skybox claire) | Pioche dans les prefabs jour |
| `GameScene_Demolition_2` | **Jour** | Pioche dans les prefabs jour |
| `GameScene_Demolition_Night_1` | **Nuit** (Directional Light bleutée, skybox sombre) | Pioche dans les prefabs nuit |
| `GameScene_Demolition_Night_2` | **Nuit** | Pioche dans les prefabs nuit |

**Règle :** si le nom contient "Night" ou "Nuit" → nuit. Sinon → jour.

L'**éclairage** (Directional Light, skybox, ambient color) est réglé **dans chaque fichier .unity** de la scène — c'est le seul élément qui est dans la scène et pas dans un prefab. Le GameManager pioche aléatoirement parmi **toutes** les scènes (jour + nuit) dans `sceneNames[]`.

## Ce que doit contenir chaque GameScene (setup minimal)

```
GameScene_Demolition.unity
├── Main Camera (perspective, 3D)
├── Directional Light (réglé jour/nuit selon le nom de la scène)
├── EventSystem
├── Canvas (Screen Space - Camera)
│   ├── ScoreText (TMP)          — "Score: 0"
│   ├── TimerText (TMP)           — "60"
│   ├── SceneNumberText (TMP)     — "Niveau en cours"
│   ├── GlobalTimerText (TMP)     — "5:00"
│   └── FadeCanvas (Image noire + CanvasGroup, alpha=0)
├── GeneralVariable.prefab        — Demolition_GeneralVariables + OSC_Manager enfant
├── GameManager (empty)
│   └── Demolition_GameManager.cs — singleton + AudioSource (auto-ajoutée)
├── ObstacleSpawner (empty)
│   └── Demolition_ObstacleSpawner.cs
├── ObstacleAnchors (empty)
│   ├── Anchor_Fantome
│   │   └── Demolition_ObstacleAnchor (isFantomeAnchor=true, fantomePrefab)
│   └── Anchor_Obstacles_N
│       └── Demolition_ObstacleAnchor (obstaclePrefabs[], spawnRadius, minCount, maxCount)
├── EnvSpawner (empty)
│   └── Demolition_EnvironmentSpawner.cs — jour/nuit terrain+env pools
└── StructuresParent (empty)      ← parent des obstacles spawnés
```

**Terrains et Envs ne sont PAS dans la scène.** Ils sont instanciés au runtime par EnvSpawner.

### GameManager — champs à remplir

| Champ | Type | Description |
|---|---|---|
| `sceneDuration` | float | 60 (surchargé par PlayerPrefs) |
| `useGlobalTimer` | bool | true |
| `scoreText` | TMP | ScoreText du Canvas |
| `timerText` | TMP | TimerText du Canvas |
| `sceneText` | TMP | SceneNumberText du Canvas |
| `globalTimerText` | TMP | GlobalTimerText du Canvas |
| `fadeCanvasGroup` | CanvasGroup | FadeCanvas |
| `sceneClearSound` | AudioClip | Sons/scene_clear.wav |
| `gameOverSound` | AudioClip | Sons/gameover.wav |
| `currentScrollSpeed` | float | 2 (non utilisé en 3D) |
| `structuresParent` | Transform | Empty parent pour les structures |
| `sceneNames[]` | string[] | Liste de **toutes** les GameScene jour+nuit (ex: GameScene_Demolition_1, GameScene_Demolition_Night_1...) |

L'EnvSpawner détecte automatiquement si la scène courante est jour ou nuit via le nom, et pioche dans les bons prefabs.

### ObstacleAnchor — configuration

- **Anchor Fantôme** : `isFantomeAnchor=true`, assigner `fantomePrefab` (le prefab 3D du fantôme avec Demolition_Fantome.cs)
- **Anchor Obstacles** : `obstaclePrefabs[]` (caisses, barils, etc.), `spawnRadius=2`, `minCount=1`, `maxCount=3`

---

## Fonctionnement

1. **OSC** : Universal_Button détecte les touches → appel via `button.Event.AddListener(pushable.OnPushed)`
2. **Environnement** : `Demolition_EnvironmentSpawner` instancie Terrain + Env aléatoires selon jour/nuit au `Start()`
3. **Fantôme** : `Demolition_Fantome.OnCollisionEnter` → dégâts par force de choc → mort → `GameManager.OnFantomeKilled()`
4. **Obstacles** : `Demolition_ObstacleSpawner` spawn obstacles/fantôme à chaque début de scène
5. **Fin de scène** : timer écoulé ou fantôme vaincu → fondu noire → scène aléatoire suivante (jour ou nuit, au hasard)
6. **Fin de partie** : timer global écoulé → transition Score

---

## Canvas — règle

Tous les TextMeshPro sont trouvés automatiquemnt par `GameObject.Find()`. Pas besoin de les assignr si le nom est correct. Mettre `raycastTarget=false` sur l'image FadeCanvas our ne pas blocer les iMPacts.