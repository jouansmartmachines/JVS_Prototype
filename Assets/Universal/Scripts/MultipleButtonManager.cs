using OSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultipleButtonManager : ReceiveParent
{
    public GameObject impact;       //impact des balles
    public GameObject startIndication;

    //caracteristiques d'un impact
    private bool gotAPt;
    private Vector3 newPt;
    [SerializeField] private Canvas renderImpact;

    [SerializeField] private Button[] _buttons;
    //screen size
    private int w, h;
    public  bool Disable { get; set; }
    // Start is called before the first frame update
    private void Start()
    {
        gotAPt = false;         //Pas de point
        w = Screen.width;
        h = Screen.height;

        if (PlayerPrefs.GetInt("ShowIndication") == -1 && startIndication != null) // check if we want to show indications (can be set on/off in the menu)
        {
            startIndication.SetActive(false);
        }

        OSC_Manager.Instance.receiveP = this;
    }
    public void SetButtons(Button[] buttons) 
    {
        _buttons = buttons;
    }

    // Update is called once per frame
    private void Update()
    {
        if(Disable)
        {
            gotAPt=false;
            return;
        }   
        
        //pour utliser le jeu avec une souris
        if (Input.GetButtonDown("Fire1"))
        {
            newPt.x = (float)Input.mousePosition.x;
            newPt.y = (float)Input.mousePosition.y;
            gotAPt = true;
        }

        if (gotAPt)
        {
            gotAPt = false;
            //si on a un impact on cree une animation
            GameObject newImpact;
            newPt.z = renderImpact.planeDistance;
            Quaternion rrotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));
            Debug.Log(newPt);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(newPt);
            clickPos.z = 0.0f;

            newImpact = Instantiate(impact, clickPos, rrotation, renderImpact.gameObject.transform);
            newImpact.SetActive(true);
            foreach (var button in _buttons) 
            {
                button.ReceivePointFromManager(clickPos);
            }
        }
    }

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        Debug.Log(yPoint + " " + xPoint);
        newPt.x = xPoint * w;
        newPt.y = yPoint * h;
        gotAPt = true;

    }
}
