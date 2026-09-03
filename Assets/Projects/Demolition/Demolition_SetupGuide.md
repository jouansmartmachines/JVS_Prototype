# Demolition 3D — Setup Guide

## Scripts modifiés / ajoutés

### Nouveaux scripts (dans `Scripts/`)

| Script | Description |
|---|---|
| `Demolition_Pushable.cs` | Force d'impulsion 3D sur l'objet touché. Binding auto via Event. |
| `Demolition_ObstacleAnchor.cs` | Zone de spawn. À mettre sur des empty dans chaque scène. |
| `Demolition_ObstacleSpawner.cs` | Spawn les obstacles au début. Auto-ajoute Universal_Button + Pushable. |
| `Demolition_Fantome.cs` | HP par force de choc, mort → notifie GameManager. |
| `Demolition_Destructible.cs` | Caisses/barils destructibles. HP, flash, destruction. |

### Modifiés

| Script | Changement |
|---|---|
| `Demolition_GameManager.cs` | Supprimé ReceiveParent, projectile, scrolling. Timer scène+global, score, scènes aléatoires, fondu noir. |
| `Demolition_GeneralVariables.cs` | Ajouté `SceneTimeKey` et `GlobalTimeKey`. |

---

## Ce que tu dois faire dans Unity

### 1. Dans chaque scène achetée

- Place un empty `ObstacleSpawner` → ajoute `Demolition_ObstacleSpawner`
- Place un empty `ObstacleAnchors` → ajoute des enfants avec `Demolition_ObstacleAnchor` :
  - Règle `obstaclePrefabs[]` (tes prefabs : barils, caisses...)
  - Règle `spawnRadius`, `minCount`, `maxCount`
  - Un anchor doit avoir `isFantomeAnchor = true` + `fantomePrefab`
- Place un `GeneralVariable.prefab` (existant)
- Place un `GameManager` avec `Demolition_GameManager` :
  - Remplir `sceneNames[]` (noms de toutes les GameScene)

### 2. Canvas GameScene

Crée un Canvas avec :
- `ScoreText` (TMP) — "Score: 0"
- `TimerText` (TMP) — "60"
- `GlobalTimerText` (TMP) — "5:00"
- `FadeCanvas` — Image noire + CanvasGroup (alpha 0)

### 3. Menu

Ajouter un **Dropdown** temps par scène (30/60/90 sec) → clé `Demolition_SceneTime`

### 4. Build Settings

Ajouter toutes les GameScene + Accueil/Menu/Score/Loading/SelectionMenu

---

## Binding (automatique)

`ObstacleSpawner` ajoute `Universal_Button` + `Pushable` sur chaque obstacle spawné et bind `button.Event.AddListener(pushable.OnPushed)`. Rien à la main.