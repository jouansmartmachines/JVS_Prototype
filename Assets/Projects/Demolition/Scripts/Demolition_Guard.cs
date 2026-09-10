using UnityEngine;
using System.Collections;

namespace Demolition
{
    public class Demolition_Guard : MonoBehaviour
    {
        [Header("Patrouille Horizontale (Axe X uniquement) - [MODE PATROUILLE DÉSACTIVÉ]")]
        public float patrolWidth = 3f;      // Amplitude du va-et-vient sur l'axe X
        public float patrolSpeed = 2f;      // Vitesse du mouvement
        public float rotationSpeed = 360f;  // Vitesse de rotation fluide (degrés par seconde)
        public float maxIdleInterval = 3f;  // Temps max avant de passer en Idle ou de reprendre la route

        [Header("Hit / Death")]
        public int maxHits = 2;             // Modifié à 2 pour mourir après 2 coups
        public float hitRecoilSpeed = 8f;

        public System.Action<Demolition_Guard> OnGuardDestroyed;

        [Header("Animation")]
        public Animator animator;           // Référence à l'Animator

        private Transform guardCenter;      
        private int hitCount = 0;
        private bool isAttacking = false;   
        private bool isIdling = false;      
        private float startX;               // Position X de départ
        private float fixedY;               // Hauteur Y fixe d'origine
        private float fixedZ;               // Profondeur Z fixe d'origine
        private Vector3 startScale;
        private float lastXPosition;        // Pour calculer la direction du mouvement

        public void Initialize(Transform center, float startAngle)
        {
            guardCenter = center;
            startScale = transform.localScale;

            startX = transform.position.x;
            fixedY = transform.position.y;
            fixedZ = transform.position.z;
            lastXPosition = startX;

            if (animator == null)
                animator = GetComponent<Animator>();
        }

        void Update()
        {
            // S'il est en train d'attaquer, il s'arrête net
            if (isAttacking)
            {
                if (animator != null)
                    animator.SetBool("IsWalking", false);
                return;
            }

            // Par défaut, sans patrouille, on s'assure que l'animation de marche est coupée
            if (animator != null)
                animator.SetBool("IsWalking", false);
        }

        private void OnDestroy()
        {
            // Déclenche l'événement juste avant que le GameObject soit détruit
            OnGuardDestroyed?.Invoke(this);
        }

        public void OnTouched()
        {
            if (isAttacking) return;

            hitCount++;

            if (hitCount >= maxHits)
            {
                Die();
            }
            else
            {
                StartCoroutine(HitAndAttackRoutine());
            }
        }

        private void Die()
        {
            // Déclenche l'animation de mort si vous en avez une :
            // if (animator != null) animator.SetTrigger("Die");

            // Détruit l'objet (déclenche automatiquement OnDestroy et l'événement OnGuardDestroyed)
            Destroy(gameObject);
        }

        private IEnumerator HitAndAttackRoutine()
        {
            isAttacking = true;

            // Stoppe la marche visuellement pendant le coup
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetTrigger("Hit");
            }

            float direction = (transform.position.x >= (guardCenter != null ? guardCenter.position.x : transform.position.x)) ? 1f : -1f;
            
            float elapsed = 0f;
            float recoilDuration = 0.12f;
            
            /*
            // Recul de l'impact
            while (elapsed < recoilDuration)
            {
                Vector3 pos = transform.position;
                pos.x += direction * (hitRecoilSpeed * Time.deltaTime);
                transform.position = pos;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
*/
            yield return new WaitForSeconds(0.15f);

            // Déclenche l'attaque aléatoire (Blend Tree)
            if (animator != null)
            {
                float randomAttack = Random.Range(0f, 2f);
                animator.SetFloat("AttackIndex", randomAttack);
                animator.SetTrigger("Attack");
            }

            // Attend la fin de l'animation d'attaque
            yield return new WaitForSeconds(0.6f);

            isAttacking = false;
        }
    }
}