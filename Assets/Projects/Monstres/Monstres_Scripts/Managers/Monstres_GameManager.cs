using MenuSelection;
using OSC;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monstres
{
    public class Monstres_GameManager : ReceiveParent
    {
        public static Monstres_GameManager Instance { get; private set; }

        [Header("Paths")]
        public List<Monstres_PathManager> pathsList;

        public List<Script_Target> targetsOnScene = new List<Script_Target>();

        [Header("Graphics")]
        public List<GameObject> littleRocks;
        public bool IsThemeScaleActive;
        public float scaleResizer;
        public float scalelimit;

        public int littleRockToSpawn = 20;
        public GameObject scoreFeedback;
        public Transform littleRockHolder;
        public Sprite noPlayerSprite;

        [Header("Speed Up")]
        public int scoreToSpeedUp = 3000;
        private bool gameIsSpeedingUp = false;

        [Header("Gameplay")]
        public float gameDuration = 60f;
        public float currentGameDuration;
        private bool gameIsRunning = true;
        public float timeBeforeLeaderboard = 5f;
        [SerializeField] public ScriptableObjectValue score;
        public List<DifficultyOptions> difficultiesValues;

        [Header("Stop")]
        [Range(0, 100)]
        public int chanceToStop = 50;
        public float minTimeBetweenStop = 10f;
        public Vector2 timeToResumeInterval;
        private float timeToResume = 0f;
        private bool isStop = false;
        private bool stopIsPossible = false;

        [Header("Sounds")]
        public List<AudioClip> impactSound;
        public AudioClip impactHumanSound;
        public AudioClip gameOverSound;
        public AudioClip stopSound;
        private AudioSource audioSourceComp;
        private AudioClip lastImpactSoundPlayed;

        //caracteristiques d'un impact
        private bool gotAPt;
        private Vector3 newPt;
        private int w, h;
        public GameObject impactPrefab;

        private int lastPlayerSpawn = -1; // represent the last player spawn (0 = player 1 ; 1 = player 2). Use to flip flop player spawn. 

        private List<Vector3> littleRockPosition = new List<Vector3>();
        private bool _countDown;
        [SerializeField]
        private TextMeshProUGUI _countDownText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            w = Screen.width;
            h = Screen.height;

            score.value = 0;

            audioSourceComp = GetComponent<AudioSource>();
            gameDuration = PlayerPrefs.GetFloat("GameDuration");
            SetupDifficulty();
            currentGameDuration = gameDuration;
            Monstres_UIManager.Instance.UpdateScore((int)score.value);

            
            OSC_Manager.Instance.receiveP = this;

            SpawnLittleRocks();
        }

        private void Update()
        {
            if (gameIsRunning)
            {
                //pour utliser le jeu avec une souris
                if (Input.GetButtonDown("Fire1"))
                {
                    newPt.x = (float)Input.mousePosition.x;
                    newPt.y = (float)Input.mousePosition.y;
                    gotAPt = true;
                }

                if (currentGameDuration > 0)
                {
                    if (currentGameDuration <= 11 && !_countDown)
                    {
                        _countDown = true;
                        StartCoroutine(CountDown());
                    }

                    currentGameDuration -= Time.deltaTime;
                    Monstres_UIManager.Instance.UpdateTimer(currentGameDuration);
                    Monstres_UIManager.Instance.timerBarPivot.rotation = Quaternion.Euler(0, 0, (1 - (currentGameDuration - Time.deltaTime) / gameDuration) * -360f);
                    Monstres_UIManager.Instance.timer.fillAmount = (currentGameDuration - Time.deltaTime) / gameDuration;

                    if (gameIsSpeedingUp && !stopIsPossible)
                    {
                        stopIsPossible = true;
                        StartCoroutine(WaitToRollStop());
                    }

                    if (isStop)
                    {
                        timeToResume -= Time.deltaTime;

                        if (timeToResume <= 0)
                        {
                            ResumeMovement();
                        }
                    }
                }
                else
                {
                    EndGame();
                }

                if (gotAPt)
                {
                    gotAPt = false;
                    newPt.z = -Camera.main.transform.position.z;
                    Vector3 clickPos = Camera.main.ScreenToWorldPoint(newPt);
                    clickPos.z = 0f;
                    Ray ray = Camera.main.ScreenPointToRay(newPt);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.GetComponentInParent<Script_Target>() != null)
                        {
                            hit.collider.GetComponentInParent<Script_Target>().Hit();
                            CheckPathsMonsters();
                        }
                        else if (hit.collider.GetComponentInParent<Script_TargetBonus>() != null)
                        {
                            hit.collider.GetComponentInParent<Script_TargetBonus>().Hit();
                        }
                    }

                    Instantiate(impactPrefab, clickPos, Quaternion.identity);
                }
            }

        }

        IEnumerator CountDown()
        {
            _countDownText.gameObject.SetActive(true);
            for (int i = 0; i < 10; i++)
            {
                _countDownText.text = (10 - i).ToString();
                yield return new WaitForSeconds(1f);
            }
            _countDownText.text = "0";
            _countDownText.gameObject.SetActive(false);
        }

        void SpawnLittleRocks()
        {
            for (int i = 0; i < pathsList.Count - 1; i++)
            {
                for (int y = 0; y < littleRockToSpawn; y++)
                {
                    int rdmRockIdx = UnityEngine.Random.Range(0, littleRocks.Count);
                    float randomXPos = UnityEngine.Random.Range(pathsList[i].transform.position.x, pathsList[i].pathPoints[pathsList[i].pathPoints.Count - 1].transform.position.x);
                    float randomYPos = UnityEngine.Random.Range(pathsList[i].transform.position.y - pathsList[i].rockYOffset * 0.7f, pathsList[i + 1].transform.position.y - pathsList[i + 1].rockYOffset * 1.5f);
                    float randomZPos = UnityEngine.Random.Range(pathsList[i].transform.position.z, pathsList[i + 1].transform.position.z - pathsList[i + 1].rockYOffset);

                    Vector3 randomPos = new Vector3(randomXPos, randomYPos, randomZPos);

                    foreach (Vector3 pos in littleRockPosition)
                    {
                        if (Vector3.Distance(randomPos, pos) < 5f)
                        {
                            continue;

                        }
                    }

                    littleRockPosition.Add(randomPos);
                    GameObject newLittleRock = Instantiate(littleRocks[rdmRockIdx], randomPos, /*littleRocks[rdmRockIdx].transform.rotation*/Quaternion.identity);
                    newLittleRock.GetComponent<SpriteRenderer>().sortingOrder = 0;

                    //newLittleRock.transform.localScale *= pathsList[i].scaleRatio;
                                       
                    if(IsThemeScaleActive)
                    {
                        float basescale = newLittleRock.transform.localScale.x;
                        float finalscale = basescale + (pathsList[i].scaleRatio*scaleResizer);
                        finalscale = Mathf.Max(finalscale, scalelimit);
                        newLittleRock.transform.localScale = new Vector3(finalscale,finalscale,finalscale);
                        
                    }
                    else
                    {
                        newLittleRock.transform.localScale *= pathsList[i].scaleRatio;
                    }

                    newLittleRock.transform.parent = littleRockHolder;
                }
            }
        }

        void SetupDifficulty()
        {
            foreach (Monstres_PathManager paths in pathsList)
            {
                switch (PlayerPrefs.GetString("Difficulty"))
                {
                    case "Easy":
                        paths.SetAccelerationTime(difficultiesValues[0].timeToMaxSpeed);
                        scoreToSpeedUp = difficultiesValues[0].scoreNeedToSpeedUp;
                        break;
                    case "Medium":
                        paths.SetAccelerationTime(difficultiesValues[1].timeToMaxSpeed);
                        scoreToSpeedUp = difficultiesValues[1].scoreNeedToSpeedUp;
                        break;
                    case "Hard":
                        paths.SetAccelerationTime(difficultiesValues[2].timeToMaxSpeed);
                        scoreToSpeedUp = difficultiesValues[2].scoreNeedToSpeedUp;
                        break;
                }
            }
        }

        public void AddScore(int scoreToAdd, Color newTextColor, Vector3 enemyPos)
        {
            GameObject newScoreFeedback = Instantiate(scoreFeedback, enemyPos, Quaternion.identity);
            newScoreFeedback.GetComponent<Script_ScoreFeedback>().SetupText(scoreToAdd, newTextColor);


            if (score.value + scoreToAdd >= 0)
            {
                score.value += scoreToAdd;
            }
            else
            {
                score.value = 0;
            }

            if (score.value >= scoreToSpeedUp && !gameIsSpeedingUp)
            {
                foreach (Monstres_PathManager paths in pathsList)
                {
                    paths.StartSpeedUp();
                }

                gameIsSpeedingUp = true;
            }

            Monstres_UIManager.Instance.UpdateScore((int)score.value);
        }


        public override void ReceivePoint(float xPoint, float yPoint)
        {
            newPt.x = xPoint * w;
            newPt.y = yPoint * h;
            gotAPt = true;
        }

        int GetSpeedAverage()
        {
            float speedAverage = 0;
            for (int i = 0; i < pathsList.Count; i++)
            {
                speedAverage += pathsList[i].targetTravelTime;
            }

            speedAverage /= pathsList.Count;

            return Mathf.RoundToInt(speedAverage);
        }

        void RollStop()
        {
            if (UnityEngine.Random.Range(0, 101) <= chanceToStop /*- (GetSpeedAverage() * 2)*/)
            {
                StopMovement();
            }
            else
            {
                StartCoroutine(WaitToRollStop());
            }
        }

        void StopMovement()
        {
            StopCoroutine(WaitToRollStop());
            audioSourceComp.PlayOneShot(stopSound);
            isStop = true;
            timeToResume = UnityEngine.Random.Range(timeToResumeInterval.x, timeToResumeInterval.y);
            foreach (Monstres_PathManager paths in pathsList)
            {
                paths.Stop();
            }
        }

        void ResumeMovement()
        {
            isStop = false;
            foreach (Monstres_PathManager paths in pathsList)
            {
                paths.Resume();
            }

            StartCoroutine(WaitToRollStop());
        }

        public bool GetGameIsRunning()
        {
            return gameIsRunning;
        }

        void EndGame()
        {
            StopAllCoroutines();
            audioSourceComp.PlayOneShot(gameOverSound);
            Monstres_UIManager.Instance.ShowEnd();
            gameIsRunning = false;
            if (MusicLoader.Instance != null)
            {
                MusicLoader.Instance.StopMusic();
            }
            PlayerPrefs.SetInt("Monstres_FinalScore", (int)score.value);
            PlayerPrefs.SetFloat( Monstres_GeneralVariables.HighScoreKey, (int)score.value);
            StartCoroutine(WaitToGoToLeaderboard());
        }

        void CheckPathsMonsters()
        {
            foreach (Monstres_PathManager paths in pathsList)
            {
                if (paths.CheckMonstersInPath())
                {
                    return;
                }
            }

            foreach (Monstres_PathManager paths in pathsList)
            {
                paths.SpeedUpNoSmooth();
            }
        }

        public Sprite GetRandomPlayerSprite()
        {
            if (OSC_Manager.Instance != null)
            {
                if (OSC_Manager.Instance.playersSprites.Count > 0)
                {
                    if (lastPlayerSpawn < OSC_Manager.Instance.playersSprites.Count - 1)
                    {
                        lastPlayerSpawn++;
                    }
                    else
                    {
                        lastPlayerSpawn = 0;
                    }

                    return OSC_Manager.Instance.playersSprites[lastPlayerSpawn];
                }
            }

            return noPlayerSprite;
        }

        public int GetScore()
        {
            return (int)score.value;
        }

        public AudioClip GetRandomImpactSound()
        {
            AudioClip randomSound = impactSound[UnityEngine.Random.Range(0, impactSound.Count)];

            if (lastImpactSoundPlayed != null)
            {
                impactSound.Add(lastImpactSoundPlayed);
            }
            lastImpactSoundPlayed = randomSound;
            impactSound.Remove(lastImpactSoundPlayed);
            return randomSound;
        }

        IEnumerator WaitToRollStop()
        {
            yield return new WaitForSeconds(minTimeBetweenStop);
            RollStop();
        }

        IEnumerator WaitToGoToLeaderboard()
        {
            yield return new WaitForSeconds(timeBeforeLeaderboard);
            if (BuildState.CurrentState == BuildState.State.normal)
            {
                SceneManager.LoadScene(Monstres_GeneralVariables.Instance.scoreScene);
            }
            else
            {
                MenuSelectionButton.Instance.gameObject.SetActive(true);
            }
            
        }
    }

    [System.Serializable]
    public class DifficultyOptions
    {
        public string difficultyName;
        public float timeToMaxSpeed;
        public int scoreNeedToSpeedUp;
    }
}