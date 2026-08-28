using UnityEngine;
using DG.Tweening;

namespace Monstres
{
    public class SpawnlingScript : MonoBehaviour
    {
        [HideInInspector] public Vector3 direction;
        [SerializeField] private float speed;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            int rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                animator.SetBool("reverse", true);
            }
            else
            {
                animator.SetBool("reverse", false);
            }

            transform.DOScale(.5f, .1f);
            GameObject parent = transform.parent.gameObject;
            Destroy(parent, 2.5f);
        }
    }
}