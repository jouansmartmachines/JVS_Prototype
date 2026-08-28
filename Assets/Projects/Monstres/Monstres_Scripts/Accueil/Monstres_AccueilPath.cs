using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monstres
{
    public class Monstres_AccueilPath : MonoBehaviour
    {
        public List<Transform> pathPoints;
        public int pathGraphLayer = 1;
        public float rockXInterval = 0.5f;
        public float rockYOffset = 2f;
        public GameObject rockPrefab;
        public List<Sprite> rockList;
        public Transform rockHolder;
        public float scaleRatio = 1f;

        public List<ScriptableObject_SpawnableTarget> targetsSpawnables;
        public bool flipSprite = false;

        public float targetTravelTime = 1f;
        private float pathTargetCurrentSpeed = 1f;

        public float spawnIntervalTimeRatio = 3f;
        private float spawnInterval;
        private float currentSpawnTimeInterval;

        // Start is called before the first frame update
        void Start()
        {
            SpawnRockOnPath();
            SetupSpeedAndSpawnInt();
            SpawnATarget();
        }

        // Update is called once per frame
        void Update()
        {
            if (currentSpawnTimeInterval > 0)
            {
                currentSpawnTimeInterval -= Time.deltaTime;
            }
            else
            {
                SpawnATarget();
            }
        }

        void SpawnRockOnPath()
        {
            List<Sprite> tempRockSprites = new List<Sprite>(rockList);
            Sprite lastRockTaken = null;

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                float pathDist = Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
                int rockNumber = Mathf.RoundToInt(pathDist / rockXInterval);
                Vector3 angle = (pathPoints[i + 1].transform.position - pathPoints[i].transform.position).normalized;

                for (int y = 0; y < rockNumber; y++)
                {
                    Vector3 nextRockPosition = pathPoints[i].transform.position + (angle * (y * rockXInterval));
                    nextRockPosition.y -= rockYOffset;
                    GameObject newRock = Instantiate(rockPrefab, nextRockPosition, Quaternion.identity, rockHolder);
                    newRock.name = gameObject.name + "Rock" + y + "Path" + gameObject.name + "Point" + i;

                    int rdmIdx = Random.Range(0, tempRockSprites.Count);

                    newRock.GetComponent<SpriteRenderer>().sprite = tempRockSprites[rdmIdx];
                    newRock.GetComponent<SpriteRenderer>().sortingOrder += (pathGraphLayer + 5);
                    newRock.GetComponent<SpriteRenderer>().flipX = (Random.Range(0, 2) == 1 ? true : false);
                    newRock.transform.localScale *= scaleRatio;

                    if (lastRockTaken != null)
                    {
                        tempRockSprites.Add(lastRockTaken);
                    }

                    lastRockTaken = tempRockSprites[rdmIdx];
                    tempRockSprites.RemoveAt(rdmIdx);
                }
            }
        }

        void SetupSpeedAndSpawnInt()
        {
            pathTargetCurrentSpeed = Vector3.Distance(pathPoints[0].transform.position, pathPoints[pathPoints.Count - 1].transform.position) / targetTravelTime;
            spawnInterval = targetTravelTime / spawnIntervalTimeRatio;
        }

        void SpawnATarget()
        {
            GameObject instantiateTarget;

            //monster spawn
            int rdmTarget = Random.Range(0, targetsSpawnables.Count);
            instantiateTarget = Instantiate(targetsSpawnables[rdmTarget].targetPrefab, pathPoints[0].transform.position, Quaternion.identity);

            if (flipSprite)
            {
                instantiateTarget.GetComponentInChildren<SpriteRenderer>().flipX = true;
            }


            instantiateTarget.GetComponent<Script_Target>().enabled = false;
            Monstres_AccueilTarget scriptTarget = instantiateTarget.AddComponent<Monstres_AccueilTarget>();
            scriptTarget.SetupBaseVariables(this, pathTargetCurrentSpeed, pathGraphLayer);
            currentSpawnTimeInterval = spawnInterval;
        }
    }
}
