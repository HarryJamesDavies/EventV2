using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ControllerManager : UnityEngine.Object
{
    [Serializable]
    public struct ControllerData
    {
        public string m_controllerName;
        public InputMapManager.InputType m_controllerType;
        public bool m_controllerAdded;
    };

    public static bool m_enablePooling = true;

    public static bool m_enableKeyboard = false;
    public static bool m_enableNES = false;
    public static bool m_enablePS4 = false;

    public static int m_maxNumInputs;
    public int m_currentNumInputs;

    public List<ControllerData> m_controllers = new List<ControllerData>();

    public List<string> m_connectedControllers = new List<string>();
    public List<string> m_prevConnectedControllers = new List<string>();

    public void Initialise()
    {
        m_connectedControllers.Clear();
        m_prevConnectedControllers.Clear();
   
        ClearAllData();
    }

    // Use this for initialization
    public void Begin()
    {
        UpdateConnectedControllers();
        UpdatePrevControllers();
    }

    public void Tick()
    {
        ControllerPoller();
    }

    public void LateTick()
    {
        UpdateConnectedControllers();

        if (!m_connectedControllers.Equals(m_prevConnectedControllers))
        {
            for (int iter = 0; iter <= m_connectedControllers.Count - 1; iter++)
            {
                if (m_connectedControllers[iter] != m_prevConnectedControllers[iter])
                {
                    if (m_connectedControllers[iter] == "")
                    {
                        ControllerDisconnected(iter);
                        ClearData(iter);
                    }
                    else
                    {
                        ControllerConnected(iter);
                    }
                }
            }
            UpdatePrevControllers();
        }
    }

    //Behaviour to perform per frame based on state
    void ControllerPoller()
    {
        if (m_enablePooling)
        {
            if (m_enableKeyboard)
            {
                if (Input.GetKeyDown("space"))
                {
                    //PawnManager.m_instance.AssignKeyboard(PawnManager.m_instance.m_activeNumberOfPawns + 1);
                }
            }

            if (m_enablePS4)
            {
                CheckPS4ControllerReady();
            }

            if (m_enableNES)
            {
                CheckNESControllerReady();
            }

            m_currentNumInputs = 0;
            for (int iter = 0; iter <= m_controllers.Count - 1; iter++)
            {
                if (m_controllers[iter].m_controllerType != InputMapManager.InputType.NULL)
                {
                    m_currentNumInputs++;
                }
            }
        }
    }

    void UpdateConnectedControllers()
    {
        string[] joystics = Input.GetJoystickNames();

        m_connectedControllers.Clear();
        m_currentNumInputs = 0;

        for (int iter = 0; iter <= joystics.Length - 1; iter++)
        {
            m_connectedControllers.Add(joystics[iter]);

            if (joystics[iter] != "")
            {
                m_currentNumInputs++;
            }
        }

        for (int iter = joystics.Length; iter <= m_maxNumInputs - 1; iter++)
        {
            m_connectedControllers.Add("");
        }
    }

    void UpdatePrevControllers()
    {
        m_prevConnectedControllers.Clear();

        foreach (string name in m_connectedControllers)
        {
            m_prevConnectedControllers.Add(name);
        }
    }

    void ControllerConnected(int _index)
    {
        switch (_index)
        {
            case 0:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER1CONNECTED);                  
                    break;
                }
            case 1:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER2CONNECTED);
                    break;
                }
            case 2:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER3CONNECTED);
                    break;
                }
            case 3:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER4CONNECTED);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void ControllerDisconnected(int _index)
    {
        switch (_index)
        {
            case 0:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER1DISCONNECTED);
                    break;
                }
            case 1:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER2DISCONNECTED);
                    break;
                }
            case 2:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER3DISCONNECTED);
                    break;
                }
            case 3:
                {
                    EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLER4DISCONNECTED);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void ControllerAdded(int _index)
    {
        InputEnableEventData data = ScriptableObject.CreateInstance<InputEnableEventData>();
        data.ControllerName = m_controllers[_index].m_controllerName;
        data.ControllerType = m_controllers[_index].m_controllerType;
        data.ControllerIndex = _index + 1;

        EventManager.m_instance.AddEvent(Events.Event.CO_CONTROLLERADDED, data);
    }

    void ClearData(int _index)
    {
        ControllerData tempData;
        tempData.m_controllerName = "Controller " + (_index + 1);
        tempData.m_controllerType = InputMapManager.InputType.NULL;
        tempData.m_controllerAdded = false;
        m_controllers[_index] = tempData;
    }

    void ClearAllData()
    {
        m_controllers.Clear();

        for (int iter = 0; iter <= 3; iter++)
        {
            ControllerData tempData;
            tempData.m_controllerName = "Controller " + (iter + 1);
            tempData.m_controllerType = InputMapManager.InputType.NULL;
            tempData.m_controllerAdded = false;
            m_controllers.Add(tempData);
        }
    }

    void CheckNESControllerReady()
    {
        for (int iter = 0; iter <= m_connectedControllers.Count - 1; iter++)
        {
            if (m_controllers[iter].m_controllerType == InputMapManager.InputType.NULL)
            {
                if (Input.GetButtonDown("Start(NES)" + (iter + 1)) || Input.GetButtonDown("A(NES)" + (iter + 1)))
                {
                    ControllerData tempData = m_controllers[iter];
                    tempData.m_controllerName = "NES " + (iter + 1);
                    tempData.m_controllerType = InputMapManager.InputType.NES;
                    tempData.m_controllerAdded = true;
                    m_controllers[iter] = tempData;

                    ControllerAdded(iter);
                }
            }
        }
    }

    void CheckPS4ControllerReady()
    {
        for (int iter = 0; iter <= m_connectedControllers.Count - 1; iter++)
        {
            if (m_controllers[iter].m_controllerType == InputMapManager.InputType.NULL)
            {
                if (Input.GetButtonDown("PS(PS4)" + (iter + 1)) || Input.GetButtonDown("X(PS4)" + (iter + 1)))
                {
                    ControllerData tempData = m_controllers[iter];
                    tempData.m_controllerName = "PS4";
                    tempData.m_controllerType = InputMapManager.InputType.PS4;
                    tempData.m_controllerAdded = true;
                    m_controllers[iter] = tempData;

                    ControllerAdded(iter);
                }
            }
        }
    }
}
