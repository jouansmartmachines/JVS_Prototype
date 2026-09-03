using OSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrageSystem : ReceiveParent
{
    public GameObject impact;
    private bool gotAPt;
    private Vector3 newPt;
    //screen size
    private int w, h;
    public AudioClip hitSound;
    public AudioSource audioS;

    private void Awake()
    {
        OSC_Manager.Instance.receiveP = this;
    }

    private void Start()
    {
        gotAPt = false;         //Pas de point
        w = Screen.width;
        h = Screen.height;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            newPt.x = Input.mousePosition.x;
            newPt.y = Input.mousePosition.y;
            gotAPt = true;
        }

        if (gotAPt)
        {
            gotAPt = false;
            newPt.z = -Camera.main.transform.position.z;
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(newPt);
            clickPos.z = 0f;
            Ray ray = Camera.main.ScreenPointToRay(newPt);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                gotAPt = false;
                newPt.z = -Camera.main.transform.position.z;
                Instantiate(impact, hit.point, Quaternion.identity);
                audioS.Play();
            }
        }
    }

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        newPt.x = xPoint * w;
        newPt.y = yPoint * h;
        gotAPt = true;
    }

}