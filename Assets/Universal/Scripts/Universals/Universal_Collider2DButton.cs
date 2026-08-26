using System.Collections;
using System.Collections.Generic;
using Tool;
using UnityEngine;

public class Universal_Collider2DButton : Universal_Button
{
    public override void ReceivePoint(float xPoint, float yPoint)
    {
        // 1. Log des coordonnées d'entrée (souvent normalisées entre 0 et 1)
        //Debug.Log($"[Button] Entrée reçue : x={xPoint}, y={yPoint}");

        xPoint *= Screen.width;
        yPoint *= Screen.height;
        Vector2 hit = new Vector2(xPoint, yPoint);
        

        Vector3 pos;
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            float distanceZ = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
            pos = Camera.main.ScreenToWorldPoint(new Vector3(hit.x, hit.y, distanceZ));
        }
        else
        {
            pos = Camera.main.ScreenToWorldPoint(hit);
        }
        pos.z = 0;
        
        // Récupération du Collider
        Collider2D col = GetComponent<Collider2D>();

        bool isInside = ToolBox.CheckPos(pos, col);

        if (isInside && IsActive)
        {
            _event?.Invoke();
        }
    }
}