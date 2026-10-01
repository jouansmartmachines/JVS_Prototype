using UnityEngine;
using System.Collections;

namespace Demolition
{
    [RequireComponent(typeof(Rigidbody))]
    public class Demolition_Guard : MonoBehaviour
    {
        [Header("Mode & État")]
        public GuardMode guardMode = GuardMode.Patrol;
        public GuardState currentState = GuardState.Idle;
        public bool startPatrol = false;

        [Header("Zone & Navigation")]
        public float patrolWidthX = 3f, patrolWidthZ = 2f;
        public float patrolSpeed = 1.8f, defensiveSpeedMultiplier = 1.55f, rotationSpeed = 360f;
        public float obstacleCheckDistance = 0.65f, maxSlopeNormalY = 0.20f;
        public LayerMask obstacleLayerMask = ~0;

        [Header("Durées & Alertes")]
        public float minIdleDuration = 1.5f, maxIdleDuration = 3.5f, dialogueChance = 0.35f;
        public float defensiveDuration = 6f, alertRadius = 9f;
        public int maxAlertedNeighbors = 2, maxHits = 2;
        public float hitRecoilSpeed = 8f;

        [Header("Références")]
        public Animator animator;
        [SerializeField] private Renderer batteryRenderer;
        public System.Action<Demolition_Guard> OnGuardDestroyed;

        [Header("Bouclier")]
        [SerializeField] private GameObject shield;
        [SerializeField] private float shieldDuration=5f;
        [SerializeField] private float minShieldDelay=3f;
        [SerializeField] private float maxShieldDelay=8f;

        [Header("Zone Guard")]
        [SerializeField] private float guardMoveRange=1.2f;

        [SerializeField] private float guardInitialIdleDuration;
        private float guardEnterTime;
        private Vector3 guardCenter;
        private bool shieldActive;
        private float shieldTimer;
        private float nextShieldTime;
        private bool hasBeenHit = false;

        [SerializeField] private float guardMinMoveDelay;
        [SerializeField] private float guardMaxMoveDelay;
        private float nextGuardMoveTime;
        private bool guardMoving;

        private float guardMoveSpeed => patrolSpeed * defensiveSpeedMultiplier;
        

        
        private float guardMoveEndTime;
        private Demolition_GuardMotor motor;
        private Demolition_GuardVisuals visuals;
        private Demolition_GuardCombat combat;
        private Rigidbody rb;

        private float stateTimer = 0f, defensiveTimer = 0f, currentIdleDuration = 2f;
        private bool hasDialogued = false, previousStartPatrol = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            motor = GetComponent<Demolition_GuardMotor>() ?? gameObject.AddComponent<Demolition_GuardMotor>();
            visuals = GetComponent<Demolition_GuardVisuals>() ?? gameObject.AddComponent<Demolition_GuardVisuals>();
            combat = GetComponent<Demolition_GuardCombat>() ?? gameObject.AddComponent<Demolition_GuardCombat>();
        }

        void OnEnable() => Demolition_GuardCombat.RegisterGuard(this);
        void OnDisable() => Demolition_GuardCombat.UnregisterGuard(this);

        public void Initialize(Transform center, float startAngle, Transform[] waypoints = null)
        {
            motor.Initialize(waypoints, obstacleLayerMask, maxSlopeNormalY, obstacleCheckDistance);
            combat.InitializeHealth(maxHits);
            maxHits = combat.MaxHits;
            visuals.Initialize(animator, batteryRenderer, maxHits);

            if(shield!=null)shield.SetActive(false);
            shieldActive=false;
            ScheduleNextShield();

            if (startPatrol) guardMode = GuardMode.Patrol;
            else guardMode = GuardMode.Idle;

            previousStartPatrol = startPatrol;

            if (guardMode == GuardMode.Patrol)
            {
                visuals.SetGuarding(false);  visuals.SetWalking(true);
                motor.PickNewDestination();
                currentState = GuardState.Walking;
            }
            else if (guardMode == GuardMode.Static)
            {
                currentState = GuardState.Static;
                visuals.SetGuarding(false);  visuals.SetWalking(false);
            }
            else EnterIdle();
        }

        void Start()
        {
            if (combat.MaxHits == 0) Initialize(transform.parent, 0f);
        }

