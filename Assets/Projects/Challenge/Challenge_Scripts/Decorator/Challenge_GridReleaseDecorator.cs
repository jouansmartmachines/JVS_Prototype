using UnityEngine;

namespace Challenge
{
    public class Challenge_GridReleaseDecorator : Challenge_TargetDecorator
    {
        private Challenge_GridManager gridManager;
        private Transform targetRect;

        // Event d’instance pour notifier que la cellule est libre
        public System.Action<Vector2> OnCellReleased;

        public void Initialize(Challenge_GridManager manager, Transform t)
        {
            gridManager = manager;
            targetRect = t;
        }

        private void OnDestroy()
        {
            if (gridManager != null && targetRect != null)
            {
                Vector2 releasedPos = targetRect.position;
                gridManager.FreeCell(releasedPos);
                OnCellReleased?.Invoke(releasedPos);
            }
        }
    }
}
