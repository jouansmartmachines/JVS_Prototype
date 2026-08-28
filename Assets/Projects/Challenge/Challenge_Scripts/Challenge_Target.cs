using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Challenge
{

   
    public enum DeathCause
    {
        Player,
        Lifetime,
        Other
    }

    [System.Serializable]
    public struct TargetCategoryValue
    {
        public TargetCategory category;
        public float spawnChance; 
    }

    public class TargetCategorySettings : MonoBehaviour
    {
        public TargetCategoryValue[] categoryValues;
    }

    public class Challenge_Target : Challenge_BaseInteractive
    {
        public int health = 5;
        public Image targetImage;

        public float scoreMultiplier = 1f;
        public Color hitColor = Color.red;
        public float flashDuration = 0.2f;
        public TargetCategoryValue category;
        public Vector2 Pos;
        private Color originalColor;
        public Image fillImage;
        private bool isDying = false;
        public GameObject particlesEffects;
        public GameObject popUp;

        public float timeAdded;
        public DeathCause lastDeathCause = DeathCause.Other; 


        
        [Header("Layers")]
        public GameObject mainLayer;
        public GameObject secondLayer;
        public GameObject thirdLayer;
        private List<GameObject> hiddenImages;
        public GameObject[] particlesImages;
        public GameObject EndLayer;
 

        private void Awake()
        {
            type = ObjectType.Target;
            if (targetImage != null) originalColor = targetImage.color;

            hiddenImages = new List<GameObject>();


            hiddenImages.Add(mainLayer);
            hiddenImages.Add(secondLayer);
            hiddenImages.Add(thirdLayer);
            
        }

        public override void TakeDamage(int amount)
        {
            if (isDying) return;
        
            health -= amount;
            TriggerHitEvent();
            if (targetImage != null) StartCoroutine(FlashHit());

            if (health <= 0)
            {
                isDying = true;
                Challenge_AudioManager.i.PlayOneShot(SoundType.Type,GetDeathSoundIndex());
                Die(DeathCause.Player,2f);
            }
        }

        private IEnumerator DelayedDestroy(float timer)
        {
            foreach (var img in hiddenImages)
            {
                img.gameObject.SetActive(false); 
            }
            yield return new WaitForSeconds(timer);
            Destroy(gameObject);
        }

        public override void Move() { /* movement handled by decorators */ }

        public override void OnHit()
        {
            TakeDamage(5);
        }

        public void Die(DeathCause cause,float timer)
        {
            TriggerDeathEvent(cause);
            StartCoroutine(DelayedDestroy(timer));
        }
        private int GetDeathSoundIndex()
        {
            switch (category.category)
            {
                case TargetCategory.Malus:
                    return 5; 

                case TargetCategory.Ephemere:
                    return 4; 

                case TargetCategory.Evolutif:
                    return 3; 

                case TargetCategory.Bonus:
                    return 2; 

                default:
                    return 1;
            }
        }
        private System.Collections.IEnumerator FlashHit()
        {
            targetImage.color = hitColor;
            yield return new WaitForSeconds(flashDuration);
            targetImage.color = originalColor;
        }
    }
}