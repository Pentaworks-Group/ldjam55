using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using UnityEngine;

namespace Assets.Scripts
{
    public class JsonEditorSlotListBehaviour : JsonEditorSlotBaseBehaviour, IJsonEditorSlotParent
    {

        [SerializeField]
        private GameObject slotsParent;

        [SerializeField]
        private GameObject newButton;


        private GameObject slotTemplate;


        private List<JsonEditorSlotBaseBehaviour> behaviours = new List<JsonEditorSlotBaseBehaviour>();


        public void InitList(GameObject slotTemplate, JToken value = null)
        {
            this.slotTemplate = slotTemplate;
            if (value != null )
            {
                JArray list = value as JArray;
                foreach (var entry in list)
                {
                    GenerateNewListItemWithValue(entry);
                }
                UpdateValidState();
            }
        }

        public void GenerateNewListItem()
        {
            GenerateNewListItemWithValue();
        }

        public void GenerateNewListItemWithValue(JToken value = null)
        {
            var slot = Instantiate(slotTemplate, slotsParent.transform);
            var templateBehaviour = slot.GetComponent<JsonEditorSlotBaseBehaviour>();
            behaviours.Add(templateBehaviour);
            templateBehaviour.InitSlotBehaviour(editorBehaviour, name, this, false);
            if (value != null )
            {
                templateBehaviour.SetValue(value);
            }
            slot.SetActive(true);
            UpdateSlotsGraphics();
            editorBehaviour.UpdateGraphics();
            UpdateValidState();
        }

        private void UpdateSlotsGraphics()
        {
            float size = 1 / ((float)behaviours.Count + 1); //divide space between button and all slots
            var rect = newButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1 - size);
            rect.anchorMax = new Vector2(1, 1);
            rect = slotsParent.GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(1, 1 - size);
            var cntSlot = 0;
            size = 1 / (float)behaviours.Count; //since we have the slots inside the slots parent we devide it by num slots
            foreach (var slot in behaviours)
            {
                rect = slot.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1 - (cntSlot + 1) * size);
                rect.anchorMax = new Vector2(1, 1 - cntSlot * size);
                cntSlot++;
            }
        }


        public override JToken GenerateToken()
        {
            var jArray = new JArray();
            //jObject.
            foreach (var behaviour in behaviours)
            {
                //jObject[behaviour.name] = behaviour.GenerateToken(); 
                jArray.Add(behaviour.GenerateToken());
            }
            return jArray;
        }

        public override Int32 Size()
        {
            return behaviours.Count + 1;
        }


        private void UpdateValidState()
        {
            foreach (var behaviour in behaviours)
            {
                if (!behaviour.HasValidValues())
                {
                    SetInvalid();
                    return;
                }
            }
            SetValid();
        }

        public void UpdateByChild(String childName)
        {
            UpdateValidState();
        }

        public void RemoveChild(JsonEditorSlotBaseBehaviour child)
        {
            behaviours.Remove(child);
            UpdateSlotsGraphics();
            editorBehaviour.UpdateGraphics();
            UpdateValidState();
        }

        public override void SetValue(JToken value)
        {
            
        }
    }
}
