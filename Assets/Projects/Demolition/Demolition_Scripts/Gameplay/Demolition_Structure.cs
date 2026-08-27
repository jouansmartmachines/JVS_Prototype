using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Gère la création d'une structure (assemblage de blocs)
    /// et son auto-destruction si elle est trop loin ou instable.
    /// </summary>
    public class Demolition_Structure : MonoBehaviour
    {
        [Header("Configuration")]
        public Transform[] anchorPoints;
        public Demolition_Block[] blocs;
        public float destructionDelay = 3f;

        private bool isCleared = false;

        void Start()
        {
            // Connecter les blocs entre eux avec des joints
            for (int i = 0; i < blocs.Length - 1; i++)
            {
                if (blocs[i] != null && blocs[i + 1] != null)
                {
                    FixedJoint2D joint = blocs[i].gameObject.AddComponent<FixedJoint2D>();
                    joint.connectedBody = blocs[i + 1].GetComponent<Rigidbody2D>();
                    float breakForce = Mathf.Min(
                        blocs[i].GetBreakForce(),
                        blocs[i + 1].GetBreakForce()
                    );
                    joint.breakForce = breakForce;
                    joint.breakTorque = breakForce;
                }
            }
        }

        void Update()
        {
            // Auto-destruction si trop loin à gauche
            if (transform.position.x < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x - 10f)
            {
                Destroy(gameObject);
            }
        }

        public bool IsCleared()
        {
            foreach (var bloc in blocs)
            {
                if (bloc != null) return false;
            }
            return true;
        }
    }
}