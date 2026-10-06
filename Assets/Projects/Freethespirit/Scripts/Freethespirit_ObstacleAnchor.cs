using UnityEngine;

namespace Freethespirit
{
    public enum GuardSpawnFormation
    {
        [Tooltip("Alignement horizontal en ligne.")]
        Line,

        [Tooltip("Disposition en grille Trapezoid.")]
        Trapezoid,

        [Tooltip("Disposition en losange.")]
        Diamond,

        [Tooltip("Disposition en parallélogramme (grille inclinée).")]
        Parallelogram
    }

    public class Freethespirit_ObstacleAnchor : MonoBehaviour
    {
        [Header("Zone de spawn Obstacles")]
        public float spawnRadius = 2f;
        public int minCount = 1, maxCount = 3;
        public bool isFantomeAnchor = false;

        [Header("Configuration des Soldats / Gardes")]
        [Tooltip("Nombre de soldats/gardes à spawner sur cette scène / ancre.")]
        public int guardCount = 2;

        [Tooltip("Configuration de la formation pour le spawn des gardes.")]
        public GuardSpawnFormation formation = GuardSpawnFormation.Line;

        [Tooltip("Facteur multiplicateur d'échelle appliqué aux gardes et au fantôme.")]
        public Vector3 spawnScale = Vector3.one;

        [Tooltip("Espacement horizontal (axe X / Right) de la formation.")]
        public float spacingX = 2f;

        [Tooltip("Espacement vertical/profondeur (axe Z / Forward) de la formation.")]
        public float spacingZ = 2f;

        [Header("Déplacement du garde central")]
        [Tooltip("Si activé, le garde normalement situé au milieu de la formation est déplacé à côté.")]
        public bool moveMiddleGuardAside = false;

        [Tooltip("Distance supplémentaire entre le bord de la formation et le garde central déplacé.")]
        public float middleGuardAsideSpacing = 2f;

        [Header("Prefabs de cet anchor")]
        public GameObject[] obstaclePrefabs;

        [Header("Caméra dédiée")]
        public Camera anchorCamera;
        public Transform[] patrolWaypoints;
        public Transform fantomeSpawnPoint;

        public Vector2 GetFormationOffset(int index, int totalCount)
        {
            if (totalCount <= 1) return Vector2.zero;

            Vector2 offset;

            switch (formation)
            {
                case GuardSpawnFormation.Line:
                {
                    float x = (index - (totalCount - 1) * 0.5f) * spacingX;
                    offset = new Vector2(x, 0f);
                    break;
                }

                case GuardSpawnFormation.Trapezoid:
                {
                    int centerIndex = totalCount / 2;
                    int sideOffset = index - centerIndex;
                    float x = sideOffset * spacingX;
                    float z = -Mathf.Abs(sideOffset) * spacingZ;
                    offset = new Vector2(x, z);
                    break;
                }

                case GuardSpawnFormation.Diamond:
                {
                    if (totalCount == 2)
                    {
                        offset = index == 0 ? new Vector2(0f, spacingZ * 0.5f) : new Vector2(0f, -spacingZ * 0.5f);
                        break;
                    }

                    if (totalCount == 3)
                    {
                        if (index == 0) offset = new Vector2(0f, spacingZ * 0.5f);
                        else if (index == 1) offset = new Vector2(-spacingX * 0.5f, -spacingZ * 0.5f);
                        else offset = new Vector2(spacingX * 0.5f, -spacingZ * 0.5f);

                        break;
                    }

                    if (totalCount == 4)
                    {
                        if (index == 0) offset = new Vector2(0f, spacingZ);
                        else if (index == 1) offset = new Vector2(spacingX, 0f);
                        else if (index == 2) offset = new Vector2(0f, -spacingZ);
                        else offset = new Vector2(-spacingX, 0f);

                        break;
                    }

                    float t = (float)index / totalCount;
                    float a = spacingX * 1.2f;
                    float b = spacingZ * 1.2f;

                    if (t < 0.25f)
                    {
                        float u = t / 0.25f;
                        offset = new Vector2(u * a, (1f - u) * b);
                    }
                    else if (t < 0.5f)
                    {
                        float u = (t - 0.25f) / 0.25f;
                        offset = new Vector2((1f - u) * a, -u * b);
                    }
                    else if (t < 0.75f)
                    {
                        float u = (t - 0.5f) / 0.25f;
                        offset = new Vector2(-u * a, -(1f - u) * b);
                    }
                    else
                    {
                        float u = (t - 0.75f) / 0.25f;
                        offset = new Vector2(-(1f - u) * a, u * b);
                    }

                    break;
                }

                case GuardSpawnFormation.Parallelogram:
                {
                    int cols = Mathf.CeilToInt(Mathf.Sqrt(totalCount));
                    int rows = Mathf.CeilToInt((float)totalCount / cols);
                    int col = index % cols;
                    int row = index / cols;

                    float skew = (row - (rows - 1) * 0.5f) * (spacingX * 0.5f);
                    float x = (col - (cols - 1) * 0.5f) * spacingX + skew;
                    float z = (row - (rows - 1) * 0.5f) * spacingZ;

                    offset = new Vector2(x, z);
                    break;
                }

                default:
                    offset = Vector2.zero;
                    break;
            }

            // =========================================================
            // DÉPLACEMENT DU GARDE DU MILIEU
            // =========================================================
            // Seulement lorsqu'il existe réellement un garde central :
            // 3 -> index 1
            // 5 -> index 2
            // 7 -> index 3
            // etc.
            //
            // Avec un nombre pair, personne n'est déplacé.

            if (moveMiddleGuardAside && totalCount >= 3 && totalCount % 2 == 1 && index == totalCount / 2)
            {
                float furthestRightX = float.MinValue;

                // Recherche la position X la plus à droite de la formation originale.
                for (int i = 0; i < totalCount; i++)
                {
                    if (i == index) continue;

                    Vector2 otherOffset = GetFormationOffsetWithoutMiddleMove(i, totalCount);

                    if (otherOffset.x > furthestRightX) furthestRightX = otherOffset.x;
                }

                // Place l'ancien garde central juste après le garde le plus à droite.
                offset.x = furthestRightX + middleGuardAsideSpacing;
            }

            return offset;
        }

