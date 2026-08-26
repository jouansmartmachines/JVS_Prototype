# Guide de création d'un jeu JVS

> **Projet** : JVS Prototype → Package → JVS Jeux (27 jeux)  
> **Auteur** : Smart Machines — Patrick Suchet  
> **Version** : 2.0 — Document de reprise

---

## 1. Les 3 types de jeux

| Type | Thèmes | Presets | Usage |
|---|---|---|---|
| **Base** | Non | Non | Jeu simple |
| **Thèmes** | ThemeSelector (menu) | Oui | Choix du thème dans le menu |
| **Thèmes Dynamiques** | ThemeAppSelector (app) | Non | Thème envoyé par l'app OSC |

Un jeu est soit **simple**, soit **thématisé** — et s'il est thématisé, le thème est choisi soit dans le menu, soit depuis l'app.

---

## 2. Nomenclature

| Élément | Règle | Exemple |
|---|---|---|
| Scripts | `NomDuJeu_` | `Monstres_GameManager` |
| Namespace | `NomDuJeu` | `namespace Monstres` |
| PlayerPrefs | `NomDuJeu_` | `Monstres_HighScore` |
| Scènes | `Accueil/Menu/Intro/GameScene/Score` + `_NomDuJeu` | `GameScene_Monstres` |
| ThemeManager | `NomDuJeu_ThemeManager` | `Monstres_ThemeManager` |
| GameTheme | `NomDuJeu_Theme_NomDuTheme` | `Monstres_Theme_Foret` |
| Thèmes app | `NomDuJeu_Lettre` | `Challenge_F` |
| Logo | `01_NomDuJeu.png` | `01_Monstres.png` |

### Auto-génération des scènes

Dans `Universal_GeneralVariables`, le champ `gameName` génère automatiquement :

```
gameName = "MonJeu"
→ menuScene = "Menu_MonJeu"
→ accueilScene = "Accueil_MonJeu"
→ introScene = "Intro_MonJeu"
→ gameScene = "GameScene_MonJeu"
→ scoreScene = "Score_MonJeu"
```

### GameScoreBoard enum

Dans `ScoreBoardManager.cs` : ajouter le jeu à l'enum.

```csharp
public enum GameScoreBoard {
    ..., Monstres, Nettoyage, MonJeu
}
```

---

## 3. Structure du dossier

```
Assets/Projects/NomDuJeu/
├── NomDuJeu_Scenes/
│   ├── Accueil_NomDuJeu.unity
│   ├── Menu_NomDuJeu.unity
│   ├── Intro_NomDuJeu.unity
│   ├── GameScene_NomDuJeu.unity
│   └── Score_NomDuJeu.unity
├── NomDuJeu_Scripts/
│   ├── GeneralVariables/
│   │   └── NomDuJeu_GeneralVariables.cs
│   ├── Menu/
│   │   ├── NomDuJeu_MenuManager.cs
│   │   └── NomDuJeu_PlayButton.cs
│   ├── Gameplay/
│   │   └── NomDuJeu_GameManager.cs
│   └── LB/
│       ├── NomDuJeu_LB_Highscores.cs
│       └── NomDuJeu_LB_Scores.cs
└── Resources/
```

---

## 4. GeneralVariable — Le cœur du jeu

### Script

```csharp
using UnityEngine;
using TMPro;

namespace NomDuJeu
{
    public class NomDuJeu_GeneralVariables : Universal_GeneralVariables
    {
        public static NomDuJeu_GeneralVariables Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [SerializeField] ScoreBoardDisplayer _scoreBoardDisplayer;
        [SerializeField] TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;
        [SerializeField] Color _winnerColor;

        public const string HighScoreKey = "NomDuJeu_HighScore";

        public override void ReceiveName(string name)
        {
            float score = PlayerPrefs.GetFloat(HighScoreKey);
            PlayerData data = new PlayerData() { Name = name, Score = score };
            PlayerData defaultPlayer = new PlayerData() { Name = Localizer.Get("Unknown"), Score = 0 };
            _scoreBoardDisplayer.InitScoreBoard(
                ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.NomDuJeu),
                Font, _winnerColor, defaultPlayer);
        }
    }
}
```

### Préfab

- Copier un GeneralVariable d'un jeu existant → `Prefab/`
- Y attacher le script + **OSC Manager**
- Remplir : `GameName`, `ScoreBoardDisplayer`, `Font`, `WinnerColor`
- **Placer dans TOUTES les scènes du jeu**

### Constantes PlayerPrefs

| Type | Constantes |
|---|---|
| Base | `HighScoreKey` |
| Thèmes | `HighScoreKey`, `UseDefaultPictureKEY`, `PicturePath`, `Difficulty` |
| Thèmes Dynamiques | `HighScoreKey`, `ScreenRatio`, `GameTime`, `EphemereTime` |

