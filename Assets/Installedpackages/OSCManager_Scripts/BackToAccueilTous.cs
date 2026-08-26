using OSC;
using UnityEngine;

public class BackToAccueilTous : ReceiveParent
{
    [SerializeField] private float timeWithNoActivity;
    private float currentTimeWithNoActivity;

    private bool gotAPt;
    private Vector3 newPt;
    private float w, h;

    private void Start()
    {
        gotAPt = false;
        w = Screen.width;
        h = Screen.height;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            newPt.x = (float)Input.mousePosition.x;
            newPt.y = (float)Input.mousePosition.y;
            gotAPt = true;
        }

        //currentTimeWithNoActivity += Time.deltaTime;
        //if (currentTimeWithNoActivity >= timeWithNoActivity)
        //{
        //    OSC_Manager.Instance.onOSCAccueilTous(0);
        //    currentTimeWithNoActivity = 0;
        //}

        if (gotAPt)
        {
            gotAPt = false;
            currentTimeWithNoActivity = 0;
        }
    }

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        newPt.x = xPoint * w;
        newPt.y = yPoint * h;
        gotAPt = true;
    }
}
