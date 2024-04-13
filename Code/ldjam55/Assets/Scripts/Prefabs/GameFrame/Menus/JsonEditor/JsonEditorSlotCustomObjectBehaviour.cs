using Newtonsoft.Json.Linq;

using TMPro;

using UnityEngine;

namespace Assets.Scripts
{
    public class JsonEditorSlotCustomObjectBehaviour : JsonEditorSlotDropdownBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI customObjectNameText;

        private JObject createdObject;

        public void OpenNewEditor(JsonEditorManagerBehaviour manager)
        {
            if (createdObject == null)
            {
                manager.OpenEditorByObjectName(this.name, SetCustomObject);
            } else {

                manager.OpenEditorByObjectName(this.name, SetCustomObject, createdObject);
            }
        }

        public void SetCustomObject(JObject createdObject)
        {
            if (createdObject != null)
            {
                this.createdObject = createdObject;
                DisplayCustom();
                SetValid();
            }
        }

        private void DisplayCustom()
        {
            dropDown.gameObject.SetActive(false);
            customObjectNameText.text = createdObject["Reference"].ToString();
            customObjectNameText.gameObject.SetActive(true);
        }

        public override void SetValue(JToken value)
        {
            var reference = value["Reference"].ToString();
            if (IsValidOption(reference))
            {
                base.SetValue(value);
            } else
            {
                SetCustomObject((JObject)value);
            }
        }

        public override JToken GenerateToken()
        {
            if (createdObject != null)
            {
                return createdObject;
            }
            return base.GenerateToken();
        }
    }
}
