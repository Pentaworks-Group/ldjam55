using System;

using Newtonsoft.Json.Linq;

using TMPro;

using UnityEngine;

namespace Assets.Scripts
{
    public class JsonEditorSlotSingleInputBehaviour : JsonEditorSlotBaseBehaviour
    {

        [SerializeField]
        private TMP_InputField inputField;

        private string value;

        private void Start()
        {
            inputField.onEndEdit.AddListener(OnEditEnd);
        }

        public void OnEditEnd(string value)
        {
            this.value = value;
            SetValid();
        }

        public override JToken GenerateToken()
        {
            return new JValue(value);
        }

        public override Int32 Size()
        {
            return 1;
        }
        public override void SetValue(JToken value)
        {
            this.value = value.ToString();
            inputField.text = this.value;
            SetValid();
        }
    }
}
