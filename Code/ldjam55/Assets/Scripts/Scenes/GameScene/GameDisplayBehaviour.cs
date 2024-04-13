
using Assets.Scripts.Core.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameDisplayBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject TemplateParent;

        [SerializeField]
        private GameObject World;
        [SerializeField]
        private Material FireMat;
        [SerializeField]
        private Material WaterMat;

        private Dictionary<string, GameObject> Templates;

        private Dictionary<string, GameObject> WorldCreep;


        private void Awake()
        {
            FetchTemplates();
        }




        public void GenerateGameField()
        {
            var fieldTemplate = Templates["Field"];
            foreach (var field in Base.Core.Game.State.GameField.Fields)
            {
                var newFieldGO = Instantiate(fieldTemplate, World.transform);
                newFieldGO.name = "Field:" + field.ID;
                newFieldGO.transform.position = new Vector3(field.Coords.X, field.Height, field.Coords.Y);
                newFieldGO.SetActive(true);
                foreach (var fieldObject in field.FieldObjects)
                {
                    SpawnFieldObject(newFieldGO, fieldObject);
                }
            }
            var wallTemplate = Templates["Wall"];
            foreach (var border in Base.Core.Game.State.GameField.Borders)
            {
                if (border.BorderType.Name == "BorderWall")
                {
                    var newFieldGO = Instantiate(wallTemplate, World.transform);
                    newFieldGO.name = "Wall";
                    SetBorderPositionAndRotation(border, newFieldGO);
                    newFieldGO.SetActive(true);
                }
            }
            WorldCreep = new Dictionary<string, GameObject>();
        }


        private void SpawnFieldObject(GameObject field, FieldObject fieldObject)
        {
            var fieldTemplate = Templates[fieldObject.Model];
            var newFieldGO = Instantiate(fieldTemplate, World.transform);
            var material = GameFrame.Base.Resources.Manager.Materials.Get(fieldObject.Material);
            newFieldGO.GetComponent<Renderer>().material = material;
            newFieldGO.name = GetFieldObjectName(field.name);
            newFieldGO.transform.position = field.transform.position + new Vector3(0, 1, 0);
            newFieldGO.SetActive(true);
        }

        private static string GetFieldObjectName(string fieldName)
        {
            return "FieldObject:" + fieldName;
        }

        private void SetBorderPositionAndRotation(Border border, GameObject borderObject)
        {
            float x1 = border.Field1.Coords.X;
            float y1 = border.Field1.Coords.Y;
            var x = (x1 - border.Field2.Coords.X) / 2;
            var z = (y1 - border.Field2.Coords.Y) / 2;
            var height = Mathf.Max(border.Field1.Height, border.Field2.Height);
            borderObject.transform.position = new Vector3(x1 - x, height + 0.55f, y1 - z);
            if (x1 != border.Field2.Coords.X)
            {
                borderObject.transform.eulerAngles = new Vector3(0, 90, 0);
            }
        }

        private void FetchTemplates()
        {
            Templates = new Dictionary<string, GameObject>();
            foreach (Transform tran in TemplateParent.transform)
            {
                Templates.Add(tran.name, tran.gameObject);
            }
        }

        public void DumpStructure()
        {
            Dictionary<string, Field> fields = new Dictionary<string, Field>();
            List<GameObject> rawWalls = new List<GameObject>();
            List<GameObject> rawFieldObjects = new List<GameObject>();
            foreach (Transform tran in World.transform)
            {
                var gO = tran.gameObject;
                if (gO.name.StartsWith("Field:"))
                {
                    var Field = new Field();
                    var x = Mathf.RoundToInt(gO.transform.position.x);
                    var y = Mathf.RoundToInt(gO.transform.position.y);
                    var z = Mathf.RoundToInt(gO.transform.position.z);
                    Field.ID = GetFieldKey(x, y, z);
                    Field.Height = y;
                    Field.Coords = new GameFrame.Core.Math.Vector2(x, z);
                    if (fields.ContainsKey(Field.ID))
                    {
                        Debug.Log("DoubleField");
                    }
                    fields.Add(Field.ID, Field);
                }
                else if (gO.name.StartsWith("Wall"))
                {
                    rawWalls.Add(gO);
                }
                else if (gO.name.StartsWith("FieldObject:"))
                {
                    rawFieldObjects.Add(gO);
                }
                else if (gO.name.StartsWith("Creep_"))
                {
                    
                }
                else
                {
                    Debug.Log("UnknownObject: " + gO.name);
                }
            }
            List<Core.Definitions.Border> borders = GenerateBorderDefinitions(fields, rawWalls);


            List<Core.Definitions.FieldObject> fieldObjects = GenerateFieldObjects(fields, rawFieldObjects);

            var gameField = new Core.Definitions.GameField() { FieldObjects = fieldObjects, Borders = borders, Fields = fields.Values.ToList() };

            var json = GameFrame.Core.Json.Handler.SerializePrettyIgnoreNull(gameField);
            var filePath = Application.streamingAssetsPath + "/dumpGameField.json";
            StreamWriter writer = new StreamWriter(filePath, false);
            writer.Write(json);
            writer.Close();
        }

        private List<Core.Definitions.FieldObject> GenerateFieldObjects(Dictionary<string, Field> fields, List<GameObject> rawFieldObjects)
        {
            var fieldDict = Base.Core.Game.State.GameField.FieldObjects.ToDictionary(field => field.Field.ID, field => field);
            var fieldObjects = new List<Core.Definitions.FieldObject>();
            string prefix = "FieldObject:Field:";
            int prefixL = prefix.Length;
            foreach (var rawFO in rawFieldObjects)
            {
                int x = Mathf.RoundToInt(rawFO.transform.position.x);
                int y = Mathf.RoundToInt(rawFO.transform.position.z);
                int height = Mathf.RoundToInt(rawFO.transform.position.y);
                Field field = FindHighestField(fields, x, y, height);
                if (field != null)
                {
                    var fieldO = new Core.Definitions.FieldObject();
                    var fieldID = rawFO.name.Substring(prefixL);
                    FieldObject f;
                    if (fieldDict.TryGetValue(fieldID, out f))
                    {
                        if (!fieldO.IsReferenced)
                        {
                            fieldO.UpdateMethod = f.UpdateMethod;
                            fieldO.UpdateMethodParameters = f.UpdateMethodParameters;
                            fieldO.Material = f.Material;
                            fieldO.Model = f.Model;
                            fieldO.Name = f.Name;
                            fieldO.Description = f.Description;

                        }
                        fieldO.FieldReference = field.ID;
                        fieldObjects.Add(fieldO);
                    }
                }
            }
            return fieldObjects;
        }

        private List<Core.Definitions.Border> GenerateBorderDefinitions(Dictionary<string, Field> fields, List<GameObject> rawWalls)
        {
            var borders = new List<Core.Definitions.Border>();
            foreach (var rawWall in rawWalls)
            {
                var heigth = Mathf.RoundToInt(rawWall.transform.position.y + 0.45f);
                var x = rawWall.transform.position.x - (int)rawWall.transform.position.x;
                Field field1;
                Field field2;
                if (0.25f < x && x < 0.75f)
                {
                    var y = Mathf.RoundToInt(rawWall.transform.position.z);
                    var x1 = (int)rawWall.transform.position.x;
                    int x2;
                    if (x1 < 0)
                    {
                        x2 = x1 - 1;
                    }
                    else
                    {
                        x2 = x1 + 1;
                    }
                    field1 = FindHighestField(fields, x1, y, heigth);
                    field2 = FindHighestField(fields, x2, y, heigth);
                }
                else
                {
                    x = Mathf.RoundToInt(rawWall.transform.position.x);
                    var y1 = (int)rawWall.transform.position.z;
                    int y2;
                    if (y1 < 0)
                    {
                        y2 = y1 - 1;

                    }
                    else
                    {
                        y2 = y1 + 1;
                    }
                    field1 = FindHighestField(fields, (int)x, y1, heigth);
                    field2 = FindHighestField(fields, (int)x, y2, heigth);
                }

                var border = new Core.Definitions.Border
                {
                    Field1Ref = field1.ID,
                    Field2Ref = field2.ID,
                    BorderType = new Core.Definitions.BorderType() { Model = "Wall", Name = "BorderWall" },
                    BorderStatus = new BorderStatus() { Value = 0 }
                };
                borders.Add(border);
            }

            return borders;
        }

        private string GetFieldKey(int x, int y, int z)
        {
            return x + "-" + z + "-" + y;
        }
        private Field FindHighestField(Dictionary<string, Field> fields, int x, int y, int height)
        {
            Field field;
            height++;
            while (height > -5)
            {
                if (fields.TryGetValue(GetFieldKey(x, height, y), out field))
                {
                    return field;
                }
                height--;
            }
            Debug.Log("ERRORSCHE:");
            return null;
        }

        public void ClearCreep()
        {
            foreach (var field in Base.Core.Game.State.GameField.Fields)
            {
                field.Creep = null;
            }
            foreach (var creepi in WorldCreep.Values)
            {
                Destroy(creepi);
            }
            WorldCreep.Clear();
        }

        public void UpdateCreep()
        {
            var creepTemplate = Templates["Creep"];
            foreach (var field in Base.Core.Game.State.GameField.Fields)
            {
                if (field.Creep != null && field.Creep.Value > 0)
                {
                    GameObject creepGO;
                    if (!WorldCreep.TryGetValue(field.ID, out creepGO))
                    {
                        creepGO = Instantiate(creepTemplate, World.transform);
                        creepGO.name = "Creep_" + field.Creep.Creeper.Name;

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
}