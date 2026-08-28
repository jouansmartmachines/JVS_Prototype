using UnityEngine;
using System.Collections.Generic;

namespace Challenge
{
    public class PathFollowStrategy : IMovementStrategy
    {
        private readonly List<Vector2> _points;
        private int _currentIndex = 0;
        private readonly RectTransform _rectTransform;

        [Header("Movement")]
        public float speed = 100f;
        public float arriveDistance = 5f;
        public float steeringSmoothing = 0.15f;

        [Header("Avoidance")]
        public bool enableAvoidance = true;
        public float avoidanceRadius = 50f;
        public float avoidanceForce = 2f;
        public List<RectTransform> otherAgents;

        private Vector2 _velocity;

        public PathFollowStrategy(Transform transform, List<Vector2> pathPoints)
        {
            _rectTransform = transform as RectTransform;
            _points = pathPoints;
        }

         public void Move(Transform transform)
        {
            // Simple translation basée sur le delta
            transform.Translate((Vector3)GetMovementDelta() * speed * Time.deltaTime);
        }

        public Vector2 GetMovementDelta()
        {
            if (_rectTransform == null || _points == null || _points.Count == 0)
                return Vector2.zero;

            Vector2 currentPos = _rectTransform.anchoredPosition;
            Vector2 targetPoint = _points[_currentIndex];

            // ---- 1) Steering vers le point cible ----
            Vector2 toTarget = targetPoint - currentPos;
            float distance = toTarget.magnitude;

            // On passe au point suivant si on est assez proche
            if (distance <= arriveDistance)
            {
                _currentIndex++;

                if (_currentIndex >= _points.Count)
                    _currentIndex = _points.Count - 1; // ne dépasse pas
            }

            Vector2 desired = toTarget.normalized * speed;

            // ---- 2) Ajout d’un comportement d’évitement (optionnel) ----
            if (enableAvoidance && otherAgents != null)
            {
                Vector2 avoidance = ComputeAvoidance(currentPos);
                desired += avoidance * avoidanceForce;
            }

            // ---- 3) Lissage du mouvement ----
            _velocity = Vector2.Lerp(_velocity, desired, steeringSmoothing);

            return _velocity * Time.deltaTime;
        }

        // ***************************************************************
        // Calcule une force d’évitement pour ne pas toucher d’autres agents
        // ***************************************************************
        private Vector2 ComputeAvoidance(Vector2 pos)
        {
            Vector2 force = Vector2.zero;

            foreach (var agent in otherAgents)
            {
                if (agent == null || agent == _rectTransform)
                    continue;

                Vector2 otherPos = agent.anchoredPosition;
                float dist = Vector2.Distance(pos, otherPos);

                if (dist < avoidanceRadius)
                {
                    // S’éloigne proportionnellement à la distance
                    force += (pos - otherPos).normalized * (1f - dist / avoidanceRadius);
                }
            }

            return force;
        }
    }
}
