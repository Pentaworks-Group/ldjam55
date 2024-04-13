using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSceneBehaviour : MonoBehaviour
{
    public TextMeshProUGUI field1;
    public TextMeshProUGUI field2;
    public TextMeshProUGUI field3;
    public TextMeshProUGUI field4;
    public TextMeshProUGUI field5;
    public TextMeshProUGUI field6;
    public TextMeshProUGUI field7;
    public TextMeshProUGUI field8;
    public TextMeshProUGUI field9;
    public TextMeshProUGUI field10;
    public TextMeshProUGUI field11;
    public TextMeshProUGUI field12;
    public TextMeshProUGUI field13;
    public TextMeshProUGUI field14;
    public TextMeshProUGUI field15;
    public TextMeshProUGUI field16;

    double[][] fields = new double[4][];
    double[][] old_values = new double[4][];

    double flow_rate = 0.5;

    // Start is called before the first frame update
    void Start()
    {
        fields[0] = new double[4];
        fields[1] = new double[4];
        fields[2] = new double[4];
        fields[3] = new double[4];

        old_values[0] = new double[4];
        old_values[1] = new double[4];
        old_values[2] = new double[4];
        old_values[3] = new double[4];

        fields[0][0] = 10;

        Debug.Log("Test");
    }

    // Update is called once per frame
    void Update()
    {
        distributeCreep();

        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());
        field1.SetText(fields[0][0].ToString());

        Debug.Log(fields);
    }

    // Go through all the fields

    // Distribute for one field
    private void distributeCreep()
    {
        for(int i=0; i<fields.Length; i++)
        {
            for(int j=0; j < fields[i].Length; j++)
            {
                old_values[i][j] = fields[i][j];
            }
        }

        for(int i = 0; i < fields.Length; i++)
        {
            //TODO: Energieerhaltung?
            for (int j = 0; j < fields[i].Length; j++)
            {
                if((i != j) && (i < fields.Length-1))
                {
                    double flow = getFlowToField(i, j, i + 1, j);
                    fields[i][j] -= flow;
                    fields[i+1][j] += flow;
                }
                if ((i != j) && (j < fields[i].Length - 1))
                {
                    double flow = getFlowToField(i, j, i, j + 1);
                    fields[i][j] -= flow;
                    fields[i][j + 1] += flow;
                }
            }
        }
    }

    private double getFlowToField(int x1, int y1, int x2, int y2)
    {
        double valueField1 = old_values[x1][y1];
        double valueField2 = old_values[x2][y2];
        double flow = (valueField1-valueField2)*flow_rate;
        return flow;
    }

}
