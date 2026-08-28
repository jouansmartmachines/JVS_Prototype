using UnityEngine;

namespace Challenge
{
    // Movement Strategy Interface (2D)
    public interface IMovementStrategy
    {
        void Move(Transform transform);
        Vector2 GetMovementDelta();
    }

    // Simple forward movement (vertical)
// Exemple pour votre classe MoveForward
    public class MoveForward : IMovementStrategy
    {
        public float speed = 5f;
        public Vector2 GetMovementDelta() => Vector2.up;

        public void Move(Transform transform)
        {
            // Simple translation basée sur le delta
            transform.Translate((Vector3)GetMovementDelta() * speed * Time.deltaTime);
        }
    }

    // Move toward a specific target (ex: player)
    public class MoveTowardPlayer : IMovementStrategy
    {
        private Transform player;
        private Transform self;

        public float speed = 100f;

        public MoveTowardPlayer(Transform selfTransform, Transform playerTransform)
        {
            self = selfTransform;
            player = playerTransform;
        }

         public void Move(Transform transform)
        {
            // Simple translation basée sur le delta
            transform.Translate((Vector3)GetMovementDelta() * speed * Time.deltaTime);
        }


        public Vector2 GetMovementDelta()
        {
            if (player == null || self == null)
                return Vector2.zero;

            Vector2 selfPos = self.position;
            Vector2 targetPos = player.position;

            return (targetPos - selfPos).normalized;
        }
    }
}
