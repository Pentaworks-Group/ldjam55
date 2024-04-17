

using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UserActionHandler
{
    public List<UserAction> UserActions => Core.Game.State.CurrentLevel.UserActions;
    private Dictionary<UserAction, Func<UserActionInput, bool>> buildActions = new();
    private CreepBehaviour creepBehaviour;


    public void Init(CreepBehaviour creepBehaviour)
    {
        this.creepBehaviour = creepBehaviour;
        buildActions.Clear();
    }
    public bool UseAction(UserAction action, UserActionInput target)
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

    private bool InvokeAction(UserAction action, UserActionInput target)
    {
        if (!buildActions.TryGetValue(action, out Func<UserActionInput, bool> builtAction))
        {
            builtAction = BuildAction(action);
        }
        return builtAction.Invoke(target);
    }

    private Func<UserActionInput, bool> BuildAction(UserAction action)
    {
        if (action.Name == "CreateSpawner")
        {
            Func<UserActionInput, bool> builtAction = BuildSpawnerAction(action);
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "SpawnCreep")
        {
            Func<UserActionInput, bool> builtAction = BuildSpawnCreepAction(action);
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "DestroyWall")
        {
            Func<UserActionInput, bool> builtAction = DestroyBorder;
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "CreateWall")
        {
            Func<UserActionInput, bool> builtAction = BuildCreateBorderAction(action);
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        else if (action.Name == "CreateField")
        {
            Func<UserActionInput, bool> builtAction = CreateField;
            buildActions.Add(action, builtAction);
            return builtAction;
        }
        return null;
    }



    private bool CreateField(UserActionInput target)
    {

        if (target.InputType == "Coords")
        {
            Vector2 coords = (Vector2)target.Input;
            return creepBehaviour.CreateField(coords.x, coords.y);
        }
        return false;
    }

    private Func<UserActionInput, bool> BuildCreateBorderAction(UserAction action)
    {
        JObject paramsObject = JObject.Parse(action.ActionParamers);
        float flowRate = float.Parse(paramsObject["FlowRate"].ToString());
        var borderTypeName = GetStrFromPar(paramsObject, "BorderTypeName", "");
        var model = GetStrFromPar(paramsObject, "Model", "Wall");
        var borderStatus = new BorderStatus() { FlowValue = flowRate };
        var borderType = new BorderType() { Name = borderTypeName, Model = model };
        var newBorder = new Border { BorderStatus = borderStatus, BorderType = borderType };

        return (target) => CreateBorder(target, newBorder);

    }

    private bool CreateBorder(UserActionInput target, Border rawBorder)
    {
        if (target.InputType == "Coords")
        {
            var coords = (Vector2)target.Input;
            var finalBorder = CreateNewBorder(rawBorder);
            return creepBehaviour.SpawnBorderAt(coords.x, coords.y, finalBorder);

        }
        return false;
    }

    private Border CreateNewBorder(Border border)
    {
        return new Border()
        {
            Methods = border.Methods,
            BorderStatus = new BorderStatus() { FlowValue = border.BorderStatus.FlowValue },
            BorderType = new BorderType() { Model = border.BorderType.Model, Name = border.BorderType.Name }
        };
    }

    private bool DestroyBorder(UserActionInput target)
    {
        if (target.InputType == "Border")
        {
            Border border = (Border)target.Input;
            if (border.Field2 != null && border.Field1 != null && border.BorderType != null)
            {
                return creepBehaviour.DestroyBorder(border);

            }
        }
        return false;
    }

    private Func<UserActionInput, bool> BuildSpawnerAction(UserAction action)
    {
        JObject paramsObject = JObject.Parse(action.ActionParamers);

        FieldObject spawner = new()
        {
            Name = GetStrFromPar(paramsObject, "Name", ""),
            Description = GetStrFromPar(paramsObject, "Description", ""),
            Material = GetStrFromPar(paramsObject, "Material", "Creeper_Yellow_Stage_3"),
            Model = GetStrFromPar(paramsObject, "Model", "Spawner_3"),
            Hidden = bool.Parse(GetStrFromPar(paramsObject, "Hidden", "false"))
        };

        bool allowMultiple = bool.Parse(GetStrFromPar(paramsObject, "AllowMultiple", "false"));

        float amount = float.Parse(paramsObject["Amount"].ToString());
        float intervall = float.Parse(paramsObject["Intervall"].ToString());
        string creeperId = paramsObject["Creeper"].ToString();
        var method = new MethodBundle();
        method.Method = "SpawnCreep";
        method.ArumentsJson = $"{{\"Amount\": {amount},\"Time\": {intervall},\"Creeper\": \"{creeperId}\", \"Intervall\": {intervall}}}";
        spawner.Methods = new List<MethodBundle>
                {
                    method
                };
        return (target) => CreateSpawner(target, spawner, allowMultiple);
    }

    private bool CreateSpawner(UserActionInput target, FieldObject spawner, bool allowMultiple)
    {
        if (target.InputType == "Coords")
        {
            var border = (Vector2)target.Input;
            var newSpawner = new FieldObject() {
                Name = spawner.Name,
                Description = spawner.Description, 
                Material = spawner.Material,
                Model = spawner.Model,
                Hidden = spawner.Hidden,
                Methods = spawner.Methods,
            };
            return creepBehaviour.CreateSpawnerAt(border.x, border.y, newSpawner, allowMultiple);
        }
        return false;
    }

    private string GetStrFromPar(JObject paramsObject, string key, string def)
    {
        var msgProperty = paramsObject.Property(key);
        if (msgProperty != null)
        {
            return msgProperty.Value.ToString();
        }
        return def;
    }

    private bool SpawnCreep(UserActionInput target, float amount, string creeperId)
    {
        if (target.InputType == "Coords")
        {
            var coords = (Vector2)target.Input;

            return creepBehaviour.SpawnCreepAt(coords.x, coords.y, amount, creeperId);
        }
        return false;
    }

    private Func<UserActionInput, bool> BuildSpawnCreepAction(UserAction action)
    {
        {
            JObject paramsObject = JObject.Parse(action.ActionParamers);
            float amount = float.Parse(paramsObject["Amount"].ToString());
            string creeperId = paramsObject["Creeper"].ToString();
            return (target) => SpawnCreep(target, amount, creeperId);
        };

    }


}
