using UnityEngine;

namespace Monstres
{
    public class Monstres_Self_Destruct : MonoBehaviour
    {
        [SerializeField] private float timer;

        private void Start()
        {
            Destroy(gameObject, timer);
        }
    }
}