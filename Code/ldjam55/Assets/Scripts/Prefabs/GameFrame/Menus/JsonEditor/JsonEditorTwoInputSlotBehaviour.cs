using System;

using Newtonsoft.Json.Linq;

using TMPro;

using UnityEngine;

namespace Assets.Scripts
{
    public class JsonEditorTwoInputSlotBehaviour : JsonEditorSlotBaseBehaviour
    {

        [SerializeField]
        private TMP_InputField input1Field;
        [SerializeField]
        private TMP_InputField input2Field;

        private bool value1Correct = false;
        private bool value2Correct = false;

        private string val1;
        private string val2;


        private void Start()
        {
            input1Field.onEndEdit.AddListener(SubmitValue1);
            input2Field.onEndEdit.AddListener(SubmitValue2);
        }



        public void SubmitValue1(string value)
        {
            if (CheckValue(value))
            {
                value1Correct = true;
                val1 = value;
                if (value2Correct)
                {
                    SetValid();
                }
            }
        }

        public void SubmitValue2(string value)
        {
            if (CheckValue(value))
            {
                value2Correct = true;
                val2 = value;
                if (value1Correct)
                {
                    SetValid();
                }
            }
        }

        public override JToken GenerateToken()
        {

            var value = new GameFrame.Core.Math.Range(Single.Parse(val1), Single.Parse(val2));

            return JObject.FromObject(value);
        }

        private bool CheckValue(string value)
        {
            return true;
        }

        public override Int32 Size()
        {
            return 2;
        }

        public override void SetValue(JToken value)
        {
            val1 = value["Min"].ToString();
            input1Field.text = val1;
            value1Correct = true; 
            val2 = value["Max"].ToString();
            input2Field.text = val2;    
            value2Correct = true;
            SetValid();
        }
    }
}
