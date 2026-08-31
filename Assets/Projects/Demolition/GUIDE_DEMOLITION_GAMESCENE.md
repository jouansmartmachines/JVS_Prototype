# Guide GameScene — Démolition (Angry Birds sur mur interactif)

> [!IMPORTANT]
> **Règle absolue du projet :**
> Interdiction de modifier les autres jeux ou le reste du projet. Toute modification doit s'effectuer **exclusivement dans le dossier `Assets/Projects/Demolition/`** et ses scènes associées.

---

## 🎯 Règles du jeu & Gameplay Feel ("Juice")

Le joueur touche le mur interactif ou clique à la souris $\rightarrow$ un projectile (**Oiseau** ou onde d'impact) est propulsé à haute vitesse vers le point d'impact $\rightarrow$ explosion avec onde de choc 2D, débris physiques, tremblement d'écran (*Screen Shake*), micro-gel temporel (*Hitstop*) et destruction en chaîne des structures.

### Objectif
**Détruire un maximum de structures et de cochons cibles** dans le temps imparti (selon la difficulté configurée).

### Matériaux des blocs
Chaque partie génère des structures composées de :
- **Bois** (`debris_bois`, 2 PV, +50 pts) : Résistance standard, casse avec fissures.
- **Verre** (`debris_verre`, 1 PV, +100 pts) : Fragile et cristallin, éclate en morceaux brillants.
- **Pierre** (`debris_pierre`, 4 PV, +30 pts) : Lourd et résistant, absorbe les chocs.
- **Cochons / Boss Cibles** (`cochon`, `cochon_vert`, `cochon_bleu`, 2 à 6 PV, +500 à +2000 pts) : Déclenchent un *Squash & Stretch*, un cri, un gros *Screen Shake*, un ralenti (*Slow-Mo*) et l'attribution des étoiles de fin de partie.

### Effets "Juice" intégrés
- **Hitstop (Micro-freeze) :** Gel de ~35ms à chaque explosion d'impact pour une sensation d'impact percutant.
- **Screen Shake :** Tremblement d'écran amorti proportionnel à la violence du tir et à l'écroulement.
- **Slow-Motion :** Ralenti spectaculaire lors des effondrements massifs ou de l'élimination des boss cochons.
- **Squash & Stretch & Hit-Flash :** Déformation élastique et flash lumineux blanc à chaque coup porté.
- **Popups de score rebondissants (Punch Scale) :** Affichage dynamique du score avec code couleur selon le matériau et multiplicateurs de combos.
- **Variations Audio :** Pitch aléatoire sur les sons d'impact et de destruction pour un univers sonore vivant.

---

## 🧩 Architecture de la GameScene

```
GameScene_Demolition.unity
├── Main Camera
├── Directional Light
├── EventSystem
├── Canvas
│   ├── TimerText (TMP)
│   ├── ScoreText (TMP)
│   └── StarText (TMP)
├── Background (SpriteRenderer)
├── Ground (BoxCollider2D + Demolition_GroundScroll)
├── GeneralVariable (Prefab Demolition_GeneralVariables + OSC)
├── Demolition_GameManager (Singleton, ReceiveParent, cycle de jeu, audio, juice)
└── StructuresParent (Conteneur des structures procédurales)
```

---

## 🔄 Contrôles & Entrées
1. **Mur interactif (OSC) :** Réception des coordonnées via `Demolition_GameManager.ReceivePoint(x, y)`.
2. **Souris & Tactile (Local / Éditeur) :** Détection automatique du clic gauche dans `Update()` (`Input.GetMouseButtonDown(0)`).
