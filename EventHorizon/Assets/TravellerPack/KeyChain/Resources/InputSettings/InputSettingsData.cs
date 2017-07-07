using UnityEngine;
using System.Collections;

public class InputSettingsData : ScriptableObject
{
    public string m_resourcePath = "";

    public int m_maxNumInputs;

    public bool m_enableKeyboard = false;
    public bool m_enableNES = false;
    public bool m_enablePS4 = false;

    public void Initialise(string _resourcePath)
    {
        m_resourcePath = _resourcePath;

        m_maxNumInputs = ControllerManager.m_maxNumInputs;

        m_enableKeyboard = ControllerManager.m_enableKeyboard;
        m_enableNES = ControllerManager.m_enableNES;
        m_enablePS4 = ControllerManager.m_enablePS4;
    }
}
