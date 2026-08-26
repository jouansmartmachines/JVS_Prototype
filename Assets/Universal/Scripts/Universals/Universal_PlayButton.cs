using OSC;
using UnityEngine;
using UnityEngine.SceneManagement;

//namespace Monstres
//{
public class Universal_PlayButton : ReceiveParent
{
    public GameObject impact;       //impact des balles
    public Transform playButton;
    public GameObject startIndication;
    public float margeY = 5f;
    public float margex = 5f;
    //caracteristiques d'un impact
    protected bool gotAPt;
    protected Vector3 newPt;
    [SerializeField] protected Canvas renderImpact;

    [SerializeField] protected GameEvent _fruitEvent;
    [SerializeField] protected bool accueil;
    [SerializeField] protected string _sceneName;

    //screen size
    protected int w, h;
    [SerializeField] protected float timeWithNoActivity = 240f;
    protected float currentTimeWithNoActivity;

    protected virtual void Start()
    {
        gotAPt = false;         //Pas de point
        w = Screen.width;
        h = Screen.height;
        OSC_Manager.Instance.receiveP = this;
        PostStart();
    }

    protected virtual void PostStart()
    {
        if (PlayerPrefs.GetInt("ShowIndication") == -1 && startIndication != null) // check if we want to show indications (can be set on/off in the menu)
        {
            startIndication.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        //pour utliser le jeu avec une souris
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
            //si on a un impact on cree une animation
            GameObject newImpact;
            //newPt.z = renderImpact.planeDistance;
            Quaternion rrotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0, 360));
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(newPt);
            clickPos.z = 0f;
            //Debug.Log(newPt + " "+ Camera.main.ScreenToWorldPoint(newPt) +" " + playButton.position);
            newImpact = Instantiate(impact, clickPos, rrotation);
            newImpact.SetActive(true);
            if (clickPos.x > playButton.position.x - margex && clickPos.x < playButton.transform.position.x + margex)
            {
                if (clickPos.y > playButton.transform.position.y - margeY && clickPos.y < playButton.transform.position.y + margeY)
                {
                    WhenPlayGotPress();
                }
            }
        }
    }

    protected virtual void WhenPlayGotPress()
    {
        Debug.Log("Play Press");

        if (_fruitEvent != null)
            _fruitEvent.Raise();
        LoadingManager.LoadScene(_sceneName);
    }

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        newPt.x = xPoint * w;
        newPt.y = yPoint * h;
        gotAPt = true;
    }
}
