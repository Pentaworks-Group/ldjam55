using Assets.Scripts.Core.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class Field
    {
        public string ID { get; set; }
        public GameFrame.Core.Math.Vector2 Coords { get; set; }
        public int Height { get; set; }

        public Creep Creep { get; set; }


        [JsonIgnore]
        public List<FieldObject> FieldObjects { get; set; } = new List<FieldObject>();
    }
}