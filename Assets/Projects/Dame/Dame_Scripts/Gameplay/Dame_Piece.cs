using UnityEngine;

namespace Dame
{
    public class Dame_Piece : MonoBehaviour
    {
        public int playerNumber { get; private set; }
        public bool isCrowned { get; private set; }
        public Dame_Cell currentCell { get; set; }
        private SpriteRenderer spriteRenderer;

        public void Init(int player, Dame_Cell cell, SpriteRenderer sr)
        {
            playerNumber = player;
            currentCell = cell;
            spriteRenderer = sr;
            isCrowned = false;
        }

        public void Crown()
        {
            isCrowned = true;
            Texture2D tex = Resources.Load<Texture2D>(playerNumber == 1 ? "Textures/dame_blanche" : "Textures/dame_noire");
            if (tex != null)
                spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}