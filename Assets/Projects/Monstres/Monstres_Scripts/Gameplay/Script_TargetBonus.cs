using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monstres
{
    public class Script_TargetBonus : MonoBehaviour
    {
        public bool hit = false;
        private bool isDying = false;
        public float monsterSpeed;
        [SerializeField] private SpriteRenderer monsterSprite;
        [SerializeField] private float extentsForOverlapBox;
        public Animator animatorComp;
        [SerializeField] private int spawnNumber;
        [SerializeField] private GameObject spawnling;

        private AudioSource audioSourceComp;
        private float oldPos;
        private bool death;
        private bool secondWave = false;
        private GameObject spawnParent;

        //private List<Script_Target> targets = new List<Script_Target>();
        //private List<Script_Target> openList = new List<Script_Target>();

        private void Awake()
        {
            animatorComp = GetComponent<Animator>();
            audioSourceComp = GetComponent<AudioSource>();
        }

        public void Hit()
        {
            if (!isDying)
            {
                hit = true;
                isDying = true;
                audioSourceComp.PlayOneShot(Monstres_GameManager.Instance.GetRandomImpactSound());
                Monstres_GameManager.Instance.AddScore(1000, Color.yellow, transform.position);
                GetComponentInChildren<Collider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;
                animatorComp.SetBool("hit", true);
                spawnParent = new GameObject();
                monsterSpeed = 0;
                //FillList();
                Spawn();
            }
        }

        private void Update()
        {
            if (hit)
            {
                Destroy(gameObject, .25f);

                oldPos = transform.position.x;

                if (death)
                {
                    return;
                }
                GetComponentInChildren<Collider>().enabled = false;
                death = true;
            }
        }

        private void Spawn()
        {
            for (int i = 0; i < spawnNumber; i++)
            {
                //Distance around the circle 
                var radians = 2 * Mathf.PI / spawnNumber * i;

                //Get the vector direction
                var vertical = Mathf.Sin(radians);
                var horizontal = Mathf.Cos(radians);

                var spawnDir = new Vector3(horizontal, vertical);

                var spawnPos = transform.position + spawnDir * 2f;

                GameObject emptyGo = new GameObject();
                emptyGo.transform.parent = spawnParent.transform;
                GameObject[] spawnPoints = new GameObject[spawnNumber];
                spawnPoints[i] = Instantiate(emptyGo, spawnPos, new Quaternion(spawnDir.x, spawnDir.y, 0, 0), spawnParent.transform);

                GameObject instSpawnling = Instantiate(spawnling, spawnPoints[i].transform.position, Quaternion.identity, spawnPoints[i].transform);
                SpawnlingScript spawnlingScript = instSpawnling.GetComponent<SpawnlingScript>();
                spawnlingScript.direction = spawnDir;
                instSpawnling.GetComponent<SpriteRenderer>().sprite = monsterSprite.sprite;
            }
        }
    }
}

//private void OnCollisionEnter(Collision collision)
//{
//    if (hit)
//    {
//        if (collision.collider.gameObject.GetComponentInParent<Script_Target>() != null)
//        {
//            collision.collider.gameObject.GetComponentInParent<Script_Target>().Hit();
//            if (targets.Exists(c => collision.collider.gameObject.GetComponentInParent<Script_Target>()))
//            {
//                targets.Remove(collision.collider.gameObject.GetComponentInParent<Script_Target>());
//            }
//            collateral--;
//        }
//    }
//}

//private void FillList()
//{
//    GetComponentInChildren<Collider>().enabled = true;
//    Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(extentsForOverlapBox, extentsForOverlapBox, extentsForOverlapBox));
//    foreach (var c in colliders)
//    {
//        if (c.gameObject.GetComponentInParent<Script_Target>() != null)
//        {
//            openList.Add(c.gameObject.GetComponentInParent<Script_Target>());
//        }
//    }

//    int neededToGetCollateral = 0;
//    IOrderedEnumerable<Script_Target> orderedOpen = null;
//    foreach (var item in openList)
//    {
//        if (item != null)
//        {
//            orderedOpen = openList.OrderBy(c => Vector2.Distance(transform.position, item.transform.position));
//        }
//    }

//    foreach (var item in orderedOpen)
//    {
//        targets.Add(item);
//        neededToGetCollateral++;
//        if (neededToGetCollateral >= collateral)
//        {
//            break;
//        }
//    }
//}
//private void OnDrawGizmos()
//{
//    Gizmos.color = Color.red;
//    Gizmos.DrawWireCube(transform.position, new Vector3(extentsForOverlapBox, extentsForOverlapBox, extentsForOverlapBox));
//}