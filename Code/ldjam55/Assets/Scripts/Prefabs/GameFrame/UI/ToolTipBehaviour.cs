using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ToolTipBehaviour : MonoBehaviour
{
    private GameObject Parent;
    private GameObject ToolTipObject;
    public TMP_Text TextField;

    public string Text;

    private void Awake()
    {
        Parent = this.transform.parent.gameObject;
        ToolTipObject = this.transform.GetChild(0).gameObject;

        EventTrigger trigger = Parent.AddComponent<EventTrigger>();

        AddTrigger(trigger, EventTriggerType.PointerEnter, Show);
        AddTrigger(trigger, EventTriggerType.PointerExit, Hide);

        TextField.text = Text;
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry()
        {
            eventID = type,
        };

        entry.callback.AddListener(action);

        trigger.triggers.Add(entry);
    }

    private void Start()
    {
        ToolTipObject.SetActive(false);
    }

    public void Show(BaseEventData baseEventData)
    {
        ToolTipObject.SetActive(true);
    }

    public void Hide(BaseEventData baseEventData)
    {
        ToolTipObject.SetActive(false);
    }
}
