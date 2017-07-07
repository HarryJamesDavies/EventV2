using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class InputMap : ScriptableObject
{
    public InputMapManager.InputType m_mapType = InputMapManager.InputType.KEYBOARD;

    [Serializable]
    public class Button
    {
        public string m_input;
        public string m_key;

        public Button(string _input, string _key)
        {
            m_input = _input;
            m_key = _key;
        }

        public Button(Button _button)
        {
            if (_button != null)
            {
                m_input = _button.m_input;
                m_key = _button.m_key;
            }
            else
            {
                Debug.LogError("Don't initialise button with null");
            }
        }

        public void SetInput(string _input)
        {
            m_input = _input;
        }
    }

    [Serializable]
    public class Action
    {
        public string m_name;
        public Button m_button;

        public Action(string _name, Button _button)
        {
            m_name = _name;
            m_button = _button;
        }

        public Action(Action _action)
        {
            if(_action != null)
            {
                m_name = _action.m_name;
                m_button = new Button(_action.m_button);
            }
            else
            {
                Debug.LogError("Don't initialise action with null");
            }
        }
    }

    [HideInInspector]
    public int m_buttonCount = 0;
    public List<Button> m_buttons = new List<Button>();
    public List<string> m_keyNames = new List<string>();

    [HideInInspector]
    public int m_defaultActionCount = 0;
    public List<Action> m_defaultAction = new List<Action>();

    [HideInInspector]
    public int m_customActionCount = 0;
    private int m_prevCustomActionCount = 0;
    public List<Action> m_customAction = new List<Action>();
    private List<Action> m_prevCustomAction = new List<Action>();

    public int m_controllerIndex = -1;
    protected bool m_initialized = false;

    private string m_defaultKey = "null";

    public InputMap()
    {
    }

    public void Initialise(InputMap _map, int _controllerID)
    {
        m_mapType = _map.m_mapType;

        m_buttonCount = _map.m_buttonCount;
        for(int iter = 0; iter <= m_buttonCount - 1; iter++)
        {
            m_buttons.Add(new Button(_map.m_buttons[iter]));
        }

        for (int iter = 0; iter <= _map.m_keyNames.Count - 1; iter++)
        {
            m_keyNames.Add(_map.m_keyNames[iter]);
        }

        m_defaultActionCount = _map.m_defaultActionCount;
        for (int iter = 0; iter <= m_defaultActionCount - 1; iter++)
        {
            m_defaultAction.Add(new Action(_map.m_defaultAction[iter]));
        }

        m_customActionCount = _map.m_customActionCount;
        for (int iter = 0; iter <= m_customActionCount - 1; iter++)
        {
            m_customAction.Add(new Action(_map.m_customAction[iter]));
        }

        SetControllerID(_controllerID);
    }

    public void Initialise(MapData _data, int _controllerID)
    {
        m_mapType = InputMapManager.InputType.PS4;

        m_buttonCount = _data.m_buttonCount;
        m_buttons = _data.m_buttons;
        m_keyNames = _data.m_keyNames;

        m_defaultActionCount = _data.m_defaultActionCount;
        m_defaultAction = _data.m_defaultAction;

        m_customActionCount = _data.m_customActionCount;
        m_customAction = _data.m_customAction;

        UpdateKeyNames();
        SetControllerID(_controllerID);
    }

    public void Awake()
    {
        SetPreviousState();
    }

    public void SetControllerID(int _controllerID)
    {
        m_controllerIndex = _controllerID;

        for (int iter = 0; iter <= m_buttonCount - 1; iter++)
        {
            m_buttons[iter].m_input = m_buttons[iter].m_input + m_controllerIndex;
        }

        for (int iter = 0; iter <= m_defaultActionCount - 1; iter++)
        {
            m_defaultAction[iter].m_button.m_input = m_defaultAction[iter].m_button.m_input + m_controllerIndex;
        }

        for (int iter = 0; iter <= m_customActionCount - 1; iter++)
        {
            m_customAction[iter].m_button.m_input = m_customAction[iter].m_button.m_input + m_controllerIndex;
        }

    }

    public void SetPreviousState()
    {
        m_prevCustomActionCount = m_customActionCount;
        m_prevCustomAction.Clear();
        foreach (Action action in m_customAction)
        {
            m_prevCustomAction.Add(new Action(action.m_name, new Button(action.m_button.m_input, action.m_button.m_key)));
        }
    }

    public void Initialized(int _controllerIndex)
    {
        m_controllerIndex = _controllerIndex;

        for (int iter = 0; iter <= m_buttonCount - 1; iter++)
        {
            m_buttons[iter].SetInput(m_buttons[iter].m_input + _controllerIndex);
        }

        m_initialized = true;
    }

    public bool CheckAlterations()
    {
        if(m_prevCustomActionCount == m_customActionCount)
        {
            for (int iter = 0; iter <= m_customAction.Count - 1; iter++)
            {
                if (m_customAction[iter].m_name != m_prevCustomAction[iter].m_name)
                {
                    return true;
                }

                if (m_customAction[iter].m_button.m_key != m_prevCustomAction[iter].m_button.m_key)
                {
                    return true;
                }

                if (m_customAction[iter].m_button.m_input != m_prevCustomAction[iter].m_button.m_input)
                {
                    return true;
                }
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    public Button GetButton(string _key)
    {
        foreach (Button button in m_buttons)
        {
            if (button.m_key == _key)
            {
                return button;
            }
        }

        return null;
    }

    public Action GetAction(string _action)
    {
        foreach (Action action in m_defaultAction)
        {
            if (action.m_name == _action)
            {
                return action;
            }
        }

        foreach (Action action in m_customAction)
        {
            if (action.m_name == _action)
            {
                return action;
            }
        }

        return null;
    }

    public void SetDefualtAction(int _iterator, string _action)
    {
        m_defaultAction[_iterator].m_name = _action;
    }

    public void SetCustomAction(int _iterator, string _action)
    {
        m_customAction[_iterator].m_name = _action;
    }

    public void SetDefualtButton(int _iterator, string _button)
    {
        m_defaultAction[_iterator].m_button = GetButton(_button);
    }

    public void SetCustomButton(int _iterator, string _button)
    {
        m_customAction[_iterator].m_button = GetButton(_button);
    }

    public string GetInput(string _action)
    {
        foreach (Action action in m_defaultAction)
        {
            if (action.m_name == _action)
            {
                return action.m_button.m_input;
            }
        }

        foreach (Action action in m_customAction)
        {
            if (action.m_name == _action)
            {
                return action.m_button.m_input;
            }
        }

        return null;
    }
    
    public void RemoveAction(string _action)
    {
        foreach (Action action in m_defaultAction)
        {
            if (action.m_name == _action)
            {
                m_defaultAction.Remove(action);
                m_defaultActionCount--;
                return;
            }
        }

        foreach (Action action in m_customAction)
        {
            if (action.m_name == _action)
            {
                m_customAction.Remove(action);
                m_customActionCount--;
                return;
            }
        }
    }

    public void RemoveInput(string _input)
    {
        foreach (Action action in m_defaultAction)
        {
            if (action.m_button.m_input == _input)
            {
                m_defaultAction.Remove(action);
                return;
            }
        }

        foreach (Action action in m_customAction)
        {
            if (action.m_button.m_input == _input)
            {
                m_customAction.Remove(action);
                return;
            }
        }
    }

    public void AddDefaultAction()
    {
        m_defaultAction.Add(new Action("NULL", new Button(GetButton(m_defaultKey))));
        m_defaultActionCount++;
    }

    public void AddDefaultAction(string _action, string _input, string _key)
    {
        m_defaultAction.Add(new Action(_action, new Button(_input, _key)));
        m_defaultActionCount++;
    }

    public void AddDefaultAction(Action _action)
    {
        m_defaultAction.Add(_action);
        m_defaultActionCount++;
    }

    public void AddCustomAction()
    {
        if(m_defaultKey == "null")
        {
            m_defaultKey = m_keyNames[0];
        }
        m_customAction.Add(new Action("", new Button(GetButton(m_defaultKey))));
        m_customActionCount++;
    }

    public void AddCustomAction(string _action, string _input, string _key)
    {
        m_customAction.Add(new Action(_action, new Button(_input, _key)));
        m_customActionCount++;
    }

    public void AddCustomAction(Action _action)
    {
        m_customAction.Add(_action);
        m_customActionCount++;
    }

    protected void UpdateKeyNames()
    {
        m_keyNames.Clear();
        
        foreach(Button button in m_buttons)
        {
            m_keyNames.Add(button.m_key);
        }

        if (m_keyNames.Count != 0)
        {
            m_defaultKey = m_keyNames[0];
        }
        else
        {
            Debug.LogWarning("No Keys Assign to Map: " + m_mapType.ToString());
        }
    } 

    public int GetKeyIndex(string _key)
    {
        int output = -1;

        int count = -1;
        foreach (string name in m_keyNames)
        {
            count++;
            if(name == _key)
            {
                output = count;
            }
        }

        return output;
    }
}