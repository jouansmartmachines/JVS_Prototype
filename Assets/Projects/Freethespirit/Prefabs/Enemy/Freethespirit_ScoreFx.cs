using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreFX : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    [Header("Animation")]
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [SerializeField] private float startScale = 0.4f;
    [SerializeField] private float popScale = 1.2f;

    [SerializeField] private float moveUp = 0.6f;

    private Vector3 baseScale;
    private Vector3 startPosition;

    void Awake()
    {
        if (scoreText == null)
            scoreText = GetComponentInChildren<TMP_Text>();

        baseScale = transform.localScale;
        startPosition = transform.position;
    }

    void OnEnable()
    {
        StartCoroutine(Animate());
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "+" + score;
    }

    private IEnumerator Animate()
    {
        if (scoreText == null)
            yield break;

        Color baseColor = scoreText.color;

        // Invisible + petit au départ
        scoreText.color = new Color(
            baseColor.r,
            baseColor.g,
            baseColor.b,
            0f
        );

        transform.localScale = baseScale * startScale;

        float time = 0f;

        // ───────────────
        // FADE IN + POP
        // ───────────────

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / fadeInDuration);

            // Ease Out
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            scoreText.color = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                t
            );

            transform.localScale =
                baseScale * Mathf.Lerp(startScale, popScale, ease);

            yield return null;
        }

        // Petit retour après le pop
        time = 0f;

        while (time < 0.12f)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / 0.12f);

            transform.localScale =
                baseScale * Mathf.Lerp(popScale, 1f, t);

            yield return null;
        }

        transform.localScale = baseScale;

        // ───────────────
        // RESTE + MONTE
        // ───────────────

        float holdDuration =
            Mathf.Max(0f, duration - fadeInDuration - fadeOutDuration - 0.12f);

        time = 0f;

        while (time < holdDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / Mathf.Max(holdDuration, 0.001f));

            transform.position =
                startPosition + Vector3.up * Mathf.Lerp(0f, moveUp * 0.65f, t);

            yield return null;
        }

        // ───────────────
        // FADE OUT
        // ───────────────

        Vector3 fadeStartPosition = transform.position;

        time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / fadeOutDuration);

            // Continue légèrement à monter
            transform.position =
                fadeStartPosition + Vector3.up * (moveUp * 0.35f * t);

            // Disparaît
            scoreText.color = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                1f - t
            );

            // Rétrécit légèrement
            transform.localScale =
                baseScale * Mathf.Lerp(1f, 0.8f, t);

            yield return null;
        }

        scoreText.color = new Color(
            baseColor.r,
            baseColor.g,
            baseColor.b,
            0f
        );
    }
}