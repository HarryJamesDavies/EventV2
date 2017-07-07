using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager m_instance;
    public delegate void EventTrigger(object data = null);

    [Serializable]
    public class EventList
    {
        public EventTrigger m_eventTrigger;
        public Events.Event m_eventType;
    }

    [Serializable]
    public class EventQueue
    {
        public Events.Event m_eventType;
        public object m_data;
    }

    public List<EventList> m_eventList = new List<EventList>();
    public List<EventQueue> m_eventBuffer = new List<EventQueue>();
    public List<EventQueue> m_tempBuffer = new List<EventQueue>();

    void Awake()
    {
        if (m_instance)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            m_instance = this;
        }

        if (m_eventList.Count == 0)
        {
            for (int i = 0; i <= (int)Events.Event.Count - 1; i++)
            {
                EventList tempList = new EventList();
                tempList.m_eventType = (Events.Event)i;
                tempList.m_eventTrigger = null;

                m_eventList.Add(tempList);
            }
        }
    }

    void Update()
    {
        if(m_tempBuffer != null)
        {
            foreach (EventQueue events in m_tempBuffer)
            {
                m_eventBuffer.Add(events);
            }

            m_tempBuffer.Clear();
        }
    }

    // Update is called once per frame
    void LateUpdate ()
    {
        BufferHandler();
    }

    void BufferHandler()
    {
        foreach(EventQueue iterEvent in m_eventBuffer)
        {
            TriggerEvent(iterEvent);
        }

        if (m_eventBuffer != null)
        {
            m_eventBuffer.Clear();
        }
    }

    /// <summary>Calls all functions subscribe to this event, using the EventQueue struct </summary>
    void TriggerEvent(EventQueue queue)
    {
        if (m_eventList[(int)queue.m_eventType].m_eventTrigger != null && queue.m_data == null)
        {
            m_eventList[(int)queue.m_eventType].m_eventTrigger();
        }
        else if (m_eventList[(int)queue.m_eventType].m_eventTrigger != null && queue.m_data != null)
        {
            m_eventList[(int)queue.m_eventType].m_eventTrigger(queue.m_data);
        }
    }

    public void AddEvent(Events.Event _event, object _data = null)
    {
        EventQueue queue = new EventQueue();
        queue.m_eventType = _event;
        queue.m_data = _data;
        m_tempBuffer.Add(queue);
    }

    /// <summary>Adds function that requires a data object to call list when this event is triggered</summary>
    public void SubscribeToEvent(Events.Event _event, EventTrigger _trigger)
    {
        EventList tempList = m_eventList[(int)_event];
        tempList.m_eventTrigger += _trigger;
        m_eventList[(int)_event] = tempList;
    }

    /// <summary>Removes function that requires a data object from call list of this event </summary>
    public void UnsubscribeToEvent(Events.Event _event, EventTrigger _trigger)
    {
        EventList tempList = m_eventList[(int)_event];
        tempList.m_eventTrigger -= _trigger;
        m_eventList[(int)_event] = tempList;
    }
}