        void Update()
        {
            motor.TickCooldown(Time.deltaTime);

            if (startPatrol != previousStartPatrol && guardMode != GuardMode.Static 
                && currentState != GuardState.Dead && currentState != GuardState.DefensiveGuard)
            {
                previousStartPatrol = startPatrol;
                guardMode = startPatrol ? GuardMode.Patrol : GuardMode.Idle;

                if (guardMode == GuardMode.Patrol)
                {
                    visuals.SetGuarding(false); visuals.SetWalking(true);
                    motor.PickNewDestination();
                    currentState = GuardState.Walking;
                    stateTimer = 0f;
                }
                else EnterIdle();
            }

            UpdateShield();
            stateTimer += Time.deltaTime;

            switch (currentState)
            {
                case GuardState.Static:
                    FaceCamera(rotationSpeed);
                    motor.UpdatePhysicsState(false, true);
                    break;

                case GuardState.Idle:
                    visuals.SetGuarding(false);  visuals.SetWalking(false);
                    motor.UpdatePhysicsState(false, true);

                    if (!hasDialogued && stateTimer >= currentIdleDuration * 0.35f)
                    {
                        hasDialogued = true;
                        if (Random.value < dialogueChance)
                        {
                            visuals.SetWalking(false);  visuals.SetGuarding(false);
                            visuals.PlayDialogue();
                            if (guardMode != GuardMode.Static) currentState = GuardState.Dialogue;
                            stateTimer = 0f;
                        }
                    }

                    if (stateTimer >= currentIdleDuration && guardMode == GuardMode.Patrol)
                    {
                        visuals.SetGuarding(false); visuals.SetWalking(true);
                        motor.PickNewDestination();
                        currentState = GuardState.Walking;
                        stateTimer = 0f;
                    }
                    break;

                case GuardState.Walking:
                    visuals.SetGuarding(false);
                    visuals.SetWalking(true, Mathf.Clamp(patrolSpeed / 1.6f, 0.7f, 1.3f));
                    motor.UpdatePhysicsState(true, false);

                    if (motor.IsStuck(Time.deltaTime) || stateTimer > 6f)
                    {
                        motor.PickNewDestination(true);
                        stateTimer = 0f;
                    }
                    break;

                case GuardState.DefensiveGuard:
                    FaceCamera(rotationSpeed*2f);
                    motor.UpdatePhysicsState(true,false);
                    guardMode=GuardMode.Defensive;

                    if(Time.time-guardEnterTime<guardInitialIdleDuration)
                    {
                        visuals.SetGuardMovement(Vector3.zero);
                        motor.UpdatePhysicsState(false, true);
                        break;
                    }

                  if(!guardMoving)
                    {
                        visuals.SetGuardMovement(Vector3.zero);
                        motor.UpdatePhysicsState(false,true);
                        if(Time.time>=nextGuardMoveTime)
                            StartGuardMove();
                    }
                    else
                    {
                        Vector3 dir=motor.TargetWorldPosition-transform.position;
                        dir.y=0f;
                        visuals.SetGuardMovement(dir);
                        motor.UpdatePhysicsState(true,false);
                        if(Time.time>=guardMoveEndTime)
                            ScheduleGuardMove();
                    }

                    break;

                case GuardState.Dialogue:
                    visuals.SetWalking(false); visuals.SetGuarding(false);
                    motor.UpdatePhysicsState(false, true);

                    if (!visuals.IsPlayingDialogue() && stateTimer >= 2.5f)
                    {
                        if (guardMode == GuardMode.Patrol)
                        {
                            visuals.SetWalking(true);
                            motor.PickNewDestination();
                            currentState = GuardState.Walking;
                            stateTimer = 0f;
                        }
                        else EnterIdle();
                    }
                    break;
            }
        }

        
        void FixedUpdate()
        {
            if(currentState==GuardState.Walking)
            {
                if(motor.MoveTowardsTarget(patrolSpeed,rotationSpeed,Time.fixedDeltaTime,true,false))
                    EnterIdle();
            }
            else if(currentState==GuardState.DefensiveGuard&&guardMoving)
            {
                motor.MoveTowardsTarget(guardMoveSpeed,rotationSpeed,Time.fixedDeltaTime,false,true);
            }
        }


        public void OnTouched()
        {
            if(currentState==GuardState.Dead)return;

            if(shieldActive)
            {
                DestroyShield();
                return;
            }

            bool firstHit=!hasBeenHit;
            hasBeenHit=true;

            bool isDead=combat.TakeDamage();
            visuals.UpdateBattery(combat.MaxHits-combat.CurrentHits,combat.MaxHits);

            visuals.PlayHit();
            StartCoroutine(ApplyHitRecoil());

            if(isDead)
            {
                Die();
                return;
            }
            if(currentState!=GuardState.DefensiveGuard)
            {
                combat.AlertAllGuards(this);
                StartCoroutine(EnterGuardAfterHit());   
            }
            
            if(firstHit)
                ActivateShield();
        }
        private IEnumerator EnterGuardAfterHit()
        {
            motor.Stop();
            yield return null;

            while(!visuals.IsPlayingHit())
                yield return null;

            while(visuals.IsPlayingHit())
            {
                motor.Stop();
                yield return null;
            }

            EnterDefensiveState();
        }

