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

    [SerializeField]
    private Terrain mainTerrain;

    [SerializeField]
    private TerrainBehaviour terrainBehaviour;

    private Dictionary<string, GameObject> Templates;

    private void Awake()
    {
        FetchTemplates();
    }

    // Start is called before the first frame update
    void Start()
    {
        creepBehaviour.StartGame();

        foreach (var fieldObject in Core.Game.State.GameField.FieldObjects)
        {
            SpawnFieldObject(fieldObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FetchTemplates()
    {
        Templates = new Dictionary<string, GameObject>();
        foreach (Transform tran in TemplateParent.transform)
        {
            Templates.Add(tran.name, tran.gameObject);
        }
    }
        

    private void SpawnFieldObject(FieldObject fieldObject)
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
        newFieldGO.name = GetFieldObjectName(fieldObject.Field.ID);

        Vector3 mapPos = new Vector3(fieldObject.Field.Coords.X, 0, fieldObject.Field.Coords.Y);
        newFieldGO.transform.position = getTerrainCoordinates(mapPos);
        newFieldGO.SetActive(true);
    }

    private static string GetFieldObjectName(string fieldName)
    {
        return "FieldObject:" + fieldName;
    }

    private Vector3 getTerrainCoordinates(Vector3 pos)
    {
        Vector3 mapSize = mainTerrain.terrainData.size;
        pos.x -= terrainBehaviour.XOffset;
        pos.z -= terrainBehaviour.YOffset;
        pos.x = (int)((2 * pos.x + 1) * mapSize.x / (2 * terrainBehaviour.FieldCountX));
        pos.z = (int)((2 * pos.z + 1) * mapSize.z / (2 * terrainBehaviour.FieldCountY));
        return pos;
    }

}
