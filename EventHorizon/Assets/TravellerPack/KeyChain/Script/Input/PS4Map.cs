using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PS4Map : InputMap
{
    public enum PS4
    {
        X,
        Square,
        Circle,
        Triangle,
        L1,
        L2A,
        L2B,
        L3,
        R1,
        R2A,
        R2B,
        R3,
        Share,
        Options,
        PS,
        PadPress,
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        DPadX,
        DPadY,
        NULL
    }

    [Serializable]
    public class PS4Button : Button
    {
        public PS4 m_PS4Key;

        public PS4Button(string _input, string _key, PS4 _PS4Key) : base(_input, _key)
        {
            m_PS4Key = _PS4Key;
        }
    }

    private static string[] m_inputStrings = { "X(PS4)", "Square(PS4)", "Circle(PS4)", "Triangle(PS4)", "L1(PS4)", "L2A(PS4)", "L2B(PS4)", "L3(PS4)", "R1(PS4)",
        "R2A(PS4)", "R2B(PS4)", "R3(PS4)", "Share(PS4)", "Options(PS4)", "PS(PS4)", "PadPress(PS4)", "LeftStickX(PS4)", "LeftStickY(PS4)", "RightStickX(PS4)",
        "RightStickY(PS4)", "DPadX(PS4)", "DPadY(PS4)" };

    private static string[] m_PS4Actions = { "Horizontal", "LeftStickX(PS4)", "Vertical", "LeftStickY(PS4)", "Jump", "X(PS4)", "Pause", "Options(PS4)", "Menu", "PadPress(PS4)" };

    public PS4Map()
    {
        
    }

    public void Initialise()
    {
        m_mapType = InputMapManager.InputType.PS4;

        m_buttonCount = m_inputStrings.Length;

        m_buttons.Clear();
        for (int iter = 0; iter <= m_buttonCount - 1; iter++)
        {
            PS4 key = (PS4)iter;
            m_buttons.Add(new PS4Button(m_inputStrings[iter], key.ToString(), key));
        }

        m_defaultActionCount = m_PS4Actions.Length / 2;

        m_defaultAction.Clear();
        for (int iter = 0; iter <= (m_defaultActionCount * 2) - 1; iter++)
        {
            m_defaultAction.Add(new Action(m_PS4Actions[iter], GetButton(m_PS4Actions[iter + 1])));
            iter++;
        }

        m_customActionCount = 0;
        m_customAction.Clear();

        UpdateKeyNames();
    }

    public void Initialise(MapData _data)
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
    }  

    new
    public PS4Button GetButton(string _input)
    {
        foreach (PS4Button button in m_buttons)
        {
            if (button.m_input == _input)
            {
                return button;
            }
        }

        return null;
    }

    public PS4Button GetButton(PS4 _PS4Key)
    {
        foreach (PS4Button button in m_buttons)
        {
            if (button.m_PS4Key == _PS4Key)
            {
                return button;
            }
        }

        return null;
    }
    
    public string GetInput(PS4 _button)
    {
        foreach (PS4Button button in m_buttons)
        {
            if (button.m_PS4Key == _button)
            {
                return button.m_input;
            }
        }

        return null;
    }

    public PS4 GetPS4Key(string _action)
    {
        foreach (Action action in m_defaultAction)
        {
            if (action.m_name == _action)
            {
                PS4Button button = (PS4Button)action.m_button;
                return button.m_PS4Key;
            }
        }

        foreach (Action action in m_customAction)
        {
            if (action.m_name == _action)
            {
                PS4Button button = (PS4Button)action.m_button;
                return button.m_PS4Key;
            }
        }

        return PS4.NULL;
    }

    public void AddDefaultAction(string _action, string _input, PS4 _key)
    {
        m_defaultAction.Add(new Action(_action, new PS4Button(_input, _key.ToString(), _key)));
    }

    public void AddCustomAction(string _action, string _input, PS4 _key)
    {
        m_customAction.Add(new Action(_action, new PS4Button(_input, _key.ToString(), _key)));
    }
}
