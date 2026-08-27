using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Dobble
{
    public class Dobble_Circles : MonoBehaviour
    {
        [Header("Circle settings")]
        public int circleCount = 20; 

        private struct Circle
        {
            public Vector2 pos;
            public float r;
            public RectTransform rt;
        }

        private List<Circle> placed = new List<Circle>();
        private System.Random rng = new System.Random();

        /*
        public void GenerateCircles(RectTransform container, int minRadius, int maxRadius, int circleCount)
        {
            placed.Clear();

            float halfW = container.rect.width / 2f;
            float halfH = container.rect.height / 2f;
            float firstR = RandomRange(minRadius, maxRadius);
            PlaceCircle(Vector2.zero, firstR, container);

            bool added = true;

            while (added)
            {
                added = false;
                List<Circle> current = new List<Circle>(placed);

                foreach (var existing in current)
                {
                    float r = RandomRange(minRadius, maxRadius);

                    for (int k = 0; k < 36; k++) // 10° steps
                    {
                        float angle = k * 10f * Mathf.Deg2Rad;
                        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        Vector2 pos = existing.pos + dir * (existing.r + r);

                        if (Mathf.Abs(pos.x) + r > halfW || Mathf.Abs(pos.y) + r > halfH)
                            continue;

                        bool overlap = false;
                        foreach (var other in placed)
                        {
                            if (Vector2.Distance(pos, other.pos) < r + other.r - 0.1f)
                            {
                                overlap = true;
                                break;
                            }
                        }

                        if (!overlap)
                        {
                            PlaceCircle(pos, r, container);
                            added = true;
                            break;
                        }
                    }
                }
            }
            Vector2 center = Vector2.zero;
            placed = placed.OrderBy(c => Vector2.SqrMagnitude(c.pos - center)).ToList();

            for (int i = circleCount; i < placed.Count; i++)
            {
                if (placed[i].rt != null)
                    DestroyImmediate(placed[i].rt.gameObject);
            }

            placed = placed.Take(Mathf.Min(circleCount, placed.Count)).ToList();
        }
        */

        public void GenerateCircles(RectTransform container, float minRadius, float maxRadius, int circleCount, float angleOffsetMax = 360f)
        {
            placed.Clear();

            float halfW = container.rect.width / 2f;
            float halfH = container.rect.height / 2f;
            float shrinkfactor =  Dobble_GameManager.i.shrinkFactor;
            minRadius *= shrinkfactor;
            maxRadius *= shrinkfactor;
            float centerR = RandomRange(minRadius, maxRadius);
            PlaceCircle(Vector2.zero, centerR, container);
            System.Random rng = new System.Random();
            int roll = rng.Next(0, 3);

            bool circleSelected = false;
            if (circleCount < 5 && roll != 0)
            {
                circleSelected = true;
                circleCount++;
            }
            int peripheralCount = circleCount - 1;
            float angleStep = 360f / peripheralCount;
            float globalOffset = (float)(rng.NextDouble() * angleOffsetMax);
            List<Circle> current = new List<Circle>(placed);
            for (int i = 0; i < peripheralCount; i++)
            {
                float angleRad = (i * angleStep + globalOffset) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                Circle? closest = current
                    .OrderBy(c => Vector2.Dot(c.pos, dir))
                    .FirstOrDefault();

                if (closest == null) continue;

                float r = RandomRange(minRadius, maxRadius);
                float enlargeFactor = Mathf.Pow(1f / shrinkfactor, 1.5f);
                Vector2 pos = closest.Value.pos  + dir * (closest.Value.r + r) * enlargeFactor;


                PlaceCircle(pos, r, container);
                if (placed.Count >= circleCount) break;
                
            }
            Vector2 center = Vector2.zero;
            placed = placed.OrderBy(c => Vector2.SqrMagnitude(c.pos - center))
                        .Take(circleCount)
                        .ToList();

            if (circleSelected) 
            {
                if (placed[0].rt != null)
                    DestroyImmediate(placed[0].rt.gameObject);
                placed.RemoveAt(0);
            }
        }






        public List<RectTransform> GetCircleRects()
        {
            return placed.Select(c => c.rt).ToList();
        }

        public List<(Vector2 pos, float radius)> GetCircleData()
        {
            return placed.Select(c => (c.pos, c.r)).ToList();
        }

        private void PlaceCircle(Vector2 pos, float r, RectTransform container)
        {
            // 🔹 Ancienne version avec prefab (désactivée)
            // var rt = Instantiate(circlePrefab, container);
            // rt.sizeDelta = new Vector2(2 * r, 2 * r);

            GameObject go = new GameObject("CircleSlot", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(container, false);

            rt.sizeDelta = new Vector2(2 * r, 2 * r); 
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;

            placed.Add(new Circle { pos = pos, r = r, rt = rt });
        }

        private float RandomRange(float min, float max)
        {
            return (float)(min + (max - min) * rng.NextDouble());
        }
    }
}

