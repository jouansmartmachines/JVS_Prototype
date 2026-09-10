using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Demolition
{
    public static class Demolition_GridGenerator
    {
        public struct PlacedBlockDebugInfo 
        { 
            public Vector2Int position; 
            public Vector2Int size; 
        }

        public struct FantomeDebugInfo 
        { 
            public Vector2Int position; 
            public Vector2Int size; 
        }

        public struct CellDebugInfo 
        { 
            public bool isFilled; 
            public bool isFantome; 
        }

        public static void GenerateProceduralStructure(
            Transform targetParent, 
            List<Demolition_ObstacleSpawner.BlockConfig> availableBlocks, 
            GameObject fantomePrefab,
            int gridW, int gridH, float cellSize, int subdivision,
            int levelIndex)
        {
            if (targetParent == null) return;

            int internalW = gridW * subdivision;
            int internalH = gridH * subdivision;
            float internalCellSize = cellSize / subdivision;

            var rbs = new List<Rigidbody>();
            var debugBlocks = new List<PlacedBlockDebugInfo>();
            var debugFantomes = new List<FantomeDebugInfo>();
            var debugGrid = new CellDebugInfo[internalW, internalH];
            bool[,] occupiedGrid = new bool[internalW, internalH];

            var validConfigs = availableBlocks.FindAll(b => b.prefab != null && b.prefab.Length > 0);
            if (validConfigs.Count == 0)
            {
                Debug.LogWarning("[GridGenerator] Aucun bloc avec prefab valide configuré dans availableBlocks.");
                return;
            }

            // Catégorisation par forme
            var horizontalBeams = validConfigs.FindAll(b => b.size.x > b.size.y);
            var verticalPillars = validConfigs.FindAll(b => b.size.y > b.size.x);
            var squareBlocks = validConfigs.FindAll(b => b.size.x == b.size.y);

            var beamConfig = horizontalBeams.Count > 0 ? horizontalBeams[Random.Range(0, horizontalBeams.Count)] : validConfigs[0];
            var pillarConfig = verticalPillars.Count > 0 ? verticalPillars[Random.Range(0, verticalPillars.Count)] : validConfigs[0];
            var squareConfig = squareBlocks.Count > 0 ? squareBlocks[Random.Range(0, squareBlocks.Count)] : validConfigs[0];

            Vector2Int fSize = new Vector2Int(2, 2) * subdivision;

            // Sélection du type de puzzle selon le niveau
            int puzzleType = (levelIndex - 1) % 5;

            switch (puzzleType)
            {
                case 0:
                    BuildDolmenPuzzle(targetParent, pillarConfig, beamConfig, squareConfig, fantomePrefab,
                        internalW, internalH, internalCellSize, subdivision, fSize,
                        occupiedGrid, debugGrid, debugBlocks, debugFantomes, rbs);
                    break;

                case 1:
                    BuildKeystoneBunkerPuzzle(targetParent, pillarConfig, beamConfig, squareConfig, fantomePrefab,
                        internalW, internalH, internalCellSize, subdivision, fSize,
                        occupiedGrid, debugGrid, debugBlocks, debugFantomes, rbs);
                    break;

                case 2:
                    BuildTwoTierTowerPuzzle(targetParent, pillarConfig, beamConfig, squareConfig, fantomePrefab,
                        internalW, internalH, internalCellSize, subdivision, fSize,
                        occupiedGrid, debugGrid, debugBlocks, debugFantomes, rbs);
                    break;

                case 3:
                    BuildCantileverPuzzle(targetParent, pillarConfig, beamConfig, squareConfig, fantomePrefab,
                        internalW, internalH, internalCellSize, subdivision, fSize,
                        occupiedGrid, debugGrid, debugBlocks, debugFantomes, rbs);
                    break;

                default:
                    BuildGrandCitadelPuzzle(targetParent, pillarConfig, beamConfig, squareConfig, fantomePrefab,
                        internalW, internalH, internalCellSize, subdivision, fSize,
                        occupiedGrid, debugGrid, debugBlocks, debugFantomes, rbs);
                    break;
            }

            var host = targetParent.gameObject.GetComponent<StructureHost>();
            if (host == null) host = targetParent.gameObject.AddComponent<StructureHost>();
            host.Init(rbs, debugGrid, debugBlocks, debugFantomes, internalW, internalH, internalCellSize);
        }

        // =========================================================================================
        // ARCHÉTYPES D'ÉNIGMES ROBUSTES
        // =========================================================================================

        private static void BuildDolmenPuzzle(
            Transform parent, Demolition_ObstacleSpawner.BlockConfig pillar, Demolition_ObstacleSpawner.BlockConfig beam, Demolition_ObstacleSpawner.BlockConfig square,
            GameObject fantome, int w, int h, float cSize, int sub, Vector2Int fSize,
            bool[,] occ, CellDebugInfo[,] dGrid, List<PlacedBlockDebugInfo> dBlocks, List<FantomeDebugInfo> dFantomes, List<Rigidbody> rbs)
        {
            Vector2Int pSize = pillar.size * sub;
            Vector2Int bSize = beam.size * sub;
            Vector2Int sqSize = square.size * sub;

            // Portée minimale pour abriter le Fantôme entre les piliers
            int span = Mathf.Max(bSize.x, pSize.x * 2 + fSize.x);
            span = Mathf.Min(span, w);
            int startX = Mathf.Clamp((w - span) / 2, 0, w - span);

            // Pilier Gauche
            PlaceBlockInstance(parent, pillar, startX, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Pilier Droit
            int rightX = startX + span - pSize.x;
            if (rightX > startX + pSize.x)
                PlaceBlockInstance(parent, pillar, rightX, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Fantôme au centre au sol
            int fX = startX + (span - fSize.x) / 2;
            if (fantome != null && CanPlace(occ, fX, 0, fSize.x, fSize.y))
                PlaceFantomeInstance(parent, fantome, fX, 0, fSize, w, cSize, occ, dGrid, dFantomes, rbs);

            // Toiture
            int roofY = pSize.y;
            if (roofY + bSize.y <= h)
            {
                // Poutres pour couvrir l'ensemble de la portée
                PlaceBlockInstance(parent, beam, startX, roofY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);
                if (rightX + pSize.x - bSize.x > startX)
                    PlaceBlockInstance(parent, beam, rightX + pSize.x - bSize.x, roofY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);

                // Caisse de faîte au sommet
                int topX = startX + (span - sqSize.x) / 2;
                if (roofY + bSize.y + sqSize.y <= h)
                    PlaceBlockInstance(parent, square, topX, roofY + bSize.y, sqSize, w, cSize, occ, dGrid, dBlocks, rbs);
            }
        }

        private static void BuildKeystoneBunkerPuzzle(
            Transform parent, Demolition_ObstacleSpawner.BlockConfig pillar, Demolition_ObstacleSpawner.BlockConfig beam, Demolition_ObstacleSpawner.BlockConfig square,
            GameObject fantome, int w, int h, float cSize, int sub, Vector2Int fSize,
            bool[,] occ, CellDebugInfo[,] dGrid, List<PlacedBlockDebugInfo> dBlocks, List<FantomeDebugInfo> dFantomes, List<Rigidbody> rbs)
        {
            Vector2Int pSize = pillar.size * sub;
            Vector2Int bSize = beam.size * sub;
            Vector2Int sqSize = square.size * sub;

            int span = Mathf.Min(w, pSize.x * 2 + sqSize.x + fSize.x + 2 * sub);
            int startX = (w - span) / 2;

            int leftX = startX;
            int midX = startX + pSize.x + 1 * sub;
            int rightX = startX + span - pSize.x;

            PlaceBlockInstance(parent, pillar, leftX, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);
            PlaceBlockInstance(parent, square, midX, 0, sqSize, w, cSize, occ, dGrid, dBlocks, rbs); // Clé de voûte
            if (rightX > midX + sqSize.x)
                PlaceBlockInstance(parent, pillar, rightX, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Fantôme dans la chambre droite
            int fX = midX + sqSize.x + 1;
            if (fantome != null && CanPlace(occ, fX, 0, fSize.x, fSize.y))
                PlaceFantomeInstance(parent, fantome, fX, 0, fSize, w, cSize, occ, dGrid, dFantomes, rbs);

            // Toiture
            int roofY = Mathf.Max(pSize.y, sqSize.y);
            if (roofY + bSize.y <= h)
            {
                PlaceBlockInstance(parent, beam, leftX, roofY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);
                if (rightX + pSize.x - bSize.x > leftX)
                    PlaceBlockInstance(parent, beam, rightX + pSize.x - bSize.x, roofY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);
            }
        }

        private static void BuildTwoTierTowerPuzzle(
            Transform parent, Demolition_ObstacleSpawner.BlockConfig pillar, Demolition_ObstacleSpawner.BlockConfig beam, Demolition_ObstacleSpawner.BlockConfig square,
            GameObject fantome, int w, int h, float cSize, int sub, Vector2Int fSize,
            bool[,] occ, CellDebugInfo[,] dGrid, List<PlacedBlockDebugInfo> dBlocks, List<FantomeDebugInfo> dFantomes, List<Rigidbody> rbs)
        {
            Vector2Int pSize = pillar.size * sub;
            Vector2Int bSize = beam.size * sub;

            int span = Mathf.Max(bSize.x, pSize.x * 2 + fSize.x);
            span = Mathf.Min(span, w);
            int startX = (w - span) / 2;

            // Étage 1
            PlaceBlockInstance(parent, pillar, startX, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);
            int rightX = startX + span - pSize.x;
            if (rightX > startX + pSize.x)
                PlaceBlockInstance(parent, pillar, rightX, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Fantôme 1 en bas
            int fX1 = startX + (span - fSize.x) / 2;
            if (fantome != null && CanPlace(occ, fX1, 0, fSize.x, fSize.y))
                PlaceFantomeInstance(parent, fantome, fX1, 0, fSize, w, cSize, occ, dGrid, dFantomes, rbs);

            // Toit Étage 1
            int tier1Y = pSize.y;
            PlaceBlockInstance(parent, beam, startX, tier1Y, bSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Étage 2 (plus étroit)
            int tier2PillarY = tier1Y + bSize.y;
            int inward = 1 * sub;
            int p2Left = startX + inward;
            int p2Right = startX + span - pSize.x - inward;

            if (tier2PillarY + pSize.y + bSize.y <= h && p2Right > p2Left)
            {
                PlaceBlockInstance(parent, pillar, p2Left, tier2PillarY, pSize, w, cSize, occ, dGrid, dBlocks, rbs);
                PlaceBlockInstance(parent, pillar, p2Right, tier2PillarY, pSize, w, cSize, occ, dGrid, dBlocks, rbs);

                int tier2RoofY = tier2PillarY + pSize.y;
                PlaceBlockInstance(parent, beam, startX, tier2RoofY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);

                // Fantôme 2 perché au sommet
                int fX2 = startX + (span - fSize.x) / 2;
                int fY2 = tier2RoofY + bSize.y;
                if (fantome != null && fY2 + fSize.y <= h && CanPlace(occ, fX2, fY2, fSize.x, fSize.y))
                    PlaceFantomeInstance(parent, fantome, fX2, fY2, fSize, w, cSize, occ, dGrid, dFantomes, rbs);
            }
        }

        private static void BuildCantileverPuzzle(
            Transform parent, Demolition_ObstacleSpawner.BlockConfig pillar, Demolition_ObstacleSpawner.BlockConfig beam, Demolition_ObstacleSpawner.BlockConfig square,
            GameObject fantome, int w, int h, float cSize, int sub, Vector2Int fSize,
            bool[,] occ, CellDebugInfo[,] dGrid, List<PlacedBlockDebugInfo> dBlocks, List<FantomeDebugInfo> dFantomes, List<Rigidbody> rbs)
        {
            Vector2Int pSize = pillar.size * sub;
            Vector2Int bSize = beam.size * sub;
            Vector2Int sqSize = square.size * sub;

            int span = Mathf.Max(bSize.x, pSize.x * 2 + fSize.x);
            span = Mathf.Min(span, w);
            int startX = (w - span) / 2;

            // Pivot décentré
            int pivotX = startX + 1 * sub;
            PlaceBlockInstance(parent, pillar, pivotX, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Poutre suspendue
            int beamY = pSize.y;
            PlaceBlockInstance(parent, beam, startX, beamY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Contrepoids à gauche
            int weightY = beamY + bSize.y;
            if (weightY + sqSize.y <= h)
            {
                PlaceBlockInstance(parent, square, startX, weightY, sqSize, w, cSize, occ, dGrid, dBlocks, rbs);
            }

            // Fantôme sous la partie suspendue à droite
            int fX = startX + span - fSize.x;
            if (fantome != null && CanPlace(occ, fX, 0, fSize.x, fSize.y))
                PlaceFantomeInstance(parent, fantome, fX, 0, fSize, w, cSize, occ, dGrid, dFantomes, rbs);
        }

        private static void BuildGrandCitadelPuzzle(
            Transform parent, Demolition_ObstacleSpawner.BlockConfig pillar, Demolition_ObstacleSpawner.BlockConfig beam, Demolition_ObstacleSpawner.BlockConfig square,
            GameObject fantome, int w, int h, float cSize, int sub, Vector2Int fSize,
            bool[,] occ, CellDebugInfo[,] dGrid, List<PlacedBlockDebugInfo> dBlocks, List<FantomeDebugInfo> dFantomes, List<Rigidbody> rbs)
        {
            Vector2Int pSize = pillar.size * sub;
            Vector2Int bSize = beam.size * sub;

            int span = Mathf.Min(w, bSize.x * 2 + 2 * sub);
            int startX = (w - span) / 2;

            int p1 = startX;
            int p2 = startX + span / 2 - pSize.x / 2;
            int p3 = startX + span - pSize.x;

            PlaceBlockInstance(parent, pillar, p1, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);
            PlaceBlockInstance(parent, pillar, p2, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);
            if (p3 > p2 + pSize.x)
                PlaceBlockInstance(parent, pillar, p3, 0, pSize, w, cSize, occ, dGrid, dBlocks, rbs);

            // Fantômes dans chaque chambre
            int fX1 = p1 + (p2 - p1 - fSize.x) / 2;
            if (fantome != null && CanPlace(occ, fX1, 0, fSize.x, fSize.y))
                PlaceFantomeInstance(parent, fantome, fX1, 0, fSize, w, cSize, occ, dGrid, dFantomes, rbs);

            int fX2 = p2 + pSize.x + 1;
            if (fantome != null && CanPlace(occ, fX2, 0, fSize.x, fSize.y))
                PlaceFantomeInstance(parent, fantome, fX2, 0, fSize, w, cSize, occ, dGrid, dFantomes, rbs);

            // Toiture
            int roofY = pSize.y;
            PlaceBlockInstance(parent, beam, p1, roofY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);
            if (p3 + pSize.x - bSize.x > p1)
                PlaceBlockInstance(parent, beam, p3 + pSize.x - bSize.x, roofY, bSize, w, cSize, occ, dGrid, dBlocks, rbs);
        }

        // =========================================================================================
        // HELPERS
        // =========================================================================================

        private static void PlaceBlockInstance(
            Transform parent, Demolition_ObstacleSpawner.BlockConfig config,
            int rx, int ry, Vector2Int internalSize,
            int totalGridW, float cellSize,
            bool[,] occupiedGrid, CellDebugInfo[,] debugGrid, List<PlacedBlockDebugInfo> debugBlocks, List<Rigidbody> rbs)
        {
            return;
            FillArea(occupiedGrid, rx, ry, internalSize, true);

            for (int x = 0; x < internalSize.x; x++)
                for (int y = 0; y < internalSize.y; y++)
                    debugGrid[rx + x, ry + y] = new CellDebugInfo { isFilled = true, isFantome = false };

            debugBlocks.Add(new PlacedBlockDebugInfo { position = new Vector2Int(rx, ry), size = internalSize });

            Vector3 localPos = new Vector3((rx + internalSize.x * 0.5f - totalGridW * 0.5f) * cellSize, ry * cellSize, 0f);
            GameObject prefabVariant = config.prefab[Random.Range(0, config.prefab.Length)];

            if (prefabVariant != null)
            {
                GameObject spawned = Object.Instantiate(prefabVariant, parent);
                spawned.transform.localPosition = localPos;

                if (spawned.TryGetComponent<Demolition_Pushable>(out var pushable))
                    spawned.transform.localRotation = Quaternion.Euler(pushable.spawnRotationOffset);
                else
                    spawned.transform.localRotation = Quaternion.identity;


            }
        }

        private static void PlaceFantomeInstance(
            Transform parent, GameObject fantomePrefab,
            int rx, int ry, Vector2Int internalSize,
            int totalGridW, float cellSize,
            bool[,] occupiedGrid, CellDebugInfo[,] debugGrid, List<FantomeDebugInfo> debugFantomes, List<Rigidbody> rbs)
        {
            FillArea(occupiedGrid, rx, ry, internalSize, true);

            for (int x = 0; x < internalSize.x; x++)
                for (int y = 0; y < internalSize.y; y++)
                    debugGrid[rx + x, ry + y] = new CellDebugInfo { isFilled = true, isFantome = true };

            debugFantomes.Add(new FantomeDebugInfo { position = new Vector2Int(rx, ry), size = internalSize });

            Vector3 localPos = new Vector3((rx + internalSize.x * 0.5f - totalGridW * 0.5f) * cellSize, ry * cellSize, 0f);
            GameObject spawned = Object.Instantiate(fantomePrefab, parent);
            spawned.transform.localPosition = localPos;
            spawned.transform.localRotation = fantomePrefab.transform.rotation;

            if (!spawned.GetComponent<Demolition_Fantome>())
                spawned.AddComponent<Demolition_Fantome>();

        }

        public static bool CanPlace(bool[,] grid, int startX, int startY, int sizeX, int sizeY)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            if (startX < 0 || startY < 0 || startX + sizeX > w || startY + sizeY > h)
                return false;

            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    if (grid[startX + x, startY + y]) return false;

            return true;
        }

        public static void FillArea(bool[,] grid, int startX, int startY, Vector2Int size, bool val)
        {
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    grid[startX + x, startY + y] = val;
        }

        public class StructureHost : MonoBehaviour
        {
            private List<Rigidbody> rbs;
            private CellDebugInfo[,] dGrid;
            private List<PlacedBlockDebugInfo> blocks;
            private List<FantomeDebugInfo> fantomes;
            private int w, h;
            private float cSize;

            public void Init(List<Rigidbody> rbs, CellDebugInfo[,] grid, List<PlacedBlockDebugInfo> blocks, List<FantomeDebugInfo> fantomes, int w, int h, float size)
            {
                this.rbs = rbs; 
                this.dGrid = grid; 
                this.blocks = blocks; 
                this.fantomes = fantomes;
                this.w = w; 
                this.h = h; 
                this.cSize = size;
            }

            private void OnDrawGizmos()
            {
                Gizmos.matrix = transform.localToWorldMatrix;

                if (dGrid != null)
                {
                    Vector3 sizeBox = new Vector3(cSize, cSize, 0.05f);
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            if (!dGrid[x, y].isFilled)
                            {
                                Vector3 pos = new Vector3((x + 0.5f - w * 0.5f) * cSize, (y + 0.5f) * cSize, 0f);
                                Gizmos.color = new Color(1f, 0f, 0f, 0.12f);
                                Gizmos.DrawCube(pos, sizeBox);
                            }
                        }
                    }
                }

                if (fantomes != null)
                {
                    foreach (var f in fantomes)
                    {
                        Vector3 sizeBox = new Vector3(f.size.x * cSize, f.size.y * cSize, 0.2f);
                        Vector3 pos = new Vector3((f.position.x + f.size.x * 0.5f - w * 0.5f) * cSize, (f.position.y + f.size.y * 0.5f) * cSize, 0f);
                        Gizmos.color = new Color(0f, 0.4f, 1f, 0.6f);
                        Gizmos.DrawCube(pos, sizeBox);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(pos, sizeBox);
                    }
                }

                if (blocks != null)
                {
                    foreach (var b in blocks)
                    {
                        Vector3 sizeBox = new Vector3(b.size.x * cSize, b.size.y * cSize, 0.2f);
                        Vector3 pos = new Vector3((b.position.x + b.size.x * 0.5f - w * 0.5f) * cSize, (b.position.y + b.size.y * 0.5f) * cSize, 0f);
                        Gizmos.color = new Color(0f, 0.9f, 0.3f, 0.5f);
                        Gizmos.DrawCube(pos, sizeBox);
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(pos, sizeBox);
                    }
                }

                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}
