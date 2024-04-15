using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;

public class TriggerHandler 
{

    public Dictionary<string, List<ActionContainer>> FieldCreeperChangeEvent = new Dictionary<string, List<ActionContainer>>();
    public Dictionary<int, ActionContainer> ObjectToActionContainer = new Dictionary<int, ActionContainer>();
    public class ActionContainer
    {
        public Field[] fields; public string creeperId; public float amount; public int objectID; public Action<Creeper> triggeredAction;
    }


    public void Reset()
    {
        FieldCreeperChangeEvent = new Dictionary<string, List<ActionContainer>>();
        ObjectToActionContainer = new Dictionary<int, ActionContainer>();
    }
    public void CreeperTrigger(string creeperId, float amount, Action<Creeper> triggeredAction, object gObject, params Field[] fields)
    {
        int objectID = gObject.GetHashCode();
        var actionContainer = new ActionContainer() { triggeredAction = triggeredAction, amount = amount, creeperId = creeperId, fields = fields, objectID = objectID };
        ObjectToActionContainer.Add(objectID, actionContainer);
        foreach (var field in fields)
        {
            if (!FieldCreeperChangeEvent.TryGetValue(field.ID, out var actions))
            {
                actions = new List<ActionContainer>();
                FieldCreeperChangeEvent.Add(field.ID, actions);
            }
            actions.Add(actionContainer);
        }
    }

    public void InvokeTriggered(Field field, Creeper oldCreeper)
    {
        if (FieldCreeperChangeEvent.TryGetValue(field.ID, out List<ActionContainer> actionContainers))
        {
            for (int i = actionContainers.Count - 1; i >= 0; i--)
            {
                var container = actionContainers[i];
                if (container.creeperId != null && field.Creep.Creeper.ID == container.creeperId)
                {
                    container.triggeredAction.Invoke(oldCreeper);
                    UnRegisterTrigger(container);
                }
            }
        }
    }


    public void UnRegisterByObjectID(object objectToDeregister)
    {
        int objectID = objectToDeregister.GetHashCode();
        if (ObjectToActionContainer.TryGetValue(objectID, out var cc))
        {
            UnRegisterTrigger(cc);
        }
    }

    private void UnRegisterTrigger(ActionContainer container)
    {
        if (ObjectToActionContainer.ContainsKey(container.objectID))
        {
            ObjectToActionContainer.Remove(container.objectID);
        }
        foreach (var field in container.fields)
        {
            if (FieldCreeperChangeEvent.TryGetValue(field.ID, out var actions))
            {
                actions.Remove(container);
                if (actions.Count == 0)
                {
                    FieldCreeperChangeEvent.Remove(field.ID);
                }
            }

        }
    }
}
