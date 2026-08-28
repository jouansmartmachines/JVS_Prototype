using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Fait exploser un bloc en 4-8 morceaux physiques avec textures aleatoires
    /// </summary>
    public class Demolition_DebrisSpawner : MonoBehaviour
    {
        public static void SpawnDebris(Vector3 position, Demolition_Block.MaterialType material, int count = 6)
        {
            string texName = "debris_bois";
            Color tint = Color.white;
            switch (material)
            {
                case Demolition_Block.MaterialType.Verre: texName = "debris_verre"; tint = new Color(0.7f, 0.85f, 0.9f); break;
                case Demolition_Block.MaterialType.Pierre: texName = "debris_pierre"; tint = Color.gray; break;
                case Demolition_Block.MaterialType.Cochon: texName = "debris_cochon"; tint = new Color(1, 0.7f, 0.7f); break;
            }

            Texture2D tex = Resources.Load<Texture2D>("Textures/" + texName);

            for (int i = 0; i < count; i++)
            {
                GameObject piece = new GameObject("Debris_" + i, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
                piece.transform.position = position + Random.insideUnitSphere * 0.3f;
                piece.transform.localScale = Vector3.one * Random.Range(0.2f, 0.5f);
                piece.layer = 0; // Default

                var sr = piece.GetComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
                if (tex != null)
                {
                    sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                sr.color = tint;

                var rb = piece.GetComponent<Rigidbody2D>();
                rb.mass = 0.3f;
                rb.drag = 0.5f;
                rb.angularDrag = 0.3f;
                rb.gravityScale = 1.5f;
                rb.AddForce(new Vector2(Random.Range(-5f, 5f), Random.Range(2f, 8f)), ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-10f, 10f));

                var col = piece.GetComponent<BoxCollider2D>();
                col.size = new Vector2(0.3f, 0.3f);

                Object.Destroy(piece, Random.Range(2f, 4f));
            }
        }

        public static void SpawnDustCloud(Vector3 position, float radius = 1.5f)
        {
            Texture2D tex = Resources.Load<Texture2D>("Textures/impact");
            if (tex == null) return;

            for (int i = 0; i < 5; i++)
            {
                GameObject dust = new GameObject("Dust", typeof(SpriteRenderer));
                dust.transform.position = position + new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0);
                dust.transform.localScale = Vector3.one * Random.Range(0.2f, 0.6f);
                dust.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

                var sr = dust.GetComponent<SpriteRenderer>();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                sr.color = new Color(0.8f, 0.8f, 0.8f, 0.6f);
                sr.sortingOrder = 6;

                var rb = dust.AddComponent<Rigidbody2D>();
                rb.mass = 0.1f;
                rb.drag = 2f;
                rb.gravityScale = -0.5f; // monte
                rb.AddForce(new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f)), ForceMode2D.Impulse);

                // Fade out puis destroy
                var fade = dust.AddComponent<Demolition_FadeOut>();
                fade.duration = Random.Range(0.5f, 1.2f);
            }
        }
    }

    public class Demolition_FadeOut : MonoBehaviour
    {
        public float duration = 1f;
        private SpriteRenderer sr;
        private float timer;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(0.6f, 0f, t);
                sr.color = c;
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }
}