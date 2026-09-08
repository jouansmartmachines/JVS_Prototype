# 📐 Demolition - Guide de Conception & Règles Éditeur

Ce document regroupe les règles strictes de conception, d'architecture et de manipulation des scènes pour le projet **Démolition**.

---

## 🛑 1. Règles d'Or & Périmètre
- **Scope Strict :** Toute modification de code ou d'asset doit se faire exclusivement dans le dossier `Assets/Projects/Demolition/`.
- **Zéro Duplication :** À chaque intervention sur l'éditeur ou les scènes, toujours vérifier, nettoyer et écraser l'ancien état proprement au lieu d'empiler des GameObjects orphelins ou en double.
- **Gestion Hors-Play :** La configuration des scènes, du Sol, de l'UI et des Backgrounds doit être faite en mode Éditeur (hors Play) via les outils du menu `Tools/Demolition/`, jamais par instanciation dynamique au lancement dans `Start()` ou `Awake()`.
- **Mémoire Éditeur :** À chaque modification du script éditeur ou demande de l'utilisateur, ce fichier doit être mis à jour avec les nouveaux retours et ajustements.

---

## 🧩 2. Énigmes Physiques Évolutives & Archétypes "Angry Birds"

La difficulté progresse de niveau en niveau avec des puzzles architecturaux distincts :

### 🏛️ Archétypes d'Énigmes :
1. **Niveau 1 : Le Portique Simple (Dolmen / Initiation)**
   - 2 piliers en bois supportant une lourde poutre horizontale avec une caisse de faîte.
   - Le Fantôme est abrité au centre au sol.
   - *Objectif / Point faible :* Abattre un des deux piliers pour faire basculer la poutre lourde sur le fantôme.
2. **Niveau 2 : Le Bunker Blindé & Clé de Voûte (Keystone Bunker)**
   - 3 colonnes (gauche, centre, droite) supportant une toiture double.
   - La colonne centrale est une caisse simple fragile (clé de voûte).
   - Le Fantôme est protégé dans la chambre droite.
   - *Objectif / Point faible :* Détruire la clé de voûte centrale pour briser l'équilibre du toit.
3. **Niveau 3 : La Tour Fortifiée à 2 Étages (Two-Tier Tower)**
   - Étage 1 large avec 2 piliers + poutre + Fantôme 1 abrité.
   - Étage 2 resserré avec 2 piliers + toit + Fantôme 2 perché au sommet.
   - *Objectif / Point faible :* Effondrement en cascade en sapant les piliers du rez-de-chaussée.
4. **Niveau 4 : Le Balancier en Porte-à-Faux (Cantilever Seesaw)**
   - Poutre reposant de manière asymétrique sur un pivot.
   - Contrepoids lourd (2 caisses empilées) à gauche, Fantôme suspendu à droite.
   - *Objectif / Point faible :* Dégager le pivot pour faire chuter le contrepoids.
5. **Niveau 5+ : La Citadelle Multi-Chambres (Grand Citadel)**
   - Large structure à 2 grandes chambres abritant plusieurs Fantômes avec toiture complexe.

---

## 🏗️ 3. Gestion des Points de Spawn & Ancres (`Demolition_ObstacleAnchor`)
- **Point de spawn précis (`obstaclePrefabs[0]`) :** Dans les prefabs d'environnement (`Env1Sun`, `Env1Night`, etc.), les objets référencés dans `anchor.obstaclePrefabs` correspondent aux GameObjects cibles placés à la position exacte et avec l'orientation idéale face à la caméra.
- **Parent d'instanciation :** Les structures procédurales sont donc directement parentées sous `anchor.obstaclePrefabs[i].transform` (avec repli sur `anchor.transform` si vide).
- **Préservation des Formes :** Les formes de blocs (poutres $2\times 1$, piliers $1\times 3$, caisses $1\times 1$, blocs $2\times 2$) proviennent toujours du `Demolition_ObstacleSpawner.availableBlocks`.
- **Nettoyage Préalable :** Avant chaque instanciation, les enfants sous `anchor.transform` et `spawnPoint.transform` sont purgés pour éviter tout dédoublement.

---

## ⚙️ 4. Physique, Support & Stabilisation

### 📐 Subdivisions & Support :
- **Subdivision Interne :** `gridSubdivision = 2` pour une précision accrue.
- **Règle d'or :** Aucun bloc lourd ne peut être généré sur la tête d'un Fantôme.
- **Condition de Victoire :** Le passage au niveau supérieur s'enclenche uniquement quand **tous les Fantômes** du niveau sont vaincus (`CheckRemainingFantomesRoutine`).

### ⚙️ Stabilisation (`Demolition_StructureStabilizer`) :
- Au spawn, amortissement renforcé (`damping = 6`) avec `FreezeRotationX | FreezeRotationZ | FreezePositionZ` pendant $2.5\text{s}$ pour laisser la structure se poser sans secousse.
- Restauration individuelle des dampings d'origine via dictionnaires après 2.5s.

---

## 🖼️ 5. Règle Stricte sur les Backgrounds & le Sol (Ground)

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

## 🛠️ 6. Outils Éditeur (`Demolition_SetupEditor`)
Accessible depuis la barre supérieure Unity : `Tools` > `Demolition - Panneau Configuration Editeur`
- **1. Configurer Background & Sol dans GameScene (Hors Play)**
- **2. Configurer Background dans Scene Accueil (Hors Play)**
- **3. Configurer Background dans Scene Menu (Hors Play)**
- **4. Configurer Background dans Scene Score (Hors Play)**
- **5. Tout Configurer (Prefabs, Sons, Scènes, UI)**
