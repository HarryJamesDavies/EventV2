using UnityEngine;
using System.Collections;

public class Pawn : MonoBehaviour
{
    public InputMap m_inputMap = null;
    public InputMapManager.InputType m_inputType = InputMapManager.InputType.NULL;

    public Pawn()
    {

    }

    public Pawn(InputMap _map, int _controllerID)
    {
        SetInputMap(_map, _controllerID);
    }

    public void Start()
    {
        EventManager.m_instance.SubscribeToEvent(Events.Event.CO_CONTROLLERADDED, EvFunc_AssignMap);
    }

    void OnDestroy()
    {
        EventManager.m_instance.UnsubscribeToEvent(Events.Event.CO_CONTROLLERADDED, EvFunc_AssignMap);
    }

    void EvFunc_AssignMap(object _data = null)
    {
        if (_data != null)
        {
            InputEnableEventData data = _data as InputEnableEventData;
            m_inputType = data.ControllerType;
            SetInputMap(KeyChainManager.InputMapManager.GetPresetMap(data.ControllerType, data.ControllerName), data.ControllerIndex);
        }
        else
        {
            Debug.Log("Can't set pawn InputMap with null data");
        }
    }

    public void SetInputMap(InputMap _map, int _controllerID)
    {
        KeyChainManager.InputMapManager.ResetMap(m_inputMap);
        m_inputMap = ScriptableObject.CreateInstance<InputMap>();
        m_inputMap.Initialise(_map, _controllerID);
    }

    public void ResetInputMap()
    {
        KeyChainManager.InputMapManager.ResetMap(m_inputMap);
    }

    public void AssignInput(InputMapManager.InputType _type, string _name, int _controllerID)
    {
        m_inputType = _type;
        SetInputMap(KeyChainManager.InputMapManager.GetPresetMap(_name), _controllerID);

        return;
    }
}
