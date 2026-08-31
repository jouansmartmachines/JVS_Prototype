using OSC;
using UnityEngine;

namespace Monstres
{
    public class CallMonstresPhoto : MonoBehaviour
    {
        void Start()
        {
            OSC_Manager.Instance.PhotoMonstresDemo();
        }
    }
}
