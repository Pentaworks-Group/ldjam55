using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManagerBehaviour : MonoBehaviour
{

    private class SheduledEvent
    {
        public float Time;
        public Action EventAction;
        public string EventName;
        public float Interval;
    }

    private List<SheduledEvent> sheduledEvents = new List<SheduledEvent>();
    private Dictionary<int, List<SheduledEvent>> eventsByObjectID = new();
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
            SheduledEvent sheduledEvent = sheduledEvents[0];
            //sheduledEvents.RemoveAt(0);
            sheduledEvent.EventAction();
            sheduledEvents.Remove(sheduledEvent);
            if (sheduledEvent.Interval > 0)
            {
                sheduledEvent.Time += sheduledEvent.Interval;
                InsertEvent(sheduledEvent);
            }
            else
            {
                if (sheduledEvents.Count > 0)
                {
                    nextEventTime = sheduledEvent.Time;
                }
                else
                {
                    nextEventTime = 0;
                }
            }

        }
    }

    private void AddToEventsByObjectID(int objectId, SheduledEvent sheduledEvent)
    {
        if (!eventsByObjectID.TryGetValue(objectId, out var sheduledEs))
        {
            sheduledEs = new();
            eventsByObjectID[objectId] = sheduledEs;
        }
        sheduledEs.Add(sheduledEvent);
    }

    public void RegisterEvent(float time, Action action, string eventName, int objectId, float Interval = 0, bool fromNow = true)
    {
        var newEvent = new SheduledEvent { EventName = eventName, EventAction = action, Interval = Interval };
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
        AddToEventsByObjectID(objectId, newEvent);
        InsertEvent(newEvent);
    }

    private void InsertEvent(SheduledEvent newEvent)
    {
        float eventTime = newEvent.Time;
        for (int i = 0; i < sheduledEvents.Count; i++)
        {
            var shEvent = sheduledEvents[i];
            if (eventTime < shEvent.Time)
            {
                sheduledEvents.Insert(i, newEvent);
                nextEventTime = sheduledEvents[0].Time;
                return;
            }
        }
        sheduledEvents.Add(newEvent);
        if (sheduledEvents.Count <= 1)
        {
            nextEventTime = sheduledEvents[0].Time;
        }
    }

    public void UnregisterByObjectID(int objectId)
    {
        if (eventsByObjectID.TryGetValue(objectId, out var sheduledEs))
        {
            foreach (var sheduledE in sheduledEs)
            {
                sheduledEvents.Remove(sheduledE);
            }
            eventsByObjectID.Remove(objectId);

            if (sheduledEvents.Count > 0)
            {
                nextEventTime = sheduledEvents[0].Time;
            }
            else
            {
                nextEventTime = default;
            }
        }
    }

    public void UnregisterEvent(string eventName, bool startingWith = false)
    {
        for (int i = sheduledEvents.Count - 1; i >= 0; i--)
        {
            var shEvent = sheduledEvents[i];
            if (startingWith)
            {
                if (shEvent.EventName.StartsWith(eventName))
                {
                    sheduledEvents.RemoveAt(i);
                }
            }
            else
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
        }
        else
        {
            nextEventTime = default;
        }
    }
}
