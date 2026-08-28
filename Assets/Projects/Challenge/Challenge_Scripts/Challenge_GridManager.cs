using System.Collections.Generic;
using UnityEngine;

namespace Challenge
{
    public class Challenge_GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public RectTransform spawnZone;
        [Range(0f, 0.4f)] 
        [Tooltip("Force du décalage aléatoire (0 = centré, 0.4 = proche du bord)")]
        public float randomOffsetIntensity = 0.25f;

        [Header("Computed (Debug)")]
        [SerializeField] private int rows;
        [SerializeField] private int cols;
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2 origin;
        [SerializeField] private int occupiedCount = 0;

        private PositionMode? lastModeUsed = null;

        private bool[,] gridOccupied;

        public void InitializeGrid(RectTransform referenceRect, float multiplier = 1.2f)
        {
            if (spawnZone == null || referenceRect == null) return;

            Rect refRect = referenceRect.rect;
            Rect zoneRect = spawnZone.rect;

            float screenRatio = Challenge_GeneralVariables.GetScreenRatioFromPrefs();

            // 🔽 Réduction uniforme de la zone
            float adjustedWidth = zoneRect.width / screenRatio;
            float adjustedHeight = zoneRect.height / screenRatio;

            Rect adjustedZone = new Rect(
                zoneRect.center.x - adjustedWidth * 0.5f,
                zoneRect.center.y - adjustedHeight * 0.5f,
                adjustedWidth,
                adjustedHeight
            );

            // 🔢 Calcul lignes / colonnes
            rows = Mathf.Max(1, Mathf.FloorToInt(adjustedZone.height / refRect.height * (1f / multiplier)));
            float cellHeight = adjustedZone.height / rows;
            cols = Mathf.Max(1, Mathf.FloorToInt(adjustedZone.width / cellHeight));

            cellSize = new Vector2(
                adjustedZone.width / cols,
                adjustedZone.height / rows
            );

            // 📍 Centre de la première cellule (bas-gauche)
            origin = new Vector2(
                -adjustedZone.width * 0.5f + cellSize.x / 2f,
                -adjustedZone.height * 0.5f + cellSize.y / 2f
            );

            gridOccupied = new bool[rows, cols];
            occupiedCount = 0;

            Debug.Log($"<color=cyan><b>[INIT]</b> Grid {rows}x{cols} prête (zone ÷ {screenRatio}).</color>");
        }


        public Vector2? GetFreeCell(float percent = 1.0f)
        {
            if (gridOccupied == null) return null;

            List<(int r, int c)> availableCells = new List<(int r, int c)>();

            // Calcul des marges pour le spawn centralisé
            int rowMargin = Mathf.FloorToInt(rows * (1f - percent) / 2f);
            int colMargin = Mathf.FloorToInt(cols * (1f - percent) / 2f);

            int minR = rowMargin;
            int maxR = rows - rowMargin;
            int minC = colMargin;
            int maxC = cols - colMargin;

            for (int r = minR; r < maxR; r++)
            {
                for (int c = minC; c < maxC; c++)
                {
                    if (IsValidIndex(r, c) && !gridOccupied[r, c])
                        availableCells.Add((r, c));
                }
            }

            if (availableCells.Count == 0)
            {
                Debug.LogWarning($"<color=orange>[GRID]</color> Aucune cellule libre !");
                return null;
            }

            var choice = availableCells[Random.Range(0, availableCells.Count)];
            SetCellInternal(choice.r, choice.c, true, $"GetFreeCell({percent * 100}%)");

            // On applique l'offset ici
            return IndexToPos(choice.r, choice.c, true);
        }

        public void FreeCell(Vector2 anchoredPos)
        {
            if (gridOccupied == null) return;

            // On retrouve l'index à partir de la position (sans l'offset)
            int c = Mathf.RoundToInt((anchoredPos.x - origin.x) / cellSize.x);
            int r = Mathf.RoundToInt((anchoredPos.y - origin.y) / cellSize.y);

            if (IsValidIndex(r, c) && gridOccupied[r, c])
            {
                SetCellInternal(r, c, false, "FreeCell(Manual)");
            }
        }

