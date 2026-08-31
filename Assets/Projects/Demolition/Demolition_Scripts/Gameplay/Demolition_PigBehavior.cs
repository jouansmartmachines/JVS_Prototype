using UnityEngine;
using System.Collections;

namespace Demolition
{
    /// <summary>
    /// Comportement vivant des cochons façon Angry Birds :
    /// - Respiration procédurale et micro-sauts joueurs en état d'attente (Idle Hop)
    /// - Devient rouge de colère / blessé au fur et à mesure des dégâts
    /// - État de panique avec tremblement et yeux écarquillés quand la structure s'effondre
    /// - Explosion d'étoiles dorées, confettis et pop satisfaisant à la défaite
    /// </summary>
    public class Demolition_PigBehavior : MonoBehaviour
    {
        private Demolition_Block block;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private Vector3 baseScale;
        private Color normalColor = Color.white;
        private Color damageColor = new Color(1f, 0.35f, 0.35f, 1f); // Rouge vif de colère

        private float nextHopTime;
        private bool isPanicking = false;
        private float idleOffset;

        void Awake()
        {
            block = GetComponent<Demolition_Block>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            baseScale = transform.localScale;
            idleOffset = Random.Range(0f, 100f);
            nextHopTime = Time.time + Random.Range(2.0f, 4.5f);
        }

        void Start()
        {
            if (spriteRenderer != null)
            {
                normalColor = spriteRenderer.color;
            }
        }

        void Update()
        {
            if (block == null || spriteRenderer == null) return;

            // 1. Détection de panique (si le cochon tombe, glisse ou tourne vite)
            CheckPanicState();

            // 2. Comportement Idle vivant si tout est calme
            if (!isPanicking)
            {
                ApplyIdleJuice();
            }
            else
            {
                ApplyPanicJuice();
            }
        }

        private void CheckPanicState()
        {
            if (rb == null) return;

            float speed = rb.linearVelocity.magnitude;
            float rotSpeed = Mathf.Abs(rb.angularVelocity);

            // Si le cochon subit une accélération ou tourne dans les airs
            if (speed > 1.2f || rotSpeed > 45f)
            {
                isPanicking = true;
            }
            else
            {
                isPanicking = false;
            }
        }

        private void ApplyIdleJuice()
        {
            // Respiration douce
            float breath = Mathf.Sin((Time.time + idleOffset) * 2.8f) * 0.035f;
            transform.localScale = new Vector3(baseScale.x * (1f + breath), baseScale.y * (1f - breath), baseScale.z);

            // Petit sautillement mignon occasionnel
            if (Time.time >= nextHopTime)
            {
                nextHopTime = Time.time + Random.Range(2.5f, 5.0f);
                StartCoroutine(DoTinyHop());
            }
        }

        private IEnumerator DoTinyHop()
        {
            float duration = 0.28f;
            float elapsed = 0f;

            Vector3 startPos = transform.localPosition;
            float hopHeight = 0.09f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float yOffset = Mathf.Sin(progress * Mathf.PI) * hopHeight;

                // Squash & Stretch pendant le saut
                float stretch = Mathf.Sin(progress * Mathf.PI) * 0.12f;
                transform.localScale = new Vector3(baseScale.x * (1f - stretch), baseScale.y * (1f + stretch), baseScale.z);

                yield return null;
            }

            transform.localScale = baseScale;
        }

        private void ApplyPanicJuice()
        {
            // Tremblement de panique
            float wobble = Mathf.Sin(Time.time * 25f) * 0.06f;
            transform.localScale = new Vector3(baseScale.x * (1f + wobble), baseScale.y * (1f - wobble), baseScale.z);
        }

        /// <summary>
        /// Appelé quand le cochon subit des dégâts : rougit de colère et gonfle ses joues.
        /// </summary>
        public void OnDamaged(int currentHp, int maxHp)
        {
            if (spriteRenderer == null) return;

            float damageRatio = 1f - Mathf.Clamp01((float)currentHp / maxHp);

            // Teinte rougeoyante progressive selon les PV restants
            spriteRenderer.color = Color.Lerp(normalColor, damageColor, damageRatio * 0.85f);

            // Réaction de gonflement de colère
            StartCoroutine(AngrySwellEffect());
        }

        private IEnumerator AngrySwellEffect()
        {
            float duration = 0.18f;
            float elapsed = 0f;

            Vector3 angryPuffScale = baseScale * 1.22f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(angryPuffScale, baseScale, t);
                yield return null;
            }

            transform.localScale = baseScale;
        }

        /// <summary>
        /// Déclenché à la mort du cochon : pop d'étoiles dorées et débris.
        /// </summary>
        public void OnDefeated()
        {
            // Nuage doré & étoiles
            Demolition_DebrisSpawner.SpawnStarBurst(transform.position);
        }
    }
}
