using OSC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dame
{
    public class Dame_PlayButton : Universal_PlayButton
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
                SceneManager.LoadScene(Dame_GeneralVariables.Instance.introScene);
            }
        }
    }
}