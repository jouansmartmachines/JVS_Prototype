using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameObject", menuName = "ScriptableObjects/GameObject", order = 1)]
public class ScriptableGameObject : ScritptableValueGeneric<GameObject>
{
    public override GameObject Load()
    {
        return null;
    }
}
