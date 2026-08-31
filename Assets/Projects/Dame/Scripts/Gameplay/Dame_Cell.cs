using UnityEngine;
using System.Collections.Generic;
using Tool;

namespace Dame
{
    public class Dame_Cell : Universal_Collider2DButton
    {
        public int row { get; private set; }
        public int col { get; private set; }
        public bool isDark { get; private set; }
        public Dame_Board board { get; private set; }

        private Dame_Piece piece;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        public void Init(int r, int c, bool dark, Dame_Board b)
        {
            row = r;
            col = c;
            isDark = dark;
            board = b;
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;

            // Configurer le Collider2D pour les touches
            var col2d = GetComponent<BoxCollider2D>();
            if (col2d == null) col2d = gameObject.AddComponent<BoxCollider2D>();
            col2d.size = Vector2.one;

            IsActive = true;
        }

        public override void ReceivePoint(float xPoint, float yPoint)
        {
            xPoint *= Screen.width;
            yPoint *= Screen.height;
            Vector2 hit = new Vector2(xPoint, yPoint);
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(hit.x, hit.y, -Camera.main.transform.position.z));
            pos.z = 0;

            Collider2D col = GetComponent<Collider2D>();
            if (ToolBox.CheckPos(pos, col) && IsActive)
            {
                if (Dame_GameManager.Instance != null)
                    Dame_GameManager.Instance.OnCellTouched(this);
            }
        }

        public Dame_Piece GetPiece() => piece;

        public void SetPiece(Dame_Piece p)
        {
            piece = p;
        }

        public void SetHighlight(bool highlight)
        {
            if (highlight)
                spriteRenderer.color = new Color(0.3f, 0.8f, 0.3f, 1f);
            else
                spriteRenderer.color = originalColor;
        }

        public void SetValidMove(bool valid)
        {
            if (valid)
                spriteRenderer.color = new Color(0.6f, 1f, 0.6f, 0.8f);
            else
                spriteRenderer.color = originalColor;
        }

        public List<Dame_Cell> GetValidMoves()
        {
            var moves = new List<Dame_Cell>();
            if (piece == null) return moves;

            int dir = piece.playerNumber == 1 ? -1 : 1;
            int[] dirs = piece.isCrowned ? new int[] { -1, 1 } : new int[] { dir };

            foreach (int d in dirs)
            {
                CheckMove(moves, row + d, col - 1);
                CheckMove(moves, row + d, col + 1);
            }
            return moves;
        }

        void CheckMove(List<Dame_Cell> moves, int r, int c)
        {
            var cell = board.GetCell(r, c);
            if (cell != null && cell.GetPiece() == null)
                moves.Add(cell);
        }

        public List<Dame_Cell> GetValidCaptures()
        {
            var captures = new List<Dame_Cell>();
            if (piece == null) return captures;

            int dir = piece.playerNumber == 1 ? -1 : 1;
            int[] dirs = piece.isCrowned ? new int[] { -1, 1 } : new int[] { dir };

            foreach (int d in dirs)
            {
                CheckCapture(captures, row + d, col - 1, row + 2 * d, col - 2);
                CheckCapture(captures, row + d, col + 1, row + 2 * d, col + 2);
            }
            return captures;
        }

        void CheckCapture(List<Dame_Cell> captures, int enemyR, int enemyC, int landR, int landC)
        {
            var enemyCell = board.GetCell(enemyR, enemyC);
            var landCell = board.GetCell(landR, landC);

            if (enemyCell == null || landCell == null) return;

            var enemyPiece = enemyCell.GetPiece();
            if (enemyPiece == null) return;
            if (enemyPiece.playerNumber == piece.playerNumber) return;
            if (landCell.GetPiece() != null) return;

            captures.Add(landCell);
        }
    }
}