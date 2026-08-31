using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Défilement infini et fluide du sol.
    /// </summary>
    public class Demolition_GroundScroll : MonoBehaviour
    {
        public System.Func<float> scrollSpeedRef;
        private SpriteRenderer sr;
        private Vector2 offset = Vector2.zero;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (sr == null || scrollSpeedRef == null) return;

            float speed = scrollSpeedRef();
            offset.x += speed * Time.deltaTime * 0.25f;

            if (sr.material != null)
            {
                sr.material.mainTextureOffset = offset;
            }
        }
    }
}
