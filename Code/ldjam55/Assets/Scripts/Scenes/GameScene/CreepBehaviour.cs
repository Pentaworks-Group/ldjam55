using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using static CreepBehaviour;

public class CreepBehaviour : MonoBehaviour
{

    [SerializeField]
    private TimeManagerBehaviour TimeManagerBehaviour;


    //private Dictionary<Creeper, List<Border>> bordersByCreep = new Dictionary<Creeper, List<Border>>();
    private List<Border> borders = new List<Border>();
    private bool isRunning = false;

    private Dictionary<string, Creeper> creepers;

    public List<Action<Border>> DestroyBorderEvent = new List<Action<Border>>();
    public Dictionary<string, List<ActionContainer>> FieldCreeperChangeEvent = new Dictionary<string, List<ActionContainer>>();

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
        ConvertFieldObjectMethods();
        creepers = Core.Game.State.Mode.Creepers.ToDictionary(creep => creep.ID);
        isRunning = true;
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
                            TimeManagerBehaviour.RegisterEvent(time, lam, method.Method + GetBorderKey(border));
                        }
                        else if (method.Trigger == "CreepTrigger")
                        {
                            string creeperId = paramsObject["TriggerCreeperId"].ToString();
                            //float amount = paramsObject["triggercreeperId"].ToString();

                            CreeperTrigger(creeperId, 0, lam, border.Field1, border.Field2);
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


    private void ConvertFieldObjectMethods()
    {
        foreach (var fieldObject in Core.Game.State.GameField.FieldObjects)
        {
            if (fieldObject != null)
            {
                foreach (var method in fieldObject.Methods)
                {
                    if (method.Method == "SpawnCreep")
                    {
                        JObject paramsObject = JObject.Parse(method.ArumentsJson);
                        float amount = float.Parse(paramsObject["Amount"].ToString());
                        float time = float.Parse(paramsObject["Time"].ToString());
                        bool isIntervall = bool.Parse(paramsObject["IsIntervall"].ToString());
                        RegisterSpawn(fieldObject, amount, time, isIntervall, paramsObject["Creeper"].ToString(), fieldObject.Name + method.Method);
                    }
                }
            }
        }
    }

    private void RegisterSpawn(FieldObject fieldObject, float amount, float time, bool isIntervall, string creeperId, string ID)
    {
        TimeManagerBehaviour.RegisterEvent(time, () => Spawn(fieldObject, amount, time, isIntervall, creeperId, ID), ID);
    }

    private void Spawn(FieldObject fieldObject, float amount, float time, bool isIntervall, string creeperId, string ID)
    {
        SpawnCreepAt(fieldObject.Field, amount, creeperId);
        if (isIntervall)
        {
            TimeManagerBehaviour.RegisterEvent(time, () => Spawn(fieldObject, amount, time, isIntervall, creeperId, ID), ID);
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
        }
    }


    public class ActionContainer
    {
        public Field[] fields; public string creeperId; public float amount; public Action triggeredAction;
    }

    private void CreeperTrigger(string creeperId, float amount, Action triggeredAction, params Field[] fields)
    {
        var actionContainer = new ActionContainer() { triggeredAction = triggeredAction, amount = amount, creeperId = creeperId, fields = fields };
        foreach (var field in fields)
        {
            if (!FieldCreeperChangeEvent.TryGetValue(field.ID, out var actions))
            {
                actions = new List<ActionContainer>();
                FieldCreeperChangeEvent.Add(field.ID, actions);
            }
            actions.Add(actionContainer);
        }
    }

    private void InvokeTriggered(Field field)
    {
        if (FieldCreeperChangeEvent.TryGetValue(field.ID, out List<ActionContainer> actionContainers))
        {
            for (int i = actionContainers.Count - 1; i >= 0; i--)
            {
                var container = actionContainers[i];
                if (container.creeperId != null && field.Creep.Creeper.ID == container.creeperId)
                {
                    container.triggeredAction.Invoke();
                    DeregisterTrigger(container);
                }
            }
        }
    }



    private void DeregisterTrigger(ActionContainer container)
    {
        foreach (var field in container.fields)
        {
            if (FieldCreeperChangeEvent.TryGetValue(field.ID, out var actions))
            {
                actions.Remove(container);
                if (actions.Count == 0)
                {
                    FieldCreeperChangeEvent.Remove(field.ID);
                }
            }

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
                InvokeTriggered(field2);
            }
        }
        else if (flow < 0)
        {
            if (field1.Creep.Creeper != field2.Creep.Creeper)
            {
                field1.Creep.Creeper = field2.Creep.Creeper;
                InvokeTriggered(field1);
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
