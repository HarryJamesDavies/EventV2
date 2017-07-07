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
