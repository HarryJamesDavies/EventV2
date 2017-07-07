using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager m_instance = null;

    public List<GameObject> m_presetUI = new List<GameObject>();
    public List<Transform> m_UIGroups = new List<Transform>();
    public List<Transform> m_tempUIGroups = new List<Transform>();

    public Transform m_currentGroup;
    public Transform m_prevGroup;

    // Use this for initialization
    void Awake()
    {
        if (m_instance)
        {
            Destroy(this.gameObject);
        }
        else
        {
            m_instance = this;
        }

        GameObject UIHolder = GameObject.FindGameObjectWithTag("UI");
        Transform Canvas = UIHolder.transform.Find("Canvas");

        for(int iter = 0; iter <= Canvas.childCount - 1; iter++)
        {
            m_UIGroups.Add(Canvas.GetChild(iter));

            if(m_UIGroups[m_UIGroups.Count - 1].gameObject.activeInHierarchy)
            {
                m_currentGroup = m_UIGroups[m_UIGroups.Count - 1];
                m_prevGroup = m_currentGroup;
            }
        }  
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ChangeUIGroup(string _pathName)
    {
        Transform NewGroup = null;

        for(int iter = 0; iter <= m_UIGroups.Count - 1; iter++)
        {
            if(m_UIGroups[iter].transform.name == _pathName)
            {
                NewGroup = m_UIGroups[iter];
            }
        }

        if(NewGroup != null)
        {
            m_prevGroup = m_currentGroup;
            m_currentGroup = NewGroup;
            m_prevGroup.gameObject.SetActive(false);
            m_currentGroup.gameObject.SetActive(true);
            EventManager.m_instance.AddEvent(Events.Event.UI_UICHANGED);
        }
        else
        {
            Debug.Log("No UI Group of that name!");
        }
    }

    public void AddTempMessageUI(string _name, string _message)
    {
        //GameObject UIHolder = GameObject.FindGameObjectWithTag("UI");
        //Transform Canvas = UIHolder.transform.Find("Canvas");
        //GameObject tempObject = Instantiate(m_presetUI[0]);
        //tempObject.transform.SetParent(Canvas);

        //tempObject.transform.GetChild(0).GetComponentInChildren<Text>().text = _message;
    }
}
