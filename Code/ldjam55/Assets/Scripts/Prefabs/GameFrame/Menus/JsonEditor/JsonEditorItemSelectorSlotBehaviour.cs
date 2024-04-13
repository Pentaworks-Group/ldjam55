using System;

using TMPro;

using UnityEngine;

namespace Assets.Scripts
{
    public class JsonEditorItemSelectorSlotBehaviour : MonoBehaviour
    {
        private JsonEditorBaseBehaviour editorBehaviour;
        public void Init(String optionName, JsonEditorBaseBehaviour editorBehaviour)
        {
            name = optionName;
            this.editorBehaviour = editorBehaviour;
            var text = transform.Find("Button/Text").GetComponent<TextMeshProUGUI>();
            text.text = optionName;
            this.gameObject.SetActive(true);
        }

        public void OnClick()
        {
            editorBehaviour.SpawnOption(name);
        }
    }
}
