using UnityEngine;
using System;
using Theme;

namespace Monstres
{
    public class BonusMonster : MonoBehaviour
    {
        public BonusValues bonusValues;
        public bool alreadyAscended;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject monsterPref;
        [SerializeField] private SwapObject monsterSprites;

        private SpriteRenderer monsterSpriteRenderer;
        private Script_TargetBonus targetBonus;
        private bool spawned;
        private GameObject instantiatedMonster;
        private bool goingLeft;
        private Rigidbody monsterRb;
        private BonusEnum bonusEnum;
        private float fallingForce;
        [SerializeField] private float minAmplitudeY;
        [SerializeField] private float maxAmplitudeY;
        [SerializeField] private float frequency = 5f;

        private void Update()
        {
            if (instantiatedMonster != null)
            {
                if (!targetBonus.hit)
                {
                    Vector3 monsterPos = monsterRb.transform.position;
                    monsterPos.y = Mathf.Clamp(monsterRb.transform.position.y, 0, 6.5f);
                    monsterRb.transform.position = monsterPos;

                    if (monsterRb.transform.position.y > 6f)
                    {
                        alreadyAscended = true;
                    }
                }
            }
            else
            {
                if (!spawned)
                {
                    if (Monstres_GameManager.Instance.currentGameDuration <= Monstres_GameManager.Instance.gameDuration / 2)
                    {
                        Spawn();
                        spawned = true;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (instantiatedMonster != null)
            {
                if (!targetBonus.hit)
                {
                    if (!goingLeft)
                    {
                        monsterRb.AddForce(Vector3.right * bonusValues.speed * Time.fixedDeltaTime, ForceMode.Force);
                        if (bonusEnum == BonusEnum.Dragon)
                        {
                            monsterSpriteRenderer.flipX = true;
                        }
                        else
                        {
                            monsterSpriteRenderer.flipX = false;
                        }
                    }
                    else
                    {
                        monsterRb.AddForce(-Vector3.right * bonusValues.speed * Time.fixedDeltaTime, ForceMode.Force);
                        if (bonusEnum == BonusEnum.Dragon)
                        {
                            monsterSpriteRenderer.flipX = false;
                        }
                        else
                        {
                            monsterSpriteRenderer.flipX = true;
                        }
                    }

                    monsterRb.AddForce(Vector3.up * Mathf.Sin(Time.time * frequency) *
                            UnityEngine.Random.Range(minAmplitudeY, maxAmplitudeY), ForceMode.Force);


                    if (monsterRb.transform.position.x <= -13)
                    {
                        goingLeft = false;
                    }
                    else if (monsterRb.transform.position.x >= 13)
                    {
                        goingLeft = true;
                    }

                    Ascend();
                }
            }
        }

        private void Ascend()
        {
            if (!alreadyAscended)
            {
                bonusValues.timerBeforeImpulse -= Time.fixedDeltaTime;
                if (bonusValues.timerBeforeImpulse <= 0)
                {
                    monsterRb.AddForce(Vector3.up * bonusValues.flightImpulseAmount * Time.fixedDeltaTime, ForceMode.Force);
                    bonusValues.timeOfImpulse -= Time.fixedDeltaTime;
                    if (bonusValues.timeOfImpulse <= 0)
                    {
                        bonusValues.timeToAscendAgain = bonusValues.defaultTimeToAscendAgain;
                        alreadyAscended = true;
                    }
                }
                fallingForce = 0;
            }
            else
            {
                bonusValues.timeToAscendAgain -= Time.fixedDeltaTime;

                if (bonusValues.timeToAscendAgain > ((bonusValues.defaultTimeToAscendAgain / 3) * 2))
                {
                    if (!goingLeft)
                    {
                        monsterRb.AddForce(Vector3.right * bonusValues.irregularImpulse * Time.fixedDeltaTime, ForceMode.Force);
                        monsterRb.AddForce(Vector3.down * ((bonusValues.irregularImpulse * 2) + UnityEngine.Random.Range(0, 500)) * Time.fixedDeltaTime,
                            ForceMode.Force); ;
                    }
                    else
                    {
                        monsterRb.AddForce(-Vector3.right * bonusValues.irregularImpulse * Time.fixedDeltaTime, ForceMode.Force);
                        monsterRb.AddForce(Vector3.down * ((bonusValues.irregularImpulse * 2) + UnityEngine.Random.Range(0, 500)) * Time.fixedDeltaTime,
                            ForceMode.Force);
                    }
                }

                fallingForce += bonusValues.gravityStrength;
                monsterRb.AddForce(Vector3.down * fallingForce * Time.fixedDeltaTime, ForceMode.Force);

                if (bonusValues.timeToAscendAgain <= 0)
                {
                    //Set everything back to default
                    bonusValues.timerBeforeImpulse = bonusValues.defaultTimerBeforeImpulse;
                    bonusValues.timeOfImpulse = bonusValues.defaultTimeOfImpulse;
                    SetImpulse();
                    alreadyAscended = false;
                }
            }
        }

        private void Spawn()
        {
            int rndSpawn = UnityEngine.Random.Range(0, spawnPoints.Length);
            int rndSprite = UnityEngine.Random.Range(0, Enum.GetNames(typeof(BonusEnum)).Length);
            ///Set the enum of the bonus monster : Dragon, bee or bat
            bonusEnum = (BonusEnum)rndSprite;

            instantiatedMonster = Instantiate(monsterPref, new Vector3(spawnPoints[rndSpawn].position.x,
                spawnPoints[rndSpawn].position.y + UnityEngine.Random.Range(-3, 3), spawnPoints[rndSpawn].position.z), Quaternion.identity);

            targetBonus = instantiatedMonster.GetComponent<Script_TargetBonus>();
            monsterRb = instantiatedMonster.GetComponent<Rigidbody>();
            goingLeft = Convert.ToBoolean(rndSpawn);

            monsterSpriteRenderer = instantiatedMonster.GetComponentInChildren<SpriteRenderer>();
            monsterSpriteRenderer.sprite = monsterSprites.GetSwapEntity<SwapSprite>().Sprites[(int)bonusEnum];
            monsterSpriteRenderer.material.SetTexture("_Emission", monsterSpriteRenderer.sprite.texture);

            switch (monsterSpriteRenderer.sprite.name)
            {
                case "Sprite_Bat":
                    targetBonus.GetComponentInChildren<Animator>().SetBool("isBat", true);
                    break;
                case "Sprite_Bee":
                    targetBonus.GetComponentInChildren<Animator>().SetBool("isBee", true);
                    break;
                case "Sprite_Dragon":
                    targetBonus.GetComponentInChildren<Animator>().SetBool("isDragon", true);
                    break;
            }

            foreach (BonusValues b in BonusValuesScript.Instance.bonuses)
            {
                if (b.monsterEnum == bonusEnum)
                {
                    bonusValues = b;
                    break;
                }
            }

            SetImpulse();
        }

        //Reset the values for more randomness in the game
        private void SetImpulse()
        {
            bonusValues.timerBeforeImpulse = UnityEngine.Random.Range(bonusValues.minBeforeImpulseTime, bonusValues.maxBeforeImpulseTime);
            bonusValues.defaultTimerBeforeImpulse = bonusValues.timerBeforeImpulse;

            bonusValues.timeOfImpulse = UnityEngine.Random.Range(bonusValues.minTimeOfImpulse, bonusValues.maxTimeOfImpulse);
            bonusValues.defaultTimeOfImpulse = bonusValues.timeOfImpulse;

            bonusValues.timeToAscendAgain = UnityEngine.Random.Range(bonusValues.minTimeToAscendAgain, bonusValues.maxTimeToAscendAgain);
            bonusValues.defaultTimeToAscendAgain = bonusValues.timeToAscendAgain;
        }
    }
}