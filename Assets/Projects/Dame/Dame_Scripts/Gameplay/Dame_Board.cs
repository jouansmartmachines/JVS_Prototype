using UnityEngine;
using System.Collections.Generic;

namespace Dame
{
    public class Dame_Board : MonoBehaviour
    {
        public int size { get; private set; }
        private Dame_Cell[,] cells;

        public void InitializeBoard(int boardSize)
        {
            size = boardSize;
            cells = new Dame_Cell[size, size];

            float cellSize = 0.8f;
            float offset = (size - 1) * cellSize / 2f;

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    bool isDark = (r + c) % 2 == 1;

                    var cellGO = new GameObject($"Cell_{r}_{c}", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Dame_Cell));
                    cellGO.transform.SetParent(transform);
                    float x = c * cellSize - offset;
                    float y = (size - 1 - r) * cellSize - offset;
                    cellGO.transform.position = new Vector3(x, y, 0);
                    cellGO.transform.localScale = Vector3.one * cellSize;

                    var sr = cellGO.GetComponent<SpriteRenderer>();
                    Texture2D tex = Resources.Load<Texture2D>(isDark ? "Textures/case_foncee" : "Textures/case_claire");
                    if (tex != null)
                        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    sr.sortingOrder = 0;

                    var col = cellGO.GetComponent<BoxCollider2D>();
                    col.size = Vector2.one;

                    var cell = cellGO.GetComponent<Dame_Cell>();
                    cell.Init(r, c, isDark, this);

                    cells[r, c] = cell;
                }
            }

            PlaceInitialPieces();
        }

        void PlaceInitialPieces()
        {
            for (int r = 6; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if ((r + c) % 2 == 1)
                        CreatePiece(cells[r, c], 1);
                }
            }
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if ((r + c) % 2 == 1)
                        CreatePiece(cells[r, c], 2);
                }
            }
        }

        void CreatePiece(Dame_Cell cell, int player)
        {
            var pieceGO = new GameObject(player == 1 ? "PionBlanc" : "PionNoir", typeof(SpriteRenderer), typeof(Dame_Piece));
            pieceGO.transform.SetParent(transform);
            pieceGO.transform.position = cell.transform.position;
            pieceGO.transform.localScale = Vector3.one * 0.7f;

            var sr = pieceGO.GetComponent<SpriteRenderer>();
            Texture2D tex = Resources.Load<Texture2D>(player == 1 ? "Textures/pion_blanc" : "Textures/pion_noir");
            if (tex != null)
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sr.sortingOrder = 1;

            var piece = pieceGO.GetComponent<Dame_Piece>();
            piece.Init(player, cell, sr);

            cell.SetPiece(piece);
        }

        public Dame_Cell GetCell(int row, int col)
        {
            if (row < 0 || row >= size || col < 0 || col >= size) return null;
            return cells[row, col];
        }

        public Dame_Cell GetCellAtWorldPos(Vector3 pos)
        {
            float cellSize = 0.8f;
            float offset = (size - 1) * cellSize / 2f;

            int col = Mathf.RoundToInt((pos.x + offset) / cellSize);
            int row = Mathf.RoundToInt((size - 1 - (pos.y + offset) / cellSize));

            return GetCell(row, col);
        }

        public void ClearHighlights()
        {
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    cells[r, c].SetHighlight(false);
        }

        public void ShowValidMoves(List<Dame_Cell> moves)
        {
            foreach (var cell in moves)
            {
                if (cell != null)
                    cell.SetValidMove(true);
            }
        }
    }
}