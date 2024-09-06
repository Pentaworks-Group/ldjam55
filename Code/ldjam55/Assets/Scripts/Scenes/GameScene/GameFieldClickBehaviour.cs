
using Assets.Scripts.Core.Model;
using System.Collections.Generic;
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
        [SerializeField]
        private float increment;

        private List<UserActionBehavior> actionBehaviors = new List<UserActionBehavior>();

        public List<UserAction> actions => Base.Core.Game.UserActionHandler.UserActions;

        public UserAction SelectedUserAction { get; set; }


        public void SelectUserAction(UserAction action)
        {
            SelectedUserAction = action;
            UpdateUI();
        }

        private void Start()
        {
            float current = 0;
            var s = Base.Core.Game.State.CurrentLevel.UserActions;
            foreach (var action in actions)
            {
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


        public void CastSelectedAction(UserActionInput target)
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

        private Border getRaycastBorder(RaycastHit hit)
        {
            Vector2Int mapPoint, neighborMapPoint;
            GetMapPoints(hit, out mapPoint, out neighborMapPoint);

            var field = terrainBehaviour.GetField(mapPoint.x, mapPoint.y);
            var neighbor = terrainBehaviour.GetField(neighborMapPoint.x, neighborMapPoint.y);

            Border border = new Border { Field1 = field, Field2 = neighbor };
            return border;
        }

        private void GetMapPoints(RaycastHit hit, out Vector2Int mapPoint, out Vector2Int neighborMapPoint)
        {
            Vector2Int terrainHitPoint = new Vector2Int((int)hit.point.x, (int)hit.point.z);
            mapPoint = terrainBehaviour.TransformTerrainCoordToMap(terrainHitPoint);
            Vector2Int fieldCenter = terrainBehaviour.TransformMapCoordToTerrainCoord(mapPoint);
            Vector2Int distance = terrainHitPoint - fieldCenter;

            float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
            //Default: Left
            neighborMapPoint = new Vector2Int(mapPoint.x - 1, mapPoint.y);
            if (angle > -45 && angle < 45)
            {
                // Right
                neighborMapPoint = new Vector2Int(mapPoint.x + 1, mapPoint.y);
            }
            else if (angle <= -45 && angle > -135)
            {
                //Bottom
                neighborMapPoint = new Vector2Int(mapPoint.x, mapPoint.y - 1);
            }
            else if (angle >= 45 && angle < 135)
            {
                //Top
                neighborMapPoint = new Vector2Int(mapPoint.x, mapPoint.y + 1);
            }
        }

        private Vector2 GetHitCoords(RaycastHit hit)
        {
            Vector2Int terrainHitPoint = new Vector2Int((int)hit.point.x, (int)hit.point.z);
            Vector2 mapPoint = terrainBehaviour.TransformTerrainCoordToMapFloat(terrainHitPoint);
            return mapPoint;
        }


        private void LateUpdate()
        {
            if (Input.GetMouseButtonUp(0)) 
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
                                var actionInput = new UserActionInput()
                                {
                                    Input = container.ContainedObject,
                                    InputType = container.ObjectType
                                };
                                CastSelectedAction(actionInput);
                            }
                            else if (raycastHit.transform.parent && raycastHit.transform.parent.gameObject.TryGetComponent<GameFieldContainerBehaviour>(out var parentContainer))
                            {
                                var actionInput = new UserActionInput()
                                {
                                    Input = parentContainer.ContainedObject,
                                    InputType = parentContainer.ObjectType
                                };
                                CastSelectedAction(actionInput);
                            }
                            else if (raycastHit.transform.gameObject.Equals(mainTerrain.gameObject))
                            {
//                                Vector2Int mapPoint = terrainBehaviour.TransformTerrainCoordToMap(new Vector2Int((int)raycastHit.point.x, (int)raycastHit.point.z));
//                                var field = terrainBehaviour.getField(mapPoint.x, mapPoint.y);
                                //var border = getRaycastBorder(raycastHit);
                                //var actionInput = new UserActionInput()
                                //{
                                //    Input = border,
                                //    InputType = "Border"
                                //};
                                Vector2 coords = GetHitCoords(raycastHit);
                                var actionInput = new UserActionInput()
                                {
                                    Input = coords,
                                    InputType = "Coords"
                                };

                                CastSelectedAction(actionInput);
                            }
                            else
                            {
                                Vector2 coords = GetHitCoords(raycastHit);
                                var actionInput = new UserActionInput()
                                {
                                    Input = coords,
                                    InputType = "Coords"
                                };
                                CastSelectedAction(actionInput);
                            }
                        }
                    }
                }
            }
            //}
        }
    }
}