        private void SetCellInternal(int r, int c, bool occupied, string source)
        {
            gridOccupied[r, c] = occupied;
            
            // Recalcul du compteur pour le debug
            occupiedCount = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (gridOccupied[i, j]) occupiedCount++;

            //Debug.Log($"<b>[PROCESS]</b> Case [{r},{c}] -> {(occupied ? "OCCUPÉ" : "LIBRE")} | Via: {source}");
        }

        private Vector2 IndexToPos(int r, int c, bool applyOffset)
        {
            float posX = origin.x + c * cellSize.x;
            float posY = origin.y + r * cellSize.y;

            if (applyOffset)
            {
                // On ajoute un décalage aléatoire dans les limites de la cellule
                float offsetX = Random.Range(-cellSize.x * randomOffsetIntensity, cellSize.x * randomOffsetIntensity);
                float offsetY = Random.Range(-cellSize.y * randomOffsetIntensity, cellSize.y * randomOffsetIntensity);
                posX += offsetX;
                posY += offsetY;
            }

            return new Vector2(posX, posY);
        }

        public Vector2 GetPositionForLevel(Vector2 basePos, Challenge_LevelSettings levelSettings,Vector2 lastpos)
        {
            if (levelSettings == null || levelSettings.positionMode == null || levelSettings.positionMode.Length == 0)
                return GetSpawnInZone(1.0f);

            PositionMode mode;

            if (lastModeUsed == PositionMode.Opposite && System.Array.Exists(levelSettings.positionMode, m => m == PositionMode.Opposite))
            {
                mode = PositionMode.Opposite;
            }
            else
            {
                int randomIndex = Random.Range(0, levelSettings.positionMode.Length);
                mode = levelSettings.positionMode[randomIndex];
            }

            lastModeUsed = mode;

            float percentValue = (int)mode / 100f;
            if (percentValue <= 0) percentValue = 1.0f;

            return mode switch
            {
                PositionMode.Opposite => GetOppositeFromPosition(lastpos),
                PositionMode.Close => GetSpawnInZone(percentValue),
                PositionMode.Nearby => GetSpawnInZone(percentValue),
                _ => GetSpawnInZone(1.0f)
            };
        }

        private Vector2 GetSpawnInZone(float percent)
        {
            Vector2? cell = GetFreeCell(percent);
            return cell ?? Vector2.zero;
        }

        public void ClearLastMode()
        {
            lastModeUsed = null;
        }

        private Vector2 GetOppositeFromPosition(Vector2 pos)
        {
        
            if (gridOccupied == null) return Vector2.zero;
            bool targetLeft = pos.x > 0;
            List<(int r, int c)> validCells = new List<(int r, int c)>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (gridOccupied[r, c]) continue;

                    float cellPosX = origin.x + (c * cellSize.x);
                    if (targetLeft && cellPosX >= -800 && cellPosX <= -300) 
                        validCells.Add((r, c));
                    else if (!targetLeft && cellPosX >= 300 && cellPosX <= 800) 
                        validCells.Add((r, c));
                }
            }
            if (validCells.Count > 0)
            {
                var choice = validCells[Random.Range(0, validCells.Count)];
                SetCellInternal(choice.r, choice.c, true, "Opposite Ping-Pong (Exclusion)");
                return IndexToPos(choice.r, choice.c, true);
            }

            // 4. Sécurité ultime : Si le côté opposé est plein, on cherche n'importe où sauf au centre
            return GetSpawnInZone(1.0f); 
        }
        private bool IsValidIndex(int r, int c) => gridOccupied != null && r >= 0 && r < rows && c >= 0 && c < cols;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (spawnZone == null || cellSize.sqrMagnitude < 0.001f) return;
            Gizmos.matrix = spawnZone.localToWorldMatrix;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector3 pos = new Vector3(origin.x + c * cellSize.x, origin.y + r * cellSize.y, 0);
                    bool isOccupied = gridOccupied != null && gridOccupied[r, c];
                    
                    Gizmos.color = isOccupied ? new Color(1, 0, 0, 0.4f) : new Color(0, 1, 0, 0.05f);
                    Gizmos.DrawCube(pos, new Vector3(cellSize.x * 0.95f, cellSize.y * 0.95f, 0.01f));
                }
            }
        }
#endif
    }
}