using UnityEngine;
using System.Collections;

public class InputEnableEventData : ScriptableObject
{
    public string ControllerName;
    public InputMapManager.InputType ControllerType;
    public int ControllerIndex;
}
