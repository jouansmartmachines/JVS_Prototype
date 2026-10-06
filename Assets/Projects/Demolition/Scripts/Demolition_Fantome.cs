using UnityEngine;
using System.Collections;

namespace Demolition
{
    public class Demolition_Fantome : MonoBehaviour
    {
        [Header("Cinématique & Délais")]
        [Tooltip("Pause (en secondes) après le 'pouf' avant le déplacement.")]
        public float delayBeforeFly = 0.4f;

        [Tooltip("Durée de l'approche vers la caméra.")]
        public float approachDuration = 0.8f;

        [Tooltip("Temps d'attente à la caméra avant de lancer l'animation IsArrived.")]
        public float delayBeforeArrived = 1.5f;

        [Tooltip("Décalage vers le bas devant la caméra (ex: -0.5 pour descendre la position).")]
        public float verticalOffset = -0.5f;

        [Tooltip("Durée de l'accélération lors de l'envol.")]
        public float accelerationDuration = 1f;

        [Header("Composants & Attributs")]
        [Tooltip("Glisse ici l'Animator de l'enfant.")]
        public Animator animator;

        [Header("Triggers & Vitesse Animator")]
        [Tooltip("Trigger déclenché TOUT AU DÉBUT de la séquence.")]
        public string gameEndedTrigger = "IsGameEnded";

        [Tooltip("Trigger déclenché quand le fantôme arrive devant la caméra.")]
        public string arrivedTrigger = "IsArrived";

        [Tooltip("Multiplicateur de vitesse pour l'animation d'arrivée (ex: 1f pour normal, 0.5f pour ralenti).")]
        public float arrivedAnimSpeed = 1f;

        [Header("Envol & Zigzag")]
        [Tooltip("Vitesse maximale d'ascension.")]
        public float maxFlySpeed = 6f;

        [Tooltip("Largeur de l'oscillation (amplitude du zigzag).")]
        public float zigzagAmplitude = 1.2f;

        [Tooltip("Vitesse de l'oscillation (fréquence du zigzag).")]
        public float zigzagFrequency = 2.5f;

        [Header("Effets")]
        public GameObject releaseEffect;
        public AudioClip releaseSound;

        private bool isFlyingAway = false;
        private Rigidbody rb;
        private Collider col;
        private Renderer meshRenderer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            meshRenderer = GetComponentInChildren<Renderer>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    Debug.Log($"[Fantome] Animator trouvé sur : {animator.gameObject.name}", animator.gameObject);
                }
                else
                {
                    Debug.LogError("[Fantome] Aucun Animator trouvé sur cet objet ou ses enfants !", this);
                }
            }
        }

        private void OnEnable()
        {
            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.RegisterFantome(this);
        }

        private void OnDisable()
        {
            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.UnregisterFantome(this);
        }

        public void ReleaseAndFlyAway()
        {
            if (isFlyingAway) return;
            isFlyingAway = true;

            StartCoroutine(FlyRoutine());
        }

        private IEnumerator FlyRoutine()
        {
            // -------------------------------------------------------------
            // 1. DÉCLENCHEMENT IMMÉDIAT DE IsGameEnded AU TOUT DÉBUT
            // -------------------------------------------------------------
            if (animator != null && !string.IsNullOrEmpty(gameEndedTrigger))
            {
                Debug.Log($"[Fantome] Début de FlyRoutine : Envoi immédiat de '{gameEndedTrigger}'", animator.gameObject);
                animator.speed = 1f;
                animator.ResetTrigger(gameEndedTrigger);
                animator.SetTrigger(gameEndedTrigger);
            }

            // Désactivation physique
            if (rb != null) rb.isKinematic = true;
            if (col != null) col.enabled = false;

            // Masquer le mesh de la cage
            if (meshRenderer != null)
                meshRenderer.enabled = false;

            // Effets visuels & sonores
            if (releaseEffect != null)
                Instantiate(releaseEffect, transform.position, Quaternion.identity);

            if (releaseSound != null && Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.PlaySfx(releaseSound);

            yield return new WaitForSeconds(delayBeforeFly);

            Quaternion originalRotation = transform.rotation;

            // -------------------------------------------------------------
            // 2. APPROCHE VERS LA CAMÉRA
            // -------------------------------------------------------------
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                float objectHeight = 1.5f;
                if (meshRenderer != null)
                    objectHeight = meshRenderer.bounds.size.y;

                float targetDistance;
                if (mainCam.orthographic)
                {
                    targetDistance = mainCam.nearClipPlane + 0.1f;
                }
                else
                {
                    float frustumHeightAtDistance = objectHeight * 2f;
                    targetDistance = frustumHeightAtDistance / (2f * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad));
                }

                Vector3 startPos = transform.position;
                Vector3 targetPos = mainCam.transform.position 
                                  + (mainCam.transform.forward * targetDistance) 
                                  + (mainCam.transform.up * verticalOffset);

                float elapsedTime = 0f;
                while (elapsedTime < approachDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsedTime / approachDuration);

                    transform.position = Vector3.Lerp(startPos, targetPos, t);
                    transform.rotation = originalRotation;

                    yield return null;
                }

                transform.position = targetPos;
                transform.rotation = originalRotation;

                // -------------------------------------------------------------
                // 3. PAUSE DE 1.5s AVANT DE DÉCLENCHER IsArrived
                // -------------------------------------------------------------
                Debug.Log($"[Fantome] Arrivé devant la caméra. Pause de {delayBeforeArrived}s avant 'IsArrived'...", this);
                yield return new WaitForSeconds(delayBeforeArrived);

                // -------------------------------------------------------------
                // 4. DÉCLENCHEMENT DE IsArrived ET ATTENTE DE LA FIN
                // -------------------------------------------------------------
                if (animator != null && !string.IsNullOrEmpty(arrivedTrigger))
                {
                    Debug.Log($"[Fantome] Lancement de '{arrivedTrigger}'", animator.gameObject);
                    animator.speed = arrivedAnimSpeed;
                    animator.ResetTrigger(arrivedTrigger);
                    animator.SetTrigger(arrivedTrigger);

                    // Attente de 2 frames pour la prise en compte du changement d'état
                    yield return null;
                    yield return null;

                    // Récupération de la durée de l'animation IsArrived
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    float duration = stateInfo.length;

                    Debug.Log($"[Fantome] Lecture de '{arrivedTrigger}' (Durée : {duration}s)...", this);

                    // Attente de la fin exacte du clip d'animation
                    float timer = 0f;
                    while (timer < duration)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }

            // -------------------------------------------------------------
            // 5. FIN DE IsArrived -> ENVOL ET ASCENSION
            // -------------------------------------------------------------
            Debug.Log("[Fantome] Animation terminée, début de l'envol en zigzag.", this);
            Vector3 basePosition = transform.position;
            Vector3 flyDirectionRight = mainCam != null ? mainCam.transform.right : transform.right;

            float flyTime = 0f;

            while (true)
            {
                flyTime += Time.deltaTime;

                float speedPercent = Mathf.Clamp01(flyTime / accelerationDuration);
                float currentSpeed = Mathf.Lerp(0f, maxFlySpeed, speedPercent * speedPercent);

                basePosition += Vector3.up * (currentSpeed * Time.deltaTime);

                float horizontalOffset = Mathf.Sin(flyTime * zigzagFrequency) * zigzagAmplitude;

                transform.position = basePosition + (flyDirectionRight * horizontalOffset);
                transform.rotation = originalRotation;

                yield return null;
            }
        }
    }
}