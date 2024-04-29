using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Assets.Scripts.Core.Model
{
    public class Field
    {
        public String ID { get; set; }
        public GameFrame.Core.Math.Vector2 Coords { get; set; }
        public int Height { get; set; }

        [JsonIgnore]
        public List<FieldObject> FieldObjects { get; set; } = new List<FieldObject>();
        [JsonIgnore]
        public List<Border> Borders { get; set; } = new List<Border>();

        public Creep Creep { get; set; }

        //[JsonIgnore]
        public string GenerateFieldID()
        {
            return this.Coords.X + ", " + this.Coords.Y + ", " + this.Height;
        }
    }
}
