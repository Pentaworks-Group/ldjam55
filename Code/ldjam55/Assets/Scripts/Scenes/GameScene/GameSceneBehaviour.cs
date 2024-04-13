using Assets.Scripts.Core;
using Assets.Scripts.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSceneBehaviour : MonoBehaviour
{
    public List<TextMeshProUGUI> fields;

    float flow_rate = 0.1f;

    GameState testGame;

    // Start is called before the first frame update
    void Start()
    {

        List<Field> gameFields = new List<Field>
        {
            new Field { Height=0, Creep=new Creep{ Value = 16, Creeper = new Creeper{ Name = "Water" } } , ID = "0,0"},
            new Field { Height=0 , ID = "0,1"},
            new Field { Height=0 , ID = "0,2"},
            new Field { Height=0, Creep=new Creep{ Value = 5, Creeper = new Creeper{ Name = "Fire" } } , ID = "0,3"},
            new Field { Height=0 , ID = "1,0"},
            new Field { Height=0 , ID = "1,1"},
            new Field { Height=0 , ID = "1,2"},
            new Field { Height=0 , ID = "1,3"},
            new Field { Height=0 , ID = "2,0"},
            new Field { Height=0 , ID = "2,1"},
            new Field { Height=0 , ID = "2,2"},
            new Field { Height=0 , ID = "2,3"},
            new Field { Height=0 , ID = "3,0"},
            new Field { Height=0 , ID = "3,1"},
            new Field { Height=0 , ID = "3,2"},
            new Field { Height=0 , ID = "3,3"},
        };

        List<Border> borders = new List<Border>
        {
            new Border { Field1 = gameFields[0], Field2 = gameFields[1], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[0], Field2 = gameFields[4], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[1], Field2 = gameFields[2], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[1], Field2 = gameFields[5], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[2], Field2 = gameFields[3], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[2], Field2 = gameFields[6], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[3], Field2 = gameFields[7], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[4], Field2 = gameFields[5], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[4], Field2 = gameFields[8], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[5], Field2 = gameFields[6], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[5], Field2 = gameFields[9], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[6], Field2 = gameFields[7], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[6], Field2 = gameFields[10], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[7], Field2 = gameFields[11], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[8], Field2 = gameFields[9], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[8], Field2 = gameFields[12], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[9], Field2 = gameFields[10], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[9], Field2 = gameFields[13], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[10], Field2 = gameFields[11], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[10], Field2 = gameFields[14], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[11], Field2 = gameFields[15], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[12], Field2 = gameFields[13], BorderStatus = new BorderStatus{ Value = flow_rate } },
            new Border { Field1 = gameFields[13], Field2 = gameFields[14], BorderStatus = new BorderStatus{ Value = 0 } },
            new Border { Field1 = gameFields[14], Field2 = gameFields[15], BorderStatus = new BorderStatus{ Value = flow_rate } },
        };

        testGame = new GameState { Fields = gameFields, Borders = borders };
    }

    // Update is called once per frame
    void Update() 
    {
        distributeCreep();

        //Just for the test scene
        for (int i = 0; i < 16;  i++)
        {
            fields[i].SetText(String.Format("{0:0.00}", testGame.Fields[i].Creep.Value));
            if (testGame.Fields[i].Creep != null && testGame.Fields[i].Creep.Creeper != null && testGame.Fields[i].Creep.Creeper.Name=="Fire" && testGame.Fields[i].Creep.Value>=0.01)
            {
                fields[i].color = Color.red;
            } else if(testGame.Fields[i].Creep != null && testGame.Fields[i].Creep.Creeper != null && testGame.Fields[i].Creep.Creeper.Name == "Water" && testGame.Fields[i].Creep.Value >= 0.01)
            {
                fields[i].color = Color.blue;
            } else
            {
                fields[i].color = Color.white;
            }
        }
    }

    private void distributeCreep()
    {
        foreach (var field in testGame.Fields)
        {
            if(field.Creep != null)
            {
                field.Creep.ValueOld = field.Creep.Value;
            }
        }

        foreach (var border in testGame.Borders)
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
        else if(flow < 0)
        {
            field1.Creep.Creeper = field2.Creep.Creeper;
        }
    }

    private float getFlow(Field field1, Field field2, float borderState)
    {
        float valueField1 = 0;
        if(field1.Creep != null)
        {
            valueField1 = field1.Creep.ValueOld;
        }
        float valueField2 = 0;
        if(field2.Creep != null)
        {
            valueField2 = field2.Creep.ValueOld;
        }
        float flow = (valueField1-valueField2) * borderState * Time.deltaTime;
        return flow;
    }

}
