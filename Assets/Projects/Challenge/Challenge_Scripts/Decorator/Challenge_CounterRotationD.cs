using UnityEngine;

namespace Challenge
{
    public class Challenge_CounterRotationD : Challenge_TargetDecorator
    {
        private RectTransform _elementA;
        private RectTransform _elementB;

        private float _speedA;
        private float _speedB;
        private bool _isReady;

        /// <summary>
        /// Instancie le container et récupère ses deux premiers enfants pour la rotation.
        /// </summary>
        /// <param name="firstLayer">Le parent (FirstLayer).</param>
        /// <param name="containerPrefab">Le prefab qui contient déjà les deux cercles.</param>
        public void Setup(GameObject firstLayer, GameObject containerPrefab, float speedA, float speedB)
        {

            GameObject containerGO = Instantiate(containerPrefab, firstLayer.transform);
        
            containerGO.transform.localPosition = Vector3.zero;
            containerGO.transform.localScale = Vector3.one;

            if (containerGO.transform.childCount >= 2)
            {
                _elementA = containerGO.transform.GetChild(0) as RectTransform;
                _elementB = containerGO.transform.GetChild(1) as RectTransform;
            }


            // 3. Configuration
            _speedA = speedA;
            _speedB = -speedB;
            _isReady = true;
        }

        private void Update()
        {
            if (!_isReady || _elementA == null || _elementB == null) return;

            float dt = Time.deltaTime;
            _elementA.Rotate(0f, 0f, _speedA * dt);
            _elementB.Rotate(0f, 0f, _speedB * dt);
        }
    }
}