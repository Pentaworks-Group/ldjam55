

using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class UserActionHandler
{
    public List<UserAction> UserActions => Core.Game.State.CurrentLevel.UserActions;
    private Dictionary<UserAction, Func<object, bool>> buildActions = new();
    private CreepBehaviour creepBehaviour;


    public void Init(CreepBehaviour creepBehaviour)
    {
        this.creepBehaviour = creepBehaviour;
        buildActions.Clear();
    }
    public bool UseAction(UserAction action, object target)
    {
        if (action != null && action.UsesRemaining > 0)
        {
            if (InvokeAction(action, target))
            {
                action.UsesRemaining--;
            }
            return true;
        }
        return false;
    }

    private bool InvokeAction(UserAction action, object target)
    {
        if (!buildActions.TryGetValue(action, out Func<object, bool> builtAction))
        {
            builtAction = BuildAction(action);
        }
        return builtAction.Invoke(target);
    }

    private Func<object, bool> BuildAction(UserAction action)
    {
        if (action.Name == "CreateSpawner")
        {
            Func<object, bool> builtAction = BuildSpawnerAction(action);
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "SpawnCreep")
        {
            Func<object, bool> builtAction = BuildSpawnCreepAction(action);
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "DestroyWall")
        {
            Func<object, bool> builtAction = DestroyBorder;
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "CreateWall")
        {
            Func<object, bool> builtAction = CreateBorder;
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        return null;
    }

    private bool CreateBorder(object target)
    {
        if (target is Border)
        {
            Border border = (Border)target;
            if (border.Field2 != null && border.Field1 != null)
            {
                return creepBehaviour.SpawnBorder((Border)target);
            }
        }
        return false;
    }

    private bool DestroyBorder(object target)
    {
        if (target is Border)
        {
            Border border = (Border)target;
            if (border.Field2 != null && border.Field1 != null && border.BorderType != null)
            {
                creepBehaviour.DestroyBorder((Border)target);
                return true;

            }
        }
        return false;
    }

    private Func<object, bool> BuildSpawnerAction(UserAction action)
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
        return (target) => CreateSpawner(target, spawner);
    }

    private bool CreateSpawner(object target, FieldObject spawner)
    {
        creepBehaviour.CreateSpawner(((Border)target).Field1, spawner);
        return true;
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

    private bool SpawnTargetSave(object target, float amount, string creeperId)
    {
        if (target != null && target is Border)
        {
            var b = (Border)target;
            if (b.Field1 == null)
            {
                return false;
            }
            creepBehaviour.SpawnCreepAt(((Border)target).Field1, amount, creeperId);
            return true;
        }
        return false;
    }

    private Func<object, bool> BuildSpawnCreepAction(UserAction action)
    {
        {
            JObject paramsObject = JObject.Parse(action.ActionParamers);
            float amount = float.Parse(paramsObject["Amount"].ToString());
            string creeperId = paramsObject["Creeper"].ToString();
            return (target) => SpawnTargetSave(target, amount, creeperId);
        };

    }


}
