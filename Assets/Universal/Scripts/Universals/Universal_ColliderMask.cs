using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tool;
using UnityEngine.Events;
using Olou;

public class Universal_ColliderMask : Universal_Button
{
    [SerializeField] private bool usePriority = false;
    [SerializeField] private int priority = 0;
    [SerializeField] private string interactableParentName = "ParentInteractable";

    [Header("Collider 2D Mode")]
    [SerializeField] private bool useCollider2DMask = false;

    [Header("Collider 3D Mode")]
    [SerializeField] private bool useCollider3DMask = false;

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        //Debug.Log($"<color=cyan>[ReceivePoint]</color> Input Reçu: x={xPoint}, y={yPoint} sur {gameObject.name}");

        float screenX = xPoint * Screen.width;
        float screenY = yPoint * Screen.height;
        Vector2 hit = new Vector2(screenX, screenY);

        Vector3 pos;

        if (useCollider2DMask)
        {
            // --- Logique Collider2D ---
            Canvas parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
            {
                float distanceZ = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
                pos = Camera.main.ScreenToWorldPoint(new Vector3(hit.x, hit.y, distanceZ));
                //Debug.Log($"[ColliderMask] Mode WorldSpace détecté. Distance Z : {distanceZ}");
            }
            else
            {
                pos = Camera.main.ScreenToWorldPoint(hit);
                //Debug.Log("[ColliderMask] Mode ScreenSpace détecté.");
            }

            pos.z = 0;

            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                //Debug.LogError("[ColliderMask] ERREUR : Aucun Collider2D trouvé sur cet objet !");
                return;
            }

            bool isInside = ToolBox.CheckPos(pos, col);
           

            if (isInside && IsActive)
            {
                //Debug.Log($"<color=yellow>[Success]</color> {gameObject.name} touché (Collider2D) ! Analyse de la priorité...");
                HandlePriority(pos, hit);
            }
        }
        else if (useCollider3DMask)
        {
            // --- Logique Collider3D ---
            Ray ray = Camera.main.ScreenPointToRay(hit);
            //Debug.Log($"[ColliderMask] Mode Collider3D — Ray origin: {ray.origin}, direction: {ray.direction}");

            Collider col3D = GetComponent<Collider>();
            if (col3D == null)
            {
                //Debug.LogError("[ColliderMask] ERREUR : Aucun Collider 3D trouvé sur cet objet !");
                return;
            }

            bool isInside = col3D.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity);
            Debug.Log($"[ColliderMask] Collider3D Hit: {isInside} | IsActive: {IsActive}");

            if (isInside && IsActive)
            {
                pos = rayHit.point;
                //Debug.Log($"<color=yellow>[Success]</color> {gameObject.name} touché (Collider3D) ! Point: {pos} Analyse de la priorité...");
                HandlePriority(pos, hit);
            }
        }
        else
        {
            // --- Logique Raycast originale ---
            float camDist = -Camera.main.transform.position.z;
            pos = Camera.main.ScreenToWorldPoint(new Vector3(hit.x, hit.y, camDist));
            pos.z = 0;

            bool isHit = ToolBox.CheckPos(pos, hit, this.gameObject.transform, rayCastAll);
            //Debug.Log($"[CheckPos] Objet: {gameObject.name} | Hit: {isHit} | Active: {IsActive} | WorldPos: {pos}");

            if (isHit && IsActive)
            {
                //Debug.Log($"<color=yellow>[Success]</color> {gameObject.name} touché ! Analyse de la priorité...");
                HandlePriority(pos, hit);
            }
        }
    }

    private void HandlePriority(Vector3 pos, Vector2 screenHit)
    {
        if (!usePriority)
        {
            //Debug.Log($"<color=green>[Final]</color> {gameObject.name} n'utilise pas de priorité. Invoke!");
            _event?.Invoke();
            return;
        }

        Transform parent = transform;
        while (parent.parent != null && parent.name != interactableParentName)
            parent = parent.parent;

        //Debug.Log($"[Priority] Parent de recherche: {parent.name}");

        bool isTop = true;
        Universal_ColliderMask blocker = null;

        foreach (var b in parent.GetComponentsInChildren<Universal_ColliderMask>())
        {
            if (b == this) continue;
            if (!b.IsActive) continue;

            if (b.priority > priority)
            {
                bool otherHit;

                if (b.useCollider2DMask)
                {
                    // Recalcul pos dans l'espace monde du frère (Collider2D)
                    Vector3 posForBrother;
                    Canvas brotherCanvas = b.GetComponentInParent<Canvas>();

                    if (brotherCanvas != null && brotherCanvas.renderMode == RenderMode.WorldSpace)
                    {
                        float distanceZ = Mathf.Abs(Camera.main.transform.position.z - b.transform.position.z);
                        posForBrother = Camera.main.ScreenToWorldPoint(new Vector3(screenHit.x, screenHit.y, distanceZ));
                    }
                    else
                    {
                        posForBrother = Camera.main.ScreenToWorldPoint(screenHit);
                    }
                    posForBrother.z = 0;

                    Collider2D col = b.GetComponent<Collider2D>();
                    otherHit = col != null && ToolBox.CheckPos(posForBrother, col);
                }
                else if (b.useCollider3DMask)
                {
                    // Raycast vers le Collider3D du frère
                    Ray ray = Camera.main.ScreenPointToRay(screenHit);
                    Collider col3D = b.GetComponent<Collider>();
                    otherHit = col3D != null && col3D.Raycast(ray, out _, Mathf.Infinity);
                }
                else
                {
                    otherHit = ToolBox.CheckPos(pos, screenHit, b.transform, b.rayCastAll);
                }

                //Debug.Log($"   -> Comparaison avec {b.name} (Prio: {b.priority}): Hit={otherHit}");

                if (otherHit)
                {
                    isTop = false;
                    blocker = b;
                    break;
                }
            }
        }

        if (isTop)
        {
            //Debug.Log($"<color=green>[Final]</color> {gameObject.name} est prioritaire (Prio: {priority}). Invoke!");
            _event?.Invoke();
        }
        else
        {
            //Debug.Log($"<color=red>[Blocked]</color> {gameObject.name} (Prio: {priority}) est masqué par {blocker.name} (Prio: {blocker.priority})");
        }
    }
}