using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using TMPro;

using UnityEngine;

namespace Assets.Scripts
{
    public class JsonEditorSlotDropdownBehaviour : JsonEditorSlotBaseBehaviour
    {

        [SerializeField]
        protected TMP_Dropdown dropDown;

        private String selectedOption;
        private List<String> possibleOptions;

        private int lastSelected = 0;

        private bool initalized = false;

        private void Start()
        {
            EnsureInitialization();
        }

        private void EnsureInitialization()
        {
            if (!initalized)
            {
                dropDown.onValueChanged.AddListener(OnEditEnd);
                PopulateDropdown();
                initalized = true;
            }
        }

        public void OnEditEnd(int index)
        {
            if (index == 0)
            {
                dropDown.value = lastSelected;
            } else
            {
                lastSelected = index;
                this.selectedOption = possibleOptions[index];
                SetValid();
            }

        }

        private void PopulateDropdown()
        {
            
            possibleOptions = editorBehaviour.GetDropDownOptions(name);            
            possibleOptions.Insert(0, "Please Select");
            dropDown.ClearOptions();
            dropDown.AddOptions(possibleOptions);
        }

        public override JToken GenerateToken()
        {
            var jsonString = $"{{'IsReferenced': true,'Reference': '{selectedOption}'}}";
            return JObject.Parse(jsonString);
        }

        public override Int32 Size()
        {
            return 1;
        }

        protected bool IsValidOption(string option)
        {
            EnsureInitialization();
            var index = possibleOptions.IndexOf(option);
            return index != -1;
        }

        public override void SetValue(JToken value)
        {
            EnsureInitialization();
            selectedOption = value["Reference"].ToString();
            lastSelected = possibleOptions.IndexOf(selectedOption);
            if (lastSelected == -1)
            {
                Debug.Log("Tried to get invalid dropdown option: " + selectedOption);
            }
            dropDown.value = lastSelected;
            //SetValid();
        }
    }
}