### SavedData (valeurs par défaut)

Le tableau `dataSaved` dans le prefab permet de définir des valeurs PlayerPrefs par défaut :

```csharp
[Serializable]
public class SavedData {
    public string saveDataName;
    public float fSaveDataBaseValue;
    public string sSaveDataBaseValue;
    public enum DataType { Float, Int, String }
    public DataType dataSavedType;
}
```

---

## 5. Les 5 scènes

| Scène | Nom | Rôle |
|---|---|---|
| **Accueil** | `Accueil_NomDuJeu` | Point d'entrée, InstructionsDisplay, bouton Play |
| **Menu** | `Menu_NomDuJeu` | Paramètres (difficulté, presets, thèmes, stickers) |
| **Intro** | `Intro_NomDuJeu` | Animation intro |
| **Jeu** | `GameScene_NomDuJeu` | La partie |
| **Score** | `Score_NomDuJeu` | Scoreboard final |

Ajouter toutes les scènes aux **Build Settings**.

### Scène Score

1. Copier un prefab `ScoreBoard_NomDuJeu` → Canvas
2. Remplir : `_unit`, `_scoreBoardObject`, `_scoreDisplay`, `_collum`, `_onDisplay`
3. Copier un `Universal_Button` → renseigner le GeneralVariable
4. Script `OSCScore` à placer dans la scène

### Scène Menu

**Type Base** : sliders, toggles simples. Pas de MenuPreset. Pas de ThemeSelector.

**Type Thèmes** :
- Ajouter le prefab `MenuPreset` (NE PAS créer)
- Chaque `Button` → assigner le `Preset` du jeu
- Toggles → PlayerPrefs `NomDuJeu_Sticker`, `NomDuJeu_Instructions`
- Sliders/Dropdowns → copier, assigner `TypePlayerPrefs`
- **ThemeSelector** → ajouter le prefab → assigner le `ThemeManager`
- Stickers → prefab `Sticker (Only Images)` ou `withCanvas`

**Type Thèmes Dynamiques** :
- Pas de ThemeSelector
- Pas de MenuPreset
- Sliders pour les paramètres

UI Prefabs disponibles : `SliderSavePlayersPref`, `SliderValueText`, `ToggleSavePlayerPref`, `DropDownPlayersPref`, `InputFieldPlayersPref`, `TMPPlayersPref`, `EventPlayerPref`.

---

## 6. Les Presets (Type Thèmes uniquement)

`Create > Game > Menu > ...`

| Preset | Type | Usage |
|---|---|---|
| Preset-Instructions | ScriptablePreset | Instructions |
| Preset-Stickers | ScriptablePreset | Stickers |
| ValueBool | ValueBool | Toggles |
| ValueInt | ValueInt | Dropdowns |
| ValueFloat | ValueFloat | Sliders |
| ValueString | ValueString | InputFields |

- Chaque Preset → PlayerPrefs `NomDuJeu_<parametre>`
- **SuperPreset** `NomDuJeu_Preset` → lier tous les presets
- `InstructionsDisplay` dans l'Accueil → assigner le ScriptablePreset Instructions

Valeurs stockées dans `data/Settings/jeux-parameter.csv` (format `ID;Easy;Normal;Hard`).

---

## 7. Système de thèmes

### Architecture

```
ThemeManager (ScriptableObject)
├── GameTheme 1 (ScriptableObject)
│   ├── SwapEntity: SwapSprite, SwapPrefab
│   └── SwapEntity: SwapAudio, SwapAnimator, SwapFont, SwapMaterial...
├── GameTheme 2
│   └── ...
└── SwapObject (ScriptableObject) — dans la scène
    └── Swaps (auto-liés via OnValidate)
```

### Création

**1. ThemeManager** → `Create > Game > Theme > ThemeManager` → `NomDuJeu_ThemeManager`
- Ajouter GameThemes + SwapObjects dans les listes
- Définir le `DefaultGameTheme`

**2. GameTheme** → `Create > Game > Theme > GameTheme` → `NomDuJeu_Theme_MonTheme`
- Ajouter les SwapEntities
- Auto-link avec le ThemeManager si la nomenclature est respectée

**3. Swap Entities** → par élément modifiable :
- `SwapSprite` / `SwapPrefab` / `SwapMaterial` / `SwapAudio` / `SwapAnimator` / `SwapFont`
- Assigner le `GameTheme` et l'attribut

**4. Swap Object** → dans la scène, sur chaque GameObject modifiable :
- Ajouter le Behaviour adapté :

