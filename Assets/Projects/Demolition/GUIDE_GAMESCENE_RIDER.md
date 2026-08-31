# Guide GameScene — Framework JVS (pour Rider / Unity)

## 🎯 Principes généraux

### Canvas obligatoire
Toute GameScene a un **Canvas** avec au minimum :
- **ScoreText** (TMP) — affiche le score en temps réel
- **TimerText** (TMP) — affiche le temps restant

Ces TextMeshPro sont trouvés **automatiquement** par le GameManager via `GameObject.Find("TimerText")` — pas besoin de les assigner si le nom est correct.

### GeneralVariable — toujours présent
Chaque scène a un **GeneralVariable.prefab** instancié. Il contient :
- Le script `MonJeu_GeneralVariables` (GameName, PlayerPrefs, OSC)
- Un enfant `OSC_Manager` (gère la communication OSC)
- **gameName** défini dans le prefab lui-même (pas dans l'override de scène)

### OSC (ReceiveParent)
Le GameManager hérite de **ReceiveParent**. Il reçoit les touches du mur interactif via `ReceivePoint(x, y)`.

---

## 🧩 Architecture type d'une GameScene

```
GameScene_MonJeu.unity
├── Main Camera
├── Directional Light
├── EventSystem               ← toujurs présent (UI)
├── Canvas                    ← Score + Timer
│   ├── ScoreText (TMP)
│   └── TimerText (TMP)
├── Background                ← image de fond (SwapImageBehaviour si thèmes)
├── MonJeu_GameManager        ← singleton ReceiveParent, chef d'orchestre
│   └── AudioSource           ← pour les sons
├── GeneralVariable           ← prefab (Dame_GeneralVariables, OSC)
└── ... (éléments spécifiques au jeu)
```

---

## 🔄 Comment les éléments marchent

### 1. GameManager — singleton + ReceiveParent

```csharp
public class MonJeu_GameManager : ReceiveParent
{
    public static MonJeu_GameManager Instance { get; private set; }

    void Awake() {
        if (Instance == null) Instance = this;
    }

    void Start() {
        // Trouve automatiquement les UI par nom
        timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

        // S'enregistre pour recevoir les touches OSC
        OSC_Manager.Instance.receiveP = this;
    }

    // Recoit les touches du mur
    public override void ReceivePoint(float xPoint, float yPoint) { ... }
}
```

### 2. Boutons (Universal_Button)

Tous les boutons sont des **composants**, pas des GameObjects séparés :
- `Universal_Button` sur un GameObject → détecte les touches via `ToolBox.CheckPos`
- `Universal_Collider2DButton` — pour les éléments 2D (cases, cellules)
- `Universal_PlayButton` — pour le bouton Jouer (Accueil)

### 3. Cellules du plateau

Héritent de `Universal_Collider2DButton`, pas de MonoBehaviour :
```csharp
public class MaCell : Universal_Collider2DButton
{
    public override void ReceivePoint(float xPoint, float yPoint) {
        // Convertit en position monde
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(xPoint, yPoint, ...));
        // Vérifie le Collider2D
        if (ToolBox.CheckPos(pos, GetComponent<Collider2D>()) && IsActive)
            GameManager.Instance.OnCellTouched(this);
    }
}
```

### 4. Sons

Chargés via `Resources.Load<AudioClip>("Sounds/nom")` dans le GameManager (fallback si non assignés dans l'inspecteur).

### 5. Sprites

- **Pré-assignés** par l'Editor script (`Tools > MonJeu - Tout configurer`)
- **Fallback** via `Resources.Load<Texture2D>("Textures/nom")` + `Sprite.Create()`

---

## ⚠️ Erreurs fréquentes (à savoir)

### "Broken PPtr"
Quand un PrefabInstance a `guid: 00000000000000000000000000000000`. Solution : supprimer le bloc PrefabInstance entier (du `--- !u!1001` au prochain `--- !u!`).

### "Could not extract GUID"
Quand un GUID n'est pas valide hex (ex: `s6s8f01aauu5nq10...`). Solution : générer un GUID hex via `uuid.uuid4().hex` et remplacer dans le .meta.

### Pas de GeneralVariable dans une scène
Chaque scène DOIT avoir GeneralVariable.prefab instancié → vérifier avec `grep "GeneralVariable" Scenes/*.unity`.

### UI elements créés en code (interdit)
**NE JAMAIS** faire `new GameObject()` + `AddComponent<Toggle>()` ou `AddComponent<Slider>()`. Les UI (Toggle, Slider, Dropdown, InputField) sont **copiés en YAML** depuis SpotTheDif.

---

## 📋 Checklist GameScene

- [ ] Canvas présent avec ScoreText + TimerText (TMP)
- [ ] GeneralVariable.prefab instancié
- [ ] GameManager : ReceiveParent, singleton, OSC_Manager.Instance.receiveP = this
- [ ] AudioSource sur le GameManager
- [ ] Background avec image assignée
- [ ] EventSystem présent
- [ ] Les cellules/boutons héritent de Universal_Button / Universal_Collider2DButton
- [ ] Les sons chargés par Resources.Load ou Editor script
- [ ] Les sprites assignés par l'Editor script ou fallback Resources.Load + Sprite.Create

---

## 🔧 Pour Dame spécifiquement

### GameManager attend
| Champ | Type | Source |
|---|---|---|
| `caseFoncee` / `caseClaire` | Sprite | Sprites/ |
| `pionBlanc` / `pionNoir` | Sprite | Sprites/ |
| `dameBlanche` / `dameNoire` | Sprite | Sprites/ |
| `boardSize` | int | 10 par défaut |
| `timePerMove` | float | Depuis PlayerPrefs (Dame_GameTime) |
| `timerText` / `scoreText` / `currentPlayerText` | TMP | GameObject.Find dans Start() |
| `moveSound` / `captureSound` / `crownSound` / `winSound` | AudioClip | Sons/ |

### Plateau
- Généré dynamiquement par `Dame_Board.InitializeBoard()` dans Start()
- Chaque cellule : `Universal_Collider2DButton` + `BoxCollider2D` + `SpriteRenderer`
- Pions : GameObjects avec `SpriteRenderer` + `Dame_Piece`

### CurrentPlayerText
Si présent dans le Canvas, affiche "Tour des Blancs" / "Tour des Noirs". Sinon ignore.

---

*Document généré pour Rider — Framework JVS / Projet Dame*