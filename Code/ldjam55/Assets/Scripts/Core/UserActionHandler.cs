

using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class UserActionHandler
{
    public List<UserAction> UserActions => Core.Game.State.CurrentLevel.UserActions;
    private Dictionary<UserAction, Action<object>> buildActions = new();
    private CreepBehaviour creepBehaviour;


    public void Init(CreepBehaviour creepBehaviour)
    {
        if (this.creepBehaviour == null)
        {
            this.creepBehaviour = creepBehaviour;
        }
    }
    public bool UseAction(UserAction action, object target)
    {
        if (action != null && action.UsesRemaining > 0)
        {
            InvokeAction(action, target);
            action.UsesRemaining--;
            return true;
        }
        return false;
    }

    private void InvokeAction(UserAction action, object target)
    {
        if (!buildActions.TryGetValue(action, out Action<object> builtAction))
        {
            builtAction = BuildAction(action);
        }
        builtAction.Invoke(target);
    }

    private Action<object> BuildAction(UserAction action)
    {
        if (action.Name == "CreateSpawner")
        {
            Action<object> builtAction = BuildSpawnerAction(action);
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "SpawnCreep")
        {
            Action<object> builtAction = BuildSpawnCreepAction(action);
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "DestroyWall")
        {
            Action<object> builtAction = DestroyBorder;
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "CreateWall")
        {
            Action<object> builtAction = CreateBorder;
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        return null;
    }

    private void CreateBorder(object target)
    {
        if (target is Border)
        {
            Border border = (Border) target;
            if (border.Field2 != null && border.Field1 != null)
            {
                creepBehaviour.SpawnBorder((Border)target);
            }
        }
    }

    private void DestroyBorder(object target)
    {
        if (target is Border)
        {
            creepBehaviour.DestroyBorder((Border)target);
        }
    }

    private Action<object> BuildSpawnerAction(UserAction action)
    {
        JObject paramsObject = JObject.Parse(action.ActionParamers);
        float amount = float.Parse(paramsObject["Amount"].ToString());
        float intervall = float.Parse(paramsObject["Intervall"].ToString());
        string creeperId = paramsObject["Creeper"].ToString();
        FieldObject spawner = new();
        spawner.Name = GetStr(paramsObject, "Name", "");
        spawner.Description = GetStr(paramsObject, "Description", "");
        spawner.Material = GetStr(paramsObject, "Material", "Creeper_Yellow_Stage_3");
        spawner.Model = GetStr(paramsObject, "Model", "Spawner_3");
        spawner.Hidden = false;
        var method = new MethodBundle();
        method.Method = "SpawnCreep";
        method.ArumentsJson = $"{{\"Amount\": {amount},\"Time\": {intervall},\"Creeper\": \"{creeperId}\", \"Intervall\": {intervall}}}";
        spawner.Methods = new List<MethodBundle>
                {
                    method
                };
        return (target) => creepBehaviour.CreateSpawner(((Border)target).Field1, spawner);
    }

    private string GetStr(JObject paramsObject, string key, string def)
    {
        var msgProperty = paramsObject.Property(key);
        if (msgProperty != null)
        {
            return msgProperty.Value.ToString();
        }
        return def;
    }

    private void SpawnTargetSave(object target, float amount, string creeperId)
    {
        if (target is Border)
        {
            creepBehaviour.SpawnCreepAt(((Border)target).Field1, amount, creeperId);
        }
        else
        {
            Debug.Print("Wrong target type for SpawnTargetSave");
        }
    }

    private Action<object> BuildSpawnCreepAction(UserAction action)
    {
        {
            JObject paramsObject = JObject.Parse(action.ActionParamers);
            float amount = float.Parse(paramsObject["Amount"].ToString());
            string creeperId = paramsObject["Creeper"].ToString();
            return (target) => SpawnTargetSave(target, amount, creeperId);
        };

    }


}
