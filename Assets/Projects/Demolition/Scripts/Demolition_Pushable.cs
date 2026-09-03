using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Applique une force d'impulsion 3D sur l'objet quand il est touché via Universal_Button.
    /// Bindé dans ObstacleSpawner via button.Event.AddListener(pushable.OnPushed).
    /// </summary>
    public class Demolition_Pushable : MonoBehaviour
    {
        [Header("Force")]
        public float pushForce = 8f;
        public float uplift = 4f;
        public float radiusVariation = 0.5f;

        private Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
        }

        /// <summary>
        /// Appelé par Universal_Button.Event quand le joueur touche l'objet.
        /// </summary>
        public void OnPushed()
        {
            if (rb == null) return;

            Vector3 camPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            Vector3 dir = (transform.position - camPos).normalized;
            Vector3 randomOffset = Random.insideUnitSphere * radiusVariation;

            rb.AddForceAtPosition(
                dir * pushForce + Vector3.up * uplift + randomOffset,
                transform.position + Random.insideUnitSphere * 0.3f,
                ForceMode.Impulse);
        }
    }
}