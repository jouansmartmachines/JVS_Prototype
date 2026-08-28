using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Fait défiler la texture du sol pour suivre le scroll des structures.
    /// </summary>
    public class Demolition_GroundScroll : MonoBehaviour
    {
        public System.Func<float> scrollSpeedRef;
        private SpriteRenderer sr;
        private Vector2 offset;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (sr == null || scrollSpeedRef == null) return;
            float speed = scrollSpeedRef();
            offset.x += speed * Time.deltaTime * 0.5f;
            sr.material.mainTextureOffset = offset;
        }
    }
}