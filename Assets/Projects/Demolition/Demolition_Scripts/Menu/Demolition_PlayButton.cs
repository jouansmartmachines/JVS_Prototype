using UnityEngine;

namespace Demolition
{
    public class Demolition_PlayButton : Universal_PlayButton
    {
        // Heritage direct de Universal_PlayButton
        // Le bouton Play appelle Universal_GeneralVariables.StartGame()
        // qui charge la GameScene via LoadingManager
    }
}