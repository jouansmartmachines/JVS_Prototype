using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    /// <summary>
    /// Générateur procédural de structures physiques destructibles façon Angry Birds.
    /// Les blocs sont positionnés au millimètre près sur le sol sans vide ni chevauchement.
    /// </summary>
    public class Demolition_StructureBuilder : MonoBehaviour
    {
        public static GameObject CreateBlock(
            string texName,
            Transform parent,
            Vector3 localPos,
            Demolition_Block.MaterialType mat,
            Vector2 size,
            bool isTarget = false,
            int starVal = 1)
        {
            GameObject go = new GameObject(texName);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;

            var sr = go.AddComponent<SpriteRenderer>();
            Texture2D tex = Resources.Load<Texture2D>("Textures/" + texName);
            if (tex != null)
            {
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var aud = go.AddComponent<AudioSource>();
            aud.playOnAwake = false;
            aud.spatialBlend = 0f;

            var blk = go.AddComponent<Demolition_Block>();
            blk.materialType = mat;
            blk.spriteRenderer = sr;
            blk.isTarget = isTarget;
            blk.starValue = starVal;

            return go;
        }

        public static void BuildRandomStructure(Transform parent, Vector3 position)
        {
            int type = Random.Range(0, 5);

            GameObject structureGO = new GameObject("Structure_" + type);
            structureGO.transform.SetParent(parent);
            structureGO.transform.position = position;

            var rootRb = structureGO.AddComponent<Rigidbody2D>();
            rootRb.bodyType = RigidbodyType2D.Kinematic;
            structureGO.AddComponent<Demolition_Structure>();

            List<GameObject> allBlocks = new List<GameObject>();

            switch (type)
            {
                case 0: BuildTower(structureGO.transform, allBlocks); break;
                case 1: BuildPyramid(structureGO.transform, allBlocks); break;
                case 2: BuildCastle(structureGO.transform, allBlocks); break;
                case 3: BuildBridge(structureGO.transform, allBlocks); break;
                case 4: BuildBunker(structureGO.transform, allBlocks); break;
            }
        }

        private static void BuildTower(Transform parent, List<GameObject> blocks)
        {
            float currentY = 0f;
            for (int i = 0; i < 3; i++)
            {
                var mat = (i == 0) ? Demolition_Block.MaterialType.Pierre : ((i % 2 == 1) ? Demolition_Block.MaterialType.Bois : Demolition_Block.MaterialType.Verre);
                string tex = mat == Demolition_Block.MaterialType.Pierre ? "pierre" : (mat == Demolition_Block.MaterialType.Verre ? "verre" : "bois");

                float pillarH = 0.95f;
                float pillarY = currentY + pillarH * 0.5f;

                blocks.Add(CreateBlock(tex, parent, new Vector3(-0.65f, pillarY, 0), mat, new Vector2(0.35f, pillarH)));
                blocks.Add(CreateBlock(tex, parent, new Vector3(0.65f, pillarY, 0), mat, new Vector2(0.35f, pillarH)));

                currentY += pillarH;

                float beamH = 0.25f;
                float beamY = currentY + beamH * 0.5f;
                blocks.Add(CreateBlock("bois", parent, new Vector3(0, beamY, 0), Demolition_Block.MaterialType.Bois, new Vector2(1.8f, beamH)));

                currentY += beamH;
            }

            float pigH = 0.75f;
            float pigY = currentY + pigH * 0.5f;
            blocks.Add(CreateBlock("cochon", parent, new Vector3(0, pigY, 0), Demolition_Block.MaterialType.Cochon, new Vector2(pigH, pigH), true, 1));
        }

        private static void BuildPyramid(Transform parent, List<GameObject> blocks)
        {
            float currentY = 0f;
            float basePillarH = 0.95f;
            float basePillarY = currentY + basePillarH * 0.5f;

            blocks.Add(CreateBlock("pierre", parent, new Vector3(-1.3f, basePillarY, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.45f, basePillarH)));
            blocks.Add(CreateBlock("pierre", parent, new Vector3(0f, basePillarY, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.45f, basePillarH)));
            blocks.Add(CreateBlock("pierre", parent, new Vector3(1.3f, basePillarY, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.45f, basePillarH)));

            currentY += basePillarH;

            float beam1H = 0.25f;
            float beam1Y = currentY + beam1H * 0.5f;
            blocks.Add(CreateBlock("bois", parent, new Vector3(0, beam1Y, 0), Demolition_Block.MaterialType.Bois, new Vector2(3.2f, beam1H)));

            currentY += beam1H;

            float midPillarH = 0.85f;
            float midPillarY = currentY + midPillarH * 0.5f;

            blocks.Add(CreateBlock("verre", parent, new Vector3(-0.65f, midPillarY, 0), Demolition_Block.MaterialType.Verre, new Vector2(0.35f, midPillarH)));
            blocks.Add(CreateBlock("verre", parent, new Vector3(0.65f, midPillarY, 0), Demolition_Block.MaterialType.Verre, new Vector2(0.35f, midPillarH)));
            blocks.Add(CreateBlock("cochon_vert", parent, new Vector3(0, midPillarY, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.65f, 0.65f), true, 2));

            currentY += midPillarH;

            float beam2H = 0.25f;
            float beam2Y = currentY + beam2H * 0.5f;
            blocks.Add(CreateBlock("bois", parent, new Vector3(0, beam2Y, 0), Demolition_Block.MaterialType.Bois, new Vector2(1.9f, beam2H)));

            currentY += beam2H;

            float topPigH = 0.65f;
            float topPigY = currentY + topPigH * 0.5f;
            blocks.Add(CreateBlock("cochon", parent, new Vector3(0, topPigY, 0), Demolition_Block.MaterialType.Cochon, new Vector2(topPigH, topPigH), true, 1));
        }

        private static void BuildCastle(Transform parent, List<GameObject> blocks)
        {
            float p1H = 1.0f;
            blocks.Add(CreateBlock("pierre", parent, new Vector3(-1.6f, 0.5f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.5f, p1H)));
            blocks.Add(CreateBlock("pierre", parent, new Vector3(1.6f, 0.5f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.5f, p1H)));

            blocks.Add(CreateBlock("verre", parent, new Vector3(0, 1.125f, 0), Demolition_Block.MaterialType.Verre, new Vector2(2.6f, 0.25f)));

            blocks.Add(CreateBlock("pierre", parent, new Vector3(-1.6f, 1.75f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.5f, p1H)));
            blocks.Add(CreateBlock("pierre", parent, new Vector3(1.6f, 1.75f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.5f, p1H)));

            blocks.Add(CreateBlock("bois", parent, new Vector3(0, 2.375f, 0), Demolition_Block.MaterialType.Bois, new Vector2(2.6f, 0.25f)));

            blocks.Add(CreateBlock("cochon", parent, new Vector3(-1.6f, 2.6f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.65f, 0.65f), true, 1));
            blocks.Add(CreateBlock("cochon_vert", parent, new Vector3(1.6f, 2.6f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.65f, 0.65f), true, 2));
            blocks.Add(CreateBlock("cochon_bleu", parent, new Vector3(0, 1.7f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.8f, 0.8f), true, 3));
        }

        private static void BuildBridge(Transform parent, List<GameObject> blocks)
        {
            float[] xs = { -2.0f, -0.65f, 0.65f, 2.0f };
            float pH = 1.0f;
            for (int i = 0; i < xs.Length; i++)
            {
                blocks.Add(CreateBlock("bois", parent, new Vector3(xs[i], 0.5f, 0), Demolition_Block.MaterialType.Bois, new Vector2(0.35f, pH)));
            }

            blocks.Add(CreateBlock("pierre", parent, new Vector3(0, 1.14f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(4.6f, 0.28f)));

            blocks.Add(CreateBlock("verre", parent, new Vector3(-1.0f, 1.63f, 0), Demolition_Block.MaterialType.Verre, new Vector2(0.35f, 0.7f)));
            blocks.Add(CreateBlock("verre", parent, new Vector3(1.0f, 1.63f, 0), Demolition_Block.MaterialType.Verre, new Vector2(0.35f, 0.7f)));
            blocks.Add(CreateBlock("cochon_vert", parent, new Vector3(0, 1.6f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.65f, 0.65f), true, 2));

            blocks.Add(CreateBlock("bois", parent, new Vector3(0, 2.09f, 0), Demolition_Block.MaterialType.Bois, new Vector2(2.6f, 0.22f)));

            blocks.Add(CreateBlock("cochon", parent, new Vector3(-1.0f, 2.5f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.6f, 0.6f), true, 1));
            blocks.Add(CreateBlock("cochon", parent, new Vector3(1.0f, 2.5f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.6f, 0.6f), true, 1));
        }

        private static void BuildBunker(Transform parent, List<GameObject> blocks)
        {
            blocks.Add(CreateBlock("pierre", parent, new Vector3(-1.4f, 0.525f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.55f, 1.05f)));
            blocks.Add(CreateBlock("pierre", parent, new Vector3(1.4f, 0.525f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(0.55f, 1.05f)));
            blocks.Add(CreateBlock("verre", parent, new Vector3(0, 0.525f, 0), Demolition_Block.MaterialType.Verre, new Vector2(1.6f, 0.8f)));
            blocks.Add(CreateBlock("cochon_bleu", parent, new Vector3(0, 0.525f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.75f, 0.75f), true, 3));

            blocks.Add(CreateBlock("pierre", parent, new Vector3(0, 1.225f, 0), Demolition_Block.MaterialType.Pierre, new Vector2(3.4f, 0.35f)));

            blocks.Add(CreateBlock("bois", parent, new Vector3(0, 1.675f, 0), Demolition_Block.MaterialType.Bois, new Vector2(0.45f, 0.55f)));
            blocks.Add(CreateBlock("cochon", parent, new Vector3(0, 2.25f, 0), Demolition_Block.MaterialType.Cochon, new Vector2(0.6f, 0.6f), true, 1));
        }
    }
}
