using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using Assets.Scripts.Scenes.GameScene;
using System.Collections.Generic;
using TMPro;
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

    [SerializeField]
    private GameStartScreenBehaviour gameStartScreenBehaviour;

    [SerializeField]
    private GameEndScreenBehaviour gameEndScreenBehaviour;

    [SerializeField]
    private CameraBehaviour cameraBehaviour;

    [SerializeField]
    private TextMeshProUGUI timeElapsedDisplay;

    [SerializeField]
    private TextMeshProUGUI levelDisplay;

    private Dictionary<string, GameObject> Templates;

    private Dictionary<string, GameObject> Borders;

    private float timeUpdate = 0;

    private float levelStartTime = 0;

    private void Awake()
    {
        FetchTemplates();
    }

    // Start is called before the first frame update
    void Start()
    {
        //        creepBehaviour.StartGame();
        
        foreach (var fieldObject in Core.Game.State.CurrentLevel.GameField.FieldObjects)
        {
            SpawnFieldObject(fieldObject);
        }

        Borders = new Dictionary<string, GameObject>();
        foreach (var border in Core.Game.State.CurrentLevel.GameField.Borders)
        {
            if (border.BorderType.Name == "BorderWall")
            {
                SpawnBorder(border);
            }
        }

        creepBehaviour.DestroyBorderEvent.Add(DestroyBorder);
        creepBehaviour.CreateFieldObjectEvent.Add(SpawnFieldObject);
        creepBehaviour.CreateBorderEvent.Add(SpawnBorder);

        creepBehaviour.gameEndConditionHandler.RegisterListener(GameEnded);

        var backgroundAudioClips = new List<AudioClip>()
            {
                GameFrame.Base.Resources.Manager.Audio.Get("Music_2"),
                GameFrame.Base.Resources.Manager.Audio.Get("Music_3"),
                GameFrame.Base.Resources.Manager.Audio.Get("Music_4"),
                GameFrame.Base.Resources.Manager.Audio.Get("Music_5")
            };

        GameFrame.Base.Audio.Background.ReplaceClips(backgroundAudioClips);

        levelDisplay.SetText(Core.Game.State.CurrentLevel.Name);

        gameStartScreenBehaviour.ShowStartScreen();
        cameraBehaviour.UpdatePosition();
    }

    public void StartLevel()
    {
        levelStartTime = Time.time;

        Core.Game.PlayButtonSound();

        gameStartScreenBehaviour.HideStartScreen();

        creepBehaviour.StartGame();

        Core.Game.isRunning = true;
    }

    private void SpawnBorder(Border border)
    {
        var wallTemplate = Templates[border.BorderType.Model];
        var newFieldGO = Instantiate(wallTemplate, World.transform);
        newFieldGO.name = "Wall";
        var container = newFieldGO.AddComponent<GameFieldContainerBehaviour>();
        container.ContainedObject = border;
        container.ObjectType = "Wall";
        SetBorderPositionAndRotation(border, newFieldGO);
        newFieldGO.SetActive(true);
        Borders.Add(GetBorderKey(border), newFieldGO);
    }

    private void Update()
    {
//        Core.Game.State.TimeElapsed += Time.deltaTime;
        if (timeUpdate < 0)
        {
            timeElapsedDisplay.text = (Time.time - levelStartTime).ToString("F0");// Core.Game.State.TimeElapsed.ToString("F1");
            timeUpdate = 0.2f;
        }
        else
        {
            timeUpdate -= Time.deltaTime;
        }
    }

    private void GameEnded(GameEndCondition conditon)
    {
        gameEndScreenBehaviour.UpdateUI(conditon, conditon.Description);
        gameEndScreenBehaviour.gameObject.SetActive(true);
        Core.Game.isRunning = false;
    }

    private void FetchTemplates()
    {
        Templates = new Dictionary<string, GameObject>();
        foreach (Transform tran in TemplateParent.transform)
        {
            Templates.Add(tran.name, tran.gameObject);
        }
    }

    public void StartNextLevel()
    {
        Core.Game.PlayButtonSound();

        Core.Game.StartNextLevel();

        levelDisplay.SetText(Core.Game.State.CurrentLevel.Name);
    }

    public void RestartLevel()
    {
        Core.Game.PlayButtonSound();

        Core.Game.RestartLevel();

        levelDisplay.SetText(Core.Game.State.CurrentLevel.Name);
    }

    private string GetBorderKey(Assets.Scripts.Core.Model.Border border)
    {
        return border.BorderType.Name + border.Field1.ID + border.Field2.ID;
    }

    public void DestroyBorder(Assets.Scripts.Core.Model.Border border)
    {
        if (Borders.TryGetValue(GetBorderKey(border), out var borderGO))
        {
            Destroy(borderGO);
        }
    }


    private void SpawnFieldObject(Assets.Scripts.Core.Model.FieldObject fieldObject)
    {
        var fieldTemplate = Templates[fieldObject.Model];
        var newFieldGO = Instantiate(fieldTemplate, World.transform);
        var container = newFieldGO.AddComponent<GameFieldContainerBehaviour>();
        container.ContainedObject = fieldObject;
        container.ObjectType = "FieldObject";
        if(fieldObject.Material.Length > 0 )
        {
            var material = GameFrame.Base.Resources.Manager.Materials.Get(fieldObject.Material);
            newFieldGO.GetComponent<Renderer>().material = material;
            foreach (Transform child in newFieldGO.transform)
            {
                child.GetComponent<Renderer>().material = material;
            }
        }
        newFieldGO.name = GetFieldObjectName(fieldObject.Field.ID);

        float height = terrainBehaviour.GetFieldHeight(fieldObject.Field) * mainTerrain.terrainData.size.y;
        Vector3 mapPos = new Vector3(fieldObject.Field.Coords.X, height, fieldObject.Field.Coords.Y);
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
        
    private void SetBorderPositionAndRotation(Assets.Scripts.Core.Model.Border border, GameObject borderObject)
    {
        float x1 = border.Field1.Coords.X;
        float y1 = border.Field1.Coords.Y;
        float x2 = border.Field2.Coords.X;
        float y2 = border.Field2.Coords.Y;
        float x = (x2 - x1) / 2f;
        float z = (y2 - y1) / 2f;
        Vector3 mapPos = new Vector3(x1 + x, 0, y1 + z);
        Vector3 terrainPos = getTerrainCoordinates(mapPos);

        float height1 = terrainBehaviour.GetFieldHeight(border.Field1) * mainTerrain.terrainData.size.y;
        float height2 = terrainBehaviour.GetFieldHeight(border.Field2) * mainTerrain.terrainData.size.y;
        float height = Mathf.Min(height1, height2);
        terrainPos.y = height;

        borderObject.transform.position = terrainPos;
        if (border.Field1.Coords.Y != border.Field2.Coords.Y)
        {
            borderObject.transform.eulerAngles = new Vector3(0, 90, 0);
        }
    }

}
