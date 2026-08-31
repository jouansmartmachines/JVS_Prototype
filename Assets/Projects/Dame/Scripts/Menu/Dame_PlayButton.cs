using UnityEngine;

namespace Dame
{
    public class Dame_PlayButton : Universal_PlayButton
    {
        // Heritage direct de Universal_PlayButton
        // Le bouton Play appelle Universal_GeneralVariables.StartGame()
        // qui charge la GameScene via LoadingManager
    }
}