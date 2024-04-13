using Assets.Scripts.Core.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameDisplayBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject TemplateParent;

        [SerializeField]
        private GameObject World;

        private Dictionary<string, GameObject> Templates;

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
                newFieldGO.transform.position = new Vector3(field.Coords.X, field.Height - 1, field.Coords.Y);
                newFieldGO.SetActive(true);
            }
            var wallTemplate = Templates["Wall"];
            foreach (var border in Base.Core.Game.State.GameField.Borders)
            {
                var newFieldGO = Instantiate(wallTemplate, World.transform);
                newFieldGO.name = "Wall";
                SetBoarderPositionAndRotation(border, newFieldGO);
                newFieldGO.SetActive(true);
            }
        }

        private void SetBoarderPositionAndRotation(Border border, GameObject borderObject)
        {
            float x1 = border.Field1.Coords.X;
            float y1 = border.Field1.Coords.Y;
            var x = (x1 - border.Field2.Coords.X) / 2;
            var z = (y1 - border.Field2.Coords.Y) / 2;
            var height = Mathf.Max(border.Field1.Height, border.Field2.Height);
            borderObject.transform.position = new Vector3(x1 - x, height - 0.45f, y1 - z);
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
            foreach (Transform tran in World.transform)
            {
                var gO = tran.gameObject;
                if (gO.name.StartsWith("Field"))
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
            }
            var borders = new List<Core.Definitions.Border>();
            foreach (var rawWall in rawWalls)
            {
                Debug.Log("Wall: " + rawWall.transform.position);
                var heigth = Mathf.RoundToInt(rawWall.transform.position.y + 0.45f);
                var x = rawWall.transform.position.x - (int)rawWall.transform.position.x;
                Field field1;
                Field field2;
                if (0.25f < x && x < 0.75f)
                {
                    var y = Mathf.RoundToInt(rawWall.transform.position.z);
                    var x1 = (int)rawWall.transform.position.x;
                    var x2 = x1 + 1;
                    field1 = FindHighestField(fields, x1, y, heigth);
                    field2 = FindHighestField(fields, x2, y, heigth);
                    Debug.Log("Oddi: y" + y + "  x1:" + x1 + "  x2:" + x2);
                }
                else
                {
                    x = Mathf.RoundToInt(rawWall.transform.position.x);
                    var y1 = (int)rawWall.transform.position.z;
                    var y2 = y1 + 1;
                    field1 = FindHighestField(fields, (int)x, y1, heigth);
                    field2 = FindHighestField(fields, (int)x, y2, heigth);
                    Debug.Log("Eve: x" + x + "  y1:" + y1 + "  y2:" + y2);
                }
                if (field1 == null || field2 == null)
                {
                    Debug.Log("Buti");
                }
                var border = new Core.Definitions.Border
                {
                    Field1Ref = field1.ID,
                    Field2Ref = field2.ID,
                    BorderType = new Core.Definitions.BorderType() { Model = "Wall" }
                };
                borders.Add(border);
            }

            var gameField = new Core.Definitions.GameField() { Borders = borders, Fields = fields.Values.ToList() };

            var json = GameFrame.Core.Json.Handler.SerializePrettyIgnoreNull(gameField);
            var filePath = Application.streamingAssetsPath + "/dumpGameField.json";
            StreamWriter writer = new StreamWriter(filePath, false);
            writer.Write(json);
            writer.Close();
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

    }
}