| Behaviour | Cible |
|---|---|
| `SwapImageBehaviour` | Image UI |
| `SwapSpriteRendererBehaviour` | SpriteRenderer |
| `SwapPrefabBehaviour` | GameObject |
| `SwapAnimatorBehaviour` | Animator |
| `SwapFontBehaviour` | TMP_Text |
| `SwapEnableBehaviour` | Enable/disable |

- Assigner le `SwapObject` et le component ciblé

### Sélection du thème — 2 façons

**A) ThemeSelector (Type Thèmes)**
- Prefab existant dans le Menu
- Dropdown avec les noms des thèmes
- Persisté en PlayerPrefs

**B) ThemeAppSelector (Type Thèmes Dynamiques)**
- Dans la scène, s'active automatiquement
- L'**app OSC** envoie une lettre
- Convention : `NomDuJeu_UneLettre` (ex: `Challenge_F`)
- Le script prend la première lettre → cherche le GameTheme correspondant
- Pas de dropdown, pas de persistance

---

## 8. GameManager

```csharp
namespace NomDuJeu
{
    public class NomDuJeu_GameManager : ReceiveParent
    {
        public static NomDuJeu_GameManager Instance { get; private set; }
        void Awake() { /* singleton */ }

        void Start() {
            OSC_Manager.Instance.receiveP = this;  // OBLIGATOIRE
        }

        public override void ReceivePoint(float xPoint, float yPoint) {
            // coordonnées normalisées (0-1)
        }

        void EndGame() {
            PlayerPrefs.SetFloat(NomDuJeu_GeneralVariables.HighScoreKey, score);
            TransitionToScore();
        }

        void TransitionToScore() {
            if (BuildState.CurrentState == BuildState.State.normal)
                SceneManager.LoadScene(NomDuJeu_GeneralVariables.Instance.scoreScene);
            else
                MenuSelectionButton.Instance.gameObject.SetActive(true);
        }
    }
}
```

---

## 9. Universal Buttons

**NE PAS** utiliser le Button Unity standard. Toujours `Universal_Button` ou ses dérivés.

| Type | Usage |
|---|---|
| `Universal_Button` | Bouton standard avec event |
| `Universal_Collider2DButton` | Activation par collider |
| `Universal_ColliderMask` | Cache les boutons en arrière |
| `Universal_PlayButton` | Bouton Play avec impact |
| `Universal_KeyboardShortcut` | Raccourcis clavier |
| `MenuSelectionButton` | Retour au menu de sélection |

---

## 10. Système de chargement

### LoadingManager

```csharp
LoadingManager.LoadScene("GameScene_MonJeu");
// ou avec notification app :
LoadingManager.LoadScene("GameScene_MonJeu", true);
```

Fonctionnement :
1. Détruit l'ancien GeneralVariable
2. Charge `LoadingScreen` en additif
3. Désactive raccourcis/OSC
4. Charge la scène cible avec barre de progression
5. Active la scène, réactive OSC
6. Décharge LoadingScreen

### Scripts à placer

- **`OSCGameScene`** → dans la GameScene → prévient l'app que le jeu commence
- **`OSCScore`** → dans la scène Score → idem

---

## 11. ScoreBoardManager

Stockage JSON : `Application.persistentDataPath + "/NomDuJeu.json"`

```csharp
[Serializable]
public class PlayerData {
    public string Name; public int Rank;
    public bool WinNow; public float Score; public string Value;
}
```

Méthodes :
- `GetScoreBoard(GameScoreBoard.MonJeu)`
- `UpdateScoreBoardDescendingOrder(data, GameScoreBoard.MonJeu)`
- `UpdateScoreBoardAscendingOrder(data, GameScoreBoard.MonJeu)`
- `CreateScoreBoard(GameScoreBoard.MonJeu)`
- `ResetAll()`

### ScoreBoardDisplayer

```csharp
_scoreBoardDisplayer.InitScoreBoard(
    datas, Font, winnerColor, defaultPlayer, displayValue
);
```

---

## 12. Localizer — Multi-langues

Fichier TSV : `Assets/Universal/Localisation/Langues_Jeux.tsv` (aussi dans Resources/)

```
Key | Clef | Français | Anglais | Espagnol | Catalan
```

```csharp
Localizer.currentLanguage = Language.Français;
string text = Localizer.Get("Score");  // "Score" ou "Points"...
```

Si clé manquante → retourne `"MISSING"`.

---

## 13. Raccourcis clavier (intégrés)

| Touche | Action |
|---|---|
| **A** | Accueil |
| **M** | Menu |
| **Escape** | Quitter |
| **S** | Reset scores |

Utilise `UnityRawInput` (fonctionne en arrière-plan).

```csharp
Universal_GeneralVariables.SetShortcutsEnabled(true/false);
```

---

## 14. MenuSelection — Écran d'accueil