        private IEnumerator ApplyHitRecoil()
        {
            if (rb == null) yield break;

            Vector3 recoilDir = -transform.forward;
            float timer = 0f, dur = 0.22f, speed = Mathf.Max(3.5f, hitRecoilSpeed * 0.45f);

            while (timer < dur)
            {
                float s = Mathf.Lerp(speed, 0f, timer / dur);
                rb.linearVelocity = new Vector3(recoilDir.x * s, rb.linearVelocity.y, recoilDir.z * s);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        private void ScheduleNextShield()
        {
            nextShieldTime=Time.time+Random.Range(minShieldDelay,maxShieldDelay);
        }

        private void ActivateShield()
        {
            //if(shield==null||currentState!=GuardState.DefensiveGuard)return;
            shieldActive=true;
            shieldTimer=shieldDuration;
            shield.SetActive(true);
        }

        private void DestroyShield()
        {
            if(!shieldActive)return;
            shieldActive=false;
            shieldTimer=0f;
            if(shield!=null)shield.SetActive(false);
            ScheduleNextShield();
        }

        private void ScheduleGuardMove()
        {
            guardMoving=false;
            motor.Stop();
            motor.UpdatePhysicsState(false,true);
            Debug.Log("Guard move scheduled. Next move in: " + guardMinMoveDelay + " to " + guardMaxMoveDelay + " seconds.");
            nextGuardMoveTime=Time.time+Random.Range(guardMinMoveDelay,guardMaxMoveDelay);
        }

        private void StartGuardMove()
        {
            Camera cam=Camera.main;
            if(cam==null)return;

            Vector3 right=cam.transform.right;
            Vector3 forward=cam.transform.forward;
            right.y=forward.y=0f;
            right.Normalize();
            forward.Normalize();

            float side=Random.Range(-guardMoveRange,guardMoveRange);
            float depth=Random.Range(-guardMoveRange*.25f,guardMoveRange*.25f);

            motor.SetTargetPosition(guardCenter+right*side+forward*depth);
            guardMoveEndTime=Time.time+Random.Range(guardMinMoveDelay,guardMaxMoveDelay);
            guardMoving=true;
        }

        private void UpdateShield()
        {
            if(currentState!=GuardState.DefensiveGuard)
            {
                //if(shieldActive)DestroyShield();
                return;
            }

            if(shieldActive)
            {
                shieldTimer-=Time.deltaTime;
                if(shieldTimer<=0f)DestroyShield();
            }
            else if(Time.time>=nextShieldTime)ActivateShield();
        }

        private void FaceCamera(float rotSpeed)
        {
            Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (cam == null) return;

            Vector3 toCam = cam.transform.position - transform.position;
            toCam.y = 0f;

            if (toCam.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toCam.normalized, Vector3.up), rotSpeed * Time.deltaTime);
        }

        public void ReceiveAlert(Vector3 threatPos, float delay)
        {
            if (currentState == GuardState.Dead || currentState == GuardState.DefensiveGuard) return;
            StartCoroutine(AlertResponseRoutine(threatPos, delay));
        }

        private IEnumerator AlertResponseRoutine(Vector3 threatPos, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (currentState == GuardState.Dead) yield break;

            motor.RotateTowardsDirection(threatPos - transform.position, rotationSpeed * 2f, 0.2f);
            yield return new WaitForSeconds(0.2f);

            if (currentState == GuardState.Dead) yield break;
            EnterDefensiveState();
        }
        public void EnterDefensiveState()
        {
            if(currentState==GuardState.Dead||currentState==GuardState.DefensiveGuard)return;

            currentState=GuardState.DefensiveGuard;

            guardCenter=transform.position;
            guardEnterTime=Time.time;
            guardMoving=false;
            nextGuardMoveTime=guardEnterTime+guardInitialIdleDuration;

           visuals.EnterGuardAnimation();
            visuals.SetGuardMovement(Vector3.zero);

            motor.UpdatePhysicsState(true,false);
            FaceCamera(9999f);
        }

        private void PickGuardDestination()
        {
            Vector2 r=Random.insideUnitCircle*guardMoveRange;
            motor.SetTargetPosition(guardCenter+new Vector3(r.x,0f,r.y));
        }
        

        private void EnterIdle()
        {
            currentState = GuardState.Idle;
            stateTimer = 0f;
            defensiveTimer = 0f;
            hasDialogued = false;
            currentIdleDuration = Random.Range(minIdleDuration, maxIdleDuration);

            motor.Stop();
            visuals.SetGuarding(false);  visuals.SetWalking(false);
            visuals.CrossFadeIdle();
        }

        private void Die()
        {
            currentState = GuardState.Dead;
            OnGuardDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        void OnCollisionStay(Collision c)
        {
            if (currentState == GuardState.Walking || currentState == GuardState.DefensiveGuard) motor.HandleCollision(c);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = currentState == GuardState.DefensiveGuard ? Color.red : Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(patrolWidthX * 2f, 0.2f, patrolWidthZ * 2f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, alertRadius);

            if (motor != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, motor.TargetWorldPosition);
            }
        }
    }
}