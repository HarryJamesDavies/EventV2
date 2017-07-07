using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class InputMapManager : UnityEngine.Object
{
    public static int InputTypeCount = Enum.GetNames(typeof(InputMapManager.InputType)).Length - 1;

    public bool m_initalised = false;

    public enum InputType
    {
        KEYBOARD = 0,
        NES = 1,
        PS4 = 2, 
        NULL
    };

    [Serializable]
    public struct InputPreset
    {
        public string m_name;
        public int m_ID;
        public InputMap m_map;
        public bool m_inUse;
    }

    public Dictionary<InputType, List<InputPreset>> m_presets = new Dictionary<InputType, List<InputPreset>>();

    public void Start()
    {
        m_initalised = false;
        Initialise();
    }

    public void Initialise()
    {
        if (!m_initalised)
        {
            m_presets.Clear();
            for (int iter = 0; iter <= InputTypeCount - 1; iter++)
            {
                m_presets.Add((InputType)iter, new List<InputPreset>());
            }
            LoadAssets();
            m_initalised = true;
        }
    }

    public int GenerateID()
    {
        int ID = UnityEngine.Random.Range(0, 10000);

        while (CheckID(ID))
        {
            ID = UnityEngine.Random.Range(0, 10000);
        }

        return ID;
    }

    bool CheckID(int _ID)
    {
        for (int iter = 0; iter <= InputTypeCount - 1; iter++)
        {
            foreach (InputPreset preset in m_presets[(InputType)iter])
            {
                if(preset.m_ID == _ID)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int GetID(string _name)
    {
        int ID = -1;

        for (int iter = 0; iter <= InputTypeCount - 1; iter++)
        {
            foreach (InputPreset preset in m_presets[(InputType)iter])
            {
                if (preset.m_name == _name)
                {
                    ID = preset.m_ID;
                }
            }
        }

        return ID;
    }

    public void LoadAssets()
    {
        for (int iter = 0; iter <= InputTypeCount - 1; iter++)
        {
            InputType temp = (InputType)iter;
            MapData[] maps = Resources.LoadAll<MapData>("InputMap/" + temp.ToString());

            if (maps.Length == 0)
            {
                Debug.Log("No Maps in " + temp.ToString());
            }

            foreach (MapData map in maps)
            {
                InputPreset tempPreset = new InputPreset();
                tempPreset.m_name = map.m_name;
                tempPreset.m_ID = GenerateID();
                tempPreset.m_inUse = false;

                switch (map.m_mapType)
                {
                    case InputType.KEYBOARD:
                        {
                            break;
                        }
                    case InputType.NES:
                        {
                            break;
                        }
                    case InputType.PS4:
                        {
                            PS4Map tempMap =  ScriptableObject.CreateInstance<PS4Map>();
                            tempMap.Initialise(map);
                            tempPreset.m_map = tempMap;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                m_presets[map.m_mapType].Add(tempPreset);
            }           
        }
    }

    public void AddEmptyPreset(string _name, InputType _type)
    {
        InputPreset tempPreset = new InputPreset();
        tempPreset.m_name = _name;
        tempPreset.m_ID = GenerateID();
        tempPreset.m_inUse = false;
        tempPreset.m_map = GetMap(_type);

        m_presets[_type].Add(tempPreset);
    }

    public void AddDuplicatePreset(string _name, InputMap _map)
    {
        InputPreset tempPreset = new InputPreset();
        tempPreset.m_name = _name;
        tempPreset.m_ID = GenerateID();
        tempPreset.m_inUse = false;
        tempPreset.m_map = _map;

        m_presets[_map.m_mapType].Add(tempPreset);
    }

    InputMap GetMap(InputType _type)
    {
        InputMap temp = null;
        switch (_type)
        {
            case InputType.KEYBOARD:
                {
                    //ADD WHEN DONE
                    break;
                }
            case InputType.NES:
                {
                    //ADD WHEN DONE!!
                    break;
                }
            case InputType.PS4:
                {
                    PS4Map tempMap = ScriptableObject.CreateInstance<PS4Map>();
                    tempMap.Initialise();
                    temp = tempMap;
                    break;
                }
            default:
                {
                    temp = ScriptableObject.CreateInstance<InputMap>();
                    break;
                }
        }
        return temp;
    }

    public void DeletePreset(InputType _type, int _ID)
    {
        InputPreset tempPreset = new InputPreset();
        tempPreset.m_ID = -1;

        foreach (InputPreset preset in m_presets[_type])
        {
            if (preset.m_ID == _ID)
            {
                tempPreset = preset;
            }
        }

        if (tempPreset.m_ID != -1)
        {
            DestroyImmediate(tempPreset.m_map);
            m_presets[_type].Remove(tempPreset);
        }
    }

    public InputType GetType(int _ID)
    {
        foreach (KeyValuePair<InputType, List<InputPreset>> keyValue in m_presets)
        {
            foreach (InputPreset preset in keyValue.Value)
            {
                if (preset.m_ID == _ID)
                {
                    return keyValue.Key;
                }
            }
        }

        return InputType.NULL;
    }

    public InputMap GetPresetMap(InputType _inputType, bool _setInUse = false)
    {
        for (int i = 0; i <= m_presets[_inputType].Count - 1; i++)
        {
            if (!m_presets[_inputType][i].m_inUse)
            {
                InputPreset tempPreset = m_presets[_inputType][i];
                tempPreset.m_inUse = _setInUse;
                m_presets[_inputType][i] = tempPreset;
                return m_presets[_inputType][i].m_map;
            }
        }

        return null;
    }

    public InputMap GetPresetMap(string _name, bool _setInUse = false)
    {
        for (int j = 0; j <= InputTypeCount - 1; j++)
        {
            for (int i = 0; i <= m_presets[(InputType)j].Count - 1; i++)
            {
                if (m_presets[(InputType)j][i].m_name == _name)
                {
                    InputPreset tempPreset = m_presets[(InputType)j][i];
                    tempPreset.m_inUse = _setInUse;
                    m_presets[(InputType)j][i] = tempPreset;
                    return m_presets[(InputType)j][i].m_map;
                }
            }

        }
        return null;
    }

    public InputMap GetPresetMap(InputType _type, string _name, bool _setInUse = false)
    {
        for (int i = 0; i <= m_presets[_type].Count - 1; i++)
        {
            if (m_presets[_type][i].m_name == _name)
            {
                InputPreset tempPreset = m_presets[_type][i];
                tempPreset.m_inUse = _setInUse;
                m_presets[_type][i] = tempPreset;
                return m_presets[_type][i].m_map;
            }
        }

        return null;
    }

    public InputMap GetReferenceMap(int _ID)
    {
        for (int j = 0; j <= InputTypeCount - 1; j++)
        {
            for (int i = 0; i <= m_presets[(InputType)j].Count - 1; i++)
            {
                if (m_presets[(InputType)j][i].m_ID == _ID)
                {
                    return m_presets[(InputType)j][i].m_map;
                }
            }
        }
        return null;
    }

    public void SetReferenceMap(int _ID, InputMap _map)
    {
        InputPreset preset = new InputPreset();
        preset.m_name = "Null";
        preset.m_inUse = false;

        InputType type = InputType.NULL;
        int iter = -1;

        for (int j = 0; j <= InputTypeCount - 1; j++)
        {
            for (int i = 0; i <= m_presets[(InputType)j].Count - 1; i++)
            {
                if (m_presets[(InputType)j][i].m_ID == _ID)
                {
                    preset = m_presets[(InputType)j][i];
                    type = (InputType)j;
                    iter = i;
                }
            }
        }

        if (preset.m_name != "Null")
        {
            preset.m_map = _map;
            m_presets[type][iter] = preset;
        }
    }

    public InputPreset GetReferencePreset(int _ID)
    {
        InputPreset NULL;
        NULL.m_name = "NULL";
        NULL.m_ID = -1;
        NULL.m_map = null;
        NULL.m_inUse = false;

        for (int j = 0; j <= InputTypeCount - 1; j++)
        {
            for (int i = 0; i <= m_presets[(InputType)j].Count - 1; i++)
            {
                if (m_presets[(InputType)j][i].m_ID == _ID)
                {
                    return m_presets[(InputType)j][i];
                }
            }
        }
        return NULL;
    }


    public List<InputPreset> GetAllMap(InputType _inputType)
    {
        if (_inputType != InputType.NULL)
        {
            return m_presets[_inputType];
        }
        return null;
    }

    public string GetTypeName(InputType _type)
    {
        string returnString = "NULL";

        switch (_type)
        {
            case InputType.KEYBOARD:
                {
                    returnString = "Keyboard";
                    break;
                }
            case InputType.NES:
                {
                    returnString = "NES";
                    break;
                }
            case InputType.PS4:
                {
                    returnString = "PS4";
                    break;
                }
            default:
                {
                    Debug.LogWarning("Not valid type name");
                    break;
                }
        }
        return returnString;
    }

    public void ResetMap(InputMap _map)
    {
        if (_map)
        {
            for (int i = 0; i <= m_presets[_map.m_mapType].Count - 1; i++)
            {
                if (m_presets[_map.m_mapType][i].m_map == _map)
                {
                    InputPreset tempPreset = m_presets[_map.m_mapType][i];
                    tempPreset.m_inUse = false;
                    m_presets[_map.m_mapType][i] = tempPreset;
                }
            }
        }
    }

    public void ResetMaps()
    {
        if (m_presets.Count != 0)
        {
            for (int j = 0; j <= InputTypeCount - 1; j++)
            {
                for (int i = 0; i <= m_presets[(InputType)j].Count - 1; i++)
                {
                    DestroyImmediate(m_presets[(InputType)j][i].m_map);
                }
            }
        }

        m_presets.Clear();
        for (int iter = 0; iter <= InputTypeCount - 1; iter++)
        {
            m_presets.Add((InputType)iter, new List<InputPreset>());
        }
    }
}
