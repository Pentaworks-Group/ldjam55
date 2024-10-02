using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using Assets.Scripts.Scene.GameScene;
using Assets.Scripts.Scenes.GameScene;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Events;

public class CreepBehaviour : MonoBehaviour
{
    public List<Action<FieldObject>> OnFieldObjectCreatedEvent = new();
    public List<Action<FieldObject>> OnFieldObjectDestroyedEvent = new();
    public List<Action<Field>> OnFieldCreatedEvent = new();
    public List<Action<Field>> OnFieldDestroyedEvent = new();
    public List<Action<Border>> OnBorderCreatedEvent = new();
    public List<Action<Border>> OnBorderDestroyedEvent = new();
    public List<Action<Creeper>> OnCreeperEliminated = new();
    public List<Action<Field, Creeper>> OnCreeperChanged = new();

    [SerializeField]
    private TimeManagerBehaviour timeManagerBehaviour;


    //private Dictionary<Creeper, List<Border>> bordersByCreep = new Dictionary<Creeper, List<Border>>();
    private List<Border> borders = new List<Border>();

    private Dictionary<string, Creeper> creepers;
    private Dictionary<Creeper, HashSet<Creep>> creepsByCreeper;

    private float CreepUpdateInterval = 0.25f;
    private float LastCreepUpdate = 0f;

    private TriggerHandler triggerHandler = new();
    private GameEndConditionHandler _gameEndConditionHandler;
    public GameEndConditionHandler gameEndConditionHandler
    {
        get
        {
            if (_gameEndConditionHandler == null)
            {
                _gameEndConditionHandler = new GameEndConditionHandler();
            }
            return _gameEndConditionHandler;
        }
    }

    private void Awake()
    {
        Core.Game.UserActionHandler.Init(this);
    }

    private void Start()
    {
        //gameEndConditionHandler = GameEndConditionHandler.Instance;
        //gameEndConditionHandler.Init();
        //gameEndConditionHandler.RegisterListener(Stop);
    }

    void Update()
    {
        if (Core.Game.IsRunning)
        {
            //distributeCreep();
            if (LastCreepUpdate <= 0f)
            {
                UpdateCreep();
                LastCreepUpdate = CreepUpdateInterval;
            } else
            {
                LastCreepUpdate -= Time.deltaTime;
            }
        }
    }



    public void StartGame()
    {
        foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
        {
            if (field.Creep != null)
            {
                Debug.Log("Field with Creep: " + field.ID);
            }
        }
        timeManagerBehaviour.Reset();
        triggerHandler.Reset();
        CheckUniqueFields();
        ConvertBorders();
        ConvertFieldObjectMethodsForAllFlieldObjects();
        creepers = Core.Game.State.Mode.Creepers.ToDictionary(creep => creep.ID);
        GatherCreep();
    }


    private void CheckUniqueFields()
    {
        var fieldDict = new Dictionary<string, Field>();
        foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
        {
            fieldDict.Add(field.ID, field);
        }
    }

    private void GatherCreep()
    {
        creepsByCreeper = new Dictionary<Creeper, HashSet<Creep>>();
        foreach (var creeper in Core.Game.State.Mode.Creepers)
        {
            creepsByCreeper.Add(creeper, new HashSet<Creep>());
        }
        foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
        {
            Creep creep = field.Creep;
            if (creep != null)
            {
                if (creep.Creeper == null)
                {
                    Debug.Log("Creep without Creeper: " + field.ID);
                    continue;
                }
                creepsByCreeper[creep.Creeper].Add(creep);
            }
        }
        OnCreeperChanged.Add(UpdateCreepsByCreeper);
    }