- Logos dans `data/Images_Jeux_JVS/Logos/` — format `01_NomDuJeu.png`
- Les 4 premiers caractères (`01_`) sont retirés → nom affiché = `NomDuJeu`
- Scène chargée : `Accueil_NomDuJeu` → doit correspondre à `gameName`

### BuildState

```csharp
public static class BuildState {
    public enum State { normal, menuSelection }
    public static State CurrentState = State.normal;
}
```

Toujours vérifier avant une transition :

```csharp
if (BuildState.CurrentState == BuildState.State.normal)
    SceneManager.LoadScene(NomDuJeu_GeneralVariables.Instance.scoreScene);
else
    MenuSelectionButton.Instance.gameObject.SetActive(true);
```

---

## 15. Ressources partagées (NE PAS RECRÉER)

### Scripts Universal

| Script | Usage |
|---|---|
| `ToolBox.cs` | Utilitaires génériques |
| `ScoreBoardManager.cs` | Gestion des scores JSON |
| `ScoreBoardDisplayer.cs` | Affichage du scoreboard |
| `LoadingManager.cs` | Transition avec écran de chargement |
| `Localizer.cs` | Multi-langues |
| `PlayerPrefsHelper.cs` | Helper PlayerPrefs avec validation |
| `CustomCountDown.cs` | Compte à rebours |
| `GameTimer.cs` | Timer sur ScriptableObject |
| `SceneManagerUniversal.cs` | Chargement scène + thèmes |
| `BaseLeaderboard.cs` | Leaderboard local |
| `PresetValueManager.cs` | Presets CSV |

### Prefabs

| Prefab | Usage |
|---|---|
| `MenuPreset` | Menu de paramètres |
| `ThemeSelector` | Dropdown de thèmes |
| `MenuSelection` | Bouton retour sélection |
| `Sticker (Only Images)` | Stickers |
| `withCanvas` | Stickers avec canvas |
| `InstructionsDisplay` | Affichage instructions |

### Autres

| Ressource | Chemin |
|---|---|
| **OscSimpl** | Paquet installé |
| **Fonts / Icônes** | Dossiers partagés |
| **VFX** | VFXManager, ImageOverTime, AnimEvent |
| **Langues** | `Assets/Universal/Localisation/Langues_Jeux.tsv` |
| **Logos** | `data/Images_Jeux_JVS/Logos/` |
| **CSV Presets** | `data/Settings/jeux-parameter.csv` |

---

## 16. Package et export

1. Clic droit sur `Assets/Projects/NomDuJeu/` → Export Package
2. Inclure les dépendances
3. Import dans JVS Jeux : Assets → Import Package → Custom Package

---

## 17. Checklist

- [ ] Type choisi : Base / Thèmes / Thèmes Dynamiques
- [ ] Dossier créé dans `Assets/Projects/NomDuJeu/`
- [ ] Namespace `NomDuJeu` pour tous les scripts
- [ ] Scripts préfixés `NomDuJeu_`
- [ ] GeneralVariable dans chaque scène
- [ ] `gameName` défini
- [ ] PlayerPrefs préfixées `NomDuJeu_`
- [ ] `GameScoreBoard` enum mis à jour
- [ ] Font assignée
- [ ] Scènes dans Build Settings
- [ ] OSC Manager fonctionnel
- [ ] `receiveP = this` dans le GameManager
- [ ] LoadingManager utilisé pour les transitions
- [ ] `OSCGameScene` / `OSCScore` placés
- [ ] Logo `01_NomDuJeu.png` créé
- [ ] Si Thèmes : ThemeManager + GameThemes + SwapEntities
- [ ] Si Thèmes : SuperPreset + MenuPreset + InstructionsDisplay
- [ ] Si Thèmes Dynamiques : ThemeAppSelector configuré
- [ ] ScoreBoard prefab fait et lié
- [ ] Package exporté et importé

---

## 18. Pièges

| Problème | Solution |
|---|---|
| **Namespace** manquant | Tous les scripts dans `namespace NomDuJeu` |
| **Button Unity** utilisé | Toujours `Universal_Button` |
| **GeneralVariable** oublié | Dans CHAQUE scène |
| **PlayerPrefs** collision | Toujours `NomDuJeu_` prefix |
| **BuildState** non vérifié | Vérifier avant chaque transition |
| **OSC** non reçu | `receiveP = this` obligatoire + coordonnées normalisées |
| **Theme auto-link** cassé | Nomenclature `NomDuJeu_Theme_*` respectée |
| **CSV Presets** décimaux | Utiliser le point (.) pas la virgule |
| **Nom de scène** erroné | Doit correspondre à `gameName` |
| **Langue manquante** | Ajouter la ligne dans le TSV |

---

*Document généré par Hermes Agent — Smart Machines © 2026*