using UnityEngine;

namespace Challenge
{
    [System.Serializable]
    public class Challenge_LateralMovementStrategy : IMovementStrategy
    {
        [Header("Movement Bounds")]
        public float minX = -500f;
        public float maxX = 500f;
        
        [Header("Timing Settings")]
        public float moveDuration = 0.15f;
        public float pauseDuration = 3.0f;

        // Référence aux points de spawn possibles
        private Transform[] _spawnPoints;
        
        private float _timer;
        private float _currentStartX;
        private float _targetX;
        private bool _isMoving;

        // Constructeur mis à jour pour accepter les points de spawn
        public Challenge_LateralMovementStrategy(float startX, Transform[] spawnPoints = null)
        {
            _currentStartX = startX;
            _targetX = startX;
            _timer = pauseDuration;
            _spawnPoints = spawnPoints;
        }

        public void Move(Transform transform)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                if (!_isMoving)
                {
                    _isMoving = true;
                    _timer = moveDuration;
                    _currentStartX = transform.localPosition.x;

                    // LOGIQUE 1/3 : Cible un enfant vs Aléatoire
                    if (_spawnPoints != null && _spawnPoints.Length > 0 && Random.value < 0.33f)
                    {
                        // On choisit un enfant au hasard dans la spawnZone
                        int randomIndex = Random.Range(0, _spawnPoints.Length);
                        _targetX = _spawnPoints[randomIndex].localPosition.x;
                        Debug.Log($"Cible choisie : Enfant n°{randomIndex} à l'X : {_targetX}");
                    }
                    else
                    {
                        // Mouvement aléatoire classique dans les bornes
                        _targetX = Random.Range(minX, maxX);
                        Debug.Log($"Cible choisie : Aléatoire à l'X : {_targetX}");
                    }
                }
                else
                {
                    _isMoving = false;
                    _timer = pauseDuration;
                    transform.localPosition = new Vector3(_targetX, transform.localPosition.y, transform.localPosition.z);
                }
            }

            if (_isMoving)
            {
                float t = 1f - (_timer / moveDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                float newX = Mathf.Lerp(_currentStartX, _targetX, smoothT);
                transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
            }
        }

        public Vector2 GetMovementDelta() => Vector2.zero;
    }
}