using OSC;
using UnityEngine;

public class LeaderboardParent : MonoBehaviour
{
    protected void ShowKeyboard()
    {
        OSC_Manager.Instance.ShowSoftKeyboard();
    }

    public virtual void NameSubmit(string directNameToSubmit = "")
    {
        Debug.Log(directNameToSubmit);
    }

    public virtual void ScrollUp()
    {

    }

    public virtual void ScrollDown()
    {

    }

    public virtual void ResetScore() { }
}
