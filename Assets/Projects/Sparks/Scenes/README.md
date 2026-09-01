# Sparks — Modifications manuelles dans Unity

Les scènes ont été copiées depuis Dobble et les GUIDs des scripts/GeneralVariables ont été
remplacés. Voici ce qui reste à faire **dans Unity** une fois le pull effectué :

---

## 1. GameScene_Sparks.unity — Missing scripts

La GameScene de Dobble référence plusieurs scripts qui n'existent pas chez Sparks.
**Supprimer les GameObjects ou components suivants :**

| GameObject | Component manquant | Action |
|---|---|---|
| `Dobble_GameManager` (ou renommé) | `Dobble_GameManager` | **Supprimer** le GameObject |
| `Dobble_Circles` | `Dobble_Circles` | **Supprimer** le GameObject |
| `Dobble_TeamManager` | `Dobble_TeamManager` | **Supprimer** le GameObject |
| `SoundManager` | `Dobble_SoundManager` | **Supprimer** le GameObject |
| `Loader` | `Dobble_LoadData` | **Supprimer** le GameObject |
| Tout GameObject avec `Dobble_ButtonLinked` | Missing script | **Supprimer** le GameObject |
| Tout GameObject avec `Dobble_Card` | Missing script | **Supprimer** le GameObject |

> **Astuce :** Dans Unity, clic droit sur la racine → Delete tout ce qui est en `Missing Script`.
> Ou : Sélectionner la scène, regarder dans la hiérarchie les GameObjects avec `(scripts manquants)`.

## 2. Ajouter les GameObjects Sparks

Après nettoyage, ajouter :

| GameObject | Components | Rôle |
|---|---|---|
| `Sparks_GameManager` | `Sparks_GameManager.cs` | Boucle de jeu, spawn, score, timer |
| `VolcanoOrigin` | Transform (vide) | Point d'éjection des primitives |
| `Canvas` | Canvas + ScoreText + TimerText | UI déjà dans la scène si conservée |

## 3. Scénographie 3D

- Ajouter une **Directional Light** si absente
- La **Camera** en mode Perspective (copiée depuis Dobble en Ortho ? A vérifier)
- Le Canvas en **Screen Space - Overlay** (c'est déjà bon pour Sparks)

## 4. GeneralVariable.prefab

- Dans le dossier `Prefabs/`, ouvrir `GeneralVariable.prefab`
- Vérifier que `gameName = "Sparks"` dans le component `Universal_GeneralVariables`
- Placer ce prefab dans **toutes les scènes** (Accueil, Menu, GameScene, Score)

## 5. Menu_Sparks.unity

Le menu Dobble a des éléments spécifiques à Dobble. Si des paramètres Sparks
sont nécessaires (Toggle Mode Rapide), les ajouter manuellement.

---

✅ **Scènes prêtes !** Une fois ces modifications faites, le jeu devrait fonctionner.
