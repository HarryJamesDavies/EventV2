using UnityEngine;
using System.Collections;

public class KeyChainManager : MonoBehaviour {

    public static KeyChainManager m_instance = null;

    public static InputMapManager InputMapManager = new InputMapManager();
    public static ControllerManager ControllerManager = new ControllerManager();

    void Awake()
    {
        if (!m_instance)
        {
            m_instance = this;
            Initialise();
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    void Initialise()
    {
        InputSettingsData data = Resources.Load("InputSettings/KeyChainSettings") as InputSettingsData;

        if (data != null)
        {
            ControllerManager.m_maxNumInputs = data.m_maxNumInputs;
            ControllerManager.m_enableKeyboard = data.m_enableKeyboard;
            ControllerManager.m_enableNES = data.m_enableNES;
            ControllerManager.m_enablePS4 = data.m_enablePS4;
        }
        else
        {
            Debug.Log("No Saved Settings");
        }

        InputMapManager.m_initalised = false;
        InputMapManager.Initialise();
        ControllerManager.Initialise();
    }

    void Start()
    {
        ControllerManager.Begin();
    }

    void Update()
    {
        ControllerManager.Tick();
    }

    void LateUpdate()
    {
        ControllerManager.LateTick();
    }
}
