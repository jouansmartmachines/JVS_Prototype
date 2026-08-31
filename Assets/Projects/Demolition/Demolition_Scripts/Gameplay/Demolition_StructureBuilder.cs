using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    public class Demolition_StructureBuilder : MonoBehaviour
    {
        // Cree un bloc directement depuis Resources/Textures/
        static GameObject CreateBlock(string texName, Transform parent, Vector3 localPos, 
            Demolition_Block.MaterialType mat, int hp, int points, bool isTarget = false, int starVal = 0)
        {
            GameObject go = new GameObject(texName, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(AudioSource), typeof(Demolition_Block));
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;

            // Sprite
            var sr = go.GetComponent<SpriteRenderer>();
            Texture2D tex = Resources.Load<Texture2D>("Textures/" + texName);
            if (tex != null)
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sr.sortingOrder = 2;

            // Block component
            var blk = go.GetComponent<Demolition_Block>();
            blk.hp = hp;
            blk.points = points;
            blk.materialType = mat;
            blk.spriteRenderer = sr;
            blk.isTarget = isTarget;
            blk.starValue = starVal;

            return go;
        }

        static Sprite LoadSprite(string texName)
        {
            Texture2D tex = Resources.Load<Texture2D>("Textures/" + texName);
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            return null;
        }

        public static void BuildRandomStructure(Transform parent, Vector3 position)
        {
            int type = Random.Range(0, 5);

            // Parent de la structure (Kinematic pour servir de support aux blocs du bas)
            GameObject structureGO = new GameObject("Structure_" + type);
            structureGO.transform.SetParent(parent);
            structureGO.transform.localPosition = position;
            var rootRb = structureGO.AddComponent<Rigidbody2D>();
            rootRb.bodyType = RigidbodyType2D.Kinematic;
            structureGO.AddComponent<Demolition_Structure>();

            List<GameObject> allBlocks = new List<GameObject>();

            switch (type)
            {
                case 0: TourSimple(structureGO.transform, allBlocks); break;
                case 1: Pyramide(structureGO.transform, allBlocks); break;
                case 2: MurAvecCochons(structureGO.transform, allBlocks); break;
                case 3: DoubleTour(structureGO.transform, allBlocks); break;
                case 4: Chateau(structureGO.transform, allBlocks); break;
            }

            // Connecter chaque bloc a son support (soit un bloc en dessous, soit le parent)
            ConnectNeighbors(allBlocks, structureGO.GetComponent<Rigidbody2D>());
        }

        static void ConnectNeighbors(List<GameObject> blocks, Rigidbody2D rootRb)
        {
            foreach (var block in blocks)
            {
                var rb = block.GetComponent<Rigidbody2D>();
                if (rb == null) continue;

                // Chercher le bloc le plus proche en dessous
                GameObject below = null;
                float minDist = 3f;
                Vector3 pos = block.transform.position;

                foreach (var other in blocks)
                {
                    if (other == block) continue;
                    float dy = pos.y - other.transform.position.y;
                    // other est en dessous ?
                    if (dy > 0 && dy < minDist)
                    {
                        float dx = Mathf.Abs(pos.x - other.transform.position.x);
                        if (dx < 1.5f)
                        {
                            minDist = dy;
                            below = other;
                        }
                    }
                }

                Rigidbody2D connectTo = below != null ? below.GetComponent<Rigidbody2D>() : rootRb;
                var joint = block.AddComponent<FixedJoint2D>();
                joint.connectedBody = connectTo;
                joint.breakForce = 300f;
                joint.breakTorque = 300f;
            }
        }

        static void TourSimple(Transform parent, List<GameObject> allBlocks)
        {
            float y = 0;
            for (int i = 0; i < 4; i++)
            {
                string tex = (i == 0) ? "pierre" : ((i == 2) ? "verre" : "bois");
                int hp = (i == 0) ? 4 : ((i == 2) ? 1 : 2);
                int pts = (i == 0) ? 20 : ((i == 2) ? 100 : 50);
                allBlocks.Add(CreateBlock(tex, parent, new Vector3(0, y, 0), 
                    (Demolition_Block.MaterialType)i, hp, pts));
                y += 1.0f;
            }
            // Cochon au sommet
            allBlocks.Add(CreateBlock("cochon", parent, new Vector3(0, y, 0), 
                Demolition_Block.MaterialType.Cochon, 3, 500, true, 1));
        }

        static void Pyramide(Transform parent, List<GameObject> allBlocks)
        {
            float startX = -1.0f;
            for (int i = 0; i < 3; i++)
            {
                string tex = (i == 1) ? "pierre" : "bois";
                int hp = (i == 1) ? 4 : 2;
                allBlocks.Add(CreateBlock(tex, parent, new Vector3(startX + i * 1.0f, 0, 0), 
                    Demolition_Block.MaterialType.Bois, hp, 50));
            }
            for (int i = 0; i < 2; i++)
            {
                allBlocks.Add(CreateBlock("verre", parent, new Vector3(startX + 0.5f + i * 1.0f, 1.0f, 0), 
                    Demolition_Block.MaterialType.Verre, 1, 100));
            }
            // Cochon vert au milieu
            allBlocks.Add(CreateBlock("cochon_vert", parent, new Vector3(startX + 1.0f, 1.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 5, 1000, true, 2));
            // Sommet
            allBlocks.Add(CreateBlock("bois", parent, new Vector3(startX + 1.0f, 2.0f, 0), 
                Demolition_Block.MaterialType.Bois, 2, 50));
        }

        static void MurAvecCochons(Transform parent, List<GameObject> allBlocks)
        {
            float startX = -2.0f;
            for (int i = 0; i < 5; i++)
            {
                string tex = (i == 0 || i == 4) ? "pierre" : "bois";
                int hp = (i == 0 || i == 4) ? 4 : 2;
                allBlocks.Add(CreateBlock(tex, parent, new Vector3(startX + i * 1.0f, 0, 0), 
                    Demolition_Block.MaterialType.Bois, hp, 50));
            }
            for (int i = 0; i < 3; i++)
            {
                allBlocks.Add(CreateBlock("verre", parent, new Vector3(startX + 1.0f + i * 1.0f, 1.0f, 0), 
                    Demolition_Block.MaterialType.Verre, 1, 100));
            }
            // Cochon rose au centre
            allBlocks.Add(CreateBlock("cochon", parent, new Vector3(startX + 2.0f, 1.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 3, 500, true, 1));
            // Cochon bleu en haut
            allBlocks.Add(CreateBlock("cochon_bleu", parent, new Vector3(startX + 2.0f, 2.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 8, 2000, true, 3));
            allBlocks.Add(CreateBlock("bois", parent, new Vector3(startX + 2.0f, 2.0f, 0), 
                Demolition_Block.MaterialType.Bois, 2, 50));
        }

        static void DoubleTour(Transform parent, List<GameObject> allBlocks)
        {
            for (int i = 0; i < 3; i++)
            {
                string tex = (i == 0) ? "pierre" : "bois";
                int hp = (i == 0) ? 4 : 2;
                allBlocks.Add(CreateBlock(tex, parent, new Vector3(-1.5f, i * 1.0f, 0), 
                    Demolition_Block.MaterialType.Bois, hp, 50));
            }
            for (int i = 0; i < 3; i++)
            {
                string tex = (i == 0) ? "pierre" : "bois";
                int hp = (i == 0) ? 4 : 2;
                allBlocks.Add(CreateBlock(tex, parent, new Vector3(1.5f, i * 1.0f, 0), 
                    Demolition_Block.MaterialType.Bois, hp, 50));
            }
            for (int i = 0; i < 3; i++)
            {
                allBlocks.Add(CreateBlock("verre", parent, new Vector3(-1.0f + i * 1.0f, 2.0f, 0), 
                    Demolition_Block.MaterialType.Verre, 1, 100));
            }
            // Cochon sur le pont
            allBlocks.Add(CreateBlock("cochon", parent, new Vector3(0, 3.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 3, 500, true, 1));
            // Cochon vert dans la tour droite
            allBlocks.Add(CreateBlock("cochon_vert", parent, new Vector3(1.5f, 1.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 5, 1000, true, 2));
        }

        static void Chateau(Transform parent, List<GameObject> allBlocks)
        {
            for (int i = 0; i < 4; i++)
            {
                string tex = (i == 0) ? "pierre" : (i == 3) ? "verre" : "bois";
                int hp = (i == 0) ? 4 : (i == 3) ? 1 : 2;
                allBlocks.Add(CreateBlock(tex, parent, new Vector3(-2.0f, i * 1.0f, 0), 
                    Demolition_Block.MaterialType.Bois, hp, 50));
            }
            for (int i = 0; i < 4; i++)
            {
                string tex = (i == 0) ? "pierre" : (i == 3) ? "verre" : "bois";
                int hp = (i == 0) ? 4 : (i == 3) ? 1 : 2;
                allBlocks.Add(CreateBlock(tex, parent, new Vector3(2.0f, i * 1.0f, 0), 
                    Demolition_Block.MaterialType.Bois, hp, 50));
            }
            for (int i = 0; i < 3; i++)
            {
                allBlocks.Add(CreateBlock("pierre", parent, new Vector3(-1.0f + i * 1.0f, 4.0f, 0), 
                    Demolition_Block.MaterialType.Pierre, 4, 20));
            }
            // Cochon bleu au sommet
            allBlocks.Add(CreateBlock("cochon_bleu", parent, new Vector3(0, 5.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 8, 2000, true, 3));
            // Cochon rose dans la tour gauche
            allBlocks.Add(CreateBlock("cochon", parent, new Vector3(-2.0f, 2.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 3, 500, true, 1));
            // Cochon vert dans la tour droite
            allBlocks.Add(CreateBlock("cochon_vert", parent, new Vector3(2.0f, 2.0f, 0), 
                Demolition_Block.MaterialType.Cochon, 5, 1000, true, 2));
        }
    }
}