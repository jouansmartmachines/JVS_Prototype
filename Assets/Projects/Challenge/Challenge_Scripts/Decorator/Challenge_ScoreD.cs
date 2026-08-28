using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

namespace Challenge
{
    public class Challenge_ScoreD : Challenge_TargetDecorator
    {
        [Header("Score Settings")]
        public int points = 10;
        public float multiplier = 1f;    
        public Challenge_ScoreManager scoreManager; 

        [Header("Point Decay Settings")]
        public float decayDelay = 10f;
        public float decayDuration = 30f;

        [Header("Popup Settings")]
        public Vector3 popupOffset = new Vector3(0, 1, 0);
        public float popupDuration = 2f;
        public float popupMoveSpeed = 1f;

        [Header("Second Popup (Simple Transform Fade)")]
        public GameObject secondPopup;
        
        private float timer = 0f;

        void Start()
        {
            if (target != null)
                target.OnDeath += OnTargetDeath;
        }

        void Update()
        {
            timer += Time.deltaTime;

            if (timer > decayDelay)
            {
                float t = Mathf.Clamp01((timer - decayDelay) / decayDuration);
                multiplier = 1f - 0.5f * t; // descend de 1 à 0.5
            }
        }

        private void OnTargetDeath(ITarget deadTarget, DeathCause cause)
        {
            if (deadTarget is Challenge_Target t && cause != DeathCause.Lifetime)
            {
                int finalPoints = multiplier > 0 ? Mathf.RoundToInt(points * multiplier) : -5;

                // --- SCORE PRINCIPAL ---
                scoreManager.AddScore(finalPoints);
                scoreManager.AddTime(t.timeAdded);

                // --- POPUP TEXTE (TMP) ---
                // On lance la coroutine sur le scoreManager pour qu'elle survive à la destruction du décorateur
                string textToShow = finalPoints > 0 ? $"+{finalPoints}" : $"{finalPoints}";
                scoreManager.StartCoroutine(ShowPopupTMP(textToShow, t.transform.position + popupOffset));

                // --- SECOND POPUP VISUEL (Image/Icone) ---
                if (secondPopup != null)
                {
                    GameObject popupInstance = Instantiate(secondPopup, scoreManager.popupParent.transform);
                    
                    // On cale la position initiale sur le transform local avant que l'objet ne disparaisse
                    popupInstance.transform.localPosition = gameObject.transform.localPosition; 

                    // LANCEMENT SUR LE MANAGER : Crucial pour que le nettoyage (Destroy) se produise
                    scoreManager.StartCoroutine(FadeSecondPopup(popupInstance));
                }
            }
        }

        // ================== COROUTINES DE GESTION DES POPUPS ==================
        // Note : Ces méthodes sont appelées via scoreManager.StartCoroutine(...)

        private IEnumerator ShowPopupTMP(string text, Vector3 worldPos)
        {
            TMP_Text tmp = Instantiate(scoreManager.scoreTMP, scoreManager.scoreTMP.transform.parent);
            tmp.transform.SetAsLastSibling();
            tmp.text = text;
            tmp.alpha = 1f;
            tmp.transform.position = Camera.main.WorldToScreenPoint(worldPos);

            float duration = popupDuration;
            float fadeStart = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (tmp == null) yield break; // Sécurité si l'objet est détruit autrement

                elapsed += Time.deltaTime;
                tmp.transform.position += Vector3.up * popupMoveSpeed * Time.deltaTime;

                if (elapsed < fadeStart)
                {
                    tmp.alpha = 1f;
                }
                else
                {
                    float fadeElapsed = elapsed - fadeStart;
                    float fadeTime = duration - fadeStart;
                    tmp.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeTime);
                }

                yield return null;
            }

            if (tmp != null) Destroy(tmp.gameObject);
        }

        private IEnumerator FadeSecondPopup(GameObject popup, Color? color = null, float fadeDelay = 0.5f, float initialHeightOffset = 100f)
        {
            if (popup == null) yield break;

            float elapsed = 0f;
            float duration = popupDuration;
            Transform tr = popup.transform;

            // ---------- POSITION ----------
            Vector3 startPos = tr.position + Vector3.up * initialHeightOffset;
            Vector3 endPos = startPos + Vector3.up * 100f;

            // ---------- SCALE ----------
            Vector3 startScale = tr.localScale;
            Vector3 punchScale = startScale * 2f; 
            tr.localScale = punchScale;

            // ---------- IMAGE & COULEUR ----------
            Image img = popup.GetComponent<Image>();
            Color startColor = img != null ? img.color : Color.white;
            if (color.HasValue && img != null) startColor = color.Value;
            if (img != null) img.color = startColor;

            // ---------- ANIMATION ----------
            while (elapsed < duration)
            {
                if (popup == null) yield break; // Sécurité si l'objet est détruit

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeT = Mathf.Sin(t * Mathf.PI * 0.5f); // ease-out

                tr.position = Vector3.Lerp(startPos, endPos, easeT);
                float scaleT = Mathf.Sin(easeT * Mathf.PI);
                tr.localScale = Vector3.Lerp(startScale, punchScale, 1f - scaleT);
                tr.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * 20f) * 15f);

                if (img != null)
                {
                    float fadeT = Mathf.Clamp01((elapsed - fadeDelay) / (duration - fadeDelay));
                    img.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, fadeT));
                }

                yield return null;
            }

            if (popup != null) Destroy(popup);
        }

        private void OnDestroy()
        {
            if (target != null)
                target.OnDeath -= OnTargetDeath;
        }
    }
}