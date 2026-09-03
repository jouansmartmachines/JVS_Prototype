using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theme
{
    [CreateAssetMenu(fileName = "SwapPrefab", menuName = "Game/Theme/Entity/SwapPrefab")]
    public class SwapPrefab : SwapEntity
    {
        public GameObject Prefab;
    }
}