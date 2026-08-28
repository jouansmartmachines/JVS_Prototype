using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    public class Demolition_StructureBuilder : MonoBehaviour
    {
        public static void BuildRandomStructure(Transform parent, Vector3 position)
        {
            int type = Random.Range(0, 5);

            GameObject blocBois = Resources.Load<GameObject>("Prefabs/Bloc_Bois");
            GameObject blocVerre = Resources.Load<GameObject>("Prefabs/Bloc_Verre");
            GameObject blocPierre = Resources.Load<GameObject>("Prefabs/Bloc_Pierre");
            GameObject cochon = Resources.Load<GameObject>("Prefabs/Cochon");
            GameObject cochonVert = Resources.Load<GameObject>("Prefabs/Cochon_Vert");
            GameObject cochonBleu = Resources.Load<GameObject>("Prefabs/Cochon_Bleu");

            if (blocBois == null) return;

            GameObject structureGO = new GameObject("Structure_" + type);
            structureGO.transform.SetParent(parent);
            structureGO.transform.localPosition = position;
            structureGO.AddComponent<Demolition_Structure>();

            List<GameObject> allBlocks = new List<GameObject>();

            // Creer la structure selon le type
            switch (type)
            {
                case 0: TourSimple(structureGO.transform, blocBois, blocVerre, blocPierre, cochon, cochonVert, allBlocks); break;
                case 1: Pyramide(structureGO.transform, blocBois, blocVerre, blocPierre, cochon, cochonVert, allBlocks); break;
                case 2: MurAvecCochons(structureGO.transform, blocBois, blocVerre, blocPierre, cochon, cochonBleu, allBlocks); break;
                case 3: DoubleTour(structureGO.transform, blocBois, blocVerre, blocPierre, cochon, cochonVert, cochonBleu, allBlocks); break;
                case 4: Chateau(structureGO.transform, blocBois, blocVerre, blocPierre, cochon, cochonVert, cochonBleu, allBlocks); break;
            }
        }

        // Connecte chaque bloc a son voisin le plus proche en dessous (ou a cote)
        static void ConnectNeighbors(List<GameObject> blocks)
        {
            if (blocks.Count < 2) return;
            for (int i = 0; i < blocks.Count; i++)
            {
                var rb = blocks[i].GetComponent<Rigidbody2D>();
                if (rb == null) continue;

                // Chercher le bloc le plus proche en dessous
                GameObject nearest = null;
                float nearestDist = 3f;
                for (int j = 0; j < blocks.Count; j++)
                {
                    if (i == j) continue;
                    float dy = blocks[j].transform.position.y - blocks[i].transform.position.y;
                    if (dy < 0 && Mathf.Abs(dy) < nearestDist)
                    {
                        float dx = Mathf.Abs(blocks[j].transform.position.x - blocks[i].transform.position.x);
                        if (dx < 1.2f)
                        {
                            nearestDist = Mathf.Abs(dy);
                            nearest = blocks[j];
                        }
                    }
                }
                if (nearest != null)
                {
                    var joint = blocks[i].AddComponent<FixedJoint2D>();
                    joint.connectedBody = nearest.GetComponent<Rigidbody2D>();
                    joint.breakForce = 300f;
                    joint.breakTorque = 300f;
                }
            }
        }

        static void TourSimple(Transform parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert, List<GameObject> allBlocks)
        {
            float y = 0;
            for (int i = 0; i < 4; i++)
            {
                GameObject prefab = (i == 0) ? pierre : ((i == 2) ? verre : bois);
                GameObject bloc = Object.Instantiate(prefab, parent);
                bloc.transform.localPosition = new Vector3(0, y, 0);
                allBlocks.Add(bloc);
                y += 1.0f;
            }
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent);
                c.transform.localPosition = new Vector3(0, y, 0);
                allBlocks.Add(c);
            }
            ConnectNeighbors(allBlocks);
        }

        static void Pyramide(Transform parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert, List<GameObject> allBlocks)
        {
            float startX = -1.0f;
            for (int i = 0; i < 3; i++)
            {
                GameObject prefab = (i == 1) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent);
                bloc.transform.localPosition = new Vector3(startX + i * 1.0f, 0, 0);
                allBlocks.Add(bloc);
            }
            for (int i = 0; i < 2; i++)
            {
                GameObject bloc = Object.Instantiate(verre, parent);
                bloc.transform.localPosition = new Vector3(startX + 0.5f + i * 1.0f, 1.0f, 0);
                allBlocks.Add(bloc);
            }
            if (cochonVert != null)
            {
                GameObject c = Object.Instantiate(cochonVert, parent);
                c.transform.localPosition = new Vector3(startX + 1.0f, 1.0f, 0);
                allBlocks.Add(c);
            }
            GameObject top = Object.Instantiate(bois, parent);
            top.transform.localPosition = new Vector3(startX + 1.0f, 2.0f, 0);
            allBlocks.Add(top);
            ConnectNeighbors(allBlocks);
        }

        static void MurAvecCochons(Transform parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonBleu, List<GameObject> allBlocks)
        {
            float startX = -2.0f;
            for (int i = 0; i < 5; i++)
            {
                GameObject prefab = (i == 0 || i == 4) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent);
                bloc.transform.localPosition = new Vector3(startX + i * 1.0f, 0, 0);
                allBlocks.Add(bloc);
            }
            for (int i = 0; i < 3; i++)
            {
                GameObject bloc = Object.Instantiate(verre, parent);
                bloc.transform.localPosition = new Vector3(startX + 1.0f + i * 1.0f, 1.0f, 0);
                allBlocks.Add(bloc);
            }
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent);
                c.transform.localPosition = new Vector3(startX + 2.0f, 1.0f, 0);
                allBlocks.Add(c);
            }
            if (cochonBleu != null)
            {
                GameObject c = Object.Instantiate(cochonBleu, parent);
                c.transform.localPosition = new Vector3(startX + 2.0f, 2.0f, 0);
                allBlocks.Add(c);
            }
            GameObject top = Object.Instantiate(bois, parent);
            top.transform.localPosition = new Vector3(startX + 2.0f, 2.0f, 0);
            allBlocks.Add(top);
            ConnectNeighbors(allBlocks);
        }

        static void DoubleTour(Transform parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert, GameObject cochonBleu, List<GameObject> allBlocks)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject prefab = (i == 0) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent);
                bloc.transform.localPosition = new Vector3(-1.5f, i * 1.0f, 0);
                allBlocks.Add(bloc);
            }
            for (int i = 0; i < 3; i++)
            {
                GameObject prefab = (i == 0) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent);
                bloc.transform.localPosition = new Vector3(1.5f, i * 1.0f, 0);
                allBlocks.Add(bloc);
            }
            for (int i = 0; i < 3; i++)
            {
                GameObject bloc = Object.Instantiate(verre, parent);
                bloc.transform.localPosition = new Vector3(-1.0f + i * 1.0f, 2.0f, 0);
                allBlocks.Add(bloc);
            }
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent);
                c.transform.localPosition = new Vector3(0, 3.0f, 0);
                allBlocks.Add(c);
            }
            if (cochonVert != null)
            {
                GameObject c = Object.Instantiate(cochonVert, parent);
                c.transform.localPosition = new Vector3(1.5f, 1.0f, 0);
                allBlocks.Add(c);
            }
            ConnectNeighbors(allBlocks);
        }

        static void Chateau(Transform parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert, GameObject cochonBleu, List<GameObject> allBlocks)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject prefab = (i == 0) ? pierre : (i == 3) ? verre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent);
                bloc.transform.localPosition = new Vector3(-2.0f, i * 1.0f, 0);
                allBlocks.Add(bloc);
            }
            for (int i = 0; i < 4; i++)
            {
                GameObject prefab = (i == 0) ? pierre : (i == 3) ? verre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent);
                bloc.transform.localPosition = new Vector3(2.0f, i * 1.0f, 0);
                allBlocks.Add(bloc);
            }
            for (int i = 0; i < 3; i++)
            {
                GameObject bloc = Object.Instantiate(pierre, parent);
                bloc.transform.localPosition = new Vector3(-1.0f + i * 1.0f, 4.0f, 0);
                allBlocks.Add(bloc);
            }
            if (cochonBleu != null)
            {
                GameObject c = Object.Instantiate(cochonBleu, parent);
                c.transform.localPosition = new Vector3(0, 5.0f, 0);
                allBlocks.Add(c);
            }
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent);
                c.transform.localPosition = new Vector3(-2.0f, 2.0f, 0);
                allBlocks.Add(c);
            }
            if (cochonVert != null)
            {
                GameObject c = Object.Instantiate(cochonVert, parent);
                c.transform.localPosition = new Vector3(2.0f, 2.0f, 0);
                allBlocks.Add(c);
            }
            ConnectNeighbors(allBlocks);
        }
    }
}