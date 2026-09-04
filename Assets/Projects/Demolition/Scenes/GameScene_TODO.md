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

### Tu ajoutes TOUS les composants manuellement sur les prefabs

Pas d'auto-setup — tu mets toi-même chaque composant dans chaque prefab obstacle.

**Pour chaque obstacle (caisse, baril) :**

Sur `Caisse.prefab` et `Baril.prefab`, ajoute ces 4 composants :

| Composant | Type | Rôle |
|---|---|---|
| `BoxCollider` | Collider 3D | Détection physique |
| `Rigidbody` | Rigidbody 3D | Gravité + collisions physiques |
| `Universal_Button` | **Universal_Button** (pas Collider2DButton, pas Mask) | Détection toucher OSC |
| `Demolition_Pushable` | Script C# | Force d'impulsion au toucher |

**Pour le fantôme (`Fantome.prefab`) :**

| Composant | Type | Rôle |
|---|---|---|
| `BoxCollider` | Collider 3D | Collision physique (pas Universal_Button) |
| `Rigidbody` | Rigidbody 3D | Physique (chocs) |
| `Demolition_Fantome` | Script C# | HP, dégâts par force de choc, mort |

→ **Pas de Universal_Button** sur le fantôme — il reçoit les dégâts via `OnCollisionEnter` (force de choc), pas par impact direct.

**Pour les obstacles décoratifs / non interactifs :**

Juste Mesh + éventuellement Collider si besoin de collision physique.

### Quel type de Universal_Button selon le contexte

| Type de rendu | Composant à ajouter |
|---|---|
| **3D** (MeshRenderer, Collider 3D) → caisses, barils | `Universal_Button` (détection Physics.Raycast) |
| **2D** (SpriteRenderer, Collider2D) | `Universal_Collider2DButton` |
| **Canvas UI** (RectTransform) | `Universal_Button` (détection Rect) |
| **Bouton Play Accueil** | `Universal_PlayButton` |

→ **Pour Demolition 3D, c'est `Universal_Button` standard** (pas Collider2DButton, pas Mask).

### Binding du Event

Ensuite, dans le script `Demolition_ObstacleSpawner`, le `SetupInteractable()` ne fait **plus que le binding** (puisque les composants sont déjà là) :

```csharp
private void SetupInteractable(GameObject obj)
{
    var btn = obj.GetComponent<Universal_Button>();
    if (btn == null) return;

    var pushable = obj.GetComponent<Demolition_Pushable>();
    if (pushable == null) return;

    btn.Event.AddListener(pushable.OnPushed);
}
```

**Donc :** tu ajoutes les 4 composants à la main → l'ObstacleSpawner bind juste l'Event. OK ?

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