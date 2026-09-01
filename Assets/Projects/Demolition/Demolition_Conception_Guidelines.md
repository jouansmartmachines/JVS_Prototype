# 📐 Demolition - Guide de Conception & Règles Éditeur

Ce document regroupe les règles strictes de conception, d'architecture et de manipulation des scènes pour le projet **Démolition**.

---

## 🛑 1. Règles d'Or & Périmètre
- **Scope Strict :** Toute modification de code ou d'asset doit se faire exclusivement dans le dossier `Assets/Projects/Demolition/`.
- **Zéro Duplication :** À chaque intervention sur l'éditeur ou les scènes, toujours vérifier, nettoyer et écraser l'ancien état proprement au lieu d'empiler des GameObjects orphelins ou en double.
- **Gestion Hors-Play :** La configuration des scènes, du Sol, de l'UI et des Backgrounds doit être faite en mode Éditeur (hors Play) via les outils du menu `Tools/Demolition/`, jamais par instanciation dynamique au lancement dans `Start()` ou `Awake()`.
- **Mémoire Éditeur :** À chaque modification du script éditeur ou demande de l'utilisateur, ce fichier doit être mis à jour avec les nouveaux retours et ajustements.

---

## 🖼️ 2. Règle Stricte sur les Backgrounds & le Sol (Ground)

### Backgrounds (Canvas UI) :
> [!IMPORTANT]
> - Dans **TOUTES les scènes** (`Accueil_Demolition`, `GameScene_Demolition`, `Menu_Demolition`, `Score_Demolition`), le `Background` doit être un **enfant direct du Canvas UI**.
> - Composant obligatoire : `Image` (et non un `SpriteRenderer` perdu dans le monde).
> - Ancrage plein écran : `anchorMin = (0, 0)`, `anchorMax = (1, 1)`, `offsetMin = (0, 0)`, `offsetMax = (0, 0)`, `localScale = (1, 1, 1)`.
> - `SetAsFirstSibling()` pour rester en arrière-plan sous les boutons et textes.
> - `raycastTarget = false` dans `GameScene` pour ne pas bloquer les tirs / clics vers le monde 2D.
> - **Nettoyage strict :** Tout GameObject `Background` orphelin hors Canvas doit être immédiatement détecté et détruit.

### Sol dans GameScene (`Ground`) :
> [!IMPORTANT]
> - Objet nommé `Ground` à `(0, -5.2f, 0)`.
> - `SpriteRenderer` avec le sprite `sol.png` explicitement assigné, `drawMode = SpriteDrawMode.Tiled`, `size = (300, 2.4f)`, `sortingOrder = 2`.
> - `BoxCollider2D` avec `size = (300, 2.4f)`.
> - Composant `Demolition_GroundScroll` pour le défilement continu synchronisé.

---

## 🎮 3. Équilibrage Gameplay & Game Feel (Juice)

### 🧱 Durabilité & Physique des Matériaux
| Matériau | PV (HP) | Masse | Comportement |
| :--- | :---: | :---: | :--- |
| 🪟 **Verre** | **2 HP** | $0.8\text{ kg}$ | Fragile, se fissure au 1er tir, se casse au 2e. Chutes sensibles. |
| 🪵 **Bois** | **4 HP** | $1.5\text{ kg}$ | Équilibré, encaisse plusieurs tirs, se fissure progressivement. |
| 🪨 **Pierre** | **8 HP** | $4.0\text{ kg}$ | Pilier lourd et ultra-résistant, sert d'armure porteuse. |
| 🐷 **Cochon** | **3 à 6 HP** | $1.0\text{ kg}$ | Cible vivante, sensible aux écrasements. |

### 🐷 Comportement Vivant des Cochons (`Demolition_PigBehavior`)
1. **Idle :** Respiration élastique continue + micro-sautillement mignon avec squash & stretch toutes les $2.5\text{s}$ à $5\text{s}$.
2. **Réaction Dégâts :** Rougit progressivement de colère (teinte rouge vif proportionnelle aux PV perdus) + joues qui gonflent.
3. **Panique :** Tremblement frénétique dès que le bâtiment vacille ou tourne dans les airs ($v > 1.2\text{ m/s}$).
4. **Victoire :** Pop d'étoiles dorées scintillantes (`SpawnStarBurst`) et affichage combo popup.

### 🕊️ Poussée & Impact Doux
- Force d'impact : `pushForce = 2.2f` (très douce, pour déstabiliser sans désintégrer toute la structure d'un coup).
- Dégâts ciblés : `1 HP` par tir sur le bloc précis touché (aucun dégât de zone massif non désiré).
- Anti-Flood : `minTimeBetweenShots = 0.12f`.

---

## 🛠️ 4. Outils Éditeur (`Demolition_SetupEditor`)
Accessible depuis la barre supérieure Unity : `Tools` > `Demolition - Panneau Configuration Editeur`
- **1. Configurer Background & Sol dans GameScene (Hors Play)**
- **2. Configurer Background dans Scene Accueil (Hors Play)**
- **3. Configurer Background dans Scene Menu (Hors Play)**
- **4. Configurer Background dans Scene Score (Hors Play)**
- **5. Tout Configurer (Prefabs, Sons, Scènes, UI)**
