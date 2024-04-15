using Assets.Scripts.Base;
using Assets.Scripts.Constants;
using Assets.Scripts.Core.Model;
using Assets.Scripts.Scenes.GameScene;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreepBehaviour : MonoBehaviour
{
    public List<Action<Border>> DestroyBorderEvent = new List<Action<Border>>();
    public List<Action<Field>> OnCreeperChanged = new List<Action<Field>>();
    public List<Action<FieldObject>> DestroyFieldObjectEvent = new();
    public List<Action<FieldObject>> CreateFieldObjectEvent = new();

    [SerializeField]
    private TimeManagerBehaviour timeManagerBehaviour;


    //private Dictionary<Creeper, List<Border>> bordersByCreep = new Dictionary<Creeper, List<Border>>();
    private List<Border> borders = new List<Border>();
    private bool isRunning = false;

    private Dictionary<string, Creeper> creepers;


    private TriggerHandler triggerHandler = new();
    public GameEndConditionHandler gameEndConditionHandler {  get; private set; }


    private void Start()
    {
        gameEndConditionHandler = GameEndConditionHandler.Instance;
        gameEndConditionHandler.Init(Win, Loose);
    }

    void Update()
    {
        if (isRunning)
        {
            distributeCreep();
        }
    }

    public void StartGame()
    {
        ConvertBorders();
        ConvertFieldObjectMethodsForAllFlieldObjects();
        creepers = Core.Game.State.Mode.Creepers.ToDictionary(creep => creep.ID);
        isRunning = true;
    }

    private void Loose(GameEndCondition condition)
    {
        Debug.Log("You have lost");
        Core.Game.ChangeScene(SceneNames.GameOver);
    }

    private void Win(GameEndCondition condition)
    {
        Debug.Log("You have won");
        Core.Game.ChangeScene(SceneNames.GameOver);
    }

    private void ConvertBorders()
    {

        borders = ConvertBordersForCreeper(null);
        //foreach (var creeper in Core.Game.State.Mode.Creepers)
        //{
        //    ConvertBordersForCreeper(creeper);
        //}
    }
    private List<Border> ConvertBordersForCreeper(Creeper creeper)
    {
        var bordersAdded = new HashSet<string>();
        var borders = new List<Border>();
        var topFields = new Dictionary<string, Field>();
        foreach (var field in Core.Game.State.GameField.Fields)
        {
            string fieldKey = GetFieldKey(field);
            if (topFields.TryGetValue(fieldKey, out Field cField))
            {
                if (cField.Height < field.Height)
                {
                    topFields[fieldKey] = field;
                }
            }
            else
            {
                topFields[fieldKey] = field;
            }
        }

        var allFields = Core.Game.State.GameField.Fields.ToDictionary(field => field.ID);
        foreach (var border in Core.Game.State.GameField.Borders)
        {
            borders.Add(border);
            bordersAdded.Add(GetBorderKey(border));
            if (border.Methods != null)
            {
                foreach (var method in border.Methods)
                {
                    if (method.Method == "DestroyBorder")
                    {
                        JObject paramsObject = JObject.Parse(method.ArumentsJson);
                        float time = float.Parse(paramsObject["Time"].ToString());
                        Action lam = () => DestroyBorder(border);
                        if (method.Trigger == null)
                        {
                            timeManagerBehaviour.RegisterEvent(time, lam, method.Method + GetBorderKey(border), border.GetHashCode());
                        }
                        else if (method.Trigger == "CreepTrigger")
                        {
                            string creeperId = paramsObject["TriggerCreeperId"].ToString();
                            //float amount = paramsObject["triggercreeperId"].ToString();

                            triggerHandler.CreeperTrigger(creeperId, 0, lam, border, border.Field1, border.Field2);
                        }
                    }
                }
            }
        }


        foreach (var field in Core.Game.State.GameField.Fields)
        {
            var neighbours = GetNeighbours(field, topFields);
            foreach (var neighbour in neighbours)
            {
                var borderKey1 = GetBorderByFields(field, neighbour);
                if (bordersAdded.Contains(borderKey1))
                {
                    continue;
                }
                var borderKey2 = GetBorderByFields(neighbour, field);
                if (bordersAdded.Contains(borderKey2))
                {
                    continue;
                }
                var newBorder = CreateNewDefaultBorder(field, neighbour, borders);
                bordersAdded.Add(GetBorderKey(newBorder));
            }
        }
        return borders;
    }

    private Border CreateNewDefaultBorder(Field field1, Field field2, List<Border> borders)
    {
        //var heightDiff = field.Height - neighbour.Height;
        //var flowRate = Core.Game.State.Mode.NothingFlowRate * heightDiff * creeper.Parameters.HightTraverseRate; 
        var flowRate = Core.Game.State.Mode.NothingFlowRate;
        var borderStatus = new BorderStatus() { FlowValue = flowRate };
        var borderType = new BorderType() { Name = "Nothing" };
        var newBorder = new Border { Field1 = field1, Field2 = field2, BorderStatus = borderStatus, BorderType = borderType };
        borders.Add(newBorder);
        return newBorder;
    }

    public void ToggleCreeperActivity()
    {
        isRunning = !isRunning;
    }

    private static string GetBorderKey(Border border)
    {
        return GetBorderByFields(border.Field1, border.Field2);
    }

    private static string GetBorderByFields(Field field1, Field field2)
    {
        return field1.ID + "_" + field2.ID;
    }

    private string GetFieldKey(Field field)
    {
        return GetFieldKeyByCoord(Mathf.RoundToInt(field.Coords.X), Mathf.RoundToInt(field.Coords.Y));
    }

    private string GetFieldKeyByCoord(int x, int y)
    {
        return x + "_" + y;
    }


    private void ConvertFieldObjectMethodsForAllFlieldObjects()
    {
        foreach (var fieldObject in Core.Game.State.GameField.FieldObjects)
        {
            if (fieldObject != null)
            {
                ConvertFieldObjectMethods(fieldObject);
            }
        }
    }

    private void ConvertFieldObjectMethods(FieldObject fieldObject)
    {
        foreach (var method in fieldObject.Methods)
        {
            if (method.Method == "SpawnCreep")
            {
                JObject paramsObject = JObject.Parse(method.ArumentsJson);
                float amount = float.Parse(paramsObject["Amount"].ToString());
                float time = float.Parse(paramsObject["Time"].ToString());
                float interval;
                if (paramsObject.TryGetValue("Intervall", out var intervalS))
                {
                    interval = float.Parse(intervalS.ToString());
                }
                else
                {
                    interval = 0;
                }
                RegisterSpawn(fieldObject, amount, time, interval, paramsObject["Creeper"].ToString(), fieldObject.Name + method.Method, fieldObject.GetHashCode());
            }
            else if (method.Method == "DestoryFieldObject")
            {
                JObject paramsObject = JObject.Parse(method.ArumentsJson);
                float time = float.Parse(paramsObject["Time"].ToString());
                Action lam = () => DestroyFieldObject(fieldObject);
                if (method.Trigger != null && method.Trigger == "CreepTrigger")
                {
                    string creeperId = paramsObject["TriggerCreeperId"].ToString();
                    triggerHandler.CreeperTrigger(creeperId, 0, lam, fieldObject, fieldObject.Field);
                }
                else
                {
                    timeManagerBehaviour.RegisterEvent(time, lam, method.Method + fieldObject.Field.ID + fieldObject.Name, fieldObject.GetHashCode());
                }
            }
            else if (method.Method == "CountDown")
            {
                JObject paramsObject = JObject.Parse(method.ArumentsJson);
                float time = float.Parse(paramsObject["Time"].ToString());
                string conditionName = paramsObject["ConditionName"].ToString();
                Action lam = () => gameEndConditionHandler.IncreaseCount(conditionName);
                if (method.Trigger != null && method.Trigger == "CreepTrigger")
                {
                    string creeperId = paramsObject["TriggerCreeperId"].ToString();
                    triggerHandler.CreeperTrigger(creeperId, 0, lam, fieldObject, fieldObject.Field);
                }
                else
                {
                    timeManagerBehaviour.RegisterEvent(time, lam, method.Method + fieldObject.Field.ID + fieldObject.Name, fieldObject.GetHashCode());
                }
            }
            else if (method.Method == "CreepTouched")
            {
                JObject paramsObject = JObject.Parse(method.ArumentsJson);
                float time = float.Parse(paramsObject["Time"].ToString());
                string conditionName = paramsObject["ConditionName"].ToString();
                Action lam = () => gameEndConditionHandler.IncreaseCount(conditionName);
                if (method.Trigger != null && method.Trigger == "CreepTrigger")
                {
                    string creeperId = paramsObject["TriggerCreeperId"].ToString();
                    triggerHandler.CreeperTrigger(creeperId, 0, lam, fieldObject, fieldObject.Field);
                }
                else
                {
                    timeManagerBehaviour.RegisterEvent(time, lam, method.Method + fieldObject.Field.ID + fieldObject.Name, fieldObject.GetHashCode());
                }
            }
        }
    }

    private void RegisterSpawn(FieldObject fieldObject, float amount, float time, float interval, string creeperId, string ID, int objectID)
    {
        timeManagerBehaviour.RegisterEvent(time, () => SpawnCreepAt(fieldObject.Field, amount, creeperId), ID, objectID, interval);
    }

    public void CreateSpawner(Field field, FieldObject spawner)
    {
        field.FieldObjects.Add(spawner);
        spawner.Field = field;
        Core.Game.State.GameField.FieldObjects.Add(spawner);
        ConvertFieldObjectMethods(spawner);
        foreach (var action in CreateFieldObjectEvent)
        {
            action.Invoke(spawner);
        }
    }

    public void SpawnCreepAt(Field field, float amount, string creeperId)
    {
        if (field.Creep != null && field.Creep.Creeper != null)
        {
            if (field.Creep.Creeper.ID != creeperId)
            {
                var div = field.Creep.Value - amount;
                if (div > 0)
                {
                    field.Creep.Value = div;
                }
                else
                {
                    field.Creep.Value -= div;
                    field.Creep.Creeper = creepers[creeperId];
                    CreeperChanged(field);
                }
            }
            else
            {
                field.Creep.Value += amount;
            }
        }
        else
        {
            var newCreep = new Creep() { Value = amount, Creeper = creepers[creeperId] };
            field.Creep = newCreep;
            CreeperChanged(field);
        }
    }

    public void DestroyBorder(Border border)
    {
        if (border != null)
        {
            borders.Remove(border);
            Core.Game.State.GameField.Borders.Remove(border);

            CreateNewDefaultBorder(border.Field1, border.Field2, borders);
            foreach (var action in DestroyBorderEvent)
            {
                action.Invoke(border);
            }
            triggerHandler.UnRegisterByObjectID(border);
            timeManagerBehaviour.UnregisterByObjectID(border.GetHashCode());
        }
    }

    public void DestroyFieldObject(FieldObject fieldO)
    {
        if (fieldO != null)
        {
            Core.Game.State.GameField.FieldObjects.Remove(fieldO);

            foreach (var action in DestroyFieldObjectEvent)
            {
                action.Invoke(fieldO);
            }
            triggerHandler.UnRegisterByObjectID(fieldO);
            timeManagerBehaviour.UnregisterByObjectID(fieldO.GetHashCode());
        }
    }


    private void CreeperChanged(Field field)
    {
        triggerHandler.InvokeTriggered(field);
        foreach (var action in OnCreeperChanged)
        {
            action(field);
        }
    }

    private List<Field> GetNeighbours(Field field, Dictionary<string, Field> topFields)
    {
        var neighbours = new List<Field>();
        var x = Mathf.RoundToInt(field.Coords.X);
        var y = Mathf.RoundToInt(field.Coords.Y);
        Field topField;
        if (topFields.TryGetValue(GetFieldKeyByCoord(x - 1, y), out topField))
        {
            neighbours.Add(topField);
        }
        if (topFields.TryGetValue(GetFieldKeyByCoord(x + 1, y), out topField))
        {
            neighbours.Add(topField);
        }
        if (topFields.TryGetValue(GetFieldKeyByCoord(x, y - 1), out topField))
        {
            neighbours.Add(topField);
        }
        if (topFields.TryGetValue(GetFieldKeyByCoord(x, y + 1), out topField))
        {
            neighbours.Add(topField);
        }
        return neighbours;
    }




    private void distributeCreep()
    {
        foreach (var field in Core.Game.State.GameField.Fields)
        {
            if (field.Creep != null)
            {
                field.Creep.ValueOld = field.Creep.Value;
            }
        }


        for (int i = borders.Count - 1; i >= 0; i--)
        {
            var border = borders[i];
            float flow = getFlow(border.Field1, border.Field2, border.BorderStatus.FlowValue);
            if (border.Field1.Creep == null)
            {
                border.Field1.Creep = new Creep { Value = 0 };
            }
            if (border.Field2.Creep == null)
            {
                border.Field2.Creep = new Creep { Value = 0 };
            }
            copyCreeper(flow, border.Field1, border.Field2);
            border.Field1.Creep.Value -= flow;
            border.Field2.Creep.Value += flow;

            //Update Border state
        }
    }

    private void copyCreeper(float flow, Field field1, Field field2)
    {
        //TODO: what happens if both fields have a creeper -> Game Over/Win?
        if (flow > 0)
        {
            if (field2.Creep.Creeper != field1.Creep.Creeper)
            {
                field2.Creep.Creeper = field1.Creep.Creeper;
                CreeperChanged(field2);
            }
        }
        else if (flow < 0)
        {
            if (field1.Creep.Creeper != field2.Creep.Creeper)
            {
                field1.Creep.Creeper = field2.Creep.Creeper;
                CreeperChanged(field1);
            }
        }
    }

    private float getFlow(Field field1, Field field2, float borderState)
    {
        float valueField1 = 0;
        if (field1.Creep != null)
        {
            valueField1 = field1.Creep.ValueOld;
        }
        float valueField2 = 0;
        if (field2.Creep != null)
        {
            valueField2 = field2.Creep.ValueOld;
        }
        float flow = (valueField1 - valueField2) * borderState * Time.deltaTime * Core.Game.State.Mode.FlowSpeed;

        var heightDiff = field1.Height - field2.Height;
        if (heightDiff > 0)
        {
            if (field1.Creep != null)
            {
                var flow1 = heightDiff * field1.Creep.Creeper.Parameters.HightTraverseRate;
                var flow2 = heightDiff * field2.Creep.Creeper.Parameters.HightTraverseRate;
                flow = flow1 - flow2;
            }
            else if (field1.Creep != null)
            {
                flow = heightDiff * field1.Creep.Creeper.Parameters.HightTraverseRate;
            }
            else if (field2.Creep != null)
            {
                flow = heightDiff * field2.Creep.Creeper.Parameters.HightTraverseRate;
            }
        }
        if (flow > -Core.Game.State.Mode.MinFlow && flow < Core.Game.State.Mode.MinFlow)
        {
            flow = 0;
        }
        return flow;
    }

}
