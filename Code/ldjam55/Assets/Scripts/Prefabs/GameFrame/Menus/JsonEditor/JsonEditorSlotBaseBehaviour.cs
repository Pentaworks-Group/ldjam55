using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public abstract class JsonEditorSlotBaseBehaviour : MonoBehaviour
    {
        private JsonEditorSlotPrefabBehaviour slotPrefabBehaviour;

        protected JsonEditorBaseBehaviour editorBehaviour;

        private IJsonEditorSlotParent parent;

        private bool validValues = false;

        private bool awakend = false;

        [SerializeField]
        private List<String> UsedForPropertyes;

        public List<String> UsedForPropertyTypes()
        {
            return UsedForPropertyes;
        }

        private void Awake()
        {
            EnsureAwake();
        }


        private void EnsureAwake()
        {
            if (!awakend)
            {
                EnsureSlotPrefabBehaviour();
                Button removeButton = transform.Find("InnerPart/Buttons/RemoveButton").GetComponent<Button>();
                removeButton.onClick.AddListener(RemoveThisSlot);
                awakend = true;
            }
        }

        public void InitSlotBehaviour(JsonEditorBaseBehaviour editorBehaviour, String name, IJsonEditorSlotParent parent, bool displayName = true)
        {
            this.parent = parent;
            this.editorBehaviour = editorBehaviour;
            EnsureAwake();
            slotPrefabBehaviour.InitSlotBehaviour(name, displayName);
        }



        private void EnsureSlotPrefabBehaviour()
        {
            if (slotPrefabBehaviour == null)
            {
                slotPrefabBehaviour = GetComponent<JsonEditorSlotPrefabBehaviour>();
            }
        }

        protected void SetInvalid()
        {
            validValues = false;
            slotPrefabBehaviour.SetInValidColor();
            parent.UpdateByChild(name);
        }

        protected void SetValid()
        {
            validValues = true;
            slotPrefabBehaviour.SetValidColor();
            parent.UpdateByChild(name);
        }

        public bool HasValidValues()
        {
            return validValues;
        }

        public abstract JToken GenerateToken();

        public abstract int Size();

        public abstract void SetValue(JToken value);

        public void RemoveThisSlot()
        {
            parent.RemoveChild(this);
            Destroy(gameObject);
        }

    }
}
