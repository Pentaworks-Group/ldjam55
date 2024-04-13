using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Scenes.Space.InputHandling
{
    public class ContinousButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private float interval = 0.1f;
        [SerializeField] private float startTime = 0f;
        [SerializeField] private UnityEvent repeatingMethod;
        [SerializeField] private UnityEvent clickMethod;


        public void OnPointerDown(PointerEventData eventdata)
        {
            if (repeatingMethod != default)
            {
                InvokeRepeating("RepeatingCall", startTime, interval);
            }
            if (clickMethod != default)
            {
                clickMethod.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventdata)
        {
            CancelInvoke("RepeatingCall");
        }

        void RepeatingCall()
        {
            repeatingMethod.Invoke();
        }
    }
}
