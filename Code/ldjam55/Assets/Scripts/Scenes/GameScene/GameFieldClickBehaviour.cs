using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Assets.Scripts.Scene.GameScene
{
    public class GameFieldClickBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Terrain mainTerrain;

        [SerializeField]
        private TerrainBehaviour terrainBehaviour;

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
            foreach (var action in actions)
            {
                var actionBehaviour = Instantiate<UserActionBehavior>(actionTemplate, actionTemplate.transform.parent);
                var rect = actionBehaviour.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(rect.anchorMin.x + current, rect.anchorMin.y);
                rect.anchorMax = new Vector2(rect.anchorMax.x + current, rect.anchorMax.y);
                actionBehaviour.Init(action, SelectUserAction);
                actionBehaviour.gameObject.SetActive(true);
                actionBehaviors.Add(actionBehaviour);
                current += increment;
            }
            UpdateUI();
        }


        public void CastSelectedAction(object target)
        {
            if (SelectedUserAction != null)
            {
                Base.Core.Game.UserActionHandler.UseAction(SelectedUserAction, target);
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
                        GameObject targetGO = raycastHit.transform.gameObject;
                        if (targetGO != null)
                        {
                            if (raycastHit.transform.gameObject.TryGetComponent<GameFieldContainerBehaviour>(out var container))
                            {
                                CastSelectedAction(container.ContainedObject);
                            }
                            else if (raycastHit.transform.parent && raycastHit.transform.parent.gameObject.TryGetComponent<GameFieldContainerBehaviour>(out var parentContainer))
                            {
                                CastSelectedAction(container.ContainedObject);
                            }
                            else if (raycastHit.transform.gameObject.Equals(mainTerrain.gameObject))
                            {
                                Vector2Int mapPoint = terrainBehaviour.TransformTerrainCoordToMap(new Vector2Int((int)raycastHit.point.x, (int)raycastHit.point.z));
                                var field = terrainBehaviour.getField(mapPoint.x, mapPoint.y);
                                CastSelectedAction(field);
                                Debug.Log("Hit Terrain: " + raycastHit.ToString() + ", " + mapPoint.ToString());
                            }
                            else
                            {
                                Debug.Log("UnkownObject: " + raycastHit.transform.gameObject);
                            }
                        }
                    }
                }
            }
            //}
        }
    }
}

