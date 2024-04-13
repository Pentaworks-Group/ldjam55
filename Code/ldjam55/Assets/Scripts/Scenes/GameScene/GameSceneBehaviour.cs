using Assets.Scripts.Base;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSceneBehaviour : MonoBehaviour
{

    float flow_rate = 0.1f;
    private List<Border> borders = new List<Border>();
    private bool isRunning = false;

    void Update()
    {
        if (isRunning)
        {
            distributeCreep();
        }
    }

    public void StartGame()
    {
        var bordersAdded = new HashSet<string>();
        borders.Clear();
        foreach (var border in Core.Game.State.GameField.Borders)
        {
            borders.Add(border);
            bordersAdded.Add(GetBorderKey(border));
        }

        var topFields = new Dictionary<string, Field>();
        foreach (var field in Core.Game.State.GameField.Fields)
        {
            Field cField;
            string fieldKey = GetFieldKey(field);
            if (topFields.TryGetValue(fieldKey, out cField))
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
                var borderStatus = new BorderStatus() { Value = flow_rate };
                var borderType = new BorderType() { Name = "Nothing" };
                var newBorder = new Border { Field1 = field, Field2 = neighbour, BorderStatus = borderStatus, BorderType = borderType };
                bordersAdded.Add(GetBorderKey(newBorder));
                borders.Add(newBorder);
            }
        }
        isRunning = true;
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

        foreach (var border in borders)
        {
            float flow = getFlow(border.Field1, border.Field2, border.BorderStatus.Value);
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
            field2.Creep.Creeper = field1.Creep.Creeper;
        }
        else if (flow < 0)
        {
            field1.Creep.Creeper = field2.Creep.Creeper;
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
        float flow = (valueField1 - valueField2) * borderState * Time.deltaTime;
        return flow;
    }

}
