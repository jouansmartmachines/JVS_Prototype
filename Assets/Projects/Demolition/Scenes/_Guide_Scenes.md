# Demolition — Guide des scènes gelées

Ces scènes ne doivent plus être modifiées directement.
Ce fichier liste tout ce qui aurait dû y être fait ou ce qui est à savoir pour les reproduire.

---

## Accueil_Demolition.unity

**Source :** Copiée depuis Basketball (`Accueil_Basketball.unity`)

### Éléments présents
- Main Camera (fileID 100) + Directional Light
- GeneralVariable.prefab (GameManager, OSC_Manager)
- Canvas + Background (Image avec sprite `bg_accueil.png`)
- Play Button avec `Universal_PlayButton` (pas de script `Demolition_PlayButton` — OK, le script universel suffit)
- InstructionsDisplay (prefab universel)

### Ce qui aurait dû être fait / problèmes connus
- [ ] Vérifier que le Canvas `m_RenderMode` est 0 (Screen Space Overlay) — OK pour une scène d'accueil sans sprites interactifs. Si problème de clic, passer en 1.
- [ ] `gameName` dans le GeneralVariable.prefab doit être "Demolition" (vérifier dans le prefab, pas dans la scène)
- [ ] Le PlayButton appelle `LoadingManager.LoadScene("Menu_Demolition")` — vérifier que le nom de scène correspond
- [ ] Background sprite assigné dans le prefab MENU — ne pas toucher à la scène directement

---

## Menu_Demolition.unity

**Source :** Copiée depuis Basketball (`Menu_Basketball.unity`)

### Éléments présents
- GeneralVariable.prefab
- Canvas + Background (`bg_menu.png`)
- MenuPreset (prefab universel / Universal/Prefab/)
- LineMenu (Universal/SelectionMenu/LineMenu.prefab)
- Instructions (Toggle — copié depuis `Menu_Differences` SpotTheDif)
- Stickers (prefab universel)
- Difficulty (Dropdown — copié depuis `Menu_Basketball`)
- ModeOiseau (Toggle — spécifique Demolition, copié depuis le pattern Demolition original)
- Stickers (Universal/Prefab/Sticker)

### D'où viennent les éléments du Menu

| Catégorie | Copié depuis | GameObject exact |
|---|---|---|
| **Dropdown Difficulty** 🏀 | `Menu_Basketball.unity` | `Difficulty` (dropdown complet + Template/Item/Arrow) |
| **Toggle ModeOiseau** 💣 | `Menu_Demolition` original | `ModeOiseau` (container + label + toggle + checkmark) |
| **Instructions Toggle** 🔍 | `Menu_Differences.unity` (SpotTheDif) | `Instructions` |
| **Stickers** 📌 | Universal/Prefab/Sticker (Only Image).prefab | Sticker (Only Image) |

### Ce qui aurait dû être fait / problèmes connus
- [ ] **Police** — NE PAS TOUCHER à la police du menu (règle projet stricte)
- [ ] Vérifier que les clés PlayerPrefs sont correctes :
  - `Demolition_GameTime` pour le dropdown Difficulty
  - `Demolition_ScrollSpeed` pour le slider si présent
  - `Demolition_ModeOiseau` pour le toggle
- [ ] **ModeOiseau Toggle** a été créé spécifiquement pour Demolition — son GameObject n'existe pas dans les autres jeux. Si tu copies Menu_Demolition pour un autre jeu, supprime-le.
- [ ] Les `SliderSavePlayersPref` sur les éléments doivent pointer vers les bonnes clés PlayerPrefs
- [ ] Le `MenuPreset` doit référencer les bons `.asset` dans `Preset/` (Demolition_Preset, Demolition_GameTime, Demolition_Instructions, Demolition_Sticker, Demolition_ScrollSpeed)

---

## Score_Demolition.unity

**Source :** Copiée depuis Basketball (`Score_Basketball.unity`)

### Éléments présents
- GeneralVariable.prefab
- Canvas + Background (`bg_score.png`)
- ScoreBoardDisplayer (dans le prefab GeneralVariable ou ScoreDisplay)
- Universal_Button pour le replay / retour

### Ce qui aurait dû être fait / problèmes connus
- [ ] Le `GeneralVariable.ReceiveName()` gère l'init du scoreboard via `_scoreBoardDisplayer.InitScoreBoard()` — pas besoin de `Demolition_ScoreBoardManager.cs`
- [ ] Vérifier que `GameScoreBoard.Demolition` existe dans `ScoreBoardManager.cs` (enum GameScoreBoard)
- [ ] La transition depuis la GameScene doit charger `Score_Demolition` via `LoadingManager`
- [ ] Le bouton de replay appelle `LoadingManager.LoadScene("GameScene_Demolition")`
- [ ] Le bouton menu appelle `LoadingManager.LoadScene("Menu_Demolition")`

---

## Règles générales applicables

- **Ne JAMAIS éditer le YAML de ces scènes à la main** → `patch()` seulement si absolument nécessaire (remplacement de GUID, string)
- **Ne JAMAIS copier une scène existante** pour un nouveau jeu → copier depuis Basketball et remplacer les références
- **Ne JAMAIS ajouter d'UI element dans ces scènes** → le faire dans des prefabs universels
- **Ne JAMAIS toucher aux polices du Menu**
- **Canvas RenderMode :** 0 (Overlay) pour Accueil et Score (pas de sprites interactifs), 1 (Camera) pour GameScene uniquement
- **GeneralVariable.prefab** doit être placé dans TOUTES les scènes (vérifier)