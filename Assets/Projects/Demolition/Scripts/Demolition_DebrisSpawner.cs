using UnityEngine;
using System.Collections;

namespace Demolition
{
    /// <summary>
    /// Spawner de débris physiques, étincelles d'impact, poussière et étoiles de victoire (Juice).
    /// </summary>
    public class Demolition_DebrisSpawner : MonoBehaviour
    {
        public static void SpawnDebris(Vector3 position, Demolition_Block.MaterialType material, int count = 5)
        {
            string texName = "debris_bois";
            Color tint = Color.white;
            switch (material)
            {
                case Demolition_Block.MaterialType.Verre:
                    texName = "debris_verre";
                    tint = new Color(0.8f, 0.95f, 1f, 0.9f);
                    break;
                case Demolition_Block.MaterialType.Pierre:
                    texName = "debris_pierre";
                    tint = new Color(0.7f, 0.7f, 0.7f, 1f);
                    break;
                case Demolition_Block.MaterialType.Cochon:
                    texName = "debris_cochon";
                    tint = new Color(1f, 0.7f, 0.75f, 1f);
                    break;
                default:
                    texName = "debris_bois";
                    tint = new Color(0.9f, 0.6f, 0.3f, 1f);
                    break;
            }

            Texture2D tex = Resources.Load<Texture2D>("Textures/" + texName);
            Sprite debrisSprite = null;
            if (tex != null)
            {
                debrisSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            for (int i = 0; i < count; i++)
            {
                GameObject piece = new GameObject("Debris_" + material + "_" + i);
                piece.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.3f);
                float scale = Random.Range(0.6f, 1.2f);
                piece.transform.localScale = Vector3.one * scale;

                var sr = piece.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 6;
                sr.color = tint;
                if (debrisSprite != null)
                {
                    sr.sprite = debrisSprite;
                }
                else
                {
                    var fallbackTex = Texture2D.whiteTexture;
                    sr.sprite = Sprite.Create(fallbackTex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
                }

                var rb = piece.AddComponent<Rigidbody2D>();
                rb.mass = 0.3f;
                rb.linearDamping = 0.5f;
                rb.angularDamping = 0.3f;
                rb.gravityScale = 2.5f;

                Vector2 dir = Random.insideUnitCircle.normalized;
                float force = Random.Range(3f, 8f);
                rb.AddForce(new Vector2(dir.x * force, Mathf.Abs(dir.y) * force + 1.5f), ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-20f, 20f));

                var col = piece.AddComponent<CircleCollider2D>();
                col.radius = 0.12f * scale;

                var fade = piece.AddComponent<Demolition_FadeOut>();
                fade.duration = Random.Range(1.0f, 1.8f);
            }
        }

        /// <summary>
        /// Pop d'étoiles dorées scintillantes quand un cochon est éliminé.
        /// </summary>
        public static void SpawnStarBurst(Vector3 position, int starCount = 8)
        {
            Texture2D tex = Resources.Load<Texture2D>("Textures/star_1");
            Sprite starSprite = null;
            if (tex != null)
            {
                starSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            for (int i = 0; i < starCount; i++)
            {
                GameObject star = new GameObject("StarBurst_" + i);
                star.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.2f);
                float sScale = Random.Range(0.6f, 1.1f);
                star.transform.localScale = Vector3.one * sScale;

                var sr = star.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 9;
                sr.color = new Color(1f, 0.9f, 0.2f, 1f);
                if (starSprite != null)
                {
                    sr.sprite = starSprite;
                }

                var rb = star.AddComponent<Rigidbody2D>();
                rb.mass = 0.1f;
                rb.linearDamping = 1.5f;
                rb.gravityScale = 0.8f;

                float angle = (i / (float)starCount) * Mathf.PI * 2f;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                rb.AddForce(dir * Random.Range(4f, 7f), ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-180f, 180f));

                var fade = star.AddComponent<Demolition_FadeOut>();
                fade.duration = Random.Range(0.8f, 1.4f);
            }
        }

        public static void SpawnDustCloud(Vector3 position, float radius = 0.8f)
        {
            Texture2D tex = Resources.Load<Texture2D>("Textures/impact");
            Sprite dustSprite = null;
            if (tex != null)
            {
                dustSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject dust = new GameObject("DustCloud");
                dust.transform.position = position + (Vector3)(Random.insideUnitCircle * radius);
                float dScale = Random.Range(0.4f, 0.9f);
                dust.transform.localScale = Vector3.one * dScale;
                dust.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

                var sr = dust.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 7;
                sr.color = new Color(0.9f, 0.9f, 0.9f, 0.4f);
                if (dustSprite != null)
                {
                    sr.sprite = dustSprite;
                }

                var rb = dust.AddComponent<Rigidbody2D>();
                rb.mass = 0.1f;
                rb.linearDamping = 3.5f;
                rb.gravityScale = -0.1f;
                rb.AddForce(new Vector2(Random.Range(-0.8f, 0.8f), Random.Range(0.4f, 1.2f)), ForceMode2D.Impulse);

                var fade = dust.AddComponent<Demolition_FadeOut>();
                fade.duration = Random.Range(0.5f, 0.9f);
            }
        }
    }

    /// <summary>
    /// Animation de fondu et réduction d'échelle pour les débris et particules.
    /// </summary>
    public class Demolition_FadeOut : MonoBehaviour
    {
        public float duration = 1.5f;
        private SpriteRenderer sr;
        private float timer;
        private Vector3 initialScale;
        private Color initialColor;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            initialScale = transform.localScale;
            if (sr != null)
                initialColor = sr.color;
        }

        void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (sr != null)
            {
                Color c = initialColor;
                c.a = Mathf.Lerp(initialColor.a, 0f, t);
                sr.color = c;
            }

            if (t > 0.5f)
            {
                float shrinkT = (t - 0.5f) / 0.5f;
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, shrinkT);
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