    private void UpdateCreepsByCreeper(Field field, Creeper oldCreeper)
    {
        var creep = field.Creep;
        if (field.Creep.Creeper == null)
        {
            Debug.Log("Missing Creeper " + field.ID);
            field.Creep = null;
            return;
        }
        HashSet<Creep> creeps = creepsByCreeper[field.Creep.Creeper];
        creeps.Add(creep);
        //Debug.Log("CreepCount "+ creep.Creeper.ID + ": " + creeps.Count);
        var creepCount = creeps.Count;
        if (oldCreeper != null)
        {
            var oldCreeps = creepsByCreeper[oldCreeper];
            //Debug.Log("CreepCount " + oldCreeper.ID + ": " + oldCreeps.Count);
            creepCount += oldCreeps.Count;
            if (!oldCreeps.Remove(creep))
            {
                Debug.Log("Creep to remove not in list");
            }
            if (oldCreeps.Count <= 0)
            {
                foreach (var action in OnCreeperEliminated)
                {
                    action.Invoke(oldCreeper);
                }
            }
        }
        var cnt = 0;
        foreach (var fieldi in Core.Game.State.CurrentLevel.GameField.Fields)
        {
            if (fieldi.Creep != null && fieldi.Creep.Creeper != null)
            {
                cnt++;
            }
        }
        //Debug.Log("CreepCount Total by field: " + cnt);
        //Debug.Log("CreepCount Total: " + creepCount);
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
        Dictionary<string, Field> topFields = GetTopFields();

        var allFields = Core.Game.State.CurrentLevel.GameField.Fields.ToDictionary(field => field.ID);
        foreach (var border in Core.Game.State.CurrentLevel.GameField.Borders)
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
                        if (method.Trigger == null)
                        {
                            Action lam = () => DestroyBorder(border);
                            timeManagerBehaviour.RegisterEvent(time, lam, method.Method + GetBorderKey(border), border.GetHashCode());
                        }
                        else if (method.Trigger == "CreepTrigger")
                        {
                            string creeperId = paramsObject["TriggerCreeper"].ToString();
                            //float amount = paramsObject["TriggerCreeper"].ToString();

                            Action<Creeper> lam = (Creeper oldCreeper) => DestroyBorder(border);
                            triggerHandler.CreeperTrigger(creeperId, 0, lam, border, border.Field1, border.Field2);
                        }
                    }
                }
            }
        }


        foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
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

    private Dictionary<string, Field> GetTopFields()
    {
        var topFields = new Dictionary<string, Field>();
        foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
        {
            string fieldKey = GetFieldKey(field);
            if (topFields.TryGetValue(fieldKey, out Field cField))
            {
                if (cField.Height < field.Height)
                {
                    topFields[fieldKey] = field;
                }
                Debug.Log("Multiple Fields at coord: " + fieldKey);
            }
            else
            {
                topFields[fieldKey] = field;
            }
        }

        return topFields;
    }




    private bool TryGetSameFields(Field field, out Field existingField)
    {
        foreach (var eField in Core.Game.State.CurrentLevel.GameField.Fields)
        {
            if (eField == field || eField.ID == field.ID)
            {
                existingField = eField;
                return true;
            }
            if (eField.Coords.X == field.Coords.X && eField.Coords.Y == field.Coords.Y)
            {
                existingField = eField;
                return true;
            }
        }
        existingField = null;
        return false;
    }

    public bool GetFieldByCoords(float rawX, float rawY, out Field field)
    {
        int x = (int)rawX;
        int y = (int)rawY;
        var topFields = GetTopFields();
        string fieldCoordKey = GetFieldKeyByCoord(x, y);
        return topFields.TryGetValue(fieldCoordKey, out field);
    }

    public bool CreateField(float rawX, float rawY)
    {
        if (GetFieldByCoords(rawX, rawY, out var field))
        {
            field.Height++;
        }
        else
        {
            field = new Field()
            {
                Coords = new GameFrame.Core.Math.Vector2((int)rawX, (int)rawY),
                Height = 0,
            };
            field.ID = field.GenerateFieldID();
            Core.Game.State.CurrentLevel.GameField.Fields.Add(field);
        }

        foreach (var listener in OnFieldCreatedEvent)
        {
            listener.Invoke(field);
        }
        return true;
    }


    private void DestroyField(Field field)
    {

        foreach (var border in field.Borders)
        {
            Core.Game.State.CurrentLevel.GameField.Borders.Remove(border);
            borders.Remove(border);
        }


        Core.Game.State.CurrentLevel.GameField.Fields.Remove(field);
        foreach (var listener in OnFieldDestroyedEvent)
        {
            listener.Invoke(field);
        }
    }

    public bool SpawnBorderAt(float rawX, float rawY, Border border)
    {
        if (!GetFieldByCoords(rawX, rawY, out Field field1))
        {
            return false;
        }

        if (!GetNearestNeighbourCoords(rawX, rawY, out int x, out int y))
        {
            return false;
        }


        if (!GetFieldByCoords(x, y, out Field field2))
        {
            return false;
        }
        border.Field1 = field1;
        border.Field2 = field2;

        return SpawnBorder(border);
    }

    private bool GetNearestNeighbourCoords(float rawX, float rawY, out int x, out int y)
    {
        x = (int)rawX;
        y = (int)rawY;


        float xOffset = rawX - x;
        float yOffset = rawY - y;
        float invYOffset = 1 - yOffset;

        if (Mathf.Abs(xOffset - yOffset) < 0.05 || Mathf.Abs(xOffset - invYOffset) < 0.05) //define
        {
            return false;
        }

        if (xOffset > yOffset)
        {
            if (xOffset > invYOffset)
            {
                x++;
            }
            else
            {
                y--;
            }
        }
        else
        {
            if (xOffset > invYOffset)
            {
                y++;
            }
            else
            {
                x--;
            }
        }
        return true;
    }

    public bool SpawnBorder(Border border)
    {
        if (TryGetBorderWithSameFields(border, out var existingBorder))
        {
            if (existingBorder.BorderType.Name != "Nothing")
            {
                return false;
            }
            DestroyBorder(existingBorder, false);
        }
        Debug.Log("CreatingBorder: " + border.Field1.ID + " <=> " + border.Field2.ID);
        Debug.Log("CreatingBorderHash: " + border.Field1.GetHashCode() + " <=> " + border.Field2.GetHashCode());
        borders.Add(border);
        Core.Game.State.CurrentLevel.GameField.Borders.Add(border);
        foreach (var listener in OnBorderCreatedEvent)
        {
            listener.Invoke(border);
        }
        return true;
    }

    private bool TryGetBorderWithSameFields(Border border, out Border similarBorder)
    {
        foreach (var eBorder in borders)
        {
            if (eBorder.Field1 == border.Field1 && eBorder.Field2 == border.Field2)
            {
                similarBorder = eBorder;
                return true;
            }
            if (eBorder.Field1 == border.Field2 && eBorder.Field2 == border.Field1)
            {
                similarBorder = eBorder;
                return true;
            }
        }
        similarBorder = null;
        return false;
    }


    private Border CreateNewDefaultBorder(Field field1, Field field2, List<Border> borders)
    {
        //var heightDiff = field.Height - neighbour.Height;
        //var flowRate = Core.Game.State.Mode.NothingFlowRate * heightDiff * creeper.Parameters.HightTraverseRate; 
        var flowRate = Core.Game.State.Mode.NothingFlowRate;
        var borderStatus = new BorderStatus() { FlowValue = flowRate };
        var borderType = new BorderType() { Name = "Nothing", Model = "Wall" };
        var newBorder = new Border { Field1 = field1, Field2 = field2, BorderStatus = borderStatus, BorderType = borderType };
        borders.Add(newBorder);
        return newBorder;
    }

    public void ToggleCreeperActivity()
    {
        Core.Game.IsRunning = !Core.Game.IsRunning;
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
        return x + "," + y;
    }

    private void ConvertFieldObjectMethodsForAllFlieldObjects()
    {
        foreach (var fieldObject in Core.Game.State.CurrentLevel.GameField.FieldObjects)
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
                if (method.Trigger != null && method.Trigger == "CreepTrigger")
                {
                    string creeperId = paramsObject["TriggerCreeper"].ToString();
                    Action<Creeper> lam = (tt) => DestroyFieldObject(fieldObject);
                    triggerHandler.CreeperTrigger(creeperId, 0, lam, fieldObject, fieldObject.Field);
                }
                else
                {
                    Action lam = () => DestroyFieldObject(fieldObject);
                    timeManagerBehaviour.RegisterEvent(time, lam, method.Method + fieldObject.Field.ID + fieldObject.Name, fieldObject.GetHashCode());
                }
            }
            else if (method.Method == "CountDown")
            {
                JObject paramsObject = JObject.Parse(method.ArumentsJson);
                float time = float.Parse(paramsObject["Time"].ToString());
                string conditionName = paramsObject["ConditionName"].ToString();
                if (method.Trigger != null && method.Trigger == "CreepTrigger")
                {
                    string creeperId = paramsObject["TriggerCreeper"].ToString();
                    Action<Creeper> lam = (tt) => gameEndConditionHandler.IncreaseCount(conditionName);
                    triggerHandler.CreeperTrigger(creeperId, 0, lam, fieldObject, fieldObject.Field);
                }
                else if (method.Trigger != null && method.Trigger == "CreeperEliminated")
                {
                    Action<Creeper> lam = (tt) => gameEndConditionHandler.IncreaseCount(conditionName);
                    OnCreeperEliminated.Add(lam);

                }
                else
                {
                    Action lam = () => gameEndConditionHandler.IncreaseCount(conditionName);
                    timeManagerBehaviour.RegisterEvent(time, lam, method.Method + fieldObject.Field.ID + fieldObject.Name, fieldObject.GetHashCode());
                }
            }
            else if (method.Method == "CreepTouched")
            {
                JObject paramsObject = JObject.Parse(method.ArumentsJson);
                float time = float.Parse(paramsObject["Time"].ToString());
                string conditionName = paramsObject["ConditionName"].ToString();
                if (method.Trigger != null && method.Trigger == "CreepTrigger")
                {
                    string creeperId = paramsObject["TriggerCreeper"].ToString();
                    Action<Creeper> lam = (tt) => gameEndConditionHandler.IncreaseCount(conditionName);
                    triggerHandler.CreeperTrigger(creeperId, 0, lam, fieldObject, fieldObject.Field);
                }
                else
                {
                    Action lam = () => gameEndConditionHandler.IncreaseCount(conditionName);
                    timeManagerBehaviour.RegisterEvent(time, lam, method.Method + fieldObject.Field.ID + fieldObject.Name, fieldObject.GetHashCode());
                }
            }
        }
    }

    private void RegisterSpawn(FieldObject fieldObject, float amount, float time, float interval, string creeperId, string ID, int objectID)
    {
        timeManagerBehaviour.RegisterEvent(time, () => SpawnCreepAt(fieldObject.Field, amount, creeperId), ID, objectID, interval);
    }

    public bool CreateSpawnerAt(float x, float y, FieldObject spawner, bool allowMultiple = true)
    {
        if (!GetFieldByCoords(x, y, out Field field))
        {
            return false;
        }
        return CreateSpawner(field, spawner, allowMultiple);
    }

    public bool CreateSpawner(Field field, FieldObject spawner, bool allowMultiple = true)
    {
        if (!allowMultiple && field.FieldObjects != null && field.FieldObjects.Count > 0)
        {
            return false;
        }
        field.FieldObjects.Add(spawner);
        spawner.Field = field;
        Core.Game.State.CurrentLevel.GameField.FieldObjects.Add(spawner);
        ConvertFieldObjectMethods(spawner);
        foreach (var action in OnFieldObjectCreatedEvent)
        {
            action.Invoke(spawner);
        }
        return true;
    }


    public bool SpawnCreepAt(float x, float y, float amount, string creeperId)
    {
        if (!GetFieldByCoords(x, y, out Field field))
        {
            return false;
        }
        return SpawnCreepAt(field, amount, creeperId);
    }

    public bool SpawnCreepAt(Field field, float amount, string creeperId)
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
                    CreeperChanged(field, null);
                }
            }
            else
            {
                field.Creep.Value += amount;
            }
        }
        else
        {
            if (field.Creep == null)
            {
                field.Creep = new Creep();
            }
            Debug.Log("NewCreep: " + field.ID);
            field.Creep.Value = amount;
            field.Creep.Creeper = creepers[creeperId];
            CreeperChanged(field, null);
        }
        return true;
    }

    public bool DestroyBorder(Border border, bool replace = true)
    {
        if (border != null)
        {
            borders.Remove(border);
            Core.Game.State.CurrentLevel.GameField.Borders.Remove(border);

            if (replace)
            {
                CreateNewDefaultBorder(border.Field1, border.Field2, borders);
            }
            foreach (var action in OnBorderDestroyedEvent)
            {
                action.Invoke(border);
            }
            triggerHandler.UnRegisterByObjectID(border);
            timeManagerBehaviour.UnregisterByObjectID(border.GetHashCode());
            return true;
        }
        return false;
    }

    public void DestroyFieldObject(FieldObject fieldO)
    {
        if (fieldO != null)
        {
            Core.Game.State.CurrentLevel.GameField.FieldObjects.Remove(fieldO);

            foreach (var action in OnFieldObjectDestroyedEvent)
            {
                action.Invoke(fieldO);
            }
            triggerHandler.UnRegisterByObjectID(fieldO);
            timeManagerBehaviour.UnregisterByObjectID(fieldO.GetHashCode());
        }
    }


    private void CreeperChanged(Field field, Creeper oldCreeper)
    {
        triggerHandler.InvokeTriggered(field, oldCreeper);
        foreach (var action in OnCreeperChanged)
        {
            action(field, oldCreeper);
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




    private void UpdateCreep()
    {
        UpdateCreepAtBorders();
    }

    private void UpdateCreepAtBorders()
    {
        //float tickFlowFactor = Time.deltaTime * Core.Game.State.Mode.FlowSpeed;
        float tickFlowFactor = 1;
        List<Creep> creeperChanged = new();
        foreach (var border in borders)
        {
            UpdateCreepAtBorder(border, tickFlowFactor, creeperChanged);
        }
        foreach (var creeper in creeperChanged)
        {
            creeper.CreeperChanged = false;
        }
    }

    private void UpdateCreepAtBorder(Border border, float tickFlowFactor, List<Creep> creeperChanged)
    {
        if (border.BorderStatus.FlowValue == 0)
        {
            return;
        }
        Creep creep1 = null, creep2 = null;
        if (border.Field1 != null)
        {
            creep1 = border.Field1.Creep;
        }
        if (border.Field2 != null)
        {
            creep2 = border.Field2.Creep;
        }
        if (creep1 == null && creep2 == null)
        {
            return;
        }
        float heightDiff = border.Field1.Height - border.Field2.Height;
        if (creep1 == null)
        {
            CreepingCreeper(border.Field1, border, tickFlowFactor, creep2, heightDiff, creeperChanged);
        }
        else if (creep2 == null)
        {
            CreepingCreeper(border.Field2, border, tickFlowFactor, creep1, -heightDiff, creeperChanged);
        }
        else
        {
            CreeperFight(border, tickFlowFactor, creep1, creep2, heightDiff, creeperChanged);
        }
    }

    private void CreeperFight(Border border, float tickFlowFactor, Creep creep1, Creep creep2, float heightDiff, List<Creep> creeperChanged)
    {
        float flow1 = GetFlowPreassure(creep1, border.BorderStatus.FlowValue, tickFlowFactor, heightDiff);
        float flow2 = GetFlowPreassure(creep2, border.BorderStatus.FlowValue, tickFlowFactor, -heightDiff);
        float diff = flow1 - flow2;
        if (flow1 < Core.Game.State.Mode.MinFlow && flow2 < Core.Game.State.Mode.MinFlow)
        {
            return;
        }
        if (creep1.Creeper == creep2.Creeper)
        {
            creep1.Value -= diff;
            creep2.Value += diff;
            return;
        }
        creep1.Value -= flow1;
        creep2.Value -= flow2;
        if (diff > 0)
        {
            UpdateLoosingCreep(border.Field2, creep1, creep2, creeperChanged, diff);
        }
        else
        {
            UpdateLoosingCreep(border.Field1, creep2, creep1, creeperChanged, -diff);
        }
    }

    private void UpdateLoosingCreep(Field field, Creep winnerCreep, Creep looserCreep, List<Creep> creeperChanged, float diff)
    {
        if (looserCreep.Value < diff)
        {
            var oldCreeper = looserCreep.Creeper;
            looserCreep.Value = diff - looserCreep.Value;
            looserCreep.Creeper = winnerCreep.Creeper;
            looserCreep.CreeperChanged = true;
            creeperChanged.Add(looserCreep);
            CreeperChanged(field, oldCreeper);
        }
        else
        {
            looserCreep.Value -= diff;
        }
    }

    private void CreepingCreeper(Field field, Border border, float tickFlowFactor, Creep creep, float heightDiff, List<Creep> creeperChanged)
    {
        float flow = GetFlowPreassure(creep, border.BorderStatus.FlowValue, tickFlowFactor, heightDiff);
        float creepValueAtOrigin = creep.Value - flow;
        if (flow < Core.Game.State.Mode.MinNewCreep || creepValueAtOrigin < Core.Game.State.Mode.MinNewCreep)
        {
            Debug.Log("rest Value: " + creep.Value + " flow: " + flow + " minCreep: " + Core.Game.State.Mode.MinNewCreep + "  creepValueAtOrigin: " + creepValueAtOrigin);
            return;
        }
        creep.Value = creepValueAtOrigin;
        if (creep.Value < 1)
        {
            Debug.Log("Value: " + creep.Value + " flow: " + flow);
        }
        var newCreep = new Creep()
        {
            Creeper = creep.Creeper,
            Value = flow,
            CreeperChanged = true
        };
        field.Creep = newCreep;
        creeperChanged.Add(newCreep);
        CreeperChanged(field, null);
    }

    private float GetFlowPreassure(Creep creep, float borderFlowFactor, float tickFlowFactor, float heightDiff)
    {
        if (creep.CreeperChanged)
        {
            return 0;
        }
        if (heightDiff == 0)
        {
            return creep.Value * borderFlowFactor * tickFlowFactor / 2;
        }
        float creeperFactor = creep.Creeper.Parameters.HightTraverseRate;
        float heightFactor = creeperFactor * heightDiff;
        if (heightFactor < 0)
        {
            heightFactor = 1 / Mathf.Abs(heightFactor);
        }
        return creep.Value * borderFlowFactor * tickFlowFactor * heightFactor / 2;
    }


}