        private Vector2 GetFormationOffsetWithoutMiddleMove(int index, int totalCount)
        {
            if (totalCount <= 1) return Vector2.zero;

            switch (formation)
            {
                case GuardSpawnFormation.Line:
                {
                    float x = (index - (totalCount - 1) * 0.5f) * spacingX;
                    return new Vector2(x, 0f);
                }

                case GuardSpawnFormation.Trapezoid:
                {
                    int centerIndex = totalCount / 2;
                    int sideOffset = index - centerIndex;
                    float x = sideOffset * spacingX;
                    float z = -Mathf.Abs(sideOffset) * spacingZ;
                    return new Vector2(x, z);
                }

                case GuardSpawnFormation.Diamond:
                {
                    if (totalCount == 2) return index == 0 ? new Vector2(0f, spacingZ * 0.5f) : new Vector2(0f, -spacingZ * 0.5f);

                    if (totalCount == 3)
                    {
                        if (index == 0) return new Vector2(0f, spacingZ * 0.5f);
                        if (index == 1) return new Vector2(-spacingX * 0.5f, -spacingZ * 0.5f);
                        return new Vector2(spacingX * 0.5f, -spacingZ * 0.5f);
                    }

                    if (totalCount == 4)
                    {
                        if (index == 0) return new Vector2(0f, spacingZ);
                        if (index == 1) return new Vector2(spacingX, 0f);
                        if (index == 2) return new Vector2(0f, -spacingZ);
                        return new Vector2(-spacingX, 0f);
                    }

                    float t = (float)index / totalCount;
                    float a = spacingX * 1.2f;
                    float b = spacingZ * 1.2f;

                    if (t < 0.25f)
                    {
                        float u = t / 0.25f;
                        return new Vector2(u * a, (1f - u) * b);
                    }

                    if (t < 0.5f)
                    {
                        float u = (t - 0.25f) / 0.25f;
                        return new Vector2((1f - u) * a, -u * b);
                    }

                    if (t < 0.75f)
                    {
                        float u = (t - 0.5f) / 0.25f;
                        return new Vector2(-u * a, -(1f - u) * b);
                    }

                    float lastU = (t - 0.75f) / 0.25f;
                    return new Vector2(-(1f - lastU) * a, lastU * b);
                }

                case GuardSpawnFormation.Parallelogram:
                {
                    int cols = Mathf.CeilToInt(Mathf.Sqrt(totalCount));
                    int rows = Mathf.CeilToInt((float)totalCount / cols);
                    int col = index % cols;
                    int row = index / cols;

                    float skew = (row - (rows - 1) * 0.5f) * (spacingX * 0.5f);
                    float x = (col - (cols - 1) * 0.5f) * spacingX + skew;
                    float z = (row - (rows - 1) * 0.5f) * spacingZ;

                    return new Vector2(x, z);
                }

                default:
                    return Vector2.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position;

            for (int i = 0; i < guardCount; i++)
            {
                Vector2 offset = GetFormationOffset(i, guardCount);
                Vector3 p = origin + transform.right * offset.x + transform.forward * offset.y;

                bool movedMiddle = moveMiddleGuardAside && guardCount >= 3 && guardCount % 2 == 1 && i == guardCount / 2;

                Gizmos.color = movedMiddle ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(p, 0.35f);
                Gizmos.DrawRay(p, transform.forward * 0.7f);
            }
        }
    }
}