using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Génère des structures complexes type Angry Birds :
    /// pyramides, tours, murs avec cochons intégrés.
    /// </summary>
    public class Demolition_StructureBuilder : MonoBehaviour
    {
        public static void BuildRandomStructure(Transform parent, Vector3 position)
        {
            int type = Random.Range(0, 5);

            // Charger les prefabs de blocs
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

            // Ajouter un Rigidbody2D à la racine pour que la structure bouge
            var rb = structureGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var structure = structureGO.AddComponent<Demolition_Structure>();

            // Créer la structure selon le type
            switch (type)
            {
                case 0: TourSimple(structureGO, blocBois, blocVerre, blocPierre, cochon, cochonVert); break;
                case 1: Pyramide(structureGO, blocBois, blocVerre, blocPierre, cochon, cochonVert); break;
                case 2: MurAvecCochons(structureGO, blocBois, blocVerre, blocPierre, cochon, cochonBleu); break;
                case 3: DoubleTour(structureGO, blocBois, blocVerre, blocPierre, cochon, cochonVert, cochonBleu); break;
                case 4: Chateau(structureGO, blocBois, blocVerre, blocPierre, cochon, cochonVert, cochonBleu); break;
            }
        }

        static void TourSimple(GameObject parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert)
        {
            // Tour de 4 blocs empilés
            float y = 0;
            for (int i = 0; i < 4; i++)
            {
                GameObject prefab = (i == 0) ? pierre : ((i == 2) ? verre : bois);
                GameObject bloc = Object.Instantiate(prefab, parent.transform);
                bloc.transform.localPosition = new Vector3(0, y, 0);
                y += 1.0f;
            }
            // Cochon au sommet
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent.transform);
                c.transform.localPosition = new Vector3(0, y, 0);
            }
        }

        static void Pyramide(GameObject parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert)
        {
            // 3 blocs en bas, 2 au milieu, 1 au sommet, cochon au milieu
            float startX = -1.0f;
            // Base: 3 blocs
            for (int i = 0; i < 3; i++)
            {
                GameObject prefab = (i == 1) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent.transform);
                bloc.transform.localPosition = new Vector3(startX + i * 1.0f, 0, 0);
            }
            // Milieu: 2 blocs + cochon
            for (int i = 0; i < 2; i++)
            {
                GameObject bloc = Object.Instantiate(verre, parent.transform);
                bloc.transform.localPosition = new Vector3(startX + 0.5f + i * 1.0f, 1.0f, 0);
            }
            // Cochon vert au milieu
            if (cochonVert != null)
            {
                GameObject c = Object.Instantiate(cochonVert, parent.transform);
                c.transform.localPosition = new Vector3(startX + 1.0f, 1.0f, 0);
            }
            // Sommet: 1 bloc
            GameObject top = Object.Instantiate(bois, parent.transform);
            top.transform.localPosition = new Vector3(startX + 1.0f, 2.0f, 0);
        }

        static void MurAvecCochons(GameObject parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonBleu)
        {
            // Mur de 5 blocs de large avec 2 cochons
            float startX = -2.0f;
            for (int i = 0; i < 5; i++)
            {
                GameObject prefab = (i == 0 || i == 4) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent.transform);
                bloc.transform.localPosition = new Vector3(startX + i * 1.0f, 0, 0);
            }
            // 2e rangée: 3 blocs + cochon
            for (int i = 0; i < 3; i++)
            {
                GameObject bloc = Object.Instantiate(verre, parent.transform);
                bloc.transform.localPosition = new Vector3(startX + 1.0f + i * 1.0f, 1.0f, 0);
            }
            // Cochon au centre
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent.transform);
                c.transform.localPosition = new Vector3(startX + 2.0f, 1.0f, 0);
            }
            // 3e rangée: 1 bloc + cochon bleu
            if (cochonBleu != null)
            {
                GameObject c = Object.Instantiate(cochonBleu, parent.transform);
                c.transform.localPosition = new Vector3(startX + 2.0f, 2.0f, 0);
            }
            GameObject top = Object.Instantiate(bois, parent.transform);
            top.transform.localPosition = new Vector3(startX + 2.0f, 2.0f, 0);
        }

        static void DoubleTour(GameObject parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert, GameObject cochonBleu)
        {
            // Deux tours reliées par un pont
            // Tour gauche: 3 blocs
            for (int i = 0; i < 3; i++)
            {
                GameObject prefab = (i == 0) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent.transform);
                bloc.transform.localPosition = new Vector3(-1.5f, i * 1.0f, 0);
            }
            // Tour droite: 3 blocs
            for (int i = 0; i < 3; i++)
            {
                GameObject prefab = (i == 0) ? pierre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent.transform);
                bloc.transform.localPosition = new Vector3(1.5f, i * 1.0f, 0);
            }
            // Pont: 3 blocs de verre
            for (int i = 0; i < 3; i++)
            {
                GameObject bloc = Object.Instantiate(verre, parent.transform);
                bloc.transform.localPosition = new Vector3(-1.0f + i * 1.0f, 2.0f, 0);
            }
            // Cochon sur le pont
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent.transform);
                c.transform.localPosition = new Vector3(0, 3.0f, 0);
            }
            // Cochon vert dans la tour droite
            if (cochonVert != null)
            {
                GameObject c = Object.Instantiate(cochonVert, parent.transform);
                c.transform.localPosition = new Vector3(1.5f, 1.0f, 0);
            }
        }

        static void Chateau(GameObject parent, GameObject bois, GameObject verre, GameObject pierre,
            GameObject cochon, GameObject cochonVert, GameObject cochonBleu)
        {
            // Structure complexe: 2 murs lateraux + toit + cochons
            // Mur gauche: 4 blocs
            for (int i = 0; i < 4; i++)
            {
                GameObject prefab = (i == 0) ? pierre : (i == 3) ? verre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent.transform);
                bloc.transform.localPosition = new Vector3(-2.0f, i * 1.0f, 0);
            }
            // Mur droit: 4 blocs
            for (int i = 0; i < 4; i++)
            {
                GameObject prefab = (i == 0) ? pierre : (i == 3) ? verre : bois;
                GameObject bloc = Object.Instantiate(prefab, parent.transform);
                bloc.transform.localPosition = new Vector3(2.0f, i * 1.0f, 0);
            }
            // Toit: 3 blocs
            for (int i = 0; i < 3; i++)
            {
                GameObject bloc = Object.Instantiate(pierre, parent.transform);
                bloc.transform.localPosition = new Vector3(-1.0f + i * 1.0f, 4.0f, 0);
            }
            // Cochon bleu au sommet
            if (cochonBleu != null)
            {
                GameObject c = Object.Instantiate(cochonBleu, parent.transform);
                c.transform.localPosition = new Vector3(0, 5.0f, 0);
            }
            // Cochon rose dans la tour gauche
            if (cochon != null)
            {
                GameObject c = Object.Instantiate(cochon, parent.transform);
                c.transform.localPosition = new Vector3(-2.0f, 2.0f, 0);
            }
            // Cochon vert dans la tour droite
            if (cochonVert != null)
            {
                GameObject c = Object.Instantiate(cochonVert, parent.transform);
                c.transform.localPosition = new Vector3(2.0f, 2.0f, 0);
            }
        }
    }
}