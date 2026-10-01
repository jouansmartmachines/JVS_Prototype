using UnityEngine;

namespace Demolition
{
    [RequireComponent(typeof(Rigidbody))]
    public class Demolition_GuardMotor : MonoBehaviour
    {
        public Vector3 TargetWorldPosition { get; private set; }
        public bool HasWaypoints => waypoints != null && waypoints.Length > 0;
        public Vector3 DebugMoveDirection => currentMoveDirection;
        public float DebugObstacleDistance => obstacleCheckDistance;
        public bool DebugObstacleDetected { get; private set; }
        public Vector3 DebugObstaclePoint { get; private set; }
        

        private Rigidbody rb;
        private Collider col;
        private Transform[] waypoints;
        private int currentWaypointIndex;
        private LayerMask obstacleLayerMask = ~0;
        private float maxSlopeNormalY = 0.2f, obstacleCheckDistance = 0.65f;
        private Vector3 lastStuckCheckPos, currentMoveDirection, avoidanceDirection;
        private float lastStuckTimer, collisionCooldown, avoidanceTimer, stuckDuration;
        private bool isWaitingForGround = true;

        public void Initialize(Transform[] newWaypoints, LayerMask mask, float slopeY, float checkDist)
        {
            rb = GetComponent<Rigidbody>(); col = GetComponent<Collider>(); waypoints = newWaypoints;
            obstacleLayerMask = mask; maxSlopeNormalY = slopeY; obstacleCheckDistance = checkDist;

            if (rb != null)
            {
                rb.isKinematic = false; rb.useGravity = true; rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            }

            if (col != null) col.material = new PhysicsMaterial("GuardFrictionless") { dynamicFriction = 0f, staticFriction = 0f, frictionCombine = PhysicsMaterialCombine.Minimum };

            TargetWorldPosition = transform.position; lastStuckCheckPos = transform.position; currentMoveDirection = transform.forward;
        }

