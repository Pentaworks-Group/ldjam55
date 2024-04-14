using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManagerBehaviour : MonoBehaviour
{

    private class SheduledEvent
    {
        public float Time;
        public Action<TimeManagerBehaviour> EventAction;
        public string EventName;
    }

    private List<SheduledEvent> sheduledEvents = new List<SheduledEvent>();
    private float nextEventTime;


    private void Awake()
    {
        nextEventTime = 0;
    }

    private void OnEnable()
    {
        nextEventTime = 0;
    }


    void Update()
    {
        if (nextEventTime != default && nextEventTime < Time.time)
        {
            sheduledEvents[0].EventAction(this);
            sheduledEvents.RemoveAt(0);
            if (sheduledEvents.Count > 0)
            {
                nextEventTime = sheduledEvents[0].Time;
            } 
            else
            {
                nextEventTime = 0;
            }
        }
    }

    public void RegisterEvent(float time, Action<TimeManagerBehaviour> action, string eventName, bool fromNow = true)
    {
        var newEvent = new SheduledEvent { EventName = eventName, EventAction = action };
        float eventTime;
        if (fromNow)
        {
            eventTime = time + Time.time;
        }
        else
        {
            eventTime = time;
        }
        newEvent.Time = eventTime;
        for (int i = 0; i < sheduledEvents.Count; i++)
        {
            var shEvent = sheduledEvents[i];
            if (eventTime < shEvent.Time)
            {
                sheduledEvents.Insert(i, newEvent);
                nextEventTime = eventTime;
                return;
            }
        }
        sheduledEvents.Add(newEvent);
        if (sheduledEvents.Count <= 1)
        {
            nextEventTime = sheduledEvents[0].Time;
        }
    }

    public void UnregisterEvent(string eventName, bool startingWith = false)
    {
        for (int i = sheduledEvents.Count - 1; i >= 0 ; i--)
        {
            var shEvent = sheduledEvents[i];
            if (startingWith)
            {
                if (shEvent.EventName.StartsWith(eventName))
                {
                    sheduledEvents.RemoveAt(i);
                }
            } else
            {
                if (shEvent.EventName == eventName)
                {
                    sheduledEvents.RemoveAt(i);
                }
            }
        }
        if (sheduledEvents.Count > 0)
        {
            nextEventTime = sheduledEvents[0].Time;
        } else
        {
            nextEventTime = default;
        }
    }
}
