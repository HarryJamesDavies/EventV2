using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapData : ScriptableObject
{
    public string m_name = "";

    public InputMapManager.InputType m_mapType = InputMapManager.InputType.NULL;

    public int m_buttonCount = 0;
    public List<InputMap.Button> m_buttons = new List<InputMap.Button>();
    public List<string> m_keyNames = new List<string>();

    public int m_defaultActionCount = 0;
    public List<InputMap.Action> m_defaultAction = new List<InputMap.Action>();

    public int m_customActionCount = 0;
    public List<InputMap.Action> m_customAction = new List<InputMap.Action>();

    public void Initialise(InputMap _map, string _name)
    {
        m_name = _name;

        m_mapType = _map.m_mapType;

        m_buttonCount = _map.m_buttonCount;
        m_buttons = _map.m_buttons;
        m_keyNames = _map.m_keyNames;

        m_defaultActionCount = _map.m_defaultActionCount;
        m_defaultAction = _map.m_defaultAction;

        m_customActionCount = _map.m_customActionCount;
        m_customAction = _map.m_customAction;
    }
}
