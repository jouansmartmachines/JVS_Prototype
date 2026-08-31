# Guide GameScene — Démolition (Angry Birds sur mur interactif)

## 🎯 Règles du jeu

Le joueur touche le mur → un **oiseau** apparaît au point touché → il vole droit devant lui → il explose contre les structures → les blocs se brisent.

### Objectif
**Détruire un maximum de blocs** (bois, verre, pierre) dans le temps imparti (30/60/90 secondes).

### Structure des blocs
Chaque partie génère des **structures aléatoires** composées de :
- **Bois** (orange) — facile à casser
- **Verre** (bleu) — casse facilement en éclats  
- **Pierre** (gris) — résistant, nécessite plusieurs impacts
- **Cochons** (optionnel) — bonus de score

### Impact
- L'oiseau explose **au premier bloc touché** → destruction en rayon autour du point d'impact
- Plusieurs blocs proches peuvent être détruits par la même explosion
- Les débris volent dans toutes les directions (particules)

### Score
- Bloc bois détruit → +10 points
- Bloc verre détruit → +20 points
- Bloc pierre détruit → +30 points
- Cochon détruit → +50 points

### Timer et fin
- Temps configurable (30/60/90s) via dropdown Difficulty dans le Menu
- Fin de partie → ScoreBoard avec le score final

---

## 🧩 Architecture de la GameScene

```
GameScene_Demolition.unity
├── Main Camera
├── Directional Light
├── EventSystem
├── Canvas
│   ├── TimerText (TMP)
│   └── ScoreText (TMP)
├── Background                ← fond de jeu (sol + ciel)
│   └── ScrollingBackground   ← script qui fait défiler
├── GroundScroll              ← sol qui défile
├── GeneralVariable           ← prefab (Demolition_GeneralVariables + OSC)
├── Demolition_GameManager    ← ReceiveParent, chef d'orchestre
├── StructuresParent          ← parent vide où les structures spawnent
```

---

## 🔄 Comment les éléments marchent

### 1. Touch → Oiseau (`ReceivePoint`)
Quand le joueur touche le mur, `Demolition_GameManager.ReceivePoint()` est appelé → il crée un **Oiseau (Projectile)** au point touché.

### 2. Oiseau (`Demolition_Projectile`)
- Apparaît à la position du toucher (X, Y)
- Vole en ligne droite vers Z=0 (le plan du jeu)
- Si il touche un bloc → `OnCollisionEnter2D` → explose
- Si il ne touche rien → `Destroy()` après timeout

### 3. Bloc (`Demolition_Block`)
- Chaque bloc a un `BoxCollider2D` + `SpriteRenderer`
- Quand touché par l'oiseau → `OnCollisionEnter2D()` → prend des dégâts
- 3 états : intact → fissuré → détruit
- Les blocs voisins subissent des dégâts de l'explosion (rayon)
- Destruction → spawn de débris (particules)

### 4. Structures générées aléatoirement
`Demolition_StructureBuilder` génère des formes aléatoires (tours, murs, pyramides) avec les 3 matériaux.

### 5. Sol qui défile (`ScrollingBackground`)
- Le sol défile pour donner l'impression de mouvement
- Vitesse configurable via le slider ScrollSpeed dans le Menu

### 6. Mode Oiseau (Toggle dans le Menu)
- **Activé** : le projectile est un oiseau (sprite oiseau)
- **Désactivé** : le projectile est une simple impact (plus petit)

---

## ⚙️ Ce que Demolition_GameManager attend

| Champ | Trouvé par |
|---|---|
| `oiseauPrefab` | `Resources.Load<GameObject>("Prefabs/Oiseau")` |
| `impactEffectPrefab` | `Resources.Load<GameObject>("Prefabs/ImpactExplosion")` |
| `impactSound` | `Resources.Load<AudioClip>("Sounds/impact")` |
| `destructionSound` | `Resources.Load<AudioClip>("Sounds/destruction")` |
| `gameOverSound` | `Resources.Load<AudioClip>("Sounds/gameover")` |
| `popupTextPrefab` | `Resources.Load<GameObject>("Prefabs/PopupText")` |
| `timerText` | `GameObject.Find("TimerText")` |
| `scoreText` | `GameObject.Find("ScoreText")` |

Tout est chargé via **Resources.Load** → pas besoin d'assigner dans l'inspecteur.

---

## ⚠️ Points sensibles

### Le projectile va en Z (profondeur)
L'oiseau ne va pas latéralement (X) — il apparaît au point touché (X,Y) et vole vers Z=0. L'explosion utilise `Physics2D.OverlapCircleAll()` pour toucher tous les blocs dans un rayon.

### Collision : OnCollisionEnter2D
Les blocs utilisent **OnCollisionEnter2D** (pas OnTrigger). L'oiseau a un Rigidbody2D avec une vélocité.

### Pas de Rigidbody sur le GameManager
Le GameManager n'a pas besoin de Rigidbody — il crée les projectiles.

---

*Document généré pour Rider — Projet Démolition / GameScene*