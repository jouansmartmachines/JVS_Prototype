using UnityEngine;
using UnityEngine.UI;

namespace Monstres
{
    public class Script_Target : MonoBehaviour
    {
        public ScriptableObject_SpawnableTarget associateScriptable;
        public bool isPlayer = false;
        public Canvas playerImageCanvas;
        public Image playerImage;
        public Sprite flipedSprite;
        private Monstres_PathManager pathToFollow;
        private int pathPointIdx = 0;
        private float speed = 0f;

        private bool canMove = true;
        private Animator animatorComp;

        //Audio
        private AudioSource audioSourceComp;
        public bool hit;

        private void Start()
        {
            animatorComp = GetComponent<Animator>();
            audioSourceComp = GetComponent<AudioSource>();
            if (!isPlayer)
            {
                var child = transform.GetChild(0);
                if (child != null && child.TryGetComponent(out BoxCollider collider) && child.TryGetComponent(out SpriteRenderer renderer))
                {
                    var bound = renderer.bounds;
                    //collider.center = bound.center;
                    collider.size = bound.size / collider.transform.parent.localScale.y;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Monstres_GameManager.Instance.GetGameIsRunning() && canMove)
            {
                if (transform.position != pathToFollow.pathPoints[pathPointIdx].transform.position)
                {
                    transform.position = Vector3.MoveTowards(transform.position, pathToFollow.pathPoints[pathPointIdx].position, speed * Time.deltaTime);
                }
                else if (transform.position == pathToFollow.pathPoints[pathPointIdx].transform.position && pathPointIdx < pathToFollow.pathPoints.Count - 1)
                {
                    NextPathPoint();
                }
                else if (transform.position == pathToFollow.pathPoints[pathPointIdx].transform.position && pathPointIdx >= pathToFollow.pathPoints.Count - 1)
                {
                    Monstres_GameManager.Instance.targetsOnScene.Remove(this);
                    Destroy(gameObject);
                }
            }
        }

        public void SetupBaseVariables(Monstres_PathManager newPathToFollow, float newSpeed, int newLayer)
        {
            pathToFollow = newPathToFollow;
            speed = newSpeed;

            if (!isPlayer)
            {
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<SpriteRenderer>())
                    {
                        child.GetComponent<SpriteRenderer>().sortingOrder = newLayer;
                    }
                }
            }
            else
            {
                playerImageCanvas.sortingOrder = newLayer;
                playerImage.sprite = Monstres_GameManager.Instance.GetRandomPlayerSprite();
            }

        }

        void NextPathPoint()
        {
            pathPointIdx++;
        }

        public void StopMove()
        {
            canMove = false;
        }

        public void ResumeMove()
        {
            canMove = true;
        }

        public void Hit()
        {
            if (!isPlayer)
            {
                audioSourceComp.PlayOneShot(Monstres_GameManager.Instance.GetRandomImpactSound());
            }
            else
            {
                audioSourceComp.PlayOneShot(Monstres_GameManager.Instance.impactHumanSound);
            }

            hit = true;
            animatorComp.SetTrigger("Hit");
            GetComponentInChildren<Collider>().enabled = false;
            canMove = false;
            //play some effect here like animation or other stuff
            Monstres_GameManager.Instance.AddScore(associateScriptable.pointValue, associateScriptable.spriteColor, transform.position);

            pathToFollow.DeleteTargetInst(this);

            if (!isPlayer)
            {
                pathToFollow.DeleteMonsterInst(this);
            }
        }

        public void UpdateSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        public void DestroyTarget()
        {
            Destroy(gameObject);
        }
    }
}