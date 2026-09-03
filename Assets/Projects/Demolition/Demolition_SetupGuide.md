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

### Scripts obsolètes (2D — à ne pas utiliser)

`Demolition_Block.cs`, `Demolition_Structure.cs`, `Demolition_StructureBuilder.cs`, `Demolition_ScrollingBackground.cs`, `Demolition_GroundScroll.cs`, `Demolition_Projectile.cs`, `Demolition_PigBehavior.cs`, `Demolition_DataLoader.cs`

---

## Préfabs à créer (dans Prefabs/)

Ces préfabs sont instanciés **par script** au runtime, pas placés dans la scène.

### Terrains (sol + ground)

| Prefab | Contenu |
|---|---|
| `Terrain.prefab` | Sol, BoxCollider, mesh décor (herbe, terre...) |
| `TerrainNight.prefab` | Variante nuit du terrain |

### Environnements (décor background)

| Prefab | Contenu |
|---|---|
| `Env1Sun.prefab` | Décor jour — version 1 |
| `Env1Night.prefab` | Décor nuit — version 1 |
| `Env2Sun.prefab` | Décor jour — version 2 |
| `Env2Night.prefab` | Décor nuit — version 2 |
| `Env3Sun.prefab` | Décor jour — version 3 |
| `Env3Night.prefab` | Décor nuit — version 3 |

Un **script d'environnement** (`Demolition_EnvironmentSpawner.cs` ou directement dans le GameManager) pioche aléatoirement un Terrain + un Env et les instancie au `Start()`.

## Ce que doit contenir la GameScene (setup minimal)

```
GameScene_Demolition.unity
├── Main Camera (perspective, 3D)
├── Directional Light
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
├── EnvSpawner (empty)            ← instancie Terrain + Env aléatoires au Start()
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
| `sceneNames[]` | string[] | Liste de toutes les GameScene (ex: GameScene_Demolition, GameScene_Demolition_2...) |

### ObstacleAnchor — configuration

- **Anchor Fantôme** : `isFantomeAnchor=true`, assigner `fantomePrefab` (le prefab 3D du fantôme avec Demolition_Fantome.cs)
- **Anchor Obstacles** : `obstaclePrefabs[]` (caisses, barils, etc.), `spawnRadius=2`, `minCount=1`, `maxCount=3`

---

## Fonctionnement

1. **OSC** : Universal_Button détecte les touches → appel via `button.Event.AddListener(pushable.OnPushed)`
2. **Fantôme** : `Demolition_Fantome.OnCollisionEnter` → dégâts par force de choc → mort → `GameManager.OnFantomeKilled()`
3. **Obstacles** : `Demolition_ObstacleSpawner` spawn à chaque début de scène
4. **Fin de scène** : timer écoulé ou fantôme vaincu → fondu noire → scène aléatoire suivante
5. **Fin de partie** : timer global écoulé → transition Score

---

## Canvas — règle

Tous les TextMeshPro sont trouvés automatiquemnt par `GameObject.Find()`. Pas besoin de les assignr si le nom est correct. Mettre `raycastTarget=false` sur l'image FadeCanvas our ne pas blocer les iMPacts.