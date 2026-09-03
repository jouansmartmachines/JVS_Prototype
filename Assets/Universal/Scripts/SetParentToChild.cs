using UnityEngine;

public class SetParentToChild : MonoBehaviour
{
    void LateUpdate()
    {
        if (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            if (child.childCount > 0)
            {
                Transform grandChild = child.GetChild(0);
                transform.position = grandChild.position;
                this.enabled = false;
            }
        }
    }
}