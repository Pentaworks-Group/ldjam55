using UnityEngine;

namespace Assets.Scripts
{
    public class JsonEditorItemSelectorBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject SlotTemplate;
        [SerializeField]
        private GameObject PossibleItems;
        [SerializeField]
        private GameObject SlotsParent;

        [SerializeField]
        private JsonEditorBaseBehaviour editorBehaviour;


        private void ClearOptions()
        {
            foreach (Transform child in SlotsParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void UpdateOptions()
        {
            ClearOptions();
            PopulateOptions();
        }

        private void PopulateOptions()
        {
            var cnt = 0;
            foreach (var option in editorBehaviour.GetNotUsedOptions())
            {
                var slot = Instantiate(SlotTemplate, SlotsParent.transform);
                var rect = slot.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0 - cnt);
                rect.anchorMax = new Vector2(1, 1 - cnt);
                var slotBehaviour = slot.GetComponent<JsonEditorItemSelectorSlotBehaviour>();
                slotBehaviour.Init(option, editorBehaviour);
                cnt++;
            }
        }
    }
}
