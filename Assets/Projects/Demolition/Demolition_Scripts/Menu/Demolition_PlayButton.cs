using OSC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demolition
{
    public class Demolition_PlayButton : Universal_PlayButton
    {
        protected override void PostStart()
        {
            if (PlayerPrefs.GetInt("ShowIndication") == -1 && startIndication != null)
                startIndication.SetActive(false);
        }

        protected override void WhenPlayGotPress()
        {
            if (!accueil)
            {
                SceneManager.LoadScene(Demolition_GeneralVariables.Instance.introScene);
            }
        }
    }
}