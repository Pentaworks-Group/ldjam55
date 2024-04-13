using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class JsonEditorSlotPrefabBehaviour : MonoBehaviour
    {
        private Image BackGround;


        [SerializeField]
        private TextMeshProUGUI nameText;


        [SerializeField]
        private Material ValidBackGroundColor;
        [SerializeField]
        private Material InvalidBackGroundColor;

        private bool awakend = false;


        private void Awake()
        {
            EnsureAwake();
        }



        private void EnsureAwake()
        {
            if (!awakend)
            {
                //InitColors();
                BackGround = GetComponent<Image>();
                awakend = true;
            }
        }


        public void InitSlotBehaviour(string name, bool displayName = true)
        {
            EnsureAwake();
            this.name = name;
            nameText.text = name;
            this.gameObject.name = name;
            if (!displayName)
            {
                var nameRect = transform.Find("InnerPart/Name");
                nameRect.gameObject.SetActive(false);

                var valuesRect = transform.Find("InnerPart/Values").GetComponent<RectTransform>();
                valuesRect.anchorMin = new Vector2(0, 0);
                valuesRect.anchorMax = new Vector2(1, 1);
            }
        }

        public void SetValidColor()
        {
            BackGround.material = ValidBackGroundColor;
        }

        public void SetInValidColor()
        {
            BackGround.material = InvalidBackGroundColor;
        }
    }
}
