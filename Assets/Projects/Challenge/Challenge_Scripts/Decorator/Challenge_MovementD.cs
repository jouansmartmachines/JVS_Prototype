using UnityEngine;

namespace Challenge
{
    public class Challenge_MovementD : Challenge_TargetDecorator
    {
        public IMovementStrategy movementStrategy;

        void Update()
        {
            // Sécurité : Si aucune stratégie n'est assignée, on ne fait rien
            if (movementStrategy == null) return;

            // On délègue tout le travail à la stratégie
            movementStrategy.Move(this.transform);
        }
    }
}