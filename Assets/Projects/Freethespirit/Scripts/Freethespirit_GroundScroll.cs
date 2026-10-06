using UnityEngine;

namespace Freethespirit
{
    /// <summary>
    /// Défilement infini et fluide du sol.
    /// </summary>
    public class Freethespirit_GroundScroll : MonoBehaviour
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
            if (sr == null) return;

            float speed = 0.5f;
            if (scrollSpeedRef != null)
            {
                speed = scrollSpeedRef();
            }
            else if (Freethespirit_GameManager.Instance != null)
            {
                speed = Freethespirit_GameManager.Instance.currentScrollSpeed;
            }

            offset.x += speed * Time.deltaTime * 0.25f;

            if (sr.material != null)
            {
                sr.material.mainTextureOffset = offset;
            }
        }
    }
}
