using UnityEngine;
using System.Collections;

namespace Demolition
{
    /// <summary>
    /// Garde qui patrouille autour du fantôme dans le plan de la caméra.
    /// Universal_Button + bound -> OnTouched().
    /// Composants manuels sur le prefab : BoxCollider + Rigidbody + Universal_Button + Demolition_Guard.
    /// </summary>
    public class Demolition_Guard : MonoBehaviour
    {
        [Header("Patrouille")]
        public float orbitRadius = 2f;
        public float orbitSpeed = 0.5f;
        public float idleBobAmplitude = 0.1f;
        public float idleBobFrequency = 2f;

        [Header("Hit / Stun")]
        public int maxHits = 3;
        public float stunDuration = 3f;
        public float hitRecoilSpeed = 8f;

        private Transform guardCenter;       // le Transform du fantôme
        private int hitCount = 0;
        private bool isStunned = false;
        private float angle;                 // angle de départ (0°, 120°, 240°)
        private Vector3 startScale;

        /// <summary>
        /// Appelé par le GameManager après spawn.
        /// </summary>
        public void Initialize(Transform center, float startAngle)
        {
            guardCenter = center;
            angle = startAngle;
            startScale = transform.localScale;
        }

        void Update()
        {
            if (guardCenter == null) return;
            if (!isStunned)
                Patrol();
            IdleBob();
        }

        private void Patrol()
        {
            angle += orbitSpeed * Time.deltaTime * 60f;
            float rad = angle * Mathf.Deg2Rad;
            // Déplacement dans le plan XY (parallèle à la caméra ortho)
            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * orbitRadius,
                Mathf.Sin(rad) * orbitRadius * 0.7f,
                0
            );
            Vector3 target = guardCenter.position + offset;
            transform.position = Vector3.MoveTowards(
                transform.position, target, orbitSpeed * Time.deltaTime * 8f
            );
        }

        private void IdleBob()
        {
            float bob = Mathf.Sin(Time.time * idleBobFrequency) * idleBobAmplitude * Time.deltaTime;
            transform.position += Vector3.up * bob;
        }

        /// <summary>
        /// Appelé par Universal_Button.Event.AddListener().
        /// </summary>
        public void OnTouched()
        {
            if (isStunned) return;
            hitCount++;

            if (hitCount >= maxHits)
                StartCoroutine(StunRoutine());
            else
                StartCoroutine(HitRecoilRoutine());
        }

        private IEnumerator HitRecoilRoutine()
        {
            Vector3 dir = (transform.position - guardCenter.position).normalized;
            float elapsed = 0f;
            float dur = 0.12f;
            while (elapsed < dur)
            {
                transform.position += dir * (hitRecoilSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator StunRoutine()
        {
            isStunned = true;
            // Squash
            float t = 0f;
            while (t < 0.2f)
            {
                float s = Mathf.Lerp(1f, 0.3f, t / 0.2f);
                transform.localScale = new Vector3(startScale.x * s, startScale.y * s, startScale.z * s);
                t += Time.deltaTime;
                yield return null;
            }
            // Stun pause
            yield return new WaitForSeconds(stunDuration);
            // Restore scale
            t = 0f;
            while (t < 0.3f)
            {
                float s = Mathf.Lerp(0.3f, 1f, t / 0.3f);
                transform.localScale = new Vector3(startScale.x * s, startScale.y * s, startScale.z * s);
                t += Time.deltaTime;
                yield return null;
            }
            isStunned = false;
            hitCount = 0;
        }
    }
}