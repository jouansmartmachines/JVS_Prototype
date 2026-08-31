# Guide GameScene — Jeu de Dames (pour Rider / Unity)

## 🎯 Règles du jeu

Le jeu de Dames se joue sur un plateau **10×10** (par défaut). Deux joueurs s'affrontent :
- **Joueur 1 (Blancs)** — pions blancs, en bas du plateau
- **Joueur 2 (Noirs)** — pions noirs, en haut du plateau

### Déplacement
- Les pions se déplacent **en diagonale** (avant uniquement)
- Les pions avancent d'**1 case** par coup (direction diagonale avant)
- Les pions ne peuvent PAS reculer

### Prise (Capture)
- Si un pion adverse est adjacent en diagonale **et** que la case derrière est libre → **prise obligatoire**
- Le pion saute par-dessus l'adversaire et arrive sur la case libre
- Le pion adverse est retiré du plateau
- **Après une prise**, le pion peut enchaîner (multi-prise) si une autre prise est possible
- C'est le même joueur qui rejoue tant qu'il peut capturer

### Dame
- Quand un pion atteint la **dernière rangée** adverse (row 0 pour les Blancs, row 9 pour les Noirs) → il est **couronné** (devient une Dame)
- La **Dame** peut se déplacer **en avant ET en arrière** (diagonale)
- Les captures en arrière sont permises

### Fin de partie
- Un joueur gagne quand **l'adversaire n'a plus de pions**
- Tour par tour : si un joueur ne peut pas jouer, il perd
- Timer : **15 secondes par coup** (configurable via Dame_GameTime)

---

## 🧩 Architecture du plateau (Dame_Board.cs)

### Création du plateau
```
Dame_GameManager.Start()
  → crée GameObject "Board" avec Dame_Board
  → board.InitializeBoard(10, caseFoncee, caseClaire, pionBlanc, pionNoir, dameBlanche, dameNoire)
```

### Cellules (Dame_Cell)
Chaque cellule est créée dynamiquement :
```
GameObject($"Cell_{r}_{c}")
├── SpriteRenderer (case foncée ou claire, sortingOrder 0)
├── BoxCollider2D (taille 1)
└── Dame_Cell : Universal_Collider2DButton
```

La cellule hérite de `Universal_Collider2DButton` → détecte les touches du mur.

### Pions (Dame_Piece)
Chaque pion est créé dans `PlaceInitialPieces()` :
```
GameObject("PionBlanc" / "PionNoir")
├── SpriteRenderer (pion, sortingOrder 1)
└── Dame_Piece
```

Quand un pion devient Dame → `Crown()` change son sprite pour `dameBlancheSprite` / `dameNoireSprite`.

### Flow d'un coup
```
1. Joueur touche une pièce → OnCellTouched(cell)
2. Si Idle → SelectPiece(cell)
   - Vérifie que la pièce appartient au joueur actuel
   - Calcule les coups valides (déplacement + captures)
   - Les captures sont prioritaires
   - Affiche les surbrillances (highlight)
3. Si PieceSelected → TryMove(cell)
   - Si la cellule est dans validMoves → exécute le déplacement ou la capture
   - Si capture → vérifie multi-prise possible
   - Sinon → EndTurn() (change de joueur)
4. Si GameOver → EndGame(winner)
```

---

## 📋 Ce que Dame_GameManager attend

Le GameManager trouve automatiquement les UI par `GameObject.Find()`. 

| Champ | Type | Trouvé par |
|---|---|---|
| `timerText` | TMP | `GameObject.Find("TimerText")` |
| `scoreText` | TMP | `GameObject.Find("ScoreText")` |
| `currentPlayerText` | TMP | `GameObject.Find("CurrentPlayerText")` (optionnel) |
| `boardSize` | int | Défaut 10 |
| `caseFoncee` / `caseClaire` | Sprite | Assigne dans l'inspecteur ou Editor script |
| `pionBlanc` / `pionNoir` | Sprite | Assigne dans l'inspecteur ou Editor script |
| `dameBlanche` / `dameNoire` | Sprite | Assigne dans l'inspecteur ou Editor script |
| `moveSound` / `captureSound` / `crownSound` / `winSound` | AudioClip | Assigne dans l'inspecteur ou Editor script |

### AudioSource
Le GameManager appelle `GetComponent<AudioSource>()` dans Start() → il faut un **AudioSource** component sur le **même GameObject**.

### StructuresParent
Le plateau est créé sous `GameObject.Find("StructuresParent")` → si ce GameObject n'existe pas, le plateau devient enfant du GameManager.

---

## 🔄 Timer (time per move)

- Temps par coup : **15 secondes** (clé `Dame_GameTime` dans PlayerPrefs)
- Modifiable via le dropdown Difficulty (Menu)
- Si le temps expire → `EndTurn()` → changement de joueur

---

## 👥 2 joueurs

- **currentPlayer** alterne entre 1 (Blancs) et 2 (Noirs)
- `currentPlayerText` affiche "Tour des Blancs" / "Tour des Noirs"
- Les noms des joueurs sont chargés depuis PlayerPrefs (`Dame_Player1`, `Dame_Player2`)
- Si la scene Score a un ScoreBoardManager, il reçoit les noms via `ReceiveName()`

---

## 🏆 Score

- `scorePlayer1` = nombre de pions capturés par le joueur 1
- `scorePlayer2` = nombre de pions capturés par le joueur 2  
- Affichage : `"0 - 0"` dans ScoreText
- En fin de partie : `PlayerPrefs.SetInt("Dame_FinalScore", score)`

---

## ⚠️ Points sensibles

### Les cellules détectent le toucher directement
Chaque `Dame_Cell` a son propre `ReceivePoint()` (hérité de `Universal_Collider2DButton`) → le GameManager **ne doit pas** override `ReceivePoint()` car les cellules gèrent déjà la détection.

### Multi-capture (enchaînement)
Après une capture, le joueur peut rejouer si une autre capture est disponible depuis la nouvelle position. Le `state` reste `PieceSelected`.

### Collider2D
Chaque cellule a un **BoxCollider2D** créé dans `InitializeBoard()`. Le `ToolBox.CheckPos()` du `Universal_Collider2DButton` l'utilise pour détecter les touches.

---

*Document généré pour Rider — Projet Dame / GameScene*