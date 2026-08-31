using UnityEngine;

namespace Dame
{
    public class Dame_Piece : MonoBehaviour
    {
        public int playerNumber { get; private set; }
        public bool isCrowned { get; private set; }
        public Dame_Cell currentCell { get; set; }
        private SpriteRenderer spriteRenderer;
        private Sprite crownSprite;

        public void Init(int player, Dame_Cell cell, SpriteRenderer sr, Sprite dameSprite)
        {
            playerNumber = player;
            currentCell = cell;
            spriteRenderer = sr;
            crownSprite = dameSprite;
            isCrowned = false;
        }

        public void Crown()
        {
            isCrowned = true;
            if (crownSprite != null)
                spriteRenderer.sprite = crownSprite;
        }
    }
}