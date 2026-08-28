using OSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monstres
{
    public class Monstres_AccueilManager : ReceiveParent
    {
        public Transform playButton;
        public float margeY = 5f;
        public float margex = 5f;
        public GameObject impactPrefab;

        private bool gotAPt;
        private Vector3 newPt;

        //screen size
        private int w, h;

        [SerializeField] private float timeWithNoActivity = 240f;
        private float currentTimeWithNoActivity;

        private void Start()
        {
            w = Screen.width;
            h = Screen.height;

            OSC_Manager.Instance.receiveP = this;
        }

        private void Update()
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
                newPt.z = -Camera.main.transform.position.z;
                Vector3 clickPos = Camera.main.ScreenToWorldPoint(newPt);
                clickPos.z = 0.0f;

                if (clickPos.x > playButton.position.x - margex && clickPos.x < playButton.transform.position.x + margex)
                {
                    if (clickPos.y > playButton.transform.position.y - margeY && clickPos.y < playButton.transform.position.y + margeY)
                    {
                        LoadingManager.LoadScene(Monstres_GeneralVariables.Instance.gameScene);
                    }
                }

                Instantiate(impactPrefab, clickPos, Quaternion.identity);
            }
        }
        public override void ReceivePoint(float xPoint, float yPoint)
        {
            newPt.x = xPoint * w;
            newPt.y = yPoint * h;
            gotAPt = true;
        }
    }
}
