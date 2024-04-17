using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Assets.Scripts.Scene.GameScene
{
    public class GameFieldTestingClickBehaviour : MonoBehaviour
    {
        [SerializeField]
        private UserActionBehavior actionTemplate;

        private List<UserActionBehavior> actionBehaviors = new List<UserActionBehavior>();

        public List<UserAction> actions => Base.Core.Game.UserActionHandler.UserActions;

        public UserAction SelectedUserAction { get; set; }


        public void SelectUserAction(UserAction action)
        {
            SelectedUserAction = action;
        }

        private void Start()
        {
            float increment = .22f;
            float current = 0;
            foreach (var action in actions) { 
                var actionBehaviour = Instantiate<UserActionBehavior>(actionTemplate, actionTemplate.transform.parent);
                var rect = actionBehaviour.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(rect.anchorMin.x + current, rect.anchorMin.y);
                rect.anchorMax = new Vector2(rect.anchorMax.x + current, rect.anchorMax.y);
                actionBehaviour.Init(action, SelectUserAction, () => SelectedUserAction);   
                actionBehaviour.gameObject.SetActive(true);
                actionBehaviors.Add(actionBehaviour);
                current += increment;
            }
            UpdateUI();
        }


        public void CastSelectedAction(GameFieldContainerBehaviour target)
        {
            if (SelectedUserAction != null)
            {
                //Base.Core.Game.UserActionHandler.UseAction(SelectedUserAction, target);
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            foreach (var action in actionBehaviors)
            {
                action.UpdateUI();
            }
        }


        private Action<GameFieldContainerBehaviour> SelectedAction;

        [SerializeField]
        private CreepBehaviour CreepBehaviour;


        [SerializeField]
        private TMP_Text SelectedActionText;
        [SerializeField]
        private TMP_InputField InputAmount;
        [SerializeField]
        private TMP_InputField InputCreeper;
        [SerializeField]
        private TMP_InputField InputIntervall;



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
        public void SetCreateSpawnerAction()
        {
            float amount = float.Parse(InputAmount.text);
            string creeperId = InputCreeper.text;
            float intervall = float.Parse(InputIntervall.text);
            SelectedAction = (GameFieldContainerBehaviour container) => CreateSpawnerAtField(container, amount, intervall, creeperId);
            SelectedActionText.text = "Spawn";
        }

        private void CreateSpawnerAtField(GameFieldContainerBehaviour container, float amount, float intervall, string creeperId)
        {
            if (container.ObjectType == "Field" || container.ObjectType == "Creep")
            {
                Core.Model.FieldObject spawner = new();
                spawner.Name = "SomeHotSauceStuff";
                spawner.Description = "Blablabla";
                spawner.Material = "Creeper_Yellow_Stage_3";
                spawner.Model = "Spawner_3";
                spawner.Hidden = false;
                var method = new MethodBundle();
                method.Method = "SpawnCreep";
                method.ArumentsJson = $"{{\"Amount\": {amount},\"Time\": {intervall},\"Creeper\": \"{creeperId}\", \"Intervall\": {intervall}}}";
                spawner.Methods = new List<MethodBundle>
                {
                    method
                };
                CreepBehaviour.CreateSpawner((Core.Model.Field)container.ContainedObject, spawner);
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
                                CastSelectedAction(container);
                                //if (SelectedAction != null)
                                //{
                                //    SelectedAction(container);
                                //}
                            }
                            else
                            {
                                if (raycastHit.transform.parent.gameObject.TryGetComponent<GameFieldContainerBehaviour>(out var parentContainer))
                                {
                                    CastSelectedAction(container);
                                    //if (SelectedAction != null)
                                    //{
                                    //    SelectedAction(parentContainer);
                                    //}
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

