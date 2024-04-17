using Assets.Scripts.Core.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Assets.Scripts.Scenes.GameScene
{
    public class DumpBehaviour : MonoBehaviour
    {
        public void DoSeDump()
        {
            GameField game = Base.Core.Game.State.CurrentLevel.GameField;
            RecenterFields(game.Fields);
            RegenerateFieldIds(game.Fields);
            RemoveDuplicates(game.Fields);
            Core.Definitions.GameField gameField = new()
            {
                Fields = ConvertFields(game.Fields),
                FieldObjects = ConvertFieldObjects(game.FieldObjects),
                Borders = ConvertBorders(game.Borders),
                Reference = game.Reference,
                IsReferenced = game.IsReferenced
            };

            //JObject obi = JObject.FromObject(gameField);

            string json = GameFrame.Core.Json.Handler.SerializePrettyIgnoreNull(gameField);
            //string json = SerializePrettyIgnoreNull(obi);
            var filePath = Application.streamingAssetsPath + "/dumpGameField.json";
            StreamWriter writer = new StreamWriter(filePath, false);
            writer.Write(json);
            writer.Close();
        }

        private void RemoveDuplicates(List<Field> fields)
        {
            var fieldDict = new Dictionary<string, List<Field>>();
            foreach (var field in fields)
            {
                var key = field.GenerateFieldID();
                if (!fieldDict.TryGetValue(key, out var list))
                {
                    list = new List<Field>();
                    fieldDict[key] = list;
                }
                list.Add(field);
            }
            foreach (var list in fieldDict.Values)
            {
                if (list.Count > 1)
                {
                    var f = list[0];

                    var fieldObjects = f.FieldObjects;
                    var borders = f.Borders;
                    for (int i = 1; i < list.Count; i++)
                    {
                        var field = list[i];
                        fieldObjects.AddRange(field.FieldObjects);
                        borders.AddRange(field.Borders);
                        fields.Remove(field);
                    }
                }                 
            }
        }

        private void RegenerateFieldIds(List<Field> fields)
        {
            foreach (var field in fields)
            {
                field.ID = field.GenerateFieldID();
            }
        }

        private void RecenterFields(List<Field> fields)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int minH = int.MaxValue;
            foreach (var field in fields)
            {
                if (minX > field.Coords.X)
                {
                    minX = (int)field.Coords.X;
                }
                if (minY > field.Coords.Y)
                {
                    minY = (int)field.Coords.Y;
                }
                if (minH > field.Height)
                {
                    minH = (int)field.Height;
                }
            }
            foreach (var field in fields)
            {
                field.Height -= minH;
                field.Coords = new GameFrame.Core.Math.Vector2(field.Coords.X - minX, field.Coords.Y - minY);
            }
        }

        private List<Core.Definitions.FieldObject> ConvertFieldObjects(List<FieldObject> fieldObjects)
        {
            var conFieldOs = new List<Core.Definitions.FieldObject>();
            foreach (var fieldObject in fieldObjects)
            {
                var conFO = new Core.Definitions.FieldObject()
                {
                    Description = fieldObject.Description,
                    FieldReference = fieldObject.Field.ID,
                    Hidden = fieldObject.Hidden,
                    Material = fieldObject.Material,
                    Model = fieldObject.Model,
                    Name = fieldObject.Name,
                    Methods = fieldObject.Methods
                };
                conFieldOs.Add(conFO);
            }

            return conFieldOs;
        }

        private List<Core.Definitions.Border> ConvertBorders(List<Border> borderList)
        {
            var borders = new List<Core.Definitions.Border>();
            foreach (var rawBorder in borderList)
            {
                var border = new Core.Definitions.Border()
                {
                    Field1Ref = rawBorder.Field1.ID,
                    Field2Ref = rawBorder.Field2.ID,
                    BorderStatus = rawBorder.BorderStatus,
                    BorderType = ConvertBorderType(rawBorder.BorderType),
                    Methods = rawBorder.Methods
                };
                borders.Add(border);
            }
            return borders;
        }

        private Core.Definitions.BorderType ConvertBorderType(BorderType borderType)
        {
            return new Core.Definitions.BorderType()
            {
                Model = borderType.Model,
                Name = borderType.Name
            };
        }

        private List<Core.Definitions.Field> ConvertFields(List<Field> fields)
        {
            var conFields = new List<Core.Definitions.Field>();
            foreach (var field in fields)
            {
                var newField = new Core.Definitions.Field()
                {
                    Coords = field.Coords,
                    //Creep = field.Creep,
                    Height = field.Height,
                    ID = field.ID,
                    FieldObjects = null
                };
                conFields.Add(newField);
            }
            return conFields;
        }

        //public static string SerializePrettyIgnoreNull(object objectToSerialize)
        //{
        //    JsonSerializerSettings settings = new JsonSerializerSettings
        //    {
        //        TypeNameHandling = TypeNameHandling.None,
        //        DefaultValueHandling = JsonIgnoreConditon.,
        //    };
        //    return JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented, settings);
        //}

        //public static string SerializePrettyIgnoreNull(System.Object objectToSerialize)
        //{
        //    var jObject = JObject.FromObject(objectToSerialize);
        //    var tokern = RemoveEmptyChildren(jObject);
        //    Debug.Log(tokern.ToString());
        //    tokern = RemoveUnreferencedTokens(tokern);
        //    Debug.Log(tokern.ToString());

        //    return tokern.ToString();
        //}
        public static JToken RemoveEmptyChildren(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }
        public static bool IsEmpty(JToken token)
        {
            return (token.Type == JTokenType.Null);
        }

        public static JToken RemoveUnreferencedTokens(JToken token)
        {
            if (token is JContainer container)
            {
                // Use ToList() to create a copy of children before iterating
                foreach (JToken child in container.Children().ToList())
                {
                    // Recursively process each child
                    JToken filteredChild = RemoveUnreferencedTokens(child);

                    // If the child is a JObject and has "IsReferenced" with value false, remove it
                    if (filteredChild is JObject obj &&
                        obj.TryGetValue("IsReferenced", out var isReferencedToken) &&
                        isReferencedToken.Type == JTokenType.Boolean &&
                        !(bool)isReferencedToken)
                    {
                        try
                        {
                            child.Remove(); // Remove the child from the parent container
                        }
                        catch (JsonException)
                        {
                            Debug.Log("Cannot remove child: " + child.ToString());
                        }
                    }
                }

                return token; // Return the container after processing children
            }
            else
            {
                return token; // Base case: Return the token as-is if it's not a container
            }
        }
    }
}