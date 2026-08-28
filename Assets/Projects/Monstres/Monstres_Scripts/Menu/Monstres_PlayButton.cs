using OSC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monstres
{
    public class Monstres_PlayButton : Universal_PlayButton
    {
        protected override void PostStart()
        {
            if (PlayerPrefs.GetInt("ShowIndication") == -1) // check if we want to show indications (can be set on/off in the menu)
            {
                startIndication.SetActive(false);
            }
        }

        protected override void WhenPlayGotPress()
        {
            if (!accueil)
            {
                SceneManager.LoadScene(Monstres_GeneralVariables.Instance.introScene);
                OSC_Manager.Instance.PhotoMonstresDemo();
            }
        }
    }
}