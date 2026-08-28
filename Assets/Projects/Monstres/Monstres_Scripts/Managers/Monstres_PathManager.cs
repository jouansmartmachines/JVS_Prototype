using System.Collections.Generic;
using UnityEngine;

namespace Monstres
{
    public class Monstres_PathManager : MonoBehaviour
    {
        public List<Transform> pathPoints;

        public float targetTravelTime = 1f;
        private float startTargetTravelTime;
        public float accelerationTime = 60f;
        public float minTraveTimeRatio = 2.5f;
        private float targetMinTravelTime;
        private float pathTargetCurrentSpeed = 1f;
        private bool isSpeedingUp = false;

        public float spawnIntervalTimeRatio = 3f;
        private float spawnInterval;
        private float currentSpawnTimeInterval;
        private bool haveSpawnHuman = false;


        public int pathGraphLayer = 1;
        public float rockXInterval = 3.0f;
        public float rockYOffset = 2f;
        public GameObject rockPrefab;
        public List<Sprite> rockList;
        public float scaleRatio = 1f;



        public Transform rockHolder;
        public bool flipSprite = true;

        public List<ScriptableObject_SpawnableTarget> targetsSpawnables;
        public GameObject humanPrefab;
        public int humanAppPercent = 40;

        private List<Script_Target> targetsInst = new List<Script_Target>(); // Register all target spawn by this path
        private List<Script_Target> monstersSpawn = new List<Script_Target>(); // Register all target spawn by this path

        private float accelerationStartTime;
        private bool isStop = false;
        private List<GameObject> _targets = new List<GameObject> ();
        private void Start()
        {
            humanAppPercent = (int)PlayerPrefs.GetFloat("PlayerAppPerc"); //setup save data
            targetTravelTime /= (PlayerPrefs.GetFloat("SpeedRatio") * 2f * 1.15f);
            SetupSpeedAndSpawnInt();
            targetMinTravelTime = targetTravelTime / minTraveTimeRatio;
            SpawnRockOnPath();
            SpawnATarget();
        }

        private void Update()
        {
            IsAtMaxSpeed();
            if (Monstres_GameManager.Instance.GetGameIsRunning() && !isStop)
            {
                if (currentSpawnTimeInterval > 0)
                {
                    currentSpawnTimeInterval -= Time.deltaTime;
                }
                else
                {
                    SpawnATarget();
                }

                if (isSpeedingUp)
                {
                    SpeedUp();
                }
            }
        }

        public int CheckTargets() 
        {
            for(int i = 0; i < _targets.Count; i++) 
            {
                if (_targets[i] == null)
                {
                    _targets.RemoveAt(i);
                    i--;
                }
            }
            return _targets.Count;
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

                    //newRock.GetComponent<SpriteRenderer>().sprite = tempRockSprites[rdmIdx];
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

        void SpawnATarget()
        {
            int rdmChance = Random.Range(0, 101);
            GameObject instantiateTarget;

            if (rdmChance <= humanAppPercent && !haveSpawnHuman)
            {
                //human spawn
                instantiateTarget = Instantiate(humanPrefab, pathPoints[0].transform.position, Quaternion.identity);
                haveSpawnHuman = true;

            }
            else
            {
                //monster spawn
                int rdmTarget = Random.Range(0, targetsSpawnables.Count);
                instantiateTarget = Instantiate(targetsSpawnables[rdmTarget].targetPrefab, pathPoints[0].transform.position, Quaternion.identity);

                if (flipSprite)
                {
                    //instantiateTarget.GetComponentInChildren<SpriteRenderer>().flipX = true;
                    //if (instantiateTarget.GetComponent<Script_Target>().flipedSprite != null)
                    //{
                    //    instantiateTarget.GetComponentInChildren<SpriteRenderer>().sprite = instantiateTarget.GetComponent<Script_Target>().flipedSprite;
                    //}
                }
                _targets.Add(instantiateTarget);
                haveSpawnHuman = false;
                monstersSpawn.Add(instantiateTarget.GetComponent<Script_Target>());

            }

            instantiateTarget.GetComponent<Script_Target>().SetupBaseVariables(this, pathTargetCurrentSpeed, pathGraphLayer);
            targetsInst.Add(instantiateTarget.GetComponent<Script_Target>());
            Monstres_GameManager.Instance.targetsOnScene.Add(instantiateTarget.GetComponent<Script_Target>());
            currentSpawnTimeInterval = spawnInterval;
        }

        public void DeleteTargetInst(Script_Target toDelete)
        {
            Monstres_GameManager.Instance.targetsOnScene.Remove(toDelete);
            targetsInst.Remove(toDelete);
        }

        public void DeleteMonsterInst(Script_Target toDelete)
        {
            monstersSpawn.Remove(toDelete);
        }

        void SetupSpeedAndSpawnInt()
        {
            pathTargetCurrentSpeed = Vector3.Distance(pathPoints[0].transform.position, pathPoints[pathPoints.Count - 1].transform.position) / targetTravelTime;
            spawnInterval = targetTravelTime / spawnIntervalTimeRatio;

            foreach (Script_Target targets in targetsInst)
            {
                targets.UpdateSpeed(pathTargetCurrentSpeed);
            }
        }

        private void SpeedUp()
        {
            float t = (Time.deltaTime - accelerationStartTime) / accelerationTime; //Percent of completion
            float newSpeed = Mathf.SmoothStep(startTargetTravelTime, targetMinTravelTime, t);
            targetTravelTime = newSpeed;

            SetupSpeedAndSpawnInt();

            if (targetTravelTime <= targetMinTravelTime)
            {
                isSpeedingUp = false;
            }
        }

        public void SpeedUpNoSmooth()
        {
            if (!isSpeedingUp && targetTravelTime > targetMinTravelTime)
            {
                targetTravelTime -= targetTravelTime / 3f;
                accelerationTime -= accelerationTime / 10f;
                SetupSpeedAndSpawnInt();
            }

        }
        public void SpeedUpNoSmoothSlower()
        {
            if ( targetTravelTime > targetMinTravelTime)
            {
                targetTravelTime -= targetTravelTime / 5f;
                
                accelerationTime -= accelerationTime / 12f;
                Debug.Log(targetTravelTime + " " + accelerationTime);
                SetupSpeedAndSpawnInt();
            }

        }

        public void StartSpeedUp()
        {
            isSpeedingUp = true;
            startTargetTravelTime = targetTravelTime;
            accelerationStartTime = Time.deltaTime;
        }

        public void SetAccelerationTime(float newTime)
        {
            accelerationTime = newTime;
        }

        public bool IsAtMaxSpeed()
        {
            if (targetTravelTime <= targetMinTravelTime)
            {
                Debug.Log("maxSpeed");
                return true;
            }

            return false;
        }

        public void Stop()
        {
            isStop = true;

            foreach (Script_Target targets in targetsInst)
            {
                targets.StopMove();
            }
        }

        public void Resume()
        {
            isStop = false;

            foreach (Script_Target targets in targetsInst)
            {
                targets.ResumeMove();
            }
        }

        public bool CheckMonstersInPath()
        {
            if (monstersSpawn.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}