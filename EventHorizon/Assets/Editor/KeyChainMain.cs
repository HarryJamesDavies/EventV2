using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class KeyChainMain : EditorWindow
{
    public struct MapTag
    {
        public string Name;
        public int ID;
    };

    public float m_screenMaxWidth;
    public float m_screenMaxHeight;

    private float m_viewPortHeight = 500.0f;
    private float m_viewPortWidth = 500.0f;

    private float m_menuBarHeight = 100.0f;
    private float m_menuBarWidth = 600.0f;
    private float m_menuBarMinimumHeight = 25.0f;

    private float m_toolBarWidth = 100.0f;
    private float m_toolBarHeight = 500.0f;
    private float m_toolBarMinimumWidth = 27.5f;
    private bool m_toolBarToogle = false;

    private bool m_menuBarToogle = false;
    private int m_menuBarTab = 1;
    private string[] m_menuTabNames = new string[] { "Settings", "Main", "Load" };

    public string m_presetName = "";
    public string m_deleteName = "";
    public InputMapManager.InputType m_presetType = InputMapManager.InputType.NULL;
    private string[] m_typeString;
    private int m_typeSelected = (int)InputMapManager.InputType.NULL;

    private Dictionary<InputMapManager.InputType, List<MapTag>> m_presetNames = new Dictionary<InputMapManager.InputType, List<MapTag>>();
    private int m_presetSelected = -1;
    private Vector2 m_presetScrollPosition = Vector2.zero;

    private int m_openPresetSelected = -1;
    private List<MapTag> m_openPresets = new List<MapTag>();

    private bool m_initialisedSettings = false;
    private bool m_initialisedPreset = false;
    private bool m_initialisedOpen = false;

    public int m_deleteID = -1;

    public static string m_resourcePath = "";

    [MenuItem("Window/KeyChain")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(KeyChainMain), false, "KeyChain").ShowUtility();
    }

    void OnEnable()
    {
        m_typeString = Enum.GetNames(typeof(InputMapManager.InputType));
        KeyChainManager.InputMapManager.m_initalised = false;
        KeyChainManager.InputMapManager.Initialise();

        m_initialisedSettings = false;
        InitialiseSettings();
        m_initialisedPreset = false;
        InitialisePresets();
        m_initialisedOpen = false;
        InitialiseOpen();
    }

    void InitialiseSettings()
    {
        if (!m_initialisedSettings)
        {
            InputSettingsData data = Resources.Load("InputSettings/KeyChainSettings") as InputSettingsData;

            if (data != null)
            {
                m_resourcePath = data.m_resourcePath;

                ControllerManager.m_maxNumInputs = data.m_maxNumInputs;
                ControllerManager.m_enableKeyboard = data.m_enableKeyboard;
                ControllerManager.m_enableNES = data.m_enableNES;
                ControllerManager.m_enablePS4 = data.m_enablePS4;
            }
            else
            {
                Debug.Log("No Saved Settings");

                if (EditorUtility.DisplayDialog("Set Resource Folder Path",
            "Resource Path is shorter than Project Path.\nResource Path must be in a sub-folder of Assets Folder.\nDo you wish to reset it now",
            "Yes", "No"))
                {
                    SetResourcePath();
                }

                CreateSettings();
            }

            m_initialisedSettings = true;
        }
    }

    void CreateSettings()
    {
        InputSettingsData asset = ScriptableObject.CreateInstance<InputSettingsData>();
        asset.Initialise(m_resourcePath);

        string path = m_resourcePath + "/InputSettings/";
        string name = "KeyChainSettings.asset";

        if (AssetDatabase.LoadAssetAtPath(path + name, typeof(InputSettingsData)) != null)
        {
            AssetDatabase.DeleteAsset(path + name);
            AssetDatabase.Refresh();
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + name);

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void InitialisePresets()
    {
        if (!m_initialisedPreset)
        {
            m_presetNames.Clear();

            for (int j = 0; j <= (int)InputMapManager.InputTypeCount - 1; j++)
            {
                m_presetNames.Add((InputMapManager.InputType)j, new List<MapTag>());
            }

            for (int j = 0; j <= (int)InputMapManager.InputTypeCount - 1; j++)
            {
                foreach (InputMapManager.InputPreset preset in KeyChainManager.InputMapManager.GetAllMap((InputMapManager.InputType)j))
                {
                    MapTag tempTag;
                    tempTag.Name = preset.m_name;
                    tempTag.ID = preset.m_ID;

                    m_presetNames[(InputMapManager.InputType)j].Add(tempTag);
                }
            }

            m_initialisedPreset = true;
        }
    }

    void InitialiseOpen()
    {
        if (!m_initialisedOpen)
        {
            m_openPresets.Clear();
            m_initialisedOpen = true;
        }
    }

    void OnGUI()
    {
        //Gets the max screen space internal Rects can inhabit
        m_screenMaxWidth = Screen.width;
        m_screenMaxHeight = Screen.height;

        if (m_menuBarToogle)
        {
            m_menuBarHeight = m_menuBarMinimumHeight;
            m_menuBarWidth = m_screenMaxWidth;
        }
        else
        {
            m_menuBarHeight = m_screenMaxHeight / 3.0f;
            m_menuBarWidth = m_screenMaxWidth;
        }

        if (m_toolBarToogle)
        {
            m_toolBarWidth = m_toolBarMinimumWidth;
            m_toolBarHeight = m_screenMaxHeight - m_menuBarHeight;
        }
        else
        {
            m_toolBarWidth = m_screenMaxWidth / 5.0f;
            m_toolBarHeight = m_screenMaxHeight - m_menuBarHeight;
        }

        m_viewPortHeight = m_screenMaxHeight - m_menuBarHeight;
        m_viewPortWidth = m_screenMaxWidth - m_toolBarWidth;

        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            {
                DrawViewPort();

                DrawToolBar();
            }
            GUILayout.EndHorizontal();

            DrawMenuBar();
        }
        GUILayout.EndVertical();
    }

    void DrawViewPort()
    {
        GUILayout.Space(-3.5f);
        EditorGUILayout.BeginVertical(GUILayout.Width(m_viewPortWidth), GUILayout.Height(m_viewPortHeight));
        {
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            float m_titleBarHeight = 25.0f;
            float m_openPresetBarHeight = 25.0f;

            GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            Rect presetBar = EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(m_viewPortWidth), GUILayout.Height(m_openPresetBarHeight), GUILayout.MinHeight(m_openPresetBarHeight));
            {
                GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                if (m_openPresets.Count >= 0)
                {
                    List<string> allNames = new List<string>();
                    foreach (MapTag tag in m_openPresets)
                    {
                        string name = tag.Name;

                        if(KeyChainManager.InputMapManager.GetReferenceMap(tag.ID).CheckAlterations())
                        {
                            name += "*";
                        }

                        allNames.Add(name);
                    }

                    m_openPresetSelected = GUILayout.Toolbar(m_openPresetSelected, allNames.ToArray(), GUILayout.Width(m_openPresets.Count * 100.0f));

                    Event currentEvent = Event.current;

                    if (currentEvent.type == EventType.ContextClick)
                    {
                        Vector2 mousePos = currentEvent.mousePosition;
                        if (presetBar.Contains(mousePos))
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddItem(new GUIContent("Close Tab"), false, ClosePreset);
                            menu.ShowAsContext();
                            currentEvent.Use();
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(-5.0f);

            EditorGUILayout.BeginHorizontal("Box", GUILayout.MinHeight(m_titleBarHeight));
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.white;
                GUILayout.Label("ViewPort", style);
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

            GUILayout.Space(-3.5f);
            //Sets up the main Rect in which all sub Rects will sit
            EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(m_viewPortWidth), GUILayout.Height(m_viewPortHeight - m_titleBarHeight - m_openPresetBarHeight));
            {
                if (m_openPresetSelected != -1)
                {
                    GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical("Box", GUILayout.Width((m_viewPortWidth / 2) - 7.0f), GUILayout.Height(m_viewPortHeight - m_titleBarHeight - m_openPresetBarHeight - 8.0f));
                        {
                            DrawDefaultActions(KeyChainManager.InputMapManager.GetReferenceMap(m_openPresets[m_openPresetSelected].ID));
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("Box", GUILayout.Width((m_viewPortWidth / 2) - 7.0f), GUILayout.Height(m_viewPortHeight - m_titleBarHeight - m_openPresetBarHeight - 8.0f));
                        {
                            DrawCustomActions(KeyChainManager.InputMapManager.GetReferenceMap(m_openPresets[m_openPresetSelected].ID));
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                    style.normal.textColor = Color.white;
                    GUILayout.Label("Open InputMap Preset Below!", style);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    }

    public void ClosePreset()
    {
        if (m_openPresets.Count != 0)
        {
            MapTag tag = m_openPresets[m_openPresetSelected];
            m_openPresets.Remove(tag);

            m_openPresetSelected = 0;
            if (m_openPresets.Count == 0)
            {
                m_openPresetSelected = -1;
            }
        }
    }

    void DrawDefaultActions(InputMap _map)
    {
        EditorGUILayout.IntField("Default Actions:", _map.m_defaultActionCount);

        EditorGUILayout.Space();

        for (int i = 0; i <= _map.m_defaultActionCount - 1; i++)
        {
            EditorGUILayout.BeginHorizontal();

            int dataLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;

            float width = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60.0f;

            EditorGUILayout.TextField("Action", _map.m_defaultAction[i].m_name);

            EditorGUILayout.Popup("Key", _map.GetKeyIndex(_map.m_defaultAction[i].m_button.m_key), _map.m_keyNames.ToArray());

            EditorGUI.indentLevel = dataLevel;
            EditorGUIUtility.labelWidth = width;

            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawCustomActions(InputMap _map)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.IntField("Custom Actions:", _map.m_customActionCount);

            Color prevColor = GUI.color;
            GUI.color = Color.green;
            if (GUILayout.Button("+", GUILayout.Width(20.0f)))
            {
                KeyChainManager.InputMapManager.GetReferenceMap(m_openPresets[m_openPresetSelected].ID).AddCustomAction();
            }
            GUI.color = prevColor;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        for (int i = 0; i <= _map.m_customActionCount - 1; i++)
        {
            EditorGUILayout.BeginHorizontal();
            {
                int dataLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;

                float width = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 60.0f;

                _map.SetCustomAction(i, EditorGUILayout.TextField("Action", _map.m_customAction[i].m_name));

                _map.SetCustomButton(i, _map.m_keyNames[EditorGUILayout.Popup("Key", _map.GetKeyIndex(_map.m_customAction[i].m_button.m_key), _map.m_keyNames.ToArray())]);

                Color prevColor = GUI.color;
                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(20.0f)))
                {
                    InputMap map = KeyChainManager.InputMapManager.GetReferenceMap(m_openPresets[m_openPresetSelected].ID);
                    map.RemoveAction(map.m_customAction[i].m_name);
                }
                GUI.color = prevColor;

                EditorGUI.indentLevel = dataLevel;
                EditorGUIUtility.labelWidth = width;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawToolBar()
    {
        float m_titleBarHeight = 25.0f;
        GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        GUILayout.Space(-8.5f);
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Space(-0.5f);

            GUILayout.BeginHorizontal("Box", GUILayout.Width(m_toolBarWidth - 0.25f), GUILayout.Height(m_titleBarHeight));
            {
                if (!m_toolBarToogle)
                {
                    GUILayout.Label("Tools", EditorStyles.boldLabel);
                }
                m_toolBarToogle = GUILayout.Toggle(m_toolBarToogle, "X", "Button", GUILayout.Width(20.0f));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(-3.5f);

            //Sets up the main Rect in which all sub Rects will sit
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(m_toolBarWidth - 0.25f), GUILayout.MinWidth(m_toolBarMinimumWidth), GUILayout.Height(m_toolBarHeight - m_titleBarHeight));
            {
                if (!m_toolBarToogle && m_openPresets.Count != 0)
                {
                    GUILayout.BeginVertical("Box", GUILayout.Width(m_toolBarWidth - 8.0f), GUILayout.Height((m_toolBarHeight - m_titleBarHeight) / 4.0f));
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                        style.normal.textColor = Color.black;
                        GUILayout.Label("Save Preset", style);

                        InputMapManager.InputPreset preset = KeyChainManager.InputMapManager.GetReferencePreset(m_openPresets[m_openPresetSelected].ID);

                        if (GUILayout.Button("Save Preset"))
                        {
                            preset.m_map.SetPreviousState();
                            CreateMapAsset(preset.m_map, preset.m_name, preset.m_map.m_mapType.ToString());
                        }
                    }
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.Label("   ");
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    }

    void DrawMenuBar()
    {
        GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        float m_titleBarHeight = 25.0f;

        GUILayout.Space(-12.5f);
        EditorGUILayout.BeginVertical();
        {
            int prevMenuBar = m_menuBarTab;
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(-0.5f);
                GUILayout.BeginHorizontal("Box", GUILayout.Width(m_menuBarWidth), GUILayout.Height(m_titleBarHeight));
                {
                    m_menuBarTab = GUILayout.Toolbar(m_menuBarTab, m_menuTabNames);
                    m_menuBarToogle = GUILayout.Toggle(m_menuBarToogle, "X", "Button", GUILayout.Width(20.0f));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(-4.5f);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(-0.5f);
                if (!m_menuBarToogle)
                {
                    GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

                    switch (m_menuBarTab)
                    {
                        case 0:
                            {
                                //Settings
                                DrawSettingSection(m_titleBarHeight);
                                break;
                            }
                        case 1:
                            {
                                //Main
                                DrawMainSection(m_titleBarHeight);
                                break;
                            }
                        case 2:
                            {
                                if(prevMenuBar != m_menuBarTab)
                                {
                                    InitialisePresets();
                                }
                                //Load
                                DrawLoadSection(m_titleBarHeight);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    }

    void DrawSettingSection(float _titleBarHeight)
    {
        GUILayout.BeginVertical("Box", GUILayout.Width(m_menuBarWidth - 10.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 15.0f));
        {
            GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            GUILayout.BeginHorizontal(GUILayout.Width(m_menuBarWidth - 8.0f));
            {
                GUILayout.BeginVertical("Box", GUILayout.Width(m_menuBarWidth / 6.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 14.0f));
                {
                    GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                    style.normal.textColor = Color.black;
                    GUILayout.Label("Paths", style);

                    EditorGUILayout.TextField(m_resourcePath);
                    if (GUILayout.Button("Set Resource Path"))
                    {
                        SetResourcePath();
                        CreateSettings();
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box", GUILayout.Width(m_menuBarWidth / 6.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 14.0f));
                {
                    GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                    style.normal.textColor = Color.black;
                    GUILayout.Label("Controllers", style);

                    GUILayout.BeginHorizontal();
                    {
                        bool keyboard = ControllerManager.m_enableKeyboard;
                        bool NES = ControllerManager.m_enableNES;
                        bool PS4 = ControllerManager.m_enablePS4;

                        ControllerManager.m_enableKeyboard = GUILayout.Toggle(ControllerManager.m_enableKeyboard, "Keyboard");
                        ControllerManager.m_enableNES = GUILayout.Toggle(ControllerManager.m_enableNES, "NES");
                        ControllerManager.m_enablePS4 = GUILayout.Toggle(ControllerManager.m_enablePS4, "PS4");

                        if(ControllerManager.m_enableKeyboard != keyboard || ControllerManager.m_enableNES != NES 
                            || ControllerManager.m_enablePS4 != PS4)
                        {
                            CreateSettings();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void SetResourcePath()
    {
        string projectPath = Application.dataPath;

        m_resourcePath = EditorUtility.SaveFolderPanel("Resource Folder Path", "", "");

        if (m_resourcePath.Length <= projectPath.Length)
        {
            if (EditorUtility.DisplayDialog("Reset Resource Folder Path",
                                "Resource Path is shorter than Project Path.\nResource Path must be in a sub-folder of Assets Folder.\nDo you wish to reset it now",
                                "Yes", "No"))
            {
                SetResourcePath();
            }
            else
            {
                m_resourcePath = "Assets";
                return;
            }
        }
        else
        {
            projectPath = projectPath.Replace("Assets", " ");
            m_resourcePath = m_resourcePath.Remove(0, projectPath.Length - 1);

        }
    }

    void DrawMainSection(float _titleBarHeight)
    {
        GUILayout.BeginHorizontal("Box", GUILayout.Width(m_menuBarWidth), GUILayout.Height(m_menuBarHeight - _titleBarHeight));
        {
            GUILayout.Space(4.0f);
            GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            GUILayout.BeginVertical("Box", GUILayout.Width(m_menuBarWidth / 6.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 14.0f));
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.black;
                GUILayout.Label("New Map", style);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Name");
                    GUILayout.Space(20.0f);
                    m_presetName = EditorGUILayout.TextField(m_presetName);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Type");
                    GUILayout.Space(25.0f);
                    m_presetType = (InputMapManager.InputType)EditorGUILayout.EnumPopup(m_presetType);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Create Map"))
                {
                    if (m_presetType != InputMapManager.InputType.NULL)
                    {
                        KeyChainManager.InputMapManager.AddEmptyPreset(m_presetName, m_presetType);
                        m_initialisedPreset = false;
                        m_presetName = "";
                    }
                    else
                    {
                        Debug.LogWarning("Don't create Input Map of type NULL");
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box", GUILayout.Width(m_menuBarWidth / 6.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 14.0f));
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.black;
                GUILayout.Label("Duplicate Map", style);

               
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box", GUILayout.Width(m_menuBarWidth / 6.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 14.0f));
            {
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.black;
                GUILayout.Label("Delete Map", style);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Name");
                    GUILayout.Space(20.0f);
                    m_deleteName = EditorGUILayout.TextField(m_deleteName);
                    m_deleteID = KeyChainManager.InputMapManager.GetID(m_presetName);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Delete Map"))
                {
                    InputMapManager.InputType type = KeyChainManager.InputMapManager.GetType(m_deleteID);

                    if (type != InputMapManager.InputType.NULL)
                    {
                        DeletePreset(type);
                    }
                    else
                    {
                        Debug.LogWarning("Preset does not exist");
                    }
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
    }

    void DeletePreset(InputMapManager.InputType _type)
    {
        string path = m_resourcePath + "/" + "InputMap/ " + _type.ToString() + "/" + m_presetName + ".asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.Refresh();
        KeyChainManager.InputMapManager.DeletePreset(_type, m_deleteID);
        m_initialisedPreset = false;
        InitialisePresets();
        m_initialisedOpen = false;
        InitialiseOpen();
        m_deleteName = "";
    }

    void DrawLoadSection(float _titleBarHeight)
    {
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        GUILayout.BeginVertical("Box", GUILayout.Width(m_menuBarWidth - 14.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 33.0f));
        {
            GUILayout.BeginHorizontal();
            {
                GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                int prevType = m_typeSelected;
                m_typeSelected = EditorGUILayout.Popup(m_typeSelected, m_typeString, GUILayout.Width(m_menuBarWidth / 6));
                m_presetType = (InputMapManager.InputType)m_typeSelected;
                if (m_typeSelected != prevType)
                {
                    m_presetSelected = -1;
                }

                GUILayout.Space(m_menuBarWidth / 1.55f);

                if (GUILayout.Button("Reset Presets", GUILayout.Width(200.0f)))
                {
                    KeyChainManager.InputMapManager.m_initalised = false;
                    KeyChainManager.InputMapManager.Initialise();
                    m_initialisedPreset = false;
                    InitialisePresets();
                }
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            GUILayout.BeginHorizontal("Box");
            {
                string presetName = "";
                int presetID = -1;

                m_presetScrollPosition = EditorGUILayout.BeginScrollView(m_presetScrollPosition,
                GUILayout.Width(m_menuBarWidth - 14.0f), GUILayout.Height(m_menuBarHeight - _titleBarHeight - 35.0f));
                {
                    List<string> allNames = new List<string>();
                    List<int> allID = new List<int>();

                    if (m_presetType == InputMapManager.InputType.NULL)
                    {
                        for (int j = 0; j <= (int)InputMapManager.InputTypeCount - 1; j++)
                        {
                            foreach(MapTag tag in m_presetNames[(InputMapManager.InputType)j])
                            {
                                allNames.Add(tag.Name);
                                allID.Add(tag.ID);
                            }
                        }
                    }
                    else
                    {
                        foreach (MapTag tag in m_presetNames[m_presetType])
                        {
                            allNames.Add(tag.Name);
                            allID.Add(tag.ID);
                        } 
                    }

                    m_presetSelected = GUILayout.SelectionGrid(m_presetSelected, allNames.ToArray(), 3);

                    if (m_presetSelected != -1)
                    {
                        presetName = allNames[m_presetSelected];
                        presetID = allID[m_presetSelected];

                        OpenPreset(presetName, presetID);
                        m_presetSelected = -1;
                    }
                    allNames.Clear();
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void OpenPreset(string _selected, int _selectedID)
    {
        if (CheckPresetOpen(_selectedID))
        {
            MapTag tag;
            tag.Name = _selected;
            tag.ID = _selectedID;

            m_openPresets.Add(tag);
            m_openPresetSelected = m_openPresets.Count - 1;

            KeyChainManager.InputMapManager.GetReferenceMap(m_openPresets[m_openPresetSelected].ID).SetPreviousState();
        }
    }

    bool CheckPresetOpen(int _ID)
    {
        foreach(MapTag tag in m_openPresets)
        {
            if(tag.ID == _ID)
            {
                return false;
            }
        }
        return true;
    }

    public void CreateMapAsset(InputMap _map, string _name, string _folder)
    {
        if (m_resourcePath != "")
        {
            MapData asset = ScriptableObject.CreateInstance<MapData>();
            asset.Initialise(_map, _name);

            string path = m_resourcePath + "/InputMap/" + _folder + "/";
            string name = _name + ".asset";

            if (AssetDatabase.LoadAssetAtPath(path + name, typeof(MapData)) != null)
            {
                if (EditorUtility.DisplayDialog("Assets already exists",
                                    "Would you like to overwrite the old file?",
                                    "Yes", "No"))
                {
                    AssetDatabase.DeleteAsset(path + name);
                    AssetDatabase.Refresh();
                }
                else
                {
                    KeyChainManager.InputMapManager.AddDuplicatePreset(_name, _map);
                }
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + name);

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogWarning("Resource path not set!");
        }
    }
}