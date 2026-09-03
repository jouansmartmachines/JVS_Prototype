using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tool;
using UnityEngine.Events;

public class Universal_Button : ReceiveParent
{
    public UnityEvent Event => _event;
    [SerializeField]
    protected UnityEvent _event;

    public bool IsActive = true;

    public List<Vector2> Hits => hits;
    [HideInInspector] public List<Vector2> hits = new();

    [SerializeField] protected bool rayCastAll = false;

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        // Convertit les coordonnées normalisées en pixels
        xPoint *= Screen.width;
        yPoint *= Screen.height;
        Vector2 hit = new Vector2(xPoint, yPoint);
        hits.Add(hit);

        // Transforme en position monde
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(hit.x, hit.y, -Camera.main.transform.position.z));
        pos.z = 0;

        // Vérifie si ce bouton est touché (forme/collider)
        if (ToolBox.CheckPos(pos, hit, this.gameObject.transform, rayCastAll) && IsActive)
        {
            //HandlePriority(pos);s
            _event?.Invoke(); 
            return; 
        }
    }

}
