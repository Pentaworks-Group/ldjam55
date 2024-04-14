using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Assets.Scripts.Scene.GameScene
{
    public class GameFieldTestingClickBehaviour : MonoBehaviour
    {
        private Action<GameFieldContainerBehaviour> SelectedAction;

        [SerializeField]
        private CreepBehaviour CreepBehaviour;


        [SerializeField]
        private TMP_Text SelectedActionText;
        [SerializeField]
        private TMP_InputField InputAmount;
        [SerializeField]
        private TMP_InputField InputCreeper;



        public void SetDestroyBorderAction()
        {
            SelectedAction = DestroyBorderAction;
            SelectedActionText.text = "DestroyBorder";
        }

        private void DestroyBorderAction(GameFieldContainerBehaviour container)
        {
            if (container.ObjectType == "Wall")
            {
                CreepBehaviour.DestroyBorder((Core.Model.Border)container.ContainedObject);
            }
        }

        public void SetSpawnAction()
        {
            float amount = float.Parse(InputAmount.text);
            string creeperId = InputCreeper.text;
            SelectedAction = (GameFieldContainerBehaviour container) => SpawnAtField(container, amount, creeperId);
            SelectedActionText.text = "Spawn";
        }

        private void SpawnAtField(GameFieldContainerBehaviour container, float amount, string creeperId)
        {
            if (container.ObjectType == "Field" || container.ObjectType == "Creep")
            {
                CreepBehaviour.SpawnCreepAt((Core.Model.Field)container.ContainedObject, amount, creeperId);
            }
        }

        private void LateUpdate()
        {
            //if (!CameraBehaviour.IsPanning())
            //if (Input.touchCount < 1 || panTimeout < 1)
            //{
            if (Input.GetMouseButtonUp(0)) //!Base.Core.Game.LockCameraMovement && 
            {
                if (!EventSystem.current.IsPointerOverGameObject())    // is the touch on the GUI
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out var raycastHit, 100.0f))
                    {
                        if (raycastHit.transform.gameObject != null)
                        {
                            if (raycastHit.transform.gameObject.TryGetComponent<GameFieldContainerBehaviour>(out var container))
                            {
                                if (SelectedAction != null)
                                {
                                    SelectedAction(container);
                                }
                            }
                            else
                            {
                                if (raycastHit.transform.parent.gameObject.TryGetComponent<GameFieldContainerBehaviour>(out var parentContainer))
                                {
                                    if (SelectedAction != null)
                                    {
                                        SelectedAction(parentContainer);
                                    }
                                }
                                else
                                {
                                    Debug.Log("UnkownObject: " + raycastHit.transform.gameObject);
                                }
                            }
                        }
                    }
                }
            }
            //}
        }
    }
}

