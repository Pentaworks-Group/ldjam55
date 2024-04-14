using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject TemplateParent;

    [SerializeField]
    private GameObject World;

    [SerializeField]
    private CreepBehaviour creepBehaviour;

    private Dictionary<string, GameObject> Templates;

    private Dictionary<string, GameObject> WorldCreep;

    private void Awake()
    {
        FetchTemplates();
    }

    // Start is called before the first frame update
    void Start()
    {
        creepBehaviour.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCreep();
    }

    private void FetchTemplates()
    {
        Templates = new Dictionary<string, GameObject>();
        foreach (Transform tran in TemplateParent.transform)
        {
            Templates.Add(tran.name, tran.gameObject);
        }
    }


    private void SpawnFieldObject(GameObject field, FieldObject fieldObject)
    {
        var fieldTemplate = Templates[fieldObject.Model];
        var newFieldGO = Instantiate(fieldTemplate, World.transform);
        var container = newFieldGO.AddComponent<GameFieldContainerBehaviour>();
        container.ContainedObject = fieldObject;
        container.ObjectType = "FieldObject";
        var material = GameFrame.Base.Resources.Manager.Materials.Get(fieldObject.Material);
        newFieldGO.GetComponent<Renderer>().material = material;
        foreach (Transform child in newFieldGO.transform)
        {
            child.GetComponent<Renderer>().material = material;
        }
        newFieldGO.name = GetFieldObjectName(field.name);
        newFieldGO.transform.position = field.transform.position + new Vector3(0, 1, 0);
        newFieldGO.SetActive(true);
    }

    private static string GetFieldObjectName(string fieldName)
    {
        return "FieldObject:" + fieldName;
    }

    public void UpdateCreep()
    {
        var creepTemplate = Templates["Creep"];
        foreach (var field in Core.Game.State.GameField.Fields)
        {
            if (field.Creep != null && field.Creep.Value > 0)
            {
                GameObject creepGO;
                if (!WorldCreep.TryGetValue(field.ID, out creepGO))
                {
                    creepGO = Instantiate(creepTemplate, World.transform);
                    creepGO.name = "Creep_" + field.Creep.Creeper.Name;
                    var container = creepGO.AddComponent<GameFieldContainerBehaviour>();
                    container.ContainedObject = field;
                    container.ObjectType = "Creep";

                    creepGO.transform.position = new Vector3(field.Coords.X, field.Height + 1, field.Coords.Y);
                    WorldCreep[field.ID] = creepGO;
                }

                var material = GameFrame.Base.Resources.Manager.Materials.Get(field.Creep.Creeper.Parameters.Material);
                creepGO.GetComponent<Renderer>().material = material;
                creepGO.transform.localScale = new Vector3(1, field.Creep.Value / 10, 1);
            }
        }
    }

}
