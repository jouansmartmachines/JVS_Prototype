// Challenge_MaterialBehaviorD.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Challenge
{
    public interface Challenge_IMaterialBehavior
    {
        void Initialize(Material targetMaterial, ITarget target);
        void Activate();
        void Stop();
    }

    public class Challenge_MaterialBehaviorD : Challenge_TargetDecorator
    {
        [Header("Références")]
        public Challenge_MaterialManager materialManager;
        public string materialKey;

        public Material targetMaterial;
        private List<Challenge_IMaterialBehavior> behaviors = new List<Challenge_IMaterialBehavior>();
        private System.Action<ITarget> onHitAction;
        private Image targetImage;

        public void SetMaterial()
        {
            // ✅ Guard : targetImage doit être set avant
            if (targetImage == null)
            {
                Debug.LogError($"[Challenge_MaterialBehaviorD] targetImage est null sur {gameObject.name}. Appelez SetTarget(Image) avant SetMaterial().");
                return;
            }

            Material matFromManager = materialManager.GetMaterial(materialKey);
            targetMaterial = matFromManager != null ? Instantiate(matFromManager) : new Material(Shader.Find("Standard"));
            targetImage.material = targetMaterial;

            foreach (var b in behaviors)
                b.Initialize(targetMaterial, target);

            if (target != null)
            {
                onHitAction = _ =>
                {
                    foreach (var b in behaviors)
                        b.Activate();
                };
                target.OnHitEvent += onHitAction;
            }
        }

        public void AddBehavior(Challenge_IMaterialBehavior behavior)
        {
            behaviors.Add(behavior);
        }

        public void SetTarget(Image image)
        {
            targetImage = image;
        }

        // ✅ FIX : cherche sur le GameObject lui-même, plus besoin de target
        public Image FindChildImage(string childName)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                    return child.GetComponent<Image>();
            }
            Debug.LogWarning($"[Challenge_MaterialBehaviorD] Image '{childName}' introuvable sur {gameObject.name}.");
            return null;
        }

        private void OnDestroy()
        {
            if (target != null && onHitAction != null)
                target.OnHitEvent -= onHitAction;

            foreach (var b in behaviors)
                b.Stop();
        }
    }
}
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Challenge
{
    public interface Challenge_IMaterialBehavior
    {
        void Initialize(Material targetMaterial, ITarget target);
        void Activate();
        void Stop();
        
    }

    public class Challenge_MaterialBehaviorD : Challenge_TargetDecorator
    {
        [Header("Références")]
        public Challenge_MaterialManager materialManager;      // Gestion centralisée des matériaux
        public string materialKey;                             // Clé pour récupérer le matériau

        public Material targetMaterial;
        private List<Challenge_IMaterialBehavior> behaviors = new List<Challenge_IMaterialBehavior>();
        private System.Action<ITarget> onHitAction;

        private Image targetImage ;

        public void SetMaterial()
        {
            // Récupérer le matériau depuis le MaterialManage
            Material matFromManager = materialManager.GetMaterial(materialKey);

            // Si aucun matériau n’est fourni, on crée un matériau par défaut
            targetMaterial = matFromManager != null ?  Instantiate(matFromManager) : new Material(Shader.Find("Standard"));

            targetImage.material = targetMaterial;

            // Initialiser tous les comportements
            foreach (var b in behaviors)
            {
                b.Initialize(targetMaterial, target);
            }

            // S'abonner à OnHitEvent pour les comportements qui doivent réagir au hit
            if (target != null)
            {
                onHitAction = _ =>
                {
                    foreach (var b in behaviors)
                        b.Activate();
                };
                target.OnHitEvent += onHitAction;
            }
        }

        public void AddBehavior(Challenge_IMaterialBehavior behavior)
        {
            behaviors.Add(behavior);
        }

        public void SetTarget(Image renderer)
        {
            targetImage = renderer;
        }

        public Image FindChildImage(string name)
        {
            if (target == null) return null;
            foreach (Transform child in (target as MonoBehaviour).transform.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child.GetComponent<Image>();
            }
            return null;
        }


        private void OnDestroy()
        {
            if (target != null && onHitAction != null)
                target.OnHitEvent -= onHitAction;

            foreach (var b in behaviors)
                b.Stop();
        }
    }
}
*/