        public void UpdatePhysicsState(bool useGravity, bool isKinematic)
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (rb != null && !isWaitingForGround) { rb.useGravity = useGravity; rb.isKinematic = isKinematic; }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isWaitingForGround || rb == null) return;
            isWaitingForGround = false; rb.useGravity = false; rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
        }
        
        public void SetTargetPosition(Vector3 position)
        {
            TargetWorldPosition=position;
        }

        public Vector3 Velocity
        {
            get
            {
                if (rb == null) return Vector3.zero;
                return new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }
        public void Stop()
        {
            if (rb == null) return;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f); rb.angularVelocity = Vector3.zero;
            avoidanceTimer = stuckDuration = 0f;
        }

        public void RotateTowardsDirection(Vector3 dir, float speed, float dt)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir.normalized), speed * dt);
        }

        public bool IsStuck(float dt, float checkInterval = 0.5f, float minDistance = 0.08f)
        {
            lastStuckTimer += dt;
            if (lastStuckTimer < checkInterval) return false;

            Vector3 moved = transform.position - lastStuckCheckPos; moved.y = 0f;
            lastStuckCheckPos = transform.position; lastStuckTimer = 0f;

            return moved.magnitude < minDistance;
        }

        public void PickNewDestination(bool avoidCurrentDirection = false, float waypointWeight = 2.5f)
        {
            lastStuckTimer = 0f; lastStuckCheckPos = transform.position;

            Vector3 pos = transform.position, baseTarget = pos;

            if (HasWaypoints)
            {
                if (avoidCurrentDirection) currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                if (waypoints[currentWaypointIndex] != null) baseTarget = waypoints[currentWaypointIndex].position;
            }

            Vector3 bestTarget = pos;
            float bestScore = -9999f;

            for (int i = 0; i < 18; i++)
            {
                Vector2 rnd = Random.insideUnitCircle * Random.Range(1.2f, 3.2f);
                Vector3 candidate = baseTarget + new Vector3(rnd.x, 0f, rnd.y);

                if (Physics.Raycast(candidate + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, obstacleLayerMask, QueryTriggerInteraction.Ignore)) candidate.y = hit.point.y;
                else candidate.y = pos.y;

                Vector3 diff = candidate - pos; diff.y = 0f;
                if (diff.magnitude < 0.8f) continue;

                Vector3 dir = diff.normalized, targetDir = baseTarget - pos; targetDir.y = 0f;
                float alignment = targetDir.sqrMagnitude > 0.01f ? Vector3.Dot(dir, targetDir.normalized) : 0f;
                float score = alignment * waypointWeight + GetDirectionClearance(dir, 1.5f) * 2f + Random.Range(0f, 0.7f);

                if (score > bestScore) { bestScore = score; bestTarget = candidate; }
            }

            if (bestScore <= -9000f) bestTarget = pos + FindBestOpenDirection(transform.forward, 2f) * 2f;
            if (HasWaypoints && Random.value < 0.35f) currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            TargetWorldPosition = bestTarget; avoidanceTimer = stuckDuration = 0f;
        }

        public bool MoveTowardsTarget(float speed,float rotationSpeed,float dt,bool rotate=true,bool smartAvoidance=false)
        {
            if(rb==null)return false;

            Vector3 toTarget=TargetWorldPosition-transform.position;
            toTarget.y=0f;

            if(toTarget.magnitude<=0.35f){Stop();return true;}

            Vector3 desired=toTarget.normalized;
            Vector3 dir=desired;

            if(CheckObstacleWide(desired,obstacleCheckDistance*1.7f))
                dir=FindBestOpenDirection(desired,2f);

            if(smartAvoidance)
            {
                Vector3 separation=Vector3.zero;
                float radius=Mathf.Max(obstacleCheckDistance*2f,1f);

                foreach(Collider hit in Physics.OverlapSphere(transform.position,radius,~0,QueryTriggerInteraction.Ignore))
                {
                    Demolition_Guard other=hit.GetComponentInParent<Demolition_Guard>();
                    if(other==null||other.transform==transform)continue;

                    Vector3 away=transform.position-other.transform.position;
                    away.y=0f;
                    float d=away.magnitude;

                    if(d>0.001f&&d<radius)
                        separation+=away.normalized*(1f-d/radius);
                }

                if(separation.sqrMagnitude>0.001f)
                {
                    Vector3 avoid=(dir+separation*1.5f).normalized;
                    dir=!CheckObstacleWide(avoid,obstacleCheckDistance)?avoid:FindBestOpenDirection(desired,2f);
                }
            }

            if(currentMoveDirection.sqrMagnitude<0.01f)currentMoveDirection=dir;
            currentMoveDirection=Vector3.Slerp(currentMoveDirection,dir,8f*dt).normalized;

            if(CheckObstacleWide(currentMoveDirection,Mathf.Max(0.7f,speed*0.2f)))
            {
                currentMoveDirection=FindBestOpenDirection(desired,2f);

                if(CheckObstacleWide(currentMoveDirection,0.6f))
                {
                    Stop();
                    return false;
                }
            }

            Vector3 v=currentMoveDirection*speed;
            rb.linearVelocity=new Vector3(v.x,rb.linearVelocity.y,v.z);

            if(rotate)
                transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(currentMoveDirection),rotationSpeed*dt);

            return false;
        }
        private Vector3 FindBestOpenDirection(Vector3 wanted, float distance)
        {
            wanted.y = 0f;
            if (wanted.sqrMagnitude < 0.001f) wanted = transform.forward;
            wanted.Normalize();

            float[] angles = { 0f, -25f, 25f, -45f, 45f, -70f, 70f, -90f, 90f, -120f, 120f, 180f };
            Vector3 best = wanted;
            float bestScore = -9999f;

            foreach (float angle in angles)
            {
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * wanted;
                float score = GetDirectionClearance(dir, distance) * 3f + Vector3.Dot(dir, wanted) * 1.5f;

                if (currentMoveDirection.sqrMagnitude > 0.01f) score += Vector3.Dot(dir, currentMoveDirection.normalized) * 0.6f;
                if (score > bestScore) { bestScore = score; best = dir; }
            }

            return best.normalized;
        }

        private float GetDirectionClearance(Vector3 dir, float distance)
        {
            if (dir.sqrMagnitude < 0.001f) return 0f;

            Vector3 origin = transform.position + Vector3.up * 0.65f;
            float radius = col != null ? Mathf.Clamp(Mathf.Min(col.bounds.extents.x, col.bounds.extents.z) * 0.55f, 0.12f, 0.35f) : 0.2f;

            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, dir.normalized, distance, obstacleLayerMask, QueryTriggerInteraction.Ignore);
            float closest = distance;

            foreach (RaycastHit hit in hits)
            {
                Transform t = hit.transform;
                if (t == transform || t.IsChildOf(transform) || hit.normal.y >= maxSlopeNormalY) continue;
                if (hit.distance < closest) closest = hit.distance;
            }

            return closest / Mathf.Max(0.01f, distance);
        }

        private bool CheckObstacleWide(Vector3 dir, float distance) => GetDirectionClearance(dir, distance) < 0.85f;

        public void HandleCollision(Collision collision)
        {
            if (collisionCooldown > 0f) return;

            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y >= maxSlopeNormalY) continue;

                Vector3 toTarget = TargetWorldPosition - transform.position; toTarget.y = 0f;
                if (toTarget.sqrMagnitude < 0.01f) continue;

                if (Vector3.Dot(contact.normal, toTarget.normalized) < -0.2f)
                {
                    avoidanceDirection = FindBestOpenDirection(toTarget.normalized, 2f);
                    currentMoveDirection = avoidanceDirection; avoidanceTimer = 0.45f; collisionCooldown = 0.25f;
                    break;
                }
            }
        }

        public void TickCooldown(float dt)
        {
            if (collisionCooldown > 0f) collisionCooldown -= dt;
        }

        public void UpdateDebugObstacle()
        {
            Vector3 dir = currentMoveDirection.sqrMagnitude > 0.01f ? currentMoveDirection.normalized : transform.forward;
            Vector3 origin = transform.position + Vector3.up * 0.65f;
            float radius = col != null ? Mathf.Clamp(Mathf.Min(col.bounds.extents.x, col.bounds.extents.z) * 0.55f, 0.12f, 0.35f) : 0.2f;

            DebugObstacleDetected = false;
            DebugObstaclePoint = origin + dir * obstacleCheckDistance;

            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, dir, obstacleCheckDistance, obstacleLayerMask, QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform) || hit.normal.y >= maxSlopeNormalY) continue;
                DebugObstacleDetected = true; DebugObstaclePoint = hit.point;
                break;
            }
        }
    }
}