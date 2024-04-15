using System.Collections.Generic;

namespace Assets.Scripts.Core.Model
{
    public class GameField
    {
        public string Reference { get; set; }

        public bool IsReferenced { get; set; }

        public List<Field> Fields { get; set; }

        public List<Border> Borders { get; set; }
        public List<FieldObject> FieldObjects { get; set; }
    }
}