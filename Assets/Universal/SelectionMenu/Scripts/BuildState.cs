using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildState
{
    public enum State
    {
        normal,
        menuSelection
    }

    public static State CurrentState = State.normal;
    public const string MenuSelectionSceneName = "SelectionMenu";
}
