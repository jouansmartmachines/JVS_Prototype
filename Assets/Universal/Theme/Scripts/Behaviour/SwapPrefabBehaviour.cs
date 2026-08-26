using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theme
{
    public class SwapPrefabBehaviour : SwapObjectBehaviour
    {
        GameObject instance;

        protected override void Swap(GameTheme theme)
        {
            var entity = _swapObject.GetSwapEntity(theme) as SwapPrefab;
            if (instance != null) Destroy(instance);
            Debug.Log($"[GameObject] {gameObject.name}");
            instance = Instantiate(entity.Prefab, transform);
        }
    }
}