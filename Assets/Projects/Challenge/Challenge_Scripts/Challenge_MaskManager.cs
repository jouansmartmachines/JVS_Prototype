/*
using UnityEngine;
using UnityEngine.UI;

namespace Challenge
{
    public class Challenge_MaskManager : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject maskPrefab;
        public Transform parent;

        [Header("Spawn Offset")]
        public Vector2 spawnOffset;
        public Challenge_LevelManager levelManager;

        private GameObject maskGo;

        private void Start()
        {
            levelManager.OnLevelChanged += HandleLevelChanged;
        }

        private void OnDestroy()
        {
            if (levelManager != null)
                levelManager.OnLevelChanged -= HandleLevelChanged;
        }

        private void HandleLevelChanged(Challenge_LevelSettings level)
        {
            if (maskGo != null)
            {
                Destroy(maskGo);
                maskGo = null;
            }

            if (level.mask)
            {
                SpawnMask(parent);
            }
        }

        private void SpawnMask(Transform parent)
        {
            Transform trueMaskTransform = maskPrefab.transform.GetChild(0);
            trueMaskTransform.SetParent(parent, false);
            var go = trueMaskTransform.gameObject;

            // Récupération des composants
            Renderer renderer = go.GetComponentInChildren<Renderer>();
            Challenge_Mask baseMask = go.GetComponentInChildren<Challenge_Mask>();

            // Positionnement (Conserve le Z)
            Vector3 currentPos = go.transform.localPosition;
            go.transform.localPosition = new Vector3(spawnOffset.x, spawnOffset.y, currentPos.z);

            if (renderer != null)
            {
                
                if (renderer is SpriteRenderer)
                    go.transform.localScale = new Vector3(30f, 30f, 30f);

                var flashDecorator = go.AddComponent<Challenge_FlashHitD>();
                flashDecorator.SetTarget(baseMask); 
                flashDecorator.Setup(renderer, Color.yellow); 
            }

            // Autres décorateurs
            go.AddComponent<Challenge_MaskD>();
            var movement = go.AddComponent<Challenge_MovementD>();
            movement.movementStrategy = new Challenge_LateralMovementStrategy(go.transform.localPosition.x) 
            {

            };
            
            var buttonDecorator = go.AddComponent<ButtonHitDecorator>();
            buttonDecorator.SetTarget(baseMask);
        }
    }
} 

*/
using Olou;
using UnityEngine;

namespace Challenge
{
    public class Challenge_MaskManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject maskPrefab;
        public Transform parent;
        public Challenge_LevelManager levelManager;

        public Challenge_SpawnManager spawnManager;

        [Header("Recipes System")]
        [Tooltip("Glissez ici le container de recettes spécifiques au Mask")]
        private Challenge_RecipeContainer maskRecipeContainer;
        public GameObject maskRecipeContainerObject;

        [Header("Spawn Offset")]
        public Vector2 spawnOffset;

        private GameObject maskGo;

        private void Start()
        {
            levelManager.OnLevelChanged += HandleLevelChanged;
            var child = maskRecipeContainerObject.transform.GetChild(0);
            maskRecipeContainer = child.GetComponent<Challenge_RecipeContainer>();
            maskPrefab.SetActive(false);
        }

        private void OnDestroy()
        {
            if (levelManager != null)
                levelManager.OnLevelChanged -= HandleLevelChanged;
        }

        private void HandleLevelChanged(Challenge_LevelSettings level)
        {
            if (maskGo != null)
            {
                Destroy(maskGo);
                maskGo = null;
            }

            if (level.mask) SpawnMask();
        }

        private void SpawnMask()
        {
            maskPrefab.SetActive(true);
            Transform trueMaskTransform = maskPrefab.transform.GetChild(0);
            trueMaskTransform.SetParent(parent, false);
            maskGo = trueMaskTransform.gameObject;
            
            // Setup de base
            Challenge_Mask baseMask = maskGo.GetComponentInChildren<Challenge_Mask>();
            RectTransform rt = maskGo.GetComponent<RectTransform>();

            Vector3 currentPos = maskGo.transform.localPosition;
            maskGo.transform.localPosition = new Vector3(spawnOffset.x, spawnOffset.y, currentPos.z);

            // --- APPLICATION DES RECETTES ---
            if (maskRecipeContainer != null && maskRecipeContainer.globalRecipes != null)
            {
                foreach (var category in maskRecipeContainer.globalRecipes)
                {
                    if (category == null || category.recipes == null) continue;

                    foreach (var recipe in category.recipes)
                    {
                        if (recipe != null)
                        {
                            // On passe 'null' pour le SpawnManager car on ne veut pas de dépendance
                            recipe.Execute(maskGo, baseMask, TargetCategory.Mask, spawnManager, rt);
                        }
                    }
                }
            }
        }
    }
}

